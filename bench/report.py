"""Cosmo's lab bench report (bench/gallery.sh): medians over the runs of each variant, and Aioros beside Next.js on the
pages both serve (shadcn/ui's blocks). usage: python3 bench/report.py <results folder> > GALLERY.md"""
import json, statistics, sys
from collections import defaultdict
from pathlib import Path

folder = Path(sys.argv[1])
runs = [json.loads(l) for l in (folder / "gallery.jsonl").read_text().splitlines() if l.strip()]
env = json.loads((folder / "env.json").read_text())
precompile = json.loads((folder / "precompile.json").read_text()) if (folder / "precompile.json").exists() else None
med = lambda xs: statistics.median(xs) if xs else None
fmt = lambda x, d=0: "-" if x is None else f"{x:,.{d}f}"

by = defaultdict(list)
for r in runs: by[r["name"]].append(r)
order = [n for n in ["aioros", "aioros-base", "aioros-engine", "nextjs", "nextjs-static"] if n in by]

def page(name, key, field, sub=None):
    xs = [p[field][sub] if sub else p[field] for r in by[name] for p in r["pages"] if p["key"] == key and p[field] is not None]
    return med(xs)

print("# Cosmo lab bench\n")
print(f"{env['cpu']}, {env['cores']} threads, {env['memGb']} GB; .NET {env['dotnet']}, Node {env['node']}, Next.js {env['next']}, Aioros {env['aioros']}; "
      f"commit {env['git']}; server CPUs {env['serverCpus']}, client {env['clientCpus']}, {env['nextWorkers']} Next.js workers; {env['http']}.\n")
if precompile:
    print(f"Precompile: {precompile['ms'] / 1000:.1f} s, plans file {precompile['plansBytes'] / 1048576:.1f} MB. " + " ".join(precompile["report"][:2]) + "\n")

print("## Servers\n")
print("| Variant | Runs | Cold start ms | Warm-up ms | Idle MB |")
print("| --- | --- | --- | --- | --- |")
for n in order:
    rs = by[n]
    print(f"| {n} | {len(rs)} | {fmt(med([r['coldStartMs'] for r in rs]))} | {fmt(med([r['warmupMs'] for r in rs if r['warmupMs'] is not None]))} | {fmt(med([r['idleRssMb'] for r in rs]))} |")

print("\n## shadcn/ui's blocks: Aioros beside Next.js\n")
print("Closed loop, medians over the runs; weight as served (`Accept-Encoding: br, gzip`).\n")
print("| Block | Variant | Status | HTML | Served | req/s | p50 ms | p99 ms | Peak MB |")
print("| --- | --- | --- | --- | --- | --- | --- | --- | --- |")
shared = [k for k in ["dashboard-01", "sidebar-01", "login-01"]]
for key in shared:
    for n in order:
        if not any(p["key"] == key for r in by[n] for p in r["pages"]): continue
        enc = next((p["encoding"] for r in by[n] for p in r["pages"] if p["key"] == key), None)
        print(f"| {key} | {n} | {fmt(page(n, key, 'status'))} | {fmt(page(n, key, 'html'))} B | {fmt(page(n, key, 'served'))} B {enc or ''} | {fmt(page(n, key, 'c64', 'rps'))} | {fmt(page(n, key, 'c64', 'p50'), 2)} | {fmt(page(n, key, 'c64', 'p99'), 2)} | {fmt(page(n, key, 'c64', 'peakRssMb'))} |")
print("\n| Block | Aioros / Next.js (every request) req/s | Aioros / Next.js (prerendered) req/s |")
print("| --- | --- | --- |")
for key in shared:
    a, d, s = page("aioros", key, "c64", "rps"), page("nextjs", key, "c64", "rps"), page("nextjs-static", key, "c64", "rps")
    print(f"| {key} | {fmt(a / d, 1) + 'x' if a and d else '-'} | {fmt(a / s, 1) + 'x' if a and s else '-'} |")

print("\n## The Gallery's own pages (Aioros)\n")
print("| Page | Variant | Status | HTML | Served | req/s | p50 ms | p99 ms | Peak MB |")
print("| --- | --- | --- | --- | --- | --- | --- | --- | --- |")
keys = [p["key"] for p in by["aioros"][0]["pages"] if p["key"] not in shared] if "aioros" in by else []
for key in keys:
    for n in [x for x in order if x.startswith("aioros")]:
        path = next(p["page"] for r in by[n] for p in r["pages"] if p["key"] == key)
        enc = next((p["encoding"] for r in by[n] for p in r["pages"] if p["key"] == key), None)
        print(f"| `{path}` | {n} | {fmt(page(n, key, 'status'))} | {fmt(page(n, key, 'html'))} B | {fmt(page(n, key, 'served'))} B {enc or ''} | {fmt(page(n, key, 'c64', 'rps'))} | {fmt(page(n, key, 'c64', 'p50'), 2)} | {fmt(page(n, key, 'c64', 'p99'), 2)} | {fmt(page(n, key, 'c64', 'peakRssMb'))} |")

print("\n## In Chromium\n")
print("First loads in a fresh context, 1400 x 900, no throttling; medians.\n")
print("| Variant | Page | Status | KB | JS KB | TTFB | FCP | LCP | TBT | CLS |")
print("| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |")
for n in order:
    b = next((r["browser"] for r in by[n] if r.get("browser")), None)
    for path, m in (b or {}).get("pages", {}).items():
        print(f"| {n} | `{path}` | {m['status']} | {m['kb']} | {m['jsKb']} | {m['ttfb']} | {m['fcp']} | {m['lcp']} | {m['tbt']} | {m['cls']} |")
print("\ndashboard-01's interactions, from the click to the state shown (ms), its INP, and on Aioros the post (status, server wait):\n")
print("| Variant | Interaction | ms | INP | Post | Server wait ms | Errors |")
print("| --- | --- | --- | --- | --- | --- | --- |")
for n in order:
    b = next((r["browser"] for r in by[n] if r.get("browser")), None)
    for step, m in (b or {}).get("interactions", {}).items():
        post = m.get("post") or {}
        print(f"| {n} | {step} | {fmt(m['ms'])} | {fmt(m['inp'])} | {','.join(map(str, post.get('status', []))) or '-'} | {fmt(post.get('waitMs'))} | {'; '.join(m['errors'])[:120] or '-'} |")
