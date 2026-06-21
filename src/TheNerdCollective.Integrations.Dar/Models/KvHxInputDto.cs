using System.Text.Json.Serialization;

namespace TheNerdCollective.Integrations.Dar.Models;

/// <summary>
/// KVHX-input i DAWA-format — kun til bagudkompatibilitet med legacy downstream-systemer.
/// Foretræk <see cref="DarAdresseopslagDto"/> / <see cref="HusnummerDto"/> i ny kode.
/// </summary>
public sealed record KvHxInputDto
{
    [JsonPropertyName("adressebetegnelse")]
    public required string Adressebetegnelse { get; init; }

    [JsonPropertyName("esrejendomsnr")]
    public required string Esrejendomsnr { get; init; }

    [JsonPropertyName("husnummer")]
    public required string Husnummer { get; init; }

    /// <summary>DAWA <c>id</c> — svarer til DAR <c>id_lokalId</c> (<see cref="DarEntityDto.IdLokalId"/>).</summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>Kommunekode — property-navnet følger eksisterende downstream-kontrakt.</summary>
    [JsonPropertyName("komunekode")]
    public required string Komunekode { get; init; }

    [JsonPropertyName("kvhxId")]
    public required string KvhxId { get; init; }

    [JsonPropertyName("postnummer")]
    public required string Postnummer { get; init; }

    [JsonPropertyName("vejkode")]
    public required string Vejkode { get; init; }

    [JsonPropertyName("vejnavn")]
    public required string Vejnavn { get; init; }

    /// <summary>Etagebetegnelse — DAWA <c>etage</c> / DAR <c>etagebetegnelse</c>.</summary>
    [JsonPropertyName("etage")]
    public string Etage { get; init; } = string.Empty;

    /// <summary>Dørbetegnelse (sidebetegnelse) — DAWA <c>dør</c> / DAR <c>doerbetegnelse</c>.</summary>
    [JsonPropertyName("dør")]
    public string Door { get; init; } = string.Empty;
}
