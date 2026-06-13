using System.Text.Json.Serialization;

namespace TheNerdCollective.Integrations.Dar.Models;

/// <summary>Kobling mellem grund og jordstykke.</summary>
public sealed record GrundJordstykkeDto : DarEntityDto
{
    [JsonPropertyName("grund")]
    public string? Grund { get; init; }

    [JsonPropertyName("jordstykke")]
    public string? Jordstykke { get; init; }
}
