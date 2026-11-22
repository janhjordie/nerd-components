// Licensed under the Apache License, Version 2.0.
// See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace TheNerdCollective.Integrations.Mollie.Models;

/// <summary>
/// A Mollie amount in ISO 4217 currency and provider-formatted decimal text.
/// </summary>
public sealed record MollieAmount(string Currency, string Value);

/// <summary>
/// The current Mollie profile.
/// </summary>
public sealed class MollieProfile
{
    public string? Id { get; init; }
    public string? Mode { get; init; }
    public string? Name { get; init; }
    public string? Website { get; init; }
    public string? Email { get; init; }
    public string? Status { get; init; }
}

/// <summary>
/// A payment method available to the Mollie profile.
/// </summary>
public sealed class MollieMethod
{
    public string? Id { get; init; }
    public string? Description { get; init; }
    public string? Status { get; init; }
    public MollieAmount? MinimumAmount { get; init; }
    public MollieAmount? MaximumAmount { get; init; }
}

/// <summary>
/// A Mollie payment resource.
/// </summary>
public sealed class MolliePayment
{
    public string? Id { get; init; }
    public string? Mode { get; init; }
    public string? Status { get; init; }
    public MollieAmount? Amount { get; init; }
    public string? Description { get; init; }
    public string? SequenceType { get; init; }
    public string? CustomerId { get; init; }
    public string? SubscriptionId { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }

    [JsonPropertyName("_links")]
    public MollieResourceLinks? Links { get; init; }
}

/// <summary>
/// A Mollie customer resource.
/// </summary>
public sealed class MollieCustomer
{
    public string? Id { get; init; }
    public string? Mode { get; init; }
    public string? Name { get; init; }
    public string? Email { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
}

/// <summary>
/// A Mollie subscription resource.
/// </summary>
public sealed class MollieSubscription
{
    public string? Id { get; init; }
    public string? Status { get; init; }
    public MollieAmount? Amount { get; init; }
    public string? Description { get; init; }
    public string? Interval { get; init; }
    public string? StartDate { get; init; }
    public string? NextPaymentDate { get; init; }
    public string? MandateId { get; init; }
}

/// <summary>
/// Links included with Mollie resources.
/// </summary>
public sealed class MollieResourceLinks
{
    public MollieResourceLink? Checkout { get; init; }
}

/// <summary>
/// A linked Mollie resource URL.
/// </summary>
public sealed class MollieResourceLink
{
    public string? Href { get; init; }
    public string? Type { get; init; }
}

internal sealed class MollieMethodsResponse
{
    [JsonPropertyName("_embedded")]
    public MollieMethodsEmbedded? Embedded { get; init; }
}

internal sealed class MollieMethodsEmbedded
{
    public List<MollieMethod> Methods { get; init; } = [];
}
