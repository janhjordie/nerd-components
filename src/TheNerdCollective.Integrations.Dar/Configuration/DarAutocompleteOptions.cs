namespace TheNerdCollective.Integrations.Dar.Configuration;

/// <summary>
/// DAR adresse-autocomplete via Adressevælger (Klimadatastyrelsens REST-API til fri-tekst søgning i DAR).
/// </summary>
public sealed class DarAutocompleteOptions
{
    public const string DefaultBaseUrl = "https://adressevaelger.dk";

    /// <summary>
    /// Adressevælger-token.
    /// Midlertidigt offentligt demo-token fra Klimadatastyrelsen: <c>adressevaelger123</c>
    /// (se README — rigtig brugerstyring via Datafordeler er endnu ikke tilgængelig).
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Valgfri override af Adressevælger base URL. Standard: <see cref="DefaultBaseUrl"/>.
    /// </summary>
    public string? BaseUrl { get; set; }

    internal string EffectiveBaseUrl =>
        string.IsNullOrWhiteSpace(BaseUrl) ? DefaultBaseUrl : BaseUrl!.TrimEnd('/');
}
