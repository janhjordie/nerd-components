using System.Text.Json.Serialization;

namespace TheNerdCollective.Integrations.Dar.Models;

/// <summary>Fælles metadatafelter på Datafordeler-registreringer.</summary>
public abstract record DarEntityDto
{
    [JsonPropertyName("id_lokalId")]
    public string? IdLokalId { get; init; }

    [JsonPropertyName("id_namespace")]
    public string? IdNamespace { get; init; }

    [JsonPropertyName("kommunekode")]
    public string? Kommunekode { get; init; }

    [JsonPropertyName("status")]
    public string? Status { get; init; }

    [JsonPropertyName("registreringFra")]
    public string? RegistreringFra { get; init; }

    [JsonPropertyName("registreringTil")]
    public string? RegistreringTil { get; init; }

    [JsonPropertyName("registreringsaktoer")]
    public string? Registreringsaktoer { get; init; }

    [JsonPropertyName("virkningFra")]
    public string? VirkningFra { get; init; }

    [JsonPropertyName("virkningTil")]
    public string? VirkningTil { get; init; }

    [JsonPropertyName("virkningsaktoer")]
    public string? Virkningsaktoer { get; init; }

    [JsonPropertyName("forretningshaendelse")]
    public string? Forretningshaendelse { get; init; }

    [JsonPropertyName("forretningsomraade")]
    public string? Forretningsomraade { get; init; }

    [JsonPropertyName("forretningsproces")]
    public string? Forretningsproces { get; init; }

    [JsonPropertyName("datafordelerOpdateringstid")]
    public string? DatafordelerOpdateringstid { get; init; }

    [JsonPropertyName("datafordelerRegisterImportSequenceNumber")]
    public string? DatafordelerRegisterImportSequenceNumber { get; init; }

    [JsonPropertyName("datafordelerRowId")]
    public string? DatafordelerRowId { get; init; }

    [JsonPropertyName("datafordelerRowVersion")]
    public string? DatafordelerRowVersion { get; init; }
}
