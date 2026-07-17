using Microsoft.Extensions.DependencyInjection;
using TheNerdCollective.MudComponents.Shared;

namespace TheNerdCollective.MudComponents.DesignTokens;

public static class NerdDesignTokenServiceCollectionExtensions
{
    public static IServiceCollection AddNerdDesignTokens(
        this IServiceCollection services,
        Action<NerdDesignTokenOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.AddNerdDesignSystem(options =>
        {
            options.DesignTokensRoute = "/nerd-design-tokens";
        });

        var options = new NerdDesignTokenOptions();
        configure(options);
        services.AddSingleton(options);
        services.AddSingleton(sp => new NerdDesignTokenCss(
            MudBlazorDesignTokenCssGenerator.Generate(sp.GetRequiredService<NerdDesignTokenOptions>())));

        if (options.Colors.Count > 0 && options.WarnOnAccessibilityFailuresAtStartup)
        {
            services.AddHostedService<NerdDesignTokenAccessibilityStartupValidator>();
        }

        return services;
    }
}
