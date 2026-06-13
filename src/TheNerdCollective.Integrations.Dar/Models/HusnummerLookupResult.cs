namespace TheNerdCollective.Integrations.Dar.Models;

/// <summary>Resultat af DAR-adresseopslag.</summary>
public sealed record HusnummerLookupResult
{
    public required HusnummerDto Husnummer { get; init; }

    public required string Adgangsadresse { get; init; }

    public required string HusnummerId { get; init; }

    /// <summary>BBR-bygnings-ID fra <c>adgangTilBygning</c>, hvis tilgængeligt.</summary>
    public string? BygningId { get; init; }
}
