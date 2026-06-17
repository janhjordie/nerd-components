namespace TheNerdCollective.Integrations.Dar.Models;

/// <summary>
/// Udvidede adresseoplysninger fra Adressevælger id-opslag (efter fonetisk søgning).
/// </summary>
public sealed record DanishAddressDetailResult
{
    /// <summary>DAR husnummer <c>id_lokalId</c> — brug videre i Datafordeler/BBR.</summary>
    public string HusnummerId { get; init; } = string.Empty;

    /// <summary>DAR adresse <c>id_lokalId</c> når opslaget startede fra type <c>adresse</c>.</summary>
    public string? AdresseId { get; init; }

    /// <summary>Adgangsadressebetegnelse eller adressebetegnelse fra Adressevælger.</summary>
    public string Betegnelse { get; init; } = string.Empty;

    public string Vejnavn { get; init; } = string.Empty;

    public string Husnummer { get; init; } = string.Empty;

    public string Postnummer { get; init; } = string.Empty;

    public string Postdistrikt { get; init; } = string.Empty;

    public string? Etagebetegnelse { get; init; }

    public string? Doerbetegnelse { get; init; }

    public string? Kommunekode { get; init; }

    /// <summary>Østkoordinat (ETRS89 / EPSG:25832).</summary>
    public double Easting { get; init; }

    /// <summary>Nordkoordinat (ETRS89 / EPSG:25832).</summary>
    public double Northing { get; init; }

    /// <summary>Breddegrad (WGS84 / EPSG:4326) — beregnet fra <see cref="Easting"/>/<see cref="Northing"/>.</summary>
    public double Latitude { get; init; }

    /// <summary>Længdegrad (WGS84 / EPSG:4326) — beregnet fra <see cref="Easting"/>/<see cref="Northing"/>.</summary>
    public double Longitude { get; init; }

    /// <summary>Koordinatsystem for <see cref="Easting"/> og <see cref="Northing"/>.</summary>
    public string CoordinateSystem { get; init; } = "EPSG:25832";
}
