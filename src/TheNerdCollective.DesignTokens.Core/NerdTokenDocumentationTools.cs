namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>Resolves per-token documentation for catalog panels (HR-107).</summary>
public static class NerdTokenDocumentationTools
{
    public static NerdTokenDocumentation? Resolve(NerdDesignTokenOptions options, NerdTokenTreeNavigation selection)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(selection);

        return selection.Kind switch
        {
            NerdTokenTreeTargetKind.Color or NerdTokenTreeTargetKind.ThemeSetColor when options.Colors.TryGetValue(selection.TargetId, out var color) =>
                new NerdTokenDocumentation(
                    selection.TargetId,
                    color.Description,
                    color.Roles?.ToList() ?? [],
                    Usage: BuildColorUsage(selection.TargetId, options),
                    DoNot: null),
            NerdTokenTreeTargetKind.RecipeRole when options.Colors.TryGetValue(selection.TargetId, out var roleColor) =>
                new NerdTokenDocumentation(
                    selection.TargetId,
                    roleColor.Description,
                    roleColor.Roles?.ToList() ?? [],
                    Usage: BuildColorUsage(selection.TargetId, options),
                    DoNot: null),
            NerdTokenTreeTargetKind.Alias when options.IsAlias(selection.TargetId) =>
                new NerdTokenDocumentation(
                    selection.TargetId,
                    $"Semantic alias → {options.Aliases[selection.TargetId]}",
                    [],
                    Usage: NerdAliasChainTools.Format(NerdAliasChainTools.Build(options, selection.TargetId)),
                    DoNot: "Do not use raw palette tokens when an intent alias exists."),
            NerdTokenTreeTargetKind.Recipe when options.Recipes.TryGetValue(selection.TargetId, out var recipe) =>
                new NerdTokenDocumentation(
                    selection.TargetId,
                    recipe.Label,
                    [],
                    Usage: recipe.Usage,
                    DoNot: "Do not mix recipe surface/content tokens outside their recipe scope."),
            NerdTokenTreeTargetKind.Spacing =>
                new NerdTokenDocumentation(selection.TargetId, null, [], Usage: $"Use class `{options.Prefix}-space-{selection.TargetId}` or `pa-{selection.TargetId}`.", null),
            NerdTokenTreeTargetKind.MotionDuration =>
                new NerdTokenDocumentation(selection.TargetId, null, [], Usage: $"Use class `{options.Prefix}-motion-{selection.TargetId}`.", null),
            NerdTokenTreeTargetKind.MotionEasing =>
                new NerdTokenDocumentation(selection.TargetId, null, [], Usage: $"Use class `{options.Prefix}-ease-{selection.TargetId}`.", null),
            _ => null
        };
    }

    private static string? BuildColorUsage(string tokenName, NerdDesignTokenOptions options)
    {
        if (options.IsAlias(tokenName))
        {
            return null;
        }

        var recipe = options.Recipes.Values.FirstOrDefault(entry =>
            string.Equals(entry.Surface, tokenName, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(entry.Content, tokenName, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(entry.Action, tokenName, StringComparison.OrdinalIgnoreCase));

        return recipe is null
            ? $"CSS variable: --{options.Prefix}-color-{tokenName}"
            : $"Used in recipe with surface/content/action roles.";
    }
}

public sealed record NerdTokenDocumentation(
    string Title,
    string? Summary,
    IReadOnlyList<string> Roles,
    string? Usage,
    string? DoNot);
