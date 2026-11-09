import { readFile, writeFile, mkdir } from "node:fs/promises"
import path from "node:path"
import { fileURLToPath } from "node:url"
import { chromium } from "playwright-core"
import { probe, formProbe, formFixtures, displayFixtures, compositeFixtures, interactive, targetProbe, overlays, overlayProbe, openOverlay } from "./capture.mjs"

const here = path.dirname(fileURLToPath(import.meta.url))
const reference = path.join(here, "out")
const cosmo = path.join(here, "cosmo")
const base = (process.argv[2] ?? "http://127.0.0.1:3031").replace(/\/$/, "")
const tolerance = 16

export const fixtures = ["button-variants", "button-sizes", "button-extra-sizes", "button-disabled", "button-icons", "badge-variants", "badge-icons", "badge-links", "card-basic", "card-plain", "card-separated", ...formFixtures(), ...displayFixtures(), ...compositeFixtures(), "date-picker-demo", "date-picker-input", "calendar-single", "calendar-range", "calendar-multiple", "calendar-disabled", "calendar-outside-hidden", ...Object.keys(overlays())]

/** How a user opens each overlay fixture on Cosmo's page: the element acted on and what shows once it is open. */
export const cosmoOverlays = {
  "popover-open": { selector: "button", wait: "[popover]:popover-open" },
  "tooltip-open": { selector: "button", wait: null },
  "kbd-tooltip-open": { selector: "button", wait: null },
  "hover-card-open": { selector: "a", wait: "[popover]:popover-open" },
  "dropdown-menu-open": { selector: "button", wait: "[popover]:popover-open" },
  "dropdown-menu-checkboxes": { selector: "button", wait: "[popover]:popover-open" },
  "context-menu-open": { selector: ".ctx > div", wait: "[popover]:popover-open" },
  "dialog-open": { selector: "button", wait: "dialog.prompt:modal" },
  "alert-dialog-open": { selector: "button", wait: "dialog.prompt:modal" },
  "sheet-open": { selector: "button", wait: "dialog.prompt:modal" },
  "sheet-top": { selector: "button", wait: "dialog.prompt:modal" },
  "drawer-open": { selector: "button", wait: "dialog.prompt:modal" },
  "sonner-stack": { selector: "button", wait: ".notice:popover-open" },
  "sonner-types": { selector: "button", wait: ".notice:popover-open" },
  "sonner-close": { selector: "button", wait: ".notice:popover-open" },
  "navigation-menu-open": { selector: "button[popovertarget=m2]", wait: "[popover]:popover-open" },
  "menubar-open": { selector: "menu button", away: true, wait: "[popover]:popover-open" },
  "command-filtered": { selector: "input", wait: null },
  "command-dialog-open": { selector: "p", wait: "dialog[open]", viewport: true },
  "combobox-open": { selector: "button", wait: "[popover]:popover-open" },
  "date-picker-open": { selector: "button", wait: "[popover]:popover-open" },
  "data-table-selected": { selector: "table", wait: null, steps: [["click", "tbody tr:nth-child(1) input[type=checkbox]"], ["click", "tbody tr:nth-child(2) input[type=checkbox]"], ["click", "thead th:nth-child(3) button"]] },
}
/** The select fixtures whose reference shows the list open: Cosmo's select is opened as a user does, its picker settled, the page moved so the fixture's left edge is the viewport's as on shadcn/ui's page (the list keeps 10 px from it). */
export const cosmoOpenLists = ["select-scrollable", "select-scrolled"]
const only = process.argv[3]?.split(",")

const diff = async (page, a, b) =>
  page.evaluate(
    async ({ a, b, tolerance }) => {
      const load = (data) => new Promise((resolve) => { const image = new Image(); image.onload = () => resolve(image); image.src = "data:image/png;base64," + data })
      const [left, right] = await Promise.all([load(a), load(b)])
      const width = Math.max(left.width, right.width), height = Math.max(left.height, right.height)
      const read = (image) => { const canvas = new OffscreenCanvas(width, height); const context = canvas.getContext("2d"); context.fillStyle = "#ff00ff"; context.fillRect(0, 0, width, height); context.drawImage(image, 0, 0); return context.getImageData(0, 0, width, height).data }
      const x = read(left), y = read(right)
      let same = 0, total = width * height, delta = 0
      for (let i = 0; i < x.length; i += 4) {
        const d = Math.max(Math.abs(x[i] - y[i]), Math.abs(x[i + 1] - y[i + 1]), Math.abs(x[i + 2] - y[i + 2]))
        delta += d
        if (d <= tolerance) same++
      }
      return { width, height, sizes: [[left.width, left.height], [right.width, right.height]], matched: same / total, meanDelta: delta / total }
    },
    { a, b, tolerance }
  )

