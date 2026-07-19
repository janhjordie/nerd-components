using System.Text;
using System.Text.RegularExpressions;

namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>
/// Builds and validates the adapter rule table from committed Mud inventory YAML (HR-134).
/// </summary>
public static class NerdMudInventoryRuleTable
{
    private static readonly Regex SelectorListItem = new(@"^\s+-\s+(\S.+)$", RegexOptions.Multiline);
    private static readonly Regex RequiredBlock = new(@"generator_required:\s*\n((?:\s+-\s+.+\n)+)", RegexOptions.Multiline);
    private static readonly Regex ForbiddenBlock = new(@"generator_forbidden:\s*\n((?:\s+-\s+.+\n)+)", RegexOptions.Multiline);
    private static readonly Regex StatesBlock = new(@"states:\s*\n((?:\s+-\s+name:.+\n(?:\s+.+\n)*?)+)", RegexOptions.Multiline);
    private static readonly Regex StatesSelectors = new(@"selectors:\s*\n((?:\s+-\s+.+\n)+)", RegexOptions.Multiline);

    public static IReadOnlyList<NerdMudInventoryRuleEntry> Load(string? mudVersion = null)
    {
        var directory = NerdMudInventoryValidator.ResolveInventoryDirectory(mudVersion);
        var entries = new List<NerdMudInventoryRuleEntry>();

        foreach (var file in Directory.EnumerateFiles(directory, "*.yaml", SearchOption.TopDirectoryOnly).OrderBy(Path.GetFileName))
        {
            var yaml = File.ReadAllText(file);
            var component = Regex.Match(yaml, @"^component:\s*(.+)$", RegexOptions.Multiline).Groups[1].Value.Trim();
            var classification = NerdMudInventoryValidator.ValidateFile(yaml, Path.GetFileName(file)).Count == 0
                ? ResolveClassification(yaml)
                : "P?";

            entries.Add(new NerdMudInventoryRuleEntry(
                component,
                Path.GetFileName(file),
                classification ?? "P?",
                ReadSelectors(yaml, RequiredBlock),
                ReadSelectors(yaml, ForbiddenBlock),
                ReadStateSelectors(yaml)));
        }

        return entries;
    }

    public static IReadOnlyList<string> ValidateGeneratedCss(
        NerdDesignTokenOptions options,
        IReadOnlyList<NerdMudInventoryRuleEntry>? entries = null)
    {
        entries ??= Load(options.MudBlazorVersion);
        var css = MudBlazorDesignTokenCssGenerator.Generate(options);
        var errors = new List<string>();

        foreach (var entry in entries)
        {
            foreach (var selector in entry.RequiredSelectors)
            {
                if (!ContainsSelector(css, selector))
                {
                    errors.Add($"{entry.Component}: missing required selector '{selector}'.");
                }
            }

            foreach (var selector in entry.ForbiddenSelectors)
            {
                if (ContainsSelector(css, selector))
                {
                    errors.Add($"{entry.Component}: forbidden selector '{selector}' found in generated CSS.");
                }
            }
        }

        return errors;
    }

    public static string RenderMarkdown(IReadOnlyList<NerdMudInventoryRuleEntry> entries)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Generated rule table — MudBlazor inventory");
        builder.AppendLine();
        builder.AppendLine("| Component | Class | Inventory | Required selectors | Forbidden |");
        builder.AppendLine("|-----------|-------|-----------|--------------------|-----------|");

        foreach (var entry in entries)
        {
            builder.Append('|').Append(entry.Component).Append('|')
                .Append(entry.Classification).Append('|')
                .Append(entry.InventoryFile).Append('|')
                .Append(string.Join(", ", entry.RequiredSelectors)).Append('|')
                .Append(string.Join(", ", entry.ForbiddenSelectors)).AppendLine("|");
        }

        return builder.ToString();
    }

    public static void WriteGeneratedArtifacts(string? mudVersion = null)
    {
        var version = mudVersion ?? new NerdDesignTokenOptions().MudBlazorVersion;
        var root = Path.Combine(
            NerdMudInventoryValidator.ResolveInventoryDirectory(version),
            "..");
        var entries = Load(version);
        File.WriteAllText(
            Path.Combine(root, "generated-rule-table.md"),
            RenderMarkdown(entries));
    }

    private static bool ContainsSelector(string css, string selector) =>
        css.Contains(selector, StringComparison.Ordinal);

    private static IReadOnlyList<string> ReadSelectors(string yaml, Regex blockPattern)
    {
        var match = blockPattern.Match(yaml);
        if (!match.Success)
        {
            return [];
        }

        return SelectorListItem.Matches(match.Groups[1].Value)
            .Select(item => TrimYamlScalar(item.Groups[1].Value))
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    private static string TrimYamlScalar(string value)
    {
        value = value.Trim();
        if (value.Length >= 2 &&
            ((value[0] == '\'' && value[^1] == '\'') || (value[0] == '"' && value[^1] == '"')))
        {
            return value[1..^1];
        }

        return value;
    }

    private static IReadOnlyList<string> ReadStateSelectors(string yaml)
    {
        var selectors = new List<string>();
        var states = StatesBlock.Match(yaml);
        if (!states.Success)
        {
            return selectors;
        }

        foreach (Match selectorBlock in StatesSelectors.Matches(states.Groups[1].Value))
        {
            selectors.AddRange(
                SelectorListItem.Matches(selectorBlock.Groups[1].Value)
                    .Select(item => item.Groups[1].Value.Trim()));
        }

        return selectors.Distinct(StringComparer.Ordinal).ToList();
    }

    private static string? ResolveClassification(string yaml)
    {
        var classification = Regex.Match(yaml, @"^classification:\s*(P[0-4])\s*$", RegexOptions.Multiline);
        if (classification.Success)
        {
            return classification.Groups[1].Value;
        }

        var priority = Regex.Match(yaml, @"^priority:\s*(P[0-4])\s*$", RegexOptions.Multiline);
        return priority.Success ? priority.Groups[1].Value : null;
    }
}

public sealed record NerdMudInventoryRuleEntry(
    string Component,
    string InventoryFile,
    string Classification,
    IReadOnlyList<string> RequiredSelectors,
    IReadOnlyList<string> ForbiddenSelectors,
    IReadOnlyList<string> StateSelectors);
