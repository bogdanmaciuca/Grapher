using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Grapher.Models
{
    public class Project
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        public string OrganizerId { get; set; }

        [ForeignKey("OrganizerId")]
        public virtual ApplicationUser Organizer { get; set; }

        public virtual ProjectAiSummary? AiSummary { get; set; }
        public virtual ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
        public virtual ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}
