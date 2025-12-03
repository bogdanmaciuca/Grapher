using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Grapher.Models;

public class Attachment {
    [Key]
    public int Id { get; set; }

    public int TaskId { get; set; }
    [ForeignKey("TaskId")]
    public TaskItem? Task { get; set; }

    public string Url { get; set; } = string.Empty;

    // e.g., "image/jpeg", "application/pdf"
    public string Type { get; set; } = string.Empty;
}
