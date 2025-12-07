using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Grapher.Models
{
    public class ProjectAiSummary
    {
        [Key, ForeignKey("Project")]
        public long ProjectId { get; set; }

        public virtual Project Project { get; set; }

        public string? SummaryText { get; set; }

        public DateTime? LastUpdated { get; set; }
    }
}
