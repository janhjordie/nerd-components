// Licensed under the Apache License, Version 2.0.
// See LICENSE file in the project root for full license information.

namespace TheNerdCollective.Integrations.Mollie.Models;

/// <summary>
/// Fields for creating a Mollie payment.
/// </summary>
public sealed class MolliePaymentCreateRequest
{
    public required MollieAmount Amount { get; init; }
    public required string Description { get; init; }
    public required string RedirectUrl { get; init; }
    public string? WebhookUrl { get; init; }
    public string? CustomerId { get; init; }
    public string? SequenceType { get; init; }
    public object? Metadata { get; init; }
}

/// <summary>
/// Fields for creating a Mollie customer.
/// </summary>
public sealed class MollieCustomerCreateRequest
{
    public required string Name { get; init; }
    public required string Email { get; init; }
    public object? Metadata { get; init; }
}

/// <summary>
/// Fields for creating a Mollie subscription.
/// </summary>
public sealed class MollieSubscriptionCreateRequest
{
    public required MollieAmount Amount { get; init; }
    public required string Description { get; init; }
    public required string Interval { get; init; }
    public string? StartDate { get; init; }
    public string? WebhookUrl { get; init; }
    public string? MandateId { get; init; }
}
