namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>
/// Builds the precise token → MudBlazor <c>MudTheme</c> property map used by <c>/nerd-theme</c>.
/// </summary>
public static class NerdMudThemeMappingTools
{
    public const string CategoryPalette = "Palette";
    public const string CategoryLayout = "Layout";
    public const string CategoryShadows = "Shadows";
    public const string CategoryZIndex = "ZIndex";

    public static IReadOnlyList<NerdMudThemeMappingEntry> BuildAll(NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return
        [
            ..BuildPaletteMappings(options),
            ..BuildLayoutMappings(options),
            ..BuildShadowMappings(options),
            ..BuildZIndexMappings(options)
        ];
    }

    public static IReadOnlyList<NerdMudThemeMappingEntry> BuildPaletteMappings(NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var bindings = options.FrameworkDefaults?.MudBlazor?.Palette
            ?? NerdMudBrandPaletteMap.CreateConventionBindings();
        var light = NerdMudBrandPaletteMap.Resolve(options, NerdMudPaletteMode.Light);
        var dark = NerdMudBrandPaletteMap.Resolve(options, NerdMudPaletteMode.Dark);

        return
        [
            Palette("Primary", "Primary", "--mud-palette-primary", bindings.Primary, light.Primary, dark.Primary, "Color", "Fill channel"),
            Palette("PrimaryContrastText", "PrimaryText", "--mud-palette-primary-text", bindings.Primary, light.PrimaryText, dark.PrimaryText, "Text", "Contrast text for Primary"),
            Palette("Secondary", "Secondary", "--mud-palette-secondary", bindings.Secondary, light.Secondary, dark.Secondary, "Color", "Fill channel"),
            Palette("SecondaryContrastText", "SecondaryText", "--mud-palette-secondary-text", bindings.Secondary, light.SecondaryText, dark.SecondaryText, "Text", "Contrast text for Secondary"),
            Palette("Tertiary", "Tertiary", "--mud-palette-tertiary", bindings.Tertiary, light.Tertiary, dark.Tertiary, "Color", "Falls back to Primary binding"),
            Palette("TertiaryContrastText", "TertiaryText", "--mud-palette-tertiary-text", bindings.Tertiary, light.TertiaryText, dark.TertiaryText, "Text", "Contrast text for Tertiary"),
            Palette("Info", "Info", "--mud-palette-info", bindings.Info, light.Info, dark.Info, "Color", "Falls back to Secondary binding"),
            Palette("InfoContrastText", "InfoText", "--mud-palette-info-text", bindings.Info, light.InfoText, dark.InfoText, "Text", "Contrast text for Info"),
            Palette("Success", "Success", "--mud-palette-success", bindings.Success, light.Success, dark.Success, "Color", "Fill channel"),
            Palette("SuccessContrastText", "SuccessText", "--mud-palette-success-text", bindings.Success, light.SuccessText, dark.SuccessText, "Text", "Contrast text for Success"),
            Palette("Warning", "Warning", "--mud-palette-warning", bindings.Warning, light.Warning, dark.Warning, "Color", "Fill channel"),
            Palette("WarningContrastText", "WarningText", "--mud-palette-warning-text", bindings.Warning, light.WarningText, dark.WarningText, "Text", "Contrast text for Warning"),
            Palette("Error", "Error", "--mud-palette-error", bindings.Error, light.Error, dark.Error, "Color", "Fill channel"),
            Palette("ErrorContrastText", "ErrorText", "--mud-palette-error-text", bindings.Error, light.ErrorText, dark.ErrorText, "Text", "Contrast text for Error"),
            Palette("Dark", "Dark", "--mud-palette-dark", bindings.Dark, light.Dark, dark.Dark, "Color", "Fill channel"),
            Palette("DarkContrastText", "DarkText", "--mud-palette-dark-text", bindings.Dark, light.DarkText, dark.DarkText, "Text", "Contrast text for Dark"),
            Palette("TextPrimary", "TextPrimary", "--mud-palette-text-primary", bindings.TextPrimary, light.TextPrimary, dark.TextPrimary, "Content", "Body text on page surface"),
            Palette("TextSecondary", "TextSecondary", "--mud-palette-text-secondary", bindings.TextSecondary, light.TextSecondary, dark.TextSecondary, "Content", "Muted body text"),
            Palette("TextDisabled", "TextDisabled", "--mud-palette-text-disabled", bindings.TextDisabled, light.TextDisabled, dark.TextDisabled, "Disabled", "Disabled label color"),
            Palette("ActionDefault", "ActionDefault", "--mud-palette-action-default", bindings.ActionDefault, light.ActionDefault, dark.ActionDefault, "Color", "Default action chrome"),
            Palette("ActionDisabled", "ActionDisabled", "--mud-palette-action-disabled", bindings.TextDisabled, light.ActionDisabled, dark.ActionDisabled, "Disabled", "Uses TextDisabled binding"),
            Palette("ActionDisabledBackground", "ActionDisabledBackground", "--mud-palette-action-disabled-background", bindings.ActionDisabled, light.ActionDisabledBackground, dark.ActionDisabledBackground, "DisabledBackground", "Fixed translucent overlay"),
            Palette("Surface", "Surface", "--mud-palette-surface", bindings.Surface, light.Surface, dark.Surface, "Surface", "Card / paper surface"),
            Palette("Background", "Background", "--mud-palette-background", bindings.Background, light.Background, dark.Background, "Surface", "Page background"),
            Palette("BackgroundGray", "BackgroundGray", "--mud-palette-background-gray", bindings.Background, light.BackgroundGray, dark.BackgroundGray, "Derived", "Derived from Background binding"),
            Palette("DrawerBackground", "DrawerBackground", "--mud-palette-drawer-background", bindings.DrawerBackground, light.DrawerBackground, dark.DrawerBackground, "Surface", "Nav drawer surface"),
            Palette("DrawerText", "DrawerText", "--mud-palette-drawer-text", bindings.DrawerText, light.DrawerText, dark.DrawerText, "Color", "Nav item text"),
            Palette("DrawerIcon", "DrawerIcon", "--mud-palette-drawer-icon", bindings.DrawerIcon, light.DrawerIcon, dark.DrawerIcon, "Color", "Nav item icon"),
            Palette("AppbarBackground", "AppbarBackground", "--mud-palette-appbar-background", bindings.AppbarBackground, light.AppbarBackground, dark.AppbarBackground, "Color", "Falls back to Secondary binding"),
            Palette("AppbarText", "AppbarText", "--mud-palette-appbar-text", bindings.AppbarText, light.AppbarText, dark.AppbarText, "Color", "App bar foreground"),
            Palette("LinesDefault", "LinesDefault", "--mud-palette-lines-default", bindings.LinesDefault, light.LinesDefault, dark.LinesDefault, "Border", "Default borders / dividers"),
            Palette("LinesInputs", "LinesInputs", "--mud-palette-lines-inputs", bindings.LinesInputs, light.LinesInputs, dark.LinesInputs, "Border", "Input underline / outline"),
            Palette("Divider", "Divider", "--mud-palette-divider", bindings.LinesDefault, light.Divider, dark.Divider, "Border", "Uses LinesDefault binding"),
            Palette("DividerLight", "DividerLight", "--mud-palette-divider-light", bindings.LinesDefault, light.DividerLight, dark.DividerLight, "Derived", "50% mix of LinesDefault"),
            Palette("TableLines", "TableLines", "--mud-palette-table-lines", bindings.LinesDefault, light.TableLines, dark.TableLines, "Border", "Uses LinesDefault binding"),
            Palette("TableStriped", "TableStriped", "--mud-palette-table-striped", bindings.Surface, light.TableStriped, dark.TableStriped, "Derived", "2% content mix on Surface"),
            Palette("TableHover", "TableHover", "--mud-palette-table-hover", bindings.ActionDefault, light.TableHover, dark.TableHover, "Hover", "Uses ActionDefault hover"),
            Palette("Skeleton", "Skeleton", "--mud-palette-skeleton", bindings.Surface, light.Skeleton, dark.Skeleton, "Derived", "11% content mix on Surface"),
            Palette("GrayDefault", "GrayDefault", "--mud-palette-gray-default", bindings.TextSecondary, light.GrayDefault, dark.GrayDefault, "Content", "Uses TextSecondary binding"),
            Palette("GrayLight", "GrayLight", "--mud-palette-gray-light", bindings.Surface, light.GrayLight, dark.GrayLight, "Derived", "Surface/content mix"),
            Palette("GrayLighter", "GrayLighter", "--mud-palette-gray-lighter", bindings.Surface, light.GrayLighter, dark.GrayLighter, "Derived", "Surface/content mix"),
            Palette("GrayDark", "GrayDark", "--mud-palette-gray-dark", bindings.TextPrimary, light.GrayDark, dark.GrayDark, "Derived", "TextPrimary/surface mix"),
            Palette("GrayDarker", "GrayDarker", "--mud-palette-gray-darker", bindings.TextPrimary, light.GrayDarker, dark.GrayDarker, "Derived", "TextPrimary/surface mix"),
            Palette("OverlayLight", "OverlayLight", "--mud-palette-overlay-light", bindings.Surface, light.OverlayLight, dark.OverlayLight, "Derived", "Surface @ 30% alpha"),
            Palette("OverlayDark", "OverlayDark", "--mud-palette-overlay-dark", bindings.Dark, light.OverlayDark, dark.OverlayDark, "Derived", "Dark @ 50% alpha"),
        ];

        NerdMudThemeMappingEntry Palette(
            string mudProperty,
            string brandMapProperty,
            string cssVariable,
            string? bindingAlias,
            string lightValue,
            string darkValue,
            string valueKind,
            string notes) =>
            new(
                CategoryPalette,
                $"Palette.{mudProperty}",
                cssVariable,
                bindingAlias,
                ResolveColorTokenName(options, bindingAlias),
                lightValue,
                darkValue,
                valueKind,
                string.IsNullOrWhiteSpace(brandMapProperty)
                    ? notes
                    : $"{notes} · BrandMap.{brandMapProperty}");
    }

