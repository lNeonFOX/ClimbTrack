using ClimbTrack.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClimbTrack.Web.Data;

public static class DbSeeder
{
    public const string AdminRole = "Admin";

    public static async Task SeedAsync(IServiceProvider services, IConfiguration config)
    {
        using var scope = services.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
        await using var db = await factory.CreateDbContextAsync();

        await db.Database.EnsureCreatedAsync();

        if (!await db.ClimbingRoutes.AnyAsync())
        {
            db.ClimbingRoutes.AddRange(SampleRoutes());
            await db.SaveChangesAsync();
        }

        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        if (!await roles.RoleExistsAsync(AdminRole))
            await roles.CreateAsync(new IdentityRole(AdminRole));

        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var admin = await EnsureUserAsync(users, config["Seed:AdminEmail"], config["Seed:AdminPassword"]);
        if (admin is not null && !await users.IsInRoleAsync(admin, AdminRole))
            await users.AddToRoleAsync(admin, AdminRole);

        var demo = await EnsureUserAsync(users, config["Seed:DemoEmail"], config["Seed:DemoPassword"]);
        if (demo is not null && !await db.ClimbRecords.AnyAsync(r => r.UserId == demo.Id))
        {
            var routes = await db.ClimbingRoutes.OrderBy(r => r.Grade).ThenBy(r => r.Name).Take(10).ToListAsync();
            var today = DateOnly.FromDateTime(DateTime.Today);
            for (var i = 0; i < routes.Count; i++)
            {
                db.ClimbRecords.Add(new ClimbRecord
                {
                    UserId = demo.Id,
                    RouteId = routes[i].Id,
                    Date = today.AddDays(-i * 2),
                    Attempts = 1 + i % 4,
                    IsCompleted = i % 3 != 2
                });
            }
            await db.SaveChangesAsync();
        }
    }

    private static async Task<ApplicationUser?> EnsureUserAsync(UserManager<ApplicationUser> users, string? email, string? password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)) return null;

        var existing = await users.FindByEmailAsync(email);
        if (existing is not null) return existing;

        var user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
        var result = await users.CreateAsync(user, password);
        return result.Succeeded ? user : null;
    }

    private static IEnumerable<ClimbingRoute> SampleRoutes()
    {
        (string Name, int Grade, WallType Wall, string Desc)[] data =
        [
            ("Chalk Dust", 0, WallType.Slab, "Friendly slab with big footholds."),
            ("First Steps", 0, WallType.Vertical, "Jug haul for absolute beginners."),
            ("Warm-Up Wiggle", 0, WallType.Slab, "Easy traverse to loosen up."),
            ("Green Ladder", 1, WallType.Vertical, "Straight-up climb on large holds."),
            ("Soft Landing", 1, WallType.Slab, "Balance and footwork practice."),
            ("Tiny Roof", 1, WallType.Overhang, "A gentle introduction to steepness."),
            ("Sunrise Slab", 1, WallType.Slab, "Delicate smears with a big finishing hold."),
            ("Pocket Rocket", 2, WallType.Vertical, "Pockets and a long reach."),
            ("Moon Walk", 2, WallType.Slab, "Trust your feet on small edges."),
            ("Ladder Rungs", 2, WallType.Vertical, "Dynamic but forgiving moves."),
            ("Easy Overhang", 2, WallType.Overhang, "Big holds on a 20 degree wall."),
            ("Crimp Candy", 3, WallType.Vertical, "First real crimps."),
            ("Heel Hook Hello", 3, WallType.Overhang, "Introduces heel hooks."),
            ("Mantle Madness", 3, WallType.Vertical, "Mantle to finish."),
            ("Corner Cut", 3, WallType.Slab, "Compression in a blunt arete."),
            ("Dyno Dreams", 4, WallType.Vertical, "Your first proper dyno."),
            ("Sloper Soup", 4, WallType.Overhang, "Slopers all the way."),
            ("Tension Test", 4, WallType.Roof, "Core-heavy roof sequence."),
            ("Pinch Panic", 5, WallType.Vertical, "Pinches and a tricky sequence."),
            ("Cave Entrance", 5, WallType.Cave, "Steep hand-foot matching."),
            ("Compression Zone", 5, WallType.Overhang, "Squeeze between volumes."),
            ("Knee Bar Bandit", 6, WallType.Overhang, "Rest on the knee bar, then go."),
            ("Crux Corner", 6, WallType.Vertical, "Tiny holds, huge commitment."),
            ("Roof Rat", 6, WallType.Roof, "Long roof with toe hooks."),
            ("Power Plant", 7, WallType.Cave, "Raw power and body tension."),
            ("Razor Edge", 7, WallType.Slab, "Skin-testing edges."),
            ("Gravity Check", 8, WallType.Overhang, "Powerful moves between poor holds."),
            ("The Dark Side", 8, WallType.Cave, "A cave classic for strong climbers."),
            ("Summit Fever", 9, WallType.Roof, "Technical roof problem."),
            ("Apex Predator", 10, WallType.Cave, "The hardest problem in the gym.")
        ];

        return data.Select(d => new ClimbingRoute { Name = d.Name, Grade = d.Grade, WallType = d.Wall, Description = d.Desc });
    }
}
