using MudBlazor;

namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>
/// Builds recipe-scoped <see cref="MudTheme"/> instances for shell layouts (HR-168).
/// </summary>
public static class NerdMudRecipeThemeFactory
{
    public static string GetPseudoCssScope(NerdDesignTokenOptions options, string recipeName) =>
        $":root .{options.Prefix}-recipe-{recipeName}";

    public static MudTheme CreateRecipeTheme(
        NerdDesignTokenOptions options,
        string recipeName,
        MudTheme brandTheme,
        bool isDarkMode)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(brandTheme);

        if (!options.Recipes.TryGetValue(recipeName, out var recipe))
        {
            return brandTheme;
        }

        var mode = isDarkMode ? NerdMudPaletteMode.Dark : NerdMudPaletteMode.Light;
        var useDark = isDarkMode;
        var brandMap = NerdMudBrandPaletteMap.Resolve(options, mode);
        var surface = NerdMudBrandPaletteMap.ResolveNamedColor(options, recipe.Surface, useDark);
        var content = NerdMudBrandPaletteMap.ResolveNamedColor(options, recipe.Content, useDark);
        var action = recipe.Action is not null
            ? NerdMudBrandPaletteMap.ResolveNamedColor(options, recipe.Action, useDark)
            : brandMap.Primary;
        var border = recipe.Border is not null
            ? NerdMudBrandPaletteMap.ResolveNamedColor(options, recipe.Border, useDark)
            : brandMap.LinesDefault;

        var map = brandMap with
        {
            Surface = surface,
            Background = surface,
            TextPrimary = content,
            Primary = action,
            ActionDefault = action,
            LinesDefault = border,
            LinesInputs = border
        };

        return new MudTheme
        {
            PaletteLight = isDarkMode
                ? brandTheme.PaletteLight
                : NerdMudThemePaletteConverter.ToPaletteLight(map),
            PaletteDark = isDarkMode
                ? NerdMudThemePaletteConverter.ToPaletteDark(map)
                : brandTheme.PaletteDark,
            Typography = brandTheme.Typography,
            Shadows = brandTheme.Shadows,
            LayoutProperties = brandTheme.LayoutProperties,
            ZIndex = brandTheme.ZIndex,
            PseudoCss = brandTheme.PseudoCss
        };
    }
}
