using System.ComponentModel.DataAnnotations;

namespace ClimbTrack.Web.Models;

/// <summary>Form/service input for creating or editing a climb record.</summary>
public class ClimbInput
{
    [Range(1, int.MaxValue, ErrorMessage = "Please choose a route.")]
    public int RouteId { get; set; }

    public DateOnly Date { get; set; }

    [Range(1, 100, ErrorMessage = "Attempts must be between 1 and 100.")]
    public int Attempts { get; set; } = 1;

    public bool IsCompleted { get; set; }

    [StringLength(300, ErrorMessage = "Notes can be at most 300 characters.")]
    public string? Notes { get; set; }
}

/// <summary>Admin form/service input for a route.</summary>
public class RouteInput
{
    [Required(ErrorMessage = "Name is required."), StringLength(80)]
    public string Name { get; set; } = "";

    [Range(Grades.Min, Grades.Max, ErrorMessage = "Grade must be between V0 and V10.")]
    public int Grade { get; set; }

    public WallType WallType { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(300), RegularExpression(@"^(https?://\S+)?$", ErrorMessage = "Image URL must start with http:// or https://")]
    public string? ImageUrl { get; set; }
}

public record RouteQuery(int? Grade, WallType? Wall, string? Search, int Page = 1, int PageSize = 12);

public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
}

/// <summary>Flat projection of a climb record plus its route's grade; the only input the pure statistics/recommendation logic needs.</summary>
public record ClimbFact(int RouteId, int Grade, DateOnly Date, int Attempts, bool IsCompleted);

public record ClimbListItem(int Id, int RouteId, string RouteName, int Grade, DateOnly Date, int Attempts, bool IsCompleted, string? Notes);

public record DashboardStats(
    int TotalRecords,
    int CompletedRoutes,
    int? BestGrade,
    int SessionCount,
    int TotalAttempts,
    double SuccessRate,
    IReadOnlyDictionary<int, int> GradeDistribution);

public record RecommendationResult(int Level, IReadOnlyList<ClimbingRoute> Routes);

public record ServiceResult(bool Success, string? Error = null)
{
    public static ServiceResult Ok() => new(true);
    public static ServiceResult Fail(string error) => new(false, error);
}
