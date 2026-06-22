using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TheNerdCollective.Integrations.Dar.Models;

/// <summary>Aktivt postnummer med alle tilknyttede kommuner (multi-kommune).</summary>
public sealed record PostnummerMedKommunerDto
{
    [JsonPropertyName("postnummer")]
    public required string Postnummer { get; init; }

    [JsonPropertyName("postdistrikt")]
    public required string Postdistrikt { get; init; }

    [JsonPropertyName("kommuner")]
    public IReadOnlyList<KommuneRefDto> Kommuner { get; init; } = Array.Empty<KommuneRefDto>();
}
