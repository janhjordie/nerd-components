using System.Text.Json.Serialization;

namespace TheNerdCollective.Blazor.ThemeKit;

public sealed class MudThemeIndexDocument
{
    [JsonPropertyName("schemaVersion")]
    public string SchemaVersion { get; set; } = "1.0";

    [JsonPropertyName("defaultThemeId")]
    public string DefaultThemeId { get; set; } = string.Empty;

    [JsonPropertyName("updatedAt")]
    public string? UpdatedAt { get; set; }

    [JsonPropertyName("themes")]
    public List<MudThemeIndexEntry> Themes { get; set; } = [];
}

public sealed class MudThemeIndexEntry
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0.0";

    [JsonPropertyName("updatedAt")]
    public string? UpdatedAt { get; set; }

    [JsonPropertyName("previewPrimaryHex")]
    public string? PreviewPrimaryHex { get; set; }

    [JsonPropertyName("file")]
    public string File { get; set; } = string.Empty;

    [JsonPropertyName("source")]
    public string? Source { get; set; }
}

public sealed class MudThemeJsonDocument
{
    [JsonPropertyName("schemaVersion")]
    public string SchemaVersion { get; set; } = "1.0";

    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0.0";

    [JsonPropertyName("updatedAt")]
    public string? UpdatedAt { get; set; }

    [JsonPropertyName("previewPrimaryHex")]
    public string? PreviewPrimaryHex { get; set; }

    [JsonPropertyName("tokens")]
    public Dictionary<string, string> Tokens { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
