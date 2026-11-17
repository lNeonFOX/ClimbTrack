namespace ClimbTrack.Web.Services;

public static class LocalUrl
{
    /// <summary>Returns a safe, app-relative URL for post-login redirects (prevents open redirects).</summary>
    public static string Sanitize(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return "/";
        var u = url.Trim();
        if (u.Contains("://") || u.StartsWith("//") || u.Contains('\\')) return "/";
        return u.StartsWith('/') ? u : "/" + u;
    }
}
