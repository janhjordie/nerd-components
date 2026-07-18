using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace TheNerdCollective.MudComponents.PlayBook;

public static class NerdPlayBookWebApplicationExtensions
{
    public static RazorComponentsEndpointConventionBuilder AddNerdPlayBook(
        this RazorComponentsEndpointConventionBuilder builder,
        IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(services);

        var options = services.GetService<NerdPlayBookOptions>();
        if (options is not null && !options.EnablePlayBookPage)
        {
            return builder;
        }

        return builder.AddAdditionalAssemblies(typeof(NerdPlayBookWebApplicationExtensions).Assembly);
    }
}
