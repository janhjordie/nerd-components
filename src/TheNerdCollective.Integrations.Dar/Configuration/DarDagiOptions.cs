using System;

namespace TheNerdCollective.Integrations.Dar.Configuration;

/// <summary>DAGI fallback (DAWA/REST/WFS) når GraphQL endnu ikke returnerer data.</summary>
public sealed class DarDagiOptions
{
    public const string DefaultRestUrl = "https://services.datafordeler.dk/DAGIM/DAGI/1/rest/DAGI";
    public const string DefaultWfsUrl = "https://wfs.datafordeler.dk/DAGI/DAGI_WFS/1.0.0/WFS";
    public const string DefaultDawaBaseUrl = "https://api.dataforsyningen.dk";

    /// <summary>Cache-varighed for kommune-listen (default 24 timer).</summary>
    public TimeSpan KommuneListCacheDuration { get; set; } = TimeSpan.FromHours(24);

    /// <summary>DAWA fallback (gratis, ingen nøgle) når Datafordeler DAGI GraphQL er tom.</summary>
    public bool EnableDawaFallback { get; set; } = true;

    /// <summary>DAWA base-URL (<see href="https://api.dataforsyningen.dk"/>).</summary>
    public string DawaBaseUrl { get; set; } = DefaultDawaBaseUrl;

    /// <summary>REST DAGI multigeometri-punkt (<see href="https://confluence.sdfi.dk/pages/viewpage.action?pageId=13666129"/>).</summary>
    public string RestUrl { get; set; } = DefaultRestUrl;

    /// <summary>WFS til fuld kommune-liste når GraphQL er tom (samme API-nøgle).</summary>
    public string WfsUrl { get; set; } = DefaultWfsUrl;

    /// <summary>
    /// Valgfri tjenestebruger til REST DAGI. Kræves typisk for REST (username/password i URL),
    /// mens GraphQL/WFS bruger API-nøgle.
    /// </summary>
    public string? RestUsername { get; set; }

    /// <summary>Adgangskode til <see cref="RestUsername"/>.</summary>
    public string? RestPassword { get; set; }
}
