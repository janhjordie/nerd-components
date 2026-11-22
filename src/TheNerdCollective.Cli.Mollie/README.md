# TheNerdCollective.Cli.Mollie

`tnc-mollie` is a .NET global tool for inspecting and managing Mollie payment resources and maintaining local payment templates.

## Microsoft CLI and .NET version choice

The CLI uses Microsoft's stable [`System.CommandLine` 2.0.12](https://www.nuget.org/packages/System.CommandLine/2.0.12) package for command parsing, asynchronous handlers, help, and shell-completion support. It is packaged as a .NET tool (`PackAsTool` / `ToolCommandName`) and installs through the standard .NET CLI.

It targets `net10.0` and the .NET 10 SDK. Microsoft lists .NET 10 as LTS through November 14, 2028. .NET 11 is scheduled to launch at .NET Conf on November 10–12, 2026, and is an STS release with a two-year support window. Keeping the first release on the repository's stable .NET 10 baseline avoids making a preview/RC runtime a prerequisite; re-evaluate .NET 11 after GA.

Microsoft references:

- [Build a command-line app with System.CommandLine](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/tutorials/system-command-line)
- [Create a .NET tool](https://learn.microsoft.com/en-us/dotnet/core/tools/global-tools-how-to-create)
- [.NET release and support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core)
- [.NET Conf 2026: .NET 11 launch](https://devblogs.microsoft.com/dotnet/dotnet-conf-2026/)

## Install

```bash
dotnet tool install --global TheNerdCollective.Cli.Mollie
```

For local development:

```bash
dotnet pack src/TheNerdCollective.Cli.Mollie -c Release
dotnet tool install --global --add-source src/TheNerdCollective.Cli.Mollie/nupkg TheNerdCollective.Cli.Mollie
```

## Authentication

The tool accepts a key from `MOLLIE_API_KEY`, `NerdConsent__Mollie__ApiKey`, or `Mollie__ApiKey`. A configuration file can also be supplied explicitly, including Nerd Consent's settings file:

```bash
tnc-mollie profile --config products/consent/src/TheNerdCollective.Consent.Host/appsettings.json
```

The tool never accepts the API key as a command-line argument and never prints it. Prefer environment variables or a secret store over committing keys in JSON files.

## Commands

```bash
tnc-mollie profile
tnc-mollie methods

tnc-mollie templates add --sku consent-monthly --name "Consent monthly" --amount 99.00 --currency DKK
tnc-mollie templates list
tnc-mollie templates remove --sku consent-monthly

tnc-mollie payments create --template consent-monthly --redirect-url https://example.test/paid
tnc-mollie payments get --payment-id tr_...

tnc-mollie customers create --name "Test customer" --email test@example.test
tnc-mollie payments create --amount 1.00 --currency DKK --description "Mandate setup" \
  --redirect-url https://example.test/paid --customer-id cst_... --sequence-type first
tnc-mollie subscriptions create --customer-id cst_... --amount 99.00 --currency DKK \
  --description "Consent monthly" --interval "1 month"
tnc-mollie subscriptions get --customer-id cst_... --subscription-id sub_...
tnc-mollie subscriptions cancel --customer-id cst_... --subscription-id sub_...
```

Creating a payment, customer, or subscription sends a real request to the configured Mollie account. For recurring payments, create and complete an initial `first` payment so Mollie can establish a mandate before creating a subscription.

## Local templates are not Mollie products

Mollie has no general product-catalog API. `templates add/list/remove` stores template data only on this computer, under the current user's application-data directory. A template can supply the amount, currency, and description for a payment; it does not create a Mollie product, inventory record, tax rule, or hosted checkout by itself.
