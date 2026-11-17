using Microsoft.AspNetCore.Identity;

namespace ClimbTrack.Web.Models;

public class ApplicationUser : IdentityUser
{
    /// <summary>Grade (0-10) used as the starting level for recommendations when the user has no completed climbs.</summary>
    public int PreferredGrade { get; set; }

    public ICollection<ClimbRecord> Records { get; set; } = new List<ClimbRecord>();
}
