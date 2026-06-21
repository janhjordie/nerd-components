namespace TheNerdCollective.Integrations.Dar.Models;

/// <summary>Resultat af automatisk adresse-match (autocomplete + id-baseret opslag).</summary>
public sealed record DarAddressResolutionResult
{
    /// <summary>Id'er til efterfølgende KvHxInput- og BBR-kald.</summary>
    public required DarAddressIds Ids { get; init; }

    /// <summary>Valgt autocomplete-række.</summary>
    public required DanishAddressAutocompleteResult Selection { get; init; }

    /// <summary>Fuldt DAR-adresseopslag inkl. native DAR og KvHxInput.</summary>
    public required AdresseopslagResult Adresseopslag { get; init; }

    /// <summary>KvHxInput — samme som <see cref="Adresseopslag"/>.<see cref="AdresseopslagResult.KvHxInput"/>.</summary>
    public KvHxInputDto KvHxInput => Adresseopslag.KvHxInput;

    /// <summary>DAR husnummer-id — brug til <c>services.Bbr.Bygning.GetAllByHusnummerIdAsync</c>.</summary>
    public string HusnummerId => Adresseopslag.HusnummerId;

    /// <summary>Enheds-adresse-id når <see cref="Ids"/> er type <c>adresse</c>; ellers null.</summary>
    public string? AdresseLocalId => Adresseopslag.AdresseLocalId;
}
