using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace TheNerdCollective.MudComponents.FeatureFeedback;

public static class NerdFeatureFeedbackWebApplicationExtensions
{
    /// <summary>
    /// Registers RCL pages from this package (<c>/feature-ideas</c>, <c>/admin/feature-ideas</c>).
    /// </summary>
    public static RazorComponentsEndpointConventionBuilder AddNerdFeatureFeedbackPages(
        this RazorComponentsEndpointConventionBuilder builder) =>
        builder.AddAdditionalAssemblies(typeof(NerdFeatureFeedbackWebApplicationExtensions).Assembly);
}

public static class NerdFeatureFeedbackServiceCollectionExtensions
{
    /// <summary>
    /// Registers a host store and optional admin gate for Feature Feedback pages.
    /// </summary>
    public static IServiceCollection AddNerdFeatureFeedback<TStore, TAdminAccess>(
        this IServiceCollection services)
        where TStore : class, TheNerdCollective.Blazor.FeatureFeedback.IFeatureFeedbackStore
        where TAdminAccess : class, TheNerdCollective.Blazor.FeatureFeedback.IFeatureFeedbackAdminAccess
    {
        services.AddScoped<TheNerdCollective.Blazor.FeatureFeedback.IFeatureFeedbackStore, TStore>();
        services.AddScoped<TheNerdCollective.Blazor.FeatureFeedback.IFeatureFeedbackAdminAccess, TAdminAccess>();
        return services;
    }
}
