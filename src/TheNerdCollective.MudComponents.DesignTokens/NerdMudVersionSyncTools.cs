using System.Text.Json;
using System.Text.RegularExpressions;

namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>
/// Validates MudBlazor package version alignment across DesignTokens, archive manifest, and host apps (TS-032).
/// </summary>
public static class NerdMudVersionSyncTools
{
    private static readonly Regex MudBlazorPackageReference = new(
        @"<PackageReference\s+Include=""MudBlazor""\s+Version=""([^""]+)""",
        RegexOptions.CultureInvariant);

    public static string ReadPinnedVersion() => new NerdDesignTokenOptions().MudBlazorVersion;

    public static string ReadManifestVersion(string designTokensRoot, string? mudVersion = null)
    {
        var version = mudVersion ?? ReadPinnedVersion();
        var manifestPath = Path.Combine(designTokensRoot, "reference", "mudblazor", version, "MANIFEST.json");
        if (!File.Exists(manifestPath))
        {
            throw new FileNotFoundException($"Missing Mud archive manifest: {manifestPath}");
        }

        using var document = JsonDocument.Parse(File.ReadAllText(manifestPath));
        return document.RootElement.GetProperty("mudblazorVersion").GetString()
            ?? throw new InvalidOperationException("mudblazorVersion missing from MANIFEST.json");
    }

    public static string? TryReadCsprojMudBlazorVersion(string csprojPath)
    {
        if (!File.Exists(csprojPath))
        {
            return null;
        }

        var match = MudBlazorPackageReference.Match(File.ReadAllText(csprojPath));
        return match.Success ? match.Groups[1].Value : null;
    }

    public static IReadOnlyList<string> ValidateCsprojMatchesPinnedVersion(string csprojPath, string? expectedVersion = null)
    {
        expectedVersion ??= ReadPinnedVersion();
        var errors = new List<string>();
        var actual = TryReadCsprojMudBlazorVersion(csprojPath);
        if (actual is null)
        {
            errors.Add($"{csprojPath}: MudBlazor PackageReference not found.");
            return errors;
        }

        if (!string.Equals(actual, expectedVersion, StringComparison.Ordinal))
        {
            errors.Add($"{csprojPath}: MudBlazor {actual} does not match pinned version {expectedVersion}.");
        }

        return errors;
    }

    public static string? TryFindTokenStudioHostCsproj()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(
                directory.FullName,
                "src",
                "TheNerdCollective.TokenStudio.Host",
                "TheNerdCollective.TokenStudio.Host.csproj");
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        return null;
    }
}
