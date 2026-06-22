using System;

namespace TheNerdCollective.Integrations.Dar.Configuration;

/// <summary>Postnummer-cache og DAWA/REST-fallback til kommune-metadata.</summary>
public sealed class DarPostnummerOptions
{
    public const string DefaultDawaBaseUrl = "https://api.dataforsyningen.dk";
    public const string DefaultRestUrl = "https://services.datafordeler.dk/DAR/DAR/3.0.0/rest";

    /// <summary>Cache-varighed for aktiv postnummer-liste (default 30 dage).</summary>
    public TimeSpan CacheDuration { get; set; } = TimeSpan.FromDays(30);

    /// <summary>Cache-varighed for cirkel-opslag (default 24 timer).</summary>
    public TimeSpan CircleCacheDuration { get; set; } = TimeSpan.FromHours(24);

    /// <summary>DAWA til kommune-opslag og filtrering på kommunekode (gratis, ingen nøgle).</summary>
    public bool EnableDawaEnrichment { get; set; } = true;

    /// <summary>DAWA base-URL (<see href="https://api.dataforsyningen.dk"/>).</summary>
    public string DawaBaseUrl { get; set; } = DefaultDawaBaseUrl;

    /// <summary>DAR REST til husnummer-baseret kommune-opslag når DAWA fejler.</summary>
    public string RestUrl { get; set; } = DefaultRestUrl;

    /// <summary>Maks. parallelle REST-opslag pr. batch.</summary>
    public int MaxParallelKommuneLookups { get; set; } = 12;
}
