namespace ClimbTrack.Web.Models;

public enum WallType
{
    Slab,
    Vertical,
    Overhang,
    Roof,
    Cave
}

public static class Grades
{
    public const int Min = 0;
    public const int Max = 10;

    public static string Label(int grade) => $"V{grade}";
}
