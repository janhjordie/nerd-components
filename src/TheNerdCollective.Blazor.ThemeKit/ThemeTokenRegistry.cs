using MudBlazor;

namespace TheNerdCollective.Blazor.ThemeKit;

public static class ThemeTokenRegistry
{
    public static IReadOnlyList<ThemeTokenDefinition> V1 { get; } =
    [
        // Palette light
        ColorToken("light.primary", "Primary", "Palette (light)", t => t.PaletteLight.Primary, (t, v) => t.PaletteLight.Primary = v),
        ColorToken("light.primaryContrastText", "Primary contrast", "Palette (light)", t => t.PaletteLight.PrimaryContrastText, (t, v) => t.PaletteLight.PrimaryContrastText = v),
        ColorToken("light.secondary", "Secondary", "Palette (light)", t => t.PaletteLight.Secondary, (t, v) => t.PaletteLight.Secondary = v),
        ColorToken("light.secondaryContrastText", "Secondary contrast", "Palette (light)", t => t.PaletteLight.SecondaryContrastText, (t, v) => t.PaletteLight.SecondaryContrastText = v),
        ColorToken("light.tertiary", "Tertiary", "Palette (light)", t => t.PaletteLight.Tertiary, (t, v) => t.PaletteLight.Tertiary = v),
        ColorToken("light.tertiaryContrastText", "Tertiary contrast", "Palette (light)", t => t.PaletteLight.TertiaryContrastText, (t, v) => t.PaletteLight.TertiaryContrastText = v),
        ColorToken("light.background", "Background", "Palette (light)", t => t.PaletteLight.Background, (t, v) => t.PaletteLight.Background = v),
        ColorToken("light.surface", "Surface", "Palette (light)", t => t.PaletteLight.Surface, (t, v) => t.PaletteLight.Surface = v),
        ColorToken("light.appbarBackground", "App bar background", "Palette (light)", t => t.PaletteLight.AppbarBackground, (t, v) => t.PaletteLight.AppbarBackground = v),
        ColorToken("light.appbarText", "App bar text", "Palette (light)", t => t.PaletteLight.AppbarText, (t, v) => t.PaletteLight.AppbarText = v),
        ColorToken("light.drawerBackground", "Drawer background", "Palette (light)", t => t.PaletteLight.DrawerBackground, (t, v) => t.PaletteLight.DrawerBackground = v),
        ColorToken("light.drawerText", "Drawer text", "Palette (light)", t => t.PaletteLight.DrawerText, (t, v) => t.PaletteLight.DrawerText = v),
        ColorToken("light.textPrimary", "Text primary", "Palette (light)", t => t.PaletteLight.TextPrimary, (t, v) => t.PaletteLight.TextPrimary = v),

        // Palette dark
        ColorToken("dark.primary", "Primary", "Palette (dark)", t => t.PaletteDark.Primary, (t, v) => t.PaletteDark.Primary = v),
        ColorToken("dark.primaryContrastText", "Primary contrast", "Palette (dark)", t => t.PaletteDark.PrimaryContrastText, (t, v) => t.PaletteDark.PrimaryContrastText = v),
        ColorToken("dark.secondary", "Secondary", "Palette (dark)", t => t.PaletteDark.Secondary, (t, v) => t.PaletteDark.Secondary = v),
        ColorToken("dark.secondaryContrastText", "Secondary contrast", "Palette (dark)", t => t.PaletteDark.SecondaryContrastText, (t, v) => t.PaletteDark.SecondaryContrastText = v),
        ColorToken("dark.tertiary", "Tertiary", "Palette (dark)", t => t.PaletteDark.Tertiary, (t, v) => t.PaletteDark.Tertiary = v),
        ColorToken("dark.tertiaryContrastText", "Tertiary contrast", "Palette (dark)", t => t.PaletteDark.TertiaryContrastText, (t, v) => t.PaletteDark.TertiaryContrastText = v),
        ColorToken("dark.background", "Background", "Palette (dark)", t => t.PaletteDark.Background, (t, v) => t.PaletteDark.Background = v),
        ColorToken("dark.surface", "Surface", "Palette (dark)", t => t.PaletteDark.Surface, (t, v) => t.PaletteDark.Surface = v),
        ColorToken("dark.appbarBackground", "App bar background", "Palette (dark)", t => t.PaletteDark.AppbarBackground, (t, v) => t.PaletteDark.AppbarBackground = v),
        ColorToken("dark.appbarText", "App bar text", "Palette (dark)", t => t.PaletteDark.AppbarText, (t, v) => t.PaletteDark.AppbarText = v),
        ColorToken("dark.drawerBackground", "Drawer background", "Palette (dark)", t => t.PaletteDark.DrawerBackground, (t, v) => t.PaletteDark.DrawerBackground = v),
        ColorToken("dark.drawerText", "Drawer text", "Palette (dark)", t => t.PaletteDark.DrawerText, (t, v) => t.PaletteDark.DrawerText = v),
        ColorToken("dark.textPrimary", "Text primary", "Palette (dark)", t => t.PaletteDark.TextPrimary, (t, v) => t.PaletteDark.TextPrimary = v),
        ColorToken("dark.textSecondary", "Text secondary", "Palette (dark)", t => t.PaletteDark.TextSecondary, (t, v) => t.PaletteDark.TextSecondary = v),
        ColorToken("dark.actionDefault", "Action default", "Palette (dark)", t => t.PaletteDark.ActionDefault, (t, v) => t.PaletteDark.ActionDefault = v),
        ColorToken("dark.actionDisabled", "Action disabled", "Palette (dark)", t => t.PaletteDark.ActionDisabled, (t, v) => t.PaletteDark.ActionDisabled = v),

        // Layout
        TextToken("layout.defaultBorderRadius", "Border radius", "Layout",
            t => t.LayoutProperties.DefaultBorderRadius,
            (t, v) => t.LayoutProperties.DefaultBorderRadius = v),

        // Typography
        TextToken("typography.defaultFontFamily", "Default font family", "Typography",
            t => MudThemeTokenValues.GetFontFamily(t.Typography.Default.FontFamily),
            (t, v) => MudThemeTokenValues.SetFontFamily(t, v)),
    ];

    private static ThemeTokenDefinition ColorToken(
        string id,
        string label,
        string group,
        Func<MudTheme, MudBlazor.Utilities.MudColor> getColor,
        Action<MudTheme, MudBlazor.Utilities.MudColor> setColor)
        => new(
            id,
            label,
            ThemeTokenKind.Color,
            group,
            t => MudThemeTokenValues.GetColor(getColor(t)),
            (t, v) => setColor(t, new MudBlazor.Utilities.MudColor(v)));

    private static ThemeTokenDefinition TextToken(
        string id,
        string label,
        string group,
        Func<MudTheme, string?> getValue,
        Action<MudTheme, string> setValue)
        => new(id, label, ThemeTokenKind.Text, group, getValue, setValue);
}
