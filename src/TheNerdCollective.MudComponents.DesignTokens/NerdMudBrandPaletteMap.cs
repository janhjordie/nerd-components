using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>
/// Resolves brand semantic aliases to MudBlazor palette slots for CSS + <see cref="MudBlazor.MudTheme"/>.
/// </summary>
public sealed record NerdMudBrandPaletteMap
{
    public string Primary { get; init; } = "#594AE2";

    public string PrimaryText { get; init; } = "#FFFFFF";

    public string PrimaryHover { get; init; } = "rgba(89,74,226,0.08)";

    public string Secondary { get; init; } = "#FF4081";

    public string SecondaryText { get; init; } = "#FFFFFF";

    public string SecondaryHover { get; init; } = "rgba(255,64,129,0.08)";

    public string Tertiary { get; init; } = "#1EC8A5";

    public string TertiaryText { get; init; } = "#FFFFFF";

    public string Info { get; init; } = "#2196F3";

    public string InfoText { get; init; } = "#FFFFFF";

    public string Success { get; init; } = "#00C853";

    public string SuccessText { get; init; } = "#FFFFFF";

    public string Warning { get; init; } = "#FF9800";

    public string WarningText { get; init; } = "#FFFFFF";

    public string Error { get; init; } = "#F44336";

    public string ErrorText { get; init; } = "#FFFFFF";

    public string Dark { get; init; } = "#424242";

    public string DarkText { get; init; } = "#FFFFFF";

    public string TextPrimary { get; init; } = "rgba(0,0,0,0.87)";

    public string TextSecondary { get; init; } = "rgba(0,0,0,0.54)";

    public string TextDisabled { get; init; } = "rgba(0,0,0,0.38)";

    public string ActionDefault { get; init; } = "rgba(0,0,0,0.54)";

    public string ActionDefaultHover { get; init; } = "rgba(0,0,0,0.04)";

    public string ActionDisabled { get; init; } = "rgba(0,0,0,0.26)";

    public string ActionDisabledBackground { get; init; } = "rgba(0,0,0,0.12)";

    public string Surface { get; init; } = "#FFFFFF";

    public string Background { get; init; } = "#FAFAFA";

    public string BackgroundGray { get; init; } = "#F5F5F5";

    public string DrawerBackground { get; init; } = "#FFFFFF";

    public string DrawerText { get; init; } = "rgba(0,0,0,0.87)";

    public string DrawerIcon { get; init; } = "rgba(0,0,0,0.54)";

    public string AppbarBackground { get; init; } = "#594AE2";

    public string AppbarText { get; init; } = "#FFFFFF";

    public string LinesDefault { get; init; } = "rgba(0,0,0,0.12)";

    public string LinesInputs { get; init; } = "rgba(0,0,0,0.42)";

    public string Divider { get; init; } = "rgba(0,0,0,0.12)";

    public string DividerLight { get; init; } = "rgba(0,0,0,0.06)";

    public string TableLines { get; init; } = "rgba(0,0,0,0.12)";

    public string TableStriped { get; init; } = "rgba(0,0,0,0.02)";

    public string TableHover { get; init; } = "rgba(0,0,0,0.04)";

    public string Skeleton { get; init; } = "rgba(0,0,0,0.11)";

    public string GrayDefault { get; init; } = "#9E9E9E";

    public string GrayLight { get; init; } = "#BDBDBD";

    public string GrayLighter { get; init; } = "#E0E0E0";

    public string GrayDark { get; init; } = "#757575";

    public string GrayDarker { get; init; } = "#616161";

    public string OverlayLight { get; init; } = "rgba(255,255,255,0.3)";

    public string OverlayDark { get; init; } = "rgba(33,33,33,0.5)";

    public static NerdMudBrandPaletteMap Resolve(NerdDesignTokenOptions options) =>
        Resolve(options, NerdMudPaletteMode.Light);

