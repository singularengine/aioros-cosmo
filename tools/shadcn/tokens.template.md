# Tokens

shadcn/ui's design tokens as Cosmo's resources. This document is written by `tools/shadcn/tokens.mjs` (from
`tokens.template.md`) together with the dictionaries in `src/Aioros.Cosmo/Theming`; run `node tools/shadcn/tokens.mjs`
after changing the pinned registry.

## Source

- The base colors (neutral, zinc, slate, stone, gray): `public/r/colors/{name}.json`, member `cssVarsV4`, of shadcn/ui
  at commit `c2477f3` (what `shadcn init` writes for the new-york-v4 style). Neutral is the default.
- The colored themes (red, rose, orange, green, blue, yellow, violet): `registry/_legacy-base-colors.ts`, member
  `baseColorsOKLCH` (the themes page's palette: each is zinc with a colored primary, ring, charts and sidebar primary).
- The radius scale: the registry's CSS updater (`--radius-sm: calc(var(--radius) * 0.6)` … `--radius-4xl: × 2.6`).

## Conversion

Every value is an `oklch(L C H [/ A])`. It is converted as CSS Color 4 specifies and as Chromium paints it on an sRGB
display:

1. OKLCH to OKLab: `a = C·cos(H)`, `b = C·sin(H)`.
2. OKLab to linear sRGB through LMS (Björn Ottosson's matrices, as CSS Color 4 gives them):
   `l = (L + 0.3963377774a + 0.2158037573b)³`, `m = (L − 0.1055613458a − 0.0638541728b)³`,
   `s = (L − 0.0894841775a − 1.2914855480b)³`, then
   `R = 4.0767416621l − 3.3077115913m + 0.2309699292s`,
   `G = −1.2684380046l + 2.6097574011m − 0.3413193965s`,
   `B = −0.0041960863l − 0.7034186147m + 1.7076147010s`.
3. The sRGB transfer function: `v ≤ 0.0031308 ? 12.92v : 1.055·v^(1/2.4) − 0.055`.
4. Each channel clipped to 0..1 (colors outside sRGB, such as `--destructive`, are clipped as Chromium clips them; no
   gamut mapping), multiplied by 255 and rounded. The alpha is kept: `oklch(1 0 0 / 10%)` is `#1AFFFFFF` (Avalonia's
   `#AARRGGBB`).

Verified two ways: the C# tests convert the pinned values independently and find the generated resources identical
(TOKENS-01), and Chromium's 2D canvas paints every one of the 744 values (12 themes × 2 variants × 31 tokens) within
1/255 of the conversion (TOKENS-02): 712 identical, 32 one level apart (the canvas stores premultiplied 8-bit values).

## Neutral (the default)

{{tokens}}

## Color themes

{{themes}}

## Component brushes and shadows

shadcn's classes apply opacities to the tokens (`bg-primary/90` is `color-mix(in oklab, var(--primary) 90%,
transparent)`, the color with 90% alpha). Cosmo precomputes them per theme and variant (values for neutral):

{{components}}

## Radius

{{radius}}
