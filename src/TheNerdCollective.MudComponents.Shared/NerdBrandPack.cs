using System.Text.Json;

namespace TheNerdCollective.MudComponents.Shared;

/// <summary>
/// Combined design-token and typography pack for white-label brand bundles.
/// </summary>
public sealed class NerdBrandPack
{
    public string ClientId { get; init; } = "default";
    public int Version { get; init; } = 1;
    public DateTimeOffset UpdatedAt { get; init; } = DateTimeOffset.UtcNow;
    public JsonElement? DesignTokens { get; init; }
    public JsonElement? Typography { get; init; }
}
