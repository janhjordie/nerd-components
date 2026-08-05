using Microsoft.Extensions.DependencyInjection;

namespace TheNerdCollective.Blazor.FeatureFeedback;

public static class FeatureFeedbackServiceCollectionExtensions
{
    /// <summary>
    /// Registers the in-memory feature feedback store (demos/tests). Prefer a host EF store in production.
    /// </summary>
    public static IServiceCollection AddInMemoryFeatureFeedback(this IServiceCollection services)
    {
        services.AddSingleton<IFeatureFeedbackStore, InMemoryFeatureFeedbackStore>();
        return services;
    }

    public static IServiceCollection AddFeatureFeedbackStore<TStore>(this IServiceCollection services)
        where TStore : class, IFeatureFeedbackStore
    {
        services.AddScoped<IFeatureFeedbackStore, TStore>();
        return services;
    }
}
