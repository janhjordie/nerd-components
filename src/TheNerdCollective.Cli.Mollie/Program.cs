// Licensed under the Apache License, Version 2.0.
// See LICENSE file in the project root for full license information.

using System.CommandLine;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheNerdCollective.Cli.Mollie;
using TheNerdCollective.Integrations.Mollie;
using TheNerdCollective.Integrations.Mollie.Extensions;
using TheNerdCollective.Integrations.Mollie.Models;

var rootCommand = new RootCommand("Mollie Payments API CLI and local product-template manager.");

var profileCommand = new Command("profile", "Show profile metadata for the configured Mollie API key.");
var profileConfigOption = CreateConfigOption();
profileCommand.Options.Add(profileConfigOption);
profileCommand.SetAction(async (parseResult, cancellationToken) =>
    await RunApiCommandAsync(parseResult.GetValue(profileConfigOption), async (client, token) =>
    {
        WriteJson(await client.GetCurrentProfileAsync(token));
        return 0;
    }, cancellationToken));
rootCommand.Subcommands.Add(profileCommand);

var methodsCommand = new Command("methods", "List payment methods available to the profile.");
var methodsConfigOption = CreateConfigOption();
methodsCommand.Options.Add(methodsConfigOption);
methodsCommand.SetAction(async (parseResult, cancellationToken) =>
    await RunApiCommandAsync(parseResult.GetValue(methodsConfigOption), async (client, token) =>
    {
        WriteJson(await client.GetPaymentMethodsAsync(cancellationToken: token));
        return 0;
    }, cancellationToken));
rootCommand.Subcommands.Add(methodsCommand);

var templatesCommand = new Command("templates", "Manage local product templates; these are not Mollie catalog products.");
var templatesListCommand = new Command("list", "List local product templates.");
templatesListCommand.SetAction(async (_, cancellationToken) =>
    await RunLocalCommandAsync(async token =>
    {
        var store = new ProductTemplateStore();
        WriteJson(await store.ListAsync(token));
        return 0;
    }, cancellationToken));

var templatesAddCommand = new Command("add", "Add a local payment template.");
var templateSkuOption = new Option<string?>("--sku") { Description = "Unique local template SKU." };
var templateNameOption = new Option<string?>("--name") { Description = "Human-readable template name." };
var templateAmountOption = new Option<string?>("--amount") { Description = "Positive decimal amount using a dot separator, for example 99.00." };
var templateCurrencyOption = new Option<string?>("--currency") { Description = "Three-letter ISO currency code (default: DKK)." };
var templateDescriptionOption = new Option<string?>("--description") { Description = "Description sent to Mollie when a payment is created." };
templatesAddCommand.Options.Add(templateSkuOption);
templatesAddCommand.Options.Add(templateNameOption);
templatesAddCommand.Options.Add(templateAmountOption);
templatesAddCommand.Options.Add(templateCurrencyOption);
templatesAddCommand.Options.Add(templateDescriptionOption);
templatesAddCommand.SetAction(async (parseResult, cancellationToken) =>
    await RunLocalCommandAsync(async token =>
    {
        var template = new ProductTemplate(
            RequireValue(parseResult.GetValue(templateSkuOption), "--sku"),
            RequireValue(parseResult.GetValue(templateNameOption), "--name"),
            RequireValue(parseResult.GetValue(templateAmountOption), "--amount"),
            parseResult.GetValue(templateCurrencyOption) ?? "DKK",
            parseResult.GetValue(templateDescriptionOption) ?? string.Empty);

        await new ProductTemplateStore().AddAsync(template, token);
        Console.WriteLine($"Saved local template '{template.Sku}'.");
        return 0;
    }, cancellationToken));

