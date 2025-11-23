using ReferrerBlock.Configuration;
using ReferrerBlock.Middleware;

namespace Microsoft.AspNetCore.Builder;

public static class ReferrerBlockMiddlewareExtensions
{
    public static IApplicationBuilder UseReferrerBlock(
        this IApplicationBuilder app,
        Action<ReferrerBlockOptions>? configure = null)
    {
        var options = new ReferrerBlockOptions();
        configure?.Invoke(options);

        // Remove null or empty values from collections to prevent blocking legitimate traffic
        options.BlockedDomains?.RemoveWhere(string.IsNullOrWhiteSpace);
        options.BlockedTLDs?.RemoveWhere(string.IsNullOrWhiteSpace);
        options.BlockedPatterns?.RemoveWhere(string.IsNullOrWhiteSpace);

        return app.UseMiddleware<ReferrerBlockMiddleware>(options);
    }
}
