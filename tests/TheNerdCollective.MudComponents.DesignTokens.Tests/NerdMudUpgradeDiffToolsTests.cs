using TheNerdCollective.MudComponents.DesignTokens;

namespace TheNerdCollective.MudComponents.DesignTokens.Tests;

public sealed class NerdMudUpgradeDiffToolsTests
{
    [Fact]
    public void ParsePaletteVariables_reads_theme_provider_interpolation()
    {
        const string sample = """
            themeStringBuilder.AppendLine($"--{Palette}-primary: {palette.Primary};");
            themeStringBuilder.AppendLine($"--{Palette}-surface-rgb: {palette.Surface.ToString()};");
            """;

        var variables = NerdMudUpgradeDiffTools.ParsePaletteVariables(sample);

        Assert.Contains("--mud-palette-primary", variables);
        Assert.Contains("--mud-palette-surface-rgb", variables);
    }

    [Fact]
    public void ParsePaletteVariables_extracts_unique_tokens()
    {
        const string sample = """
            --mud-palette-primary: #594AE2;
            color: var(--mud-palette-text-primary);
            border-color: var(--mud-palette-lines-default);
            """;

        var variables = NerdMudUpgradeDiffTools.ParsePaletteVariables(sample);

        Assert.Contains("--mud-palette-primary", variables);
        Assert.Contains("--mud-palette-text-primary", variables);
        Assert.Contains("--mud-palette-lines-default", variables);
        Assert.Equal(3, variables.Count);
    }

    [Fact]
    public void DiffPaletteManifests_reports_added_and_removed()
    {
        var baseline = new HashSet<string>(StringComparer.Ordinal)
        {
            "--mud-palette-primary",
            "--mud-palette-secondary"
        };
        var candidate = new HashSet<string>(StringComparer.Ordinal)
        {
            "--mud-palette-primary",
            "--mud-palette-tertiary"
        };

        var diff = NerdMudUpgradeDiffTools.DiffPaletteManifests(baseline, candidate);

        Assert.Equal(["--mud-palette-tertiary"], diff.Added);
        Assert.Equal(["--mud-palette-secondary"], diff.Removed);
        Assert.Equal(1, diff.Unchanged);
    }

    [Fact]
    public void ReadCommittedPaletteVariables_loads_current_manifest_catalog()
    {
        var root = NerdMudUpgradeDiffTools.ResolveDesignTokensRoot();
        var variables = NerdMudUpgradeDiffTools.ReadCommittedPaletteVariables(root, "9.7.0");

        Assert.Contains("--mud-palette-primary", variables);
        Assert.Contains("--mud-palette-surface", variables);
        Assert.True(variables.Count >= MudBlazorPaletteManifest.AllPaletteVariables.Count());
    }

    [Fact]
    public void Write_upgrade_report_when_requested()
    {
        var fromVersion = Environment.GetEnvironmentVariable("MUD_UPGRADE_FROM");
        var toVersion = Environment.GetEnvironmentVariable("MUD_UPGRADE_TO");
        var baselinePath = Environment.GetEnvironmentVariable("MUD_UPGRADE_BASELINE_SOURCE");
        var candidatePath = Environment.GetEnvironmentVariable("MUD_UPGRADE_CANDIDATE_SOURCE");
        if (string.IsNullOrWhiteSpace(fromVersion) ||
            string.IsNullOrWhiteSpace(toVersion) ||
            string.IsNullOrWhiteSpace(candidatePath) ||
            !File.Exists(candidatePath))
        {
            return;
        }

        var root = NerdMudUpgradeDiffTools.ResolveDesignTokensRoot();
        var baseline = !string.IsNullOrWhiteSpace(baselinePath) && File.Exists(baselinePath)
            ? NerdMudUpgradeDiffTools.ParsePaletteVariables(File.ReadAllText(baselinePath))
            : NerdMudUpgradeDiffTools.ReadCommittedPaletteVariables(root, fromVersion);
        var candidate = NerdMudUpgradeDiffTools.ParsePaletteVariables(File.ReadAllText(candidatePath));
        var diff = NerdMudUpgradeDiffTools.DiffPaletteManifests(baseline, candidate);
        var report = NerdMudUpgradeDiffTools.FormatUpgradeMarkdown(
            fromVersion,
            toVersion,
            diff,
            $"https://github.com/MudBlazor/MudBlazor/blob/v{toVersion}/src/MudBlazor/Components/ThemeProvider/MudThemeProvider.razor.cs");

        var outputPath = NerdMudUpgradeDiffTools.ResolveUpgradeReportPath(root, fromVersion, toVersion);
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        File.WriteAllText(outputPath, report);
        Assert.True(File.Exists(outputPath));
    }
}
