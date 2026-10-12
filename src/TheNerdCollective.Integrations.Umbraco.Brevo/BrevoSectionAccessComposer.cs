using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;

namespace TheNerdCollective.Integrations.Umbraco.Brevo;

/// <summary>
/// Grants the Brevo section to the Administrators user group on startup.
/// </summary>
public sealed class BrevoSectionAccessComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
        => builder.AddNotificationAsyncHandler<UmbracoApplicationStartedNotification, BrevoSectionAccessNotification>();
}

internal sealed class BrevoSectionAccessNotification : INotificationAsyncHandler<UmbracoApplicationStartedNotification>
{
    private readonly IUserGroupService _userGroupService;
    private readonly UmbracoBrevoIntegrationOptions _options;

    public BrevoSectionAccessNotification(
        IUserGroupService userGroupService,
        IOptions<UmbracoBrevoIntegrationOptions> options)
    {
        _userGroupService = userGroupService;
        _options = options.Value;
    }

    public async Task HandleAsync(UmbracoApplicationStartedNotification notification, CancellationToken cancellationToken)
    {
        IUserGroup? administrators = await _userGroupService.GetAsync(Constants.Security.AdminGroupAlias);
        if (administrators is null)
        {
            return;
        }

        string sectionAlias = _options.SectionAlias;
        IEnumerable<string> sections = administrators.AllowedSections ?? Array.Empty<string>();
        if (sections.Contains(sectionAlias, StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        administrators.AddAllowedSection(sectionAlias);
        await _userGroupService.UpdateAsync(administrators, Constants.Security.SuperUserKey);
    }
}
