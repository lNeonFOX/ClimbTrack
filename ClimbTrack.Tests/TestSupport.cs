using ClimbTrack.Web.Data;
using ClimbTrack.Web.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ClimbTrack.Tests;

/// <summary>In-memory SQLite database shared by every context created from this factory (the connection stays open).</summary>
public sealed class TestDb : IDbContextFactory<AppDbContext>, IDisposable
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    public TestDb()
    {
        _connection.Open();
        using var db = CreateDbContext();
        db.Database.EnsureCreated();
    }

    public AppDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options);

    public async Task<string> AddUserAsync(string id)
    {
        await using var db = CreateDbContext();
        db.Users.Add(new ApplicationUser { Id = id, UserName = id, NormalizedUserName = id.ToUpperInvariant() });
        await db.SaveChangesAsync();
        return id;
    }

    public async Task<ClimbingRoute> AddRouteAsync(string name, int grade, WallType wall = WallType.Vertical)
    {
        await using var db = CreateDbContext();
        var route = new ClimbingRoute { Name = name, Grade = grade, WallType = wall, Description = "test" };
        db.ClimbingRoutes.Add(route);
        await db.SaveChangesAsync();
        return route;
    }

    public void Dispose() => _connection.Dispose();
}

public sealed class FixedTime(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;
}
