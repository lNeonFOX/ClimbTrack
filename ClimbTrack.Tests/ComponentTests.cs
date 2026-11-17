using Bunit;
using ClimbTrack.Web.Components.Shared;
using ClimbTrack.Web.Models;
using Microsoft.AspNetCore.Components;

namespace ClimbTrack.Tests;

public class ComponentTests : Bunit.TestContext
{
    [Theory]
    [InlineData(1, "tier-1")]
    [InlineData(4, "tier-2")]
    [InlineData(7, "tier-3")]
    [InlineData(10, "tier-4")]
    public void GradeBadge_ShowsLabelAndTier(int grade, string tier)
    {
        var cut = RenderComponent<GradeBadge>(p => p.Add(x => x.Grade, grade));

        var span = cut.Find("span");
        Assert.Equal($"V{grade}", span.TextContent.Trim());
        Assert.Contains(tier, span.ClassName);
    }

    [Fact]
    public void RouteCard_RendersDetails_AndOptionalLogAction()
    {
        var route = new ClimbingRoute { Id = 7, Name = "Moon Walk", Grade = 2, WallType = WallType.Slab, Description = "Small edges" };

        var without = RenderComponent<RouteCard>(p => p.Add(x => x.Route, route));
        Assert.Contains("Moon Walk", without.Markup);
        Assert.DoesNotContain("Log climb", without.Markup);

        var with = RenderComponent<RouteCard>(p => p.Add(x => x.Route, route).Add(x => x.ShowActions, true));
        Assert.Contains("routeId=7", with.Markup);
    }

    [Fact]
    public void Pager_RaisesPageChanged_AndDisablesBoundaries()
    {
        var page = 0;
        var cut = RenderComponent<Pager>(p => p
            .Add(x => x.Page, 1)
            .Add(x => x.TotalPages, 3)
            .Add(x => x.PageChanged, EventCallback.Factory.Create<int>(this, v => page = v)));

        Assert.True(cut.Find("button[aria-label='Previous page']").HasAttribute("disabled"));
        cut.Find("button[aria-label='Next page']").Click();
        Assert.Equal(2, page);
    }

    [Fact]
    public void Pager_RendersNothing_ForASinglePage()
    {
        var cut = RenderComponent<Pager>(p => p.Add(x => x.Page, 1).Add(x => x.TotalPages, 1));
        Assert.Empty(cut.FindAll("nav"));
    }

    [Fact]
    public void GradeChart_RendersOneColumnPerGrade()
    {
        var data = Enumerable.Range(0, 11).ToDictionary(g => g, g => g == 3 ? 4 : 0);

        var cut = RenderComponent<GradeChart>(p => p.Add(x => x.Data, data));

        Assert.Equal(11, cut.FindAll(".chart-col").Count);
        Assert.Contains("height:100%", cut.Markup);
    }

    [Fact]
    public void StatCard_ShowsTitleValueAndHint()
    {
        var cut = RenderComponent<StatCard>(p => p
            .Add(x => x.Title, "Best grade").Add(x => x.Value, "V4").Add(x => x.Hint, "Highest send"));

        Assert.Contains("Best grade", cut.Markup);
        Assert.Contains("V4", cut.Markup);
        Assert.Contains("Highest send", cut.Markup);
    }
}
