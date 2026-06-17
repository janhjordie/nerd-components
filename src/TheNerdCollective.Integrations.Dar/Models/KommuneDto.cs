using System.Text.Json.Serialization;

namespace TheNerdCollective.Integrations.Dar.Models;

/// <summary>Kommune fra DAGI (Datafordeler).</summary>
public sealed record KommuneDto
{
    [JsonPropertyName("id_lokalId")]
    public string? IdLokalId { get; init; }

    [JsonPropertyName("navn")]
    public string? Navn { get; init; }

    [JsonPropertyName("kommunekode")]
    public string? Kommunekode { get; init; }
}
