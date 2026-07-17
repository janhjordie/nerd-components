using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace TheNerdCollective.MudComponents.ResponsiveTypography;

public static class NerdResponsiveTypographyWebApplicationExtensions
{
    public static RazorComponentsEndpointConventionBuilder AddNerdResponsiveTypographyCatalog(
        this RazorComponentsEndpointConventionBuilder builder,
        IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(services);

        var options = services.GetService<NerdResponsiveTypographyOptions>();
        if (options is null || !options.EnableCatalogPage)
        {
            return builder;
        }

        return builder.AddAdditionalAssemblies(typeof(NerdResponsiveTypographyWebApplicationExtensions).Assembly);
    }
}
