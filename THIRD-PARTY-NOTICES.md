# Third-party notices

Aioros.Cosmo ports design tokens, class values and component structure from the following projects. Cosmo is an
independent port of shadcn/ui and is not affiliated with or endorsed by shadcn. Their notices are kept here as their
licenses require; every package carries this file.

| Component | Where it is used | License |
|---|---|---|
| [shadcn/ui](https://github.com/shadcn-ui/ui) at `c2477f311c19982ef6e246085438e6cfec3e35ea` (new-york-v4) | `src/Aioros.Cosmo` (the design tokens and their values, the radius scale, the components' variants, sizes, spacing and states, ported from `apps/v4/registry/new-york-v4/ui`, `public/r/colors` and `registry/_legacy-base-colors.ts`); `src/Aioros.Cosmo.Charts` (the chart container's restyling, the tooltip's and the legend's content, from `ui/chart.tsx`); the Gallery's chart examples and their data (`registry/new-york-v4/charts`); the Gallery's login and signup blocks and their brand icons' paths (`registry/new-york-v4/blocks`); `src/Aioros.Cosmo/Chat` (the marker, bubble, message, attachment and message scroller components, from `ui/marker.tsx`, `ui/bubble.tsx`, `ui/message.tsx`, `ui/attachment.tsx` and `ui/message-scroller.tsx`, and the code block, the trace and the questionnaire, from the radix base's `ui/code-block.tsx`, `ui/trace.tsx` and `ui/questionnaire.tsx` in the radix-rhea style, `registry/styles/style-rhea.css`) and the Gallery's chat examples (`apps/v4/examples/radix`); not shipped: the pinned sources in `tools/shadcn/registry` | MIT, Copyright (c) 2023 shadcn |
| [Radix UI Primitives](https://github.com/radix-ui/primitives) | Not shipped: the behavior of the components Cosmo ports follows Radix's (focus, keyboard, ARIA) as shadcn/ui uses it; `tools/shadcn` renders the reference with `radix-ui` | MIT, Copyright (c) 2022 WorkOS |
| [sonner](https://github.com/emilkowalski/sonner) 2.0.7 | `src/Aioros.Cosmo/Overlays` (the toast's box, text, icon and action values and the stack's geometry, from sonner's `styles.css` and constants, as shadcn/ui's `sonner` styles them); `tools/shadcn` renders the reference with it | MIT, Copyright (c) 2023 Emil Kowalski |
| [vaul](https://github.com/emilkowalski/vaul) 1.1.2 | Not shipped: shadcn/ui's `drawer` is built on it; `tools/shadcn` renders the reference with it | MIT, Copyright (c) 2023 Emil Kowalski |
| [cmdk](https://github.com/pacocoursey/cmdk) 1.1.1 | `src/Aioros.Cosmo/Search` (the command menu's filtering score, ported from `command-score` as cmdk bundles it, its selection and keys, as shadcn/ui's `command` uses them); `tools/shadcn` renders the reference with it | MIT, Copyright (c) 2022 Paco Coursey |
| [TanStack Table](https://github.com/TanStack/table) 9.0.0 | `src/Aioros.Cosmo/Tables` (the data table's sorting, filtering, column visibility, row selection and pagination behavior, as shadcn/ui's data table guide uses them); `tools/shadcn` renders the reference with it | MIT, Copyright (c) 2016 Tanner Linsley |
| [react-resizable-panels](https://github.com/bvaughn/react-resizable-panels) 4.5.8 | `src/Aioros.Cosmo/Layout` (the panels' percentages, limits and the separator's keys, as shadcn/ui's `resizable` uses them); `tools/shadcn` renders the reference with it | MIT, Copyright (c) 2018 Brian Vaughn |
| [Embla Carousel](https://github.com/davidjerleke/embla-carousel) 8.5.2 | Not shipped: shadcn/ui's `carousel` is built on it, Cosmo's carousel follows its options and keys; `tools/shadcn` renders the reference with it | MIT, Copyright (c) 2019 David Jerleke |
| [Recharts](https://github.com/recharts/recharts) 3.8.0 and 2.15.4 | Not shipped by Cosmo: shadcn/ui's `chart` is built on it; `Aioros.Cosmo.Charts` draws its charts with Aioros.Pictor, a port of Recharts 2.15.4 that carries Recharts' notice; `tools/shadcn` renders the reference with both versions | MIT, Copyright (c) 2015-present recharts |
| [Lucide](https://lucide.dev) | The icons the Gallery and the reference show (drawn by Aioros's `Icon`, which ships them; `lucide-react` in `tools/shadcn`), and the paths of the check, chevron, x, toast and display icons Cosmo's themes draw (`circle-check`, `info`, `triangle-alert`, `octagon-x`, `loader-circle`, `chevron-down`, `chevron-left`, `chevrons-up-down`, `ellipsis`, `slash`); the Gallery's icon set (`GalleryIcons`), the chat examples' icons (`samples/Gallery/tools/chat.py`) and the glyph of its placeholder image (`image`, in `Assets/placeholder.png` and the reference's `placeholder.svg`) | ISC, Copyright (c) for portions of Lucide are held by Cole Bemis 2013-2022 as part of Feather (MIT); all other copyright (c) for Lucide are held by Lucide Contributors 2022 |
| [Tabler Icons](https://tabler.io/icons) 3.49.0 | Not shipped by the library: the Gallery's `folder-code` and `cloud` icons (`GalleryIcons`), as shadcn/ui's empty examples use them; `@tabler/icons-react` in `tools/shadcn` | MIT, Copyright (c) 2020-2026 Paweł Kuna |
| [Inter](https://rsms.me/inter/) 3.19 | The font of both targets (served by Aioros on the web, `Avalonia.Fonts.Inter` on the desktop); not shipped by Cosmo: the files in `tools/shadcn/fonts` used by the reference render | SIL Open Font License 1.1, Copyright (c) 2016-2020 The Inter Project Authors (`tools/shadcn/fonts/Inter-LICENSE.txt`) |
| [Geist](https://vercel.com/font) | Shipped: Geist (Regular, Medium, SemiBold, Bold) and Geist Mono (Regular, Medium), shadcn/ui's fonts, as Avalonia resources of `Aioros.Cosmo` (`Assets/Fonts`, from the `geist` package 1.5.1) | SIL Open Font License 1.1, Copyright (c) 2023 Vercel, in collaboration with basement.studio |
| Cosmo Symbols (from Adwaita Sans 50.0, GNOME's adwaita-fonts, after Inter) | Shipped: `CosmoSymbols-Regular.ttf` and `CosmoSymbols-Medium.ttf` (`Assets/Fonts`), the keyboard symbols Geist lacks (⌘ ⌥ ⌃ ⌫ ⌦ ⎋ ⇪) instanced from Adwaita Sans at its default optical size and subset, renamed as a modified version; Cosmo's sans font takes them after Geist on both targets | SIL Open Font License 1.1, Copyright (c) 2016 The Inter Project Authors, 2015-2025 Renzhi Li, 2025 Jamie Gravendeel, 2025 Florian Müllner (`Assets/Fonts/CosmoSymbols-LICENSE.txt`) |
| Cosmo Emoji (from Noto Color Emoji, Google) | Shipped: `CosmoEmoji-Regular.ttf` (`Assets/Fonts`, 62.9 KB), the emoji Cosmo's chat examples (👍 🔥 👀 😭) and its sidebar blocks' teams and pages use subset from Noto Color Emoji with their color bitmaps and renamed; Cosmo's sans font takes them after Geist and Cosmo Symbols on both targets | SIL Open Font License 1.1 (`Assets/Fonts/CosmoEmoji-LICENSE.txt`) |
| Cosmo Mono Symbols (from JetBrains Mono, JetBrains) | Shipped: `CosmoMonoSymbols-Regular.ttf` (`Assets/Fonts`, 6.2 KB), the check mark (✓) Geist Mono lacks, subset from JetBrains Mono and renamed; Cosmo's code blocks take it after Geist Mono (`CosmoFontCode`) on both targets | SIL Open Font License 1.1, Copyright 2020 The JetBrains Mono Project Authors (`Assets/Fonts/CosmoMonoSymbols-LICENSE.txt`) |
| [Noto Naskh Arabic](https://github.com/notofonts/arabic) 2.021 | Not shipped by the library: the Gallery's `NotoNaskhArabic-Regular.ttf` and `NotoNaskhArabic-Medium.ttf` (`samples/Gallery/Gallery/Assets/Fonts`, unmodified), the Arabic text of the `code-block-rtl` fixture on both targets and in the reference render | SIL Open Font License 1.1, Copyright 2022 The Noto Project Authors (`samples/Gallery/Gallery/Assets/Fonts/NotoNaskhArabic-LICENSE.txt`) |
| [@shadcn/react](https://github.com/shadcn-ui/ui/tree/main/packages/react) 0.3.1 | `src/Aioros.Cosmo/Chat` (the message scroller's behavior: its opening position, following the end and the scroll-to-end button, from `MessageScroller`; the questionnaire's navigation, validation, errors and shortcuts, from `Questionnaire`); `tools/shadcn` renders the reference with it | MIT, Copyright (c) 2023 shadcn |
| [Shiki](https://github.com/shikijs/shiki) 3.23.0 and its github-light and github-dark themes | Not shipped: the Gallery's code blocks are highlighted with it when their XAML is written (`tools/shadcn/highlight.mjs`), and `tools/shadcn` renders the reference with it; the eight code brushes in `src/Aioros.Cosmo/Chat/CodeBlock.axaml` carry the github themes' token colors (from GitHub's VS Code themes, primer/github-vscode-theme, MIT) | MIT, Copyright (c) 2021 Pine Wu, (c) 2023 Anthony Fu |
| shadcn (the package) 4.21.5 | Not shipped: its `dist/tailwind.css` utilities (`shimmer`, `scroll-fade`, `no-scrollbar`) in `tools/shadcn/app/src/shadcn-utilities.css`, for the reference render | MIT, Copyright (c) 2023 shadcn |
| [Markdig](https://github.com/xoofx/markdig) 1.3.2 | A dependency of `Aioros.Cosmo.Markdown` (not bundled): `c:Markdown` parses its text as CommonMark with it | BSD-2-Clause, Copyright (c) 2018-2019, Alexandre Mutel |
| [Streamdown](https://github.com/vercel/streamdown) 2.5.0, the [AI SDK](https://github.com/vercel/ai) (`ai` 7.0.0-canary.159, `@ai-sdk/react` 4.0.0-canary.162) | Not shipped: shadcn/ui's chat examples render their markdown and messages with them; `tools/shadcn` renders the reference with them | Apache-2.0, Copyright Vercel, Inc. |
| zod 3.25.76, @shadcn/helpers 0.2.0 | Not shipped: imported by the examples `tools/shadcn` renders | MIT |
| React, Vite, Tailwind CSS, class-variance-authority, clsx, tailwind-merge, tw-animate-css, Playwright | Not shipped: `tools/shadcn` builds and captures the reference with them | MIT, Apache-2.0 |

## shadcn/ui (MIT)

```
MIT License

Copyright (c) 2023 shadcn

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

Radix UI Primitives carries the same MIT text with "Copyright (c) 2022 WorkOS".

## sonner and vaul (MIT)

```
MIT License

Copyright (c) 2023 Emil Kowalski

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

## cmdk, TanStack Table, react-resizable-panels, Embla Carousel and Recharts (MIT)

```
MIT License

Copyright (c) 2022 Paco Coursey (cmdk)
Copyright (c) 2016 Tanner Linsley (TanStack Table)
Copyright (c) 2018 Brian Vaughn (react-resizable-panels)
Copyright (c) 2019 David Jerleke (Embla Carousel)
Copyright (c) 2015-present recharts (Recharts)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

## Lucide (ISC)

```
ISC License

Copyright (c) for portions of Lucide are held by Cole Bemis 2013-2022 as part of Feather (MIT). All other copyright
(c) for Lucide are held by Lucide Contributors 2022.

Permission to use, copy, modify, and/or distribute this software for any purpose with or without fee is hereby
granted, provided that the above copyright notice and this permission notice appear in all copies.

THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH REGARD TO THIS SOFTWARE INCLUDING ALL
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT,
INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR PERFORMANCE OF
THIS SOFTWARE.
```

## Tabler Icons (MIT)

```
MIT License

Copyright (c) 2020-2026 Paweł Kuna

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
documentation files (the "Software"), to deal in the Software without restriction, including without limitation the
rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit
persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the
Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
```

## Markdig (BSD-2-Clause)

```
Copyright (c) 2018-2019, Alexandre Mutel
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the
following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following
   disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following
   disclaimer in the documentation and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
```
