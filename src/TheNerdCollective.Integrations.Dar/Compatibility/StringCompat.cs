using System;

namespace TheNerdCollective.Integrations.Dar.Compatibility
{
    internal static class StringCompat
    {
        internal static bool ContainsOrdinal(string source, string value) =>
            source.IndexOf(value, StringComparison.Ordinal) >= 0;

        internal static string SubstringFromEnd(string source, int lengthFromEnd) =>
            source.Substring(source.Length - lengthFromEnd);

        internal static string[] SplitTrimmed(string source, char separator)
        {
            var parts = source.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < parts.Length; i++)
            {
                parts[i] = parts[i].Trim();
            }

            return parts;
        }
    }
}
