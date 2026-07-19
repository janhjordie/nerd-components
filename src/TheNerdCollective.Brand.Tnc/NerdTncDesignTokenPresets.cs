using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.Brand.Tnc;

public static class NerdTncDesignTokenPresets
{
    public const string Navy = "#122E43";
    public const string Coral = "#F27271";
    public const string Snow = "#FFFFFF";
    public const string Ink = "#111827";

    public static void Apply(NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Prefix = "tnc";

        options.Add("navy", new NerdColorToken
        {
            Value = Navy,
            Surface = Navy,
            Content = Snow,
            ContrastText = Snow,
            Hover = NerdColorDerivatives.Darken(Navy, 0.08)
        });
        options.Add("coral", new NerdColorToken
        {
            Value = Coral,
            Surface = Coral,
            Content = Snow,
            ContrastText = Snow,
            Hover = NerdColorDerivatives.Darken(Coral, 0.1)
        });
        options.Add("snow", new NerdColorToken
        {
            Value = Snow,
            Surface = Snow,
            Content = Ink,
            ContrastText = Ink,
            Hover = NerdColorDerivatives.Darken(Snow, 0.04)
        });
        options.Add("ink", new NerdColorToken
        {
            Value = Ink,
            Content = Ink,
            ContrastText = Snow,
            Hover = NerdColorDerivatives.Darken(Ink, 0.08)
        });
        options.Add("chalk", new NerdColorToken
        {
            Value = Snow,
            Surface = Snow,
            Content = Snow,
            ContrastText = Ink
        });

        options.Alias("primary-action", "coral");
        options.Alias("secondary-action", "navy");
        options.Alias("on-primary-action", "chalk");
        options.Alias("page-surface", "snow");
        options.Alias("brand-chrome", "navy");
        options.Alias("on-brand-chrome", "chalk");
        options.Alias("nav-surface", "snow");
        options.Alias("nav-item", "ink");
        options.Alias("nav-item-active", "coral");
        options.Alias("input-surface", "snow");
        options.Alias("input-border", "ink");
        options.Alias("focus-ring", "coral");
        options.Alias("muted-content", "ink");
        options.Alias("info", "navy");
        options.Alias("highlight", "coral");
        options.Alias("danger", "coral");
        options.Alias("success", "navy");

        options.AddRecipe("hero", new NerdDesignTokenRecipe("navy", "chalk", "coral"));
        options.AddRecipe("header", new NerdDesignTokenRecipe("snow", "ink", "coral"));
        options.AddRecipe("tagline", new NerdDesignTokenRecipe("navy", "coral", "chalk"));
        options.AddRecipe("cta", new NerdDesignTokenRecipe("coral", "chalk", "navy"));
        options.AddRecipe(
            NerdDesignSystemUi.SidebarRecipe,
            new NerdDesignTokenRecipe(
                Surface: "snow",
                Content: "ink",
                Action: "coral",
                Label: "App sidebar",
                Usage: "Drawer navigation with coral active accent"));

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

        NerdSpacingScaleTools.ApplyDefaultScale(options);
        NerdFoundationTaxonomyTools.ApplyDefaults(options);
    }
}