    public static IReadOnlyList<NerdMudThemeMappingEntry> BuildLayoutMappings(NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var entries = new List<NerdMudThemeMappingEntry>();

        if (TryResolveRadius(options, out var radiusKey, out var radiusValue))
        {
            entries.Add(new NerdMudThemeMappingEntry(
                CategoryLayout,
                "LayoutProperties.DefaultBorderRadius",
                "--mud-default-borderradius",
                null,
                radiusKey,
                radiusValue,
                radiusValue,
                "Radius",
                $"From radii[\"{radiusKey}\"]"));
        }

        if (options.Spacing.TryGetValue("drawer-width", out var drawerWidth))
        {
            entries.Add(new NerdMudThemeMappingEntry(
                CategoryLayout,
                "LayoutProperties.DrawerWidthLeft",
                string.Empty,
                null,
                "drawer-width",
                drawerWidth,
                drawerWidth,
                "Spacing",
                "Also applied to DrawerWidthRight"));
            entries.Add(new NerdMudThemeMappingEntry(
                CategoryLayout,
                "LayoutProperties.DrawerWidthRight",
                string.Empty,
                null,
                "drawer-width",
                drawerWidth,
                drawerWidth,
                "Spacing",
                "Mirrors DrawerWidthLeft"));
        }

        return entries;
    }

