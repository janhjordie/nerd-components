// Licensed under the Apache License, Version 2.0.

namespace TheNerdCollective.Integrations.Brevo;

/// <summary>
/// Resolves the Brevo API key (e.g. from secure store or configuration).
/// </summary>
public interface IBrevoApiKeyProvider
{
    ValueTask<string?> GetApiKeyAsync(CancellationToken cancellationToken = default);
}
