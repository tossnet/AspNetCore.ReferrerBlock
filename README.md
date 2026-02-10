# ![ReferrerBlock](https://github.com/tossnet/AspNetCore.ReferrerBlock/blob/main/src/AspNetCore.ReferrerBlock/ReferrerBlock.png)

# ReferrerBlock

[![NuGet](https://img.shields.io/nuget/v/ReferrerBlock.svg)](https://www.nuget.org/packages/ReferrerBlock/)  ![BlazorWinOld Nuget Package](https://img.shields.io/nuget/dt/ReferrerBlock)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

ReferrerBlock middleware to block referrer spam and malicious traffic.

## ⚙️ Usage
```csharp
var builder = WebApplication.CreateBuilder(args); var app = builder.Build();

app.UseReferrerBlock();

app.Run();
```

The middleware uses default blocking rules. Optionally, you can customize them:
```csharp
app.UseReferrerBlock(options => { 
    options.BlockedDomains.Add("spam-site.com"); 
    options.BlockedTLDs.Add(".suspicious"); 
    options.BlockedPatterns.Add("malicious");
    options.BlockedSubdomainPrefixes.Add("spam");
    });
```

## 📝 Examples

### Disable default TLD blocking
```csharp
app.UseReferrerBlock(options => 
{ 
    options.BlockedTLDs.Clear(); // Remove all default TLDs
    options.BlockedDomains.Add("spam-site.com"); 
});
```

### Use only custom rules
```csharp
app.UseReferrerBlock(options => 
{ 
    // Clear all default rules
    options.BlockedTLDs.Clear();
    options.BlockedDomains.Clear();
    options.BlockedPatterns.Clear();
    options.BlockedSubdomainPrefixes.Clear();
    
    // Add only your custom rules
    options.BlockedDomains.Add("spam-site.com");
    options.BlockedDomains.Add("malicious-domain.com");
    options.BlockedTLDs.Add(".scam");
    options.BlockedPatterns.Add("suspicious");
    options.BlockedSubdomainPrefixes.Add("bot");
});
```

### Combine default rules with custom ones
```csharp
app.UseReferrerBlock(options => 
{ 
    // Keep default rules and add custom ones
    options.BlockedDomains.Add("spam-site.com"); 
    options.BlockedTLDs.Add(".suspicious"); 
    options.BlockedPatterns.Add("malicious");
    options.BlockedSubdomainPrefixes.Add("bot");
});
```

### Block subdomain prefixes with numeric variations
The `BlockedSubdomainPrefixes` option allows you to block subdomains that start with a specific prefix followed by optional digits.

```csharp
app.UseReferrerBlock(options => 
{ 
    // Block subdomains like: iqri., iqri1., iqri18., hk., hk1., hk12., etc.
    options.BlockedSubdomainPrefixes.Add("iqri");
    options.BlockedSubdomainPrefixes.Add("hk");
    options.BlockedSubdomainPrefixes.Add("spam");
});
```

This will block referrers like:
- `iqri.example.com` ✅ blocked
- `iqri1.spammer.net` ✅ blocked
- `iqri18.malicious.org` ✅ blocked
- `hk12.badsite.com` ✅ blocked

But will NOT block:
- `iqri1x.example.com` ❌ not blocked (has letters after digits)
- `myiqri1.example.com` ❌ not blocked (prefix not at start)
- `iqrisite.com` ❌ not blocked (in domain name, not subdomain)

### Block domains with wildcard patterns
The `BlockedWildcardPatterns` option allows you to block domains using wildcard patterns where `*` matches any characters.

```csharp
app.UseReferrerBlock(options => 
{ 
    // Block patterns like: *crmsoftware*.com, sdk*freegame.top
    options.BlockedWildcardPatterns.Add("*crmsoftware*.com");
    options.BlockedWildcardPatterns.Add("sdk*freegame.top");
    options.BlockedWildcardPatterns.Add("*spam*.net");
});
```

Examples of what gets blocked:
- `*crmsoftware*.com` blocks:
  - `crmsoftwareedge.com` ✅ blocked
  - `crmsoftwarefocus.com` ✅ blocked
  - `mycrmsoftwarehub.com` ✅ blocked
  - `testcrmsoftware.com` ✅ blocked
  - But NOT `crmsoftwareedge.net` ❌ (different TLD)

- `sdk*freegame.top` blocks:
  - `sdk0freegame.top` ✅ blocked
  - `sdk3freegame.top` ✅ blocked
  - `sdk7freegame.top` ✅ blocked
  - `sdkanyfreegame.top` ✅ blocked
  - But NOT `sdkfreegame.com` ❌ (different TLD)

- `*spam*.net` blocks:
  - `spam.net` ✅ blocked
  - `myspamsite.net` ✅ blocked
  - `spamnetwork.net` ✅ blocked
  - `test-spam-tools.net` ✅ blocked

**Why use wildcard patterns instead of simple patterns?**
- Simple patterns (BlockedPatterns) use `Contains()` and match anywhere in any TLD
- Wildcard patterns give you precise control with specific TLD requirements
- Example: `"crmsoftware"` in BlockedPatterns would block ALL TLDs (.com, .net, .org, etc.)
- Example: `"*crmsoftware*.com"` in BlockedWildcardPatterns blocks ONLY .com domains

## 🚀 Performance

The middleware is optimized for high-performance scenarios using `ReadOnlySpan<char>` instead of traditional `Uri` parsing, resulting in minimal memory allocations.

### Benchmark Results

```
BenchmarkDotNet v0.15.8, Windows 11, Intel Core i9-9900K CPU 3.60GHz
.NET 9.0.12, X64 RyuJIT x86-64-v3
```

| Method | Mean | Allocated | Improvement |
|--------|------|-----------|-------------|
| ✅ Optimized (Span) | **1.243 µs** | **704 B** | Baseline |
| ❌ Original (Uri) | 6.892 µs | 9,952 B | - |

| Metric | Gain |
|--------|------|
| **Speed** | **5.5x faster** |
| **Memory** | **14x less allocations** |

Run benchmarks yourself:
```bash
cd benchmarks/AspNetCore.ReferrerBlock.Benchmarks
dotnet run -c Release
```

## 📊 Blocked Domains

See [BLOCKED_DOMAINS.md](BLOCKED_DOMAINS.md) for the complete list of blocked domains, TLDs, and patterns with their addition history.
