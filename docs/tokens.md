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

| Token | Resource | Light (oklch) | Light (sRGB) | Dark (oklch) | Dark (sRGB) |
| --- | --- | --- | --- | --- | --- |
| `--background` | `CosmoBackgroundColor`, `CosmoBackgroundBrush` | `oklch(1 0 0)` | `#FFFFFF` | `oklch(0.145 0 0)` | `#0A0A0A` |
| `--foreground` | `CosmoForegroundColor`, `CosmoForegroundBrush` | `oklch(0.145 0 0)` | `#0A0A0A` | `oklch(0.985 0 0)` | `#FAFAFA` |
| `--card` | `CosmoCardColor`, `CosmoCardBrush` | `oklch(1 0 0)` | `#FFFFFF` | `oklch(0.205 0 0)` | `#171717` |
| `--card-foreground` | `CosmoCardForegroundColor`, `CosmoCardForegroundBrush` | `oklch(0.145 0 0)` | `#0A0A0A` | `oklch(0.985 0 0)` | `#FAFAFA` |
| `--popover` | `CosmoPopoverColor`, `CosmoPopoverBrush` | `oklch(1 0 0)` | `#FFFFFF` | `oklch(0.205 0 0)` | `#171717` |
| `--popover-foreground` | `CosmoPopoverForegroundColor`, `CosmoPopoverForegroundBrush` | `oklch(0.145 0 0)` | `#0A0A0A` | `oklch(0.985 0 0)` | `#FAFAFA` |
| `--primary` | `CosmoPrimaryColor`, `CosmoPrimaryBrush` | `oklch(0.205 0 0)` | `#171717` | `oklch(0.922 0 0)` | `#E5E5E5` |
| `--primary-foreground` | `CosmoPrimaryForegroundColor`, `CosmoPrimaryForegroundBrush` | `oklch(0.985 0 0)` | `#FAFAFA` | `oklch(0.205 0 0)` | `#171717` |
| `--secondary` | `CosmoSecondaryColor`, `CosmoSecondaryBrush` | `oklch(0.97 0 0)` | `#F5F5F5` | `oklch(0.269 0 0)` | `#262626` |
| `--secondary-foreground` | `CosmoSecondaryForegroundColor`, `CosmoSecondaryForegroundBrush` | `oklch(0.205 0 0)` | `#171717` | `oklch(0.985 0 0)` | `#FAFAFA` |
| `--muted` | `CosmoMutedColor`, `CosmoMutedBrush` | `oklch(0.97 0 0)` | `#F5F5F5` | `oklch(0.269 0 0)` | `#262626` |
| `--muted-foreground` | `CosmoMutedForegroundColor`, `CosmoMutedForegroundBrush` | `oklch(0.556 0 0)` | `#737373` | `oklch(0.708 0 0)` | `#A1A1A1` |
| `--accent` | `CosmoAccentColor`, `CosmoAccentBrush` | `oklch(0.97 0 0)` | `#F5F5F5` | `oklch(0.269 0 0)` | `#262626` |
| `--accent-foreground` | `CosmoAccentForegroundColor`, `CosmoAccentForegroundBrush` | `oklch(0.205 0 0)` | `#171717` | `oklch(0.985 0 0)` | `#FAFAFA` |
| `--destructive` | `CosmoDestructiveColor`, `CosmoDestructiveBrush` | `oklch(0.577 0.245 27.325)` | `#E7000B` (clipped) | `oklch(0.704 0.191 22.216)` | `#FF6467` (clipped) |
| `--border` | `CosmoBorderColor`, `CosmoBorderBrush` | `oklch(0.922 0 0)` | `#E5E5E5` | `oklch(1 0 0 / 10%)` | `#1AFFFFFF` |
| `--input` | `CosmoInputColor`, `CosmoInputBrush` | `oklch(0.922 0 0)` | `#E5E5E5` | `oklch(1 0 0 / 15%)` | `#26FFFFFF` |
| `--ring` | `CosmoRingColor`, `CosmoRingBrush` | `oklch(0.708 0 0)` | `#A1A1A1` | `oklch(0.556 0 0)` | `#737373` |
| `--chart-1` | `CosmoChart1Color`, `CosmoChart1Brush` | `oklch(0.87 0 0)` | `#D4D4D4` | `oklch(0.87 0 0)` | `#D4D4D4` |
| `--chart-2` | `CosmoChart2Color`, `CosmoChart2Brush` | `oklch(0.556 0 0)` | `#737373` | `oklch(0.556 0 0)` | `#737373` |
| `--chart-3` | `CosmoChart3Color`, `CosmoChart3Brush` | `oklch(0.439 0 0)` | `#525252` | `oklch(0.439 0 0)` | `#525252` |
| `--chart-4` | `CosmoChart4Color`, `CosmoChart4Brush` | `oklch(0.371 0 0)` | `#404040` | `oklch(0.371 0 0)` | `#404040` |
| `--chart-5` | `CosmoChart5Color`, `CosmoChart5Brush` | `oklch(0.269 0 0)` | `#262626` | `oklch(0.269 0 0)` | `#262626` |
| `--sidebar` | `CosmoSidebarColor`, `CosmoSidebarBrush` | `oklch(0.985 0 0)` | `#FAFAFA` | `oklch(0.205 0 0)` | `#171717` |
| `--sidebar-foreground` | `CosmoSidebarForegroundColor`, `CosmoSidebarForegroundBrush` | `oklch(0.145 0 0)` | `#0A0A0A` | `oklch(0.985 0 0)` | `#FAFAFA` |
| `--sidebar-primary` | `CosmoSidebarPrimaryColor`, `CosmoSidebarPrimaryBrush` | `oklch(0.205 0 0)` | `#171717` | `oklch(0.488 0.243 264.376)` | `#1447E6` |
| `--sidebar-primary-foreground` | `CosmoSidebarPrimaryForegroundColor`, `CosmoSidebarPrimaryForegroundBrush` | `oklch(0.985 0 0)` | `#FAFAFA` | `oklch(0.985 0 0)` | `#FAFAFA` |
| `--sidebar-accent` | `CosmoSidebarAccentColor`, `CosmoSidebarAccentBrush` | `oklch(0.97 0 0)` | `#F5F5F5` | `oklch(0.269 0 0)` | `#262626` |
| `--sidebar-accent-foreground` | `CosmoSidebarAccentForegroundColor`, `CosmoSidebarAccentForegroundBrush` | `oklch(0.205 0 0)` | `#171717` | `oklch(0.985 0 0)` | `#FAFAFA` |
| `--sidebar-border` | `CosmoSidebarBorderColor`, `CosmoSidebarBorderBrush` | `oklch(0.922 0 0)` | `#E5E5E5` | `oklch(1 0 0 / 10%)` | `#1AFFFFFF` |
| `--sidebar-ring` | `CosmoSidebarRingColor`, `CosmoSidebarRingBrush` | `oklch(0.708 0 0)` | `#A1A1A1` | `oklch(0.556 0 0)` | `#737373` |

