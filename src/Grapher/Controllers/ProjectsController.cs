using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Grapher.Configuration;
using Grapher.Data;
using Grapher.Models;
using Grapher.Services;
using Microsoft.AspNetCore.SignalR;

namespace Grapher.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppRoles _roles;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ProjectsController> _logger;

        public ProjectsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IOptions<AppRoles> rolesOptions,
            IEmailSender emailSender,
            ILogger<ProjectsController> logger)
        {
            _context = context;
            _userManager = userManager;
            _roles = rolesOptions?.Value ?? new AppRoles();
            _emailSender = emailSender;
            _logger = logger;
        }

        // GET: Projects - Admins see all, authenticated non-admins see only their projects
        public async Task<IActionResult> Index()
        {
            var currentUserId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole(_roles.AdminRole);

            IQueryable<Project> projectsQuery = _context.Projects.Include(p => p.Organizer);

            if (isAdmin)
            {
                // Admin: all projects
            }
            else if (!string.IsNullOrEmpty(currentUserId))
            {
                // Authenticated non-admin: only projects they organize or are a member of
                projectsQuery = projectsQuery.Where(p =>
                    p.OrganizerId == currentUserId ||
                    p.Members.Any(m => m.UserId == currentUserId));
            }
            else
            {
                // Unauthenticated: no access
                return Forbid();
            }

            ViewBag.AdminRole = _roles.AdminRole;
            return View(await projectsQuery.ToListAsync());
        }

        // GET: Projects/Details - Only organizer, members, or admins can view
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(p => p.Organizer)
                .Include(p => p.Members)
                    .ThenInclude(pm => pm.User)
                .Include(p => p.AiSummary)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole(_roles.AdminRole);
            var isOrganizer = project.OrganizerId == currentUserId;
            var isMember = !string.IsNullOrEmpty(currentUserId) && project.Members.Any(m => m.UserId == currentUserId);

            if (!isAdmin && !isOrganizer && !isMember)
            {
                return Forbid();
            }

            // make organizer flag available to the view
            ViewBag.IsOrganizer = isOrganizer;
            ViewBag.IsMember = isMember;
            return View(project);
        }

        // GET: Projects/Create - authenticated non-guest users allowed to create
        [Authorize]
        public async Task<IActionResult> Create()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || currentUser.IsGuest)
            {
                return Forbid();
            }

            var currentUserId = currentUser.Id;
            ViewData["OrganizerId"] = new SelectList(_context.Users.Where(u => u.Id == currentUserId), "Id", "UserName", currentUserId);
            return View();
        }

        // POST: Projects/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("Title,Description,StartDate")] Project project)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || currentUser.IsGuest)
            {
                return Forbid();
            }

            project.OrganizerId = currentUser.Id;
            ModelState.Remove("OrganizerId");

            if (!project.StartDate.HasValue || project.StartDate == default)
            {
                project.StartDate = DateTime.UtcNow.Date;
            }

            if (ModelState.IsValid)
            {
                project.CreatedAt = DateTime.UtcNow;
                _context.Add(project);
                await _context.SaveChangesAsync();

                /// Ensure the organizer is recorded as a project member with role "Organizer"
                /// Use composite key find to avoid duplicates for existing data
                var existingOrganizer = await _context.ProjectMembers.FindAsync(new object[] { project.Id, project.OrganizerId });
                if (existingOrganizer == null)
                {
                    var organizerMember = new ProjectMember
                    {
                        ProjectId = project.Id,
                        UserId = project.OrganizerId,
                        Role = "Organizer",
                        JoinedAt = project.CreatedAt
                    };
                    _context.ProjectMembers.Add(organizerMember);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            return View(project);
        }

        // GET: Projects/Edit/5 - Only organizer or Admin can edit
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole(_roles.AdminRole);
            var isOrganizer = project.OrganizerId == currentUserId;

            if (!isAdmin && !isOrganizer)
            {
                return Forbid();
            }

            ViewData["OrganizerId"] = new SelectList(
                _context.Users.Where(u => u.Id == project.OrganizerId),
                "Id",
                "UserName",
                project.OrganizerId);

            return View(project);
        }

        // POST: Projects/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            // Load the existing tracked entity from the context
            var existingProject = await _context.Projects.FindAsync(id);
            if (existingProject == null)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole(_roles.AdminRole);
            var isOrganizer = existingProject.OrganizerId == currentUserId;

            if (!isAdmin && !isOrganizer)
            {
                return Forbid();
            }

            // Bind only the editable properties from the form onto the tracked entity
            if (await TryUpdateModelAsync(existingProject, "",
                p => p.Title,
                p => p.Description,
                p => p.StartDate))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(existingProject.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // If binding/validation failed, render view with the tracked entity (includes validation errors)
            return View(existingProject);
        }

        // GET: Projects/Delete/5 - Only organizer or Admin can delete
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(p => p.Organizer)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole(_roles.AdminRole);
            var isOrganizer = project.OrganizerId == currentUserId;

            if (!isAdmin && !isOrganizer)
            {
                return Forbid();
            }

            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            var currentUserId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole(_roles.AdminRole);

            if (project != null && (isAdmin || project.OrganizerId == currentUserId))
            {
                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }

        // GET: /Projects/SearchUsers?q=term
        [HttpGet]
        public async Task<IActionResult> SearchUsers(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return Json(Array.Empty<string>());
            }

            // limit results; do not leak other user info
            var matches = await _userManager.Users
                .Where(u => u.Email.Contains(q))
                .OrderBy(u => u.Email)
                .Select(u => u.Email)
                .Take(10)
                .ToListAsync();

            return Json(matches);
        }

        // POST: /Projects/Invite
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Invite(int projectId, string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest("Email required");
            }

            var project = await _context.Projects
                .Include(p => p.Members)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || project.OrganizerId != currentUser.Id)
            {
                return Forbid();
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                TempData["InviteError"] = "No user found with that email.";
                return RedirectToAction(nameof(Details), new { id = projectId });
            }

            if (user.Id == project.OrganizerId)
            {
                TempData["InviteError"] = "Cannot invite project organizer.";
                return RedirectToAction(nameof(Details), new { id = projectId });
            }

            if (project.Members.Any(m => m.UserId == user.Id))
            {
                TempData["InviteError"] = "User is already a project member.";
                return RedirectToAction(nameof(Details), new { id = projectId });
            }

            var member = new ProjectMember
            {
                ProjectId = projectId,
                UserId = user.Id,
                Role = "Member",
                JoinedAt = DateTime.UtcNow
            };

            _context.ProjectMembers.Add(member);
            await _context.SaveChangesAsync();

            // Send minimal email (IEmailSender must be registered)
            try
            {
                var acceptUrl = Url.Action("Details", "Projects", new { id = projectId }, Request.Scheme);
                var subject = $"You were added to project \"{project.Title}\"";
                var body = $"You have been added to project \"{project.Title}\". View it here: {acceptUrl}";
                await _emailSender.SendEmailAsync(email, subject, body);
                TempData["InviteSuccess"] = "Member added.";
            }
            catch
            {
                TempData["InviteSuccess"] = "Member added; sending email failed.";
            }

            return RedirectToAction(nameof(Details), new { id = projectId });
        }

        // GET: Projects/ManageMembers/5
        [Authorize]
        public async Task<IActionResult> ManageMembers(int? id)
        {
            if (id == null) return NotFound();

            var project = await _context.Projects
                .Include(p => p.Organizer)
                .Include(p => p.Members)
                    .ThenInclude(pm => pm.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || project.OrganizerId != currentUser.Id)
            {
                return Forbid();
            }

            return View(project);
        }

        // POST: Projects/RemoveMember
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> RemoveMember(int projectId, string userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) return BadRequest();

            // basic input validation
            if (userId.Length > 256) return BadRequest();

            var project = await _context.Projects
                .Include(p => p.Members)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || project.OrganizerId != currentUser.Id)
            {
                _logger.LogWarning("Unauthorized RemoveMember attempt by {UserId} on project {ProjectId}", currentUser?.Id, projectId);
                return Forbid();
            }

            // Prevent removing organizer
            if (userId == project.OrganizerId)
            {
                TempData["MemberError"] = "Cannot remove the organizer.";
                return RedirectToAction(nameof(ManageMembers), new { id = projectId });
            }

            var member = await _context.ProjectMembers.FindAsync(new object[] { projectId, userId });
            if (member == null)
            {
                TempData["MemberError"] = "Member not found.";
                return RedirectToAction(nameof(ManageMembers), new { id = projectId });
            }

            // Use transaction for safety
            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.ProjectMembers.Remove(member);
                await _context.SaveChangesAsync();

                // Optional: record audit/log entry
                _logger.LogInformation("Project member removed: ProjectId={ProjectId}, RemovedUserId={RemovedUserId}, ByUserId={ByUserId}",
                    projectId, userId, currentUser.Id);

                await tx.CommitAsync();
                TempData["MemberSuccess"] = "Member removed.";
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _logger.LogError(ex, "Failed to remove member {UserId} from project {ProjectId}", userId, projectId);
                TempData["MemberError"] = "Failed to remove member.";
            }

            return RedirectToAction(nameof(ManageMembers), new { id = projectId });
        }

        // Leave from project
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Leave(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Forbid();
            }

            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            // Safety Check: Organizers cannot leave their own project
            if (project.OrganizerId == userId)
            {
                TempData["Error"] = "You cannot leave a project you own. Delete it instead.";
                return RedirectToAction(nameof(Details), new { id = id });
            }

            // Find the membership record
            var memberRecord = await _context.ProjectMembers.FindAsync(id, userId);

            if (memberRecord != null)
            {
                _context.ProjectMembers.Remove(memberRecord);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
