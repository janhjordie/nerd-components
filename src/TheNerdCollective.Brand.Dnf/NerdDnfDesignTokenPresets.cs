using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.Brand.Dnf;

/// <summary>
/// Danmarks Naturfredningsforening (DNF) brand colors from the 2025 identity palette.
/// Pairing: <c>Kridt</c> (#FDFAF3) on dark tokens, <c>Skov</c> (#002D26) on light tokens.
/// </summary>
public static class NerdDnfDesignTokenPresets
{
    public const string KridtText = "#FDFAF3";
    public const string SkovText = "#002D26";

    public static void Apply(NerdDesignTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Prefix = "dnf";

        AddDark(options, "jord", "#3B342E");
        AddDark(options, "ler", "#6D5346");
        AddLight(options, "kridt", "#E8E0D3");
        AddLight(options, "kridt-lys", "#FDFAF3");
        AddLight(options, "sol", "#FE993F");
        AddLight(options, "morgenrode", "#FF5E63");
        AddDark(options, "hav", "#0C2E3A");
        AddLight(options, "himmel", "#81ABFF");
        AddLight(options, "flod", "#6BE6E4");
        AddDark(options, "skov", "#002D26");
        AddDark(options, "blad", "#0E4D3A");
        AddLight(options, "graes", "#A6E54C");

        options.AddRecipe(
            "kridt-himmel",
            new NerdDesignTokenRecipe(
                Surface: "kridt",
                Content: "skov",
                Action: "himmel",
                Border: "himmel"));

        options.AddRecipe(
            "hero",
            new NerdDesignTokenRecipe(
                Surface: "kridt-lys",
                Content: "skov",
                Action: "himmel"));

        options.AddRecipe(
            "cta-strip",
            new NerdDesignTokenRecipe(
                Surface: "skov",
                Content: "kridt",
                Action: "sol"));

        options.AddRecipe(
            "link-card",
            new NerdDesignTokenRecipe(
                Surface: "kridt",
                Content: "skov",
                Action: "hav"));

        options.AddRecipe(
            "footer",
            new NerdDesignTokenRecipe(
                Surface: "jord",
                Content: "kridt",
                Action: "himmel"));

        options.Alias("primary-action", "himmel");
        options.Alias("page-surface", "kridt-lys");
        options.Alias("brand-chrome", "skov");
        options.Alias("on-brand-chrome", "kridt-lys");
        options.Alias("muted-content", "hav");
        options.Alias("info", "flod");
        options.Alias("highlight", "sol");
        options.Alias("danger", "morgenrode");
        options.Alias("success", "graes");

        options.AddOpacity("watermark", new NerdOpacityToken("skov", 0.12));
        options.AddOpacity("hero-overlay", new NerdOpacityToken("jord", 0.35));
    }

    private static void AddDark(NerdDesignTokenOptions options, string name, string value) =>
        options.Add(name, new NerdColorToken
        {
            Value = value,
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
