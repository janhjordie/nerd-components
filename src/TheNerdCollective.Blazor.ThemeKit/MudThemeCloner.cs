using MudBlazor;

namespace TheNerdCollective.Blazor.ThemeKit;

public static class MudThemeCloner
{
    public static MudTheme Clone(MudTheme source)
    {
        var theme = new MudTheme();
        CopyPalette(source.PaletteLight, theme.PaletteLight);
        CopyPalette(source.PaletteDark, theme.PaletteDark);
        theme.LayoutProperties.DefaultBorderRadius = source.LayoutProperties.DefaultBorderRadius;

        if (source.Typography.Default.FontFamily is { Length: > 0 })
        {
            theme.Typography.Default.FontFamily = source.Typography.Default.FontFamily.ToArray();
        }

        return theme;
    }

    private static void CopyPalette(Palette from, Palette to)
    {
        to.Primary = from.Primary;
        to.Secondary = from.Secondary;
        to.Tertiary = from.Tertiary;
        to.Background = from.Background;
        to.Surface = from.Surface;
        to.AppbarBackground = from.AppbarBackground;
        to.AppbarText = from.AppbarText;
        to.DrawerBackground = from.DrawerBackground;
        to.DrawerText = from.DrawerText;
        to.TextPrimary = from.TextPrimary;
        to.TextSecondary = from.TextSecondary;
        to.PrimaryContrastText = from.PrimaryContrastText;
        to.SecondaryContrastText = from.SecondaryContrastText;
        to.TertiaryContrastText = from.TertiaryContrastText;
        to.ActionDefault = from.ActionDefault;
        to.ActionDisabled = from.ActionDisabled;
    }
}