    public static NerdMudBrandPaletteMap Resolve(NerdDesignTokenOptions options, NerdMudPaletteMode mode)
    {
        ArgumentNullException.ThrowIfNull(options);
        var bindings = options.FrameworkDefaults?.MudBlazor?.Palette ?? CreateConventionBindings();
        var useDark = mode == NerdMudPaletteMode.Dark;
        return new NerdMudBrandPaletteMap
        {
            Primary = ResolveColor(options, bindings.Primary, "#594AE2", useDark),
            PrimaryText = ResolveText(options, bindings.Primary, "#FFFFFF", useDark),
            PrimaryHover = ResolveHover(options, bindings.Primary, useDark),
            Secondary = ResolveColor(options, bindings.Secondary, "#FF4081", useDark),
            SecondaryText = ResolveText(options, bindings.Secondary, "#FFFFFF", useDark),
            SecondaryHover = ResolveHover(options, bindings.Secondary, useDark),
            Tertiary = ResolveColorOrFallback(options, bindings.Tertiary, bindings.Primary, "#1EC8A5", useDark),
            TertiaryText = ResolveTextOrFallback(options, bindings.Tertiary, bindings.Primary, "#FFFFFF", useDark),
            Info = ResolveColorOrFallback(options, bindings.Info, bindings.Secondary, "#2196F3", useDark),
            InfoText = ResolveTextOrFallback(options, bindings.Info, bindings.Secondary, "#FFFFFF", useDark),
            Success = ResolveColor(options, bindings.Success, "#00C853", useDark),
            SuccessText = ResolveText(options, bindings.Success, "#FFFFFF", useDark),
            Warning = ResolveColor(options, bindings.Warning, "#FF9800", useDark),
            WarningText = ResolveText(options, bindings.Warning, "#FFFFFF", useDark),
            Error = ResolveColor(options, bindings.Error, "#F44336", useDark),
            ErrorText = ResolveText(options, bindings.Error, "#FFFFFF", useDark),
            Dark = ResolveColor(options, bindings.Dark, "#424242", useDark),
            DarkText = ResolveText(options, bindings.Dark, "#FFFFFF", useDark),
            TextPrimary = ResolveContent(options, bindings.TextPrimary, "rgba(0,0,0,0.87)", useDark),
            TextSecondary = ResolveContent(options, bindings.TextSecondary, "rgba(0,0,0,0.54)", useDark),
            TextDisabled = ResolveDisabled(options, bindings.TextDisabled, useDark),
            ActionDefault = ResolveColor(options, bindings.ActionDefault, bindings.Primary, useDark),
            ActionDefaultHover = ResolveHover(options, bindings.ActionDefault, useDark),
            ActionDisabled = ResolveDisabled(options, bindings.TextDisabled, useDark),
            ActionDisabledBackground = ResolveDisabledBackground(options, bindings.ActionDisabled),
            Surface = ResolveSurface(options, bindings.Surface, useDark),
            Background = ResolveSurface(options, bindings.Background, useDark),
            BackgroundGray = ResolveGray(options, bindings.Background, useDark),
            DrawerBackground = ResolveSurface(options, bindings.DrawerBackground, useDark),
            DrawerText = ResolveContent(options, bindings.DrawerText, "rgba(0,0,0,0.87)", useDark),
            DrawerIcon = ResolveContent(options, bindings.DrawerIcon, "rgba(0,0,0,0.54)", useDark),
            AppbarBackground = ResolveColorOrFallback(options, bindings.AppbarBackground, bindings.Secondary, "#594AE2", useDark),
            AppbarText = ResolveText(options, bindings.AppbarText, "#FFFFFF", useDark),
            LinesDefault = ResolveBorder(options, bindings.LinesDefault, useDark),
            LinesInputs = ResolveBorder(options, bindings.LinesInputs, useDark),
            Divider = ResolveBorder(options, bindings.LinesDefault, useDark),
            DividerLight = ResolveDividerLight(options, bindings.LinesDefault, useDark),
            TableLines = ResolveBorder(options, bindings.LinesDefault, useDark),
            TableStriped = ResolveTableStriped(options, bindings.Surface, useDark),
            TableHover = ResolveHover(options, bindings.ActionDefault, useDark),
            Skeleton = ResolveSkeleton(options, bindings.Surface, useDark),
            GrayDefault = ResolveGray(options, bindings.TextSecondary, useDark),
            GrayLight = ResolveGrayLight(options, bindings.Surface, useDark),
            GrayLighter = ResolveGrayLighter(options, bindings.Surface, useDark),
            GrayDark = ResolveGrayDark(options, bindings.TextPrimary, useDark),
            GrayDarker = ResolveGrayDarker(options, bindings.TextPrimary, useDark),
            OverlayLight = ResolveOverlay(options, bindings.Surface, 0.3, light: true, useDark),
            OverlayDark = ResolveOverlay(options, bindings.Dark, 0.5, light: false, useDark)
        };
    }

