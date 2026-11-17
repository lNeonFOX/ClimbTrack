using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ClimbTrack.Web.Models;

public class ClimbingRoute
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    public string Name { get; set; } = "";

    [Range(Grades.Min, Grades.Max)]
    public int Grade { get; set; }

    public WallType WallType { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(300)]
    public string? ImageUrl { get; set; }

    [JsonIgnore]
    public ICollection<ClimbRecord> Records { get; set; } = new List<ClimbRecord>();
}
