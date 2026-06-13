using System.Text.Json.Serialization;

namespace TheNerdCollective.Integrations.Dar.Models;

/// <summary>Relation mellem bygning og ejendom.</summary>
public sealed record BygningEjendomsrelationDto : DarEntityDto
{
    [JsonPropertyName("bygning")]
    public string? Bygning { get; init; }

    [JsonPropertyName("bygningPaaFremmedGrund")]
    public string? BygningPaaFremmedGrund { get; init; }
}
