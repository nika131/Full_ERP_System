# TenexERP — Landing page & search discovery update

Prepared from **`nika131/Full_ERP_System` `main` at `10b567b` (October 31, 2026)**.

## What it changes

- Centers hero CTA buttons and quick benefits at mobile/tablet widths; keeps desktop two-column alignment.
- Consistent **14 days of full feature access** offer across Landing, Register and Companies screens. Removes sales copy about unfinished billing.
- Gives the site a proper TenexERP title, description, canonical URL, Open Graph and social media metadata.
- Adds the TenexERP icon in SVG, PNG, ICO, and Apple touch sizes, plus a 1200x630 share image.
- Adds `robots.txt`, `sitemap.xml`, `llms.txt` and a public HTML overview for readers/crawlers not executing JavaScript.
- Corrects old **NEXUS ERP** branding on the legal-page header.

## Applying

Extract this directory into the repository root (the directory containing `NexusERP`). In PowerShell:

```powershell
python .\tenexerp-website-discovery-v1\apply.py --dry-run
python .\tenexerp-website-discovery-v1\apply.py
cd NexusERP\nexus-erp-client
npm ci
npm run build
```

If successful, from repository root:

```powershell
git status
git add NexusERP/nexus-erp-client
# Review: git diff --stat && git diff
git commit -m "feat(web): polish landing page and improve search discovery"
git push origin main
```

**The installer refuses to overwrite existing different assets** or to apply a replacement when the expected source doesn't match. It stages edits in memory before writing, so a validation error doesn't leave a half-applied patch. The script is safe to rerun.

## After Azure deployment

Verify these addresses serve a real asset / HTML instead of the SPA fallback page:

- https://tenexerp.com/
- https://tenexerp.com/favicon.ico
- https://tenexerp.com/icon-96.png
- https://tenexerp.com/og-tenexerp.png
- https://tenexerp.com/robots.txt
- https://tenexerp.com/sitemap.xml
- https://tenexerp.com/business-management.html
- https://tenexerp.com/llms.txt

Next, verify `tenexerp.com` in [Google Search Console](https://search.google.com/search-console), submit `/sitemap.xml`, and use URL Inspection → Request Indexing for the homepage. Confirm your production canonical domain resolves, other variants redirect to it, and DNS is correct. Check social preview with Facebook Sharing Debugger / LinkedIn Post Inspector.

Search rankings, search snippets, favicons and recommendations from AI assistants are controlled by third parties; the metadata and crawlable pages improve eligibility and comprehension but cannot guarantee placement, timescales, or endorsements. `llms.txt` is informational, not an enforced crawler standard. SEO for the main SPA marketing content would be further improved by future server-side/static prerendering. Private app routes must still enforce login and authorization; robots.txt is not security.

### Offer accuracy

This package changes **marketing wording only**, not subscription enforcement. Before using the 14-day offer publicly, make sure the backend and terms actually support it. No paid-billing availability is claimed.

## Scope

The changes are limited to public marketing and metadata. No ERP business logic, backend permissions, or POS transactions are modified.
