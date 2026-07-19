namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>Shell layout blocks for Stitch / DESIGN.md composition export (HR-150).</summary>
public static class NerdLayoutCompositionCatalog
{
    private static readonly (string Recipe, string Title, string Description, string? Overlay)[] Catalog =
    [
        ("hero-photo", "Hero with photo", "Full-width hero with dark overlay, headline and primary CTA.", "hero-overlay"),
        ("hero-organic", "Hero organic", "Dark forest hero with organic watermark and CTA.", "hero-overlay"),
        ("hero-light", "Hero light", "Light hero band with headline and secondary link.", null),
        ("feature-panel", "Feature panel", "Stacked feature rows with dividers.", null),
        ("partner-row", "Partner row", "Logo / partner strip centered below a label.", null),
        ("footer-minimal", "Footer minimal", "Compact footer with legal links.", null),
        ("formular", "Form strip", "Single-column form with input-surface fields and primary CTA.", null)
    ];

    public static IReadOnlyList<NerdLayoutCompositionBlock> Describe(NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var blocks = new List<NerdLayoutCompositionBlock>();

        foreach (var (recipe, title, description, overlay) in Catalog)
        {
            if (!options.Recipes.ContainsKey(recipe))
            {
                continue;
            }

            var overlayClass = overlay is not null && options.Opacities.ContainsKey(overlay)
                ? $"{options.Prefix}-opacity-{overlay}"
                : null;

            blocks.Add(new NerdLayoutCompositionBlock(recipe, title, description, overlayClass));
        }

        return blocks;
    }
}

public sealed record NerdLayoutCompositionBlock(
    string RecipeName,
    string Title,
    string Description,
    string? OverlayClass);
