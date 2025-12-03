using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Grapher.Models;

namespace Grapher.Data;

public static class SeedData
{
    public static async Task InitializeAsync(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        // Check if data already exists
        if (context.UserProfiles.Any())
        {
            return;
        }

        // Create sample users
        var users = new[]
        {
            new IdentityUser 
            { 
                Id = "user-1",
                UserName = "alice.chen",
                Email = "alice.chen@example.com",
                EmailConfirmed = true
            },
            new IdentityUser 
            { 
                Id = "user-2",
                UserName = "bob.martinez",
                Email = "bob.martinez@example.com",
                EmailConfirmed = true
            },
            new IdentityUser 
            { 
                Id = "user-3",
                UserName = "carol.johnson",
                Email = "carol.johnson@example.com",
                EmailConfirmed = true
            },
            new IdentityUser 
            { 
                Id = "user-4",
                UserName = "david.kumar",
                Email = "david.kumar@example.com",
                EmailConfirmed = true
            },
            new IdentityUser 
            { 
                Id = "user-5",
                UserName = "emma.wilson",
                Email = "emma.wilson@example.com",
                EmailConfirmed = true
            }
        };

        // Add users if they don't exist
        foreach (var user in users)
        {
            if (!await userManager.Users.AnyAsync(u => u.Id == user.Id))
            {
                var result = await userManager.CreateAsync(user, "Password123!");
                if (!result.Succeeded)
                {
                    throw new Exception($"Failed to create user {user.UserName}");
                }
            }
        }

        // Create user profiles
        var userProfiles = new[]
        {
            new UserProfile
            {
                UserId = "user-1",
                FirstName = "Alice",
                LastName = "Chen",
                JobTitle = "Product Manager",
                UsesDarkMode = true
            },
            new UserProfile
            {
                UserId = "user-2",
                FirstName = "Bob",
                LastName = "Martinez",
                JobTitle = "Senior Developer",
                UsesDarkMode = false
            },
            new UserProfile
            {
                UserId = "user-3",
                FirstName = "Carol",
                LastName = "Johnson",
                JobTitle = "UX Designer",
                UsesDarkMode = true
            },
            new UserProfile
            {
                UserId = "user-4",
                FirstName = "David",
                LastName = "Kumar",
                JobTitle = "QA Engineer",
                UsesDarkMode = false
            },
            new UserProfile
            {
                UserId = "user-5",
                FirstName = "Emma",
                LastName = "Wilson",
                JobTitle = "DevOps Engineer",
                UsesDarkMode = true
            }
        };

        await context.UserProfiles.AddRangeAsync(userProfiles);
        await context.SaveChangesAsync();

        // Create projects
        var projects = new[]
        {
            new Project
            {
                Title = "Customer Portal Redesign",
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                OrganizerId = "user-1"
            },
            new Project
            {
                Title = "API Performance Optimization",
                CreatedAt = DateTime.UtcNow.AddDays(-20),
                OrganizerId = "user-2"
            },
            new Project
            {
                Title = "Mobile App MVP",
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                OrganizerId = "user-3"
            }
        };

        await context.Projects.AddRangeAsync(projects);
        await context.SaveChangesAsync();

        var projectIds = context.Projects.AsNoTracking().Select(p => p.Id).ToList();

        // Create project members
        var projectMembers = new[]
        {
            new ProjectMember { ProjectId = projectIds[0], UserId = "user-1", Role = "Manager", JoinedAt = DateTime.UtcNow.AddDays(-30) },
            new ProjectMember { ProjectId = projectIds[0], UserId = "user-2", Role = "Developer", JoinedAt = DateTime.UtcNow.AddDays(-28) },
            new ProjectMember { ProjectId = projectIds[0], UserId = "user-3", Role = "Designer", JoinedAt = DateTime.UtcNow.AddDays(-28) },
            new ProjectMember { ProjectId = projectIds[1], UserId = "user-2", Role = "Manager", JoinedAt = DateTime.UtcNow.AddDays(-20) },
            new ProjectMember { ProjectId = projectIds[1], UserId = "user-4", Role = "Developer", JoinedAt = DateTime.UtcNow.AddDays(-19) },
            new ProjectMember { ProjectId = projectIds[1], UserId = "user-5", Role = "DevOps", JoinedAt = DateTime.UtcNow.AddDays(-19) },
            new ProjectMember { ProjectId = projectIds[2], UserId = "user-3", Role = "Manager", JoinedAt = DateTime.UtcNow.AddDays(-15) },
            new ProjectMember { ProjectId = projectIds[2], UserId = "user-4", Role = "QA", JoinedAt = DateTime.UtcNow.AddDays(-14) },
            new ProjectMember { ProjectId = projectIds[2], UserId = "user-5", Role = "Developer", JoinedAt = DateTime.UtcNow.AddDays(-14) }
        };

        await context.ProjectMembers.AddRangeAsync(projectMembers);
        await context.SaveChangesAsync();

        // Create AI summaries
        var aiSummaries = new[]
        {
            new ProjectAISummary
            {
                ProjectId = projectIds[0],
                SummaryText = "The customer portal redesign project is focused on modernizing the user interface and improving accessibility. Current phase involves wireframing and user testing.",
                LastUpdated = DateTime.UtcNow.AddDays(-2)
            },
            new ProjectAISummary
            {
                ProjectId = projectIds[1],
                SummaryText = "API optimization efforts have achieved a 40% reduction in response times through database query optimization and caching implementation. Load testing is scheduled for next week.",
                LastUpdated = DateTime.UtcNow.AddDays(-1)
            },
            new ProjectAISummary
            {
                ProjectId = projectIds[2],
                SummaryText = "Mobile MVP development is on track with core features completed. Currently focusing on iOS compatibility and App Store submission requirements.",
                LastUpdated = DateTime.UtcNow
            }
        };

        await context.ProjectAISummaries.AddRangeAsync(aiSummaries);
        await context.SaveChangesAsync();

        // Create tasks
        var tasks = new[]
        {
            new TaskItem
            {
                Title = "Create wireframes for dashboard",
                Description = "Design low-fidelity wireframes for the main dashboard layout and navigation flow.",
                ProjectId = projectIds[0],
                Status = "In Progress",
                Priority = "High",
                CreatedAt = DateTime.UtcNow.AddDays(-25)
            },
            new TaskItem
            {
                Title = "Implement user authentication",
                Description = "Set up OAuth 2.0 integration and implement secure login functionality with MFA support.",
                ProjectId = projectIds[0],
                Status = "Pending",
                Priority = "High",
                CreatedAt = DateTime.UtcNow.AddDays(-24)
            },
            new TaskItem
            {
                Title = "Conduct user acceptance testing",
                Description = "Gather feedback from stakeholders on the redesigned portal using A/B testing methodology.",
                ProjectId = projectIds[0],
                Status = "Pending",
                Priority = "Medium",
                CreatedAt = DateTime.UtcNow.AddDays(-20)
            },
            new TaskItem
            {
                Title = "Optimize database queries",
                Description = "Analyze slow queries and implement indexing strategies to improve API response times.",
                ProjectId = projectIds[1],
                Status = "In Progress",
                Priority = "High",
                CreatedAt = DateTime.UtcNow.AddDays(-18)
            },
            new TaskItem
            {
                Title = "Implement Redis caching layer",
                Description = "Set up Redis cluster for distributed caching of frequently accessed data.",
                ProjectId = projectIds[1],
                Status = "In Progress",
                Priority = "High",
                CreatedAt = DateTime.UtcNow.AddDays(-17)
            },
            new TaskItem
            {
                Title = "Load testing and benchmarking",
                Description = "Perform load testing under various conditions and document performance metrics.",
                ProjectId = projectIds[1],
                Status = "Pending",
                Priority = "Medium",
                CreatedAt = DateTime.UtcNow.AddDays(-15)
            },
            new TaskItem
            {
                Title = "Design mobile app screens",
                Description = "Create high-fidelity mockups for all core screens including onboarding and main features.",
                ProjectId = projectIds[2],
                Status = "Completed",
                Priority = "High",
                CreatedAt = DateTime.UtcNow.AddDays(-12)
            },
            new TaskItem
            {
                Title = "Develop payment integration",
                Description = "Integrate Stripe API for processing payments and manage transaction records.",
                ProjectId = projectIds[2],
                Status = "In Progress",
                Priority = "High",
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            },
            new TaskItem
            {
                Title = "Write unit tests for payment module",
                Description = "Create comprehensive unit tests for all payment processing functions.",
                ProjectId = projectIds[2],
                Status = "Pending",
                Priority = "Medium",
                CreatedAt = DateTime.UtcNow.AddDays(-8)
            }
        };

        await context.TaskItems.AddRangeAsync(tasks);
        await context.SaveChangesAsync();

        var taskIds = context.TaskItems.AsNoTracking().Select(t => t.Id).ToList();

        // Create task assignments
        var taskAssignments = new[]
        {
            new TaskAssignment { TaskId = taskIds[0], UserId = "user-3", AssignedByUserId = "user-1", AssignedAt = DateTime.UtcNow.AddDays(-25) },
            new TaskAssignment { TaskId = taskIds[1], UserId = "user-2", AssignedByUserId = "user-1", AssignedAt = DateTime.UtcNow.AddDays(-24) },
            new TaskAssignment { TaskId = taskIds[2], UserId = "user-4", AssignedByUserId = "user-1", AssignedAt = DateTime.UtcNow.AddDays(-20) },
            new TaskAssignment { TaskId = taskIds[3], UserId = "user-2", AssignedByUserId = "user-2", AssignedAt = DateTime.UtcNow.AddDays(-18) },
            new TaskAssignment { TaskId = taskIds[4], UserId = "user-5", AssignedByUserId = "user-2", AssignedAt = DateTime.UtcNow.AddDays(-17) },
            new TaskAssignment { TaskId = taskIds[5], UserId = "user-4", AssignedByUserId = "user-2", AssignedAt = DateTime.UtcNow.AddDays(-15) },
            new TaskAssignment { TaskId = taskIds[6], UserId = "user-3", AssignedByUserId = "user-3", AssignedAt = DateTime.UtcNow.AddDays(-12) },
            new TaskAssignment { TaskId = taskIds[7], UserId = "user-5", AssignedByUserId = "user-3", AssignedAt = DateTime.UtcNow.AddDays(-10) },
            new TaskAssignment { TaskId = taskIds[8], UserId = "user-4", AssignedByUserId = "user-3", AssignedAt = DateTime.UtcNow.AddDays(-8) }
        };

        await context.TaskAssignments.AddRangeAsync(taskAssignments);
        await context.SaveChangesAsync();

        // Create comments
        var comments = new[]
        {
            new Comment
            {
                TaskId = taskIds[0],
                AuthorId = "user-3",
                Content = "Initial wireframes are complete and ready for review. I've incorporated feedback from the stakeholder meeting last week.",
                PostedAt = DateTime.UtcNow.AddDays(-22)
            },
            new Comment
            {
                TaskId = taskIds[0],
                AuthorId = "user-1",
                Content = "Great work! The layout looks clean. Can we adjust the spacing in the sidebar to better match our design system?",
                PostedAt = DateTime.UtcNow.AddDays(-21)
            },
            new Comment
            {
                TaskId = taskIds[1],
                AuthorId = "user-2",
                Content = "OAuth integration is complete. I've set up the necessary scopes and tested with Google and GitHub providers.",
                PostedAt = DateTime.UtcNow.AddDays(-20)
            },
            new Comment
            {
                TaskId = taskIds[3],
                AuthorId = "user-2",
                Content = "Identified 15 slow queries. The main bottleneck is in the user activity aggregation function. I'm creating indexes now.",
                PostedAt = DateTime.UtcNow.AddDays(-15)
            },
            new Comment
            {
                TaskId = taskIds[3],
                AuthorId = "user-5",
                Content = "Good catch! Adding indexes to the user_activity table should help significantly. Let me know the performance improvement after deployment.",
                PostedAt = DateTime.UtcNow.AddDays(-14)
            },
            new Comment
            {
                TaskId = taskIds[4],
                AuthorId = "user-5",
                Content = "Redis cluster is now live with 3 nodes. I've configured replication and persistence settings. Memory usage is stable at 2.1GB.",
                PostedAt = DateTime.UtcNow.AddDays(-12)
            },
            new Comment
            {
                TaskId = taskIds[6],
                AuthorId = "user-3",
                Content = "All screen designs are complete and approved by the design team. Ready for development handoff.",
                PostedAt = DateTime.UtcNow.AddDays(-10)
            },
            new Comment
            {
                TaskId = taskIds[7],
                AuthorId = "user-5",
                Content = "Stripe integration is almost done. I'm currently testing webhook handlers for payment confirmations and refunds.",
                PostedAt = DateTime.UtcNow.AddDays(-7)
            },
            new Comment
            {
                TaskId = taskIds[7],
                AuthorId = "user-1",
                Content = "Make sure to handle edge cases for failed transactions and retry logic. Can you add documentation for the payment flow?",
                PostedAt = DateTime.UtcNow.AddDays(-6)
            }
        };

        await context.Comments.AddRangeAsync(comments);
        await context.SaveChangesAsync();

        // Create attachments
        var attachments = new[]
        {
            new Attachment
            {
                TaskId = taskIds[0],
                FileName = "dashboard_wireframe_v1.pdf",
                FilePath = "/uploads/attachments/dashboard_wireframe_v1.pdf",
                FileSize = 2048576,
                UploadedBy = "user-3",
                UploadedAt = DateTime.UtcNow.AddDays(-22)
            },
            new Attachment
            {
                TaskId = taskIds[1],
                FileName = "oauth_implementation_guide.md",
                FilePath = "/uploads/attachments/oauth_implementation_guide.md",
                FileSize = 512000,
                UploadedBy = "user-2",
                UploadedAt = DateTime.UtcNow.AddDays(-20)
            },
            new Attachment
            {
                TaskId = taskIds[3],
                FileName = "query_analysis_report.xlsx",
                FilePath = "/uploads/attachments/query_analysis_report.xlsx",
                FileSize = 1024000,
                UploadedBy = "user-2",
                UploadedAt = DateTime.UtcNow.AddDays(-15)
            },
            new Attachment
            {
                TaskId = taskIds[4],
                FileName = "redis_cluster_config.yaml",
                FilePath = "/uploads/attachments/redis_cluster_config.yaml",
                FileSize = 256000,
                UploadedBy = "user-5",
                UploadedAt = DateTime.UtcNow.AddDays(-12)
            },
            new Attachment
            {
                TaskId = taskIds[6],
                FileName = "mobile_app_designs_final.xd",
                FilePath = "/uploads/attachments/mobile_app_designs_final.xd",
                FileSize = 5120000,
                UploadedBy = "user-3",
                UploadedAt = DateTime.UtcNow.AddDays(-10)
            },
            new Attachment
            {
                TaskId = taskIds[7],
                FileName = "stripe_api_documentation.pdf",
                FilePath = "/uploads/attachments/stripe_api_documentation.pdf",
                FileSize = 3048576,
                UploadedBy = "user-5",
                UploadedAt = DateTime.UtcNow.AddDays(-7)
            }
        };

        await context.Attachments.AddRangeAsync(attachments);
        await context.SaveChangesAsync();
    }
}using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Grapher.Models;

