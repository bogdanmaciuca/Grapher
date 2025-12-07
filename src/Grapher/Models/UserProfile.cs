using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Grapher.Models
{
    public class UserProfile
    {
        [Key, ForeignKey("User")]
        public string UserId { get; set; } = null!;
        [Required]
        public virtual ApplicationUser User { get; set; } = null!;

        [StringLength(100)]
        public string? FirstName { get; set; }

        [StringLength(100)]
        public string? LastName { get; set; }

        [StringLength(100)]
        public string? JobTitle { get; set; }

        public bool UsesDarkMode { get; set; } = false;

        [NotMapped]
        public string DisplayName => $"{FirstName} {LastName}";
    }
}
