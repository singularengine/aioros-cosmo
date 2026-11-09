# Benchmarks

How Cosmo performs when compared with shadcn/ui on the stack it targets, and the latest results.

## What is measured

The Gallery, published and precompiled, is served by Aioros next to shadcn/ui's own blocks served by Next.js, on the same machine and cores. Next.js is measured two ways: rendering every request, and prerendered at build time, which is the best case for a page with no request data.

Each page gets a closed loop at 64 connections, and a first load in Chromium. The server and the client are pinned to separate cores, and runs are interleaved.

## Latest results

Measured on 2026-10-28, on a self-hosted machine. Requests per second:

| Block | Aioros | Next.js, every request | Next.js, prerendered |
| --- | --- | --- | --- |
| `dashboard-01` | 2,095 | 210 | 2,377 |
| `sidebar-01` | 21,461 | 754 | 4,774 |
| `login-01` | 27,443 | 1,149 | 6,558 |

On the heaviest page, the prerendered Next.js build is ahead. Interactions on `dashboard-01`, from click to visible state, take 98 to 164 ms on Aioros and 77 to 155 ms on Next.js.

## Running it

```bash
(cd tools/shadcn && npm ci)
bench/gallery.sh
RUNS=1 VARIANTS="aioros nextjs" SKIP_BUILD=1 bench/gallery.sh
```

It needs Chromium. Results are written under `bench/results`. On a machine doing other work, compare ratios and not absolute numbers.
