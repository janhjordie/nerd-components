namespace TheNerdCollective.Integrations.Dar.Configuration;

/// <summary>
/// Konfiguration til <c>TheNerdCollective:Dar</c>.
/// </summary>
public sealed class DarOptions
{
    public const string SectionName = "TheNerdCollective:Dar";
    public const string DefaultBbrGraphQlUrl = "https://graphql.datafordeler.dk/BBR/v3";
    public const string DefaultDarGraphQlUrl = "https://graphql.datafordeler.dk/DAR/v3";

    /// <summary>Datafordeler API-nøgle til DAR/BBR GraphQL.</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Valgfri override af BBR GraphQL-endpoint.</summary>
    public string BbrGraphQlUrl { get; set; } = DefaultBbrGraphQlUrl;

    /// <summary>Valgfri override af DAR GraphQL-endpoint.</summary>
    public string DarGraphQlUrl { get; set; } = DefaultDarGraphQlUrl;

    /// <summary>DAR adresse-autocomplete (Adressevælger REST).</summary>
    public DarAutocompleteOptions Autocomplete { get; set; } = new();

    /// <summary>
    /// Autocomplete-token (flad binding — svarer til <see cref="Autocomplete"/>.<see cref="DarAutocompleteOptions.Token"/>).
    /// </summary>
    public string AutocompleteToken
    {
        get => Autocomplete.Token;
        set => Autocomplete.Token = value;
    }
}
