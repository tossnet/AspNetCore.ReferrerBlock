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

## 📊 Blocked Domains

See [BLOCKED_DOMAINS.md](BLOCKED_DOMAINS.md) for the complete list of blocked domains, TLDs, and patterns with their addition history.
