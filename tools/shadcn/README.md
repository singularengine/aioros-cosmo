# tools/shadcn

The reference Cosmo is compared with: shadcn/ui pinned at commit `c2477f311c19982ef6e246085438e6cfec3e35ea`
(2026-10-15, CLI 4.21.5), style new-york-v4.

| Path | What it is |
| --- | --- |
| `registry/ui`, `registry/examples` | shadcn/ui's new-york-v4 component sources and the examples of the ported components, as published at the pinned commit (`apps/v4/registry/new-york-v4`). |
| `registry/radix`, `registry/styles/style-rhea.css` | The radix base's `code-block`, `trace` and `questionnaire` (`apps/v4/registry/bases/radix`: `ui`, `lib/highlighter.tsx`, `hooks/use-highlighted-code.ts`, and the `ui` they import) and their examples (`apps/v4/examples/radix`), with the radix-rhea style (`apps/v4/registry/styles/style-rhea.css`), as published at the pinned commit. `app/src/rhea.tsx` draws them inside `.style-rhea` on new-york-v4's tokens, the style imported in the base layer as shadcn's `app/style-registry.css` does; `app/src/icon-placeholder.tsx` draws the Lucide icon of shadcn's `IconPlaceholder`. Code is highlighted by shiki 3.23.0, as the pinned app. |
| `registry/colors/*.json` | The base colors `shadcn init` writes (`apps/v4/public/r/colors`, member `cssVarsV4`). |
| `registry/_legacy-base-colors.ts` | The colored themes of shadcn's themes page (`baseColorsOKLCH`). |
| `registry/components.txt` | Every component shadcn/ui documents (`apps/v4/content/docs/components/radix`), checked against docs/coverage.md. |
| `registry/LICENSE.md` | shadcn/ui's license. |
| `fonts` | The Inter files Aioros serves, so both renders use the same font (SIL OFL 1.1). |
| `next` | shadcn/ui's `dashboard-01`, `sidebar-01` and `login-01` blocks, their pages unchanged, served by Next.js (the stack shadcn/ui targets) for Cosmo's lab bench (`bench/gallery.sh`): `npm run build:next` renders them on every request into `next/.next`, `NEXT_STATIC=1` prerenders them into `next/.next-static`; `next/cluster.mjs` serves one Next.js server per core. |
| `app` | A Vite + React app rendering each fixture (`app/src/fixtures.tsx`) with the CSS `shadcn init` writes; `?fixture=…&theme=dark&color=blue&radius=0.5rem&font=geist`. |
| `capture.mjs` | Builds nothing: captures `dist` in Chromium into `out` (PNGs and `metrics.json`). |
| `compare.mjs` | Captures a running Gallery's `/fidelity/*` pages and compares them with `out` (`out/compare.json`, `out/summary.json`). |
| `versions.mjs` | Compares the chart examples drawn by Recharts 3.8.0 (`dist`) and 2.15.4 (`dist-recharts2`, `npm run build:recharts2`, captured with `RECHARTS=2 node capture.mjs`) into `out/recharts-versions.json`. |
| `tokens.mjs`, `oklch.mjs` | Generate the token dictionaries, `TokenDictionaries.cs`, `docs/tokens.md` and the tests' `themes.json`. |

```
npm ci
npm run build                  # the reference app into dist/
node capture.mjs               # the reference captures into out/
node tokens.mjs                # the tokens into src/Aioros.Cosmo/Theming and docs/tokens.md
node compare.mjs http://127.0.0.1:3031   # a running Gallery against the reference
```

Chromium is `/usr/bin/chromium` (or `CHROMIUM`). npm is used only here; `node_modules` and `dist` are git-ignored.
