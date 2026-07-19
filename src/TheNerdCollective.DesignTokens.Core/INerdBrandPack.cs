namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>
/// A installable brand preset (colors, aliases, recipes) for <see cref="NerdDesignTokenOptions"/>.
/// </summary>
public interface INerdBrandPack
{
    string Id { get; }

    string DisplayName { get; }

    /// <summary>Brand identity guide revision (e.g. DNF "2025.1"), independent of NuGet package version.</summary>
    string IdentityVersion { get; }

    INerdPairingPolicy? PairingPolicy { get; }

    void Configure(NerdDesignTokenOptions options);
}
