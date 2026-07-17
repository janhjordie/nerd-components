namespace TheNerdCollective.MudComponents.Shared;

public sealed class NerdDesignSystemOptions
{
    public bool EnableHubPage { get; set; } = true;
    public string HubRoute { get; set; } = "/nerd-design-system";
    public string DesignTokensRoute { get; set; } = "/nerd-design-tokens";
    public string TypographyRoute { get; set; } = "/nerd-typography";
    public bool RestrictHubToDevelopment { get; set; } = true;
}
