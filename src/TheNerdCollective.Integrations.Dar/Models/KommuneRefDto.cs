using System.Text.Json.Serialization;

namespace TheNerdCollective.Integrations.Dar.Models;

/// <summary>Kommune-reference til postnummer med flere kommuner.</summary>
public sealed record KommuneRefDto
{
    [JsonPropertyName("kommunekode")]
    public required string Kommunekode { get; init; }

    [JsonPropertyName("navn")]
    public required string Navn { get; init; }
}
