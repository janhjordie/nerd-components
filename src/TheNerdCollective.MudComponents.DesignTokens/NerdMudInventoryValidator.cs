using System.Text.RegularExpressions;

namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>
/// Validates committed MudBlazor component inventory YAML files (HR-133).
/// </summary>
public static class NerdMudInventoryValidator
{
    private static readonly Regex ComponentLine = new(@"^component:\s*(.+)$", RegexOptions.Multiline);
    private static readonly Regex ClassificationLine = new(@"^classification:\s*(P[0-4])\s*$", RegexOptions.Multiline);
    private static readonly Regex PriorityLine = new(@"^priority:\s*(P[0-4])\s*$", RegexOptions.Multiline);
    private static readonly Regex StatesLine = new(@"^states:\s*$", RegexOptions.Multiline);
    private static readonly Regex PartsLine = new(@"^parts:\s*$", RegexOptions.Multiline);
    private static readonly Regex PortalLine = new(@"^portal:\s*true\s*$", RegexOptions.Multiline);

    public static IReadOnlyList<string> ValidateFile(string yaml, string? fileName = null)
    {
        var errors = new List<string>();
        var label = fileName ?? "inventory";

        if (string.IsNullOrWhiteSpace(yaml))
        {
            errors.Add($"{label}: file is empty.");
            return errors;
        }

        var componentMatch = ComponentLine.Match(yaml);
        if (!componentMatch.Success || string.IsNullOrWhiteSpace(componentMatch.Groups[1].Value))
        {
            errors.Add($"{label}: missing required 'component:' key.");
        }

        var classification = ResolveClassification(yaml);
        if (classification is null)
        {
            errors.Add($"{label}: missing 'classification: P0–P4' or 'priority: P0–P4'.");
            return errors;
        }

        if (classification is "P1" or "P3")
        {
            if (!StatesLine.IsMatch(yaml))
            {
                errors.Add($"{label}: P1/P3 inventory must declare 'states:'.");
            }
        }

        if (classification is "P3" && PortalLine.IsMatch(yaml) && !PartsLine.IsMatch(yaml))
        {
            errors.Add($"{label}: portal composites (P3) must declare 'parts:'.");
        }

        return errors;
    }

    public static IReadOnlyList<string> ValidateDirectory(string inventoryDirectory)
    {
        if (!Directory.Exists(inventoryDirectory))
        {
            return [$"Inventory directory not found: {inventoryDirectory}"];
        }

        var errors = new List<string>();
        foreach (var file in Directory.EnumerateFiles(inventoryDirectory, "*.yaml", SearchOption.TopDirectoryOnly))
        {
            var yaml = File.ReadAllText(file);
            errors.AddRange(ValidateFile(yaml, Path.GetFileName(file)));
        }

        if (errors.Count == 0 && !Directory.EnumerateFiles(inventoryDirectory, "*.yaml").Any())
        {
            errors.Add($"No inventory YAML files found in {inventoryDirectory}.");
        }

        return errors;
    }

    /// <summary>
    /// Ensures every upstream SCSS file listed in sources/COMPONENTS.md has a committed inventory YAML (HR-133 wave 14 gate).
    /// </summary>
    public static IReadOnlyList<string> ValidateHarvestCoverage(string? mudVersion = null)
    {
        var version = mudVersion ?? new NerdDesignTokenOptions().MudBlazorVersion;
        var root = FindDesignTokensRoot();
        var inventoryDirectory = Path.Combine(root, "reference", "mudblazor", version, "inventory");
        var componentsList = Path.Combine(root, "reference", "mudblazor", version, "sources", "COMPONENTS.md");
        if (!File.Exists(componentsList))
        {
            return [$"COMPONENTS.md not found for MudBlazor {version}: {componentsList}"];
        }

        var errors = new List<string>();
        var inventoryFiles = Directory.Exists(inventoryDirectory)
            ? Directory.EnumerateFiles(inventoryDirectory, "*.yaml", SearchOption.TopDirectoryOnly)
                .Select(Path.GetFileName)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .ToHashSet(StringComparer.OrdinalIgnoreCase)
            : [];

        foreach (var line in File.ReadAllLines(componentsList))
        {
            var trimmed = line.Trim();
            if (!trimmed.StartsWith('_') || !trimmed.EndsWith(".scss", StringComparison.Ordinal))
            {
                continue;
            }

            var scssBase = trimmed[..^".scss".Length];
            var inventoryFile = ResolveInventoryFileName(scssBase);
            if (!inventoryFiles.Contains(inventoryFile))
            {
                errors.Add($"Missing inventory YAML for {trimmed}: expected inventory/{inventoryFile}");
            }
        }

        return errors;
    }

    private static string ResolveInventoryFileName(string scssBase) =>
        scssBase switch
        {
            "_charts" => "_chart.yaml",
            _ => $"{scssBase}.yaml",
        };

    public static string ResolveInventoryDirectory(string? mudVersion = null)
    {
        var version = mudVersion ?? new NerdDesignTokenOptions().MudBlazorVersion;
        var root = FindDesignTokensRoot();
        return Path.Combine(root, "reference", "mudblazor", version, "inventory");
    }

    private static string? ResolveClassification(string yaml)
    {
        var classification = ClassificationLine.Match(yaml);
        if (classification.Success)
        {
            return classification.Groups[1].Value;
        }

        var priority = PriorityLine.Match(yaml);
        return priority.Success ? priority.Groups[1].Value : null;
    }

    private static string FindDesignTokensRoot()
    {
        var assemblyDir = Path.GetDirectoryName(typeof(NerdMudInventoryValidator).Assembly.Location);
        if (!string.IsNullOrWhiteSpace(assemblyDir))
        {
            var archiveRoot = TryResolveArchiveRoot(assemblyDir);
            if (archiveRoot is not null)
            {
                return archiveRoot;
            }

            if (Directory.Exists(Path.Combine(assemblyDir, "reference", "mudblazor")))
            {
                return assemblyDir;
            }
        }

        var archiveFromBase = TryResolveArchiveRoot(AppContext.BaseDirectory);
        if (archiveFromBase is not null)
        {
            return archiveFromBase;
        }

        if (Directory.Exists(Path.Combine(AppContext.BaseDirectory, "reference", "mudblazor")))
        {
            return AppContext.BaseDirectory;
        }

        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "src", "TheNerdCollective.MudComponents.DesignTokens");
            if (Directory.Exists(candidate))
            {
                return candidate;
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

    private static string? TryResolveArchiveRoot(string startDirectory)
    {
        var directory = new DirectoryInfo(startDirectory);
        while (directory is not null)
        {
            var mudblazor = Path.Combine(directory.FullName, "reference", "mudblazor");
            if (Directory.Exists(mudblazor))
            {
                foreach (var versionDir in Directory.EnumerateDirectories(mudblazor))
                {
                    if (File.Exists(Path.Combine(versionDir, "sources", "COMPONENTS.md")))
                    {
                        return directory.FullName;
                    }
                }
            }

            directory = directory.Parent;
        }

        return null;
    }
}
