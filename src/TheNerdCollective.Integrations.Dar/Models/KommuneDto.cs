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

    /// <summary>Regionskode (fire cifre, fx <c>1084</c> for Region Hovedstaden).</summary>
    [JsonPropertyName("regionskode")]
    public string? Regionskode { get; init; }

    /// <summary>Regionnavn (fx <c>Region Hovedstaden</c>).</summary>
    [JsonPropertyName("regionnavn")]
    public string? Regionnavn { get; init; }

    /// <summary>Repræsentativt punkt (WGS84 længdegrad) fra DAGI-geometri.</summary>
    [JsonPropertyName("repraesentativPunktLongitude")]
    public double? RepræsentativPunktLongitude { get; init; }

    /// <summary>Repræsentativt punkt (WGS84 breddegrad) fra DAGI-geometri.</summary>
    [JsonPropertyName("repraesentativPunktLatitude")]
    public double? RepræsentativPunktLatitude { get; init; }
}
