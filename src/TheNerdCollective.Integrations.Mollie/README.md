# TheNerdCollective.Integrations.Mollie

Typed .NET 10 client for Mollie's Payments API. The integration uses Mollie's HTTPS REST API directly so it can cover recurring-payment fields such as `sequenceType` and `customerId` without relying on SDK request models that omit them.

## Register the client

```csharp
using TheNerdCollective.Integrations.Mollie;
using TheNerdCollective.Integrations.Mollie.Extensions;

services.AddMollieIntegration(configuration, "NerdConsent:Mollie");
```

The `ApiKey` setting can come from configuration or an environment variable:

```json
{
  "NerdConsent": {
    "Mollie": {
      "ApiKey": "<set outside source control>"
    }
  }
}
```

The corresponding environment variable is `NerdConsent__Mollie__ApiKey`. Keep keys in user-secrets, a deployment secret store, or environment variables; do not commit a real key.

## Use the API

```csharp
var mollie = serviceProvider.GetRequiredService<MollieApiClient>();
var profile = await mollie.GetCurrentProfileAsync(cancellationToken);
var methods = await mollie.GetPaymentMethodsAsync(cancellationToken: cancellationToken);
```

The client supports profile and payment-method inspection, payment creation/retrieval, customer creation/retrieval, and subscription create/get/cancel. Create operations accept an optional Mollie idempotency key and all API methods accept a cancellation token. Provider failures are surfaced as `MollieApiException`; its message contains the HTTP status only, never the configured API key or response body.

Mollie does not provide a general product-catalog API. Product templates in `tnc-mollie` are stored locally and only become Mollie resources when used to create a payment or subscription. They are not remote products, inventory, or tax configuration.

## CLI and runtime guidance

The companion [TheNerdCollective.Cli.Mollie](../TheNerdCollective.Cli.Mollie/README.md) is a .NET global tool built with Microsoft's `System.CommandLine` package. It targets .NET 10 LTS, the stable runtime used by this repository. Microsoft schedules the .NET 11 launch for .NET Conf on November 10–12, 2026; .NET 11 is an STS release, while .NET 10 remains LTS. Reassess the target framework after .NET 11 GA rather than building this first release on a preview/RC.

- [Microsoft: Build a command-line app with System.CommandLine](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/tutorials/system-command-line)
- [Microsoft: Create a .NET tool](https://learn.microsoft.com/en-us/dotnet/core/tools/global-tools-how-to-create)
- [Microsoft: .NET support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core)
- [Microsoft: .NET Conf 2026 and .NET 11 launch](https://devblogs.microsoft.com/dotnet/dotnet-conf-2026/)
