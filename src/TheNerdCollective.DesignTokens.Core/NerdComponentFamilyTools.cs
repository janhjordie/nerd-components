using System.Text.RegularExpressions;

namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>
/// Loads framework-neutral component-family intent maps (HR-138 / TS-034).
/// </summary>
public static class NerdComponentFamilyTools
{
    private static readonly Regex FamilyLine = new(@"^family:\s*(.+)$", RegexOptions.Multiline);
    private static readonly Regex PartIdLine = new(@"^\s+-\s+id:\s*(.+)$", RegexOptions.Multiline);
    private static readonly Regex IntentAliasLine = new(@"^\s+intentAlias:\s*(.+)$", RegexOptions.Multiline);
    private static readonly Regex SelectorLine = new(@"^\s+-\s+(\S.+)$", RegexOptions.Multiline);

    public static IReadOnlyList<NerdComponentFamilyPart> LoadFamily(string familyName)
    {
        var directory = ResolveFamiliesDirectory();
        var file = Path.Combine(directory, $"{familyName}.yaml");
        if (!File.Exists(file))
        {
            return [];
        }

        return ParseParts(File.ReadAllText(file), familyName);
    }

    public static IReadOnlyList<NerdComponentFamilyPart> LoadAll()
    {
        var directory = ResolveFamiliesDirectory();
        if (!Directory.Exists(directory))
        {
            return [];
        }

        return Directory.EnumerateFiles(directory, "*.yaml", SearchOption.TopDirectoryOnly)
            .SelectMany(file =>
            {
                var yaml = File.ReadAllText(file);
                var familyMatch = FamilyLine.Match(yaml);
                var family = familyMatch.Success ? familyMatch.Groups[1].Value.Trim() : Path.GetFileNameWithoutExtension(file);
                return ParseParts(yaml, family);
            })
            .OrderBy(part => part.Family, StringComparer.Ordinal)
            .ThenBy(part => part.PartId, StringComparer.Ordinal)
            .ToList();
    }

    public static IReadOnlyList<NerdComponentFamilyBinding> ResolveBindings(NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return LoadAll()
            .Where(part => !string.IsNullOrWhiteSpace(part.IntentAlias))
            .Select(part => new NerdComponentFamilyBinding(
                part.Family,
                part.PartId,
                part.IntentAlias!,
                part.MudSelectors,
                ResolveCssVariable(options, part.IntentAlias!),
                ResolveSampleColor(options, part.IntentAlias!)))
            .ToList();
    }

    public static string ResolveCssVariable(NerdDesignTokenOptions options, string intentAlias)
    {
        var token = options.Aliases.TryGetValue(intentAlias, out var target)
            ? target
            : intentAlias;

        return $"--{options.Prefix}-color-{token}";
    }

    public static string? ResolveSampleColor(NerdDesignTokenOptions options, string intentAlias)
    {
        var token = options.Aliases.TryGetValue(intentAlias, out var target)
            ? target
            : intentAlias;

        return options.Colors.TryGetValue(token, out var color)
            ? color.Value
            : null;
    }

    private static IReadOnlyList<NerdComponentFamilyPart> ParseParts(string yaml, string family)
    {
        var parts = new List<NerdComponentFamilyPart>();
        var blocks = Regex.Split(yaml, @"(?=^\s+-\s+id:\s*)", RegexOptions.Multiline)
            .Where(block => block.Contains("id:", StringComparison.Ordinal))
            .ToList();

        foreach (var block in blocks)
        {
            var idMatch = PartIdLine.Match(block);
            if (!idMatch.Success)
            {
                continue;
            }

            var partId = idMatch.Groups[1].Value.Trim();
            var intentMatch = IntentAliasLine.Match(block);
            var intentAlias = intentMatch.Success ? intentMatch.Groups[1].Value.Trim() : null;
            var selectors = new List<string>();
            var inlineSelectors = Regex.Match(block, @"mudSelectors:\s*\[([^\]]*)\]");
            if (inlineSelectors.Success)
            {
                foreach (var value in inlineSelectors.Groups[1].Value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    selectors.Add(value.Trim().Trim('"', '\''));
                }
            }
            else
            {
                var selectorsIndex = block.IndexOf("mudSelectors:", StringComparison.Ordinal);
                if (selectorsIndex >= 0)
                {
                    var selectorsBlock = block[selectorsIndex..];
                    foreach (Match match in SelectorLine.Matches(selectorsBlock))
                    {
                        var value = match.Groups[1].Value.Trim();
                        if (value.StartsWith('['))
                        {
                            selectors.Add(value.Trim('[', ']').Split(',')[0].Trim());
                        }
                        else
                        {
                            selectors.Add(value);
                        }
                    }
                }
            }

            parts.Add(new NerdComponentFamilyPart(family, partId, intentAlias, selectors));
        }

        return parts;
    }

    private static string ResolveFamiliesDirectory()
    {
        var root = FindDesignTokensRoot();
        return Path.Combine(root, "reference", "component-families");
    }

    private static string FindDesignTokensRoot()
    {
        var assemblyDir = Path.GetDirectoryName(typeof(NerdComponentFamilyTools).Assembly.Location);
        if (!string.IsNullOrWhiteSpace(assemblyDir))
        {
            var families = Path.Combine(assemblyDir, "reference", "component-families");
            if (Directory.Exists(families))
            {
                return assemblyDir;
            }
        }

        if (Directory.Exists(Path.Combine(AppContext.BaseDirectory, "reference", "component-families")))
        {
            return AppContext.BaseDirectory;
        }

        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var project = Path.Combine(directory.FullName, "src", "TheNerdCollective.MudComponents.DesignTokens");
            if (Directory.Exists(project))
            {
                return project;
            }

            var sibling = Path.Combine(directory.FullName, "TheNerdCollective.MudComponents.DesignTokens");
            if (Directory.Exists(sibling))
            {
                return sibling;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not locate TheNerdCollective.MudComponents.DesignTokens project root.");
    }
}

public sealed record NerdComponentFamilyPart(
    string Family,
    string PartId,
    string? IntentAlias,
    IReadOnlyList<string> MudSelectors);

public sealed record NerdComponentFamilyBinding(
    string Family,
    string PartId,
    string IntentAlias,
    IReadOnlyList<string> MudSelectors,
    string CssVariable,
    string? SampleColor);
