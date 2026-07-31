using Microsoft.Extensions.DependencyInjection;

namespace TheNerdCollective.Blazor.Observability.SigNoz;

/// <summary>Fluent builder for SigNoz observability extension points.</summary>
public sealed class SigNozObservabilityBuilder
{
    private readonly IServiceCollection _services;
    private readonly List<Type> _parserTypes = [];
    private readonly List<Type> _mutatorTypes = [];
    private bool _enableDiscovery;

    internal SigNozObservabilityBuilder(IServiceCollection services) => _services = services;

    /// <summary>Registers a custom response parser (runs before built-in parsers).</summary>
    public SigNozObservabilityBuilder AddResponseParser<TParser>()
        where TParser : class, ISigNozResponseParser
    {
        _parserTypes.Add(typeof(TParser));
        return this;
    }

    /// <summary>Registers a query_range body mutator.</summary>
    public SigNozObservabilityBuilder AddQueryMutator<TMutator>()
        where TMutator : class, ISigNozQueryMutator
    {
        _mutatorTypes.Add(typeof(TMutator));
        return this;
    }

    /// <summary>Enables startup capability discovery (path + schema).</summary>
    public SigNozObservabilityBuilder EnableCapabilityDiscovery(bool enabled = true)
    {
        _enableDiscovery = enabled;
        return this;
    }

    internal void Apply()
    {
        foreach (var parserType in _parserTypes)
        {
            _services.AddSingleton(typeof(ISigNozResponseParser), parserType);
        }

        foreach (var mutatorType in _mutatorTypes)
        {
            _services.AddSingleton(typeof(ISigNozQueryMutator), mutatorType);
        }

        if (_enableDiscovery)
        {
            _services.PostConfigure<SigNozBackendOptions>(o => o.DiscoverOnStartup = true);
        }
    }
}
