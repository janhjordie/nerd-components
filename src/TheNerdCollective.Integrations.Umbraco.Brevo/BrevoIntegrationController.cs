using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TheNerdCollective.Integrations.Brevo;
using Umbraco.Cms.Api.Management.Controllers;
using Umbraco.Cms.Api.Management.Routing;

namespace TheNerdCollective.Integrations.Umbraco.Brevo;

[VersionedApiBackOfficeRoute("the-nerd-collective/brevo")]
[ApiExplorerSettings(GroupName = "The Nerd Collective Brevo")]
public sealed class BrevoIntegrationController : ManagementApiControllerBase
{
    private const int DefaultPageSize = 50;

    private readonly BrevoService _brevo;
    private readonly BrevoPublishedSettingsReader _settings;
    private readonly UmbracoBrevoIntegrationOptions _options;

    public BrevoIntegrationController(
        BrevoService brevo,
        BrevoPublishedSettingsReader settings,
        IOptions<UmbracoBrevoIntegrationOptions> options)
    {
        _brevo = brevo;
        _settings = settings;
        _options = options.Value;
    }

    [HttpGet("status")]
    public async Task<ActionResult<BrevoIntegrationStatusResponse>> GetStatus(CancellationToken cancellationToken)
        => Ok(await BuildStatusAsync(cancellationToken));

    [HttpGet("dashboard")]
    public async Task<ActionResult<BrevoDashboardResponse>> GetDashboard(
        [FromQuery] int limit = DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        limit = Math.Clamp(limit, 1, 50);
        BrevoIntegrationStatusResponse status = await BuildStatusAsync(cancellationToken);

        if (!status.ApiKeyConfigured)
        {
            return Ok(new BrevoDashboardResponse(
                status,
                null,
                null,
                null,
                null,
                "Brevo:ApiKey er ikke sat på serveren."));
        }

        if (!status.Connected)
        {
            return Ok(new BrevoDashboardResponse(
                status,
                null,
                null,
                null,
                null,
                status.Error));
        }

        BrevoDataResult<BrevoPagedResult<BrevoListSummary>> lists =
            await _brevo.ListContactListsAsync(limit, 0, cancellationToken);
        BrevoDataResult<BrevoPagedResult<BrevoSegmentSummary>> segments =
            await _brevo.ListSegmentsAsync(limit, 0, cancellationToken);
        BrevoDataResult<BrevoContactListResult> contacts =
            await _brevo.ListContactsAsync(limit, 0, cancellationToken);
        BrevoDataResult<BrevoPagedResult<BrevoCampaignSummary>> campaigns =
            await _brevo.ListEmailCampaignsAsync(limit, 0, cancellationToken);

        string? dataError = FirstApiError(lists.Error, segments.Error, contacts.Error, campaigns.Error)
            ?? (!lists.Succeeded ? "Kunne ikke hente lister." : null)
            ?? (!segments.Succeeded ? "Kunne ikke hente segmenter." : null)
            ?? (!contacts.Succeeded ? "Kunne ikke hente kontakter." : null)
            ?? (!campaigns.Succeeded ? "Kunne ikke hente kampagner." : null);

        return Ok(new BrevoDashboardResponse(
            status,
            MapPage(lists),
            MapPage(segments),
            MapContacts(contacts),
            MapPage(campaigns),
            dataError));
    }

    private async Task<BrevoIntegrationStatusResponse> BuildStatusAsync(CancellationToken cancellationToken)
    {
        BrevoPublishedSettings? cms = _settings.GetSettings();
        if (!_brevo.IsConfigured)
        {
            return new BrevoIntegrationStatusResponse(
                ApiKeyConfigured: false,
                Connected: false,
                StatusCode: 0,
                Error: "Brevo:ApiKey er ikke sat på serveren.",
                SettingsLocationHint: _options.SettingsLocationHint,
                AccountEmail: null,
                PlanType: null,
                ContactCount: null,
                CmsSettings: MapCms(cms));
        }

        BrevoDataResult<BrevoAccount> account = await _brevo.GetAccountAsync(cancellationToken);
        if (!account.Succeeded || account.Data is null)
        {
            return new BrevoIntegrationStatusResponse(
                ApiKeyConfigured: true,
                Connected: false,
                StatusCode: account.StatusCode,
                Error: account.Error,
                SettingsLocationHint: _options.SettingsLocationHint,
                AccountEmail: null,
                PlanType: null,
                ContactCount: null,
                CmsSettings: MapCms(cms));
        }

        int? contactCount = null;
        BrevoDataResult<BrevoContactListResult> contacts = await _brevo.ListContactsAsync(1, 0, cancellationToken);
        if (contacts.Succeeded && contacts.Data is not null)
        {
            contactCount = contacts.Data.Count;
        }

        return new BrevoIntegrationStatusResponse(
            ApiKeyConfigured: true,
            Connected: true,
            StatusCode: account.StatusCode,
            Error: null,
            SettingsLocationHint: _options.SettingsLocationHint,
            AccountEmail: account.Data.Email,
            PlanType: account.Data.PlanType,
            ContactCount: contactCount,
            CmsSettings: MapCms(cms));
    }

    private static BrevoPagedDto<T>? MapPage<T>(BrevoDataResult<BrevoPagedResult<T>> result)
        where T : class
        => result.Succeeded && result.Data is not null
            ? new BrevoPagedDto<T>(result.Data.Count, result.Data.Items)
            : null;

    private static BrevoContactsPageDto? MapContacts(BrevoDataResult<BrevoContactListResult> result)
        => result.Succeeded && result.Data is not null
            ? new BrevoContactsPageDto(result.Data.Count, result.Data.Contacts)
            : null;

    private static string? FirstApiError(params string?[] errors)
    {
        foreach (string? error in errors)
        {
            if (!string.IsNullOrWhiteSpace(error))
            {
                return TruncateError(error);
            }
        }

        return null;
    }

    private static string TruncateError(string error)
        => error.Length <= 200 ? error : error[..200] + "…";

    private static BrevoCmsSettingsDto? MapCms(BrevoPublishedSettings? cms)
        => cms is null
            ? null
            : new BrevoCmsSettingsDto(
                cms.NewsletterListId,
                cms.SenderEmail,
                cms.SenderName,
                cms.NotifyEmail);
}

public sealed record BrevoIntegrationStatusResponse(
    bool ApiKeyConfigured,
    bool Connected,
    int StatusCode,
    string? Error,
    string SettingsLocationHint,
    string? AccountEmail,
    string? PlanType,
    int? ContactCount,
    BrevoCmsSettingsDto? CmsSettings);

public sealed record BrevoDashboardResponse(
    BrevoIntegrationStatusResponse Status,
    BrevoPagedDto<BrevoListSummary>? Lists,
    BrevoPagedDto<BrevoSegmentSummary>? Segments,
    BrevoContactsPageDto? Contacts,
    BrevoPagedDto<BrevoCampaignSummary>? Campaigns,
    string? DataError);

public sealed record BrevoPagedDto<T>(int Count, IReadOnlyList<T> Items);

public sealed record BrevoContactsPageDto(int Count, IReadOnlyList<BrevoContactSummary> Items);

public sealed record BrevoCmsSettingsDto(
    int NewsletterListId,
    string SenderEmail,
    string SenderName,
    string NotifyEmail);
