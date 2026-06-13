namespace TheNerdCollective.Integrations.Dar.Configuration;

/// <summary>
/// Konfiguration til Adressevælger (Klimadatastyrelsen) — bruges til fri-tekst adresse-autocomplete.
/// </summary>
public sealed class AdressevaelgerOptions
{
    public string BaseUrl { get; set; } = "https://adressevaelger.dk";

    /// <summary>
    /// Adressevælger API-token.
    /// Midlertidigt offentligt demo-token fra Klimadatastyrelsen: <c>adressevaelger123</c>
    /// (se README — rigtig brugerstyring via Datafordeler er endnu ikke tilgængelig).
    /// </summary>
    public string Token { get; set; } = string.Empty;
}
