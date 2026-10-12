using Microsoft.Extensions.DependencyInjection;
using TheNerdCollective.Integrations.Brevo.Extensions;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace TheNerdCollective.Integrations.Umbraco.Brevo;

public sealed class BrevoIntegrationComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddControllers()
            .AddApplicationPart(typeof(BrevoIntegrationComposer).Assembly);

        builder.Services.Configure<UmbracoBrevoIntegrationOptions>(
            builder.Config.GetSection(UmbracoBrevoIntegrationOptions.SectionName));
        builder.Services.AddBrevoIntegration(builder.Config);
        builder.Services.AddSingleton<BrevoPublishedSettingsReader>();
    }
}
