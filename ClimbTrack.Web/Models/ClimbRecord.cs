using System.ComponentModel.DataAnnotations;

namespace ClimbTrack.Web.Models;

public class ClimbRecord
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = "";
    public ApplicationUser? User { get; set; }

    public int RouteId { get; set; }
    public ClimbingRoute? Route { get; set; }

    public DateOnly Date { get; set; }

    [Range(1, 100)]
    public int Attempts { get; set; } = 1;

    public bool IsCompleted { get; set; }

    [StringLength(300)]
    public string? Notes { get; set; }
}
