namespace TheNerdCollective.Integrations.Dar.Models;

/// <summary>Resultat af DAR-adresseopslag.</summary>
public sealed record AdresseopslagResult
{
    /// <summary>
    /// Native DAR-resultat fra Datafordeler — foretræk dette i ny kode (<c>id_lokalId</c>, DAR-feltnavne).
    /// </summary>
    public required DarAdresseopslagDto Dar { get; init; }

    /// <summary>DAR_Husnummer — samme entitet som <see cref="Dar"/>.<see cref="DarAdresseopslagDto.Husnummer"/>.</summary>
    public required HusnummerDto Husnummer { get; init; }

    public required string Adgangsadresse { get; init; }

    /// <summary>DAR_Husnummer.id_lokalId — samme værdi som <see cref="Dar"/>.<see cref="DarAdresseopslagDto.Husnummer"/>.<see cref="DarEntityDto.IdLokalId"/>.</summary>
    public required string HusnummerId { get; init; }

    /// <summary>BBR-bygnings-ID fra <c>adgangTilBygning</c>, hvis tilgængeligt.</summary>
    public string? BygningId { get; init; }

    /// <summary>
    /// DAR adresse-id når opslaget stammer fra autocomplete med type <c>adresse</c> (enhed).
    /// Null for adgangsadresse/husnummer-opslag.
    /// </summary>
    public string? AdresseLocalId { get; init; }

    public required string StreetAndNumber { get; init; }

    public required string PostalCode { get; init; }

    public string? City { get; init; }

    /// <summary>
    /// KVHX-input i DAWA-format — kun til bagudkompatibilitet med legacy downstream-systemer.
    /// Foretræk <see cref="Dar"/> i ny kode; property kan fjernes når DAWA er helt udfaset.
    /// </summary>
    public required KvHxInputDto KvHxInput { get; init; }
}
