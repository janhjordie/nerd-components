using TheNerdCollective.Blazor.Observability;
using Microsoft.Extensions.Options;

namespace TheNerdCollective.Blazor.Observability.SigNoz;

/// <summary>Resolves query path and schema from discovery profile or static options.</summary>
public sealed class SigNozRuntimeProfileProvider(
    IOptions<SigNozBackendOptions> options) : ISigNozRuntimeProfileProvider
{
    private SigNozRuntimeProfile? _profile;

    public SigNozRuntimeProfile? Profile => _profile;

    public void SetProfile(SigNozRuntimeProfile profile) => _profile = profile;

    public string ResolveQueryRangePath() =>
        _profile?.QueryRangePath
        ?? options.Value.QueryRangePath
        ?? SigNozBackendOptions.DefaultQueryRangePath;

    public string? ResolveSchemaVersion() =>
        _profile is not null
            ? _profile.RecommendedSchemaVersion
            : options.Value.SchemaVersion;
}