    public static IReadOnlyList<NerdMudThemeMappingEntry> BuildShadowMappings(NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var entries = new List<NerdMudThemeMappingEntry>();
        MapShadow(options, entries, "0", 0);
        MapShadow(options, entries, "1", 1);
        MapShadow(options, entries, "sm", 1);
        MapShadow(options, entries, "2", 2);
        MapShadow(options, entries, "md", 2);
        MapShadow(options, entries, "3", 3);
        MapShadow(options, entries, "lg", 3);
        MapShadow(options, entries, "4", 4);
        MapShadow(options, entries, "xl", 4);
        return entries;
    }

    public static IReadOnlyList<NerdMudThemeMappingEntry> BuildZIndexMappings(NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var entries = new List<NerdMudThemeMappingEntry>();
        MapZ(options, entries, "ZIndex.Drawer", ["drawer", "sticky"]);
        MapZ(options, entries, "ZIndex.Popover", ["popover", "dropdown"]);
        MapZ(options, entries, "ZIndex.AppBar", ["appbar", "sticky"]);
        MapZ(options, entries, "ZIndex.Dialog", ["dialog", "modal"]);
        MapZ(options, entries, "ZIndex.Snackbar", ["snackbar"]);
        MapZ(options, entries, "ZIndex.Tooltip", ["tooltip"]);
        return entries;
    }

