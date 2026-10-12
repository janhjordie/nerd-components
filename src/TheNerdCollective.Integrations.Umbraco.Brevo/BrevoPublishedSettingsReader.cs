using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace TheNerdCollective.Integrations.Umbraco.Brevo;

public sealed class BrevoPublishedSettingsReader
{
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;
    private readonly UmbracoBrevoIntegrationOptions _options;

    public BrevoPublishedSettingsReader(
        IUmbracoContextAccessor umbracoContextAccessor,
        IOptions<UmbracoBrevoIntegrationOptions> options)
    {
        _umbracoContextAccessor = umbracoContextAccessor;
        _options = options.Value;
    }

    public BrevoPublishedSettings? GetSettings()
    {
        if (!_umbracoContextAccessor.TryGetUmbracoContext(out IUmbracoContext? context))
        {
            return null;
        }

        IPublishedContent? node = ResolveSettingsNode(context);
        if (node is null)
        {
            return null;
        }

        return new BrevoPublishedSettings(
            node.Value<int>(_options.NewsletterListIdPropertyAlias),
            node.Value<string>(_options.SenderEmailPropertyAlias) ?? string.Empty,
            node.Value<string>(_options.SenderNamePropertyAlias) ?? _options.DefaultSenderName,
            node.Value<string>(_options.NotifyEmailPropertyAlias) ?? string.Empty);
    }

    private IPublishedContent? ResolveSettingsNode(IUmbracoContext context)
    {
        if (_options.SettingsContentKey is Guid key)
        {
            return context.Content?.GetById(key);
        }

        return null;
    }
}

public sealed record BrevoPublishedSettings(
    int NewsletterListId,
    string SenderEmail,
    string SenderName,
    string NotifyEmail);
