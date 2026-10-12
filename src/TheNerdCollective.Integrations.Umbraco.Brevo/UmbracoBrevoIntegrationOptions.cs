namespace TheNerdCollective.Integrations.Umbraco.Brevo;

/// <summary>
/// Configuration for the Umbraco Brevo backoffice integration.
/// Bind from the "UmbracoBrevo" configuration section.
/// </summary>
public sealed class UmbracoBrevoIntegrationOptions
{
    public const string SectionName = "UmbracoBrevo";

    /// <summary>Top-level backoffice section alias (must match umbraco-package.json).</summary>
    public string SectionAlias { get; set; } = "TheNerdCollective.Brevo";

    /// <summary>Plugin controller area for <see cref="Umbraco.Cms.Web.Common.Attributes.PluginControllerAttribute"/>.</summary>
    public string BackOfficeArea { get; set; } = "TheNerdCollective";

    /// <summary>Published content type alias for site Brevo settings.</summary>
    public string ContentTypeAlias { get; set; } = "brevoSiteSettings";

    /// <summary>Optional fixed content key for the settings node.</summary>
    public Guid? SettingsContentKey { get; set; }

    /// <summary>Hint shown in the dashboard for editors (e.g. where to edit settings in the tree).</summary>
    public string SettingsLocationHint { get; set; } = "Content → site settings → Brevo";

    public string NewsletterListIdPropertyAlias { get; set; } = "newsletterListId";

    public string SenderEmailPropertyAlias { get; set; } = "senderEmail";

    public string SenderNamePropertyAlias { get; set; } = "senderName";

    public string NotifyEmailPropertyAlias { get; set; } = "notifyEmail";

    public string DefaultSenderName { get; set; } = "DRYK";
}
