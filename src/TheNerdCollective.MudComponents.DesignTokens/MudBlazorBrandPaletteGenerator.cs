using System.Text;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.DesignTokens;

internal static class MudBlazorBrandPaletteGenerator
{
    /// <summary>
    /// Global Mud palette is owned by <see cref="NerdMudThemeFactory"/> via <see cref="MudBlazor.MudThemeProvider"/> (HR-158).
    /// </summary>
    public static void AppendBrandRootPalette(StringBuilder css, NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _ = css;
    }

    public static void AppendIntentPaletteOverrides(
        StringBuilder css,
        string root,
        string aliasName,
        string variable,
        string textVariable,
        string hoverVariable,
        string contentVariable,
        string borderVariable,
        string importantSuffix)
    {
        css.AppendLine($"{root} {{");
        NerdMudIntentPaletteMap.AppendIntentPaletteOverrides(
            css,
            aliasName,
            variable,
            textVariable,
            hoverVariable,
            contentVariable,
            borderVariable,
            importantSuffix);
        css.AppendLine("}");

        css.AppendLine($"{root} .mud-switch-base, {root}.mud-switch-base {{");
        css.AppendLine($"  color: var({textVariable}){importantSuffix};");
        css.AppendLine("}");

        css.AppendLine();
    }
}
