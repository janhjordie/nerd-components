using Microsoft.Extensions.DependencyInjection;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.PlayBook;

public static class NerdPlayBookServiceCollectionExtensions
{
    public static IServiceCollection AddNerdPlayBook(
        this IServiceCollection services,
        Action<NerdPlayBookOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new NerdPlayBookOptions();
        configure?.Invoke(options);

        services.AddNerdDesignSystem(hub => hub.PlayBookRoute = options.PlayBookRoute);
        services.AddSingleton(options);

        return services;
    }
}
