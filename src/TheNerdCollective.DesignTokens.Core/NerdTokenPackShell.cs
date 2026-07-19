namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>Application shell bindings from a token pack (app bar, drawer, nav, main).</summary>
public sealed class NerdTokenPackShell
{
    public NerdTokenPackShellSlot? AppBar { get; init; }

    public NerdTokenPackShellSlot? Drawer { get; init; }

    public NerdTokenPackShellSlot? NavMenu { get; init; }

    public NerdTokenPackShellSlot? Main { get; init; }

    public void Validate()
    {
        AppBar?.Validate(nameof(AppBar));
        Drawer?.Validate(nameof(Drawer));
        NavMenu?.Validate(nameof(NavMenu));
        Main?.Validate(nameof(Main));
    }
}
