using MudBlazor;
using TheNerdCollective.MudComponents.ResponsiveTypography;

namespace TheNerdCollective.MudComponents.PlayBook;

/// <summary>
/// Typography presets available in the PlayBook toolbar.
/// </summary>
public static class NerdPlayBookTypography
{
    public const string DefaultPreset = "default";
    public const string MarketingPreset = "marketing";
    public const string DensePreset = "dense";

    public static IReadOnlyList<(string Id, string Label)> Presets { get; } =
    [
        (DefaultPreset, "Default"),
        (MarketingPreset, "Marketing"),
        (DensePreset, "Dense app")
    ];

    public static MudTheme CreateTheme(string presetId, NerdResponsiveTypographyOptions? configuredOptions = null)
    {
        var typography = new ResponsiveTypographyOptions();
        if (configuredOptions is not null)
        {
            configuredOptions.Typography.CopyTo(typography);
        }

        switch (presetId)
        {
            case MarketingPreset:
                NerdTypographyPresets.ApplyMarketing(typography);
                break;
            case DensePreset:
                NerdTypographyPresets.ApplyDenseApp(typography);
                break;
        }

        var theme = new MudTheme();
        theme.UseResponsiveTypography(typography.CopyTo);
        return theme;
    }
}
