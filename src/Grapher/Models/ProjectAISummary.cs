using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Grapher.Models
{
    public class ProjectAiSummary
    {
        [Key, ForeignKey("Project")]
        public int ProjectId { get; set; }

        public virtual required Project Project { get; set; }

        public string? SummaryText { get; set; }

        public DateTime? LastUpdated { get; set; }
    }
}
