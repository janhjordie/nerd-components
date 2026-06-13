using System.Text.Json.Serialization;

namespace TheNerdCollective.Integrations.Dar.Models;

/// <summary>BBR-ejendomsrelation (BFE m.m.).</summary>
public sealed record EjendomsrelationDto : DarEntityDto
{
    [JsonPropertyName("bfeNummer")]
    public long? BfeNummer { get; init; }

    [JsonPropertyName("bygningPaaFremmedGrund")]
    public string? BygningPaaFremmedGrund { get; init; }

    [JsonPropertyName("ejendommensEjerforholdskode")]
    public string? EjendommensEjerforholdskode { get; init; }

    [JsonPropertyName("ejendomsnummer")]
    public string? Ejendomsnummer { get; init; }

    [JsonPropertyName("ejendomstype")]
    public string? Ejendomstype { get; init; }

    [JsonPropertyName("ejerlejlighed")]
    public string? Ejerlejlighed { get; init; }

    [JsonPropertyName("ejerlejlighedsnummer")]
    public string? Ejerlejlighedsnummer { get; init; }

    [JsonPropertyName("samletFastEjendom")]
    public string? SamletFastEjendom { get; init; }

    [JsonPropertyName("tinglystAreal")]
    public decimal? TinglystAreal { get; init; }

    [JsonPropertyName("vurderingsejendomsnummer")]
    public string? Vurderingsejendomsnummer { get; init; }
}
