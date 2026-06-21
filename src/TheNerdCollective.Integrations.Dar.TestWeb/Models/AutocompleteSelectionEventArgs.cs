using TheNerdCollective.Integrations.Dar.Models;

namespace TheNerdCollective.Integrations.Dar.TestWeb.Models;

public sealed record AutocompleteSelectionEventArgs(
    DanishAddressAutocompleteResult Result,
    string SearchText);
