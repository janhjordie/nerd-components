using TheNerdCollective.MudComponents.DesignTokens;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.PlayBook;

/// <summary>Resolves PlayBook preview scope: semantic intents (default) or raw palette tokens.</summary>
public static class NerdPlayBookTokenFilter
{
    public const string AllIntents = "intents";
    public const string AllPalette = "all-palette";

    public static bool IsIntentScope(string filter) =>
        string.Equals(filter, AllIntents, StringComparison.Ordinal) ||
        NerdIntentCatalogTools.StandardIntents.Any(entry =>
            string.Equals(entry.Name, filter, StringComparison.OrdinalIgnoreCase));

    public static bool IsPaletteScope(string filter) =>
        string.Equals(filter, AllPalette, StringComparison.Ordinal);

    public static bool IsValid(NerdDesignTokenOptions options, string filter, IReadOnlyList<string> paletteTokenNames)
    {
        if (string.Equals(filter, AllIntents, StringComparison.Ordinal) ||
            string.Equals(filter, AllPalette, StringComparison.Ordinal))
        {
            return true;
        }

        if (NerdIntentCatalogTools.StandardIntents.Any(entry =>
                string.Equals(entry.Name, filter, StringComparison.OrdinalIgnoreCase)) &&
            options.Aliases.ContainsKey(filter))
        {
            return true;
        }

        return paletteTokenNames.Contains(filter, StringComparer.OrdinalIgnoreCase);
    }

    public static IReadOnlyList<string> ResolveVisibleClasses(
        NerdDesignTokenOptions options,
        string filter,
        IReadOnlyList<string> paletteTokenNames)
    {
        if (string.Equals(filter, AllIntents, StringComparison.Ordinal))
        {
            return NerdIntentCatalogTools.StandardIntents
                .Where(entry => options.Aliases.ContainsKey(entry.Name))
                .Select(entry => NerdIntentCatalogTools.FormatClass(options, entry.Name))
                .ToList();
        }

        if (string.Equals(filter, AllPalette, StringComparison.Ordinal))
        {
            return paletteTokenNames
                .Select(name => NerdDesignSystemUi.TokenClass(options.Prefix, name))
                .ToList();
        }

        if (NerdIntentCatalogTools.StandardIntents.Any(entry =>
                string.Equals(entry.Name, filter, StringComparison.OrdinalIgnoreCase)) &&
            options.Aliases.ContainsKey(filter))
        {
            return [NerdIntentCatalogTools.FormatClass(options, filter)];
        }

        return [NerdDesignSystemUi.TokenClass(options.Prefix, filter)];
    }
}
