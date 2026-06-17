using System.Globalization;

namespace TheNerdCollective.Integrations.Dar.TestWeb.Services;

internal static class MethodCallFormatter
{
    public static string CsString(string? value) =>
        value is null ? "null" : $"\"{value.Replace("\\", "\\\\").Replace("\"", "\\\"")}\"";

    public static string CsNullableString(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "null" : CsString(value);

    public static string Number(double value) =>
        value.ToString(CultureInfo.InvariantCulture);
}
