using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Grapher.Data;
using Grapher.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Grapher.Configuration;

namespace Grapher.Controllers
{
    public class TaskItemsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppRoles _roles;

        public TaskItemsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IOptions<AppRoles> rolesOptions)
        {
            _context = context;
            _userManager = userManager;
            _roles = rolesOptions?.Value ?? new AppRoles();

        }

        // GET: TaskItems
        public async Task<IActionResult> Index()
        {
            var currentUserId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole(_roles.AdminRole);

            // base query with useful includes
            IQueryable<TaskItem> tasksQuery = _context.TaskItems
                .Include(t => t.Project)
                    .ThenInclude(p => p.Members)
                .Include(t => t.Project)
                    .ThenInclude(p => p.Organizer)
                .Include(t => t.Creator)
                .Include(t => t.Assignments)
                    .ThenInclude(a => a.User)
                .AsNoTracking();

            if (isAdmin)
            {
                // Admin: see all tasks
            }
            else if (!string.IsNullOrEmpty(currentUserId))
            {
                // Authenticated non-admin: only tasks they created, assigned to, or that belong to projects they organize / are member of
                tasksQuery = tasksQuery.Where(t =>
                    t.CreatorId == currentUserId ||
                    t.Assignments.Any(a => a.UserId == currentUserId) ||
                    (t.Project != null && t.Project.OrganizerId == currentUserId) ||
                    (t.Project != null && t.Project.Members.Any(m => m.UserId == currentUserId)));
            }
            else
            {
                // Unauthenticated: no access
                return Forbid();
            }

            return View(await tasksQuery.ToListAsync());
        }

        // GET: TaskItems/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskItem = await _context.TaskItems
                .Include(t => t.Project)
                    .ThenInclude(p => p.Members)
                .Include(t => t.Project)
                    .ThenInclude(p => p.Organizer)
                .Include(t => t.Creator)
                .Include(t => t.Assignments)
                    .ThenInclude(a => a.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (taskItem == null)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole(_roles.AdminRole);
            var isCreator = !string.IsNullOrEmpty(currentUserId) && taskItem.CreatorId == currentUserId;
            var isProjectOrganizer = !string.IsNullOrEmpty(currentUserId) && taskItem.Project != null && taskItem.Project.OrganizerId == currentUserId;
            var isAssigned = !string.IsNullOrEmpty(currentUserId) && taskItem.Assignments.Any(a => a.UserId == currentUserId);
            var isMember = !string.IsNullOrEmpty(currentUserId) && taskItem.Project != null && taskItem.Project.Members.Any(m => m.UserId == currentUserId);

            if (!isAdmin && !isCreator && !isProjectOrganizer && !isAssigned && !isMember)
            {
                return Forbid();
            }

            return View(taskItem);
        }

        // GET: TaskItems/Create
        [Authorize]
        public async Task<IActionResult> Create()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || currentUser.IsGuest)
            {
                return Forbid();
            }

            var currentUserId = currentUser.Id;
            var isAdmin = User.IsInRole(_roles.AdminRole);

            IQueryable<Project> projectsQuery = _context.Projects.AsNoTracking();

            if (!isAdmin)
            {
                projectsQuery = projectsQuery.Where(p =>
                    p.OrganizerId == currentUserId ||
                    p.Members.Any(m => m.UserId == currentUserId));
            }

            ViewData["ProjectId"] = new SelectList(await projectsQuery.ToListAsync(), "Id", "Title");
            ViewData["Users"] = new SelectList(_context.Users, "Id", "UserName");
            return View();
        }

        // POST: TaskItems/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("Id,ProjectId,Title,Description,Status,StartDate,EndDate")] TaskItem taskItem, string[] selectedUsers)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || currentUser.IsGuest)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                taskItem.CreatorId = currentUser.Id;

                if (selectedUsers != null)
                {
                    foreach (var user in selectedUsers)
                    {
                        taskItem.Assignments.Add(new TaskAssignment { UserId = user });
                    }
                }

                _context.Add(taskItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Title", taskItem.ProjectId);
            ViewData["Users"] = new SelectList(_context.Users, "Id", "UserName");
            return View(taskItem);
        }

        // GET: TaskItems/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskItem = await _context.TaskItems
                .Include(t => t.Assignments)
                .Include(t => t.Project)
                    .ThenInclude(p => p.Members)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (taskItem == null)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole(_roles.AdminRole);
            var isCreator = !string.IsNullOrEmpty(currentUserId) && taskItem.CreatorId == currentUserId;
            var isProjectOrganizer = !string.IsNullOrEmpty(currentUserId) && taskItem.Project != null && taskItem.Project.OrganizerId == currentUserId;
            var isProjectMember = !string.IsNullOrEmpty(currentUserId) && taskItem.Project != null && taskItem.Project.Members.Any(m => m.UserId == currentUserId);

            if (!isAdmin && !isCreator && !isProjectOrganizer && !isProjectMember)
            {
                return Forbid();
            }

            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Title", taskItem.ProjectId);
            var userIds = taskItem.Assignments.Select(a => a.UserId).ToList();
            ViewData["Users"] = new MultiSelectList(_context.Users, "Id", "UserName", userIds);
            return View(taskItem);
        }

        // POST: TaskItems/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProjectId,Title,Description,Status,StartDate,EndDate")] TaskItem taskItem, string[] selectedUsers)
        {
            if (id != taskItem.Id)
            {
                return NotFound();
            }

            var existingTask = await _context.TaskItems
                .Include(t => t.Assignments)
                .Include(t => t.Project)
                    .ThenInclude(p => p.Members)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (existingTask == null)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole(_roles.AdminRole);
            var isCreator = !string.IsNullOrEmpty(currentUserId) && existingTask.CreatorId == currentUserId;
            var isProjectOrganizer = !string.IsNullOrEmpty(currentUserId) && existingTask.Project != null && existingTask.Project.OrganizerId == currentUserId;
            var isProjectMember = !string.IsNullOrEmpty(currentUserId) && existingTask.Project != null && existingTask.Project.Members.Any(m => m.UserId == currentUserId);

            if (!isAdmin && !isCreator && !isProjectOrganizer && !isProjectMember)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    existingTask.ProjectId = taskItem.ProjectId;
                    existingTask.Title = taskItem.Title;
                    existingTask.Description = taskItem.Description;
                    existingTask.Status = taskItem.Status;
                    existingTask.StartDate = taskItem.StartDate;
                    existingTask.EndDate = taskItem.EndDate;

                    existingTask.Assignments.Clear();
                    if (selectedUsers != null)
                    {
                        foreach (var userId in selectedUsers)
                        {
                            existingTask.Assignments.Add(new TaskAssignment { UserId = userId });
                        }
                    }

                    _context.Update(existingTask);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaskItemExists(taskItem.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Title", taskItem.ProjectId);
            ViewData["Users"] = new MultiSelectList(_context.Users, "Id", "UserName", selectedUsers);
            return View(taskItem);
        }

        // GET: TaskItems/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskItem = await _context.TaskItems
                .Include(t => t.Project)
                .Include(t => t.Creator)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (taskItem == null)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole(_roles.AdminRole);
            var isCreator = !string.IsNullOrEmpty(currentUserId) && taskItem.CreatorId == currentUserId;
            var isProjectOrganizer = !string.IsNullOrEmpty(currentUserId) && taskItem.Project != null && taskItem.Project.OrganizerId == currentUserId;

            if (!isAdmin && !isCreator && !isProjectOrganizer)
            {
                return Forbid();
            }

            return View(taskItem);
        }

        // POST: TaskItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var taskItem = await _context.TaskItems.FindAsync(id);
            if (taskItem != null)
            {
                var currentUserId = _userManager.GetUserId(User);
                var isAdmin = User.IsInRole(_roles.AdminRole);
                var isCreator = !string.IsNullOrEmpty(currentUserId) && taskItem.CreatorId == currentUserId;
                var project = await _context.Projects.FindAsync(taskItem.ProjectId);
                var isProjectOrganizer = !string.IsNullOrEmpty(currentUserId) && project != null && project.OrganizerId == currentUserId;

                if (!isAdmin && !isCreator && !isProjectOrganizer)
                {
                    return Forbid();
                }

                _context.TaskItems.Remove(taskItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TaskItemExists(int id)
        {
            return _context.TaskItems.Any(e => e.Id == id);
        }
    }
}
