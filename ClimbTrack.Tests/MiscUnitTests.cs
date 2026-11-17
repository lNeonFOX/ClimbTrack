using ClimbTrack.Web.Models;
using ClimbTrack.Web.Services;
using ClimbTrack.Web.State;

namespace ClimbTrack.Tests;

public class MiscUnitTests
{
    [Fact]
    public void FilterState_NotifiesSubscribers_AndResetsPageOnFilterChange()
    {
        var state = new RouteFilterState();
        var notifications = 0;
        state.Changed += () => notifications++;

        state.SetPage(3);
        state.SetGrade(4);

        Assert.Equal(2, notifications);
        Assert.Equal(1, state.Page);
        Assert.Equal(4, state.ToQuery().Grade);

        state.SetGrade(4);               // unchanged value: no notification
        Assert.Equal(2, notifications);
    }

    [Theory]
    [InlineData(null, "/")]
    [InlineData("", "/")]
    [InlineData("journal", "/journal")]
    [InlineData("/dashboard", "/dashboard")]
    [InlineData("https://evil.example", "/")]
    [InlineData("//evil.example", "/")]
    [InlineData("/\\evil.example", "/")]
    public void LocalUrl_OnlyAllowsLocalTargets(string? input, string expected) =>
        Assert.Equal(expected, LocalUrl.Sanitize(input));

    [Fact]
    public void InputValidator_ReportsErrors()
    {
        Assert.NotNull(InputValidator.Validate(new ClimbInput { RouteId = 0, Attempts = 0 }));
        Assert.Null(InputValidator.Validate(new ClimbInput { RouteId = 1, Attempts = 3 }));
    }
}
