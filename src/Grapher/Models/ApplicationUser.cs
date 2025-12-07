using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Grapher.Models
{
    public class ApplicationUser : IdentityUser
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;  // CHANGED: DateTime.Now -> DateTime.UtcNow

        public DateTime? LastLogin { get; set; }

        public bool IsGuest { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public bool AcceptsTerms { get; set; }

        public virtual UserProfile? Profile { get; set; }
        public virtual ICollection<Project> OrganizedProjects { get; set; } = new List<Project>();
        public virtual ICollection<ProjectMember> ProjectMemberships { get; set; } = new List<ProjectMember>();
        public virtual ICollection<TaskAssignment> TaskAssignments { get; set; } = new List<TaskAssignment>();
    }
}