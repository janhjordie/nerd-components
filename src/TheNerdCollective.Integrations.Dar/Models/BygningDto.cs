using System.Text.Json.Serialization;

namespace TheNerdCollective.Integrations.Dar.Models;

/// <summary>BBR-bygning.</summary>
public sealed record BygningDto : DarEntityDto
{
    [JsonPropertyName("byg007Bygningsnummer")]
    public string? Bygningsnummer { get; init; }

    [JsonPropertyName("byg021BygningensAnvendelse")]
    public string? BygningensAnvendelse { get; init; }

    [JsonPropertyName("byg024AntalLejlighederMedKoekken")]
    public int? AntalLejlighederMedKoekken { get; init; }

    [JsonPropertyName("byg025AntalLejlighederUdenKoekken")]
    public int? AntalLejlighederUdenKoekken { get; init; }

    [JsonPropertyName("byg026Opfoerelsesaar")]
    public int? Opfoerelsesaar { get; init; }

    [JsonPropertyName("byg027OmTilbygningsaar")]
    public int? OmTilbygningsaar { get; init; }

    [JsonPropertyName("byg029DatoForMidlertidigOpfoertBygning")]
    public string? DatoForMidlertidigOpfoertBygning { get; init; }

    [JsonPropertyName("byg030Vandforsyning")]
    public string? Vandforsyning { get; init; }

    [JsonPropertyName("byg031Afloebsforhold")]
    public string? Afloebsforhold { get; init; }

    [JsonPropertyName("byg032YdervaeggensMateriale")]
    public string? YdervaeggensMateriale { get; init; }

    [JsonPropertyName("byg033Tagdaekningsmateriale")]
    public string? Tagdaekningsmateriale { get; init; }

    [JsonPropertyName("byg034SupplerendeYdervaeggensMateriale")]
    public string? SupplerendeYdervaeggensMateriale { get; init; }

    [JsonPropertyName("byg035SupplerendeTagdaekningsMateriale")]
    public string? SupplerendeTagdaekningsMateriale { get; init; }

    [JsonPropertyName("byg036AsbestholdigtMateriale")]
    public string? AsbestholdigtMateriale { get; init; }

    [JsonPropertyName("byg037KildeTilBygningensMaterialer")]
    public string? KildeTilBygningensMaterialer { get; init; }

    [JsonPropertyName("byg038SamletBygningsareal")]
    public decimal? SamletBygningsareal { get; init; }

    [JsonPropertyName("byg039BygningensSamledeBoligAreal")]
    public decimal? BygningensSamledeBoligAreal { get; init; }

    [JsonPropertyName("byg040BygningensSamledeErhvervsAreal")]
    public decimal? BygningensSamledeErhvervsAreal { get; init; }

    [JsonPropertyName("byg041BebyggetAreal")]
    public decimal? BebyggetAreal { get; init; }

    [JsonPropertyName("byg042ArealIndbyggetGarage")]
    public decimal? ArealIndbyggetGarage { get; init; }

    [JsonPropertyName("byg043ArealIndbyggetCarport")]
    public decimal? ArealIndbyggetCarport { get; init; }

    [JsonPropertyName("byg044ArealIndbyggetUdhus")]
    public decimal? ArealIndbyggetUdhus { get; init; }

    [JsonPropertyName("byg045ArealIndbyggetUdestueEllerLign")]
    public decimal? ArealIndbyggetUdestueEllerLign { get; init; }

    [JsonPropertyName("byg046SamletArealAfLukkedeOverdaekningerPaaBygningen")]
    public decimal? SamletArealAfLukkedeOverdaekningerPaaBygningen { get; init; }

    [JsonPropertyName("byg047ArealAfAffaldsrumITerraenniveau")]
    public decimal? ArealAfAffaldsrumITerraenniveau { get; init; }

    [JsonPropertyName("byg048AndetAreal")]
    public decimal? AndetAreal { get; init; }

    [JsonPropertyName("byg049ArealAfOverdaekketAreal")]
    public decimal? ArealAfOverdaekketAreal { get; init; }

    [JsonPropertyName("byg050ArealAabneOverdaekningerPaaBygningenSamlet")]
    public decimal? ArealAabneOverdaekningerPaaBygningenSamlet { get; init; }

    [JsonPropertyName("byg051Adgangsareal")]
    public decimal? Adgangsareal { get; init; }

    [JsonPropertyName("byg052BeregningsprincipCarportAreal")]
    public string? BeregningsprincipCarportAreal { get; init; }

    [JsonPropertyName("byg053BygningsarealerKilde")]
    public string? BygningsarealerKilde { get; init; }

    [JsonPropertyName("byg054AntalEtager")]
    public int? AntalEtager { get; init; }

    [JsonPropertyName("byg055AfvigendeEtager")]
    public string? AfvigendeEtager { get; init; }

    [JsonPropertyName("byg056Varmeinstallation")]
    public string? Varmeinstallation { get; init; }

    [JsonPropertyName("byg057Opvarmningsmiddel")]
    public string? Opvarmningsmiddel { get; init; }

    [JsonPropertyName("byg058SupplerendeVarme")]
    public string? SupplerendeVarme { get; init; }

    [JsonPropertyName("byg069Sikringsrumpladser")]
    public int? Sikringsrumpladser { get; init; }

    [JsonPropertyName("byg070Fredning")]
    public string? Fredning { get; init; }

