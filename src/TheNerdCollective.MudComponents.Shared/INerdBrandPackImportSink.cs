namespace TheNerdCollective.MudComponents.Shared;

/// <summary>Host hook for importing a token-pack JSON document from the design system hub.</summary>
public interface INerdBrandPackImportSink
{
    Task<NerdBrandPackImportResult> ImportAsync(string json);
}

/// <summary>Outcome of a hub token-pack import.</summary>
public sealed record NerdBrandPackImportResult(bool Success, string Message);
