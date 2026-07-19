using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace TheNerdCollective.MudComponents.DesignTokens;

public static class NerdDesignTokenWebApplicationExtensions
{
    public static RazorComponentsEndpointConventionBuilder AddNerdDesignTokenCatalog(
        this RazorComponentsEndpointConventionBuilder builder,
        IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(services);

        var options = services.GetService<NerdDesignTokenOptions>();
        if (options is not null && !options.EnableCatalogPage)
        {
            return builder;
        }

        return builder.AddAdditionalAssemblies(typeof(NerdDesignTokenWebApplicationExtensions).Assembly);
    }
}
