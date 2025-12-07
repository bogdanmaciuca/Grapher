using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Grapher.Models
{
    public class Attachment
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public string Url { get; set; }

        [Required]
        public string Type { get; set; }

        public long TaskId { get; set; }
        [ForeignKey("TaskId")]
        public virtual TaskItem Task { get; set; }
    }
}
