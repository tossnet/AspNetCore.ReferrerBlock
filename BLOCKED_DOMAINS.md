# Blocked Domains Registry

This file documents spam/malicious domains, TLDs, and patterns blocked by default in ReferrerBlock middleware.

## Purpose
- **Transparency**: Understand why domains are blocked
- **Maintenance**: Track when domains were added
- **Contribution**: Help reviewers validate new additions

## Reporting Issues
If a domain is incorrectly blocked, please [open an issue](https://github.com/tossnet/AspNetCore.ReferrerBlock/issues).

## WHOIS Lookup
Use [whois.com](https://www.whois.com/) or similar services to investigate suspicious domains.

---

## Blocked Items

| Type | Value | Added | Reason |
|------|-------|-------|--------|
| Domain | (hk n.)airplanesupdate.com | 2025-11-23 | Referrer spam. Multiple subdomain variants) |
| Domain | periodcostume.top | 2025-11-23 | Referrer spam |
| Domain | globalinsuranceresourcecenter.com | 2025-11-23 | Referrer spam |
| Domain | liveasiannews.com | 2025-11-24 | Referrer spam |
| Domain | indonesiamapan.com | 2025-11-24 | Referrer spam |
| Domain | biayakuliah.id | 2025-11-24 | Referrer spam |
| Domain | iklanbarisposkota.top | 2025-11-24 | Referrer spam |
| Domain | chfaloans.com | 2025-11-24 | Referrer spam |
| Domain | wagnerdom.local | 2025-11-25 | Referrer spam |
| Domain | brandipphoto.com | 2025-11-26 | Referrer spam |
| Domain | (glad., eth.)teknowarta.com | 2025-11-26 | Referrer spam. Multiple subdomain variants |
| Domain | def-atk.com | 2025-11-26 | Referrer spam |
| Domain | (tools.)genshindb.org | 2025-11-26 | Referrer spam |
| Domain | hellyeahomeland.com | 2025-11-26 | Referrer spam |
| Domain | petalsearch.com | 2025-11-26 | Referrer spam |
| Domain | (hk n.)seohost.us | 2025-11-27 | Referrer spam. Multiple subdomain variants |
| Domain | (sports.)nativeindonesia.com | 2025-11-27 | Referrer spam |
| Domain | (hk n.)selectaproperty.us | 2025-11-28 | Referrer spam. Multiple subdomain variants |
| Domain | (iqri n.)pauljimandjoespodcast.com | 2025-11-28 | Referrer spam. Multiple subdomain variants |
| Domain | thebirdlore.com | 2025-11-28 | Referrer spam |
| Domain | porosjambimedia.com | 2025-11-28 | Referrer spam |
| Domain | (iqri n.)mitsubishiprice.com | 2025-11-30 | Referrer spam. Multiple subdomain variants |
| Domain | lovesyh.com | 2025-12-01 | Referrer spam |
| Domain | tipgalore.com | 2025-12-01 | Referrer spam |
| Domain | vtpcys.com | 2025-12-02 | Referrer spam |
| Domain | scfxdl.com | 2025-12-03 | Referrer spam |
| Domain | roomvivo.com | 2025-12-03 | Referrer spam |
| Domain | wavpn.click | 2025-12-03 | Referrer spam |
| Domain | crminsightspro.com | 2025-12-05 | Referrer spam |
| Domain | aaruslan.soldatiki.info | 2025-12-05 | Referrer spam |
| Domain | vip.vivarecharge.com | 2025-12-05 | Referrer spam |
| Domain | fintech.syrfy.com | 2025-12-05 | Referrer spam |
| Domain | jy47.top | 2025-12-05 | Referrer spam |


---

## How to Contribute

1. Verify the domain/pattern is actually spam (check analytics, WHOIS)
2. Add entry to the table with current date
3. Submit a pull request with evidence/reasoning
4. Keep entries sorted alphabetically by value within each type

**Types:**
- **Domain**: Specific domain names (e.g., `spam-site.com`)
- **TLD**: Top-level domains (e.g., `.xyz`, `.top`)
- **Pattern**: Keyword patterns in referrer URLs (e.g., `casino`, `viagra`)