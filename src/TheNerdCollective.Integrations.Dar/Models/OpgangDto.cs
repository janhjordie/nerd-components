using System.Text.Json.Serialization;

namespace TheNerdCollective.Integrations.Dar.Models;

/// <summary>BBR-opgang.</summary>
public sealed record OpgangDto : DarEntityDto
{
    [JsonPropertyName("adgangFraHusnummer")]
    public string? AdgangFraHusnummer { get; init; }

    [JsonPropertyName("bygning")]
    public string? Bygning { get; init; }

    [JsonPropertyName("opg020Elevator")]
    public string? Elevator { get; init; }

    [JsonPropertyName("opg021HusnummerFunktion")]
    public string? HusnummerFunktion { get; init; }

    [JsonPropertyName("opg500Notatlinjer")]
    public string? Notatlinjer { get; init; }
}
