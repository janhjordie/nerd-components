namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>MudBlazor component → semantic intent mappings from a token pack.</summary>
public sealed class NerdMudBlazorFrameworkDefaults
{
    public NerdMudBlazorPaletteBindings? Palette { get; init; }

    public NerdMudBlazorButtonDefaults? Button { get; init; }

    public NerdMudBlazorComponentIntent? TextField { get; init; }

    public NerdMudBlazorComponentIntent? DatePicker { get; init; }

    public NerdMudBlazorComponentIntent? Select { get; init; }

    public NerdMudBlazorNavLinkDefaults? NavLink { get; init; }
}

public sealed class NerdMudBlazorButtonDefaults
{
    public string? Filled { get; init; }

    public string? Outlined { get; init; }

    public string? Text { get; init; }
}

public sealed class NerdMudBlazorComponentIntent
{
    public string? Intent { get; init; }

    public string? Popover { get; init; }

    /// <summary>Semantic intent for selected list/menu option (shell: nav-item-active).</summary>
    public string? Selected { get; init; }
}

public sealed class NerdMudBlazorNavLinkDefaults
{
    public string? Default { get; init; }

    public string? Active { get; init; }
}
