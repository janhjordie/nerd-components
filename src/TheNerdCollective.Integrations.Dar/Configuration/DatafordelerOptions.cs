namespace TheNerdCollective.Integrations.Dar.Configuration;

public sealed class DatafordelerOptions
{
    public const string SectionName = "TheNerdCollective:Dar";

    public string ApiKey { get; set; } = string.Empty;
    public string BbrGraphQlUrl { get; set; } = "https://graphql.datafordeler.dk/BBR/v3";
    public string DarGraphQlUrl { get; set; } = "https://graphql.datafordeler.dk/DAR/v3";

    public AdressevaelgerOptions Adressevaelger { get; set; } = new();
}
