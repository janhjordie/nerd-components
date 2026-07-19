using MudBlazor;

namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>
/// Optional hook for host packages (e.g. responsive typography) to enrich <see cref="MudTheme"/> (HR-173).
/// </summary>
public interface INerdMudThemeConfigurator
{
    void Configure(MudTheme theme);

    void OnBrandPackApplied(string brandPackId, MudTheme theme);
}

internal sealed class NullNerdMudThemeConfigurator : INerdMudThemeConfigurator
{
    public void Configure(MudTheme theme) => ArgumentNullException.ThrowIfNull(theme);

    public void OnBrandPackApplied(string brandPackId, MudTheme theme) => ArgumentNullException.ThrowIfNull(theme);
}
