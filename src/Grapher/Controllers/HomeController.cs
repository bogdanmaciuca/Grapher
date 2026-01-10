using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Grapher.Models;
using Grapher.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace Grapher.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> ToggleDarkMode(bool useDarkMode)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var profile = await _context.UserProfiles.FindAsync(user.Id);
        if (profile == null)
        {
            profile = new UserProfile { UserId = user.Id, User = user };
            _context.UserProfiles.Add(profile);
        }

        profile.UsesDarkMode = useDarkMode;
        await _context.SaveChangesAsync();

        return Ok();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
