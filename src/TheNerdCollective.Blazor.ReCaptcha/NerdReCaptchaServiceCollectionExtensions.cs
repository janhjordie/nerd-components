using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TheNerdCollective.Blazor.ReCaptcha;

public static class NerdReCaptchaServiceCollectionExtensions
{
    public static IServiceCollection AddNerdReCaptcha(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<NerdReCaptchaOptions>(configuration.GetSection(NerdReCaptchaOptions.SectionName));
        return services.AddNerdReCaptcha();
    }

    public static IServiceCollection AddNerdReCaptcha(
        this IServiceCollection services,
        Action<NerdReCaptchaOptions>? configure = null)
    {
        if (configure is not null)
        {
            services.Configure(configure);
        }

        services.AddHttpClient(nameof(NerdReCaptchaVerifier));
        services.AddSingleton<INerdReCaptchaVerifier, NerdReCaptchaVerifier>();
        return services;
    }
}
