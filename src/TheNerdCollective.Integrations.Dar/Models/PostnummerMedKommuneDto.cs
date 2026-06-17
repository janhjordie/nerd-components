namespace TheNerdCollective.Integrations.Dar.Models;

/// <summary>Postnummer med primær tilknyttet kommune.</summary>
public sealed record PostnummerMedKommuneDto : PostnummerDto
{
    /// <summary>Kommunekode (4 cifre), fx "0163".</summary>
    public string? Kommunekode { get; init; }

    /// <summary>Kommunenavn, fx "Herlev".</summary>
    public string? Kommunenavn { get; init; }
}
