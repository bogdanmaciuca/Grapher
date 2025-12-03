using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Grapher.Models;

[PrimaryKey(nameof(ProjectId), nameof(UserId))]
public class ProjectMember {
    // Part 1 of the Key
    public int ProjectId { get; set; }

    [ForeignKey("ProjectId")]
    public Project? Project { get; set; }

    // Part 2 of the Key
    public string UserId { get; set; } = string.Empty;

    [ForeignKey("UserId")]
    public IdentityUser? User { get; set; }

    public string Role { get; set; } = "Member";

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}

