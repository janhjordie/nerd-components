using Microsoft.Extensions.DependencyInjection;

namespace TheNerdCollective.MudComponents.DesignTokens;

public static class NerdDesignTokenServiceCollectionExtensions
{
    public static IServiceCollection AddNerdDesignTokens(
        this IServiceCollection services,
        Action<NerdDesignTokenOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        var options = new NerdDesignTokenOptions();
        configure(options);
        services.AddSingleton(options);
        services.AddSingleton(sp => new NerdDesignTokenCss(
            MudBlazorDesignTokenCssGenerator.Generate(sp.GetRequiredService<NerdDesignTokenOptions>())));

        return services;
    }
}
