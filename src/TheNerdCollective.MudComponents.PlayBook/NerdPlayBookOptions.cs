namespace TheNerdCollective.MudComponents.PlayBook;

/// <summary>
/// Configuration for the MudBlazor PlayBook playground.
/// </summary>
public sealed class NerdPlayBookOptions
{
    /// <summary>Enables the PlayBook page.</summary>
    public bool EnablePlayBookPage { get; set; } = true;

    /// <summary>Route for the PlayBook page.</summary>
    public string PlayBookRoute { get; set; } = "/nerd-playbook";

    /// <summary>Restricts the PlayBook to development environments.</summary>
    public bool RestrictPlayBookToDevelopment { get; set; } = true;

    /// <summary>Base URL for MudBlazor component documentation links.</summary>
    public string MudBlazorDocsBaseUrl { get; set; } = "https://mudblazor.com/components";
}
