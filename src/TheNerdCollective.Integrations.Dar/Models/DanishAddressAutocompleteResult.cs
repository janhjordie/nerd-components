namespace TheNerdCollective.Integrations.Dar.Models;

/// <summary>
/// Resultat fra Adressevælger autocomplete-søgning.
/// </summary>
public sealed record DanishAddressAutocompleteResult(
    string LocalId,
    string DisplayName,
    string AddressLine1,
    string PostalCode,
    string City,
    string AddressLine2 = "",
    bool IsCompleteAddress = true,
    string ResultType = "",
    string? HusnummerId = null);
