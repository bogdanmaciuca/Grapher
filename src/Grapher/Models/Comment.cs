using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Grapher.Models
{
    public class Comment
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime PostedAt { get; set; } = DateTime.Now;

        public long TaskId { get; set; }
        [ForeignKey("TaskId")]
        public virtual TaskItem Task { get; set; }

        [Required]
        public string AuthorId { get; set; }
        [ForeignKey("AuthorId")]
        public virtual ApplicationUser Author { get; set; }
    }
}

