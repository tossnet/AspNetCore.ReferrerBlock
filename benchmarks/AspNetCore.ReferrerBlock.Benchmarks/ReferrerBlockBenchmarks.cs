using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace AspNetCore.ReferrerBlock.Benchmarks;

/// <summary>
/// Benchmarks comparing the original Uri-based parsing vs optimized Span-based parsing
/// for the ReferrerBlock middleware.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class ReferrerBlockBenchmarks
{
    private readonly HashSet<string> _blockedTLDs = new(StringComparer.OrdinalIgnoreCase)
    {
        ".icu", ".xyz", ".biz.id", ".co.id", ".in"
    };

    private readonly HashSet<string> _blockedDomains = new(StringComparer.OrdinalIgnoreCase)
    {
        "spam.com", "malware.net", "bad-actor.org"
    };

    private readonly HashSet<string> _blockedPatterns = new(StringComparer.OrdinalIgnoreCase)
    {
        "cekitchenware", "crmlogichub", "followpathcrm"
    };

    private readonly HashSet<string> _blockedSubdomainPrefixes = new(StringComparer.OrdinalIgnoreCase)
    {
        "iqri", "hk", "ag", "san"
    };

    private string[] _testReferrers = null!;

    [GlobalSetup]
    public void Setup()
    {
        _testReferrers =
        [
            // Legitimate referrers (should pass through)
            "https://www.google.com/search?q=test",
            "https://github.com/user/repo",
            "http://stackoverflow.com/questions/12345",
            "https://docs.microsoft.com/en-us/dotnet/",
            
            // Blocked by TLD
            "https://spam-site.icu/page",
            "https://malicious.xyz",
            "http://bad.biz.id/track",
            
            // Blocked by domain
            "https://spam.com/tracker",
            "http://malware.net/payload",
            
            // Blocked by pattern
            "https://my-cekitchenware-shop.com/",
            "http://crmlogichub-analytics.net/track",
            
            // Blocked by subdomain prefix
            "https://iqri1.somesite.com/",
            "http://hk18.tracker.net/pixel",
            "https://ag.spammer.org/",
            
            // Edge cases
            "example.com",  // No scheme
            "https://[::1]:8080/path",  // IPv6
            "",  // Empty
        ];
    }

    [Benchmark(Baseline = true)]
    public int Original_UriParsing()
    {
        int blockedCount = 0;
        
        foreach (var referer in _testReferrers)
        {
            if (IsBlocked_Original(referer))
                blockedCount++;
        }
        
        return blockedCount;
    }

    [Benchmark]
    public int Optimized_SpanParsing()
    {
        int blockedCount = 0;
        
        foreach (var referer in _testReferrers)
        {
            if (IsBlocked_Optimized(referer))
                blockedCount++;
        }
        
        return blockedCount;
    }

    #region Original Implementation (Uri-based)

    private bool IsBlocked_Original(string referer)
    {
        if (string.IsNullOrEmpty(referer))
            return false;

        try
        {
            // Original: add schema if missing
            if (!referer.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !referer.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                referer = "https://" + referer;
            }

            var uri = new Uri(referer);
            var host = uri.Host.ToLowerInvariant();

            return IsHostBlocked_Original(host);
        }
        catch (UriFormatException)
        {
            return false;
        }
    }

    private bool IsHostBlocked_Original(string host)
    {
        // Check TLDs
        if (_blockedTLDs.Any(tld => !string.IsNullOrEmpty(tld) &&
            host.EndsWith(tld, StringComparison.OrdinalIgnoreCase)))
            return true;

        // Check exact domains
        if (_blockedDomains.Contains(host))
            return true;

        // Check subdomains (with string interpolation)
        if (_blockedDomains.Any(blocked => !string.IsNullOrEmpty(blocked) &&
            host.EndsWith($".{blocked}", StringComparison.OrdinalIgnoreCase)))
            return true;

        // Check patterns
        if (_blockedPatterns.Any(pattern => !string.IsNullOrEmpty(pattern) &&
            host.Contains(pattern, StringComparison.OrdinalIgnoreCase)))
            return true;

        // Check subdomain prefixes
        if (_blockedSubdomainPrefixes.Any(prefix => !string.IsNullOrEmpty(prefix) &&
            IsMatchingSubdomainPrefix_Original(host, prefix)))
            return true;

        return false;
    }

    private static bool IsMatchingSubdomainPrefix_Original(string host, string prefix)
    {
        if (!host.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return false;

        var remaining = host.AsSpan()[prefix.Length..];
        int i = 0;
        while (i < remaining.Length && char.IsAsciiDigit(remaining[i]))
            i++;

        return i < remaining.Length && remaining[i] == '.';
    }

    #endregion

    #region Optimized Implementation (Span-based)

    private bool IsBlocked_Optimized(string referer)
    {
        if (string.IsNullOrEmpty(referer))
            return false;

        var refererSpan = referer.AsSpan();
        ReadOnlySpan<char> host;

        if (refererSpan.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
        {
            host = ExtractHost(refererSpan["http://".Length..]);
        }
        else if (refererSpan.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            host = ExtractHost(refererSpan["https://".Length..]);
        }
        else
        {
            host = ExtractHost(refererSpan);
        }

        if (host.Length == 0)
            return false;

        return IsHostBlocked_Optimized(host);
    }

    private static ReadOnlySpan<char> ExtractHost(ReadOnlySpan<char> urlWithoutScheme)
    {
        var endIndex = urlWithoutScheme.IndexOfAny(['/', ':', '?', '#']);
        return endIndex >= 0 ? urlWithoutScheme[..endIndex] : urlWithoutScheme;
    }

    private bool IsHostBlocked_Optimized(ReadOnlySpan<char> host)
    {
        // Check TLDs
        foreach (var tld in _blockedTLDs)
        {
            if (!string.IsNullOrEmpty(tld) && host.EndsWith(tld, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        // Check exact domains (need string for HashSet lookup)
        var hostString = host.ToString();
        if (_blockedDomains.Contains(hostString))
            return true;

        // Check subdomains - avoid string interpolation
        foreach (var blocked in _blockedDomains)
        {
            if (string.IsNullOrEmpty(blocked))
                continue;

            if (host.Length > blocked.Length + 1 &&
                host[^(blocked.Length + 1)] == '.' &&
                host[^blocked.Length..].Equals(blocked, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        // Check patterns
        foreach (var pattern in _blockedPatterns)
        {
            if (!string.IsNullOrEmpty(pattern) && host.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        // Check subdomain prefixes
        foreach (var prefix in _blockedSubdomainPrefixes)
        {
            if (!string.IsNullOrEmpty(prefix) && IsMatchingSubdomainPrefix_Optimized(host, prefix))
                return true;
        }

        return false;
    }

    private static bool IsMatchingSubdomainPrefix_Optimized(ReadOnlySpan<char> host, string prefix)
    {
        if (!host.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return false;

        var remaining = host[prefix.Length..];
        int i = 0;
        while (i < remaining.Length && char.IsAsciiDigit(remaining[i]))
            i++;

        return i < remaining.Length && remaining[i] == '.';
    }

    #endregion
}
