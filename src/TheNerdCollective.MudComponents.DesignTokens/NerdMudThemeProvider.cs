using System.Text;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>
/// Single <see cref="MudThemeProvider"/> emitting brand :root + intent/recipe PseudoCss scopes (HR-170).
/// </summary>
public partial class NerdMudThemeProvider : MudThemeProvider
{
    [Parameter]
    public NerdDesignTokenOptions? DesignTokenOptions { get; set; }

    private bool IsDark => base.IsDarkMode;

    protected string BuildNerdScopedTheme()
    {
        var themeStringBuilder = new StringBuilder();
        themeStringBuilder.AppendLine("<style class=\"mud-theme-provider\">");
        themeStringBuilder.AppendLine(":root {");
        GenerateTheme(themeStringBuilder);
        if (DesignTokenOptions is not null)
        {
            NerdMudRootTokenVariables.Append(themeStringBuilder, DesignTokenOptions);
        }
        themeStringBuilder.AppendLine("}");

        if (DesignTokenOptions?.UseIntentPseudoCssThemes == true)
        {
            AppendIntentScopes(themeStringBuilder);
            AppendRecipeScopes(themeStringBuilder);
        }

        themeStringBuilder.AppendLine("</style>");
        return themeStringBuilder.ToString();
    }

    private void AppendIntentScopes(StringBuilder themeStringBuilder)
    {
        var options = DesignTokenOptions!;
        var brandTheme = Theme ?? NerdMudThemeFactory.Create(options);

        foreach (var alias in NerdMudIntentPaletteMap.GetIntentAliases(options))
        {
            var intentTheme = NerdMudIntentThemeFactory.CreateIntentTheme(options, alias, brandTheme, IsDark);
            var palette = IsDark ? intentTheme.PaletteDark : intentTheme.PaletteLight;
            themeStringBuilder.Append(NerdMudIntentPaletteMap.GetPseudoCssScope(options, alias));
            themeStringBuilder.AppendLine(" {");
            NerdMudThemeCssEmitter.AppendPalette(themeStringBuilder, palette);
            themeStringBuilder.AppendLine("}");
        }
    }

    private void AppendRecipeScopes(StringBuilder themeStringBuilder)
    {
        var options = DesignTokenOptions!;
        var brandTheme = Theme ?? NerdMudThemeFactory.Create(options);

        foreach (var recipeName in options.Recipes.Keys.OrderBy(name => name, StringComparer.OrdinalIgnoreCase))
        {
            var recipeTheme = NerdMudRecipeThemeFactory.CreateRecipeTheme(
                options,
                recipeName,
                brandTheme,
                IsDark);
            var palette = IsDark ? recipeTheme.PaletteDark : recipeTheme.PaletteLight;
            themeStringBuilder.Append(NerdMudRecipeThemeFactory.GetPseudoCssScope(options, recipeName));
            themeStringBuilder.AppendLine(" {");
            NerdMudThemeCssEmitter.AppendPalette(themeStringBuilder, palette);
            themeStringBuilder.AppendLine("}");
        }
    }
}