namespace Grapher.Data;

public static class SeedData
{
    public static async Task InitializeAsync(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        // Check if data already exists
        if (context.UserProfiles.Any())
        {
            return;
        }

        // Create sample users
        var users = new[]
        {
            new IdentityUser 
            { 
                Id = "user-1",
                UserName = "alice.chen",
                Email = "alice.chen@example.com",
                EmailConfirmed = true
            },
            new IdentityUser 
            { 
                Id = "user-2",
                UserName = "bob.martinez",
                Email = "bob.martinez@example.com",
                EmailConfirmed = true
            },
            new IdentityUser 
            { 
                Id = "user-3",
                UserName = "carol.johnson",
                Email = "carol.johnson@example.com",
                EmailConfirmed = true
            },
            new IdentityUser 
            { 
                Id = "user-4",
                UserName = "david.kumar",
                Email = "david.kumar@example.com",
                EmailConfirmed = true
            },
            new IdentityUser 
            { 
                Id = "user-5",
                UserName = "emma.wilson",
                Email = "emma.wilson@example.com",
                EmailConfirmed = true
            }
        };

        // Add users if they don't exist
        foreach (var user in users)
        {
            if (!await userManager.Users.AnyAsync(u => u.Id == user.Id))
            {
                var result = await userManager.CreateAsync(user, "Password123!");
                if (!result.Succeeded)
                {
                    throw new Exception($"Failed to create user {user.UserName}");
                }
            }
        }

        // Create user profiles
        var userProfiles = new[]
        {
            new UserProfile
            {
                UserId = "user-1",
                FirstName = "Alice",
                LastName = "Chen",
                JobTitle = "Product Manager",
                UsesDarkMode = true
            },
            new UserProfile
            {
                UserId = "user-2",
                FirstName = "Bob",
                LastName = "Martinez",
                JobTitle = "Senior Developer",
                UsesDarkMode = false
            },
            new UserProfile
            {
                UserId = "user-3",
                FirstName = "Carol",
                LastName = "Johnson",
                JobTitle = "UX Designer",
                UsesDarkMode = true
            },
            new UserProfile
            {
                UserId = "user-4",
                FirstName = "David",
                LastName = "Kumar",
                JobTitle = "QA Engineer",
                UsesDarkMode = false
            },
            new UserProfile
            {
                UserId = "user-5",
                FirstName = "Emma",
                LastName = "Wilson",
                JobTitle = "DevOps Engineer",
                UsesDarkMode = true
            }
        };

        await context.UserProfiles.AddRangeAsync(userProfiles);
        await context.SaveChangesAsync();

        // Create projects
        var projects = new[]
        {
            new Project
            {
                Title = "Customer Portal Redesign",
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                OrganizerId = "user-1"
            },
            new Project
            {
                Title = "API Performance Optimization",
                CreatedAt = DateTime.UtcNow.AddDays(-20),
                OrganizerId = "user-2"
            },
            new Project
            {
                Title = "Mobile App MVP",
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                OrganizerId = "user-3"
            }
        };

        await context.Projects.AddRangeAsync(projects);
        await context.SaveChangesAsync();

        var projectIds = context.Projects.AsNoTracking().Select(p => p.Id).ToList();

        // Create project members
        var projectMembers = new[]
        {
            new ProjectMember { ProjectId = projectIds[0], UserId = "user-1", Role = "Manager", JoinedAt = DateTime.UtcNow.AddDays(-30) },
            new ProjectMember { ProjectId = projectIds[0], UserId = "user-2", Role = "Developer", JoinedAt = DateTime.UtcNow.AddDays(-28) },
            new ProjectMember { ProjectId = projectIds[0], UserId = "user-3", Role = "Designer", JoinedAt = DateTime.UtcNow.AddDays(-28) },
            new ProjectMember { ProjectId = projectIds[1], UserId = "user-2", Role = "Manager", JoinedAt = DateTime.UtcNow.AddDays(-20) },
            new ProjectMember { ProjectId = projectIds[1], UserId = "user-4", Role = "Developer", JoinedAt = DateTime.UtcNow.AddDays(-19) },
            new ProjectMember { ProjectId = projectIds[1], UserId = "user-5", Role = "DevOps", JoinedAt = DateTime.UtcNow.AddDays(-19) },
            new ProjectMember { ProjectId = projectIds[2], UserId = "user-3", Role = "Manager", JoinedAt = DateTime.UtcNow.AddDays(-15) },
            new ProjectMember { ProjectId = projectIds[2], UserId = "user-4", Role = "QA", JoinedAt = DateTime.UtcNow.AddDays(-14) },
            new ProjectMember { ProjectId = projectIds[2], UserId = "user-5", Role = "Developer", JoinedAt = DateTime.UtcNow.AddDays(-14) }
        };

        await context.ProjectMembers.AddRangeAsync(projectMembers);
        await context.SaveChangesAsync();

        // Create AI summaries
        var aiSummaries = new[]
        {
            new ProjectAISummary
            {
                ProjectId = projectIds[0],
                SummaryText = "The customer portal redesign project is focused on modernizing the user interface and improving accessibility. Current phase involves wireframing and user testing.",
                LastUpdated = DateTime.UtcNow.AddDays(-2)
            },
            new ProjectAISummary
            {
                ProjectId = projectIds[1],
                SummaryText = "API optimization efforts have achieved a 40% reduction in response times through database query optimization and caching implementation. Load testing is scheduled for next week.",
                LastUpdated = DateTime.UtcNow.AddDays(-1)
            },
            new ProjectAISummary
            {
                ProjectId = projectIds[2],
                SummaryText = "Mobile MVP development is on track with core features completed. Currently focusing on iOS compatibility and App Store submission requirements.",
                LastUpdated = DateTime.UtcNow
            }
        };

        await context.ProjectAISummaries.AddRangeAsync(aiSummaries);
        await context.SaveChangesAsync();

        // Create tasks
        var tasks = new[]
        {
            new TaskItem
            {
                Title = "Create wireframes for dashboard",
                Description = "Design low-fidelity wireframes for the main dashboard layout and navigation flow.",
                ProjectId = projectIds[0],
                Status = "In Progress",
                Priority = "High",
                CreatedAt = DateTime.UtcNow.AddDays(-25)
            },
            new TaskItem
            {
                Title = "Implement user authentication",
                Description = "Set up OAuth 2.0 integration and implement secure login functionality with MFA support.",
                ProjectId = projectIds[0],
                Status = "Pending",
                Priority = "High",
                CreatedAt = DateTime.UtcNow.AddDays(-24)
            },
            new TaskItem
            {
                Title = "Conduct user acceptance testing",
                Description = "Gather feedback from stakeholders on the redesigned portal using A/B testing methodology.",
                ProjectId = projectIds[0],
                Status = "Pending",
                Priority = "Medium",
                CreatedAt = DateTime.UtcNow.AddDays(-20)
            },
            new TaskItem
            {
                Title = "Optimize database queries",
                Description = "Analyze slow queries and implement indexing strategies to improve API response times.",
                ProjectId = projectIds[1],
                Status = "In Progress",
                Priority = "High",
                CreatedAt = DateTime.UtcNow.AddDays(-18)
            },
            new TaskItem
            {
                Title = "Implement Redis caching layer",
                Description = "Set up Redis cluster for distributed caching of frequently accessed data.",
                ProjectId = projectIds[1],
                Status = "In Progress",
                Priority = "High",
                CreatedAt = DateTime.UtcNow.AddDays(-17)
            },
            new TaskItem
            {
                Title = "Load testing and benchmarking",
                Description = "Perform load testing under various conditions and document performance metrics.",
                ProjectId = projectIds[1],
                Status = "Pending",
                Priority = "Medium",
                CreatedAt = DateTime.UtcNow.AddDays(-15)
            },
            new TaskItem
            {
                Title = "Design mobile app screens",
                Description = "Create high-fidelity mockups for all core screens including onboarding and main features.",
                ProjectId = projectIds[2],
                Status = "Completed",
                Priority = "High",
                CreatedAt = DateTime.UtcNow.AddDays(-12)
            },
            new TaskItem
            {
                Title = "Develop payment integration",
                Description = "Integrate Stripe API for processing payments and manage transaction records.",
                ProjectId = projectIds[2],
                Status = "In Progress",
                Priority = "High",
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            },
            new TaskItem
            {
                Title = "Write unit tests for payment module",
                Description = "Create comprehensive unit tests for all payment processing functions.",
                ProjectId = projectIds[2],
                Status = "Pending",
                Priority = "Medium",
                CreatedAt = DateTime.UtcNow.AddDays(-8)
            }
        };

        await context.TaskItems.AddRangeAsync(tasks);
        await context.SaveChangesAsync();

        var taskIds = context.TaskItems.AsNoTracking().Select(t => t.Id).ToList();

        // Create task assignments
        var taskAssignments = new[]
        {
            new TaskAssignment { TaskId = taskIds[0], UserId = "user-3", AssignedByUserId = "user-1", AssignedAt = DateTime.UtcNow.AddDays(-25) },
            new TaskAssignment { TaskId = taskIds[1], UserId = "user-2", AssignedByUserId = "user-1", AssignedAt = DateTime.UtcNow.AddDays(-24) },
            new TaskAssignment { TaskId = taskIds[2], UserId = "user-4", AssignedByUserId = "user-1", AssignedAt = DateTime.UtcNow.AddDays(-20) },
            new TaskAssignment { TaskId = taskIds[3], UserId = "user-2", AssignedByUserId = "user-2", AssignedAt = DateTime.UtcNow.AddDays(-18) },
            new TaskAssignment { TaskId = taskIds[4], UserId = "user-5", AssignedByUserId = "user-2", AssignedAt = DateTime.UtcNow.AddDays(-17) },
            new TaskAssignment { TaskId = taskIds[5], UserId = "user-4", AssignedByUserId = "user-2", AssignedAt = DateTime.UtcNow.AddDays(-15) },
            new TaskAssignment { TaskId = taskIds[6], UserId = "user-3", AssignedByUserId = "user-3", AssignedAt = DateTime.UtcNow.AddDays(-12) },
            new TaskAssignment { TaskId = taskIds[7], UserId = "user-5", AssignedByUserId = "user-3", AssignedAt = DateTime.UtcNow.AddDays(-10) },
            new TaskAssignment { TaskId = taskIds[8], UserId = "user-4", AssignedByUserId = "user-3", AssignedAt = DateTime.UtcNow.AddDays(-8) }
        };

        await context.TaskAssignments.AddRangeAsync(taskAssignments);
        await context.SaveChangesAsync();

        // Create comments
        var comments = new[]
        {
            new Comment
            {
                TaskId = taskIds[0],
                AuthorId = "user-3",
                Content = "Initial wireframes are complete and ready for review. I've incorporated feedback from the stakeholder meeting last week.",
                PostedAt = DateTime.UtcNow.AddDays(-22)
            },
            new Comment
            {
                TaskId = taskIds[0],
                AuthorId = "user-1",
                Content = "Great work! The layout looks clean. Can we adjust the spacing in the sidebar to better match our design system?",
                PostedAt = DateTime.UtcNow.AddDays(-21)
            },
            new Comment
            {
                TaskId = taskIds[1],
                AuthorId = "user-2",
                Content = "OAuth integration is complete. I've set up the necessary scopes and tested with Google and GitHub providers.",
                PostedAt = DateTime.UtcNow.AddDays(-20)
            },
            new Comment
            {
                TaskId = taskIds[3],
                AuthorId = "user-2",
                Content = "Identified 15 slow queries. The main bottleneck is in the user activity aggregation function. I'm creating indexes now.",
                PostedAt = DateTime.UtcNow.AddDays(-15)
            },
            new Comment
            {
                TaskId = taskIds[3],
                AuthorId = "user-5",
                Content = "Good catch! Adding indexes to the user_activity table should help significantly. Let me know the performance improvement after deployment.",
                PostedAt = DateTime.UtcNow.AddDays(-14)
            },
            new Comment
            {
                TaskId = taskIds[4],
                AuthorId = "user-5",
                Content = "Redis cluster is now live with 3 nodes. I've configured replication and persistence settings. Memory usage is stable at 2.1GB.",
                PostedAt = DateTime.UtcNow.AddDays(-12)
            },
            new Comment
            {
                TaskId = taskIds[6],
                AuthorId = "user-3",
                Content = "All screen designs are complete and approved by the design team. Ready for development handoff.",
                PostedAt = DateTime.UtcNow.AddDays(-10)
            },
            new Comment
            {
                TaskId = taskIds[7],
                AuthorId = "user-5",
                Content = "Stripe integration is almost done. I'm currently testing webhook handlers for payment confirmations and refunds.",
                PostedAt = DateTime.UtcNow.AddDays(-7)
            },
            new Comment
            {
                TaskId = taskIds[7],
                AuthorId = "user-1",
                Content = "Make sure to handle edge cases for failed transactions and retry logic. Can you add documentation for the payment flow?",
                PostedAt = DateTime.UtcNow.AddDays(-6)
            }
        };

        await context.Comments.AddRangeAsync(comments);
        await context.SaveChangesAsync();

        // Create attachments
        var attachments = new[]
        {
            new Attachment
            {
                TaskId = taskIds[0],
                FileName = "dashboard_wireframe_v1.pdf",
                FilePath = "/uploads/attachments/dashboard_wireframe_v1.pdf",
                FileSize = 2048576,
                UploadedBy = "user-3",
                UploadedAt = DateTime.UtcNow.AddDays(-22)
            },
            new Attachment
            {
                TaskId = taskIds[1],
                FileName = "oauth_implementation_guide.md",
                FilePath = "/uploads/attachments/oauth_implementation_guide.md",
                FileSize = 512000,
                UploadedBy = "user-2",
                UploadedAt = DateTime.UtcNow.AddDays(-20)
            },
            new Attachment
            {
                TaskId = taskIds[3],
                FileName = "query_analysis_report.xlsx",
                FilePath = "/uploads/attachments/query_analysis_report.xlsx",
                FileSize = 1024000,
                UploadedBy = "user-2",
                UploadedAt = DateTime.UtcNow.AddDays(-15)
            },
            new Attachment
            {
                TaskId = taskIds[4],
                FileName = "redis_cluster_config.yaml",
                FilePath = "/uploads/attachments/redis_cluster_config.yaml",
                FileSize = 256000,
                UploadedBy = "user-5",
                UploadedAt = DateTime.UtcNow.AddDays(-12)
            },
            new Attachment
            {
                TaskId = taskIds[6],
                FileName = "mobile_app_designs_final.xd",
                FilePath = "/uploads/attachments/mobile_app_designs_final.xd",
                FileSize = 5120000,
                UploadedBy = "user-3",
                UploadedAt = DateTime.UtcNow.AddDays(-10)
            },
            new Attachment
            {
                TaskId = taskIds[7],
                FileName = "stripe_api_documentation.pdf",
                FilePath = "/uploads/attachments/stripe_api_documentation.pdf",
                FileSize = 3048576,
                UploadedBy = "user-5",
                UploadedAt = DateTime.UtcNow.AddDays(-7)
            }
        };

        await context.Attachments.AddRangeAsync(attachments);
        await context.SaveChangesAsync();
    }
}