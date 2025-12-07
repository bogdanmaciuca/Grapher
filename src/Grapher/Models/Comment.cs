using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Grapher.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public required string Content { get; set; }

        public DateTime PostedAt { get; set; } = DateTime.UtcNow;

        public int TaskId { get; set; }
        [ForeignKey("TaskId")]
        public virtual TaskItem? Task { get; set; }

        [Required]
        public required string AuthorId { get; set; }
        [ForeignKey("AuthorId")]
        public virtual ApplicationUser? Author { get; set; }
    }
}

