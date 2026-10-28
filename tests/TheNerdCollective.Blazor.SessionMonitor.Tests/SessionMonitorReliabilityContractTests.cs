using TheNerdCollective.Blazor.SessionMonitor;
using Xunit;

namespace TheNerdCollective.Blazor.SessionMonitor.Tests;

public sealed class SessionMonitorReliabilityContractTests
{
    [Fact]
    public void Admin_prefix_does_not_match_a_longer_sibling_path()
    {
        var prefixes = new[] { "/developer/monitor" };

        Assert.True(SessionPathNormalizer.MatchesPathPrefix("/developer/monitor", prefixes));
        Assert.True(SessionPathNormalizer.MatchesPathPrefix("/developer/monitor?x=1", prefixes));
        Assert.True(SessionPathNormalizer.MatchesPathPrefix("/developer/monitor/details", prefixes));
        Assert.False(SessionPathNormalizer.MatchesPathPrefix("/developer/monitoring", prefixes));
    }

    [Fact]
    public void Query_limits_reject_zero_and_oversized_windows()
    {
        Assert.NotNull(SessionMonitorQueryLimits.ValidateHistoryCount(0));
        Assert.NotNull(SessionMonitorQueryLimits.ValidateDeploymentWindow(0, 24));
        Assert.Null(SessionMonitorQueryLimits.ValidateDeploymentWindow(5, 24));
    }

    [Fact]
    public void Bootstrap_script_shares_storage_and_does_not_invent_an_id_without_it()
    {
        var script = File.ReadAllText(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..",
            "src", "TheNerdCollective.Blazor.SessionMonitor", "wwwroot", "js", "session-monitor-client.js"));

        Assert.Contains("localStorage", script, StringComparison.Ordinal);
        Assert.Contains("Path=/", script, StringComparison.Ordinal);
        Assert.Contains("if (!clientId)", script, StringComparison.Ordinal);
    }
}
