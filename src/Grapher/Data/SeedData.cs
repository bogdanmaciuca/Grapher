using Grapher.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Grapher.Configuration;

namespace Grapher.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider) {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var appRoles = scope.ServiceProvider.GetRequiredService<IOptions<AppRoles>>().Value;

            // Ensure roles exist
            if (!await roleManager.RoleExistsAsync(appRoles.AdminRole))
            {
                await roleManager.CreateAsync(new IdentityRole(appRoles.AdminRole));
            }
            if (!await roleManager.RoleExistsAsync(appRoles.MemberRole))
            {
                await roleManager.CreateAsync(new IdentityRole(appRoles.MemberRole));
            }

            // Nuke the fuckin' thing (order matters btw)
            if (context.TaskItems.Any()) { context.TaskItems.RemoveRange(context.TaskItems); }
            if (context.Projects.Any()) { context.Projects.RemoveRange(context.Projects); }
            await context.SaveChangesAsync();
            foreach (var user in userManager.Users.ToList()) { await userManager.DeleteAsync(user); }

            // Create human life
            var admin = new ApplicationUser {
                UserName = "Admin",
                Email = "admin@grapher.com",
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(admin, "Password123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, appRoles.AdminRole);
            }
            // if (!result.Succeeded) {
            //     Console.WriteLine("Seed failed!");
            // }

            var bobomac = new ApplicationUser {
                UserName = "BoboMac",
                Email = "bobomac@gmail.com",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(bobomac, "Password123!");

            var coq = new ApplicationUser {
                UserName = "Coq",
                Email = "coq@gmail.com",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(coq, "Password123!");

            var odin = new ApplicationUser {
                UserName = "Odin",
                Email = "odin@gmail.com",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(odin, "Password123!");

            var zeus = new ApplicationUser {
                UserName = "Zeus",
                Email = "zeus@gmail.com",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(zeus, "Password123!");

            await context.SaveChangesAsync();

            // Create endeavours
            var voxels = new Project {
                Title = "Voxel engine",
                Description = "Engine featuring easily modifiable terrain and smooth meshing which supports sharp features",
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                OrganizerId = bobomac.Id,
            };

            var grapher = new Project {
                Title = "Grapher",
                Description = "Task management website for increased productivity, featuring a tree-like hierarchical structure of the project",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                OrganizerId = coq.Id
            };

            var texed = new Project {
                Title = "TexEd",
                Description = "GUI text editor with vim motions and opinionated defaults",
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                OrganizerId = bobomac.Id
            };

            context.Projects.AddRange(voxels, grapher, texed);
            await context.SaveChangesAsync();

            // Assign workforce to said endeavours
            var memberships = new List<ProjectMember> {
                // Voxels
                new ProjectMember {
                    ProjectId = voxels.Id,
                    UserId = odin.Id,
                    Role = "Member",
                },
                // Grapher
                new ProjectMember {
                    ProjectId = grapher.Id,
                    UserId = bobomac.Id,
                    Role = "Member",
                },
                new ProjectMember {
                    ProjectId = grapher.Id,
                    UserId = odin.Id,
                    Role = "Member",
                },
                new ProjectMember {
                    ProjectId = grapher.Id,
                    UserId = zeus.Id,
                    Role = "Member",
                },
                // TexEd
                new ProjectMember {
                    ProjectId = texed.Id,
                    UserId = coq.Id,
                    Role = "Member",
                },
            };
            context.ProjectMembers.AddRange(memberships);
            await context.SaveChangesAsync();

            // // Tasks
            // var tasks = new List<TaskItem> {
            //     // Voxels
            //     new TaskItem {
            //         Title = "Create platform layer",
            //         Description = "Create API for: window creation, event polling, window resizing, swapchain operations.",
            //         Status = Grapher.Models.TaskStatus.Completed,
            //         StartDate = DateTime.UtcNow.AddDays(-9),
            //         EndDate = DateTime.UtcNow.AddDays(-8),
            //         CreatorId = bobomac.Id,
            //     }
            // }
        }
    }
}
