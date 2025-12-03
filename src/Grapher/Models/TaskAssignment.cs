using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Grapher.Models;

[PrimaryKey(nameof(TaskId), nameof(UserId))]
public class TaskAssignment {
    public int TaskId { get; set; }
    [ForeignKey("TaskId")]
    public TaskItem? Task { get; set; }

    // Link to the User (The person doing the work)
    public string UserId { get; set; } = string.Empty;
    [ForeignKey("UserId")]
    public IdentityUser? User { get; set; }

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    // Who assigned this task? (Audit trail)
    public string? AssignedByUserId { get; set; }
    [ForeignKey("AssignedByUserId")]
    public IdentityUser? AssignedByUser { get; set; }
}
