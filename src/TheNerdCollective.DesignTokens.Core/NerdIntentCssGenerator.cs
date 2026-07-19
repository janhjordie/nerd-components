using System.Text;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>
/// Emits framework-neutral <c>--nerd-intent-*</c> variables for cross-framework adapters.
/// </summary>
public static class NerdIntentCssGenerator
{
    public static void AppendBrandIntentVariables(StringBuilder css, NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(css);
        if (options.Aliases.Count == 0)
        {
            return;
        }

        var prefix = options.Prefix;
        var important = options.UseImportantOverrides ? " !important" : string.Empty;
        var root = $".{NerdIntentCssManifest.BrandRootClass(prefix)}";

        css.AppendLine($"{root} {{");
        foreach (var alias in options.Aliases.Keys.OrderBy(name => name, StringComparer.Ordinal))
        {
            AppendAliasIntentChannels(css, prefix, alias, important);
        }

        css.AppendLine("}");
        css.AppendLine();
    }

    private static void AppendAliasIntentChannels(
        StringBuilder css,
        string prefix,
        string alias,
        string importantSuffix)
    {
        var baseVariable = $"--{prefix}-color-{alias}";
        css.AppendLine($"  {NerdIntentCssManifest.IntentVariable(alias, "surface")}: var({baseVariable}-surface){importantSuffix};");
        css.AppendLine($"  {NerdIntentCssManifest.IntentVariable(alias, "content")}: var({baseVariable}-content){importantSuffix};");
        css.AppendLine($"  {NerdIntentCssManifest.IntentVariable(alias, "interactive")}: var({baseVariable}-interactive){importantSuffix};");
        css.AppendLine($"  {NerdIntentCssManifest.IntentVariable(alias, "hover")}: var({baseVariable}-hover){importantSuffix};");
        css.AppendLine($"  {NerdIntentCssManifest.IntentVariable(alias, "border")}: var({baseVariable}-border){importantSuffix};");
        css.AppendLine($"  {NerdIntentCssManifest.IntentVariable(alias, "disabled")}: var({baseVariable}-disabled){importantSuffix};");
    }

    public static IReadOnlyList<string> ResolveSemanticAliases(NerdDesignTokenOptions options) =>
        options.Aliases.Keys
            .Where(alias =>
                string.Equals(alias, NerdDesignSystemUi.PrimaryAction, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(alias, NerdDesignSystemUi.PageSurface, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(alias, NerdDesignSystemUi.BrandChrome, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(alias, NerdDesignSystemUi.NavSurface, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(alias, NerdDesignSystemUi.InputSurface, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(alias, NerdDesignSystemUi.MutedContent, StringComparison.OrdinalIgnoreCase))
            .Order(StringComparer.Ordinal)
            .ToList();
}
