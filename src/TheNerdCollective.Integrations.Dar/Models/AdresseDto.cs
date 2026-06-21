using System.Text.Json.Serialization;

namespace TheNerdCollective.Integrations.Dar.Models;

/// <summary>Native DAR_Adresse-entitet (enhedsadresse med etage og dørbetegnelse).</summary>
public sealed record AdresseDto : DarEntityDto
{
    [JsonPropertyName("adressebetegnelse")]
    public string? Adressebetegnelse { get; init; }

    [JsonPropertyName("etagebetegnelse")]
    public string? Etagebetegnelse { get; init; }

    [JsonPropertyName("doerbetegnelse")]
    public string? Doerbetegnelse { get; init; }

    [JsonPropertyName("husnummer")]
    public string? Husnummer { get; init; }
}
