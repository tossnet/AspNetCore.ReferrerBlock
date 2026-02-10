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
        var refererHeader = context.Request.Headers.Referer;
        
        if (refererHeader.Count > 0 && !string.IsNullOrEmpty(refererHeader[0]))
        {
            var referer = refererHeader[0].AsSpan();
            
            try
            {
                // Parse host directly without full Uri allocation when possible
                ReadOnlySpan<char> host;
                
                if (referer.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                {
                    host = ExtractHost(referer["http://".Length..]);
                }
                else if (referer.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    host = ExtractHost(referer["https://".Length..]);
                }
                else
                {
                    // No scheme - treat as host directly
                    host = ExtractHost(referer);
                }

                if (host.Length > 0 && IsBlocked(host))
                {
                    _logger.LogWarning(
                        "🚫 Referrer spam blocked: {Host} | IP: {IP} | Path: {Path}",
                        host.ToString(),
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
            catch (Exception ex) when (ex is UriFormatException or ArgumentException)
            {
                _logger.LogDebug("Malformed referrer detected: {Referer}", refererHeader[0]);
            }
        }

        await _next(context);
    }

    /// <summary>
    /// Extracts the host from a URL without scheme, handling port and path.
    /// </summary>
    private static ReadOnlySpan<char> ExtractHost(ReadOnlySpan<char> urlWithoutScheme)
    {
        // Find the end of host (first / or : for port, or end of string)
        var endIndex = urlWithoutScheme.IndexOfAny(['/', ':', '?', '#']);
        var host = endIndex >= 0 ? urlWithoutScheme[..endIndex] : urlWithoutScheme;
        
        return host;
    }

    private bool IsBlocked(ReadOnlySpan<char> host)
    {
        // Check TLDs - TLDs already contain the dot (e.g., ".icu")
        if (_options.BlockedTLDs is { Count: > 0 })
        {
            foreach (var tld in _options.BlockedTLDs)
            {
                if (!string.IsNullOrEmpty(tld) && host.EndsWith(tld, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
        }

        // Check exact domains
        if (_options.BlockedDomains is { Count: > 0 })
        {
            // HashSet lookup requires string, but it's O(1)
            var hostString = host.ToString();
            if (_options.BlockedDomains.Contains(hostString))
                return true;

            // Check subdomains - avoid string interpolation
            foreach (var blocked in _options.BlockedDomains)
            {
                if (string.IsNullOrEmpty(blocked))
                    continue;

                // Check if host ends with ".blocked"
                if (host.Length > blocked.Length + 1 &&
                    host[^(blocked.Length + 1)] == '.' &&
                    host[^blocked.Length..].Equals(blocked, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
        }

        // Check patterns
        if (_options.BlockedPatterns is { Count: > 0 })
        {
            foreach (var pattern in _options.BlockedPatterns)
            {
                if (!string.IsNullOrEmpty(pattern) && host.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
        }

        // Check subdomain prefixes (e.g., iqri1., hk12.)
        if (_options.BlockedSubdomainPrefixes is { Count: > 0 })
        {
            foreach (var prefix in _options.BlockedSubdomainPrefixes)
            {
                if (!string.IsNullOrEmpty(prefix) && IsMatchingSubdomainPrefix(host, prefix))
                    return true;
            }
        }

        // Check wildcard patterns (e.g., *crmsoftware*.com, sdk*freegame.top)
        if (_options.BlockedWildcardPatterns is { Count: > 0 })
        {
            foreach (var pattern in _options.BlockedWildcardPatterns)
            {
                if (!string.IsNullOrEmpty(pattern) && MatchesWildcardPattern(host, pattern))
                    return true;
            }
        }

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

    /// <summary>
    /// Checks if host matches a wildcard pattern (* = any characters).
    /// Examples: "*crmsoftware*.com" matches mycrmsoftwarehub.com, crmsoftwareedge.com
    ///           "sdk*freegame.top" matches sdk0freegame.top, sdk7freegame.top
    /// </summary>
    private static bool MatchesWildcardPattern(ReadOnlySpan<char> host, string pattern)
    {
        // Handle simple cases
        if (pattern == "*")
            return true;

        if (!pattern.Contains('*'))
            return host.Equals(pattern, StringComparison.OrdinalIgnoreCase);

        var patternSpan = pattern.AsSpan();
        int hostPos = 0;
        int patternPos = 0;
        int lastWildcardPos = -1;
        int lastHostPos = -1;

        while (hostPos < host.Length)
        {
            if (patternPos < patternSpan.Length && patternSpan[patternPos] == '*')
            {
                // Remember wildcard position for backtracking
                lastWildcardPos = patternPos;
                lastHostPos = hostPos;
                patternPos++;
            }
            else if (patternPos < patternSpan.Length && 
                     (char.ToLowerInvariant(host[hostPos]) == char.ToLowerInvariant(patternSpan[patternPos])))
            {
                // Characters match, advance both
                hostPos++;
                patternPos++;
            }
            else if (lastWildcardPos != -1)
            {
                // Mismatch after wildcard, backtrack
                patternPos = lastWildcardPos + 1;
                lastHostPos++;
                hostPos = lastHostPos;
            }
            else
            {
                // No match and no wildcard to backtrack to
                return false;
            }
        }

        // Skip remaining wildcards in pattern
        while (patternPos < patternSpan.Length && patternSpan[patternPos] == '*')
            patternPos++;

        // Match if we've consumed both host and pattern
        return patternPos == patternSpan.Length;
    }
}