using System.Text.Json.Serialization;

namespace TheNerdCollective.Blazor.ThemeKit;

public sealed class MudThemeSessionCollection
{
    [JsonPropertyName("schemaVersion")]
    public string SchemaVersion { get; set; } = "1.0";

    [JsonPropertyName("sessions")]
    public Dictionary<string, MudThemeSession> Sessions { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public sealed class MudThemeSession
{
    [JsonPropertyName("savedAtUtc")]
    public string SavedAtUtc { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0.0";

    [JsonPropertyName("document")]
    public MudThemeJsonDocument Document { get; set; } = new();
}

public sealed record MudThemeSessionSummary(
    string ThemeId,
    string Version,
    string SavedAtUtc);
