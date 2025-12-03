using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Grapher.Models;

public class ProjectAISummary {
    [Key]
    public int Id { get; set; }

    // Foreign key
    public int ProjectId { get; set; }

    [ForeignKey("ProjectId")]
    public Project? Project { get; set; }

    public string SummaryText { get; set; } = string.Empty;

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

