// Licensed under the Apache License, Version 2.0.
// See LICENSE file in the project root for full license information.

namespace TheNerdCollective.Integrations.Brevo;

public sealed record BrevoContactAttribute(
    string Name,
    string Category,
    string? Type,
    IReadOnlyList<BrevoContactAttributeOption>? Enumeration,
    IReadOnlyList<string>? MultiCategoryOptions,
    bool? IsRecurring,
    string? Value,
    string? CalculatedValue);

public sealed record BrevoContactAttributeOption(long? Value, string? ValueStr, string Label);

public sealed record BrevoContactAttributeWriteOptions(
    string? Type = null,
    IReadOnlyList<BrevoContactAttributeOption>? Enumeration = null,
    bool? IsRecurring = null,
    IReadOnlyList<string>? MultiCategoryOptions = null,
    string? Value = null);
