# Fidelity

How Cosmo's components are compared with shadcn/ui's own render, and how close they are.

## Method

1. **The reference.** `tools/shadcn/app` renders each fixture with the pinned shadcn/ui sources, and `node tools/shadcn/capture.mjs` captures it in Chromium, along with every element's box and computed styles.
2. **Cosmo.** The Gallery's `/fidelity/{fixture}` pages hold the same fixtures in XAML, and `node tools/shadcn/compare.mjs http://127.0.0.1:3031` captures them the same way.
3. **The verdict.** A pixel matches when each of its three channels differs by at most 16 out of 255. A pixel only one image has counts as different.

Both sides use the same Geist and Geist Mono files.

The tests repeat this against the committed reference on every run, and they also compare computed styles and boxes element by element, at rest, under the pointer and with keyboard focus, in both themes.

## Results

Across 180 web captures, the average is 99.45% of pixels. A sample of the foundation's fixtures:

| Fixture | Light | Dark |
| --- | --- | --- |
| `button-variants` | 100.00% | 100.00% |
| `button-sizes` | 99.86% | 99.86% |
| `button-icons` | 99.52% | 99.52% |
| `badge-variants` | 100.00% | 100.00% |
| `badge-icons` | 99.30% | 99.30% |
| `card-basic` | 100.00% | 100.00% |

Those fixtures have exactly the reference's size, and every background, text and border color is identical to within 1 in 255.

## What remains different

- Icons are drawn from Aioros's Lucide set at their own stroke, and the digits of count badges sit half a pixel off.
- Different builds of Chromium antialias text differently. This can cost around 3 points, so the tests' floors leave room for it.

## Running it

```bash
cd tools/shadcn && npm ci
node capture.mjs
dotnet run --project ../../samples/Gallery/Gallery &
node compare.mjs http://127.0.0.1:3031
```

The Gallery's `/fidelity` page shows the results.
