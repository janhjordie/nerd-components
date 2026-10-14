// Licensed under the Apache License, Version 2.0.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace TheNerdCollective.Integrations.Brevo;

public sealed class BrevoApiKeyProvider : IBrevoApiKeyProvider
{
    private readonly BrevoOptions _options;
    private readonly IBrevoApiKeyStore? _store;

    public BrevoApiKeyProvider(IOptions<BrevoOptions> options)
    {
        _options = options.Value;
        _store = null;
    }

    public BrevoApiKeyProvider(IOptions<BrevoOptions> options, IServiceProvider serviceProvider)
    {
        _options = options.Value;
        _store = serviceProvider.GetService<IBrevoApiKeyStore>();
    }

    public async ValueTask<string?> GetApiKeyAsync(CancellationToken cancellationToken = default)
    {
        if (_store is not null)
        {
            string? stored = await _store.GetAsync(cancellationToken);
            if (!string.IsNullOrWhiteSpace(stored))
            {
                return stored;
            }
        }

        return string.IsNullOrWhiteSpace(_options.ApiKey) ? null : _options.ApiKey;
    }
}
