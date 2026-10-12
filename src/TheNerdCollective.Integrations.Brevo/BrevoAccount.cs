// Licensed under the Apache License, Version 2.0.
// See LICENSE file in the project root for full license information.

namespace TheNerdCollective.Integrations.Brevo;

public sealed record BrevoAccount(
    string Email,
    string? CompanyName,
    string? PlanType);

public sealed record BrevoContactSummary(
    string Email,
    string? FirstName,
    string? LastName);

public sealed record BrevoContactListResult(
    int Count,
    IReadOnlyList<BrevoContactSummary> Contacts);

public sealed record BrevoDataResult<T>(bool Succeeded, int StatusCode, string? Error, T? Data)
{
    public static BrevoDataResult<T> Ok(T data, int statusCode) => new(true, statusCode, null, data);

    public static BrevoDataResult<T> Fail(int statusCode, string? error) => new(false, statusCode, error, default);
}
