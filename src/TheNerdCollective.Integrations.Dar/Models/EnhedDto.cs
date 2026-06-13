using System.Text.Json.Serialization;

namespace TheNerdCollective.Integrations.Dar.Models;

/// <summary>BBR-enhed (bolig/enhed i bygning).</summary>
public sealed record EnhedDto : DarEntityDto
{
    [JsonPropertyName("adresseIdentificerer")]
    public string? AdresseIdentificerer { get; init; }

    [JsonPropertyName("bygning")]
    public string? Bygning { get; init; }

    [JsonPropertyName("enh008UUIDTilModerlejlighed")]
    public string? UuidTilModerlejlighed { get; init; }

    [JsonPropertyName("enh020EnhedensAnvendelse")]
    public string? EnhedensAnvendelse { get; init; }

    [JsonPropertyName("enh023Boligtype")]
    public string? Boligtype { get; init; }

    [JsonPropertyName("enh024KondemneretBoligenhed")]
    public string? KondemneretBoligenhed { get; init; }

    [JsonPropertyName("enh025OprettelsesdatoForEnhedensIdentifikation")]
    public string? OprettelsesdatoForEnhedensIdentifikation { get; init; }

    [JsonPropertyName("enh026EnhedensSamledeAreal")]
    public decimal? EnhedensSamledeAreal { get; init; }

    [JsonPropertyName("enh027ArealTilBeboelse")]
    public decimal? ArealTilBeboelse { get; init; }

    [JsonPropertyName("enh028ArealTilErhverv")]
    public decimal? ArealTilErhverv { get; init; }

    [JsonPropertyName("enh030KildeTilEnhedensArealer")]
    public string? KildeTilEnhedensArealer { get; init; }

    [JsonPropertyName("enh031AntalVaerelser")]
    public int? AntalVaerelser { get; init; }

    [JsonPropertyName("enh032Toiletforhold")]
    public string? Toiletforhold { get; init; }

    [JsonPropertyName("enh033Badeforhold")]
    public string? Badeforhold { get; init; }

    [JsonPropertyName("enh034Koekkenforhold")]
    public string? Koekkenforhold { get; init; }

    [JsonPropertyName("enh035Energiforsyning")]
    public string? Energiforsyning { get; init; }

    [JsonPropertyName("enh039AndetAreal")]
    public decimal? AndetAreal { get; init; }

    [JsonPropertyName("enh041LovligAnvendelse")]
    public string? LovligAnvendelse { get; init; }

    [JsonPropertyName("enh047IndflytningDato")]
    public string? IndflytningDato { get; init; }

    [JsonPropertyName("enh051Varmeinstallation")]
    public string? Varmeinstallation { get; init; }

    [JsonPropertyName("enh052Opvarmningsmiddel")]
    public string? Opvarmningsmiddel { get; init; }

    [JsonPropertyName("enh053SupplerendeVarme")]
    public string? SupplerendeVarme { get; init; }

    [JsonPropertyName("enh060EnhedensAndelFaellesAdgangsareal")]
    public decimal? EnhedensAndelFaellesAdgangsareal { get; init; }

    [JsonPropertyName("enh127FysiskArealTilBeboelse")]
    public decimal? FysiskArealTilBeboelse { get; init; }

    [JsonPropertyName("enh128FysiskArealTilErhverv")]
    public decimal? FysiskArealTilErhverv { get; init; }

    [JsonPropertyName("enh500Notatlinjer")]
    public string? Notatlinjer { get; init; }

    [JsonPropertyName("etage")]
    public string? Etage { get; init; }

    [JsonPropertyName("opgang")]
    public string? Opgang { get; init; }
}
