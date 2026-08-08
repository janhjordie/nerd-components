using System.Globalization;
using System.Reflection;
using MudBlazor;
using MudBlazor.Utilities;

namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>
/// Builds the complete MudBlazor <c>MudTheme</c> inventory for <c>/nerd-theme</c>,
/// including mapped, derived, hardcoded, and <strong>unmapped</strong> properties so gaps are visible.
/// </summary>
public static class NerdMudThemeMappingTools
{
    public const string CategoryPalette = "Palette";
    public const string CategoryLayout = "Layout";
    public const string CategoryShadows = "Shadows";
    public const string CategoryZIndex = "ZIndex";
    public const string CategoryTypography = "Typography";
    public const string CategoryPseudoCss = "PseudoCss";

    private static readonly string[] PaletteChannels =
    [
        "Primary", "Secondary", "Tertiary", "Info", "Success", "Warning", "Error", "Dark"
    ];

    private static readonly string[] TypographyRoles =
    [
        "Default", "H1", "H2", "H3", "H4", "H5", "H6",
        "Subtitle1", "Subtitle2", "Body1", "Body2", "Button", "Caption", "Overline"
    ];

    private static readonly string[] TypographyFields =
    [
        "FontFamily", "FontSize", "FontWeight", "LineHeight", "LetterSpacing", "TextTransform"
    ];

    public static IReadOnlyList<NerdMudThemeMappingEntry> BuildAll(NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var theme = NerdMudThemeFactory.Create(options);
        var known = BuildKnownMappings(options)
            .ToDictionary(entry => entry.MudThemeProperty, StringComparer.Ordinal);
        var result = new List<NerdMudThemeMappingEntry>();

        foreach (var slot in EnumerateMudThemeInventory())
        {
            if (known.TryGetValue(slot.Property, out var mapped))
            {
                result.Add(mapped);
                continue;
            }

            if (TryBuildChannelDerivative(slot, known, theme, out var derived))
            {
                result.Add(derived);
                continue;
            }

            result.Add(BuildUnmapped(slot, theme));
        }

        return result;
    }

    public static NerdMudThemeMappingCoverage EvaluateCoverage(IEnumerable<NerdMudThemeMappingEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        var list = entries as IReadOnlyList<NerdMudThemeMappingEntry> ?? entries.ToList();
        return new NerdMudThemeMappingCoverage(
            list.Count,
            list.Count(entry => entry.Status == NerdMudThemeMappingStatus.Mapped),
            list.Count(entry => entry.Status == NerdMudThemeMappingStatus.Hardcoded),
            list.Count(entry => entry.Status == NerdMudThemeMappingStatus.Derived),
            list.Count(entry => entry.Status == NerdMudThemeMappingStatus.Unmapped));
    }

    public static IReadOnlyList<NerdMudThemeMappingEntry> BuildPaletteMappings(NerdDesignTokenOptions options) =>
        BuildAll(options).Where(entry => entry.Category == CategoryPalette).ToList();

    public static IReadOnlyList<NerdMudThemeMappingEntry> BuildLayoutMappings(NerdDesignTokenOptions options) =>
        BuildAll(options).Where(entry => entry.Category == CategoryLayout).ToList();

    public static IReadOnlyList<NerdMudThemeMappingEntry> BuildShadowMappings(NerdDesignTokenOptions options) =>
        BuildAll(options).Where(entry => entry.Category == CategoryShadows).ToList();

    public static IReadOnlyList<NerdMudThemeMappingEntry> BuildZIndexMappings(NerdDesignTokenOptions options) =>
        BuildAll(options).Where(entry => entry.Category == CategoryZIndex).ToList();

