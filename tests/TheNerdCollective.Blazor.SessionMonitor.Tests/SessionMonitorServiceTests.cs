using Microsoft.Extensions.Options;
using TheNerdCollective.Blazor.SessionMonitor;
using Xunit;

namespace TheNerdCollective.Blazor.SessionMonitor.Tests;

/// <summary>
/// Documents circuit-counting semantics used by /developer/monitor.
/// ActiveSessions counts connected circuits only; disconnected retained circuits
/// stay in the registry until OnCircuitClosed and are exposed via DisconnectedSessions.
/// </summary>
public sealed class SessionMonitorServiceTests
{
    [Fact]
    public void Page_reload_increments_started_and_keeps_disconnected_circuit_in_active()
    {
        var monitor = CreateMonitor();

        monitor.OnCircuitOpened("circuit-old", "/events/2783/10545");
        monitor.OnConnectionDown("circuit-old");
        monitor.OnCircuitOpened("circuit-new", "/events/2783/10545");

        var metrics = monitor.GetCurrentMetrics();

        Assert.Equal(2, metrics.TotalSessionsStarted);
        Assert.Equal(2, metrics.TotalCircuitsOpened);
        Assert.Equal(0, metrics.TotalSessionsEnded);
        Assert.Equal(1, metrics.ActiveSessions);
        Assert.Equal(1, metrics.DisconnectedSessions);
        Assert.Equal(1, metrics.TotalDisconnects);
    }

    [Fact]
    public void Retention_close_after_reload_drops_active_but_started_stays()
    {
        var monitor = CreateMonitor();

        monitor.OnCircuitOpened("circuit-old", "/");
        monitor.OnConnectionDown("circuit-old");
        monitor.OnCircuitOpened("circuit-new", "/");
        monitor.OnCircuitClosed("circuit-old");

        var metrics = monitor.GetCurrentMetrics();

        Assert.Equal(2, metrics.TotalSessionsStarted);
        Assert.Equal(2, metrics.TotalCircuitsOpened);
        Assert.Equal(1, metrics.TotalSessionsEnded);
        Assert.Equal(1, metrics.ActiveSessions);
        Assert.Equal(0, metrics.DisconnectedSessions);
    }

    [Fact]
    public void Same_circuit_reconnect_does_not_increment_started()
    {
        var monitor = CreateMonitor();

        monitor.OnCircuitOpened("circuit-1", "/developer/monitor");
        monitor.OnConnectionDown("circuit-1");
        monitor.OnConnectionUp("circuit-1");

        var metrics = monitor.GetCurrentMetrics();

        Assert.Equal(1, metrics.TotalSessionsStarted);
        Assert.Equal(1, metrics.TotalCircuitsOpened);
        Assert.Equal(1, metrics.ActiveSessions);
        Assert.Equal(0, metrics.DisconnectedSessions);
        Assert.Equal(1, metrics.TotalDisconnects);
        Assert.Equal(1, metrics.TotalReconnects);
    }

    [Fact]
    public void Average_duration_is_computed_when_circuit_closes()
    {
        var monitor = CreateMonitor();

        monitor.OnCircuitOpened("circuit-1", "/");
        Thread.Sleep(50);
        monitor.OnCircuitClosed("circuit-1");

        var metrics = monitor.GetCurrentMetrics();

        Assert.Equal(1, metrics.TotalSessionsEnded);
        Assert.Equal(0, metrics.ActiveSessions);
        Assert.NotNull(metrics.AverageSessionDurationSeconds);
        Assert.True(metrics.AverageSessionDurationSeconds >= 0.05);
    }

    [Fact]
    public void Active_count_excludes_disconnected_retained_circuits()
    {
        var monitor = CreateMonitor();

        monitor.OnCircuitOpened("connected", "/booking");
        monitor.OnCircuitOpened("zombie", "/booking");
        monitor.OnConnectionDown("zombie");

        var metrics = monitor.GetCurrentMetrics();

        Assert.Equal(1, metrics.ActiveSessions);
        Assert.Equal(1, metrics.DisconnectedSessions);
    }

