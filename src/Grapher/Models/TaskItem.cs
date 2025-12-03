using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Grapher.Models;

public enum TaskStatus {
    NotStarted = 0,
    InProgress = 1,
    Completed  = 2
}

public class TaskItem {
    [Key]
    public int Id { get; set; }

    // Foreign key
    public int ProjectId { get; set; }

    [ForeignKey("ProjectId")]
    public Project? Project { get; set; } 

    [Required]
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public TaskStatus Status { get; set; } = TaskStatus.NotStarted;

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public ICollection<TaskAssignment> Assignments { get; set; } = new List<TaskAssignment>();
    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}

