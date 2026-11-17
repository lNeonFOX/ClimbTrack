using ClimbTrack.Web.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ClimbTrack.Web.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<ClimbingRoute> ClimbingRoutes => Set<ClimbingRoute>();
    public DbSet<ClimbRecord> ClimbRecords => Set<ClimbRecord>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ClimbingRoute>(e =>
        {
            e.HasIndex(r => r.Name).IsUnique();
            e.HasIndex(r => new { r.Grade, r.WallType });
            e.Property(r => r.WallType).HasConversion<string>().HasMaxLength(20);
        });

        builder.Entity<ClimbRecord>(e =>
        {
            e.HasIndex(r => new { r.UserId, r.Date });
            e.HasIndex(r => new { r.UserId, r.RouteId });
            e.HasOne(r => r.User).WithMany(u => u.Records).HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(r => r.Route).WithMany(r => r.Records).HasForeignKey(r => r.RouteId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
