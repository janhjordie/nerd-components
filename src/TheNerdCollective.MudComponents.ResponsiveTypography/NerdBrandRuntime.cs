using MudBlazor;
using TheNerdCollective.MudComponents.DesignTokens;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.ResponsiveTypography;

/// <summary>
/// Restores the configured default design-token brand when a new Blazor circuit starts (app load or F5).
/// </summary>
public static class NerdBrandRuntime
{
    public static void RestoreDefaultBrand(
        NerdDesignTokenOptions tokenOptions,
        NerdDesignSystemOptions hubOptions,
        NerdResponsiveTypographyOptions typographyOptions,
        NerdDesignTokenCss tokenCss,
        NerdResponsiveTypographyCss typographyCss,
        MudTheme? theme = null)
    {
        ArgumentNullException.ThrowIfNull(tokenOptions);
        ArgumentNullException.ThrowIfNull(hubOptions);
        ArgumentNullException.ThrowIfNull(typographyOptions);
        ArgumentNullException.ThrowIfNull(tokenCss);
        ArgumentNullException.ThrowIfNull(typographyCss);

        var brandId = tokenOptions.DefaultBrandPackId;
        if (string.IsNullOrWhiteSpace(brandId))
        {
            return;
        }

        if (string.Equals(tokenOptions.ActiveBrandPackId, brandId, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(hubOptions.ActiveTokenPackId, brandId, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        NerdBrandPackRegistry.Instance.Configure(brandId, tokenOptions);
        tokenCss.Update(tokenOptions);
        hubOptions.ActiveTokenPackId = tokenOptions.ActiveBrandPackId;
        hubOptions.ActiveBrandIdentityVersion = tokenOptions.ActiveBrandIdentityVersion;
        hubOptions.TokenPrefix = tokenOptions.Prefix;
        NerdBrandTypographySwitcher.TrySwitchBrand(brandId, typographyOptions, hubOptions, theme);
        typographyCss.Update(typographyOptions.Typography);
    }
}