    public static NerdMudBlazorPaletteBindings CreateConventionBindings() => new()
    {
        Primary = NerdDesignSystemUi.PrimaryAction,
        Secondary = NerdDesignSystemUi.SecondaryAction,
        Tertiary = NerdDesignSystemUi.Highlight,
        Info = NerdDesignSystemUi.Info,
        Success = NerdDesignSystemUi.Success,
        Warning = NerdDesignSystemUi.Highlight,
        Error = NerdDesignSystemUi.Danger,
        Dark = NerdDesignSystemUi.MutedContent,
        Surface = NerdDesignSystemUi.PageSurface,
        Background = NerdDesignSystemUi.PageSurface,
        TextPrimary = NerdDesignSystemUi.PageSurface,
        TextSecondary = NerdDesignSystemUi.MutedContent,
        TextDisabled = NerdDesignSystemUi.MutedContent,
        ActionDefault = NerdDesignSystemUi.PrimaryAction,
        AppbarBackground = NerdDesignSystemUi.BrandChrome,
        AppbarText = NerdDesignSystemUi.OnBrandChrome,
        DrawerBackground = NerdDesignSystemUi.NavSurface,
        DrawerText = NerdDesignSystemUi.NavItem,
        DrawerIcon = NerdDesignSystemUi.NavItem,
        LinesDefault = NerdDesignSystemUi.InputBorder,
        LinesInputs = NerdDesignSystemUi.InputBorder
    };

    private static string ResolveColor(NerdDesignTokenOptions options, string? alias, string fallback, bool useDark = false) =>
        TryResolveAliasColor(options, alias, out var color, useDark) ? color : fallback;

    private static string ResolveColorOrFallback(
        NerdDesignTokenOptions options,
        string? alias,
        string? fallbackAlias,
        string hardFallback,
        bool useDark = false) =>
        TryResolveAliasColor(options, alias, out var color, useDark)
            ? color
            : TryResolveAliasColor(options, fallbackAlias, out var fallback, useDark)
                ? fallback
                : hardFallback;

    private static string ResolveText(NerdDesignTokenOptions options, string? alias, string fallback, bool useDark = false) =>
        TryResolveAliasText(options, alias, out var text, useDark) ? text : fallback;

    private static string ResolveTextOrFallback(
        NerdDesignTokenOptions options,
        string? alias,
        string? fallbackAlias,
        string hardFallback,
        bool useDark = false) =>
        TryResolveAliasText(options, alias, out var text, useDark)
            ? text
            : TryResolveAliasText(options, fallbackAlias, out var fallback, useDark)
                ? fallback
                : hardFallback;

    private static string ResolveContent(NerdDesignTokenOptions options, string? alias, string fallback, bool useDark = false) =>
        TryResolveAliasContent(options, alias, out var content, useDark) ? content : fallback;

