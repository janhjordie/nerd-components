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
        options.Alias("secondary-action", "violet");
        options.Alias("on-primary-action", "paper");
        options.Alias("page-surface", "paper");
        options.Alias("brand-chrome", "slate");
        options.Alias("on-brand-chrome", "paper");
        options.Alias("nav-surface", "paper");
        options.Alias("nav-item", "slate");
        options.Alias("nav-item-active", "violet");
        options.Alias("input-surface", "paper");
        options.Alias("input-border", "slate");
        options.Alias("focus-ring", "violet");
        options.Alias("muted-content", "slate");
        options.Alias("info", "sky");
        options.Alias("highlight", "sky");
        options.Alias("accent", "sky");
        options.Alias("danger", "violet");
        options.Alias("success", "sky");

        options.AddRecipe("cta-strip", new NerdDesignTokenRecipe("slate", "paper", "sky"));
        options.AddRecipe(
            NerdDesignSystemUi.SidebarRecipe,
            new NerdDesignTokenRecipe("paper", "slate", "violet", Label: "App sidebar", Usage: "Drawer navigation with violet active accent"));

        options.Shell = NerdTokenPackShellTools.DefaultShell;
        options.FrameworkDefaults = new NerdFrameworkDefaults
        {
            MudBlazor = new NerdMudBlazorFrameworkDefaults
            {
                Palette = NerdMudBrandPaletteMap.CreateConventionBindings(),
                Button = NerdTokenPackShellTools.DefaultMudBlazorDefaults.Button,
                TextField = NerdTokenPackShellTools.DefaultMudBlazorDefaults.TextField,
                DatePicker = NerdTokenPackShellTools.DefaultMudBlazorDefaults.DatePicker,
                NavLink = NerdTokenPackShellTools.DefaultMudBlazorDefaults.NavLink
            }
        };
    }
}
