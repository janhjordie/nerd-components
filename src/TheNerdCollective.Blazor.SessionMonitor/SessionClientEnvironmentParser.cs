using Microsoft.AspNetCore.Http;

namespace TheNerdCollective.Blazor.SessionMonitor;

/// <summary>
/// Parses User-Agent and client hint headers into a compact session environment snapshot.
/// </summary>
public static class SessionClientEnvironmentParser
{
    private const int MaxUserAgentLength = 512;

    public static SessionClientEnvironment Parse(HttpContext? context)
    {
        if (context is null)
        {
            return SessionClientEnvironment.Unknown;
        }

        var userAgent = context.Request.Headers.UserAgent.ToString();
        var secChUa = context.Request.Headers["sec-ch-ua"].ToString();
        var secChUaMobile = context.Request.Headers["sec-ch-ua-mobile"].ToString();
        var secChUaPlatform = context.Request.Headers["sec-ch-ua-platform"].ToString();

        return Parse(userAgent, secChUa, secChUaMobile, secChUaPlatform);
    }

    internal static SessionClientEnvironment Parse(
        string? userAgent,
        string? secChUa = null,
        string? secChUaMobile = null,
        string? secChUaPlatform = null)
    {
        var ua = userAgent ?? string.Empty;
        var environment = new SessionClientEnvironment
        {
            UserAgent = TruncateUserAgent(ua),
            Platform = ParsePlatform(ua, secChUaPlatform),
            FormFactor = ParseFormFactor(ua, secChUaMobile),
            Browser = ParseBrowser(ua, secChUa)
        };

        ApplyAutomationDetection(environment, ua, secChUa);
        return environment;
    }

    private static void ApplyAutomationDetection(
        SessionClientEnvironment environment,
        string userAgent,
        string? secChUa)
    {
        if (ContainsAutomationToken(userAgent, "Billetsalg-Playwright", "Playwright"))
        {
            environment.IsAutomation = true;
            environment.AutomationKind = "Playwright";
            return;
        }

        if (ContainsAutomationToken(userAgent, "HeadlessChrome")
            || ContainsAutomationToken(secChUa, "HeadlessChrome"))
        {
            environment.IsAutomation = true;
            environment.AutomationKind = "Headless";
        }
    }

    private static bool ContainsAutomationToken(string? value, params string[] tokens)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        foreach (var token in tokens)
        {
            if (value.Contains(token, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static string ParseBrowser(string userAgent, string? secChUa)
    {
        if (string.IsNullOrWhiteSpace(userAgent))
        {
            return "Unknown";
        }

        if (userAgent.Contains("Edg/", StringComparison.OrdinalIgnoreCase)
            || userAgent.Contains("EdgA/", StringComparison.OrdinalIgnoreCase)
            || userAgent.Contains("EdgiOS/", StringComparison.OrdinalIgnoreCase))
        {
            return "Edge";
        }

        if (userAgent.Contains("Firefox/", StringComparison.OrdinalIgnoreCase))
        {
            return "Firefox";
        }

        if (userAgent.Contains("CriOS/", StringComparison.OrdinalIgnoreCase)
            || userAgent.Contains("Chrome/", StringComparison.OrdinalIgnoreCase)
            || userAgent.Contains("HeadlessChrome", StringComparison.OrdinalIgnoreCase)
            || ContainsBrowserBrand(secChUa, "Google Chrome", "Chromium"))
        {
            return "Chrome";
        }

        if (userAgent.Contains("Safari/", StringComparison.OrdinalIgnoreCase)
            || ContainsBrowserBrand(secChUa, "Safari"))
        {
            return "Safari";
        }

        return "Unknown";
    }

    private static bool ContainsBrowserBrand(string? secChUa, params string[] brands)
    {
        if (string.IsNullOrWhiteSpace(secChUa))
        {
            return false;
        }

        foreach (var brand in brands)
        {
            if (secChUa.Contains(brand, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static string ParsePlatform(string userAgent, string? secChUaPlatform)
    {
        if (!string.IsNullOrWhiteSpace(secChUaPlatform))
        {
            var hinted = UnquoteClientHint(secChUaPlatform);
            if (!string.IsNullOrWhiteSpace(hinted))
            {
                return NormalizePlatformName(hinted);
            }
        }

        if (userAgent.Contains("iPhone", StringComparison.OrdinalIgnoreCase)
            || userAgent.Contains("iPod", StringComparison.OrdinalIgnoreCase))
        {
            return "iOS";
        }

        if (userAgent.Contains("iPad", StringComparison.OrdinalIgnoreCase))
        {
            return "iPadOS";
        }

        if (userAgent.Contains("Android", StringComparison.OrdinalIgnoreCase))
        {
            return "Android";
        }

        if (userAgent.Contains("Windows", StringComparison.OrdinalIgnoreCase))
        {
            return "Windows";
        }

        if (userAgent.Contains("Mac OS X", StringComparison.OrdinalIgnoreCase)
            || userAgent.Contains("Macintosh", StringComparison.OrdinalIgnoreCase))
        {
            return "macOS";
        }

        if (userAgent.Contains("CrOS", StringComparison.OrdinalIgnoreCase))
        {
            return "ChromeOS";
        }

        if (userAgent.Contains("Linux", StringComparison.OrdinalIgnoreCase))
        {
            return "Linux";
        }

        return "Unknown";
    }

    private static string ParseFormFactor(string userAgent, string? secChUaMobile)
    {
        if (string.Equals(secChUaMobile?.Trim(), "?1", StringComparison.Ordinal))
        {
            return "Mobile";
        }

        if (userAgent.Contains("iPad", StringComparison.OrdinalIgnoreCase))
        {
            return "Tablet";
        }

        if (userAgent.Contains("Android", StringComparison.OrdinalIgnoreCase))
        {
            return userAgent.Contains("Mobile", StringComparison.OrdinalIgnoreCase)
                ? "Mobile"
                : "Tablet";
        }

        if (userAgent.Contains("iPhone", StringComparison.OrdinalIgnoreCase)
            || userAgent.Contains("iPod", StringComparison.OrdinalIgnoreCase)
            || userAgent.Contains("Mobile", StringComparison.OrdinalIgnoreCase))
        {
            return "Mobile";
        }

        return "Desktop";
    }

    private static string NormalizePlatformName(string platform)
        => platform switch
        {
            "macOS" or "Mac OS X" or "MacOS" => "macOS",
            "Windows" => "Windows",
            "Android" => "Android",
            "iOS" => "iOS",
            "Chrome OS" or "ChromeOS" or "CrOS" => "ChromeOS",
            "Linux" => "Linux",
            _ => platform
        };

    private static string UnquoteClientHint(string value)
    {
        var trimmed = value.Trim();
        if (trimmed.Length >= 2 && trimmed.StartsWith('"') && trimmed.EndsWith('"'))
        {
            return trimmed[1..^1];
        }

        return trimmed;
    }

    private static string? TruncateUserAgent(string userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent))
        {
            return null;
        }

        return userAgent.Length <= MaxUserAgentLength
            ? userAgent
            : userAgent[..MaxUserAgentLength];
    }
}
