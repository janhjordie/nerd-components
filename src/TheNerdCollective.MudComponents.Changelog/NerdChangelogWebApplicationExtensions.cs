using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace TheNerdCollective.MudComponents.Changelog;

public static class NerdChangelogWebApplicationExtensions
{
    public static RazorComponentsEndpointConventionBuilder AddNerdChangelog(
        this RazorComponentsEndpointConventionBuilder builder,
        IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(services);

        var options = services.GetService<NerdChangelogOptions>();
        if (options is not null && !options.EnableChangelogPage)
        {
            return builder;
        }

        return builder.AddAdditionalAssemblies(typeof(NerdChangelogWebApplicationExtensions).Assembly);
    }
}
