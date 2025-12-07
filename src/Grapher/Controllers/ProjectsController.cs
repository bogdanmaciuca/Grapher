using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Grapher.Data;
using Grapher.Models;

namespace Grapher.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProjectsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Projects - Anyone can view
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Projects.Include(p => p.Organizer);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Projects/Details/5 - Anyone can view details
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
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

            return View(project);
        }

        // GET: Projects/Create - Only Members and Admins can create
        [Authorize(Roles = "Member,Administrator")]
        public IActionResult Create()
        {
            var currentUserId = _userManager.GetUserId(User);
            ViewData["OrganizerId"] = new SelectList(_context.Users.Where(u => u.Id == currentUserId), "Id", "UserName", currentUserId);
            return View();
        }

        // POST: Projects/Create - Only Members and Admins can create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Member,Administrator")]
        public async Task<IActionResult> Create([Bind("Title,Description")] Project project)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Forbid();
            }

            project.OrganizerId = currentUserId;
            if (ModelState.IsValid)
            {
                project.CreatedAt = DateTime.UtcNow;
                _context.Add(project);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(project);
        }

        // GET: Projects/Edit/5 - Only organizer or Admin can edit
        [Authorize(Roles = "Member,Administrator")]
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

            // Check authorization: Must be organizer or admin
            var currentUserId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("Administrator");
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
        [Authorize(Roles = "Member,Administrator")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,OrganizerId")] Project project)
        {
            if (id != project.Id)
            {
                return NotFound();
            }

            // Check authorization
            var currentUserId = _userManager.GetUserId(User);
            var existingProject = await _context.Projects.FindAsync(id);
            var isAdmin = User.IsInRole("Administrator");
            var isOrganizer = existingProject?.OrganizerId == currentUserId;

            if (!isAdmin && !isOrganizer)
            {
                return Forbid();
            }

            // Prevent changing organizer
            if (existingProject != null)
            {
                project.OrganizerId = existingProject.OrganizerId;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(project);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.Id))
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

            return View(project);
        }

        // GET: Projects/Delete/5 - Only organizer or Admin can delete
        [Authorize(Roles = "Member,Administrator")]
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

            // Check authorization
            var currentUserId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("Administrator");
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
        [Authorize(Roles = "Member,Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            var currentUserId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("Administrator");

            // Only organizer or admin can delete
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
    }
}