var templatesRemoveCommand = new Command("remove", "Remove a local product template by SKU.");
var templateRemoveSkuOption = new Option<string?>("--sku") { Description = "Local template SKU to remove." };
templatesRemoveCommand.Options.Add(templateRemoveSkuOption);
templatesRemoveCommand.SetAction(async (parseResult, cancellationToken) =>
    await RunLocalCommandAsync(async token =>
    {
        var sku = RequireValue(parseResult.GetValue(templateRemoveSkuOption), "--sku");
        await new ProductTemplateStore().RemoveAsync(sku, token);
        Console.WriteLine($"Removed local template '{sku}'.");
        return 0;
    }, cancellationToken));

templatesCommand.Subcommands.Add(templatesListCommand);
templatesCommand.Subcommands.Add(templatesAddCommand);
templatesCommand.Subcommands.Add(templatesRemoveCommand);
rootCommand.Subcommands.Add(templatesCommand);

var paymentsCommand = new Command("payments", "Create and inspect Mollie payments.");
var paymentGetCommand = new Command("get", "Get a payment by Mollie payment ID.");
var paymentGetConfigOption = CreateConfigOption();
var paymentIdOption = new Option<string?>("--payment-id") { Description = "Mollie payment ID." };
paymentGetCommand.Options.Add(paymentGetConfigOption);
paymentGetCommand.Options.Add(paymentIdOption);
paymentGetCommand.SetAction(async (parseResult, cancellationToken) =>
    await RunApiCommandAsync(parseResult.GetValue(paymentGetConfigOption), async (client, token) =>
    {
        var paymentId = RequireValue(parseResult.GetValue(paymentIdOption), "--payment-id");
        WriteJson(await client.GetPaymentAsync(paymentId, token));
        return 0;
    }, cancellationToken));

var paymentCreateCommand = new Command("create", "Create a Mollie payment or recurring-first payment.");
var paymentCreateConfigOption = CreateConfigOption();
var templateOption = new Option<string?>("--template") { Description = "Use a local template SKU for amount, currency, and description." };
var paymentAmountOption = new Option<string?>("--amount") { Description = "Positive decimal amount using a dot separator, for example 99.00." };
var paymentCurrencyOption = new Option<string?>("--currency") { Description = "Three-letter ISO currency code (default: DKK)." };
var paymentDescriptionOption = new Option<string?>("--description") { Description = "Payment description." };
var redirectUrlOption = new Option<string?>("--redirect-url") { Description = "Customer return URL required by Mollie." };
var paymentWebhookUrlOption = new Option<string?>("--webhook-url") { Description = "Optional webhook URL." };
var paymentCustomerIdOption = new Option<string?>("--customer-id") { Description = "Mollie customer ID for a recurring first payment." };
var sequenceTypeOption = new Option<string?>("--sequence-type") { Description = "Payment sequence: oneoff, first, or recurring." };
var idempotencyKeyOption = new Option<string?>("--idempotency-key") { Description = "Optional Mollie idempotency key; otherwise a fresh key is generated." };
paymentCreateCommand.Options.Add(templateOption);
paymentCreateCommand.Options.Add(paymentCreateConfigOption);
paymentCreateCommand.Options.Add(paymentAmountOption);
paymentCreateCommand.Options.Add(paymentCurrencyOption);
paymentCreateCommand.Options.Add(paymentDescriptionOption);
paymentCreateCommand.Options.Add(redirectUrlOption);
paymentCreateCommand.Options.Add(paymentWebhookUrlOption);
paymentCreateCommand.Options.Add(paymentCustomerIdOption);
paymentCreateCommand.Options.Add(sequenceTypeOption);
paymentCreateCommand.Options.Add(idempotencyKeyOption);
paymentCreateCommand.SetAction(async (parseResult, cancellationToken) =>
    await RunApiCommandAsync(parseResult.GetValue(paymentCreateConfigOption), async (client, token) =>
    {
        var templateSku = parseResult.GetValue(templateOption);
        ProductTemplate? template = null;
        if (!string.IsNullOrWhiteSpace(templateSku))
        {
            template = (await new ProductTemplateStore().ListAsync(token))
                .FirstOrDefault(item => string.Equals(item.Sku, templateSku, StringComparison.OrdinalIgnoreCase));
            if (template is null)
            {
                throw new InvalidOperationException($"No local template with SKU '{templateSku}' exists.");
            }
        }

        var amount = template?.Amount ?? RequireValue(parseResult.GetValue(paymentAmountOption), "--amount or --template");
        var currency = template?.Currency ?? parseResult.GetValue(paymentCurrencyOption) ?? "DKK";
        var description = template?.Description
            ?? RequireValue(parseResult.GetValue(paymentDescriptionOption), "--description or --template");
        var request = new MolliePaymentCreateRequest
        {
            Amount = new MollieAmount(currency, amount),
            Description = description,
            RedirectUrl = RequireValue(parseResult.GetValue(redirectUrlOption), "--redirect-url"),
            WebhookUrl = parseResult.GetValue(paymentWebhookUrlOption),
            CustomerId = parseResult.GetValue(paymentCustomerIdOption),
            SequenceType = parseResult.GetValue(sequenceTypeOption)
        };

        var idempotencyKey = parseResult.GetValue(idempotencyKeyOption) ?? Guid.NewGuid().ToString("N");
        WriteJson(await client.CreatePaymentAsync(request, idempotencyKey, token));
        return 0;
    }, cancellationToken));