    /// <summary>Complete MudBlazor 9.8 <see cref="MudTheme"/> leaf-property inventory.</summary>
    public static IReadOnlyList<NerdMudThemeInventorySlot> EnumerateMudThemeInventory()
    {
        var slots = new List<NerdMudThemeInventorySlot>();

        foreach (var property in typeof(Palette).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                     .OrderBy(property => property.Name, StringComparer.Ordinal))
        {
            slots.Add(new NerdMudThemeInventorySlot(
                CategoryPalette,
                $"Palette.{property.Name}",
                ResolvePaletteCssVariable(property.Name),
                property.Name,
                null,
                null));
        }

        foreach (var property in typeof(LayoutProperties).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                     .OrderBy(property => property.Name, StringComparer.Ordinal))
        {
            slots.Add(new NerdMudThemeInventorySlot(
                CategoryLayout,
                $"LayoutProperties.{property.Name}",
                property.Name == "DefaultBorderRadius" ? "--mud-default-borderradius" : string.Empty,
                null,
                property.Name,
                null));
        }

        for (var index = 0; index < 26; index++)
        {
            slots.Add(new NerdMudThemeInventorySlot(
                CategoryShadows,
                $"Shadows.Elevation[{index}]",
                $"--mud-elevation-{index}",
                null,
                null,
                index));
        }

        foreach (var property in typeof(ZIndex).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                     .OrderBy(property => property.Name, StringComparer.Ordinal))
        {
            slots.Add(new NerdMudThemeInventorySlot(
                CategoryZIndex,
                $"ZIndex.{property.Name}",
                string.Empty,
                null,
                property.Name,
                null));
        }

        foreach (var role in TypographyRoles)
        {
            foreach (var field in TypographyFields)
            {
                slots.Add(new NerdMudThemeInventorySlot(
                    CategoryTypography,
                    $"Typography.{role}.{field}",
                    ResolveTypographyCssVariable(role, field),
                    null,
                    $"{role}.{field}",
                    null));
            }
        }

        slots.Add(new NerdMudThemeInventorySlot(
            CategoryPseudoCss,
            "PseudoCss.Scope",
            string.Empty,
            null,
            "Scope",
            null));

        return slots;
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

    private static IEnumerable<NerdMudThemeMappingEntry> BuildKnownMappings(NerdDesignTokenOptions options)
    {
        foreach (var entry in BuildKnownPaletteMappings(options))
        {
            yield return entry;
        }

        foreach (var entry in BuildKnownLayoutMappings(options))
        {
            yield return entry;
        }

        foreach (var entry in BuildKnownShadowMappings(options))
        {
            yield return entry;
        }

        foreach (var entry in BuildKnownZIndexMappings(options))
        {
            yield return entry;
        }
    }

    private static IEnumerable<NerdMudThemeMappingEntry> BuildKnownPaletteMappings(NerdDesignTokenOptions options)
    {
        var bindings = ResolveBindings(options);
        var light = NerdMudBrandPaletteMap.Resolve(options, NerdMudPaletteMode.Light);
        var dark = NerdMudBrandPaletteMap.Resolve(options, NerdMudPaletteMode.Dark);

        yield return Palette("Primary", "Primary", "--mud-palette-primary", bindings.Primary, light.Primary, dark.Primary, "Color", "Fill channel", NerdMudThemeMappingStatus.Mapped);
        yield return Palette("PrimaryContrastText", "PrimaryText", "--mud-palette-primary-text", bindings.Primary, light.PrimaryText, dark.PrimaryText, "Text", "Contrast text for Primary", NerdMudThemeMappingStatus.Mapped);
        yield return Palette("Secondary", "Secondary", "--mud-palette-secondary", bindings.Secondary, light.Secondary, dark.Secondary, "Color", "Fill channel", NerdMudThemeMappingStatus.Mapped);
        yield return Palette("SecondaryContrastText", "SecondaryText", "--mud-palette-secondary-text", bindings.Secondary, light.SecondaryText, dark.SecondaryText, "Text", "Contrast text for Secondary", NerdMudThemeMappingStatus.Mapped);
        yield return Palette("Tertiary", "Tertiary", "--mud-palette-tertiary", bindings.Tertiary, light.Tertiary, dark.Tertiary, "Color", "Falls back to Primary binding", NerdMudThemeMappingStatus.Mapped);
        yield return Palette("TertiaryContrastText", "TertiaryText", "--mud-palette-tertiary-text", bindings.Tertiary, light.TertiaryText, dark.TertiaryText, "Text", "Contrast text for Tertiary", NerdMudThemeMappingStatus.Mapped);
        yield return Palette("Info", "Info", "--mud-palette-info", bindings.Info, light.Info, dark.Info, "Color", "Falls back to Secondary binding", NerdMudThemeMappingStatus.Mapped);
        yield return Palette("InfoContrastText", "InfoText", "--mud-palette-info-text", bindings.Info, light.InfoText, dark.InfoText, "Text", "Contrast text for Info", NerdMudThemeMappingStatus.Mapped);
        yield return Palette("Success", "Success", "--mud-palette-success", bindings.Success, light.Success, dark.Success, "Color", "Fill channel", NerdMudThemeMappingStatus.Mapped);
        yield return Palette("SuccessContrastText", "SuccessText", "--mud-palette-success-text", bindings.Success, light.SuccessText, dark.SuccessText, "Text", "Contrast text for Success", NerdMudThemeMappingStatus.Mapped);
        yield return Palette("Warning", "Warning", "--mud-palette-warning", bindings.Warning, light.Warning, dark.Warning, "Color", "Fill channel", NerdMudThemeMappingStatus.Mapped);
        yield return Palette("WarningContrastText", "WarningText", "--mud-palette-warning-text", bindings.Warning, light.WarningText, dark.WarningText, "Text", "Contrast text for Warning", NerdMudThemeMappingStatus.Mapped);
        yield return Palette("Error", "Error", "--mud-palette-error", bindings.Error, light.Error, dark.Error, "Color", "Fill channel", NerdMudThemeMappingStatus.Mapped);
        yield return Palette("ErrorContrastText", "ErrorText", "--mud-palette-error-text", bindings.Error, light.ErrorText, dark.ErrorText, "Text", "Contrast text for Error", NerdMudThemeMappingStatus.Mapped);
        yield return Palette("Dark", "Dark", "--mud-palette-dark", bindings.Dark, light.Dark, dark.Dark, "Color", "Fill channel", NerdMudThemeMappingStatus.Mapped);
        yield return Palette("DarkContrastText", "DarkText", "--mud-palette-dark-text", bindings.Dark, light.DarkText, dark.DarkText, "Text", "Contrast text for Dark", NerdMudThemeMappingStatus.Mapped);
        yield return Palette("TextPrimary", "TextPrimary", "--mud-palette-text-primary", bindings.TextPrimary, light.TextPrimary, dark.TextPrimary, "Content", "Body text on page surface", NerdMudThemeMappingStatus.Mapped);
        yield return Palette("TextSecondary", "TextSecondary", "--mud-palette-text-secondary", bindings.TextSecondary, light.TextSecondary, dark.TextSecondary, "Content", "Muted body text", NerdMudThemeMappingStatus.Mapped);
        yield return Palette("TextDisabled", "TextDisabled", "--mud-palette-text-disabled", bindings.TextDisabled, light.TextDisabled, dark.TextDisabled, "Disabled", "Disabled label color", NerdMudThemeMappingStatus.Mapped);
        yield return Palette("ActionDefault", "ActionDefault", "--mud-palette-action-default", bindings.ActionDefault, light.ActionDefault, dark.ActionDefault, "Color", "Default action chrome", NerdMudThemeMappingStatus.Mapped);
        yield return Palette("ActionDisabled", "ActionDisabled", "--mud-palette-action-disabled", bindings.TextDisabled, light.ActionDisabled, dark.ActionDisabled, "Disabled", "Uses TextDisabled binding", NerdMudThemeMappingStatus.Mapped);
        yield return Palette("ActionDisabledBackground", "ActionDisabledBackground", "--mud-palette-action-disabled-background", bindings.ActionDisabled, light.ActionDisabledBackground, dark.ActionDisabledBackground, "DisabledBackground", "Fixed translucent overlay", NerdMudThemeMappingStatus.Mapped);
        yield return Palette("Surface", "Surface", "--mud-palette-surface", bindings.Surface, light.Surface, dark.Surface, "Surface", "Card / paper surface", NerdMudThemeMappingStatus.Mapped);
        yield return Palette("Background", "Background", "--mud-palette-background", bindings.Background, light.Background, dark.Background, "Surface", "Page background", NerdMudThemeMappingStatus.Mapped);
        yield return Palette("BackgroundGray", "BackgroundGray", "--mud-palette-background-gray", bindings.Background, light.BackgroundGray, dark.BackgroundGray, "Derived", "Derived from Background binding", NerdMudThemeMappingStatus.Mapped);
        yield return Palette("DrawerBackground", "DrawerBackground", "--mud-palette-drawer-background", bindings.DrawerBackground, light.DrawerBackground, dark.DrawerBackground, "Surface", "Nav drawer surface", NerdMudThemeMappingStatus.Mapped);
        yield return Palette("DrawerText", "DrawerText", "--mud-palette-drawer-text", bindings.DrawerText, light.DrawerText, dark.DrawerText, "Color", "Nav item text", NerdMudThemeMappingStatus.Mapped);
        yield return Palette("DrawerIcon", "DrawerIcon", "--mud-palette-drawer-icon", bindings.DrawerIcon, light.DrawerIcon, dark.DrawerIcon, "Color", "Nav item icon", NerdMudThemeMappingStatus.Mapped);
        yield return Palette("AppbarBackground", "AppbarBackground", "--mud-palette-appbar-background", bindings.AppbarBackground, light.AppbarBackground, dark.AppbarBackground, "Color", "Falls back to Secondary binding", NerdMudThemeMappingStatus.Mapped);
        yield return Palette("AppbarText", "AppbarText", "--mud-palette-appbar-text", bindings.AppbarText, light.AppbarText, dark.AppbarText, "Color", "App bar foreground", NerdMudThemeMappingStatus.Mapped);
        yield return Palette("LinesDefault", "LinesDefault", "--mud-palette-lines-default", bindings.LinesDefault, light.LinesDefault, dark.LinesDefault, "Border", "Default borders / dividers", NerdMudThemeMappingStatus.Mapped);
        yield return Palette("LinesInputs", "LinesInputs", "--mud-palette-lines-inputs", bindings.LinesInputs, light.LinesInputs, dark.LinesInputs, "Border", "Input underline / outline", NerdMudThemeMappingStatus.Mapped);
        yield return Palette("Divider", "Divider", "--mud-palette-divider", bindings.LinesDefault, light.Divider, dark.Divider, "Border", "Uses LinesDefault binding", NerdMudThemeMappingStatus.Mapped);
        yield return Palette("DividerLight", "DividerLight", "--mud-palette-divider-light", bindings.LinesDefault, light.DividerLight, dark.DividerLight, "Derived", "50% mix of LinesDefault", NerdMudThemeMappingStatus.Mapped);
        yield return Palette("TableLines", "TableLines", "--mud-palette-table-lines", bindings.LinesDefault, light.TableLines, dark.TableLines, "Border", "Uses LinesDefault binding", NerdMudThemeMappingStatus.Mapped);
        yield return Palette("TableStriped", "TableStriped", "--mud-palette-table-striped", bindings.Surface, light.TableStriped, dark.TableStriped, "Derived", "2% content mix on Surface", NerdMudThemeMappingStatus.Mapped);
        yield return Palette("TableHover", "TableHover", "--mud-palette-table-hover", bindings.ActionDefault, light.TableHover, dark.TableHover, "Hover", "Uses ActionDefault hover", NerdMudThemeMappingStatus.Mapped);
        yield return Palette("Skeleton", "Skeleton", "--mud-palette-skeleton", bindings.Surface, light.Skeleton, dark.Skeleton, "Derived", "11% content mix on Surface", NerdMudThemeMappingStatus.Mapped);
        yield return Palette("GrayDefault", "GrayDefault", "--mud-palette-gray-default", bindings.TextSecondary, light.GrayDefault, dark.GrayDefault, "Content", "Uses TextSecondary binding", NerdMudThemeMappingStatus.Mapped);
        yield return Palette("GrayLight", "GrayLight", "--mud-palette-gray-light", bindings.Surface, light.GrayLight, dark.GrayLight, "Derived", "Surface/content mix", NerdMudThemeMappingStatus.Mapped);
        yield return Palette("GrayLighter", "GrayLighter", "--mud-palette-gray-lighter", bindings.Surface, light.GrayLighter, dark.GrayLighter, "Derived", "Surface/content mix", NerdMudThemeMappingStatus.Mapped);
        yield return Palette("GrayDark", "GrayDark", "--mud-palette-gray-dark", bindings.TextPrimary, light.GrayDark, dark.GrayDark, "Derived", "TextPrimary/surface mix", NerdMudThemeMappingStatus.Mapped);
        yield return Palette("GrayDarker", "GrayDarker", "--mud-palette-gray-darker", bindings.TextPrimary, light.GrayDarker, dark.GrayDarker, "Derived", "TextPrimary/surface mix", NerdMudThemeMappingStatus.Mapped);
        yield return Palette("OverlayLight", "OverlayLight", "--mud-palette-overlay-light", bindings.Surface, light.OverlayLight, dark.OverlayLight, "Derived", "Surface @ 30% alpha", NerdMudThemeMappingStatus.Mapped);
        yield return Palette("OverlayDark", "OverlayDark", "--mud-palette-overlay-dark", bindings.Dark, light.OverlayDark, dark.OverlayDark, "Derived", "Dark @ 50% alpha", NerdMudThemeMappingStatus.Mapped);
        yield return Palette("Black", string.Empty, "--mud-palette-black", null, "#000000", "#000000", "Color", "Hardcoded by NerdMudThemePaletteConverter", NerdMudThemeMappingStatus.Hardcoded);
        yield return Palette("White", string.Empty, "--mud-palette-white", null, "#FFFFFF", "#FFFFFF", "Color", "Hardcoded by NerdMudThemePaletteConverter", NerdMudThemeMappingStatus.Hardcoded);
        yield return Palette("HoverOpacity", string.Empty, string.Empty, null, "0.08", "0.08", "Opacity", "Hardcoded HoverOpacity in NerdMudThemePaletteConverter", NerdMudThemeMappingStatus.Hardcoded);

        NerdMudThemeMappingEntry Palette(
            string mudProperty,
            string brandMapProperty,
            string cssVariable,
            string? bindingAlias,
            string lightValue,
            string darkValue,
            string valueKind,
            string notes,
            NerdMudThemeMappingStatus status) =>
            new(
                CategoryPalette,
                $"Palette.{mudProperty}",
                cssVariable,
                bindingAlias,
                ResolveColorTokenName(options, bindingAlias),
                lightValue,
                darkValue,
                valueKind,
                status,
                string.IsNullOrWhiteSpace(brandMapProperty)
                    ? notes
                    : $"{notes} · BrandMap.{brandMapProperty}");
    }

    private static IEnumerable<NerdMudThemeMappingEntry> BuildKnownLayoutMappings(NerdDesignTokenOptions options)
    {
        if (TryResolveRadius(options, out var radiusKey, out var radiusValue))
        {
            yield return new NerdMudThemeMappingEntry(
                CategoryLayout,
                "LayoutProperties.DefaultBorderRadius",
                "--mud-default-borderradius",
                null,
                radiusKey,
                radiusValue,
                radiusValue,
                "Radius",
                NerdMudThemeMappingStatus.Mapped,
                $"From radii[\"{radiusKey}\"]");
        }

        if (options.Spacing.TryGetValue("drawer-width", out var drawerWidth))
        {
            yield return new NerdMudThemeMappingEntry(
                CategoryLayout,
                "LayoutProperties.DrawerWidthLeft",
                string.Empty,
                null,
                "drawer-width",
                drawerWidth,
                drawerWidth,
                "Spacing",
                NerdMudThemeMappingStatus.Mapped,
                "Also applied to DrawerWidthRight");
            yield return new NerdMudThemeMappingEntry(
                CategoryLayout,
                "LayoutProperties.DrawerWidthRight",
                string.Empty,
                null,
                "drawer-width",
                drawerWidth,
                drawerWidth,
                "Spacing",
                NerdMudThemeMappingStatus.Mapped,
                "Mirrors DrawerWidthLeft");
        }
    }

    private static IEnumerable<NerdMudThemeMappingEntry> BuildKnownShadowMappings(NerdDesignTokenOptions options)
    {
        var byIndex = new Dictionary<int, (string Key, string Value)>();
        TryMapShadow(options, byIndex, "0", 0);
        TryMapShadow(options, byIndex, "1", 1);
        TryMapShadow(options, byIndex, "sm", 1);
        TryMapShadow(options, byIndex, "2", 2);
        TryMapShadow(options, byIndex, "md", 2);
        TryMapShadow(options, byIndex, "3", 3);
        TryMapShadow(options, byIndex, "lg", 3);
        TryMapShadow(options, byIndex, "4", 4);
        TryMapShadow(options, byIndex, "xl", 4);

        foreach (var pair in byIndex.OrderBy(entry => entry.Key))
        {
            yield return new NerdMudThemeMappingEntry(
                CategoryShadows,
                $"Shadows.Elevation[{pair.Key}]",
                $"--mud-elevation-{pair.Key}",
                null,
                pair.Value.Key,
                pair.Value.Value,
                pair.Value.Value,
                "Shadow",
                NerdMudThemeMappingStatus.Mapped,
                $"From shadows[\"{pair.Value.Key}\"]");
        }
    }

    private static IEnumerable<NerdMudThemeMappingEntry> BuildKnownZIndexMappings(NerdDesignTokenOptions options)
    {
        if (TryResolveZ(options, ["drawer", "sticky"], out var drawerKey, out var drawer))
        {
            yield return Z("ZIndex.Drawer", drawerKey, drawer);
        }

        if (TryResolveZ(options, ["popover", "dropdown"], out var popoverKey, out var popover))
        {
            yield return Z("ZIndex.Popover", popoverKey, popover);
        }

        if (TryResolveZ(options, ["appbar", "sticky"], out var appBarKey, out var appBar))
        {
            yield return Z("ZIndex.AppBar", appBarKey, appBar);
        }

        if (TryResolveZ(options, ["dialog", "modal"], out var dialogKey, out var dialog))
        {
            yield return Z("ZIndex.Dialog", dialogKey, dialog);
        }

        if (TryResolveZ(options, ["snackbar"], out var snackbarKey, out var snackbar))
        {
            yield return Z("ZIndex.Snackbar", snackbarKey, snackbar);
        }

        if (TryResolveZ(options, ["tooltip"], out var tooltipKey, out var tooltip))
        {
            yield return Z("ZIndex.Tooltip", tooltipKey, tooltip);
        }

        static NerdMudThemeMappingEntry Z(string mudProperty, string key, string value) =>
            new(
                CategoryZIndex,
                mudProperty,
                string.Empty,
                null,
                key,
                value,
                value,
                "ZIndex",
                NerdMudThemeMappingStatus.Mapped,
                $"From zIndex[\"{key}\"]");
    }

    private static bool TryBuildChannelDerivative(
        NerdMudThemeInventorySlot slot,
        IReadOnlyDictionary<string, NerdMudThemeMappingEntry> known,
        MudTheme theme,
        out NerdMudThemeMappingEntry entry)
    {
        entry = null!;
        if (slot.Category != CategoryPalette || string.IsNullOrWhiteSpace(slot.PaletteProperty))
        {
            return false;
        }

        foreach (var channel in PaletteChannels)
        {
            if (!slot.PaletteProperty.StartsWith(channel, StringComparison.Ordinal))
            {
                continue;
            }

            var suffix = slot.PaletteProperty[channel.Length..];
            if (suffix is not ("Darken" or "Lighten"))
            {
                continue;
            }

            if (!known.ContainsKey($"Palette.{channel}"))
            {
                continue;
            }

            var light = FormatThemeValue(ReadPaletteProperty(theme.PaletteLight, slot.PaletteProperty));
            var dark = FormatThemeValue(ReadPaletteProperty(theme.PaletteDark, slot.PaletteProperty));
            entry = new NerdMudThemeMappingEntry(
                CategoryPalette,
                slot.Property,
                slot.CssVariable,
                known[$"Palette.{channel}"].BindingAlias,
                known[$"Palette.{channel}"].ColorToken,
                light,
                dark,
                "Derived",
                NerdMudThemeMappingStatus.Derived,
                $"MudColor derives {suffix.ToLowerInvariant()} from Palette.{channel}");
            return true;
        }

        return false;
    }

    private static NerdMudThemeMappingEntry BuildUnmapped(NerdMudThemeInventorySlot slot, MudTheme theme)
    {
        var (light, dark) = ReadSlotValues(slot, theme);
        var notes = slot.Category switch
        {
            CategoryTypography => "Unmapped — apply via AddNerdResponsiveTypography / INerdMudThemeConfigurator",
            CategoryLayout => "Unmapped — Mud default (no pack radii/spacing key)",
            CategoryShadows => "Unmapped — Mud default elevation (no pack shadows key)",
            CategoryZIndex => "Unmapped — Mud default (no pack zIndex key)",
            CategoryPseudoCss => "Unmapped — Mud default PseudoCss.Scope (:root)",
            CategoryPalette => "Unmapped — Mud default (not set by NerdMudThemeFactory)",
            _ => "Unmapped"
        };

        return new NerdMudThemeMappingEntry(
            slot.Category,
            slot.Property,
            slot.CssVariable,
            null,
            null,
            light,
            dark,
            "Unmapped",
            NerdMudThemeMappingStatus.Unmapped,
            notes);
    }

    private static (string Light, string Dark) ReadSlotValues(NerdMudThemeInventorySlot slot, MudTheme theme)
    {
        return slot.Category switch
        {
            CategoryPalette when slot.PaletteProperty is not null => (
                FormatThemeValue(ReadPaletteProperty(theme.PaletteLight, slot.PaletteProperty)),
                FormatThemeValue(ReadPaletteProperty(theme.PaletteDark, slot.PaletteProperty))),
            CategoryLayout when slot.MemberPath is not null => (
                FormatThemeValue(ReadProperty(theme.LayoutProperties, slot.MemberPath)),
                FormatThemeValue(ReadProperty(theme.LayoutProperties, slot.MemberPath))),
            CategoryShadows when slot.ElevationIndex is int index => (
                FormatThemeValue(ReadElevation(theme.Shadows, index)),
                FormatThemeValue(ReadElevation(theme.Shadows, index))),
            CategoryZIndex when slot.MemberPath is not null => (
                FormatThemeValue(ReadProperty(theme.ZIndex, slot.MemberPath)),
                FormatThemeValue(ReadProperty(theme.ZIndex, slot.MemberPath))),
            CategoryTypography when slot.MemberPath is not null => (
                FormatThemeValue(ReadTypography(theme.Typography, slot.MemberPath)),
                FormatThemeValue(ReadTypography(theme.Typography, slot.MemberPath))),
            CategoryPseudoCss => (
                FormatThemeValue(theme.PseudoCss.Scope),
                FormatThemeValue(theme.PseudoCss.Scope)),
            _ => ("—", "—")
        };
    }

    private static object? ReadElevation(Shadow shadows, int index)
    {
        if (index < 0 || index >= shadows.Elevation.Length)
        {
            return null;
        }

        return shadows.Elevation[index];
    }

    private static object? ReadTypography(Typography typography, string path)
    {
        var parts = path.Split('.', 2);
        if (parts.Length != 2)
        {
            return null;
        }

        var role = ReadProperty(typography, parts[0]);
        return role is null ? null : ReadProperty(role, parts[1]);
    }

    private static object? ReadPaletteProperty(Palette palette, string name) =>
        ReadProperty(palette, name);

    private static object? ReadProperty(object target, string name)
    {
        var property = target.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
        return property?.GetValue(target);
    }

    private static string FormatThemeValue(object? value)
    {
        switch (value)
        {
            case null:
                return "—";
            case MudColor color:
                return color.Value;
            case string[] array:
                return array.Length == 0 ? "—" : string.Join(", ", array);
            case double number:
                return number.ToString(CultureInfo.InvariantCulture);
            case int integer:
                return integer.ToString(CultureInfo.InvariantCulture);
            default:
                var text = Convert.ToString(value, CultureInfo.InvariantCulture);
                return string.IsNullOrWhiteSpace(text) ? "—" : text;
        }
    }

    private static string ResolvePaletteCssVariable(string propertyName)
    {
        var kebab = ToKebab(propertyName);
        return propertyName switch
        {
            "HoverOpacity" or "BorderOpacity" or "RippleOpacity" or "RippleOpacitySecondary" => string.Empty,
            _ => $"--mud-palette-{kebab}"
        };
    }

    private static string ResolveTypographyCssVariable(string role, string field)
    {
        var roleKebab = ToKebab(role);
        var fieldKey = field switch
        {
            "FontFamily" => "family",
            "FontSize" => "size",
            "FontWeight" => "weight",
            "LineHeight" => "lineheight",
            "LetterSpacing" => "letterspacing",
            "TextTransform" => "text-transform",
            _ => ToKebab(field)
        };
        return $"--mud-typography-{roleKebab}-{fieldKey}";
    }

    private static string ToKebab(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        Span<char> buffer = stackalloc char[value.Length * 2];
        var written = 0;
        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];
            if (char.IsUpper(c) && i > 0)
            {
                buffer[written++] = '-';
            }

            buffer[written++] = char.ToLowerInvariant(c);
        }

        return new string(buffer[..written]);
    }

    private static void TryMapShadow(
        NerdDesignTokenOptions options,
        Dictionary<int, (string Key, string Value)> byIndex,
        string key,
        int elevationIndex)
    {
        if (!options.Shadows.TryGetValue(key, out var value))
        {
            return;
        }

        byIndex[elevationIndex] = (key, value);
    }

    private static bool TryResolveZ(
        NerdDesignTokenOptions options,
        string[] keys,
        out string key,
        out string value)
    {
        foreach (var candidate in keys)
        {
            if (options.ZIndex.TryGetValue(candidate, out value!))
            {
                key = candidate;
                return true;
            }
        }

        key = string.Empty;
        value = string.Empty;
        return false;
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

/// <summary>One leaf property in the MudBlazor <see cref="MudTheme"/> inventory.</summary>
public sealed record NerdMudThemeInventorySlot(
    string Category,
    string Property,
    string CssVariable,
    string? PaletteProperty,
    string? MemberPath,
    int? ElevationIndex);
