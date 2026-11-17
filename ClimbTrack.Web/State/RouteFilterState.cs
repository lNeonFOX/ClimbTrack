using ClimbTrack.Web.Models;

namespace ClimbTrack.Web.State;

/// <summary>
/// Scoped (per Blazor circuit) state container for the catalog filters. Components subscribe to
/// <see cref="Changed"/>; filters survive navigation between pages within the same session.
/// </summary>
public class RouteFilterState
{
    public int? Grade { get; private set; }
    public WallType? Wall { get; private set; }
    public string Search { get; private set; } = "";
    public int Page { get; private set; } = 1;

    public event Action? Changed;

    public void SetGrade(int? grade)
    {
        if (Grade == grade) return;
        Grade = grade; Page = 1; Notify();
    }

    public void SetWall(WallType? wall)
    {
        if (Wall == wall) return;
        Wall = wall; Page = 1; Notify();
    }

    public void SetSearch(string? search)
    {
        var value = search ?? "";
        if (Search == value) return;
        Search = value; Page = 1; Notify();
    }

    public void SetPage(int page)
    {
        page = Math.Max(1, page);
        if (Page == page) return;
        Page = page; Notify();
    }

    public void Reset()
    {
        Grade = null; Wall = null; Search = ""; Page = 1; Notify();
    }

    public RouteQuery ToQuery(int pageSize = 12) => new(Grade, Wall, Search, Page, pageSize);

    private void Notify() => Changed?.Invoke();
}
