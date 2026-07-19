using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.Brand.Acme;

public static class NerdAcmeDesignTokenPresets
{
    public static void Apply(NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Prefix = "acme";

        options.Add("forest", new NerdColorToken
        {
            Value = "#1F6B3A",
            ContrastText = "#FFFFFF",
            Hover = NerdColorDerivatives.Darken("#1F6B3A", 0.1)
        });
        options.Add("sunrise", new NerdColorToken
        {
            Value = "#F4A236",
            ContrastText = "#1F2937",
            Hover = NerdColorDerivatives.Darken("#F4A236", 0.1)
        });
        options.Add("cloud", new NerdColorToken
        {
            Value = "#F5F7FA",
            ContrastText = "#1F2937",
            Hover = NerdColorDerivatives.Darken("#F5F7FA", 0.05)
        });
        options.Add("ink", new NerdColorToken
        {
            Value = "#111827",
            ContrastText = "#FFFFFF",
            Hover = NerdColorDerivatives.Darken("#111827", 0.08)
        });

        options.Alias("primary-action", "forest");
        options.Alias("page-surface", "cloud");
        options.Alias("brand-chrome", "ink");
        options.Alias("on-brand-chrome", "cloud");
        options.Alias("muted-content", "ink");
        options.Alias("info", "forest");
        options.Alias("highlight", "sunrise");
        options.Alias("danger", "sunrise");
        options.Alias("success", "forest");

        options.AddRecipe("hero", new NerdDesignTokenRecipe("cloud", "ink", "forest"));
    }
}
