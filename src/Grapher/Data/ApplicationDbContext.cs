using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Grapher.Models;

namespace Grapher.Data;

public class ApplicationDbContext : IdentityDbContext {
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectMember> ProjectMembers { get; set; }
    public DbSet<ProjectAISummary> ProjectAISummaries { get; set; }
    public DbSet<TaskItem> TaskItems { get; set; }
    public DbSet<TaskAssignment> TaskAssignments { get; set; }
    public DbSet<Attachment> Attachments { get; set; }
    public DbSet<Comment> Comments { get; set; }

    protected override void OnModelCreating(ModelBuilder builder) {
        base.OnModelCreating(builder);

        builder.Entity<ProjectMember>()
            .HasKey(pm => new { pm.ProjectId, pm.UserId });

        builder.Entity<TaskAssignment>()
            .HasKey(ta => new { ta.TaskId, ta.UserId });

        builder.Entity<Project>()
            .HasOne(p => p.AISummary)
            .WithOne(s => s.Project)
            .HasForeignKey<ProjectAISummary>(s => s.ProjectId);
    } 
}

