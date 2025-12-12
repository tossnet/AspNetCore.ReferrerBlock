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
                // add a schema by defaut is missing
                if (!referer.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                    !referer.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    referer = "https://" + referer;
                }


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
        if (_options.BlockedTLDs?.Any(tld => !string.IsNullOrEmpty(tld) && 
            host.EndsWith(tld, StringComparison.OrdinalIgnoreCase)) == true)
            return true;

        // Check exact domains
        if (_options.BlockedDomains?.Contains(host, StringComparer.OrdinalIgnoreCase) == true)
            return true;

        // Check subdomains
        if (_options.BlockedDomains?.Any(blocked => !string.IsNullOrEmpty(blocked) && 
            host.EndsWith($".{blocked}", StringComparison.OrdinalIgnoreCase)) == true)
            return true;

        // Check patterns
        if (_options.BlockedPatterns?.Any(pattern => !string.IsNullOrEmpty(pattern) && 
            host.Contains(pattern, StringComparison.OrdinalIgnoreCase)) == true)
            return true;

        // Check subdomain prefixes (e.g., iqri1., hk12.)
        if (_options.BlockedSubdomainPrefixes?.Any(prefix => !string.IsNullOrEmpty(prefix) && 
            IsMatchingSubdomainPrefix(host, prefix)) == true)
            return true;

        return false;
    }

    /// <summary>
    /// Checks if host starts with prefix followed by optional digits and a dot.
    /// Matches: iqri., iqri1., iqri18. but not iqrix. or iqri1x.
    /// </summary>
    private static bool IsMatchingSubdomainPrefix(ReadOnlySpan<char> host, string prefix)
    {
        if (!host.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return false;

        var remaining = host[prefix.Length..];

        // Skip any digits after the prefix
        int i = 0;
        while (i < remaining.Length && char.IsAsciiDigit(remaining[i]))
            i++;

        // Must be followed by a dot (subdomain separator)
        return i < remaining.Length && remaining[i] == '.';
    }
}