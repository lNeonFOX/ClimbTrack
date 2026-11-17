using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;

namespace ClimbTrack.Tests;

/// <summary>Boots the real app against a temporary SQLite file (seeded on startup).</summary>
public class ClimbTrackFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"climbtrack-test-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:Default", $"Data Source={_dbPath}");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        SqliteConnection.ClearAllPools();
        try { File.Delete(_dbPath); } catch (IOException) { /* best effort */ }
    }
}

public class IntegrationTests : IClassFixture<ClimbTrackFactory>
{
    private readonly HttpClient _client;

    public IntegrationTests(ClimbTrackFactory factory) =>
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    [Fact]
    public async Task Api_Routes_ReturnsSeededRoutesWithPaging()
    {
        var json = await _client.GetFromJsonAsync<JsonElement>("/api/routes?pageSize=5&grade=0");

        var total = json.GetProperty("totalCount").GetInt32();
        Assert.True(total > 0);
        Assert.Equal(Math.Min(5, total), json.GetProperty("items").GetArrayLength());
        Assert.All(json.GetProperty("items").EnumerateArray(), item => Assert.Equal(0, item.GetProperty("grade").GetInt32()));
    }

    [Fact]
    public async Task Api_Climbs_RequiresAuthentication()
    {
        var response = await _client.GetAsync("/api/climbs");

        Assert.True(response.StatusCode is HttpStatusCode.Redirect or HttpStatusCode.Unauthorized or HttpStatusCode.NotFound,
            $"Unexpected status {response.StatusCode}");
        Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task RoutesPage_IsPublic_AndRendersCatalog()
    {
        var response = await _client.GetAsync("/routes");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Route catalog", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task HealthEndpoint_ReportsHealthy()
    {
        var response = await _client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
