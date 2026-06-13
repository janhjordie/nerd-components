using System.Text.Json.Serialization;

namespace TheNerdCollective.Integrations.Dar.Models;

/// <summary>BBR-grund.</summary>
public sealed record GrundDto : DarEntityDto
{
    [JsonPropertyName("bestemtFastEjendom")]
    public string? BestemtFastEjendom { get; init; }

    [JsonPropertyName("gru009Vandforsyning")]
    public string? Vandforsyning { get; init; }

    [JsonPropertyName("gru010Afloebsforhold")]
    public string? Afloebsforhold { get; init; }

    [JsonPropertyName("gru500Notatlinjer")]
    public string? Notatlinjer { get; init; }

    [JsonPropertyName("husnummer")]
    public string? Husnummer { get; init; }
}
