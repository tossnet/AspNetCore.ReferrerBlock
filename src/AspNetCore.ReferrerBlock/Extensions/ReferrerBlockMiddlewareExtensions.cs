using AspNetCore.ReferrerBlock.Configuration;
using AspNetCore.ReferrerBlock.Middleware;

namespace Microsoft.AspNetCore.Builder;

public static class ReferrerBlockMiddlewareExtensions
{
    public static IApplicationBuilder UseReferrerBlock(
        this IApplicationBuilder app,
        Action<ReferrerBlockOptions>? configure = null)
    {
        var options = new ReferrerBlockOptions();
        configure?.Invoke(options);

        return app.UseMiddleware<ReferrerBlockMiddleware>(options);
    }
}