paymentsCommand.Subcommands.Add(paymentGetCommand);
paymentsCommand.Subcommands.Add(paymentCreateCommand);
rootCommand.Subcommands.Add(paymentsCommand);

var customersCommand = new Command("customers", "Create and inspect Mollie customers.");
var customerNameOption = new Option<string?>("--name") { Description = "Customer name." };
var customerEmailOption = new Option<string?>("--email") { Description = "Customer email address." };
var customerCreateCommand = new Command("create", "Create a Mollie customer.");
var customerCreateConfigOption = CreateConfigOption();
customerCreateCommand.Options.Add(customerCreateConfigOption);
customerCreateCommand.Options.Add(customerNameOption);
customerCreateCommand.Options.Add(customerEmailOption);
customerCreateCommand.SetAction(async (parseResult, cancellationToken) =>
    await RunApiCommandAsync(parseResult.GetValue(customerCreateConfigOption), async (client, token) =>
    {
        var request = new MollieCustomerCreateRequest
        {
            Name = RequireValue(parseResult.GetValue(customerNameOption), "--name"),
            Email = RequireValue(parseResult.GetValue(customerEmailOption), "--email")
        };
        var key = Guid.NewGuid().ToString("N");
        WriteJson(await client.CreateCustomerAsync(request, key, token));
        return 0;
    }, cancellationToken));

var customerGetCommand = new Command("get", "Get a Mollie customer.");
var customerGetConfigOption = CreateConfigOption();
var customerIdOption = new Option<string?>("--customer-id") { Description = "Mollie customer ID." };
customerGetCommand.Options.Add(customerGetConfigOption);
customerGetCommand.Options.Add(customerIdOption);
customerGetCommand.SetAction(async (parseResult, cancellationToken) =>
    await RunApiCommandAsync(parseResult.GetValue(customerGetConfigOption), async (client, token) =>
    {
        var customerId = RequireValue(parseResult.GetValue(customerIdOption), "--customer-id");
        WriteJson(await client.GetCustomerAsync(customerId, token));
        return 0;
    }, cancellationToken));
customersCommand.Subcommands.Add(customerCreateCommand);
customersCommand.Subcommands.Add(customerGetCommand);
rootCommand.Subcommands.Add(customersCommand);

