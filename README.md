# ![ReferrerBlock](https://github.com/tossnet/AspNetCore.ReferrerBlock/blob/main/src/AspNetCore.ReferrerBlock/ReferrerBlock.png)

# ReferrerBlock

[![NuGet](https://img.shields.io/nuget/v/AspNetCore.ReferrerBlock.svg)](https://www.nuget.org/packages/ReferrerBlock/)  ![BlazorWinOld Nuget Package](https://img.shields.io/nuget/dt/ReferrerBlock)
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
    });
```