## Color themes

| Theme | Source | `--primary` light | `--primary` dark | `--background` dark | `--ring` light |
| --- | --- | --- | --- | --- | --- |
| Neutral | `public/r/colors/neutral.json` | `#171717` | `#E5E5E5` | `#0A0A0A` | `#A1A1A1` |
| Zinc | `public/r/colors/zinc.json` | `#18181B` | `#E4E4E7` | `#09090B` | `#9F9FA9` |
| Slate | `public/r/colors/slate.json` | `#0F172B` | `#E2E8F0` | `#020618` | `#90A1B9` |
| Stone | `public/r/colors/stone.json` | `#1C1917` | `#E7E5E4` | `#0C0A09` | `#A6A09B` |
| Gray | `public/r/colors/gray.json` | `#101828` | `#E5E7EB` | `#030712` | `#99A1AF` |
| Red | `baseColorsOKLCH.red` | `#E7000B` | `#FB2C36` | `#09090B` | `#FF6467` |
| Rose | `baseColorsOKLCH.rose` | `#EC003F` | `#FF2056` | `#09090B` | `#FF637E` |
| Orange | `baseColorsOKLCH.orange` | `#F54900` | `#FF6900` | `#09090B` | `#FF8904` |
| Green | `baseColorsOKLCH.green` | `#5EA500` | `#5EA500` | `#09090B` | `#9AE600` |
| Blue | `baseColorsOKLCH.blue` | `#1447E6` | `#1447E6` | `#09090B` | `#A1A1A1` |
| Yellow | `baseColorsOKLCH.yellow` | `#FDC700` | `#F0B100` | `#09090B` | `#FDC700` |
| Violet | `baseColorsOKLCH.violet` | `#7F22FE` | `#8E51FF` | `#09090B` | `#A684FF` |

## Component brushes and shadows

