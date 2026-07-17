namespace TheNerdCollective.MudComponents.Shared;

/// <summary>
/// Hub and catalog route configuration for the Nerd design system packages.
/// </summary>
public sealed class NerdDesignSystemOptions
{
    /// <summary>Enables the <c>/nerd-design-system</c> hub page.</summary>
    public bool EnableHubPage { get; set; } = true;

    /// <summary>Route for the design system hub page.</summary>
    public string HubRoute { get; set; } = "/nerd-design-system";

    /// <summary>Route linked from the hub to the design token catalog.</summary>
    public string DesignTokensRoute { get; set; } = "/nerd-design-tokens";

    /// <summary>Route linked from the hub to the responsive typography catalog.</summary>
    public string TypographyRoute { get; set; } = "/nerd-typography";

    /// <summary>Route linked from the hub to the MudBlazor PlayBook.</summary>
    public string PlayBookRoute { get; set; } = "/nerd-playbook";

    /// <summary>Restricts the hub page to development environments.</summary>
    public bool RestrictHubToDevelopment { get; set; } = true;
}