    private static string ResolveSurface(NerdDesignTokenOptions options, string? alias, bool useDark = false) =>
        TryResolveAliasSurface(options, alias, out var surface, useDark) ? surface : "#FFFFFF";

    private static string ResolveHover(NerdDesignTokenOptions options, string? alias, bool useDark = false)
    {
        if (TryResolveAliasToken(options, alias, out var token))
        {
            var baseColor = useDark
                ? token.Dark ?? token.Light ?? token.Value
                : token.Light ?? token.Value;
            var hover = token.Hover ?? NerdColorDerivatives.Darken(baseColor, 0.08);
            return $"color-mix(in srgb, {hover} 8%, transparent)";
        }

        return "rgba(0,0,0,0.04)";
    }

    private static string ResolveDisabled(NerdDesignTokenOptions options, string? alias, bool useDark = false)
    {
        if (TryResolveAliasToken(options, alias, out var token))
        {
            var baseColor = useDark
                ? token.Dark ?? token.Light ?? token.Value
                : token.Light ?? token.Value;
            return token.Disabled ?? NerdColorDerivatives.Lighten(baseColor, 0.35);
        }

        return "rgba(0,0,0,0.38)";
    }

    private static string ResolveDisabledBackground(NerdDesignTokenOptions options, string? alias) =>
        "rgba(0,0,0,0.12)";

    private static string ResolveBorder(NerdDesignTokenOptions options, string? alias, bool useDark = false)
    {
        if (TryResolveAliasToken(options, alias, out var token))
        {
            var light = useDark
                ? token.Dark ?? token.Light ?? token.Value
                : token.Light ?? token.Value;
            return token.Border ?? token.Content ?? NerdColorParser.ContentText(light, token.ContrastText);
        }

        return "rgba(0,0,0,0.12)";
    }

    private static string ResolveDividerLight(NerdDesignTokenOptions options, string? alias, bool useDark = false) =>
        $"color-mix(in srgb, {ResolveBorder(options, alias, useDark)} 50%, transparent)";

    private static string ResolveTableStriped(NerdDesignTokenOptions options, string? alias, bool useDark = false) =>
        $"color-mix(in srgb, {ResolveContent(options, alias, "rgba(0,0,0,0.87)", useDark)} 2%, transparent)";

    private static string ResolveSkeleton(NerdDesignTokenOptions options, string? alias, bool useDark = false) =>
        $"color-mix(in srgb, {ResolveContent(options, alias, "rgba(0,0,0,0.87)", useDark)} 11%, transparent)";

    private static string ResolveGray(NerdDesignTokenOptions options, string? alias, bool useDark = false) =>
        ResolveContent(options, alias, "#9E9E9E", useDark);

    private static string ResolveGrayLight(NerdDesignTokenOptions options, string? alias, bool useDark = false) =>
        $"color-mix(in srgb, {ResolveSurface(options, alias, useDark)} 85%, {ResolveContent(options, alias, "#000", useDark)})";

    private static string ResolveGrayLighter(NerdDesignTokenOptions options, string? alias, bool useDark = false) =>
        $"color-mix(in srgb, {ResolveSurface(options, alias, useDark)} 70%, {ResolveContent(options, alias, "#000", useDark)})";

    private static string ResolveGrayDark(NerdDesignTokenOptions options, string? alias, bool useDark = false) =>
        $"color-mix(in srgb, {ResolveContent(options, alias, "#000", useDark)} 54%, {ResolveSurface(options, alias, useDark)})";

    private static string ResolveGrayDarker(NerdDesignTokenOptions options, string? alias, bool useDark = false) =>
        $"color-mix(in srgb, {ResolveContent(options, alias, "#000", useDark)} 62%, {ResolveSurface(options, alias, useDark)})";

