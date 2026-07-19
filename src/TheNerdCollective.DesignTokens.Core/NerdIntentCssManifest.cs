namespace TheNerdCollective.MudComponents.DesignTokens;

/// <summary>Framework-neutral intent CSS contract (HR-117 / TS-017).</summary>
public static class NerdIntentCssManifest
{
    public static readonly string[] Channels =
    [
        "surface",
        "content",
        "interactive",
        "hover",
        "border",
        "disabled"
    ];

    public static string BrandRootClass(string prefix) => $"{prefix}-nerd-brand";

    public static string IntentVariable(string alias, string channel) => $"--nerd-intent-{alias}-{channel}";
}
