using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Grapher.Models
{
    public class Attachment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public required string Url { get; set; }

        [Required]
        public required string Type { get; set; }

        public int TaskId { get; set; }
        [ForeignKey("TaskId")]
        public virtual TaskItem? Task { get; set; }
    }
}
