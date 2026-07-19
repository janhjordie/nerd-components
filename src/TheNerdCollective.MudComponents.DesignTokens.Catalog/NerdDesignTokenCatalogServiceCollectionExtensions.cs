using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace TheNerdCollective.MudComponents.DesignTokens;

public static class NerdDesignTokenCatalogServiceCollectionExtensions
{
    public static IServiceCollection AddNerdDesignTokenCatalog(
        this IServiceCollection services,
        string commentsDirectory = "App_Data/token-comments")
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddSingleton<INerdTokenCommentStore>(
            _ => new FileNerdTokenCommentStore(commentsDirectory));
        return services;
    }
}
