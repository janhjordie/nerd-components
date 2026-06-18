using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace TheNerdCollective.Blazor.ThemeKit;

public sealed class ThemeEditorGate : IThemeEditorGate
{
    public ThemeEditorGate(
        IHostEnvironment environment,
        IConfiguration configuration,
        IOptions<MudThemeKitOptions> options)
    {
        var kitOptions = options.Value;
        var configEnabled = configuration.GetValue<bool?>("Ui:ThemeEditor:Enabled") ?? false;
        var allowSwitcherInProduction = configuration.GetValue<bool>("Ui:ThemeEditor:AllowSwitcherInProduction");

        var isDevOrTest = environment.IsDevelopment()
            || environment.IsEnvironment("Test")
            || environment.IsEnvironment("Staging");

        if (kitOptions.PlaybookMode)
        {
            IsEditorEnabled = true;
            IsSwitcherEnabled = true;
            return;
        }

        IsEditorEnabled = configEnabled || isDevOrTest;
        IsSwitcherEnabled = IsEditorEnabled || allowSwitcherInProduction;
    }

    public bool IsEditorEnabled { get; }

    public bool IsSwitcherEnabled { get; }
}
