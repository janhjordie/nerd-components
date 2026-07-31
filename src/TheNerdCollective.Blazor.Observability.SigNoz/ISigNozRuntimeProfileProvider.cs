namespace TheNerdCollective.Blazor.Observability.SigNoz;

/// <summary>Provides resolved SigNoz query path and schema (config, discovery, or defaults).</summary>
public interface ISigNozRuntimeProfileProvider
{
    SigNozRuntimeProfile? Profile { get; }

    void SetProfile(SigNozRuntimeProfile profile);

    string ResolveQueryRangePath();

    string? ResolveSchemaVersion();
}
