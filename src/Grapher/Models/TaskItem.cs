using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Grapher.Validation;

namespace Grapher.Models
{
    public class TaskItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(256)]
        public required string Title { get; set; }

        [StringLength(4096)]
        public string? Description { get; set; }

        public TaskStatus Status { get; set; } = TaskStatus.NotStarted;

        [SensibleDate]
        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        [SensibleDate]
        [DateGreaterThan("StartDate")]
        public DateTime? EndDate { get; set; }

        [Required]
        public int ProjectId { get; set; }
        [ForeignKey("ProjectId")]
        public virtual Project Project { get; set; } = null!;

        /// Parent can be null (root tasks)
        public int? ParentTaskId { get; set; }
        [ForeignKey("ParentTaskId")]
        public virtual TaskItem? ParentTask { get; set; }

        // children
        public virtual ICollection<TaskItem> SubTasks { get; set; } = new List<TaskItem>();

        [Required]
        public required string CreatorId { get; set; }
        [ForeignKey("CreatorId")]
        public virtual ApplicationUser? Creator { get; set; }

        public virtual ICollection<TaskAssignment> Assignments { get; set; } = new List<TaskAssignment>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    }
}
