// Licensed under the Apache License, Version 2.0.
// See LICENSE file in the project root for full license information.

namespace TheNerdCollective.Integrations.Brevo;

public sealed record BrevoListSummary(long Id, string Name, int? TotalSubscribers);

public sealed record BrevoSegmentSummary(long Id, string Name, string? CategoryName);

public sealed record BrevoCampaignSummary(
    long Id,
    string Name,
    string? Subject,
    string? Status,
    string? CreatedAt,
    string? SentDate);

public sealed record BrevoPagedResult<T>(int Count, IReadOnlyList<T> Items);
