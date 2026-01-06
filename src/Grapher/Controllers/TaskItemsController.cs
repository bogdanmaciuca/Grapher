using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Buffers.Binary;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Grapher.Configuration;
using Grapher.Data;
using Grapher.Models;
using System.Runtime.CompilerServices;


namespace Grapher.Controllers
{
    public class TaskItemsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppRoles _roles;

        private static readonly string[] AllowedImageExtensions = { ".png", ".jpg", ".jpeg", ".svg" };
        private const long MaxImageBytes = 67 * 1024 * 1024; // 67 MB allowed at most

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
        public async Task<IActionResult> Index(int? page, int? pageSize)
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

            if (!string.IsNullOrEmpty(currentUserId))
            {
                // Organizers: see all tasks that belong to projects they organize
                // Members (non-organizers): see only tasks they are assigned to
                tasksQuery = tasksQuery.Where(t =>
                    (t.Project != null && t.Project.OrganizerId == currentUserId) ||
                    t.Assignments.Any(a => a.UserId == currentUserId));
            }
            else if (!isAdmin)
            {
                // Unauthenticated: no access
                return Forbid();
            }

            // pagination defaults
            var pageNumber = page ?? 1;
            var size = pageSize.HasValue && pageSize.Value > 0 ? pageSize.Value : 10;

            // deterministic ordering
            tasksQuery = tasksQuery.OrderByDescending(t => t.StartDate).ThenByDescending(t => t.Id);

