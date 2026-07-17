namespace TheNerdCollective.MudComponents.Shared;

internal sealed class NerdDesignSystemConfigureAction(Action<NerdDesignSystemOptions> configure)
{
    public Action<NerdDesignSystemOptions> Configure { get; } = configure;
}
