using System;
using System.Text.Json.Serialization;

namespace TheNerdCollective.Integrations.Dar.Models;

/// <summary>BBR-etage (inkl. kælder).</summary>
public sealed record EtageDto : DarEntityDto
{
    [JsonPropertyName("bygning")]
    public string? Bygning { get; init; }

    [JsonPropertyName("eta006BygningensEtagebetegnelse")]
    public string? BygningensEtagebetegnelse { get; init; }

    [JsonPropertyName("eta020SamletArealAfEtage")]
    public decimal? SamletArealAfEtage { get; init; }

    [JsonPropertyName("eta021ArealAfUdnyttetDelAfTagetage")]
    public decimal? ArealAfUdnyttetDelAfTagetage { get; init; }

    [JsonPropertyName("eta022Kaelderareal")]
    public decimal? Kaelderareal { get; init; }

    [JsonPropertyName("eta023ArealAfLovligBeboelseIKaelder")]
    public decimal? ArealAfLovligBeboelseIKaelder { get; init; }

    [JsonPropertyName("eta024EtagensAdgangsareal")]
    public decimal? EtagensAdgangsareal { get; init; }

    [JsonPropertyName("eta025Etagetype")]
    public string? Etagetype { get; init; }

    [JsonPropertyName("eta026ErhvervIKaelder")]
    public decimal? ErhvervIKaelder { get; init; }

    [JsonPropertyName("eta500Notatlinjer")]
    public string? Notatlinjer { get; init; }

    /// <summary>True hvis etagen er kælder (etagetype 1/2 eller betegnelse starter med "kl").</summary>
    public bool IsKaelder =>
        Etagetype == "1" || Etagetype == "2"
        || (BygningensEtagebetegnelse != null
            && BygningensEtagebetegnelse.StartsWith("kl", StringComparison.OrdinalIgnoreCase));
}
