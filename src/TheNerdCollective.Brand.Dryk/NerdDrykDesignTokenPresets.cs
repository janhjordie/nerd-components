using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.Brand.Dryk;

/// <summary>
/// DRYK brand colors from dryk.dk theme CSS and product packaging.
/// Pairing: <c>kridt</c> on dark chrome, <c>ink</c>/<c>skov</c> on light surfaces.
/// </summary>
public static class NerdDrykDesignTokenPresets
{
    public const string Skov = "#2A4B42";
    public const string Mint = "#DAEAE0";
    public const string Graes = "#93C1A4";
    public const string Sten = "#EEEEEE";
    public const string Kridt = "#FFFFFF";
    public const string Ink = "#000000";
    public const string KridtText = Kridt;
    public const string InkText = Ink;
    public const string SkovText = Skov;

    public static void Apply(NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Prefix = "dryk";

        AddDark(options, "skov", Skov);
        AddLight(options, "mint", Mint);
        AddLight(options, "graes", Graes);
        AddLight(options, "sten", Sten);
        AddLight(options, "kridt", Kridt);
        AddLight(options, "ink", Ink);
        AddLight(options, "aerter", "#6B9B4F");
        AddLight(options, "havre", "#E5D4A0");
        AddLight(options, "sol", "#D97928");

        options.AddRecipe(
            "hero",
            new NerdDesignTokenRecipe(
                Surface: "sten",
                Content: "skov",
                Action: "graes"));

        options.AddRecipe(
            "cta-strip",
            new NerdDesignTokenRecipe(
                Surface: "skov",
                Content: "kridt",
                Action: "graes"));

        options.AddRecipe(
            "link-card",
            new NerdDesignTokenRecipe(
                Surface: "kridt",
                Content: "ink",
                Action: "skov"));

        options.AddRecipe(
            "footer",
            new NerdDesignTokenRecipe(
                Surface: "skov",
                Content: "kridt",
                Action: "graes"));

        options.Alias("primary-action", "graes");
        options.Alias("secondary-action", "skov");
        options.Alias("on-primary-action", "skov");
        options.Alias("page-surface", "sten");
        options.Alias("brand-chrome", "skov");
        options.Alias("on-brand-chrome", "kridt");
        options.Alias("nav-surface", "kridt");
        options.Alias("nav-item", "skov");
        options.Alias("nav-item-active", "graes");
        options.Alias("input-surface", "kridt");
        options.Alias("input-border", "skov");
        options.Alias("focus-ring", "graes");
        options.Alias("muted-content", "ink");
        options.Alias("info", "mint");
        options.Alias("highlight", "sol");
        options.Alias("danger", "sol");
        options.Alias("success", "graes");

        options.AddOpacity("watermark", new NerdOpacityToken("skov", 0.12));
        options.AddOpacity("hero-overlay", new NerdOpacityToken("skov", 0.45));

        options.AddRecipe(
            NerdDesignSystemUi.SidebarRecipe,
            new NerdDesignTokenRecipe(
                Surface: "kridt",
                Content: "skov",
                Action: "graes",
                Label: "App sidebar",
                Usage: "Drawer navigation with græs active accent"));

        options.AddRecipe(
            "hero-photo",
            new NerdDesignTokenRecipe(
                Surface: "skov",
                Content: "kridt",
                Action: "graes",
                Label: "Photo hero",
                Usage: "Full-bleed photo hero with kridt headline and græs CTA"));

        options.AddRecipe(
            "footer-minimal",
            new NerdDesignTokenRecipe(
                Surface: "skov",
                Content: "kridt",
                Label: "Minimal footer",
                Usage: "Single-line legal footer bar"));

        options.AddRecipe(
            "formular",
            new NerdDesignTokenRecipe(
                Surface: "kridt",
                Content: "ink",
                Action: "graes",
                Label: "Form strip",
                Usage: "Light form surface with ink labels"));

        NerdSpacingScaleTools.ApplyDefaultScale(options);
        NerdFoundationTaxonomyTools.ApplyDefaults(options);

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

    private static void AddDark(NerdDesignTokenOptions options, string name, string value) =>
        options.Add(name, new NerdColorToken
        {
            Value = value,
            Surface = value,
            Content = KridtText,
            ContrastText = KridtText,
            Hover = NerdColorDerivatives.Darken(value, 0.08)
        });

    private static void AddLight(NerdDesignTokenOptions options, string name, string value) =>
        options.Add(name, new NerdColorToken
        {
            Value = value,
            ContrastText = string.Equals(name, "ink", StringComparison.OrdinalIgnoreCase) ? Kridt : InkText,
            Hover = NerdColorDerivatives.Darken(value, 0.12)
        });
}
