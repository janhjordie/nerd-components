namespace TheNerdCollective.Integrations.Dar.Models;

/// <summary>Aktivt DAR-postnummer (status 3).</summary>
public record PostnummerDto
{
    /// <summary>Firecifret postnummer, fx "2730".</summary>
    public string Postnummer { get; init; } = string.Empty;

    /// <summary>Postdistriktsnavn, fx "Herlev".</summary>
    public string Postdistrikt { get; init; } = string.Empty;
}