shadcn's classes apply opacities to the tokens (`bg-primary/90` is `color-mix(in oklab, var(--primary) 90%,
transparent)`, the color with 90% alpha). Cosmo precomputes them per theme and variant (values for neutral):

| Resource | Light | Dark |
| --- | --- | --- |
| `CosmoButtonPrimaryHoverBrush` | `--primary` at 90% (`#E6171717`) | `--primary` at 90% (`#E6E5E5E5`) |
| `CosmoMutedForegroundThirdBrush` | `--muted-foreground` at 30% (`#4D737373`) | `--muted-foreground` at 30% (`#4DA1A1A1`) |
| `CosmoButtonDestructiveBrush` | `--destructive` at 100% (`#E7000B`) | `--destructive` at 60% (`#99FF6467`) |
| `CosmoButtonDestructiveHoverBrush` | `--destructive` at 90% (`#E6E7000B`) | `--destructive` at 60% (`#99FF6467`) |
| `CosmoButtonDestructiveFocusRingBrush` | `--destructive` at 20% (`#33E7000B`) | `--destructive` at 40% (`#66FF6467`) |
| `CosmoButtonOutlineBrush` | `--background` at 100% (`#FFFFFF`) | `--input` at 30% (`#0BFFFFFF`) |
| `CosmoButtonOutlineBorderBrush` | `--border` at 100% (`#E5E5E5`) | `--input` at 100% (`#26FFFFFF`) |
| `CosmoButtonOutlineHoverBrush` | `--accent` at 100% (`#F5F5F5`) | `--input` at 50% (`#13FFFFFF`) |
| `CosmoButtonOutlineFocusBorderBrush` | `--ring` at 100% (`#A1A1A1`) | `--input` at 100% (`#26FFFFFF`) |
| `CosmoButtonOutlineInvalidBorderBrush` | `--destructive` at 100% (`#E7000B`) | `--input` at 100% (`#26FFFFFF`) |
| `CosmoButtonSecondaryHoverBrush` | `--secondary` at 80% (`#CCF5F5F5`) | `--secondary` at 80% (`#CC262626`) |
| `CosmoButtonGhostHoverBrush` | `--accent` at 100% (`#F5F5F5`) | `--accent` at 50% (`#80262626`) |
| `CosmoFocusRingBrush` | `--ring` at 50% (`#80A1A1A1`) | `--ring` at 50% (`#80737373`) |
| `CosmoBadgeDestructiveBrush` | `--destructive` at 100% (`#E7000B`) | `--destructive` at 60% (`#99FF6467`) |
| `CosmoBadgeSecondaryHoverBrush` | `--secondary` at 90% (`#E6F5F5F5`) | `--secondary` at 90% (`#E6262626`) |
| `CosmoKbdTooltipBrush` | `--background` at 20% (`#33FFFFFF`) | `--background` at 10% (`#1A0A0A0A`) |
| `CosmoBadgeDestructiveHoverBrush` | `--destructive` at 90% (`#E6E7000B`) | `--destructive` at 90% (`#E6FF6467`) |
| `CosmoInputBackgroundBrush` | `--background` at 0% (`#00FFFFFF`) | `--input` at 30% (`#0BFFFFFF`) |
| `CosmoInputHoverBackgroundBrush` | `--background` at 0% (`#00FFFFFF`) | `--input` at 50% (`#13FFFFFF`) |
| `CosmoSwitchOffBrush` | `--input` at 100% (`#E5E5E5`) | `--input` at 80% (`#1EFFFFFF`) |
| `CosmoSwitchThumbBrush` | `--background` at 100% (`#FFFFFF`) | `--foreground` at 100% (`#FAFAFA`) |
| `CosmoSwitchThumbOnBrush` | `--background` at 100% (`#FFFFFF`) | `--primary-foreground` at 100% (`#171717`) |
| `CosmoProgressTrackBrush` | `--primary` at 20% (`#33171717`) | `--primary` at 20% (`#33E5E5E5`) |
| `CosmoChoiceCheckedBrush` | `--primary` at 5% (`#0D171717`) | `--primary` at 10% (`#1AE5E5E5`) |
| `CosmoMenuDestructiveFocusBrush` | `--destructive` at 10% (`#1AE7000B`) | `--destructive` at 20% (`#33FF6467`) |
| `CosmoMutedHalfBrush` | `--muted` at 50% (`#80F5F5F5`) | `--muted` at 50% (`#80262626`) |
| `CosmoTabsTriggerBrush` | `--foreground` at 60% (`#990A0A0A`) | `--muted-foreground` at 100% (`#A1A1A1`) |
| `CosmoTabsActiveBrush` | `--background` at 100% (`#FFFFFF`) | `--input` at 30% (`#0BFFFFFF`) |
| `CosmoTabsActiveBorderBrush` | `--background` at 0% (`#00FFFFFF`) | `--input` at 100% (`#26FFFFFF`) |
| `CosmoTabsLineFocusBorderBrush` | `--ring` at 100% (`#A1A1A1`) | `--ring` at 0% (`#00737373`) |
| `CosmoTabsActiveFocusBorderBrush` | `--ring` at 100% (`#A1A1A1`) | `--input` at 100% (`#26FFFFFF`) |
| `CosmoAccentHalfBrush` | `--accent` at 50% (`#80F5F5F5`) | `--accent` at 50% (`#80262626`) |
| `CosmoAlertDestructiveDescriptionBrush` | `--destructive` at 90% (`#E6E7000B`) | `--destructive` at 90% (`#E6FF6467`) |
| `CosmoChartTooltipBorderBrush` | `--border` at 50% (`#80E5E5E5`) | `--border` at 50% (`#0DFFFFFF`) |
| `CosmoBubbleDestructiveBrush` | `--destructive` at 10% (`#1AE7000B`) | `--destructive` at 20% (`#33FF6467`) |
| `CosmoBubbleHoverBrush` | `--primary` at 80% (`#CC171717`) | `--primary` at 80% (`#CCE5E5E5`) |
| `CosmoBubbleOutlineHoverBrush` | `--muted` at 100% (`#F5F5F5`) | `--input` at 30% (`#0BFFFFFF`) |
| `CosmoBubbleGhostHoverBrush` | `--muted` at 100% (`#F5F5F5`) | `--muted` at 50% (`#80262626`) |
| `CosmoBubbleDestructiveHoverBrush` | `--destructive` at 20% (`#33E7000B`) | `--destructive` at 30% (`#4DFF6467`) |
| `CosmoAttachmentErrorBorderBrush` | `--destructive` at 30% (`#4DE7000B`) | `--destructive` at 30% (`#4DFF6467`) |
| `CosmoAttachmentErrorMediaBrush` | `--destructive` at 10% (`#1AE7000B`) | `--destructive` at 10% (`#1AFF6467`) |
| `CosmoAttachmentErrorDescriptionBrush` | `--destructive` at 80% (`#CCE7000B`) | `--destructive` at 80% (`#CCFF6467`) |
| `CosmoCodeBlockRingBrush` | `--foreground` at 5% (`#0D0A0A0A`) | `--foreground` at 10% (`#1AFAFAFA`) |
| `CosmoMarkdownQuoteBrush` | `--muted-foreground` at 30% (`#4D737373`) | `--muted-foreground` at 30% (`#4DA1A1A1`) |
| `CosmoCodeBlockButtonHoverBrush` | `--foreground` at 5% (`#0D0A0A0A`) | `--foreground` at 10% (`#1AFAFAFA`) |
| `CosmoCodeBlockDestructiveBrush` | `--destructive` at 5% (`#0DE7000B`) | `--destructive` at 10% (`#1AFF6467`) |
| `CosmoCodeBlockDestructiveRingBrush` | `--destructive` at 15% (`#26E7000B`) | `--destructive` at 20% (`#33FF6467`) |
| `CosmoCodeBlockLineHighlightedBrush` | `--foreground` at 5% (`#0D0A0A0A`) | `--foreground` at 5% (`#0DFAFAFA`) |
| `CosmoCodeBlockLineRemovedBrush` | `--destructive` at 10% (`#1AE7000B`) | `--destructive` at 10% (`#1AFF6467`) |
| `CosmoTraceDescriptionBrush` | `--muted-foreground` at 70% (`#B3737373`) | `--muted-foreground` at 70% (`#B3A1A1A1`) |
| `CosmoQuestionnaireChoiceHoverBrush` | `--input` at 40% (`#66E5E5E5`) | `--input` at 40% (`#0FFFFFFF`) |
| `CosmoQuestionnaireChoiceCheckedBrush` | `--primary` at 10% (`#1A171717`) | `--primary` at 10% (`#1AE5E5E5`) |
| `CosmoQuestionnaireChoiceCheckedBorderBrush` | `--primary` at 40% (`#66171717`) | `--primary` at 40% (`#66E5E5E5`) |
| `CosmoQuestionnaireIndicatorBrush` | `--input` at 90% (`#E6E5E5E5`) | `--input` at 90% (`#22FFFFFF`) |
| `CosmoQuestionnaireInputBrush` | `--input` at 50% (`#80E5E5E5`) | `--input` at 50% (`#13FFFFFF`) |
| `CosmoQuestionnaireShortcutBrush` | `--background` at 80% (`#CCFFFFFF`) | `--background` at 80% (`#CC0A0A0A`) |
| `CosmoQuestionnaireShortcutBorderBrush` | `--primary` at 10% (`#1A171717`) | `--primary` at 10% (`#1AE5E5E5`) |
| `SystemControlErrorTextForegroundBrush` (Fluent's key) | `--destructive` at 100% | `--destructive` at 100% |
| `CheckBoxCheckBackgroundStrokeUnchecked` (Fluent's key) | `--input` at 100% | `--input` at 100% |
| `CheckBoxCheckBackgroundStrokeUncheckedPointerOver` (Fluent's key) | `--input` at 100% | `--input` at 100% |
| `CheckBoxCheckBackgroundStrokeUncheckedPressed` (Fluent's key) | `--input` at 100% | `--input` at 100% |
| `CheckBoxCheckBackgroundStrokeUncheckedDisabled` (Fluent's key) | `--input` at 100% | `--input` at 100% |
| `CheckBoxCheckBackgroundFillUnchecked` (Fluent's key) | `--background` at 0% | `--input` at 30% |
| `CheckBoxCheckBackgroundFillUncheckedPointerOver` (Fluent's key) | `--background` at 0% | `--input` at 30% |
| `CheckBoxCheckBackgroundFillUncheckedPressed` (Fluent's key) | `--background` at 0% | `--input` at 30% |
| `CheckBoxCheckBackgroundFillUncheckedDisabled` (Fluent's key) | `--background` at 0% | `--input` at 30% |
| `CheckBoxCheckGlyphForegroundUnchecked` (Fluent's key) | `--foreground` at 100% | `--foreground` at 100% |
| `CheckBoxCheckGlyphForegroundUncheckedPointerOver` (Fluent's key) | `--foreground` at 100% | `--foreground` at 100% |
| `CheckBoxCheckGlyphForegroundUncheckedPressed` (Fluent's key) | `--foreground` at 100% | `--foreground` at 100% |
| `CheckBoxCheckGlyphForegroundUncheckedDisabled` (Fluent's key) | `--foreground` at 100% | `--foreground` at 100% |
| `CheckBoxCheckBackgroundStrokeChecked` (Fluent's key) | `--primary` at 100% | `--primary` at 100% |
| `CheckBoxCheckBackgroundStrokeCheckedPointerOver` (Fluent's key) | `--primary` at 100% | `--primary` at 100% |
| `CheckBoxCheckBackgroundStrokeCheckedPressed` (Fluent's key) | `--primary` at 100% | `--primary` at 100% |
| `CheckBoxCheckBackgroundStrokeCheckedDisabled` (Fluent's key) | `--primary` at 100% | `--primary` at 100% |
| `CheckBoxCheckBackgroundFillChecked` (Fluent's key) | `--primary` at 100% | `--primary` at 100% |
| `CheckBoxCheckBackgroundFillCheckedPointerOver` (Fluent's key) | `--primary` at 100% | `--primary` at 100% |
| `CheckBoxCheckBackgroundFillCheckedPressed` (Fluent's key) | `--primary` at 100% | `--primary` at 100% |
| `CheckBoxCheckBackgroundFillCheckedDisabled` (Fluent's key) | `--primary` at 100% | `--primary` at 100% |
| `CheckBoxCheckGlyphForegroundChecked` (Fluent's key) | `--primary-foreground` at 100% | `--primary-foreground` at 100% |
| `CheckBoxCheckGlyphForegroundCheckedPointerOver` (Fluent's key) | `--primary-foreground` at 100% | `--primary-foreground` at 100% |
| `CheckBoxCheckGlyphForegroundCheckedPressed` (Fluent's key) | `--primary-foreground` at 100% | `--primary-foreground` at 100% |
| `CheckBoxCheckGlyphForegroundCheckedDisabled` (Fluent's key) | `--primary-foreground` at 100% | `--primary-foreground` at 100% |
| `CheckBoxCheckBackgroundStrokeIndeterminate` (Fluent's key) | `--input` at 100% | `--input` at 100% |
| `CheckBoxCheckBackgroundStrokeIndeterminatePointerOver` (Fluent's key) | `--input` at 100% | `--input` at 100% |
| `CheckBoxCheckBackgroundStrokeIndeterminatePressed` (Fluent's key) | `--input` at 100% | `--input` at 100% |
| `CheckBoxCheckBackgroundStrokeIndeterminateDisabled` (Fluent's key) | `--input` at 100% | `--input` at 100% |
| `CheckBoxCheckBackgroundFillIndeterminate` (Fluent's key) | `--background` at 0% | `--input` at 30% |
| `CheckBoxCheckBackgroundFillIndeterminatePointerOver` (Fluent's key) | `--background` at 0% | `--input` at 30% |
| `CheckBoxCheckBackgroundFillIndeterminatePressed` (Fluent's key) | `--background` at 0% | `--input` at 30% |
| `CheckBoxCheckBackgroundFillIndeterminateDisabled` (Fluent's key) | `--background` at 0% | `--input` at 30% |
| `CheckBoxCheckGlyphForegroundIndeterminate` (Fluent's key) | `--foreground` at 100% | `--foreground` at 100% |
| `CheckBoxCheckGlyphForegroundIndeterminatePointerOver` (Fluent's key) | `--foreground` at 100% | `--foreground` at 100% |
| `CheckBoxCheckGlyphForegroundIndeterminatePressed` (Fluent's key) | `--foreground` at 100% | `--foreground` at 100% |
| `CheckBoxCheckGlyphForegroundIndeterminateDisabled` (Fluent's key) | `--foreground` at 100% | `--foreground` at 100% |
| `CheckBoxForegroundUnchecked` (Fluent's key) | `--foreground` at 100% | `--foreground` at 100% |
| `CheckBoxForegroundUncheckedPointerOver` (Fluent's key) | `--foreground` at 100% | `--foreground` at 100% |
| `CheckBoxForegroundUncheckedPressed` (Fluent's key) | `--foreground` at 100% | `--foreground` at 100% |
| `CheckBoxForegroundUncheckedDisabled` (Fluent's key) | `--foreground` at 100% | `--foreground` at 100% |
| `CheckBoxForegroundChecked` (Fluent's key) | `--foreground` at 100% | `--foreground` at 100% |
| `CheckBoxForegroundCheckedPointerOver` (Fluent's key) | `--foreground` at 100% | `--foreground` at 100% |
| `CheckBoxForegroundCheckedPressed` (Fluent's key) | `--foreground` at 100% | `--foreground` at 100% |
| `CheckBoxForegroundCheckedDisabled` (Fluent's key) | `--foreground` at 100% | `--foreground` at 100% |
| `CheckBoxForegroundIndeterminate` (Fluent's key) | `--foreground` at 100% | `--foreground` at 100% |
| `CheckBoxForegroundIndeterminatePointerOver` (Fluent's key) | `--foreground` at 100% | `--foreground` at 100% |
| `CheckBoxForegroundIndeterminatePressed` (Fluent's key) | `--foreground` at 100% | `--foreground` at 100% |
| `CheckBoxForegroundIndeterminateDisabled` (Fluent's key) | `--foreground` at 100% | `--foreground` at 100% |
| `RadioButtonOuterEllipseStroke` (Fluent's key) | `--input` at 100% | `--input` at 100% |
| `RadioButtonOuterEllipseStrokePointerOver` (Fluent's key) | `--input` at 100% | `--input` at 100% |
| `RadioButtonOuterEllipseStrokePressed` (Fluent's key) | `--input` at 100% | `--input` at 100% |
| `RadioButtonOuterEllipseStrokeDisabled` (Fluent's key) | `--input` at 100% | `--input` at 100% |
| `RadioButtonOuterEllipseFill` (Fluent's key) | `--background` at 0% | `--input` at 30% |
| `RadioButtonOuterEllipseFillPointerOver` (Fluent's key) | `--background` at 0% | `--input` at 30% |
| `RadioButtonOuterEllipseFillPressed` (Fluent's key) | `--background` at 0% | `--input` at 30% |
| `RadioButtonOuterEllipseFillDisabled` (Fluent's key) | `--background` at 0% | `--input` at 30% |
| `RadioButtonOuterEllipseCheckedStroke` (Fluent's key) | `--input` at 100% | `--input` at 100% |
| `RadioButtonOuterEllipseCheckedStrokePointerOver` (Fluent's key) | `--input` at 100% | `--input` at 100% |
| `RadioButtonOuterEllipseCheckedStrokePressed` (Fluent's key) | `--input` at 100% | `--input` at 100% |
| `RadioButtonOuterEllipseCheckedStrokeDisabled` (Fluent's key) | `--input` at 100% | `--input` at 100% |
| `RadioButtonOuterEllipseCheckedFill` (Fluent's key) | `--background` at 100% | `--background` at 100% |
| `RadioButtonOuterEllipseCheckedFillPointerOver` (Fluent's key) | `--background` at 100% | `--background` at 100% |
| `RadioButtonOuterEllipseCheckedFillPressed` (Fluent's key) | `--background` at 100% | `--background` at 100% |
| `RadioButtonOuterEllipseCheckedFillDisabled` (Fluent's key) | `--background` at 100% | `--background` at 100% |
| `RadioButtonCheckGlyphFill` (Fluent's key) | `--primary` at 100% | `--primary` at 100% |
| `RadioButtonCheckGlyphFillPointerOver` (Fluent's key) | `--primary` at 100% | `--primary` at 100% |
| `RadioButtonCheckGlyphFillPressed` (Fluent's key) | `--primary` at 100% | `--primary` at 100% |
| `RadioButtonCheckGlyphFillDisabled` (Fluent's key) | `--primary` at 100% | `--primary` at 100% |
| `RadioButtonForeground` (Fluent's key) | `--foreground` at 100% | `--foreground` at 100% |
| `RadioButtonForegroundPointerOver` (Fluent's key) | `--foreground` at 100% | `--foreground` at 100% |
| `RadioButtonForegroundPressed` (Fluent's key) | `--foreground` at 100% | `--foreground` at 100% |
| `RadioButtonForegroundDisabled` (Fluent's key) | `--foreground` at 100% | `--foreground` at 100% |
| `ToggleSwitchFillOff` (Fluent's key) | `--input` at 100% | `--input` at 80% |
| `ToggleSwitchFillOffPointerOver` (Fluent's key) | `--input` at 100% | `--input` at 80% |
| `ToggleSwitchFillOffPressed` (Fluent's key) | `--input` at 100% | `--input` at 80% |
| `ToggleSwitchFillOffDisabled` (Fluent's key) | `--input` at 100% | `--input` at 80% |
| `ToggleSwitchStrokeOff` (Fluent's key) | `--background` at 0% | `--background` at 0% |
| `ToggleSwitchStrokeOffPointerOver` (Fluent's key) | `--background` at 0% | `--background` at 0% |
| `ToggleSwitchStrokeOffPressed` (Fluent's key) | `--background` at 0% | `--background` at 0% |
| `ToggleSwitchStrokeOffDisabled` (Fluent's key) | `--background` at 0% | `--background` at 0% |
| `ToggleSwitchFillOn` (Fluent's key) | `--primary` at 100% | `--primary` at 100% |
| `ToggleSwitchFillOnPointerOver` (Fluent's key) | `--primary` at 100% | `--primary` at 100% |
| `ToggleSwitchFillOnPressed` (Fluent's key) | `--primary` at 100% | `--primary` at 100% |
| `ToggleSwitchFillOnDisabled` (Fluent's key) | `--primary` at 100% | `--primary` at 100% |
| `ToggleSwitchStrokeOn` (Fluent's key) | `--background` at 0% | `--background` at 0% |
| `ToggleSwitchStrokeOnPointerOver` (Fluent's key) | `--background` at 0% | `--background` at 0% |
| `ToggleSwitchStrokeOnPressed` (Fluent's key) | `--background` at 0% | `--background` at 0% |
| `ToggleSwitchStrokeOnDisabled` (Fluent's key) | `--background` at 0% | `--background` at 0% |
| `ToggleSwitchKnobFillOff` (Fluent's key) | `--background` at 100% | `--foreground` at 100% |
| `ToggleSwitchKnobFillOffPointerOver` (Fluent's key) | `--background` at 100% | `--foreground` at 100% |
| `ToggleSwitchKnobFillOffPressed` (Fluent's key) | `--background` at 100% | `--foreground` at 100% |
| `ToggleSwitchKnobFillOffDisabled` (Fluent's key) | `--background` at 100% | `--foreground` at 100% |
| `ToggleSwitchKnobFillOn` (Fluent's key) | `--background` at 100% | `--primary-foreground` at 100% |
| `ToggleSwitchKnobFillOnPointerOver` (Fluent's key) | `--background` at 100% | `--primary-foreground` at 100% |
| `ToggleSwitchKnobFillOnPressed` (Fluent's key) | `--background` at 100% | `--primary-foreground` at 100% |
| `ToggleSwitchKnobFillOnDisabled` (Fluent's key) | `--background` at 100% | `--primary-foreground` at 100% |
| `ToggleSwitchContentForeground` (Fluent's key) | `--foreground` at 100% | `--foreground` at 100% |
| `ToggleButtonBackgroundPointerOver` (Fluent's key) | `--muted` at 100% | `--muted` at 100% |
| `ToggleButtonBackgroundPressed` (Fluent's key) | `--muted` at 100% | `--muted` at 100% |
| `ToggleButtonBackgroundDisabled` (Fluent's key) | `--background` at 0% | `--background` at 0% |
| `ToggleButtonForegroundPointerOver` (Fluent's key) | `--muted-foreground` at 100% | `--muted-foreground` at 100% |
| `ToggleButtonForegroundPressed` (Fluent's key) | `--muted-foreground` at 100% | `--muted-foreground` at 100% |
| `ToggleButtonForegroundDisabled` (Fluent's key) | `--foreground` at 100% | `--foreground` at 100% |
| `ToggleButtonBackgroundChecked` (Fluent's key) | `--accent` at 100% | `--accent` at 100% |
| `ToggleButtonForegroundChecked` (Fluent's key) | `--accent-foreground` at 100% | `--accent-foreground` at 100% |
| `ToggleButtonBackgroundCheckedPointerOver` (Fluent's key) | `--accent` at 100% | `--accent` at 100% |
| `ToggleButtonForegroundCheckedPointerOver` (Fluent's key) | `--accent-foreground` at 100% | `--accent-foreground` at 100% |
| `ToggleButtonBackgroundCheckedPressed` (Fluent's key) | `--accent` at 100% | `--accent` at 100% |
| `ToggleButtonForegroundCheckedPressed` (Fluent's key) | `--accent-foreground` at 100% | `--accent-foreground` at 100% |
| `ToggleButtonBackgroundCheckedDisabled` (Fluent's key) | `--accent` at 100% | `--accent` at 100% |
| `ToggleButtonForegroundCheckedDisabled` (Fluent's key) | `--accent-foreground` at 100% | `--accent-foreground` at 100% |
| `ToggleButtonBorderBrushPointerOver` (Fluent's key) | `--input` at 100% | `--input` at 100% |
| `ToggleButtonBorderBrushPressed` (Fluent's key) | `--input` at 100% | `--input` at 100% |
| `ToggleButtonBorderBrushDisabled` (Fluent's key) | `--input` at 100% | `--input` at 100% |
| `ToggleButtonBorderBrushChecked` (Fluent's key) | `--input` at 100% | `--input` at 100% |
| `ToggleButtonBorderBrushCheckedPointerOver` (Fluent's key) | `--input` at 100% | `--input` at 100% |
| `ToggleButtonBorderBrushCheckedPressed` (Fluent's key) | `--input` at 100% | `--input` at 100% |
| `ToggleButtonBorderBrushCheckedDisabled` (Fluent's key) | `--input` at 100% | `--input` at 100% |
| `FlyoutPresenterBackground` (Fluent's key) | `--popover` at 100% | `--popover` at 100% |
| `FlyoutBorderThemeBrush` (Fluent's key) | `--border` at 100% | `--border` at 100% |
| `MenuFlyoutPresenterBackground` (Fluent's key) | `--popover` at 100% | `--popover` at 100% |
| `MenuFlyoutPresenterBorderBrush` (Fluent's key) | `--border` at 100% | `--border` at 100% |
| `MenuFlyoutItemForeground` (Fluent's key) | `--popover-foreground` at 100% | `--popover-foreground` at 100% |
| `MenuFlyoutItemBackgroundPointerOver` (Fluent's key) | `--accent` at 100% | `--accent` at 100% |
| `MenuFlyoutItemForegroundPointerOver` (Fluent's key) | `--accent-foreground` at 100% | `--accent-foreground` at 100% |
| `MenuFlyoutItemKeyboardAcceleratorTextForeground` (Fluent's key) | `--muted-foreground` at 100% | `--muted-foreground` at 100% |
| `MenuFlyoutSubItemChevron` (Fluent's key) | `--muted-foreground` at 100% | `--muted-foreground` at 100% |
| `ToolTipBackground` (Fluent's key) | `--foreground` at 100% | `--foreground` at 100% |
| `ToolTipForeground` (Fluent's key) | `--background` at 100% | `--background` at 100% |
| `ToolTipBorderBrush` (Fluent's key) | `--foreground` at 100% | `--foreground` at 100% |
| `NotificationCardBackgroundBrush` (Fluent's key) | `--popover` at 100% | `--popover` at 100% |
| `NotificationCardForegroundBrush` (Fluent's key) | `--popover-foreground` at 100% | `--popover-foreground` at 100% |
| `CosmoFocusRingShadow` | `0 0 0 3 #80A1A1A1` | `0 0 0 3 #80737373` |
| `CosmoAttachmentFocusShadow` | `0 0 0 1 #80A1A1A1` | `0 0 0 1 #80737373` |
| `CosmoCodeBlockFloatingShadow` | `0 0 0 1 #1A0A0A0A` | `0 0 0 1 #1AFAFAFA` |
| `CosmoTabsFocusShadow` | `0 0 0 1 #A1A1A1, 0 0 0 3 #80A1A1A1` | `0 0 0 1 #737373, 0 0 0 3 #80737373` |
| `CosmoTabsActiveFocusShadow` | `0 0 0 1 #A1A1A1, 0 0 0 3 #80A1A1A1, 0 1 3 0 #1A000000, 0 1 2 -1 #1A000000` | `0 0 0 1 #737373, 0 0 0 3 #80737373, 0 1 3 0 #1A000000, 0 1 2 -1 #1A000000` |
| `CosmoDestructiveFocusRingShadow` | `0 0 0 3 #33E7000B` | `0 0 0 3 #66FF6467` |
| `CosmoOutlineFocusShadow` | `0 0 0 3 #80A1A1A1, 0 1 2 0 #0D000000` | `0 0 0 3 #80737373, 0 1 2 0 #0D000000` |
| `CosmoOutlineDestructiveFocusShadow` | `0 0 0 3 #33E7000B, 0 1 2 0 #0D000000` | `0 0 0 3 #66FF6467, 0 1 2 0 #0D000000` |
| `CosmoRingBackgroundShadow` | `0 0 0 2 #FFFFFF` | `0 0 0 2 #0A0A0A` |
| `CosmoShadowXs` | `0 1 2 0 #0D000000` | `0 1 2 0 #0D000000` |
| `CosmoShadowSm` | `0 1 3 0 #1A000000, 0 1 2 -1 #1A000000` | `0 1 3 0 #1A000000, 0 1 2 -1 #1A000000` |
| `CosmoShadowMd` | `0 4 6 -1 #1A000000, 0 2 4 -2 #1A000000` | `0 4 6 -1 #1A000000, 0 2 4 -2 #1A000000` |
| `CosmoShadowLg` | `0 10 15 -3 #1A000000, 0 4 6 -4 #1A000000` | `0 10 15 -3 #1A000000, 0 4 6 -4 #1A000000` |
| `CosmoToastShadow` | `0 4 12 0 #1A000000` | `0 4 12 0 #1A000000` |
| `CosmoInvalidFocusShadow` | `0 0 0 3 #33E7000B, 0 1 2 0 #0D000000` | `0 0 0 3 #66FF6467, 0 1 2 0 #0D000000` |
| `CosmoSliderThumbHoverShadow` | `0 0 0 4 #80A1A1A1, 0 1 3 0 #1A000000, 0 1 2 -1 #1A000000` | `0 0 0 4 #80737373, 0 1 3 0 #1A000000, 0 1 2 -1 #1A000000` |

## Radius

| `CosmoRadius` | rem | px | Sm | Md | Lg | Xl | 2Xl | 3Xl | 4Xl |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| None | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| Small | 0.3 | 4.8 | 2.88 | 3.84 | 4.8 | 6.72 | 8.64 | 10.56 | 12.48 |
| Medium | 0.5 | 8 | 4.8 | 6.4 | 8 | 11.2 | 14.4 | 17.6 | 20.8 |
| Default | 0.625 | 10 | 6 | 8 | 10 | 14 | 18 | 22 | 26 |
| Large | 0.75 | 12 | 7.2 | 9.6 | 12 | 16.8 | 21.6 | 26.4 | 31.2 |
| ExtraLarge | 1 | 16 | 9.6 | 12.8 | 16 | 22.4 | 28.8 | 35.2 | 41.6 |
