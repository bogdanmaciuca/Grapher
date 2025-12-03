using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Grapher.Models;

public class Comment {
    [Key]
    public int Id { get; set; }

    // Link to the Task being discussed
    public int TaskId { get; set; }
    [ForeignKey("TaskId")]
    public TaskItem? Task { get; set; }

    public string AuthorId { get; set; } = string.Empty;
    [ForeignKey("AuthorId")]
    public IdentityUser? Author { get; set; }

    [Required]
    public string Content { get; set; } = string.Empty;

    public DateTime PostedAt { get; set; } = DateTime.UtcNow;
}
