using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Grapher.Models
{
    [PrimaryKey(nameof(TaskId), nameof(UserId))]
    public class TaskAssignment
    {
        public int TaskId { get; set; }
        [ForeignKey("TaskId")]
        public virtual TaskItem? Task { get; set; }

        public required string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        public string? AssignedByUserId { get; set; }
    }
}
