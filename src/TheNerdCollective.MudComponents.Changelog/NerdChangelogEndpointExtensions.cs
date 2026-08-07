using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace TheNerdCollective.MudComponents.Changelog;

public static class NerdChangelogEndpointExtensions
{
    public const string CanonicalRoute = "/nerd-changelog";

    /// <summary>
    /// Rewrites <see cref="NerdChangelogOptions.ChangelogRoute"/> to the canonical
    /// <c>/nerd-changelog</c> page for SSR endpoint matching. No-op when routes match.
    /// Pair with <see cref="IsConfiguredChangelogPath"/> in the interactive Router NotFound handler.
    /// </summary>
    public static IApplicationBuilder UseNerdChangelogRouteOverride(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        return app.Use(async (ctx, next) =>
        {
            var options = ctx.RequestServices.GetService<NerdChangelogOptions>();
            if (options is { EnableChangelogPage: true }
                && TryGetOverrideTarget(options.ChangelogRoute, out var configured)
                && PathEquals(ctx.Request.Path, configured))
            {
                ctx.Request.Path = CanonicalRoute;
            }

            await next();
        });
    }

    /// <summary>True when the browser path is the configured public changelog URL (alias or canonical).</summary>
    public static bool IsConfiguredChangelogPath(PathString path, NerdChangelogOptions? options)
    {
        if (options is not { EnableChangelogPage: true })
        {
            return PathEquals(path, CanonicalRoute);
        }

        return PathEquals(path, options.ChangelogRoute) || PathEquals(path, CanonicalRoute);
    }

    /// <summary>True when path is the configured public route and it differs from the canonical page route.</summary>
    public static bool IsChangelogRouteAlias(PathString path, NerdChangelogOptions? options)
    {
        if (options is not { EnableChangelogPage: true })
        {
            return false;
        }

        if (!TryGetOverrideTarget(options.ChangelogRoute, out var configured))
        {
            return false;
        }

        return PathEquals(path, configured);
    }

    private static bool TryGetOverrideTarget(string? route, out PathString configured)
    {
        configured = default;
        if (string.IsNullOrWhiteSpace(route))
        {
            return false;
        }

        var normalized = Normalize(route);
        if (string.Equals(normalized, CanonicalRoute, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        configured = normalized;
        return true;
    }

    private static string Normalize(string route)
    {
        var value = route.Trim();
        if (!value.StartsWith('/'))
        {
            value = "/" + value;
        }

        if (value.Length > 1)
        {
            value = value.TrimEnd('/');
        }

        return value;
    }

    private static bool PathEquals(PathString path, string route) =>
        path.Equals(route, StringComparison.OrdinalIgnoreCase)
        || path.StartsWithSegments(route, StringComparison.OrdinalIgnoreCase, out var remaining)
           && (remaining.Value is null or "" or "/");
}
