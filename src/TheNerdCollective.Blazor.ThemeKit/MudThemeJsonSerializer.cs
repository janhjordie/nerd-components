using System.Text.Json;
using MudBlazor;

namespace TheNerdCollective.Blazor.ThemeKit;

public static class MudThemeJsonSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
    };

    public static MudThemeIndexDocument LoadIndex(string themesDirectory)
    {
        var indexPath = Path.Combine(themesDirectory, "themes.index.json");
        if (!File.Exists(indexPath))
        {
            throw new FileNotFoundException($"Theme index not found: {indexPath}", indexPath);
        }

        var json = File.ReadAllText(indexPath);
        return JsonSerializer.Deserialize<MudThemeIndexDocument>(json, JsonOptions)
               ?? throw new InvalidOperationException($"Failed to deserialize theme index: {indexPath}");
    }

    public static MudThemeJsonDocument LoadThemeFile(string themesDirectory, string fileName)
    {
        var themePath = Path.Combine(themesDirectory, fileName);
        if (!File.Exists(themePath))
        {
            throw new FileNotFoundException($"Theme file not found: {themePath}", themePath);
        }

        var json = File.ReadAllText(themePath);
        return JsonSerializer.Deserialize<MudThemeJsonDocument>(json, JsonOptions)
               ?? throw new InvalidOperationException($"Failed to deserialize theme file: {themePath}");
    }

    public static MudTheme ApplyDocument(MudTheme baseTheme, MudThemeJsonDocument document)
    {
        var theme = MudThemeCloner.Clone(baseTheme);
        ApplyTokens(theme, document.Tokens);
        return theme;
    }

    public static void ApplyTokens(MudTheme theme, IReadOnlyDictionary<string, string> tokens)
    {
        foreach (var (tokenId, value) in tokens)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            var token = ThemeTokenRegistry.V1.FirstOrDefault(t => t.Id.Equals(tokenId, StringComparison.OrdinalIgnoreCase));
            token?.SetValue(theme, value);
        }
    }

    public static Dictionary<string, string> ExtractTokens(MudTheme theme)
    {
        var tokens = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var token in ThemeTokenRegistry.V1)
        {
            var value = token.GetValue(theme);
            if (!string.IsNullOrWhiteSpace(value))
            {
                tokens[token.Id] = value;
            }
        }

        return tokens;
    }

    public static MudThemeJsonDocument CreateDocument(
        MudTheme theme,
        string id,
        string version,
        string? displayName = null,
        string? updatedAt = null,
        string? previewPrimaryHex = null)
    {
        return new MudThemeJsonDocument
        {
            SchemaVersion = "1.0",
            Id = id,
            DisplayName = displayName,
            Version = version,
            UpdatedAt = updatedAt ?? DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd"),
            PreviewPrimaryHex = previewPrimaryHex ?? ExtractTokens(theme).GetValueOrDefault("light.primary"),
            Tokens = ExtractTokens(theme),
        };
    }

    public static string WriteThemeDocument(MudThemeJsonDocument document)
        => JsonSerializer.Serialize(document, JsonOptions);

    public static string WriteIndexDocument(MudThemeIndexDocument document)
        => JsonSerializer.Serialize(document, JsonOptions);

    public static string ExportThemeJson(
        MudTheme theme,
        string id,
        string version,
        string? displayName = null,
        string? updatedAt = null)
        => WriteThemeDocument(CreateDocument(theme, id, version, displayName, updatedAt));
}