var subscriptionsCommand = new Command("subscriptions", "Create, inspect, or cancel Mollie subscriptions.");
var subscriptionCreateCommand = new Command("create", "Create a subscription for a customer with an active mandate.");
var subscriptionCreateConfigOption = CreateConfigOption();
var subscriptionCustomerIdOption = new Option<string?>("--customer-id") { Description = "Mollie customer ID with an active mandate." };
var subscriptionAmountOption = new Option<string?>("--amount") { Description = "Positive decimal amount using a dot separator, for example 99.00." };
var subscriptionCurrencyOption = new Option<string?>("--currency") { Description = "Three-letter ISO currency code (default: DKK)." };
var subscriptionDescriptionOption = new Option<string?>("--description") { Description = "Subscription description." };
var subscriptionIntervalOption = new Option<string?>("--interval") { Description = "Billing interval, for example '1 month'." };
var subscriptionStartDateOption = new Option<string?>("--start-date") { Description = "Optional first billing date (YYYY-MM-DD)." };
var subscriptionWebhookUrlOption = new Option<string?>("--webhook-url") { Description = "Optional webhook URL." };
subscriptionCreateCommand.Options.Add(subscriptionCustomerIdOption);
subscriptionCreateCommand.Options.Add(subscriptionCreateConfigOption);
subscriptionCreateCommand.Options.Add(subscriptionAmountOption);
subscriptionCreateCommand.Options.Add(subscriptionCurrencyOption);
subscriptionCreateCommand.Options.Add(subscriptionDescriptionOption);
subscriptionCreateCommand.Options.Add(subscriptionIntervalOption);
subscriptionCreateCommand.Options.Add(subscriptionStartDateOption);
subscriptionCreateCommand.Options.Add(subscriptionWebhookUrlOption);
subscriptionCreateCommand.SetAction(async (parseResult, cancellationToken) =>
    await RunApiCommandAsync(parseResult.GetValue(subscriptionCreateConfigOption), async (client, token) =>
    {
        var request = new MollieSubscriptionCreateRequest
        {
            Amount = new MollieAmount(
                parseResult.GetValue(subscriptionCurrencyOption) ?? "DKK",
                RequireValue(parseResult.GetValue(subscriptionAmountOption), "--amount")),
            Description = RequireValue(parseResult.GetValue(subscriptionDescriptionOption), "--description"),
            Interval = RequireValue(parseResult.GetValue(subscriptionIntervalOption), "--interval"),
            StartDate = parseResult.GetValue(subscriptionStartDateOption),
            WebhookUrl = parseResult.GetValue(subscriptionWebhookUrlOption)
        };
        var customerId = RequireValue(parseResult.GetValue(subscriptionCustomerIdOption), "--customer-id");
        WriteJson(await client.CreateSubscriptionAsync(customerId, request, Guid.NewGuid().ToString("N"), token));
        return 0;
    }, cancellationToken));

var subscriptionGetCommand = new Command("get", "Get a Mollie subscription.");
var subscriptionGetConfigOption = CreateConfigOption();
var subscriptionIdOption = new Option<string?>("--subscription-id") { Description = "Mollie subscription ID." };
var subscriptionGetCustomerIdOption = new Option<string?>("--customer-id") { Description = "Mollie customer ID." };
subscriptionGetCommand.Options.Add(subscriptionGetConfigOption);
subscriptionGetCommand.Options.Add(subscriptionGetCustomerIdOption);
subscriptionGetCommand.Options.Add(subscriptionIdOption);
subscriptionGetCommand.SetAction(async (parseResult, cancellationToken) =>
    await RunApiCommandAsync(parseResult.GetValue(subscriptionGetConfigOption), async (client, token) =>
    {
        var customerId = RequireValue(parseResult.GetValue(subscriptionGetCustomerIdOption), "--customer-id");
        var subscriptionId = RequireValue(parseResult.GetValue(subscriptionIdOption), "--subscription-id");
        WriteJson(await client.GetSubscriptionAsync(customerId, subscriptionId, token));
        return 0;
    }, cancellationToken));

