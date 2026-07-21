using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.Brand.Dnf;

/// <summary>
/// Danmarks Naturfredningsforening (DNF) brand colors from the 2025 identity palette.
/// Pairing: <c>Kridt</c> (#FDFAF3) on dark tokens, <c>Skov</c> (#002D26) on light tokens.
/// </summary>
public static class NerdDnfDesignTokenPresets
{
    public const string Graes = "#A6E54C";
    public const string Kridt = "#FDFAF3";
    public const string Skov = "#002D26";
    public const string KridtText = Kridt;
    public const string SkovText = Skov;

    public static void Apply(NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Prefix = "dnf";

        AddDark(options, "jord", "#3B342E");
        AddDark(options, "ler", "#6D5346");
        AddLight(options, "kridt", "#E8E0D3");
        AddLight(options, "kridt-lys", Kridt);
        AddLight(options, "sol", "#FE993F");
        AddLight(options, "morgenrode", "#FF5E63");
        AddDark(options, "hav", "#0C2E3A");
        AddLight(options, "himmel", "#80ABFF");
        AddLight(options, "flod", "#6BE6E4");
        AddDark(options, "skov", Skov);
        AddDark(options, "blad", "#0E4D3A");
        AddLight(options, "graes", Graes);

        // DNF's core identity is skov, kridt and græs. Himmel/flod remain
        // supporting colors for links and information states.
        options.AddRecipe(
            "kridt-himmel",
            new NerdDesignTokenRecipe(
                Surface: "kridt",
                Content: "skov",
                Action: "graes",
                Border: "graes"));

        options.AddRecipe(
            "hero",
            new NerdDesignTokenRecipe(
                Surface: "kridt-lys",
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
                Content: "skov",
                Action: "skov"));

        options.AddRecipe(
            "footer",
            new NerdDesignTokenRecipe(
                Surface: "jord",
                Content: "kridt",
                Action: "graes"));

        options.Alias("primary-action", "graes");
        options.Alias("secondary-action", "skov");
        options.Alias("on-primary-action", "skov");
        options.Alias("page-surface", "kridt-lys");
        options.Alias("brand-chrome", "skov");
        options.Alias("on-brand-chrome", "kridt-lys");
        options.Alias("nav-surface", "kridt-lys");
        options.Alias("nav-item", "skov");
        options.Alias("nav-item-active", "graes");
        options.Alias("input-surface", "kridt-lys");
        options.Alias("input-border", "skov");
        options.Alias("focus-ring", "graes");
        options.Alias("muted-content", "skov");
        options.Alias("info", "flod");
        options.Alias("highlight", "sol");
        options.Alias("danger", "morgenrode");
        options.Alias("success", "graes");

        options.AddOpacity("watermark", new NerdOpacityToken("skov", 0.12));
        options.AddOpacity("hero-overlay", new NerdOpacityToken("jord", 0.35));

        options.AddRecipe(
            NerdDesignSystemUi.SidebarRecipe,
            new NerdDesignTokenRecipe(
                Surface: "kridt-lys",
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
            "hero-organic",
            new NerdDesignTokenRecipe(
                Surface: "skov",
                Content: "kridt",
                Action: "graes",
                Label: "Organic hero",
                Usage: "Dark skov hero with organic watermark and græs CTA"));

        options.AddRecipe(
            "hero-light",
            new NerdDesignTokenRecipe(
                Surface: "himmel",
                Content: "skov",
                Action: "graes",
                Label: "Light hero",
                Usage: "Light himmel hero with skov headline"));

        options.AddRecipe(
            "footer-minimal",
            new NerdDesignTokenRecipe(
                Surface: "skov",
                Content: "kridt",
                Label: "Minimal footer",
                Usage: "Single-line legal footer bar"));

        options.AddRecipe(
            "feature-panel",
            new NerdDesignTokenRecipe(
                Surface: "skov",
                Content: "kridt",
                Action: "graes",
                Label: "Feature panel",
                Usage: "Skov panel with topic rows on hero"));

        options.AddRecipe(
            "partner-row",
            new NerdDesignTokenRecipe(
                Surface: "jord",
                Content: "kridt",
                Label: "Partner row",
                Usage: "I samarbejde med logo strip"));

        options.AddRecipe(
            "formular",
            new NerdDesignTokenRecipe(
                Surface: "kridt-lys",
                Content: "skov",
                Action: "graes",
                Label: "Form strip",
                Usage: "Light form surface with skov labels"));

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
            ContrastText = SkovText,
            Hover = NerdColorDerivatives.Darken(value, 0.12)
        });
}