async function main() {
  await mkdir(cosmo, { recursive: true })
  const metrics = JSON.parse(await readFile(path.join(reference, "metrics.json"), "utf8"))
  const browser = await chromium.launch({ executablePath: process.env.CHROMIUM ?? "/usr/bin/chromium", args: ["--no-sandbox", "--force-color-profile=srgb", "--disable-lcd-text"] })
  const results = {}
  for (const theme of ["light", "dark"]) {
    const context = await browser.newContext({ viewport: { width: 1000, height: 900 }, deviceScaleFactor: 1 })
    await context.addCookies([{ name: "theme", value: theme, url: base }])
    const page = await context.newPage()
    for (const fixture of fixtures.filter((name) => (!only || only.includes(name)) && cosmoOverlays[name]?.web !== false)) {
      const sized = cosmoOverlays[fixture]?.viewport ? overlays()[fixture]?.viewport : null
      await page.setViewportSize(sized ? { width: sized[0], height: sized[1] } : { width: 1000, height: 900 })
      const answer = await page.goto(`${base}/fidelity/${fixture}`)
      if (!answer.ok()) {
        continue
      }
      await page.evaluate(() => document.fonts.ready)
      if (sized) {
        await page.evaluate(() => Object.assign(document.getElementById("fixture").style, { position: "fixed", left: "0", top: "0" }))
      }
      const overlay = overlays()[fixture]
      if (overlay) {
        const cosmo = cosmoOverlays[fixture]
        await openOverlay(page, { ...overlay, ...cosmo, action: overlay.action === "none" ? "click" : overlay.action }, cosmo.selector)
        await page.evaluate(() => document.fonts.ready)
      }
      if (cosmoOpenLists.includes(fixture)) {
        await page.addStyleTag({ content: "*,*::before,*::after{transition:none!important;animation:none!important}" })
        await page.evaluate(() => {
          document.body.style.position = "relative"
          document.body.style.left = -document.getElementById("fixture").getBoundingClientRect().left + "px"
        })
        await page.click("#fixture select")
        await page.waitForFunction(() => getComputedStyle(document.querySelector("#fixture [data-aioros-scroller=down]")).display === "block", null, { polling: "raf" })
        await page.evaluate(() => new Promise((done) => requestAnimationFrame(() => requestAnimationFrame(done))))
      }
      await page.addStyleTag({ content: "*,*::before,*::after{transition:none!important;animation:none!important}" })
      const shot = sized ? await page.screenshot({ animations: "disabled" }) : await (await page.$("#fixture")).screenshot({ animations: "disabled" })
      await writeFile(path.join(cosmo, `${fixture}.${theme}.png`), shot)
      const expected = await readFile(path.join(reference, `${fixture}.${theme}.png`))
      const form = formFixtures().includes(fixture) || displayFixtures().includes(fixture) || compositeFixtures().includes(fixture)
      const entry = { pixels: await diff(page, expected.toString("base64"), shot.toString("base64")), normal: await page.evaluate(overlay ? overlayProbe() : form ? formProbe : probe) }
      if (form && interactive[fixture]) {
        entry.hover = []
        entry.focus = []
        const selector = interactive[fixture][1]
        const targets = await page.$$("#fixture " + selector)
        for (const [index, target] of targets.entries()) {
          await target.hover({ force: true })
          entry.hover.push(await page.evaluate(targetProbe(selector, index)))
        }
        await page.mouse.move(990, 890)
        for (const [index, target] of targets.entries()) {
          await target.evaluate((el) => el.focus({ focusVisible: true }))
          entry.focus.push(await page.evaluate(targetProbe(selector, index)))
        }
        await page.evaluate(() => document.activeElement?.blur())
      }
      if (fixture === "button-variants") {
        entry.hover = []
        entry.focus = []
        const buttons = await page.$$("#fixture button")
        for (const [index, button] of buttons.entries()) {
          await button.hover()
          entry.hover.push((await page.evaluate(probe))[index])
          const hovered = await button.screenshot({ animations: "disabled" })
          const target = await readFile(path.join(reference, `${fixture}.${theme}.hover-${index}.png`))
          entry.hover[index].pixels = await diff(page, target.toString("base64"), hovered.toString("base64"))
        }
        await page.mouse.move(990, 890)
        await page.focus("body")
        await page.keyboard.press("Tab")
        for (const [index] of buttons.entries()) {
          entry.focus.push((await page.evaluate(probe))[index])
          await page.keyboard.press("Tab")
        }
      }
      entry.reference = metrics[`${fixture}.${theme}`]
      results[`${fixture}.${theme}`] = entry
      console.log(`${fixture}.${theme}: ${(entry.pixels.matched * 100).toFixed(2)}% of pixels within ${tolerance}/255, mean delta ${entry.pixels.meanDelta.toFixed(2)}, sizes ${JSON.stringify(entry.pixels.sizes)}`)
    }
    await context.close()
  }
  const previous = only ? JSON.parse(await readFile(path.join(here, "out", "compare.json"), "utf8").catch(() => "{}")) : {}
  Object.assign(previous, results)
  await writeFile(path.join(here, "out", "compare.json"), JSON.stringify(previous, null, 1))
  const summary = Object.entries(previous).map(([key, entry]) => {
    const [fixture, theme] = key.split(".")
    return { fixture, theme, matched: +(entry.pixels.matched * 100).toFixed(2), meanDelta: +entry.pixels.meanDelta.toFixed(2) }
  })
  await writeFile(path.join(here, "out", "summary.json"), JSON.stringify(summary, null, 1) + "\n")
  await browser.close()
}

if (process.argv[1] === fileURLToPath(import.meta.url)) await main()
