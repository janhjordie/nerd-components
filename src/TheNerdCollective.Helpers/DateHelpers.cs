using System.Globalization;

namespace TheNerdCollective.Helpers;

/// <summary>
/// Date and time helper methods for formatting and timezone conversions.
/// </summary>
public static class DateHelpers
{
    /// <summary>
    /// Converts Unix timestamp to DateTime.
    /// </summary>
    public static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
    {
        return DateTimeOffset.FromUnixTimeMilliseconds(unixTimeStamp).DateTime;
    }

    /// <summary>
    /// Converts UTC DateTime to specified timezone.
    /// </summary>
    public static DateTime ToTimeZone(this DateTime utcDate, string timeZone = "Romance Standard Time")
    {
        return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(utcDate, timeZone);
    }

    /// <summary>
    /// Formats DateTime to Danish format (dd-MM-yyyy HH:mm).
    /// </summary>
    public static string ToDanish(this DateTime utcDate, string format = "dd-MM-yyyy HH:mm")
    {
        var timeZoneDateTime = utcDate.ToTimeZone();
        return timeZoneDateTime.ToString(format, CultureInfo.InvariantCulture);
    }
}