    /// <summary>Walks alias chains to the concrete color token name, or <c>null</c> when unbound.</summary>
    public static string? ResolveColorTokenName(NerdDesignTokenOptions options, string? alias)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (string.IsNullOrWhiteSpace(alias) || !options.Aliases.TryGetValue(alias, out var target))
        {
            return null;
        }

        var current = target;
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        while (options.Aliases.TryGetValue(current, out var next))
        {
            if (!visited.Add(current))
            {
                break;
            }

            current = next;
        }

        return options.Colors.ContainsKey(current) ? current : current;
    }

    /// <summary>
    /// Returns the active Mud palette bindings (pack <c>frameworkDefaults.mudblazor.palette</c> or conventions).
    /// </summary>
    public static NerdMudBlazorPaletteBindings ResolveBindings(NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return options.FrameworkDefaults?.MudBlazor?.Palette
            ?? NerdMudBrandPaletteMap.CreateConventionBindings();
    }

    /// <summary>Binding alias fields → Mud palette channel names for the bindings table.</summary>
    public static IReadOnlyList<(string MudSlot, string? Alias)> EnumerateBindingSlots(
        NerdMudBlazorPaletteBindings bindings)
    {
        ArgumentNullException.ThrowIfNull(bindings);
        return
        [
            ("Primary", bindings.Primary),
            ("Secondary", bindings.Secondary),
            ("Tertiary", bindings.Tertiary),
            ("Info", bindings.Info),
            ("Success", bindings.Success),
            ("Warning", bindings.Warning),
            ("Error", bindings.Error),
            ("Dark", bindings.Dark),
            ("Surface", bindings.Surface),
            ("Background", bindings.Background),
            ("TextPrimary", bindings.TextPrimary),
            ("TextSecondary", bindings.TextSecondary),
            ("TextDisabled", bindings.TextDisabled),
            ("ActionDefault", bindings.ActionDefault),
            ("ActionDisabled", bindings.ActionDisabled),
            ("AppbarBackground", bindings.AppbarBackground),
            ("AppbarText", bindings.AppbarText),
            ("DrawerBackground", bindings.DrawerBackground),
            ("DrawerText", bindings.DrawerText),
            ("DrawerIcon", bindings.DrawerIcon),
            ("LinesDefault", bindings.LinesDefault),
            ("LinesInputs", bindings.LinesInputs),
        ];
    }

    private static void MapShadow(
        NerdDesignTokenOptions options,
        List<NerdMudThemeMappingEntry> entries,
        string key,
        int elevationIndex)
    {
        if (!options.Shadows.TryGetValue(key, out var value))
        {
            return;
        }

        entries.Add(new NerdMudThemeMappingEntry(
            CategoryShadows,
            $"Shadows.Elevation[{elevationIndex}]",
            $"--mud-elevation-{elevationIndex}",
            null,
            key,
            value,
            value,
            "Shadow",
            $"From shadows[\"{key}\"]"));
    }

    private static void MapZ(
        NerdDesignTokenOptions options,
        List<NerdMudThemeMappingEntry> entries,
        string mudProperty,
        string[] keys)
    {
        foreach (var key in keys)
        {
            if (!options.ZIndex.TryGetValue(key, out var raw))
            {
                continue;
            }

            entries.Add(new NerdMudThemeMappingEntry(
                CategoryZIndex,
                mudProperty,
                string.Empty,
                null,
                key,
                raw,
                raw,
                "ZIndex",
                $"From zIndex[\"{key}\"]"));
            return;
        }
    }

    private static bool TryResolveRadius(
        NerdDesignTokenOptions options,
        out string key,
        out string value)
    {
        foreach (var candidate in new[] { "default", "md", "base", "sm" })
        {
            if (options.Radii.TryGetValue(candidate, out value!))
            {
                key = candidate;
                return true;
            }
        }

        var first = options.Radii.FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(first.Key))
        {
            key = first.Key;
            value = first.Value;
            return true;
        }

        key = string.Empty;
        value = string.Empty;
        return false;
    }
}
