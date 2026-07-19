namespace TheNerdCollective.MudComponents.DesignTokens;

public static class NerdAliasUsageTools
{
    public static IReadOnlyList<string> GetUsages(NerdDesignTokenOptions options, string aliasName, string targetToken)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(aliasName);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetToken);
        var usages = new List<string>();

        foreach (var recipe in options.Recipes)
        {
            if (ReferencesToken(recipe.Value.Surface, targetToken))
            {
                usages.Add($"Recipe {recipe.Key} (surface)");
            }

            if (ReferencesToken(recipe.Value.Content, targetToken))
            {
                usages.Add($"Recipe {recipe.Key} (content)");
            }

            if (ReferencesToken(recipe.Value.Action, targetToken))
            {
                usages.Add($"Recipe {recipe.Key} (action)");
            }

            if (ReferencesToken(recipe.Value.Border, targetToken))
            {
                usages.Add($"Recipe {recipe.Key} (border)");
            }
        }

        foreach (var alias in options.Aliases)
        {
            if (string.Equals(alias.Key, aliasName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (string.Equals(alias.Value, targetToken, StringComparison.OrdinalIgnoreCase))
            {
                usages.Add($"Alias {alias.Key} → {alias.Value}");
            }
        }

        if (usages.Count == 0)
        {
            usages.Add("Class available as MudBlazor token class");
        }

        return usages;
    }

    private static bool ReferencesToken(string? value, string targetToken) =>
        !string.IsNullOrWhiteSpace(value) &&
        string.Equals(value, targetToken, StringComparison.OrdinalIgnoreCase);
}
