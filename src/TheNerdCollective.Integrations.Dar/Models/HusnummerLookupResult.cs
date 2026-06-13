namespace TheNerdCollective.Integrations.Dar.Models;

/// <summary>Resultat af DAR-husnummeropslag uden KVHX/DAWA-mapping.</summary>
public sealed record HusnummerLookupResult
{
    /// <summary>Native DAR-resultat fra Datafordeler — foretræk dette i ny kode.</summary>
    public required DarAdresseopslagDto Dar { get; init; }

    /// <summary>DAR_Husnummer — samme entitet som <see cref="Dar"/>.<see cref="DarAdresseopslagDto.Husnummer"/>.</summary>
    public required HusnummerDto Husnummer { get; init; }

    public required string Adgangsadresse { get; init; }

    /// <summary>DAR_Husnummer.id_lokalId.</summary>
    public required string HusnummerId { get; init; }

    /// <summary>BBR-bygnings-ID fra <c>adgangTilBygning</c>, hvis tilgængeligt.</summary>
    public string? BygningId { get; init; }
}
