namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>
/// Central brand switching for host app bar and catalog pages (HR-172).
/// </summary>
public interface INerdBrandSwitcher
{
    string ActiveBrandId { get; }

    event Action<string>? BrandChanged;

    void SwitchBrand(string brandId);
}
