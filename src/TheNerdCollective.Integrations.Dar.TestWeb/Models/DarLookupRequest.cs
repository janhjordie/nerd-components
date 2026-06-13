namespace TheNerdCollective.Integrations.Dar.TestWeb.Models;

public sealed class DarLookupRequest
{
    public string StreetAndNumber { get; set; } = "Århusvej 69a";

    public string PostalCode { get; set; } = "3000";

    public string? City { get; set; } = "Helsingør";
}
