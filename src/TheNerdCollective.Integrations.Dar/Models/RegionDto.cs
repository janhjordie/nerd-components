using System.Text.Json.Serialization;

namespace TheNerdCollective.Integrations.Dar.Models;

/// <summary>Region fra DAGI (Datafordeler).</summary>
public sealed record RegionDto
{
    [JsonPropertyName("id_lokalId")]
    public string? IdLokalId { get; init; }

    [JsonPropertyName("navn")]
    public string? Regionnavn { get; init; }

    [JsonPropertyName("regionskode")]
    public string? Regionskode { get; init; }
}
