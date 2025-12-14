using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Grapher.Models;

namespace Grapher.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            // get a logger early so we can report problems
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("SeedData");

            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                try
                {
                    // Prefer migrations when using EF migrations
                    await context.Database.MigrateAsync();
                }
                catch (Exception ex)
                {
                    // fall back but log — this helps diagnose local dev issues
                    logger.LogWarning(ex, "Migration failed, falling back to EnsureCreated()");
                    context.Database.EnsureCreated();
                }

                var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                // ===== SEED ROLES =====
                await EnsureRoleAsync(roleManager, "Administrator");
                await EnsureRoleAsync(roleManager, "Member");

                // ===== SEED TEST USERS =====

                // Admin User
                await SeedAdminUserAsync(userManager, "admin@grapher.com", "Admin123!");

                // Regular Member Users
                await SeedMemberUserAsync(userManager, "alice@grapher.com", "Alice", "Chen", "Alice123!");
                await SeedMemberUserAsync(userManager, "bob@grapher.com", "Bob", "Martinez", "Bob123!");
                await SeedMemberUserAsync(userManager, "carol@grapher.com", "Carol", "Johnson", "Carol123!");
                await SeedMemberUserAsync(userManager, "david@grapher.com", "David", "Kumar", "David123!");

                // ===== SEED USER PROFILES =====
                await SeedUserProfilesAsync(context, userManager);

                // ===== SEED SAMPLE PROJECTS =====
                await SeedSampleProjectsAsync(context, userManager);

                // ===== SEED SAMPLE TASKS =====
                await SeedSampleTasksAsync(context, userManager);

                // ===== SEED SAMPLE COMMENTS =====
                await SeedSampleCommentsAsync(context, userManager);

                // DIAGNOSTIC LOGGING: list users and projects so you can verify association
                try
                {
                    var users = await userManager.Users.ToListAsync();
                    logger.LogInformation("Seeded users ({Count}):", users.Count);
                    foreach (var u in users)
                    {
                        logger.LogInformation("  User: {Email} Id: {Id}", u.Email, u.Id);
                    }

                    var projects = await context.Projects.AsNoTracking().ToListAsync();
                    logger.LogInformation("Seeded projects ({Count}):", projects.Count);
                    foreach (var p in projects)
                    {
                        var organizer = await userManager.FindByIdAsync(p.OrganizerId);
                        var organizerEmail = organizer?.Email ?? "(organizer id not found)";
                        logger.LogInformation("  Project: {Title} Id: {Id} OrganizerId: {OrgId} OrganizerEmail: {OrgEmail}",
                            p.Title, p.Id, p.OrganizerId, organizerEmail);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to write diagnostic logs after seeding.");
                }
            }
        }

        /// <summary>
        /// Seeds an Administrator user
        /// </summary>
        private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager, string email, string password)
        {
            var existingUser = await userManager.FindByEmailAsync(email);

            if (existingUser == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    IsActive = true,
                    IsGuest = false,
                    AcceptsTerms = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(adminUser, password);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Administrator");
                }
            }
        }

        /// <summary>
        /// Seeds a regular Member user
        /// </summary>
        private static async Task SeedMemberUserAsync(UserManager<ApplicationUser> userManager, string email, string firstName, string lastName, string password)
        {
            var existingUser = await userManager.FindByEmailAsync(email);

            if (existingUser == null)
            {
                var memberUser = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    IsActive = true,
                    IsGuest = false,
                    AcceptsTerms = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(memberUser, password);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(memberUser, "Member");
                }
            }
        }

        /// <summary>
        /// Seeds user profiles for all users
        /// </summary>
        private static async Task SeedUserProfilesAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            var users = await userManager.Users.ToListAsync();

            foreach (var user in users)
            {
                var existingProfile = await context.UserProfiles.FirstOrDefaultAsync(up => up.UserId == user.Id);

                if (existingProfile == null)
                {
                    var (firstName, lastName, jobTitle) = ExtractUserInfo(user.Email ?? "user@example.com");

                    var profile = new UserProfile
                    {
                        UserId = user.Id,
                        FirstName = firstName,
                        LastName = lastName,
                        JobTitle = jobTitle,
                        UsesDarkMode = false
                    };

                    context.UserProfiles.Add(profile);
                }
            }

            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Seeds sample projects (organized by different users)
        /// </summary>
        private static async Task SeedSampleProjectsAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            // Only seed if no projects exist
            if (await context.Projects.AnyAsync())
            {
                return;
            }

            var referenceDate = new DateTime(2025, 11, 03, 0, 0, 0, DateTimeKind.Utc);
            var users = await userManager.Users.ToListAsync();

            if (users.Count < 2)
            {
                return;
            }

            // Alice and Bob will be organizers
            var alice = await userManager.FindByEmailAsync("alice@grapher.com");
            var bob = await userManager.FindByEmailAsync("bob@grapher.com");

            // if specific users not found fall back to first two users
            var aliceId = alice?.Id ?? users[0].Id;
            var bobId = bob?.Id ?? (users.Count > 1 ? users[1].Id : users[0].Id);

            var projects = new[]
            {
                new Project
                {
                    Title = "Customer Portal Redesign",
                    Description = "Modernize the customer-facing portal with improved UI/UX and accessibility features.",
                    CreatedAt = referenceDate.AddDays(-30),
                    OrganizerId = aliceId
                },
                new Project
                {
                    Title = "API Performance Optimization",
                    Description = "Improve API response times and scalability through database optimization and caching strategies.",
                    CreatedAt = referenceDate.AddDays(-20),
                    OrganizerId = bobId
                },
                new Project
                {
                    Title = "Mobile App MVP",
                    Description = "Develop a minimum viable product for our mobile application focusing on core features.",
                    CreatedAt = referenceDate.AddDays(-15),
                    OrganizerId = aliceId
                }
            };

            context.Projects.AddRange(projects);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Seeds sample tasks for projects
        /// </summary>
        private static async Task SeedSampleTasksAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            // Only seed if no tasks exist
            if (await context.TaskItems.AnyAsync())
            {
                return;
            }

            var referenceDate = new DateTime(2025, 11, 03, 0, 0, 0, DateTimeKind.Utc);
            var projects = await context.Projects.ToListAsync();
            var adminUser = await userManager.FindByEmailAsync("admin@grapher.com");

            if (!projects.Any() || adminUser == null)
            {
                return;
            }

            var tasks = new[]
            {
                new TaskItem
                {
                    Title = "Design mockups",
                    Description = "Create high-fidelity mockups for all pages",
                    ProjectId = projects[0].Id,
                    Status = Models.TaskStatus.InProgress,
                    StartDate = referenceDate.AddDays(-25),
                    EndDate = referenceDate.AddDays(5),
                    CreatorId = adminUser.Id
                },
                new TaskItem
                {
                    Title = "Implement authentication",
                    Description = "Set up OAuth 2.0 and JWT tokens",
                    ProjectId = projects[0].Id,
                    Status = Models.TaskStatus.NotStarted,
                    StartDate = referenceDate.AddDays(-20),
                    EndDate = referenceDate.AddDays(10),
                    CreatorId = adminUser.Id
                },
                new TaskItem
                {
                    Title = "Database optimization",
                    Description = "Analyze and optimize slow queries",
                    ProjectId = projects[1].Id,
                    Status = Models.TaskStatus.InProgress,
                    StartDate = referenceDate.AddDays(-18),
                    EndDate = referenceDate.AddDays(3),
                    CreatorId = adminUser.Id
                },
                new TaskItem
                {
                    Title = "Setup caching layer",
                    Description = "Implement Redis for distributed caching",
                    ProjectId = projects[1].Id,
                    Status = Models.TaskStatus.Completed,
                    StartDate = referenceDate.AddDays(-15),
                    EndDate = referenceDate.AddDays(-5),
                    CreatorId = adminUser.Id
                },
                new TaskItem
                {
                    Title = "Design app interface",
                    Description = "Create UI/UX designs for mobile app",
                    ProjectId = projects[2].Id,
                    Status = Models.TaskStatus.InProgress,
                    StartDate = referenceDate.AddDays(-12),
                    EndDate = referenceDate.AddDays(8),
                    CreatorId = adminUser.Id
                }
            };

            context.TaskItems.AddRange(tasks);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Seeds sample comments on tasks
        /// </summary>
        private static async Task SeedSampleCommentsAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            // Only seed if no comments exist
            if (await context.Comments.AnyAsync())
            {
                return;
            }

            var referenceDate = new DateTime(2025, 11, 03, 0, 0, 0, DateTimeKind.Utc);
            var tasks = await context.TaskItems.ToListAsync();
            var users = await userManager.Users.ToListAsync();

            if (tasks.Count < 3 || users.Count < 3)
            {
                return;
            }

            var comments = new[]
            {
                new Comment
                {
                    TaskId = tasks[0].Id,
                    AuthorId = users[0].Id,
                    Content = "Great start! The mockups look amazing. Ready for implementation.",
                    PostedAt = referenceDate.AddDays(-20)
                },
                new Comment
                {
                    TaskId = tasks[0].Id,
                    AuthorId = users[1].Id,
                    Content = "I reviewed the designs. Minor adjustments needed in the navigation bar.",
                    PostedAt = referenceDate.AddDays(-18)
                },
                new Comment
                {
                    TaskId = tasks[2].Id,
                    AuthorId = users[2].Id,
                    Content = "Query optimization is complete. Performance improved by 60%.",
                    PostedAt = referenceDate.AddDays(-10)
                }
            };

            context.Comments.AddRange(comments);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Helper to extract user info from email
        /// </summary>
        private static (string firstName, string lastName, string jobTitle) ExtractUserInfo(string email)
        {
            var parts = email.Split('@')[0].Split('.');

            var firstName = parts.Length > 0 ? char.ToUpper(parts[0][0]) + parts[0].Substring(1) : "User";
            var lastName = parts.Length > 1 ? char.ToUpper(parts[1][0]) + parts[1].Substring(1) : "";
            var jobTitle = email.Contains("admin") ? "Administrator" : "Developer";

            return (firstName, lastName, jobTitle);
        }

        /// <summary>
        /// Ensures a role exists
        /// </summary>
        private static async Task EnsureRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
        {
            var roleExists = await roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }
}
