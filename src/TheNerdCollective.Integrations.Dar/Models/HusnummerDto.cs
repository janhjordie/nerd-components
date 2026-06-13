using System.Text.Json.Serialization;

namespace TheNerdCollective.Integrations.Dar.Models;

/// <summary>Native DAR_Husnummer-entitet fra Datafordeler (foretræk frem for <see cref="KvHxInputDto"/>).</summary>
public sealed record HusnummerDto : DarEntityDto
{
    [JsonPropertyName("adgangsadressebetegnelse")]
    public string? Adgangsadressebetegnelse { get; init; }

    [JsonPropertyName("adgangspunkt")]
    public string? Adgangspunkt { get; init; }

    [JsonPropertyName("adgangTilBygning")]
    public string? AdgangTilBygning { get; init; }

    [JsonPropertyName("adgangTilTekniskAnlaeg")]
    public string? AdgangTilTekniskAnlaeg { get; init; }

    [JsonPropertyName("afstemningsomraade")]
    public string? Afstemningsomraade { get; init; }

    [JsonPropertyName("geoDanmarkBygning")]
    public string? GeoDanmarkBygning { get; init; }

    [JsonPropertyName("husnummerretning")]
    public KoordinatDto? Husnummerretning { get; init; }

    [JsonPropertyName("husnummertekst")]
    public string? Husnummertekst { get; init; }

    [JsonPropertyName("jordstykke")]
    public string? Jordstykke { get; init; }

    [JsonPropertyName("kommuneinddeling")]
    public string? Kommuneinddeling { get; init; }

    [JsonPropertyName("menighedsraadsafstemningsomraade")]
    public string? Menighedsraadsafstemningsomraade { get; init; }

    [JsonPropertyName("navngivenVej")]
    public string? NavngivenVej { get; init; }

    [JsonPropertyName("placeretPaaForeloebigtJordstykke")]
    public string? PlaceretPaaForeloebigtJordstykke { get; init; }

    [JsonPropertyName("postnummer")]
    public string? Postnummer { get; init; }

    [JsonPropertyName("sogneinddeling")]
    public string? Sogneinddeling { get; init; }

    [JsonPropertyName("supplerendeBynavn")]
    public string? SupplerendeBynavn { get; init; }

    [JsonPropertyName("vejmidte")]
    public string? Vejmidte { get; init; }

    [JsonPropertyName("vejpunkt")]
    public string? Vejpunkt { get; init; }
}
