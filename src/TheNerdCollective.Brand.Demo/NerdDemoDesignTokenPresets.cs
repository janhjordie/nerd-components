using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.Brand.Demo;

public static class NerdDemoDesignTokenPresets
{
    public static void Apply(NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Prefix = "demo";

        options.Add("violet", new NerdColorToken
        {
            Value = "#6D28D9",
            ContrastText = "#FFFFFF",
            Hover = NerdColorDerivatives.Darken("#6D28D9", 0.1)
        });
        options.Add("sky", new NerdColorToken
        {
            Value = "#38BDF8",
            ContrastText = "#0F172A",
            Hover = NerdColorDerivatives.Darken("#38BDF8", 0.1)
        });
        options.Add("paper", new NerdColorToken
        {
            Value = "#F8FAFC",
            ContrastText = "#0F172A",
            Hover = NerdColorDerivatives.Darken("#F8FAFC", 0.05)
        });
        options.Add("slate", new NerdColorToken
        {
            Value = "#334155",
            ContrastText = "#FFFFFF",
            Hover = NerdColorDerivatives.Darken("#334155", 0.08)
        });

        options.Alias("primary-action", "violet");
        options.Alias("page-surface", "paper");
        options.Alias("brand-chrome", "slate");
        options.Alias("on-brand-chrome", "paper");
        options.Alias("muted-content", "slate");
        options.Alias("info", "sky");
        options.Alias("highlight", "sky");
        options.Alias("accent", "sky");
        options.Alias("danger", "violet");
        options.Alias("success", "sky");

        options.AddRecipe("cta-strip", new NerdDesignTokenRecipe("slate", "paper", "sky"));
    }
}
