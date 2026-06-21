using TheNerdCollective.Integrations.Dar.Models;

namespace TheNerdCollective.Integrations.Dar.TestWeb.Models;

public sealed class DarLookupRequest
{
    public string StreetAndNumber { get; set; } = "Århusvej 69a";

    public string PostalCode { get; set; } = "3000";

    public string? City { get; set; } = "Helsingør";

    /// <summary>Valgt autocomplete-resultat — aktiverer id-baseret opslag.</summary>
    public DanishAddressAutocompleteResult? AutocompleteSelection { get; set; }

    /// <summary>Original søgetekst fra autocomplete (til ResolveBestMatch).</summary>
    public string? AutocompleteSearchText { get; set; }
}
