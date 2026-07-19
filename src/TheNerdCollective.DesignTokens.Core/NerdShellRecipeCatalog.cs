using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>Required shell recipe names for parity and manual compliance (HR-114 / HR-152).</summary>
public static class NerdShellRecipeCatalog
{
    public static readonly string[] CoreShellRecipes =
    [
        NerdDesignSystemUi.SidebarRecipe,
        "hero"
    ];

    public static readonly string[] ExtendedShellRecipes =
    [
        NerdDesignSystemUi.SidebarRecipe,
        "hero",
        "hero-photo",
        "footer",
        "footer-minimal",
        "formular"
    ];
}
