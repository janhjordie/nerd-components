using System.Text.Json.Serialization;
using TheNerdCollective.Integrations.Dar.Models;

namespace TheNerdCollective.Integrations.Dar.Services.Dar.Internal;

/// <summary>Internt GraphQL-resultat for <c>DAGI_Kommuneinddeling</c>.</summary>
internal sealed class KommuneGraphDto
{
    [JsonPropertyName("id_lokalId")]
    public string? IdLokalId { get; init; }

    [JsonPropertyName("navn")]
    public string? Navn { get; init; }

    [JsonPropertyName("kommunekode")]
    public string? Kommunekode { get; init; }

    [JsonPropertyName("regionLokalid")]
    public string? RegionLokalid { get; init; }

    [JsonPropertyName("geometri")]
    public KoordinatDto? Geometri { get; init; }
}
