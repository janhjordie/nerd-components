using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;

namespace TheNerdCollective.MudComponents.DesignTokens;

public static class NerdDesignTokenWebApplicationExtensions
{
    public static RazorComponentsEndpointConventionBuilder AddNerdDesignTokenCatalog(
        this RazorComponentsEndpointConventionBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.AddAdditionalAssemblies(typeof(NerdDesignTokenWebApplicationExtensions).Assembly);
    }
}
