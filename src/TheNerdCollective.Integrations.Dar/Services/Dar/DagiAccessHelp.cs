namespace TheNerdCollective.Integrations.Dar.Services.Dar;

internal static class DagiAccessHelp
{
    internal const string GraphQlDocUrl =
        "https://confluence.sdfi.dk/pages/viewpage.action?pageId=199984259";

    internal const string DawaDocUrl =
        "https://dawadocs.dataforsyningen.dk/dok/api/kommune";

    internal const string RestDocUrl =
        "https://confluence.sdfi.dk/pages/viewpage.action?pageId=13666129";

    internal const string EmptyKommuneResultMessage =
        "Ingen kommuner returneret. Forsøgte Datafordeler DAGI GraphQL, DAWA (api.dataforsyningen.dk/kommuner) og WFS. " +
        "DAWA kræver ingen API-nøgle — tjek at EnableDawaFallback er true (default).";

    internal const string EmptyRegionResultMessage =
        "Ingen regioner returneret. Forsøgte Datafordeler DAGI GraphQL, DAWA (api.dataforsyningen.dk/regioner) og WFS. " +
        "DAWA kræver ingen API-nøgle — tjek at EnableDawaFallback er true (default).";

    internal const string PointLookupFailedMessage =
        "Ingen kommune fundet for koordinaterne. Forsøgte Datafordeler DAGI GraphQL, DAWA reverse geocoding og REST DAGI.";
}
