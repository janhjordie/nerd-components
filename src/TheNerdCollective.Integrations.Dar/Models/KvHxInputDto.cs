using System.Text.Json.Serialization;

namespace TheNerdCollective.Integrations.Dar.Models;

/// <summary>KVHX-input til downstream-systemer (DAWA-format).</summary>
public sealed record KvHxInputDto
{
    [JsonPropertyName("adressebetegnelse")]
    public required string Adressebetegnelse { get; init; }

    [JsonPropertyName("esrejendomsnr")]
    public required string Esrejendomsnr { get; init; }

    [JsonPropertyName("husnummer")]
    public required string Husnummer { get; init; }

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
}
