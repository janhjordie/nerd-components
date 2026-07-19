namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>
/// Resolves live studio pairings from configured brand recipes.
/// </summary>
public static class NerdRecipeStudioTools
{
    public static bool HasRecipes(NerdDesignTokenOptions options) =>
        options.Recipes.Count > 0;

    public static KeyValuePair<string, NerdDesignTokenRecipe> GetDefaultRecipe(NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (options.Recipes.Count == 0)
        {
            throw new InvalidOperationException("No recipes configured.");
        }

        if (options.Recipes.TryGetValue("hero", out var hero))
        {
            return new KeyValuePair<string, NerdDesignTokenRecipe>("hero", hero);
        }

        return options.Recipes
            .OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase)
            .First();
    }

    public static KeyValuePair<string, NerdDesignTokenRecipe>? FindRecipeForToken(
        string tokenName,
        NerdDesignTokenOptions options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenName);
        ArgumentNullException.ThrowIfNull(options);

        if (options.Recipes.Count == 0)
        {
            return null;
        }

        var ordered = OrderRecipes(options.Recipes);

        var contentMatch = ordered.FirstOrDefault(pair => TokenEquals(pair.Value.Content, tokenName));
        if (!string.IsNullOrEmpty(contentMatch.Key))
        {
            return contentMatch;
        }

        var surfaceMatch = ordered.FirstOrDefault(pair => TokenEquals(pair.Value.Surface, tokenName));
        if (!string.IsNullOrEmpty(surfaceMatch.Key))
        {
            return surfaceMatch;
        }

        var actionMatch = ordered.FirstOrDefault(pair => TokenEquals(pair.Value.Action, tokenName));
        if (!string.IsNullOrEmpty(actionMatch.Key))
        {
            return actionMatch;
        }

        return null;
    }

    public static KeyValuePair<string, NerdDesignTokenRecipe>? FindRecipeForSurface(
        string surfaceToken,
        NerdDesignTokenOptions options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(surfaceToken);
        ArgumentNullException.ThrowIfNull(options);

        return OrderRecipes(options.Recipes)
            .FirstOrDefault(pair => TokenEquals(pair.Value.Surface, surfaceToken));
    }

    public static KeyValuePair<string, NerdDesignTokenRecipe>? FindRecipeForPairing(
        string surfaceToken,
        string contentToken,
        NerdDesignTokenOptions options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(surfaceToken);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentToken);
        ArgumentNullException.ThrowIfNull(options);

        return OrderRecipes(options.Recipes)
            .FirstOrDefault(pair =>
                TokenEquals(pair.Value.Surface, surfaceToken) &&
                TokenEquals(pair.Value.Content, contentToken));
    }

    public static IReadOnlyList<string> GetStudioSurfaceOptions(NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return options.Recipes.Values
            .Select(recipe => recipe.Surface)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static IReadOnlyList<string> GetStudioContentOptions(
        string surfaceToken,
        NerdDesignTokenOptions options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(surfaceToken);
        ArgumentNullException.ThrowIfNull(options);

        return options.Recipes.Values
            .Where(recipe => TokenEquals(recipe.Surface, surfaceToken))
            .Select(recipe => recipe.Content)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static IReadOnlyList<string> GetStudioActionOptions(
        string surfaceToken,
        string contentToken,
        NerdDesignTokenOptions options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(surfaceToken);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentToken);
        ArgumentNullException.ThrowIfNull(options);

        var fromRecipes = options.Recipes.Values
            .Where(recipe =>
                TokenEquals(recipe.Surface, surfaceToken) &&
                TokenEquals(recipe.Content, contentToken) &&
                !string.IsNullOrWhiteSpace(recipe.Action))
            .Select(recipe => recipe.Action!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (fromRecipes.Count > 0)
        {
            return fromRecipes;
        }

        return
        [
            NerdTokenPairingTools.SuggestActionToken(options, surfaceToken, contentToken)
        ];
    }

    private static IEnumerable<KeyValuePair<string, NerdDesignTokenRecipe>> OrderRecipes(
        IReadOnlyDictionary<string, NerdDesignTokenRecipe> recipes) =>
        recipes
            .OrderBy(pair => RecipePriority(pair.Key))
            .ThenBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase);

    private static int RecipePriority(string recipeName) =>
        string.Equals(recipeName, "hero", StringComparison.OrdinalIgnoreCase) ? 0 : 1;

    private static bool TokenEquals(string? left, string? right) =>
        string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
}