    private static string ResolveOverlay(NerdDesignTokenOptions options, string? alias, double alpha, bool light, bool useDark = false)
    {
        var baseColor = light
            ? ResolveSurface(options, alias, useDark)
            : ResolveColor(options, alias, "#212121", useDark);
        var rgb = NerdColorDerivatives.ToRgbString(baseColor);
        return $"rgba({rgb}, {alpha.ToString(System.Globalization.CultureInfo.InvariantCulture)})";
    }

    private static bool TryResolveAliasColor(
        NerdDesignTokenOptions options,
        string? alias,
        out string color,
        bool useDark = false)
    {
        color = string.Empty;
        if (!TryResolveAliasToken(options, alias, out var token))
        {
            return false;
        }

        color = useDark
            ? token.Dark ?? token.Light ?? token.Value
            : token.Light ?? token.Value;
        return true;
    }

    private static bool TryResolveAliasText(
        NerdDesignTokenOptions options,
        string? alias,
        out string text,
        bool useDark = false)
    {
        text = string.Empty;
        if (!TryResolveAliasToken(options, alias, out var token))
        {
            return false;
        }

        var light = useDark
            ? token.Dark ?? token.Light ?? token.Value
            : token.Light ?? token.Value;
        var contrast = useDark
            ? token.DarkContrastText ?? token.ContrastText ?? NerdColorValue.ContrastText(light)
            : token.ContrastText ?? NerdColorValue.ContrastText(light);
        text = contrast;
        return true;
    }

    private static bool TryResolveAliasContent(
        NerdDesignTokenOptions options,
        string? alias,
        out string content,
        bool useDark = false)
    {
        content = string.Empty;
        if (!TryResolveAliasToken(options, alias, out var token))
        {
            return false;
        }

        var light = useDark
            ? token.Dark ?? token.Light ?? token.Value
            : token.Light ?? token.Value;
        var contrast = useDark
            ? token.DarkContrastText ?? token.ContrastText ?? NerdColorValue.ContrastText(light)
            : token.ContrastText ?? NerdColorValue.ContrastText(light);
        content = token.Content ?? NerdColorParser.ContentText(light, contrast);
        return true;
    }

    private static bool TryResolveAliasSurface(
        NerdDesignTokenOptions options,
        string? alias,
        out string surface,
        bool useDark = false)
    {
        surface = string.Empty;
        if (!TryResolveAliasToken(options, alias, out var token))
        {
            return false;
        }

        surface = useDark
            ? token.Surface ?? token.Dark ?? token.Light ?? token.Value
            : token.Surface ?? token.Light ?? token.Value;
        return true;
    }

    private static bool TryResolveAliasToken(NerdDesignTokenOptions options, string? alias, out NerdColorToken token)
    {
        token = null!;
        if (string.IsNullOrWhiteSpace(alias) || !options.Aliases.TryGetValue(alias, out var target))
        {
            return false;
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

        return options.Colors.TryGetValue(current, out token!);
    }

    public static NerdMudAliasColorBundle ResolveAliasBundle(
        NerdDesignTokenOptions options,
        string alias,
        NerdMudPaletteMode mode)
    {
        ArgumentNullException.ThrowIfNull(options);
        var useDark = mode == NerdMudPaletteMode.Dark;
        if (!options.Aliases.TryGetValue(alias, out _))
        {
            return new NerdMudAliasColorBundle(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
        }

        var color = ResolveColor(options, alias, "#594AE2", useDark);
        var text = ResolveText(options, alias, "#FFFFFF", useDark);
        var content = ResolveContent(options, alias, "rgba(0,0,0,0.87)", useDark);
        var border = ResolveBorder(options, alias, useDark);
        var hover = ResolveHover(options, alias, useDark);
        var surface = ResolveSurface(options, alias, useDark);
        return new NerdMudAliasColorBundle(color, text, content, border, hover, surface);
    }

    public static string ResolveNamedColor(NerdDesignTokenOptions options, string colorName, bool useDark) =>
        ResolveColor(options, colorName, "#594AE2", useDark);
}