    [Fact]
    public void Reload_same_clientId_does_not_increment_started_but_increments_circuits_opened()
    {
        const string clientId = "client-abc-123";
        var monitor = CreateMonitor();

        monitor.OnCircuitOpened("circuit-old", "/events/2783/10545", clientId);
        monitor.OnConnectionDown("circuit-old");
        monitor.OnCircuitOpened("circuit-new", "/events/2783/10545", clientId);

        var metrics = monitor.GetCurrentMetrics();

        Assert.Equal(1, metrics.TotalSessionsStarted);
        Assert.Equal(2, metrics.TotalCircuitsOpened);
        Assert.Equal(1, metrics.ActiveSessions);
        Assert.Equal(1, metrics.DisconnectedSessions);
    }

    [Fact]
    public void Reload_same_clientId_after_retention_close_still_deduplicates_started()
    {
        const string clientId = "client-abc-123";
        var monitor = CreateMonitor();

        monitor.OnCircuitOpened("circuit-old", "/", clientId);
        monitor.OnConnectionDown("circuit-old");
        monitor.OnCircuitOpened("circuit-new", "/", clientId);
        monitor.OnCircuitClosed("circuit-old");

        var metrics = monitor.GetCurrentMetrics();

        Assert.Equal(1, metrics.TotalSessionsStarted);
        Assert.Equal(2, metrics.TotalCircuitsOpened);
        Assert.Equal(1, metrics.TotalSessionsEnded);
        Assert.Equal(1, metrics.ActiveSessions);
    }

    [Fact]
    public void GetCurrentMetrics_includes_instance_id_from_override()
    {
        var monitor = new SessionMonitorService(Options.Create(new SessionMonitorOptions
        {
            InstanceIdOverride = "test-replica-1"
        }));

        var metrics = monitor.GetCurrentMetrics();

        Assert.Equal("test-replica-1", metrics.InstanceId);
        Assert.Equal(Environment.MachineName, metrics.MachineName);
    }

    [Fact]
    public void GetCurrentMetrics_resolves_instance_id_from_environment()
    {
        const string envVar = "CONTAINER_APP_REPLICA_NAME";
        var previous = Environment.GetEnvironmentVariable(envVar);
        Environment.SetEnvironmentVariable(envVar, "replica-from-env");

        try
        {
            var monitor = CreateMonitor();
            var metrics = monitor.GetCurrentMetrics();

            Assert.Equal("replica-from-env", metrics.InstanceId);
        }
        finally
        {
            Environment.SetEnvironmentVariable(envVar, previous);
        }
    }

    [Fact]
    public void GetActiveSessionsByClient_groups_tabs_from_same_browser()
    {
        const string clientId = "client-abc-1234567890";
        var monitor = CreateMonitor();

        monitor.OnCircuitOpened("circuit-tab-1", "/developer/monitor", clientId);
        monitor.OnCircuitOpened("circuit-tab-2", "/events", clientId);
        monitor.OnCircuitOpened("circuit-other", "/");

        var summaries = monitor.GetActiveSessionsByClient().ToList();
        var sameBrowser = summaries.Single(s => s.ClientId == clientId);

        Assert.Equal(2, sameBrowser.ActiveSessionCount);
        Assert.Equal(2, sameBrowser.ConnectedCount);
        Assert.Equal(2, sameBrowser.DistinctPathCount);
        Assert.Equal(2, summaries.Count);
    }

    [Fact]
    public void GetActiveSessions_includes_client_label()
    {
        const string clientId = "abcdef1234567890";
        var monitor = CreateMonitor();

        monitor.OnCircuitOpened("circuit-1", "/developer/monitor", clientId);

        var session = monitor.GetActiveSessions().Single();

        Assert.Equal(clientId, session.ClientId);
        Assert.Equal("abcdef12", session.ClientLabel);
    }

    private static SessionMonitorService CreateMonitor()
        => new(Options.Create(new SessionMonitorOptions()));
}
