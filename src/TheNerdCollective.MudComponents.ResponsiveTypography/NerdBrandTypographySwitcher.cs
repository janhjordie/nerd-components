using MudBlazor;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.ResponsiveTypography;

/// <summary>
/// Applies brand typography when switching design-token brand packs in catalogs.
/// </summary>
public static class NerdBrandTypographySwitcher
{
    public static bool TrySwitchBrand(
        string brandId,
        NerdResponsiveTypographyOptions typographyOptions,
        NerdDesignSystemOptions hubOptions,
        MudTheme? theme = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(brandId);
        ArgumentNullException.ThrowIfNull(typographyOptions);
        ArgumentNullException.ThrowIfNull(hubOptions);

        if (!NerdBrandTypographyRegistry.Instance.TryGet(brandId, out _))
        {
            return false;
        }

        NerdBrandTypographyRegistry.Instance.Configure(brandId, typographyOptions);
        hubOptions.ActiveTypographyPackId = brandId;
        hubOptions.TypographyRoleCount = typographyOptions.Typography.ConfiguredRoles.Count;
        SyncTheme(theme, typographyOptions);
        return true;
    }

    public static void SwitchBrand(
        string brandId,
        NerdResponsiveTypographyOptions typographyOptions,
        NerdDesignSystemOptions hubOptions,
        MudTheme? theme = null)
    {
        if (!TrySwitchBrand(brandId, typographyOptions, hubOptions, theme))
        {
            throw new ArgumentException($"Unknown brand typography pack '{brandId}'.", nameof(brandId));
        }
    }

    private static void SyncTheme(
        MudTheme? theme,
        NerdResponsiveTypographyOptions typographyOptions,
        NerdResponsiveTypographyCss? typographyCss = null)
    {
        if (theme is null)
        {
            typographyCss?.Update(typographyOptions.Typography);
            return;
        }

        theme.UseResponsiveTypography(typographyOptions.Typography);
        typographyCss?.Update(typographyOptions.Typography);
    }
}