            var paged = await Models.PaginatedList<TaskItem>.CreateAsync(tasksQuery, pageNumber, size);
            ViewData["PageSize"] = size;
            return View(paged);
        }

        // GET: TaskItems/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var taskItem = await _context.TaskItems
                .Include(t => t.Project)
                    .ThenInclude(p => p.Members)
                .Include(t => t.Project)
                    .ThenInclude(p => p.Organizer)
                .Include(t => t.Creator)
                .Include(t => t.Assignments)
                    .ThenInclude(a => a.User)
                .Include(t => t.Attachments)
                .Include(t => t.ParentTask)
                .Include(t => t.SubTasks)
                    .ThenInclude(st => st.Creator)
                .Include(t => t.SubTasks)
                    .ThenInclude(st => st.Assignments)
                        .ThenInclude(a => a.User)
                .Include(t => t.Comments)
                    .ThenInclude(c => c.Author)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (taskItem == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole(_roles.AdminRole);

            // authorize: admins, project organizer, or explicitly assigned users
            var isProjectOrganizer = !string.IsNullOrEmpty(currentUserId) && taskItem.Project != null && taskItem.Project.OrganizerId == currentUserId;
            var isAssigned = !string.IsNullOrEmpty(currentUserId) && taskItem.Assignments.Any(a => a.UserId == currentUserId);

            if (!isAdmin && !isProjectOrganizer && !isAssigned)
            {
                return Forbid();
            }

            return View(taskItem);
        }

        // GET: TaskItems/Create
        [Authorize]
        public async Task<IActionResult> Create(int? parentId)
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

            var projects = await projectsQuery.ToListAsync();

            // Handle parentId pre-selection
            int? selectedProjectId = null;
            if (parentId.HasValue)
            {
                var parentTask = await _context.TaskItems.AsNoTracking().FirstOrDefaultAsync(t => t.Id == parentId.Value);
                if (parentTask != null)
                {
                    // Verify user has access to this project
                    if (projects.Any(p => p.Id == parentTask.ProjectId))
                    {
                        selectedProjectId = parentTask.ProjectId;
                    }
                    else
                    {
                        // User trying to create subtask for inaccessible project -> ignore parentId
                        parentId = null;
                    }
                }
            }

            ViewData["ProjectId"] = new SelectList(projects, "Id", "Title", selectedProjectId);
            ViewData["Users"] = new SelectList(_context.Users, "Id", "UserName");

            // Parent tasks: show tasks in projects the user can access
            var accessibleProjectIds = projects.Select(p => p.Id).ToList();
            var parentTasks = await _context.TaskItems
                .Where(t => accessibleProjectIds.Contains(t.ProjectId))
                .Select(t => new { t.Id, t.Title })
                .ToListAsync();

            ViewData["ParentTaskId"] = new SelectList(parentTasks, "Id", "Title", parentId);

            return View();
        }

        // POST: TaskItems/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("Id,ProjectId,Title,Description,Status,StartDate,EndDate,ParentTaskId")] TaskItem taskItem, string[] selectedUsers)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || currentUser.IsGuest) return Forbid();

            // validate that user can create tasks for the selected project
            var project = await _context.Projects
                .Include(p => p.Members)
                .FirstOrDefaultAsync(p => p.Id == taskItem.ProjectId);

            if (project == null) return BadRequest("Invalid project.");

            var currentUserId = currentUser.Id;
            var isAdmin = User.IsInRole(_roles.AdminRole);
            var isOrganizer = project.OrganizerId == currentUserId;
            var isMember = project.Members.Any(m => m.UserId == currentUserId);

            if (!isAdmin && !isOrganizer && !isMember)
            {
                return Forbid();
            }

            // Validate parent if provided (ParentTaskId uses 0 as "none" when not set on form)
            if (taskItem.ParentTaskId.HasValue && taskItem.ParentTaskId.Value != 0)
            {
                var parent = await _context.TaskItems.FindAsync(taskItem.ParentTaskId.Value);
                if (parent == null || parent.ProjectId != taskItem.ProjectId)
                {
                    ModelState.AddModelError(nameof(TaskItem.ParentTaskId), "Parent task must exist and belong to the same project.");
                }
                else if (parent.Id == taskItem.Id && taskItem.Id != 0)
                {
                    ModelState.AddModelError(nameof(TaskItem.ParentTaskId), "A task cannot be its own parent.");
                }
            }

            taskItem.CreatorId = currentUser.Id;
            ModelState.Remove(nameof(TaskItem.Creator));
            ModelState.Remove(nameof(TaskItem.CreatorId));

            if (ModelState.IsValid)
            {
                // Assign selected users only if they are project members
                if (selectedUsers != null && selectedUsers.Length > 0)
                {
                    var memberIds = project.Members.Select(m => m.UserId).ToHashSet();
                    foreach (var user in selectedUsers.Where(u => memberIds.Contains(u)))
                    {
                        taskItem.Assignments.Add(new TaskAssignment { UserId = user });
                    }
                }

                // If creator is a non-organizer member, auto-assign the creator to the task.
                if (!isAdmin && !isOrganizer)
                {
                    // avoid duplicating if creator already present in selectedUsers
                    if (!taskItem.Assignments.Any(a => a.UserId == currentUserId))
                    {
                        taskItem.Assignments.Add(new TaskAssignment { UserId = currentUserId });
                    }
                }

                _context.Add(taskItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Title", taskItem.ProjectId);
            ViewData["Users"] = new SelectList(_context.Users, "Id", "UserName");

            var parentTasks = await _context.TaskItems
                .Where(t => t.ProjectId == taskItem.ProjectId)
                .Select(t => new { t.Id, t.Title })
                .ToListAsync();
            ViewData["ParentTaskId"] = new SelectList(parentTasks, "Id", "Title", taskItem.ParentTaskId);

            return View(taskItem);
        }

        // GET: TaskItems/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var taskItem = await _context.TaskItems
                .Include(t => t.Assignments)
                .Include(t => t.Project)
                    .ThenInclude(p => p.Members)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (taskItem == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole(_roles.AdminRole);

            var isOrganizer = taskItem.Project != null && taskItem.Project.OrganizerId == currentUserId;
            var isCreator = taskItem.CreatorId == currentUserId;

            // permission: organizer of the project can edit any task in project;
            // non-organizer member can edit only own tasks
            if (!isAdmin && !isOrganizer && !isCreator) return Forbid();

            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Title", taskItem.ProjectId);

            // Build users list restricted to project members (organizers assign only project members)
            var memberIds = taskItem.Project?.Members?.Select(m => m.UserId).ToList() ?? new List<string>();
            var memberUsers = await _context.Users.Where(u => memberIds.Contains(u.Id)).ToListAsync();
            var selectedUserIds = taskItem.Assignments.Select(a => a.UserId).ToList();

            ViewData["Users"] = new MultiSelectList(memberUsers, "Id", "UserName", selectedUserIds);

            // Parent tasks: same project, exclude current to avoid self-parent
            var parentTasks = await _context.TaskItems
                .Where(t => t.ProjectId == taskItem.ProjectId && t.Id != taskItem.Id)
                .Select(t => new { t.Id, t.Title })
                .ToListAsync();
            ViewData["ParentTaskId"] = new SelectList(parentTasks, "Id", "Title", taskItem.ParentTaskId);

            return View(taskItem);
        }

        // POST: TaskItems/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProjectId,Title,Description,Status,StartDate,EndDate,ParentTaskId")] TaskItem taskItem, string[] selectedUsers)
        {
            if (id != taskItem.Id) return NotFound();

            var existingTask = await _context.TaskItems
                .Include(t => t.Assignments)
                .Include(t => t.Project)
                    .ThenInclude(p => p.Members)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (existingTask == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole(_roles.AdminRole);

            var isOrganizer = existingTask.Project != null && existingTask.Project.OrganizerId == currentUserId;
            var isCreator = existingTask.CreatorId == currentUserId;

            if (!isAdmin && !isOrganizer && !isCreator) return Forbid();

            // Validate parent if provided
            if (taskItem.ParentTaskId.HasValue && taskItem.ParentTaskId.Value != 0)
            {
                if (taskItem.ParentTaskId.Value == existingTask.Id)
                {
                    ModelState.AddModelError(nameof(TaskItem.ParentTaskId), "A task cannot be its own parent.");
                }
                else
                {
                    var parent = await _context.TaskItems.FindAsync(taskItem.ParentTaskId.Value);
                    if (parent == null || parent.ProjectId != existingTask.ProjectId)
                    {
                        ModelState.AddModelError(nameof(TaskItem.ParentTaskId), "Parent task must exist and belong to the same project.");
                    }
                }
            }

            // preserve creator
            taskItem.CreatorId = existingTask.CreatorId;
            ModelState.Remove(nameof(TaskItem.Creator));
            ModelState.Remove(nameof(TaskItem.CreatorId));

            // Prepare project member ids for validating assignees
            var projectMemberIds = existingTask.Project?.Members?.Select(m => m.UserId).ToHashSet() ?? new HashSet<string>();

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
                    existingTask.ParentTaskId = taskItem.ParentTaskId == 0 ? null : taskItem.ParentTaskId;

                    // Update assignments:
                    // - only admins or organizers may change assignees;
                    // - when allowed, only assign users that are project members.
                    if (isAdmin || isOrganizer)
                    {
                        existingTask.Assignments.Clear();
                        if (selectedUsers != null && selectedUsers.Length > 0)
                        {
                            foreach (var userId in selectedUsers.Distinct())
                            {
                                if (projectMemberIds.Contains(userId))
                                {
                                    existingTask.Assignments.Add(new TaskAssignment { UserId = userId });
                                }
                            }
                        }
                    }
                    else
                    {
                        // Non-organizer creators cannot change assignees � ensure creator remains assigned
                        if (!existingTask.Assignments.Any(a => a.UserId == existingTask.CreatorId))
                        {
                            existingTask.Assignments.Add(new TaskAssignment { UserId = existingTask.CreatorId });
                        }
                    }

                    _context.Update(existingTask);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaskItemExists(taskItem.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Title", taskItem.ProjectId);

            // Rebuild Users MultiSelect using project members
            var memberUsers = await _context.Users.Where(u => projectMemberIds.Contains(u.Id)).ToListAsync();
            ViewData["Users"] = new MultiSelectList(memberUsers, "Id", "UserName", selectedUsers);

            var parentTasks = await _context.TaskItems
                .Where(t => t.ProjectId == existingTask.ProjectId && t.Id != existingTask.Id)
                .Select(t => new { t.Id, t.Title })
                .ToListAsync();
            ViewData["ParentTaskId"] = new SelectList(parentTasks, "Id", "Title", taskItem.ParentTaskId);

            return View(taskItem);
        }

        // GET: TaskItems/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var taskItem = await _context.TaskItems
                .Include(t => t.Project)
                .Include(t => t.Creator)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (taskItem == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole(_roles.AdminRole);
            var isProjectOrganizer = taskItem.Project != null && taskItem.Project.OrganizerId == currentUserId;
            var isCreator = taskItem.CreatorId == currentUserId;

            if (!isAdmin && !isProjectOrganizer && !isCreator) return Forbid();

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
                var isCreator = taskItem.CreatorId == currentUserId;
                var project = await _context.Projects.FindAsync(taskItem.ProjectId);
                var isProjectOrganizer = project != null && project.OrganizerId == currentUserId;

                if (!isAdmin && !isCreator && !isProjectOrganizer) return Forbid();

                _context.TaskItems.Remove(taskItem);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: TaskItems/UploadAttachment
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> UploadAttachment(int taskId, IFormFile file)
        {
            if (file == null)
            {
                TempData["AttachmentError"] = "No file provided.";
                return RedirectToAction(nameof(Details), new { id = taskId });
            }

            if (file.Length == 0 || file.Length > MaxImageBytes)
            {
                TempData["AttachmentError"] = "File is empty or exceeds size limit (5 MB).";
                return RedirectToAction(nameof(Details), new { id = taskId });
            }

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || Array.IndexOf(AllowedImageExtensions, ext) < 0)
            {
                TempData["AttachmentError"] = "Unsupported file type. Allowed: .png, .jpg, .jpeg, .svg";
                return RedirectToAction(nameof(Details), new { id = taskId });
            }

            // Basic content-type sanity (image/* or svg)
            if (!(file.ContentType.StartsWith("image/") || file.ContentType == "image/svg+xml"))
            {
                TempData["AttachmentError"] = "Unsupported content type.";
                return RedirectToAction(nameof(Details), new { id = taskId });
            }

            // Validate file signature / content to avoid spoofed content-types
            if (!await IsValidImageFileAsync(file, ext))
            {
                TempData["AttachmentError"] = "Uploaded file failed validation (not a valid image or SVG).";
                return RedirectToAction(nameof(Details), new { id = taskId });
            }

            var taskItem = await _context.TaskItems
                .Include(t => t.Project)
                    .ThenInclude(p => p.Members)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (taskItem == null)
            {
                return NotFound();
            }

            // Authorization: only admin, task creator or project organizer can add attachments
            var currentUserId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole(_roles.AdminRole);
            var isCreator = !string.IsNullOrEmpty(currentUserId) && taskItem.CreatorId == currentUserId;
            var isProjectOrganizer = !string.IsNullOrEmpty(currentUserId) && taskItem.Project != null && taskItem.Project.OrganizerId == currentUserId;

            if (!isAdmin && !isCreator && !isProjectOrganizer)
            {
                return Forbid();
            }

            // Save file to wwwroot/uploads/tasks/{taskId}/
            var uploadsRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "tasks", taskId.ToString());
            Directory.CreateDirectory(uploadsRoot);

            // Generate safe filename
            var safeName = Path.GetRandomFileName();
            var filename = Path.ChangeExtension(safeName, ext);
            var filePath = Path.Combine(uploadsRoot, filename);

            // Write file - rewind stream if helpers read it
            file.OpenReadStream().Position = 0;
            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Store attachment record (Url stored relative to site root)
            var url = $"/uploads/tasks/{taskId}/{filename}";
            var attachment = new Attachment
            {
                Url = url,
                Type = ext.TrimStart('.').ToLowerInvariant(),
                TaskId = taskId
            };

            _context.Attachments.Add(attachment);
            await _context.SaveChangesAsync();

            TempData["AttachmentSuccess"] = "File uploaded.";
            return RedirectToAction(nameof(Details), new { id = taskId });
        }

        // POST: TaskItems/AddAttachmentUrl
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> AddAttachmentUrl(int taskId, string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                TempData["AttachmentError"] = "URL is required.";
                return RedirectToAction(nameof(Details), new { id = taskId });
            }

            // Basic URL validation
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                TempData["AttachmentError"] = "Invalid URL.";
                return RedirectToAction(nameof(Details), new { id = taskId });
            }

            // Optional: restrict external URLs to known providers to avoid abuse
            if (!IsAllowedExternalProvider(uri))
            {
                // If you want to allow arbitrary links, remove this check.
                TempData["AttachmentError"] = "Only links from supported providers (YouTube, Vimeo) or plain http/https are allowed.";
                // fallback: to accept all links comment out the return below
                // return RedirectToAction(nameof(Details), new { id = taskId });
            }

            var taskItem = await _context.TaskItems
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (taskItem == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole(_roles.AdminRole);
            var isCreator = !string.IsNullOrEmpty(currentUserId) && taskItem.CreatorId == currentUserId;
            var isProjectOrganizer = !string.IsNullOrEmpty(currentUserId) && taskItem.Project != null && taskItem.Project.OrganizerId == currentUserId;

            if (!isAdmin && !isCreator && !isProjectOrganizer)
            {
                return Forbid();
            }

            var attachment = new Attachment
            {
                Url = url,
                Type = "link",
                TaskId = taskId
            };

            _context.Attachments.Add(attachment);
            await _context.SaveChangesAsync();

            TempData["AttachmentSuccess"] = "Link added.";
            return RedirectToAction(nameof(Details), new { id = taskId });
        }

        // POST: TaskItems/RemoveAttachment
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> RemoveAttachment(int id)
        {
            var attachment = await _context.Attachments
                .Include(a => a.Task)
                    .ThenInclude(t => t.Project)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (attachment == null) return NotFound();

            var taskItem = attachment.Task;
            var currentUserId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole(_roles.AdminRole);
            var isCreator = !string.IsNullOrEmpty(currentUserId) && taskItem != null && taskItem.CreatorId == currentUserId;
            var isProjectOrganizer = !string.IsNullOrEmpty(currentUserId) && taskItem?.Project != null && taskItem.Project.OrganizerId == currentUserId;

            if (!isAdmin && !isCreator && !isProjectOrganizer)
            {
                return Forbid();
            }

            // If attachment is a local file (stored under /uploads/), remove file from disk
            if (!string.IsNullOrEmpty(attachment.Url) && attachment.Url.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
            {
                var wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var physicalPath = Path.Combine(wwwrootPath, attachment.Url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                try
                {
                    if (System.IO.File.Exists(physicalPath))
                    {
                        System.IO.File.Delete(physicalPath);
                    }
                }
                catch
                {
                    // swallow file deletion errors but proceed to remove DB record
                }
            }

            _context.Attachments.Remove(attachment);
            await _context.SaveChangesAsync();

            TempData["AttachmentSuccess"] = "Attachment removed.";
            return RedirectToAction(nameof(Details), new { id = taskItem?.Id ?? 0 });
        }

        // POST: TaskItems/AddComment
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> AddComment(int taskId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["CommentError"] = "Comment content is required.";
                return RedirectToAction(nameof(Details), new { id = taskId });   
            }

            var taskItem = await _context.TaskItems
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (taskItem == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole(_roles.AdminRole);
            var isCreator = !string.IsNullOrEmpty(currentUserId) && taskItem.CreatorId == currentUserId;
            var isProjectOrganizer = !string.IsNullOrEmpty(currentUserId) && taskItem.Project != null && taskItem.Project.OrganizerId == currentUserId;

            if (!isAdmin && !isCreator && !isProjectOrganizer)
            {
                return Forbid();
            }

            var comment = new Comment
            {
                Content = content,
                PostedAt = DateTime.UtcNow,
                TaskId = taskId,
                AuthorId = _userManager.GetUserId(User)
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            TempData["CommentSuccess"] = "Comment added.";
            return RedirectToAction(nameof(Details), new { id = taskId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> RemoveComment(int id)
        {
            var comment = await _context.Comments
                .Include(c => c.Task)
                    .ThenInclude(t => t.Project)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (comment == null) return NotFound();

            var taskItem = comment.Task;
            var currentUserId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole(_roles.AdminRole);
            var isAuthor = !string.IsNullOrEmpty(currentUserId) && comment.AuthorId == currentUserId;
            var isProjectOrganizer = !string.IsNullOrEmpty(currentUserId) && taskItem?.Project != null && taskItem.Project.OrganizerId == currentUserId;
            if (!isAdmin && !isAuthor && !isProjectOrganizer)
            {
                return Forbid();
            }
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            TempData["CommentSuccess"] = "Comment removed.";
            return RedirectToAction(nameof(Details), new { id = taskItem?.Id ?? 0 });
        }

        // Helper: validate file signature / SVG content
        private static async Task<bool> IsValidImageFileAsync(IFormFile file, string ext)
        {
            // read the first bytes
            await using var stream = file.OpenReadStream();
            var header = new byte[12];
            var read = await stream.ReadAsync(header, 0, header.Length);

            // reset stream so later copy works
            try { stream.Position = 0; } catch { /* ignore if not seekable */ }

            ext = ext?.ToLowerInvariant() ?? "";

            if (ext == ".png")
            {
                // PNG header: 89 50 4E 47 0D 0A 1A 0A
                if (read >= 8 &&
                    header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 &&
                    header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A)
                    return true;
                return false;
            }

            if (ext == ".jpg" || ext == ".jpeg")
            {
                // JPEG starts with FF D8 FF
                if (read >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
                    return true;
                return false;
            }

            if (ext == ".svg")
            {
                // SVG is text/XML. Read a larger prefix (first 4KB) and check for <svg and absence of <script>
                stream.Position = 0;
                using var reader = new StreamReader(stream, leaveOpen: true);
                var prefix = await reader.ReadToEndAsync();

                // reset position if possible
                try { stream.Position = 0; } catch { }

                var lower = prefix.ToLowerInvariant();
                if (lower.Contains("<svg") && !lower.Contains("<script") && !lower.Contains("onload="))
                {
                    return true;
                }
                return false;
            }

            return false;
        }

        // Helper: check allowed external providers (relaxed)
        private static bool IsAllowedExternalProvider(Uri uri)
        {
            var host = uri.Host.ToLowerInvariant();
            if (host.Contains("youtube.com") || host.Contains("youtu.be") || host.Contains("vimeo.com"))
                return true;

            return false;
        }

        private bool TaskItemExists(int id)
        {
            return _context.TaskItems.Any(e => e.Id == id);
        }
    }
}
