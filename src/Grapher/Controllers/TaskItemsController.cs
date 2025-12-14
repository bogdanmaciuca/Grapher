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

namespace Grapher.Controllers
{
    public class TaskItemsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TaskItemsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: TaskItems
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.TaskItems.Include(t => t.Project);
            return View(await applicationDbContext.ToListAsync());
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
                .Include(t => t.Creator)
                .Include(t => t.Assignments)
                    .ThenInclude(a => a.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (taskItem == null)
            {
                return NotFound();
            }

            return View(taskItem);
        }

        // GET: TaskItems/Create
        public IActionResult Create()
        {
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Title");
            ViewData["Users"] = new SelectList(_context.Users, "Id", "UserName");
            return View();
        }

        // POST: TaskItems/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ProjectId,Title,Description,Status,StartDate,EndDate")] TaskItem taskItem, string[] selectedUsers)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                taskItem.CreatorId = userId;

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
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskItem = await _context.TaskItems.Include(t => t.Assignments).FirstOrDefaultAsync(t => t.Id == id);
            if (taskItem == null)
            {
                return NotFound();
            }
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Title", taskItem.ProjectId);
            var userIds = taskItem.Assignments.Select(a => a.UserId).ToList();
            ViewData["Users"] = new MultiSelectList(_context.Users, "Id", "UserName", userIds);
            return View(taskItem);
        }

        // POST: TaskItems/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProjectId,Title,Description,Status,StartDate,EndDate")] TaskItem taskItem, string[] selectedUsers)
        {
            if (id != taskItem.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var taskToUpdate = await _context.TaskItems.Include(t => t.Assignments).FirstOrDefaultAsync(t => t.Id == id);
                    if (taskToUpdate == null)
                    {
                        return NotFound();
                    }

                    taskToUpdate.ProjectId = taskItem.ProjectId;
                    taskToUpdate.Title = taskItem.Title;
                    taskToUpdate.Description = taskItem.Description;
                    taskToUpdate.Status = taskItem.Status;
                    taskToUpdate.StartDate = taskItem.StartDate;
                    taskToUpdate.EndDate = taskItem.EndDate;

                    taskToUpdate.Assignments.Clear();
                    if (selectedUsers != null)
                    {
                        foreach (var userId in selectedUsers)
                        {
                            taskToUpdate.Assignments.Add(new TaskAssignment { UserId = userId });
                        }
                    }

                    _context.Update(taskToUpdate);
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
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskItem = await _context.TaskItems
                .Include(t => t.Project)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (taskItem == null)
            {
                return NotFound();
            }

            return View(taskItem);
        }

        // POST: TaskItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var taskItem = await _context.TaskItems.FindAsync(id);
            if (taskItem != null)
            {
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
