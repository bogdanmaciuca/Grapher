using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Grapher.Models
{
    public class TaskItem
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        public string? Description { get; set; }

        public TaskStatusEnum Status { get; set; } = TaskStatusEnum.NotStarted;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public long ProjectId { get; set; }
        [ForeignKey("ProjectId")]
        public virtual Project Project { get; set; }

        public string? AssigneeId { get; set; }
        [ForeignKey("AssigneeId")]
        public virtual ApplicationUser? Assignee { get; set; }

        public virtual ICollection<TaskAssignment> Assignments { get; set; } = new List<TaskAssignment>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    }
}