    [JsonPropertyName("byg071BevaringsvaerdighedReference")]
    public string? BevaringsvaerdighedReference { get; init; }

    [JsonPropertyName("byg094Revisionsdato")]
    public string? Revisionsdato { get; init; }

    [JsonPropertyName("byg111StormraadetsOversvoemmelsesSelvrisiko")]
    public string? StormraadetsOversvoemmelsesSelvrisiko { get; init; }

    [JsonPropertyName("byg112DatoForRegistreringFraStormraadet")]
    public string? DatoForRegistreringFraStormraadet { get; init; }

    [JsonPropertyName("byg113Byggeskadeforsikringsselskab")]
    public string? Byggeskadeforsikringsselskab { get; init; }

    [JsonPropertyName("byg114DatoForByggeskadeforsikring")]
    public string? DatoForByggeskadeforsikring { get; init; }

    [JsonPropertyName("byg119Udledningstilladelse")]
    public string? Udledningstilladelse { get; init; }

    [JsonPropertyName("byg121OmfattetAfByggeskadeforsikring")]
    public string? OmfattetAfByggeskadeforsikring { get; init; }

    [JsonPropertyName("byg122Gyldighedsdato")]
    public string? Gyldighedsdato { get; init; }

    [JsonPropertyName("byg123MedlemskabAfSpildevandsforsyning")]
    public string? MedlemskabAfSpildevandsforsyning { get; init; }

    [JsonPropertyName("byg124PaabudVedrSpildevandsafledning")]
    public string? PaabudVedrSpildevandsafledning { get; init; }

    [JsonPropertyName("byg125FristVedrSpildevandsafledning")]
    public string? FristVedrSpildevandsafledning { get; init; }

    [JsonPropertyName("byg126TilladelseTilUdtraeden")]
    public string? TilladelseTilUdtraeden { get; init; }

    [JsonPropertyName("byg127DatoForTilladelseTilUdtraeden")]
    public string? DatoForTilladelseTilUdtraeden { get; init; }

    [JsonPropertyName("byg128TilladelseTilAlternativBortskaffelseEllerAfledning")]
    public string? TilladelseTilAlternativBortskaffelseEllerAfledning { get; init; }

    [JsonPropertyName("byg129DatoForTilladelseTilAlternativBortskaffelseEllerAfledning")]
    public string? DatoForTilladelseTilAlternativBortskaffelseEllerAfledning { get; init; }

    [JsonPropertyName("byg130ArealAfUdvendigEfterisolering")]
    public decimal? ArealAfUdvendigEfterisolering { get; init; }

    [JsonPropertyName("byg131DispensationFritagelseIftKollektivVarmeforsyning")]
    public string? DispensationFritagelseIftKollektivVarmeforsyning { get; init; }

    [JsonPropertyName("byg132DatoForDispensationFritagelseIftKollektivVarmeforsyning")]
    public string? DatoForDispensationFritagelseIftKollektivVarmeforsyning { get; init; }

    [JsonPropertyName("byg133KildeTilKoordinatsaet")]
    public string? KildeTilKoordinatsaet { get; init; }

    [JsonPropertyName("byg134KvalitetAfKoordinatsaet")]
    public string? KvalitetAfKoordinatsaet { get; init; }

    [JsonPropertyName("byg135SupplerendeOplysningOmKoordinatsaet")]
    public string? SupplerendeOplysningOmKoordinatsaet { get; init; }

    [JsonPropertyName("byg136PlaceringPaaSoeterritorie")]
    public string? PlaceringPaaSoeterritorie { get; init; }

    [JsonPropertyName("byg137BanedanmarkBygvaerksnummer")]
    public string? BanedanmarkBygvaerksnummer { get; init; }

    [JsonPropertyName("byg140ServitutForUdlejningsEjendomDato")]
    public string? ServitutForUdlejningsEjendomDato { get; init; }

    [JsonPropertyName("byg150Gulvbelaegning")]
    public string? Gulvbelaegning { get; init; }

    [JsonPropertyName("byg151Frihoejde")]
    public decimal? Frihoejde { get; init; }

    [JsonPropertyName("byg152AabenLukketKonstruktion")]
    public string? AabenLukketKonstruktion { get; init; }

    [JsonPropertyName("byg153Konstruktionsforhold")]
    public string? Konstruktionsforhold { get; init; }

    [JsonPropertyName("byg301TypeAfFlytning")]
    public string? TypeAfFlytning { get; init; }

    [JsonPropertyName("byg302Tilflytterkommune")]
    public string? Tilflytterkommune { get; init; }

    [JsonPropertyName("byg403OevrigeBemaerkningerFraStormraadet")]
    public string? OevrigeBemaerkningerFraStormraadet { get; init; }

    [JsonPropertyName("byg404Koordinat")]
    public KoordinatDto? Koordinat { get; init; }

    [JsonPropertyName("byg406Koordinatsystem")]
    public string? Koordinatsystem { get; init; }

    [JsonPropertyName("byg500Notatlinjer")]
    public string? Notatlinjer { get; init; }

    [JsonPropertyName("ejerlejlighed")]
    public string? Ejerlejlighed { get; init; }

    [JsonPropertyName("grund")]
    public string? Grund { get; init; }

    [JsonPropertyName("husnummer")]
    public string? Husnummer { get; init; }

    [JsonPropertyName("jordstykke")]
    public string? Jordstykke { get; init; }
}
