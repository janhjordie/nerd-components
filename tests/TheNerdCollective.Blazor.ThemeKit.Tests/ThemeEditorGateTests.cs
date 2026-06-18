using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TheNerdCollective.Blazor.ThemeKit;
using Xunit;

namespace TheNerdCollective.Blazor.ThemeKit.Tests;

public class ThemeEditorGateTests
{
    [Fact]
    public void Development_enables_editor_and_switcher()
    {
        var gate = CreateGate(Environments.Development, playbookMode: false, configEnabled: false);

        Assert.True(gate.IsEditorEnabled);
        Assert.True(gate.IsSwitcherEnabled);
    }

    [Fact]
    public void Production_disables_editor_by_default()
    {
        var gate = CreateGate(Environments.Production, playbookMode: false, configEnabled: false);

        Assert.False(gate.IsEditorEnabled);
        Assert.False(gate.IsSwitcherEnabled);
    }

    [Fact]
    public void Test_environment_enables_editor()
    {
        var gate = CreateGate("Test", playbookMode: false, configEnabled: false);

        Assert.True(gate.IsEditorEnabled);
        Assert.True(gate.IsSwitcherEnabled);
    }

    [Fact]
    public void Playbook_mode_always_enables_editor()
    {
        var gate = CreateGate(Environments.Production, playbookMode: true, configEnabled: false);

        Assert.True(gate.IsEditorEnabled);
        Assert.True(gate.IsSwitcherEnabled);
    }

    private static ThemeEditorGate CreateGate(string environmentName, bool playbookMode, bool configEnabled)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Ui:ThemeEditor:Enabled"] = configEnabled ? "true" : "false",
            })
            .Build();

        var environment = new HostEnvironmentStub(environmentName);
        var options = Options.Create(new MudThemeKitOptions { PlaybookMode = playbookMode });

        return new ThemeEditorGate(environment, configuration, options);
    }

    private sealed class HostEnvironmentStub(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;

        public string ApplicationName { get; set; } = "tests";

        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

        public IFileProvider ContentRootFileProvider { get; set; } = null!;
    }
}
