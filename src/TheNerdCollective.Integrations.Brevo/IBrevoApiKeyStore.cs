// Licensed under the Apache License, Version 2.0.

namespace TheNerdCollective.Integrations.Brevo;

/// <summary>
/// Optional persistent store for the API key (implemented by Umbraco backoffice).
/// </summary>
public interface IBrevoApiKeyStore
{
    ValueTask<string?> GetAsync(CancellationToken cancellationToken = default);

    ValueTask SetAsync(string apiKey, CancellationToken cancellationToken = default);

    ValueTask ClearAsync(CancellationToken cancellationToken = default);
}
