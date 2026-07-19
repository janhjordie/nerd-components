using System.Text;
using System.Text.RegularExpressions;

namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>
/// Diff helpers for MudBlazor upgrade rehearsals (HR-147 / TS-053).
/// </summary>
public static class NerdMudUpgradeDiffTools
{
    private static readonly Regex PaletteVariablePattern = new(
        @"--mud-palette-[a-z0-9-]+",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static readonly Regex ThemeProviderPalettePattern = new(
        @"\$""--\{Palette\}-([a-z0-9-]+)",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    public static IReadOnlySet<string> ParsePaletteVariables(string text)
    {
        var variables = new HashSet<string>(StringComparer.Ordinal);
        foreach (Match match in PaletteVariablePattern.Matches(text))
        {
            variables.Add(match.Value.ToLowerInvariant());
        }

        foreach (Match match in ThemeProviderPalettePattern.Matches(text))
        {
            variables.Add($"--mud-palette-{match.Groups[1].Value.ToLowerInvariant()}");
        }

        return variables;
    }

    public static IReadOnlySet<string> ReadCommittedPaletteVariables(string designTokensRoot, string version)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(designTokensRoot);
        ArgumentException.ThrowIfNullOrWhiteSpace(version);

        if (string.Equals(version, MudBlazorPaletteManifest.MudBlazorVersion, StringComparison.Ordinal))
        {
            return MudBlazorPaletteManifest.AllPaletteVariables
                .Select(variable => variable.ToLowerInvariant())
                .ToHashSet(StringComparer.Ordinal);
        }

        var manifestPath = Path.Combine(
            designTokensRoot,
            "reference",
            "mudblazor",
            version,
            "PALETTE-MANIFEST.md");

        if (!File.Exists(manifestPath))
        {
            throw new FileNotFoundException($"Missing palette manifest for Mud {version}: {manifestPath}");
        }

        return ParsePaletteVariables(File.ReadAllText(manifestPath));
    }

    public static NerdMudPaletteManifestDiff DiffPaletteManifests(
        IReadOnlySet<string> baseline,
        IReadOnlySet<string> candidate)
    {
        ArgumentNullException.ThrowIfNull(baseline);
        ArgumentNullException.ThrowIfNull(candidate);

        return new NerdMudPaletteManifestDiff(
            Added: candidate.Except(baseline, StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList(),
            Removed: baseline.Except(candidate, StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList(),
            Unchanged: baseline.Intersect(candidate, StringComparer.Ordinal).Count());
    }

    public static string FormatUpgradeMarkdown(
        string fromVersion,
        string toVersion,
        NerdMudPaletteManifestDiff diff,
        string? candidateSourceUrl = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fromVersion);
        ArgumentException.ThrowIfNullOrWhiteSpace(toVersion);
        ArgumentNullException.ThrowIfNull(diff);

        var builder = new StringBuilder();
        builder.AppendLine($"# MudBlazor {fromVersion} → {toVersion} — palette manifest diff");
        builder.AppendLine();
        builder.AppendLine($"Generated: {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm} UTC");
        if (!string.IsNullOrWhiteSpace(candidateSourceUrl))
        {
            builder.AppendLine($"Candidate source: {candidateSourceUrl}");
        }

        builder.AppendLine();
        builder.AppendLine("## Summary");
        builder.AppendLine();
        builder.AppendLine($"- Added: **{diff.Added.Count}**");
        builder.AppendLine($"- Removed: **{diff.Removed.Count}**");
        builder.AppendLine($"- Unchanged: **{diff.Unchanged}**");
        builder.AppendLine();
        builder.AppendLine("## Adapter checklist");
        builder.AppendLine();
        builder.AppendLine("1. Update `MudBlazorPaletteManifest` + `PALETTE-MANIFEST.md` for the target version.");
        builder.AppendLine("2. Extend `MudBlazorBrandPaletteGenerator` if new palette slots require brand bindings.");
        builder.AppendLine("3. Re-run `scripts/harvest-mudblazor-inventory.sh` and fix inventory/CSS rule gaps.");
        builder.AppendLine("4. Refresh Playwright visual baselines under `reference/mudblazor/{version}/visual/`.");
        builder.AppendLine("5. Bump `NerdDesignTokenOptions.MudBlazorVersion` + all `PackageReference` pins.");
        builder.AppendLine();

        AppendVariableSection(builder, "Added palette variables", diff.Added);
        AppendVariableSection(builder, "Removed palette variables", diff.Removed);

        return builder.ToString().TrimEnd() + Environment.NewLine;
    }

    public static string ResolveDesignTokensRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(
                directory.FullName,
                "src",
                "TheNerdCollective.MudComponents.DesignTokens");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            "Could not locate TheNerdCollective.MudComponents.DesignTokens from the current working directory.");
    }

    public static string ResolveUpgradeReportPath(string designTokensRoot, string fromVersion, string toVersion) =>
        Path.Combine(designTokensRoot, "reference", "mudblazor", "upgrades", $"{fromVersion}-to-{toVersion}.md");

    private static void AppendVariableSection(StringBuilder builder, string title, IReadOnlyList<string> variables)
    {
        builder.AppendLine($"## {title}");
        builder.AppendLine();
        if (variables.Count == 0)
        {
            builder.AppendLine("_None._");
            builder.AppendLine();
            return;
        }

        foreach (var variable in variables)
        {
            builder.AppendLine($"- `{variable}`");
        }

        builder.AppendLine();
    }
}

public sealed record NerdMudPaletteManifestDiff(
    IReadOnlyList<string> Added,
    IReadOnlyList<string> Removed,
    int Unchanged);
