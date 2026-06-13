using System.Text.Json.Serialization;

namespace TheNerdCollective.Integrations.Dar.Models;

/// <summary>Geometri fra BBR (WKT m.m.).</summary>
public sealed record KoordinatDto
{
    [JsonPropertyName("type")]
    public string? Type { get; init; }

    [JsonPropertyName("crs")]
    public string? Crs { get; init; }

    [JsonPropertyName("dimension")]
    public int? Dimension { get; init; }

    [JsonPropertyName("wkt")]
    public string? Wkt { get; init; }
}
