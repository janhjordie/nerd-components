using System.Text.Json;

namespace TheNerdCollective.MudComponents.DesignTokens;

public static class NerdTokenPackDiff
{
    public static IReadOnlyList<NerdTokenPackDiffEntry> Compare(
        NerdTokenPack baseline,
        NerdTokenPack current)
    {
        ArgumentNullException.ThrowIfNull(baseline);
        ArgumentNullException.ThrowIfNull(current);
        var entries = new List<NerdTokenPackDiffEntry>();
        DiffDictionary("color", baseline.Colors, current.Colors, entries, SerializeColor, SerializeColor);
        DiffDictionary("alias", baseline.Aliases, current.Aliases, entries, static value => value, static value => value);
        DiffDictionary("radius", baseline.Radii, current.Radii, entries, static value => value, static value => value);
        DiffDictionary("shadow", baseline.Shadows, current.Shadows, entries, static value => value, static value => value);
        DiffDictionary("recipe", baseline.Recipes, current.Recipes, entries, SerializeRecipe, SerializeRecipe);
        return entries;
    }

    public static IReadOnlyList<NerdTokenPackDiffEntry> CompareToPreset(
        string presetName,
        NerdDesignTokenOptions current,
        string clientId = "current")
    {
        ArgumentNullException.ThrowIfNull(current);
        var baseline = NerdTokenPack.FromPreset(presetName, presetName);
        var currentPack = NerdTokenPack.FromOptions(current, clientId);
        return Compare(baseline, currentPack);
    }

    private static void DiffDictionary<TValue>(
        string section,
        IReadOnlyDictionary<string, TValue> baseline,
        IReadOnlyDictionary<string, TValue> current,
        ICollection<NerdTokenPackDiffEntry> entries,
        Func<TValue, string> formatBaseline,
        Func<TValue, string> formatCurrent)
    {
        foreach (var name in baseline.Keys.Union(current.Keys, StringComparer.OrdinalIgnoreCase).OrderBy(name => name, StringComparer.Ordinal))
        {
            var inBaseline = baseline.TryGetValue(name, out var baselineValue);
            var inCurrent = current.TryGetValue(name, out var currentValue);
            if (inBaseline && !inCurrent)
            {
                entries.Add(new NerdTokenPackDiffEntry(section, name, NerdTokenPackDiffKind.Removed, formatBaseline(baselineValue!), null));
                continue;
            }

            if (!inBaseline && inCurrent)
            {
                entries.Add(new NerdTokenPackDiffEntry(section, name, NerdTokenPackDiffKind.Added, null, formatCurrent(currentValue!)));
                continue;
            }

            if (inBaseline && inCurrent && !string.Equals(formatBaseline(baselineValue!), formatCurrent(currentValue!), StringComparison.Ordinal))
            {
                entries.Add(new NerdTokenPackDiffEntry(
                    section,
                    name,
                    NerdTokenPackDiffKind.Modified,
                    formatBaseline(baselineValue!),
                    formatCurrent(currentValue!)));
            }
        }
    }

    private static string SerializeColor(NerdColorToken token) =>
        JsonSerializer.Serialize(token);

    private static string SerializeRecipe(NerdDesignTokenRecipe recipe) =>
        JsonSerializer.Serialize(recipe);
}