var subscriptionCancelCommand = new Command("cancel", "Cancel a Mollie subscription.");
var subscriptionCancelConfigOption = CreateConfigOption();
var subscriptionCancelCustomerIdOption = new Option<string?>("--customer-id") { Description = "Mollie customer ID." };
var subscriptionCancelIdOption = new Option<string?>("--subscription-id") { Description = "Mollie subscription ID to cancel." };
subscriptionCancelCommand.Options.Add(subscriptionCancelCustomerIdOption);
subscriptionCancelCommand.Options.Add(subscriptionCancelConfigOption);
subscriptionCancelCommand.Options.Add(subscriptionCancelIdOption);
subscriptionCancelCommand.SetAction(async (parseResult, cancellationToken) =>
    await RunApiCommandAsync(parseResult.GetValue(subscriptionCancelConfigOption), async (client, token) =>
    {
        var customerId = RequireValue(parseResult.GetValue(subscriptionCancelCustomerIdOption), "--customer-id");
        var subscriptionId = RequireValue(parseResult.GetValue(subscriptionCancelIdOption), "--subscription-id");
        await client.CancelSubscriptionAsync(customerId, subscriptionId, token);
        Console.WriteLine($"Cancelled Mollie subscription '{subscriptionId}'.");
        return 0;
    }, cancellationToken));

subscriptionsCommand.Subcommands.Add(subscriptionCreateCommand);
subscriptionsCommand.Subcommands.Add(subscriptionGetCommand);
subscriptionsCommand.Subcommands.Add(subscriptionCancelCommand);
rootCommand.Subcommands.Add(subscriptionsCommand);

return await rootCommand.Parse(args).InvokeAsync();

static async Task<int> RunApiCommandAsync(
    FileInfo? configFile,
    Func<MollieApiClient, CancellationToken, Task<int>> action,
    CancellationToken cancellationToken)
{
    try
    {
        var configuration = LoadConfiguration(configFile);
        var apiKey = Environment.GetEnvironmentVariable("MOLLIE_API_KEY")
            ?? configuration["NerdConsent:Mollie:ApiKey"]
            ?? configuration["Mollie:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                "No Mollie API key found. Set MOLLIE_API_KEY or NerdConsent__Mollie__ApiKey, or pass --config.");
        }

        var services = new ServiceCollection();
        services.AddMollieIntegration(configuration, "NerdConsent:Mollie");
        services.Configure<MollieOptions>(options => options.ApiKey = apiKey);
        using var provider = services.BuildServiceProvider();
        return await action(provider.GetRequiredService<MollieApiClient>(), cancellationToken);
    }
    catch (Exception exception)
    {
        Console.Error.WriteLine($"Error: {exception.Message}");
        return 1;
    }
}

static async Task<int> RunLocalCommandAsync(
    Func<CancellationToken, Task<int>> action,
    CancellationToken cancellationToken)
{
    try
    {
        return await action(cancellationToken);
    }
    catch (Exception exception)
    {
        Console.Error.WriteLine($"Error: {exception.Message}");
        return 1;
    }
}

static IConfigurationRoot LoadConfiguration(FileInfo? configFile)
{
    var builder = new ConfigurationBuilder();
    if (configFile is not null)
    {
        if (!configFile.Exists)
        {
            throw new FileNotFoundException($"Configuration file '{configFile.FullName}' was not found.");
        }

        builder.AddJsonFile(configFile.FullName, optional: false, reloadOnChange: false);
    }

    builder.AddEnvironmentVariables();
    return builder.Build();
}

static Option<FileInfo?> CreateConfigOption() =>
    new("--config")
    {
        Description = "Optional appsettings.json file containing NerdConsent:Mollie:ApiKey."
    };

static string RequireValue(string? value, string optionName) =>
    !string.IsNullOrWhiteSpace(value)
        ? value
        : throw new InvalidOperationException($"Option '{optionName}' is required.");

static void WriteJson<T>(T value) =>
    Console.WriteLine(JsonSerializer.Serialize(value, new JsonSerializerOptions(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    }));
