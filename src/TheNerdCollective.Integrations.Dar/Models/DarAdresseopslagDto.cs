namespace TheNerdCollective.Integrations.Dar.Models;

/// <summary>
/// Native DAR-resultat fra Datafordeler (DAR_Husnummer + relaterede oplysninger).
/// Brug dette i nye integrationer — ikke <see cref="KvHxInputDto"/>.
/// </summary>
public sealed record DarAdresseopslagDto
{
    /// <summary>DAR_Husnummer-entitet med <c>id_lokalId</c> og Datafordeler-feltnavne.</summary>
    public required HusnummerDto Husnummer { get; init; }

    /// <summary>Resolved vejnavn fra DAR_NavngivenVej, når tilgængeligt.</summary>
    public string? Vejnavn { get; init; }

    /// <summary>DAR_Adresse når opslaget er en enhedsadresse (etage/dør).</summary>
    public AdresseDto? Adresse { get; init; }
}
