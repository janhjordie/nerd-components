using System.Text.Json.Serialization;

namespace TheNerdCollective.Integrations.Dar.Models;

/// <summary>BBR-teknisk anlæg (fx olietank, varmepumpe).</summary>
public sealed record TekniskAnlaegDto : DarEntityDto
{
    [JsonPropertyName("bygning")]
    public string? Bygning { get; init; }

    [JsonPropertyName("bygningPaaFremmedGrund")]
    public string? BygningPaaFremmedGrund { get; init; }

    [JsonPropertyName("ejerlejlighed")]
    public string? Ejerlejlighed { get; init; }

    [JsonPropertyName("enhed")]
    public string? Enhed { get; init; }

    [JsonPropertyName("grund")]
    public string? Grund { get; init; }

    [JsonPropertyName("husnummer")]
    public string? Husnummer { get; init; }

    [JsonPropertyName("jordstykke")]
    public string? Jordstykke { get; init; }

    [JsonPropertyName("tek007Anlaegsnummer")]
    public string? Anlaegsnummer { get; init; }

    [JsonPropertyName("tek020Klassifikation")]
    public string? Klassifikation { get; init; }

    [JsonPropertyName("tek021FabrikatType")]
    public string? FabrikatType { get; init; }

    [JsonPropertyName("tek024Etableringsaar")]
    public int? Etableringsaar { get; init; }

    [JsonPropertyName("tek025TilOmbygningsaar")]
    public int? TilOmbygningsaar { get; init; }

    [JsonPropertyName("tek026StoerrelsesklasseOlietank")]
    public string? StoerrelsesklasseOlietank { get; init; }

    [JsonPropertyName("tek027Placering")]
    public string? Placering { get; init; }

    [JsonPropertyName("tek032Stoerrelse")]
    public decimal? Stoerrelse { get; init; }

    [JsonPropertyName("tek033Type")]
    public string? Type { get; init; }

    [JsonPropertyName("tek037Areal")]
    public decimal? Areal { get; init; }

    [JsonPropertyName("tek039Effekt")]
    public decimal? Effekt { get; init; }

    [JsonPropertyName("tek109Koordinat")]
    public KoordinatDto? Koordinat { get; init; }

    [JsonPropertyName("tek110Driftstatus")]
    public string? Driftstatus { get; init; }

    [JsonPropertyName("tek500Notatlinjer")]
    public string? Notatlinjer { get; init; }
}
