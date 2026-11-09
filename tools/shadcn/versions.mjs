import { readFile, writeFile, mkdir } from "node:fs/promises"
import path from "node:path"
import { fileURLToPath } from "node:url"
import { chromium } from "playwright-core"
import { chartFixtures } from "./capture.mjs"

const here = path.dirname(fileURLToPath(import.meta.url))
const out = path.join(here, "out")
const diffs = process.argv[2]
const tolerance = 16

/** Compares shadcn/ui's chart examples drawn by Recharts 3.8.0 (shadcn's pin) with the same examples drawn by Recharts 2.15.4 (Pictor's reference). */
async function main() {
  const browser = await chromium.launch({ executablePath: process.env.CHROMIUM ?? "/usr/bin/chromium", args: ["--no-sandbox"] })
  const page = await browser.newPage()
  if (diffs) await mkdir(diffs, { recursive: true })
  const results = {}
  for (const theme of ["light", "dark"]) {
    for (const fixture of chartFixtures()) {
      const three = (await readFile(path.join(out, `${fixture}.${theme}.png`))).toString("base64")
      const two = (await readFile(path.join(out, `${fixture}.recharts2.${theme}.png`))).toString("base64")
      const result = await page.evaluate(async ({ three, two, tolerance }) => {
        const load = (data) => new Promise((resolve) => { const image = new Image(); image.onload = () => resolve(image); image.src = "data:image/png;base64," + data })
        const [a, b] = await Promise.all([load(three), load(two)])
        const width = Math.max(a.width, b.width), height = Math.max(a.height, b.height)
        const read = (image) => { const canvas = new OffscreenCanvas(width, height); const context = canvas.getContext("2d"); context.fillStyle = "#ff00ff"; context.fillRect(0, 0, width, height); context.drawImage(image, 0, 0); return context.getImageData(0, 0, width, height).data }
        const x = read(a), y = read(b)
        const canvas = new OffscreenCanvas(width, height)
        const context = canvas.getContext("2d")
        const marked = context.createImageData(width, height)
        let same = 0
        for (let i = 0; i < x.length; i += 4) {
          const d = Math.max(Math.abs(x[i] - y[i]), Math.abs(x[i + 1] - y[i + 1]), Math.abs(x[i + 2] - y[i + 2]))
          const gray = (x[i] + x[i + 1] + x[i + 2]) / 12 + 160
          if (d <= tolerance) { same++; marked.data.set([gray, gray, gray, 255], i) } else marked.data.set([255, 0, 0, 255], i)
        }
        context.putImageData(marked, 0, 0)
        const blob = await canvas.convertToBlob({ type: "image/png" })
        const bytes = new Uint8Array(await blob.arrayBuffer())
        let text = ""
        for (const byte of bytes) text += String.fromCharCode(byte)
        return { sizes: [[a.width, a.height], [b.width, b.height]], matched: same / (width * height), diff: btoa(text) }
      }, { three, two, tolerance })
      if (diffs) await writeFile(path.join(diffs, `${fixture}.${theme}.png`), Buffer.from(result.diff, "base64"))
      results[`${fixture}.${theme}`] = { sizes: result.sizes, matched: +(result.matched * 100).toFixed(2) }
      console.log(`${fixture}.${theme}: Recharts 3.8.0 against 2.15.4 ${(result.matched * 100).toFixed(2)}% of pixels within ${tolerance}/255, sizes ${JSON.stringify(result.sizes)}`)
    }
  }
  await writeFile(path.join(out, "recharts-versions.json"), JSON.stringify(results, null, 1) + "\n")
  await browser.close()
}

if (process.argv[1] === fileURLToPath(import.meta.url)) await main()
