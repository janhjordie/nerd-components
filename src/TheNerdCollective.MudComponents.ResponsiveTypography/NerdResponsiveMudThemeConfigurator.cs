using MudBlazor;
using TheNerdCollective.MudComponents.DesignTokens;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.ResponsiveTypography;

/// <summary>
/// Applies responsive typography to <see cref="MudTheme"/> instances (HR-173).
/// </summary>
public sealed class NerdResponsiveMudThemeConfigurator : INerdMudThemeConfigurator
{
    private readonly NerdResponsiveTypographyOptions _typographyOptions;
    private readonly NerdResponsiveTypographyCss _typographyCss;
    private readonly NerdDesignSystemOptions _hubOptions;

    public NerdResponsiveMudThemeConfigurator(
        NerdResponsiveTypographyOptions typographyOptions,
        NerdResponsiveTypographyCss typographyCss,
        NerdDesignSystemOptions hubOptions)
    {
        _typographyOptions = typographyOptions;
        _typographyCss = typographyCss;
        _hubOptions = hubOptions;
    }

    public void Configure(MudTheme theme) =>
        theme.UseResponsiveTypography(_typographyOptions.Typography);

    public void OnBrandPackApplied(string brandPackId, MudTheme theme)
    {
        NerdBrandTypographySwitcher.TrySwitchBrand(brandPackId, _typographyOptions, _hubOptions, theme);
        _typographyCss.Update(_typographyOptions.Typography);
    }
}
