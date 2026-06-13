namespace TheNerdCollective.Integrations.Dar.Models;

/// <summary>Resultat af DAR-adresseopslag.</summary>
public sealed record AdresseopslagResult
{
    public required HusnummerDto Husnummer { get; init; }

    public required string Adgangsadresse { get; init; }

    public required string HusnummerId { get; init; }

    /// <summary>BBR-bygnings-ID fra <c>adgangTilBygning</c>, hvis tilgængeligt.</summary>
    public string? BygningId { get; init; }

    public required string StreetAndNumber { get; init; }

    public required string PostalCode { get; init; }

    public string? City { get; init; }

    /// <summary>KVHX-input i DAWA-format til downstream-integration.</summary>
    public required KvHxInputDto KvHxInput { get; init; }
}
