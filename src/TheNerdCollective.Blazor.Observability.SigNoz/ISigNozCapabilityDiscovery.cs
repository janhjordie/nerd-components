namespace TheNerdCollective.Blazor.Observability.SigNoz;

/// <summary>Probes live SigNoz to discover working query path and schema version.</summary>
public interface ISigNozCapabilityDiscovery
{
    Task<SigNozRuntimeProfile> DiscoverAsync(CancellationToken cancellationToken = default);
}
