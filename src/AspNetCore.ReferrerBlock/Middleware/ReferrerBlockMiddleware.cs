using ReferrerBlock.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ReferrerBlock.Middleware;

public class ReferrerBlockMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ReferrerBlockMiddleware> _logger;
    private readonly ReferrerBlockOptions _options;

    public ReferrerBlockMiddleware(
        RequestDelegate next,
        ILogger<ReferrerBlockMiddleware> logger,
        ReferrerBlockOptions options)
    {
        _next = next;
        _logger = logger;
        _options = options;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var referer = context.Request.Headers.Referer.ToString();

        if (!string.IsNullOrEmpty(referer))
        {
            try
            {
                var uri = new Uri(referer);
                var host = uri.Host.ToLowerInvariant();

                if (IsBlocked(host))
                {
                    _logger.LogWarning(
                        "🚫 Referrer spam blocked: {Host} | IP: {IP} | Path: {Path}",
                        host,
                        context.Connection.RemoteIpAddress,
                        context.Request.Path
                    );

                    // Slow down aggressive bots
                    await Task.Delay(Random.Shared.Next(100, 500));

                    context.Response.StatusCode = StatusCodes.Status410Gone;
                    await context.Response.WriteAsync("<!DOCTYPE html><html><head><title>Gone</title></head><body></body></html>");
                    return;
                }
            }
            catch (UriFormatException)
            {
                _logger.LogDebug("Malformed referrer detected: {Referer}", referer);
            }
        }

        await _next(context);
    }

    private bool IsBlocked(string host)
    {
        // Check TLDs - TLDs already contain the dot (e.g., ".icu")
        if (_options.BlockedTLDs != null && 
            _options.BlockedTLDs.Any(tld => !string.IsNullOrWhiteSpace(tld) && 
                                           host.EndsWith(tld, StringComparison.OrdinalIgnoreCase)))
            return true;

        // Check exact domains
        if (_options.BlockedDomains != null && 
            _options.BlockedDomains.Contains(host, StringComparer.OrdinalIgnoreCase))
            return true;

        // Check subdomains
        if (_options.BlockedDomains != null && 
            _options.BlockedDomains.Any(blocked => !string.IsNullOrWhiteSpace(blocked) && 
                                                   host.EndsWith($".{blocked}", StringComparison.OrdinalIgnoreCase)))
            return true;

        // Check patterns
        if (_options.BlockedPatterns != null && 
            _options.BlockedPatterns.Any(pattern => !string.IsNullOrWhiteSpace(pattern) && 
                                                    host.Contains(pattern, StringComparison.OrdinalIgnoreCase)))
            return true;

        return false;
    }
}