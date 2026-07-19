using MudBlazor;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>
/// Builds a <see cref="MudTheme"/> from the active brand pack so <see cref="MudThemeProvider"/> owns global :root palette (HR-157).
/// </summary>
public static class NerdMudThemeFactory
{
    public static MudTheme Create(NerdDesignTokenOptions options) =>
        Create(options, configure: null);

    public static MudTheme Create(NerdDesignTokenOptions options, Action<MudTheme>? configure)
    {
        ArgumentNullException.ThrowIfNull(options);
        var lightMap = NerdMudBrandPaletteMap.Resolve(options, NerdMudPaletteMode.Light);
        var darkMap = NerdMudBrandPaletteMap.Resolve(options, NerdMudPaletteMode.Dark);

        var theme = new MudTheme
        {
            PaletteLight = NerdMudThemePaletteConverter.ToPaletteLight(lightMap),
            PaletteDark = NerdMudThemePaletteConverter.ToPaletteDark(darkMap),
            LayoutProperties = ResolveLayoutProperties(options),
            Shadows = ResolveShadows(options)
        };

        configure?.Invoke(theme);
        return theme;
    }

    private static LayoutProperties ResolveLayoutProperties(NerdDesignTokenOptions options)
    {
        var layout = new LayoutProperties();
        if (TryResolveRadius(options, out var borderRadius))
        {
            layout.DefaultBorderRadius = borderRadius;
        }

        if (options.Spacing.TryGetValue("drawer-width", out var drawerWidth))
        {
            layout.DrawerWidthLeft = drawerWidth;
            layout.DrawerWidthRight = drawerWidth;
        }

        return layout;
    }

    private static Shadow ResolveShadows(NerdDesignTokenOptions options)
    {
        if (options.Shadows.Count == 0)
        {
            return new Shadow();
        }

        var shadow = new Shadow();
        var elevations = shadow.Elevation.ToArray();
        MapShadow(options, "0", elevations, 0);
        MapShadow(options, "1", elevations, 1);
        MapShadow(options, "sm", elevations, 1);
        MapShadow(options, "2", elevations, 2);
        MapShadow(options, "md", elevations, 2);
        MapShadow(options, "3", elevations, 3);
        MapShadow(options, "lg", elevations, 3);
        MapShadow(options, "4", elevations, 4);
        MapShadow(options, "xl", elevations, 4);
        shadow.Elevation = elevations;
        return shadow;
    }

    private static void MapShadow(NerdDesignTokenOptions options, string key, string[] elevations, int index)
    {
        if (index < 0 || index >= elevations.Length || !options.Shadows.TryGetValue(key, out var value))
        {
            return;
        }

        elevations[index] = value;
    }

    private static bool TryResolveRadius(NerdDesignTokenOptions options, out string borderRadius)
    {
        foreach (var key in new[] { "default", "md", "base", "sm" })
        {
            if (options.Radii.TryGetValue(key, out borderRadius!))
            {
                return true;
            }
        }

        borderRadius = options.Radii.Values.FirstOrDefault() ?? string.Empty;
        return !string.IsNullOrWhiteSpace(borderRadius);
    }
}
