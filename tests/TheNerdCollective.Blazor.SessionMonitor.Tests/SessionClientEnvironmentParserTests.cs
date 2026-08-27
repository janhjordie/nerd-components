using TheNerdCollective.Blazor.SessionMonitor;
using Xunit;

namespace TheNerdCollective.Blazor.SessionMonitor.Tests;

public class SessionClientEnvironmentParserTests
{
    [Fact]
    public void Parse_detects_headless_playwright_chrome_on_desktop_mac()
    {
        const string userAgent =
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) HeadlessChrome/133.0.0.0 Safari/537.36";

        var environment = SessionClientEnvironmentParser.Parse(
            userAgent,
            secChUa: "\"Not(A:Brand\";v=\"99\", \"HeadlessChrome\";v=\"133\", \"Chromium\";v=\"133\"",
            secChUaMobile: "?0",
            secChUaPlatform: "\"macOS\"");

        Assert.Equal("Chrome", environment.Browser);
        Assert.Equal("Desktop", environment.FormFactor);
        Assert.Equal("macOS", environment.Platform);
        Assert.True(environment.IsAutomation);
        Assert.Equal("Headless", environment.AutomationKind);
    }

    [Fact]
    public void Parse_detects_explicit_playwright_user_agent()
    {
        const string userAgent = "Billetsalg-Playwright-E2E/1.0 (integration tests) Chrome/133.0.0.0";

        var environment = SessionClientEnvironmentParser.Parse(userAgent);

        Assert.Equal("Chrome", environment.Browser);
        Assert.True(environment.IsAutomation);
        Assert.Equal("Playwright", environment.AutomationKind);
    }

    [Fact]
    public void Parse_detects_mobile_android_chrome()
    {
        const string userAgent =
            "Mozilla/5.0 (Linux; Android 14; Pixel 8) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/133.0.0.0 Mobile Safari/537.36";

        var environment = SessionClientEnvironmentParser.Parse(
            userAgent,
            secChUaMobile: "?1",
            secChUaPlatform: "\"Android\"");

        Assert.Equal("Chrome", environment.Browser);
        Assert.Equal("Mobile", environment.FormFactor);
        Assert.Equal("Android", environment.Platform);
        Assert.False(environment.IsAutomation);
    }

    [Fact]
    public void Parse_detects_ipad_tablet()
    {
        const string userAgent =
            "Mozilla/5.0 (iPad; CPU OS 17_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.0 Mobile/15E148 Safari/604.1";

        var environment = SessionClientEnvironmentParser.Parse(userAgent);

        Assert.Equal("Safari", environment.Browser);
        Assert.Equal("Tablet", environment.FormFactor);
        Assert.Equal("iPadOS", environment.Platform);
    }

    [Fact]
    public void OnCircuitOpened_stores_client_environment_on_summary()
    {
        var monitor = new SessionMonitorService(Microsoft.Extensions.Options.Options.Create(new SessionMonitorOptions()));
        var environment = SessionClientEnvironmentParser.Parse(
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) HeadlessChrome/133.0.0.0 Safari/537.36");

        monitor.OnCircuitOpened("pw-test", "/events", "playwright-client", environment);

        var summary = monitor.GetActiveSessionsByClient().Single();

        Assert.Equal("Chrome", summary.ClientEnvironment.Browser);
        Assert.Equal("Desktop", summary.ClientEnvironment.FormFactor);
        Assert.True(summary.ClientEnvironment.IsAutomation);
    }
}
