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

    /// <summary>Route linked from the hub to the design token color swatches catalog.</summary>
    public string DesignTokensRoute { get; set; } = "/nerd-design-tokens";

    /// <summary>Route linked from the hub to the design token recipes catalog.</summary>
    public string DesignTokenRecipesRoute { get; set; } = "/nerd-design-token-recipes";

    /// <summary>Route linked from the hub to the responsive typography catalog.</summary>
    public string TypographyRoute { get; set; } = "/nerd-typography";

    /// <summary>Route linked from the hub to the MudBlazor PlayBook.</summary>
    public string PlayBookRoute { get; set; } = "/nerd-playbook";

    /// <summary>Route for the guided brand workbook wizard.</summary>
    public string BrandWorkbookRoute { get; set; } = "/nerd-brand-workbook";

    /// <summary>Route for the WCAG 2.1 best-practice guide.</summary>
    public string WcagGuideRoute { get; set; } = "/nerd-wcag";

    /// <summary>Enables the WCAG guide page.</summary>
    public bool EnableWcagGuidePage { get; set; } = true;

    /// <summary>Restricts the WCAG guide page to development environments.</summary>
    public bool RestrictWcagGuideToDevelopment { get; set; } = true;

    /// <summary>Restricts the hub page to development environments.</summary>
    public bool RestrictHubToDevelopment { get; set; } = true;

    /// <summary>Number of configured color tokens (set by AddNerdDesignTokens).</summary>
    public int DesignTokenCount { get; set; }

    /// <summary>Number of configured token recipes (set by AddNerdDesignTokens).</summary>
    public int DesignTokenRecipeCount { get; set; }

    /// <summary>Number of configured typography roles (set by AddNerdResponsiveTypography).</summary>
    public int TypographyRoleCount { get; set; }

    /// <summary>Last loaded client token pack id, if any.</summary>
    public string? ActiveTokenPackId { get; set; }

    /// <summary>Active brand identity version for the token pack, if any.</summary>
    public string? ActiveBrandIdentityVersion { get; set; }

    /// <summary>Last loaded client typography pack id, if any.</summary>
    public string? ActiveTypographyPackId { get; set; }

    /// <summary>Formats the active token pack chip label (id plus optional identity version).</summary>
    public string FormatActiveTokenPackLabel()
    {
        if (string.IsNullOrWhiteSpace(ActiveTokenPackId))
        {
            return string.Empty;
        }

        return string.IsNullOrWhiteSpace(ActiveBrandIdentityVersion)
            ? ActiveTokenPackId
            : $"{ActiveTokenPackId} ({ActiveBrandIdentityVersion})";
    }

    /// <summary>CSS class prefix for design tokens (set by AddNerdDesignTokens).</summary>
    public string TokenPrefix { get; set; } = "nerd";
}
