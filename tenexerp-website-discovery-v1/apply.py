"""TenexERP marketing + crawlability patch, prepared against GitHub main 10b567b.
Run from repository root: python tenexerp-website-discovery-v1/apply.py
Staged in memory and verified before writing. No package dependencies required.
"""
from __future__ import annotations

import argparse
from pathlib import Path

HERE = Path(__file__).resolve().parent
WEBSITE = Path('NexusERP/nexus-erp-client')
CURRENT_INDEX_TITLE = '<title>nexus-erp-client</title>'


def single(text: str, old: str, new: str, file_name: str) -> str:
    count = text.count(old)
    if new in text:
        return text  # idempotent even when original anchor is part of the replacement
    if count != 1:
        raise RuntimeError(f'{file_name}: expected one anchor, found {count}: {old[:105]!r}')
    return text.replace(old, new, 1)


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument('--dry-run', action='store_true', help='Validate without writing anything')
    args = parser.parse_args()
    base = Path.cwd()
    if not (base / WEBSITE / 'src/pages/Landing.tsx').exists():
        raise SystemExit('Run from the repository root (folder containing NexusERP).')

    staged: dict[Path, bytes] = {}

    def patch(relative: str, changes: list[tuple[str, str]]) -> None:
        target = base / WEBSITE / relative
        original = target.read_text(encoding='utf-8-sig')
        result = original
        for before, after in changes:
            result = single(result, before, after, relative)
        if result != original:
            staged[target] = result.encode('utf-8')

    patch('src/pages/Landing.tsx', [
        (
            "['Is TenexERP only for large companies?', 'Not at all. TenexERP is being built with small and growing businesses in mind. Start with the tools you need and add more as your team grows.'],",
            "['Is TenexERP only for large companies?', 'No. TenexERP is designed for small businesses and growing teams, with inventory, sales and team management in one workspace.'],",
        ),
        (
            "['Do I need technical experience?', 'No specialist setup knowledge is required to explore the platform. We focus on a clear workspace and familiar business workflows.'],",
            "['Do I need technical experience?', 'No. Set up your company, organize stores and products, and bring your team on board with straightforward workflows.'],",
        ),
        (
            "['What happens after the introductory month?', 'Paid plans are planned but not available yet. We’ll explain the choices and terms before any subscription or charge begins.'],",
            "['What do I get during my first 14 days?', 'Full access to TenexERP features, including inventory management, point of sale, stores, sales insights and employee permissions.'],",
        ),
        (
            "['Does it work with my staff?', 'Yes. You can invite colleagues to your company and decide what each role is allowed to view or change.'],",
            "['Can my team use it too?', 'Yes. Add teammates, set permissions for their roles, and give in-store cashiers access to the POS experience.'],",
        ),
        ('<div className="relative"><div className="mb-7 inline-flex', '<div className="relative text-center lg:text-left"><div className="mb-7 inline-flex'),
        ('<h1 className="max-w-2xl text-[clamp(3rem,6.6vw,6.5rem)]', '<h1 className="mx-auto max-w-2xl lg:mx-0 text-[clamp(3rem,6.6vw,6.5rem)]'),
        ('<p className="mt-8 max-w-xl text-lg leading-8', '<p className="mx-auto mt-8 max-w-xl text-lg leading-8 lg:mx-0'),
        ('<div className="mt-9 flex flex-wrap items-center gap-3">', '<div className="mt-9 flex flex-wrap items-center justify-center gap-3 lg:justify-start">'),
        ('className="inline-flex items-center gap-3 rounded-2xl bg-[#123f33] px-7 py-4 font-bold text-white', 'className="inline-flex w-full items-center justify-center gap-3 rounded-2xl bg-[#123f33] px-7 py-4 font-bold text-white sm:w-auto'),
        ('className="rounded-2xl border border-[#cfdbd0] bg-white px-7 py-4 font-bold transition', 'className="inline-flex w-full items-center justify-center rounded-2xl border border-[#cfdbd0] bg-white px-7 py-4 font-bold transition sm:w-auto'),
        ('<div className="mt-7 flex flex-wrap items-center gap-5 text-sm font-medium text-[#597166]">', '<div className="mt-7 flex flex-wrap items-center justify-center gap-x-5 gap-y-3 text-sm font-medium text-[#597166] lg:justify-start">'),
        ('/> Introductory month</span>', '/> 14 days of full access</span>'),
        ('/> No payment setup yet</span>', '/> Every feature included</span>'),
        ('/> Start small</span>', '/> Built for growing teams</span>'),
        ("We're building practical software for real people running real businesses.", 'Made for the people who keep businesses moving.'),
        ('Get started without payment setup', '14 days. Every feature.'),
        ('Discover what it feels like when the little things run smoother. Early-access trial terms will be confirmed in your account as we launch subscriptions.', 'Explore inventory, point of sale, stores, sales insights and team permissions together with 14 days of full access.'),
        ('className="grid grid-cols-2 gap-x-6 gap-y-4 text-sm font-semibold text-slate-600 sm:grid-cols-4"', 'className="grid grid-cols-2 gap-x-6 gap-y-4 text-sm font-semibold text-slate-600 sm:grid-cols-5"'),
        ('<Link to="/login" className="inline-flex items-center gap-2 whitespace-nowrap transition hover:text-emerald-700 focus-visible:underline"><LogIn size={17} aria-hidden="true"/> Log in</Link>', '<Link to="/login" className="inline-flex items-center gap-2 whitespace-nowrap transition hover:text-emerald-700 focus-visible:underline"><LogIn size={17} aria-hidden="true"/> Log in</Link>\n          <a href="/business-management.html" className="inline-flex items-center gap-2 whitespace-nowrap transition hover:text-emerald-700 focus-visible:underline"><Boxes size={17} aria-hidden="true"/> Overview</a>'),
    ])

    patch('src/pages/Register.tsx', [
        ('🎁 Welcome offer: one month of full access', 'Your first 14 days: full feature access'),
        ('We’re preparing an introductory month with access to all features. Subscription and billing aren’t enabled yet; we’ll explain the exact start date and terms when the offer becomes active.', 'Explore inventory, POS, store management, sales insights and team permissions with full access for your first 14 days.'),
    ])

    patch('src/pages/Companies.tsx', [
        ('Your first 30 days are on us.', 'Your first 14 days are yours to explore.'),
        ('Explore the full TenexERP experience with no credit card. Future subscription options are coming later.', 'Get full access to inventory, POS, stores, reporting and team features during your first 14 days.'),
    ])

    patch('src/pages/LegalPage.tsx', [
        ('to="/login"\n                        className="text-xl font-bold tracking-wide text-emerald-600"', 'to="/"\n                        className="text-xl font-bold tracking-wide text-emerald-600"'),
        ('NEXUS ERP', 'TenexERP'),
    ])

    target = base / WEBSITE / 'index.html'
    original = target.read_text(encoding='utf-8-sig')
    new_index = (HERE / 'files' / WEBSITE / 'index.html').read_text(encoding='utf-8')
    if original != new_index:
        if CURRENT_INDEX_TITLE not in original or '<script type="module" src="/src/main.tsx"></script>' not in original:
            raise RuntimeError('index.html: unexpected website version; review your existing SEO tags before applying this patch')
        staged[target] = new_index.encode('utf-8')

    public = HERE / 'files' / WEBSITE / 'public'
    for source in public.iterdir():
        if not source.is_file():
            continue
        target = base / WEBSITE / 'public' / source.name
        new = source.read_bytes()
        if target.exists():
            if target.read_bytes() != new:
                raise RuntimeError(f'{target} exists with different contents; review before overwriting')
        else:
            staged[target] = new

    print('Validation successful. Files to create/update:')
    for file in staged:
        print(' +', file.relative_to(base))
    if args.dry_run:
        print('Dry run only; no files were changed.')
        return
    for target, contents in staged.items():
        target.parent.mkdir(parents=True, exist_ok=True)
        target.write_bytes(contents)
    print(f'Applied {len(staged)} files. Run npm ci && npm run build in NexusERP/nexus-erp-client.')


if __name__ == '__main__':
    main()
