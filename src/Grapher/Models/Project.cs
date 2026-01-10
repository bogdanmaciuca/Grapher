using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Grapher.Validation;

namespace Grapher.Models
{
    public class Project
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(128)]
        public required string Title { get; set; }

        [Required]
        [StringLength(1024)]
        public required string Description { get; set; }

        public DateTime CreatedAt { get; set; }

        // Optional start date (user may supply this)
        [SensibleDate]
        public DateTime? StartDate { get; set; }

        [Required]
        public required string OrganizerId { get; set; }

        [ForeignKey("OrganizerId")]
        public virtual ApplicationUser? Organizer { get; set; }

        public virtual ProjectAiSummary? AiSummary { get; set; }
        public virtual ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
        public virtual ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}
