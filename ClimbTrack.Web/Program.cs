using System.Security.Claims;
using System.Text.Json.Serialization;
using ClimbTrack.Web.Components;
using ClimbTrack.Web.Data;
using ClimbTrack.Web.Models;
using ClimbTrack.Web.Services;
using ClimbTrack.Web.State;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHealthChecks();
builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// --- Persistence ---------------------------------------------------------------
// A DbContext factory gives each service call its own short-lived context (the recommended
// pattern for Blazor Server). Identity needs a scoped context, so one is also exposed.
// The connection string is read lazily so tests can override it.
builder.Services.AddDbContextFactory<AppDbContext>((sp, options) =>
    options.UseSqlite(sp.GetRequiredService<IConfiguration>().GetConnectionString("Default") ?? "Data Source=climbtrack.db"));
builder.Services.AddScoped(sp => sp.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());

// --- Authentication / authorization --------------------------------------------
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
}).AddIdentityCookies();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 8;
        options.Lockout.MaxFailedAccessAttempts = 5;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddSignInManager();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/Login";
    options.Cookie.HttpOnly = true;
    options.SlidingExpiration = true;
});
builder.Services.AddAuthorization();

// --- Application services ------------------------------------------------------
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<RouteService>();
builder.Services.AddScoped<ClimbService>();
builder.Services.AddScoped<StatisticsService>();
builder.Services.AddScoped<RecommendationService>();
builder.Services.AddScoped<RouteFilterState>();

var app = builder.Build();

await DbSeeder.SeedAsync(app.Services, app.Configuration);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.MapHealthChecks("/health");

// --- Minimal JSON API (documented in README) -----------------------------------
app.MapGet("/api/routes", async (RouteService routes, int? grade, WallType? wall, string? q, int page = 1, int pageSize = 12) =>
    Results.Ok(await routes.SearchAsync(new RouteQuery(grade, wall, q, page, pageSize))));

app.MapGet("/api/climbs", async (ClaimsPrincipal user, ClimbService climbs, int page = 1, int pageSize = 20) =>
{
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
    return userId is null ? Results.Unauthorized() : Results.Ok(await climbs.GetPageAsync(userId, page, pageSize));
}).RequireAuthorization();

// Sign-out must be a plain HTTP request (cookies cannot be changed from an interactive circuit).
// Binding a [FromForm] parameter makes ASP.NET Core validate the antiforgery token automatically.
app.MapPost("/account/logout", async (SignInManager<ApplicationUser> signInManager, [FromForm] string? returnUrl) =>
{
    await signInManager.SignOutAsync();
    return Results.LocalRedirect("~/");
});

app.Run();

public partial class Program { }
