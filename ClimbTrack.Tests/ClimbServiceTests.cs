using ClimbTrack.Web.Models;
using ClimbTrack.Web.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace ClimbTrack.Tests;

public class ClimbServiceTests : IDisposable
{
    private static readonly DateTimeOffset Now = new(2026, 11, 17, 10, 0, 0, TimeSpan.Zero);
    private readonly TestDb _db = new();
    private readonly ClimbService _service;

    public ClimbServiceTests() =>
        _service = new ClimbService(_db, new FixedTime(Now), NullLogger<ClimbService>.Instance);

    public void Dispose() => _db.Dispose();

    private static ClimbInput Input(int routeId, DateOnly? date = null, int attempts = 2) =>
        new() { RouteId = routeId, Date = date ?? new DateOnly(2026, 11, 10), Attempts = attempts, IsCompleted = true };

    [Fact]
    public async Task Add_ValidInput_IsStoredForTheUser()
    {
        var user = await _db.AddUserAsync("u1");
        var route = await _db.AddRouteAsync("R1", 2);

        var result = await _service.AddAsync(user, Input(route.Id));

        Assert.True(result.Success);
        var page = await _service.GetPageAsync(user, 1, 10);
        Assert.Single(page.Items);
        Assert.Equal("R1", page.Items[0].RouteName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    [InlineData(101)]
    public async Task Add_InvalidAttempts_IsRejected(int attempts)
    {
        var user = await _db.AddUserAsync("u1");
        var route = await _db.AddRouteAsync("R1", 2);

        var result = await _service.AddAsync(user, Input(route.Id, attempts: attempts));

        Assert.False(result.Success);
        Assert.Empty((await _service.GetPageAsync(user, 1, 10)).Items);
    }

    [Fact]
    public async Task Add_FutureDate_IsRejected()
    {
        var user = await _db.AddUserAsync("u1");
        var route = await _db.AddRouteAsync("R1", 2);

        var result = await _service.AddAsync(user, Input(route.Id, new DateOnly(2026, 12, 25)));

        Assert.False(result.Success);
    }

    [Fact]
    public async Task Add_UnknownRoute_IsRejected()
    {
        var user = await _db.AddUserAsync("u1");
        Assert.False((await _service.AddAsync(user, Input(9999))).Success);
    }

    [Fact]
    public async Task Update_AnotherUsersRecord_IsRejected()
    {
        var owner = await _db.AddUserAsync("owner");
        var intruder = await _db.AddUserAsync("intruder");
        var route = await _db.AddRouteAsync("R1", 2);
        await _service.AddAsync(owner, Input(route.Id));
        var id = (await _service.GetPageAsync(owner, 1, 10)).Items[0].Id;

        var result = await _service.UpdateAsync(intruder, id, Input(route.Id, attempts: 9));

        Assert.False(result.Success);
        Assert.Equal(2, (await _service.GetPageAsync(owner, 1, 10)).Items[0].Attempts);
    }

    [Fact]
    public async Task Delete_OnlyWorksForTheOwner()
    {
        var owner = await _db.AddUserAsync("owner");
        var intruder = await _db.AddUserAsync("intruder");
        var route = await _db.AddRouteAsync("R1", 2);
        await _service.AddAsync(owner, Input(route.Id));
        var id = (await _service.GetPageAsync(owner, 1, 10)).Items[0].Id;

        Assert.False((await _service.DeleteAsync(intruder, id)).Success);
        Assert.Single((await _service.GetPageAsync(owner, 1, 10)).Items);

        Assert.True((await _service.DeleteAsync(owner, id)).Success);
        Assert.Empty((await _service.GetPageAsync(owner, 1, 10)).Items);
    }

    [Fact]
    public async Task Update_OwnRecord_ChangesValues()
    {
        var user = await _db.AddUserAsync("u1");
        var route = await _db.AddRouteAsync("R1", 2);
        await _service.AddAsync(user, Input(route.Id));
        var id = (await _service.GetPageAsync(user, 1, 10)).Items[0].Id;

        var result = await _service.UpdateAsync(user, id, Input(route.Id, attempts: 7));

        Assert.True(result.Success);
        Assert.Equal(7, (await _service.GetPageAsync(user, 1, 10)).Items[0].Attempts);
    }
}
