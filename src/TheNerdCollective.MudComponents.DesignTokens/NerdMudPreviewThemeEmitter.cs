using System.Text;
using MudBlazor;

using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>
/// Emits PseudoCss intent/recipe scopes for inactive brand packs (TS-069 / HR-168).
/// </summary>
public static class NerdMudPreviewThemeEmitter
{
    public static void AppendPreviewScopes(
        StringBuilder themeStringBuilder,
        IReadOnlyList<string> previewBrandPackIds,
        string activePrefix,
        MudTheme? brandTheme,
        bool isDark)
    {
        ArgumentNullException.ThrowIfNull(themeStringBuilder);
        ArgumentNullException.ThrowIfNull(previewBrandPackIds);

        foreach (var packId in previewBrandPackIds)
        {
            if (!NerdBrandPackRegistry.Instance.TryGet(packId, out _))
            {
                continue;
            }

            var previewOptions = new NerdDesignTokenOptions();
            NerdBrandPackRegistry.Instance.Configure(packId, previewOptions);
            if (string.Equals(previewOptions.Prefix, activePrefix, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            AppendIntentScopes(themeStringBuilder, previewOptions, brandTheme: null, isDark);
            AppendRecipeScopes(themeStringBuilder, previewOptions, brandTheme: null, isDark);
            AppendPseudoCssIntentBridges(themeStringBuilder, previewOptions);
        }
    }

    /// <summary>
    /// Bridge Mud components to --mud-palette-* set by PseudoCss scopes (preview packs lack token CSS).
    /// </summary>
    public static void AppendPseudoCssIntentBridges(StringBuilder themeStringBuilder, NerdDesignTokenOptions options)
    {
        foreach (var alias in NerdMudIntentPaletteMap.GetIntentAliases(options))
        {
            if (!IsActionIntent(alias))
            {
                continue;
            }

            var root = $".{options.Prefix}-{alias}";
            var (color, text, hover, border) = ResolvePaletteVariables(alias);

            themeStringBuilder.AppendLine($"{root}.mud-button-filled {{");
            themeStringBuilder.AppendLine($"  background-color: var({color});");
            themeStringBuilder.AppendLine($"  color: var({text});");
            themeStringBuilder.AppendLine("}");

            themeStringBuilder.AppendLine($"{root}.mud-button-outlined {{");
            themeStringBuilder.AppendLine($"  color: var({color});");
            themeStringBuilder.AppendLine($"  border-color: var({border});");
            themeStringBuilder.AppendLine("  background-color: transparent;");
            themeStringBuilder.AppendLine("}");
        }
    }

    private static bool IsActionIntent(string alias) =>
        string.Equals(alias, NerdDesignSystemUi.PrimaryAction, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(alias, NerdDesignSystemUi.SecondaryAction, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(alias, NerdDesignSystemUi.Success, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(alias, NerdDesignSystemUi.Danger, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(alias, NerdDesignSystemUi.Highlight, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(alias, NerdDesignSystemUi.Info, StringComparison.OrdinalIgnoreCase);

    private static (string Color, string Text, string Hover, string Border) ResolvePaletteVariables(string alias)
    {
        if (string.Equals(alias, NerdDesignSystemUi.SecondaryAction, StringComparison.OrdinalIgnoreCase))
        {
            return ("--mud-palette-secondary", "--mud-palette-secondary-text", "--mud-palette-secondary-hover", "--mud-palette-secondary");
        }

        if (string.Equals(alias, NerdDesignSystemUi.Highlight, StringComparison.OrdinalIgnoreCase))
        {
            return ("--mud-palette-warning", "--mud-palette-warning-text", "--mud-palette-warning-hover", "--mud-palette-warning");
        }

        if (string.Equals(alias, NerdDesignSystemUi.Info, StringComparison.OrdinalIgnoreCase))
        {
            return ("--mud-palette-info", "--mud-palette-info-text", "--mud-palette-info-hover", "--mud-palette-info");
        }

        if (string.Equals(alias, NerdDesignSystemUi.Success, StringComparison.OrdinalIgnoreCase))
        {
            return ("--mud-palette-success", "--mud-palette-success-text", "--mud-palette-success-hover", "--mud-palette-success");
        }

        if (string.Equals(alias, NerdDesignSystemUi.Danger, StringComparison.OrdinalIgnoreCase))
        {
            return ("--mud-palette-error", "--mud-palette-error-text", "--mud-palette-error-hover", "--mud-palette-error");
        }

        return ("--mud-palette-primary", "--mud-palette-primary-text", "--mud-palette-primary-hover", "--mud-palette-primary");
    }

    public static void AppendIntentScopes(
        StringBuilder themeStringBuilder,
        NerdDesignTokenOptions options,
        MudTheme? brandTheme,
        bool isDark)
    {
        var resolvedTheme = brandTheme ?? NerdMudThemeFactory.Create(options);

        foreach (var alias in NerdMudIntentPaletteMap.GetIntentAliases(options))
        {
            var intentTheme = NerdMudIntentThemeFactory.CreateIntentTheme(options, alias, resolvedTheme, isDark);
            var palette = isDark ? intentTheme.PaletteDark : intentTheme.PaletteLight;
            themeStringBuilder.Append(NerdMudIntentPaletteMap.GetPseudoCssScope(options, alias));
            themeStringBuilder.AppendLine(" {");
            NerdMudThemeCssEmitter.AppendPalette(themeStringBuilder, palette);
            themeStringBuilder.AppendLine("}");
        }
    }

    public static void AppendRecipeScopes(
        StringBuilder themeStringBuilder,
        NerdDesignTokenOptions options,
        MudTheme? brandTheme,
        bool isDark)
    {
        var resolvedTheme = brandTheme ?? NerdMudThemeFactory.Create(options);

        foreach (var recipeName in options.Recipes.Keys.OrderBy(name => name, StringComparer.OrdinalIgnoreCase))
        {
            var recipeTheme = NerdMudRecipeThemeFactory.CreateRecipeTheme(
                options,
                recipeName,
                resolvedTheme,
                isDark);
            var palette = isDark ? recipeTheme.PaletteDark : recipeTheme.PaletteLight;
            themeStringBuilder.Append(NerdMudRecipeThemeFactory.GetPseudoCssScope(options, recipeName));
            themeStringBuilder.AppendLine(" {");
            NerdMudThemeCssEmitter.AppendPalette(themeStringBuilder, palette);
            themeStringBuilder.AppendLine("}");
        }
    }
}
