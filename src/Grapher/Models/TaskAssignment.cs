using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Grapher.Models
{
    public class TaskAssignment
    {
        public long TaskId { get; set; }
        [ForeignKey("TaskId")]
        public virtual TaskItem Task { get; set; }

        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.Now;

        public string? AssignedByUserId { get; set; }
    }
}
