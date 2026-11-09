import { createServer } from "node:http"
import { readFile, mkdir, writeFile } from "node:fs/promises"
import path from "node:path"
import { fileURLToPath } from "node:url"
import { chromium } from "playwright-core"

const here = path.dirname(fileURLToPath(import.meta.url))
const recharts2 = process.env.RECHARTS === "2"
const dist = path.join(here, recharts2 ? "dist-recharts2" : "dist")
const out = path.join(here, "out")
const types = { ".html": "text/html", ".js": "text/javascript", ".css": "text/css", ".woff2": "font/woff2", ".woff": "font/woff", ".svg": "image/svg+xml", ".png": "image/png" }

export const fixtures = ["tokens", "button-variants", "button-sizes", "button-extra-sizes", "button-disabled", "button-icons", "badge-variants", "badge-icons", "badge-links", "card-basic", "card-plain", "card-separated", ...formFixtures(), ...displayFixtures(), ...compositeFixtures(), ...dateFixtures(), ...chartFixtures(), ...blockFixtures(), ...shellFixtures(), ...chatFixtures(), ...rheaFixtures(), ...Object.keys(overlays())]

export function formFixtures() {
  return ["input-states", "input-file", "textarea-states", "field-form", "field-content", "field-responsive", "button-invalid", "checkbox-states", "radio-group", "switch-states", "native-select", "select-trigger", "select-open", "select-scrollable", "select-scrolled", "slider-states", "slider-range", "progress", "toggle-states", "toggle-group", "input-group", "input-otp"]
}

export function displayFixtures() {
  return ["separator-demo", "skeleton-demo", "spinner-demo", "kbd-demo", "avatar-demo", "avatar-badge", "avatar-group-count", "alert-demo", "aspect-ratio-demo", "typography-headings", "typography-text", "empty-demo", "empty-outline", "item-demo", "item-variant", "item-header-footer", ...navigationFixtures()]
}

export function navigationFixtures() {
  return ["tabs-demo", "tabs-list", "tabs-line", "tabs-vertical", "tabs-vertical-line", "accordion-demo", "collapsible-demo", "collapsible-open", "breadcrumb-demo", "breadcrumb-separator", "pagination-demo", "button-group-demo", "button-group-size", "button-group-orientation", "scroll-area-demo", "scroll-area-horizontal", "table-demo", "navigation-menu-demo", "menubar-demo"]
}

export function compositeFixtures() {
  return ["sidebar-demo", "sidebar-icon", "sidebar-floating", "sidebar-inset", "command-demo", "combobox-demo", "data-table-demo", "resizable-demo", "resizable-handle", "resizable-vertical", "carousel-demo", "carousel-api", "carousel-orientation"]
}

export function dateFixtures() {
  return ["calendar-single", "calendar-range", "calendar-multiple", "calendar-dropdown", "calendar-week-numbers", "calendar-disabled", "calendar-outside-hidden", "date-picker-demo", "date-picker-input"]
}

export function blockFixtures() {
  return ["login-01", "login-02", "login-03", "login-04", "login-05", "signup-01", "signup-02", "signup-03", "signup-04", "signup-05"]
}

/** The app shell blocks (dashboard-01, sidebar-01 to sidebar-16), captured in a 1280 × 800 viewport they fill. */
export function shellFixtures() {
  return ["dashboard-01", ...Array.from({ length: 16 }, (_, index) => `sidebar-${String(index + 1).padStart(2, "0")}`)]
}

export function chatFixtures() {
  return ["message-demo", "message-group", "message-header-footer", "message-avatar", "bubble-demo", "bubble-variants", "bubble-alignment", "bubble-group-demo", "marker-demo", "marker-variants", "marker-icon", "marker-border", "marker-shimmer", "marker-status", "marker-link-button", "bubble-link-button", "bubble-collapsible", "attachment-trigger", "attachment-image", "attachment-demo", "attachment-states", "attachment-sizes", "attachment-group", "message-scroller-end", "message-scroller-start", "bubble-markdown"]
}

/** The radix base's code-block, trace and questionnaire examples, drawn in the radix-rhea style (app/src/rhea.tsx). */
export function rheaFixtures() {
  return ["code-block-demo", "code-block-basic", "code-block-header", "code-block-line-numbers", "code-block-highlighted-lines", "code-block-diff", "code-block-destructive", "code-block-size", "code-block-highlighting", "code-block-actions", "code-block-ansi", "code-block-chat", "code-block-floating-actions", "code-block-height", "code-block-languages", "code-block-markdown", "code-block-rtl", "code-block-show-more-scroll", "code-block-show-more", "code-block-start-line", "code-block-streaming", "code-block-tabs", "code-block-trace", "code-block-typeset", "code-block-wrap", "trace-static", "trace-status", "trace-description", "trace-group", "trace-collapsible", "trace-steps", "questionnaire-demo", "questionnaire-multiple", "questionnaire-freeform", "questionnaire-animated", "questionnaire-card", "questionnaire-conditional", "questionnaire-controlled", "questionnaire-dialog", "questionnaire-navigation-state", "questionnaire-progress", "questionnaire-resume", "questionnaire-shortcuts", "questionnaire-skip", "questionnaire-validation", "trace-commit", "trace-demo", "trace-dialog", "trace-link", "trace-open", "trace-sheet", "trace-sources", "trace-thinking", "trace-tool-calls", "trace-turn"]
}

export function chartFixtures() {
  return ["chart-area-default", "chart-area-gradient", "chart-area-stacked", "chart-area-interactive", "chart-bar-default", "chart-bar-multiple", "chart-bar-horizontal", "chart-bar-label", "chart-bar-interactive", "chart-line-default", "chart-line-multiple", "chart-line-dots", "chart-line-interactive", "chart-pie-simple", "chart-pie-donut", "chart-pie-donut-text", "chart-pie-legend", "chart-radar-default", "chart-radar-dots", "chart-radar-multiple", "chart-radial-simple", "chart-radial-text", "chart-radial-stacked", "chart-tooltip-default", "chart-tooltip-indicator-line", "chart-tooltip-indicator-none"]
}

/**
 * How a fixture is captured: a page running scroll-driven animations (shadcn/ui's scroll-fade utilities show their fades
 * by them) is stilled by hand, a finite time-based animation finished and an endless one cancelled as Playwright's
 * `animations: "disabled"` does, and captured with its scroll-driven animations left running; any other page is
 * captured with Playwright's own `"disabled"`.
 */
async function still(page) {
  return await page.evaluate(() => {
    const animations = document.getAnimations()
    const scrolled = (animation) => typeof ScrollTimeline !== "undefined" && animation.timeline instanceof ScrollTimeline
    if (!animations.some(scrolled)) return "disabled"
    for (const animation of animations.filter((animation) => !scrolled(animation))) {
      if (animation.effect?.getComputedTiming().iterations === Infinity) animation.cancel()
      else animation.finish()
    }
    return "allow"
  })
}

/** Waits until every chart's markup stays the same for 45 animation frames in a row (Recharts animates in script, its pies after 400 ms). */
export async function settleCharts(page) {
  await page.waitForFunction(() => {
    const now = [...document.querySelectorAll("#fixture svg")].map((svg) => svg.innerHTML).join("|")
    window.__steady = now === window.__markup ? (window.__steady ?? 0) + 1 : 0
    window.__markup = now
    return window.__steady >= 45
  }, null, { polling: "raf", timeout: 10000 })
}

export function overlays() {
  return {
    "popover-open": { action: "click", selector: "button", away: true, wait: "[data-slot=popover-content]" },
    "tooltip-open": { action: "hover", selector: "button", wait: "[data-slot=tooltip-content]" },
    "kbd-tooltip-open": { action: "hover", selector: "button", wait: "[data-slot=tooltip-content]" },
    "scroll-area-hover": { action: "hover", selector: "[data-slot=scroll-area]" },
    "hover-card-open": { action: "hover", selector: "button", wait: "[data-slot=hover-card-content]" },
    "dropdown-menu-open": { action: "click", selector: "button", away: true, wait: "[data-slot=dropdown-menu-content]" },
    "dropdown-menu-checkboxes": { action: "click", selector: "button", away: true, wait: "[data-slot=dropdown-menu-content]" },
    "context-menu-open": { action: "contextmenu", selector: "[data-slot=context-menu-trigger]", away: true, wait: "[data-slot=context-menu-content]" },
    "dialog-open": { action: "click", selector: "button", viewport: [640, 480], wait: "[data-slot=dialog-content]" },
    "alert-dialog-open": { action: "click", selector: "button", viewport: [640, 360], wait: "[data-slot=alert-dialog-content]" },
    "sheet-open": { action: "click", selector: "button", viewport: [640, 600], wait: "[data-slot=sheet-content]" },
    "sheet-top": { action: "click", selector: "button", viewport: [640, 360], wait: "[data-slot=sheet-content]" },
    "drawer-open": { action: "click", selector: "button", viewport: [640, 600], wait: "[data-slot=drawer-content]" },
    "sonner-stack": { action: "none", selector: "button", viewport: [640, 360], wait: "[data-sonner-toast]" },
    "sonner-types": { action: "none", selector: "button", viewport: [640, 260], wait: "[data-sonner-toast]" },
    "sonner-close": { action: "none", selector: "button", viewport: [640, 260], wait: "[data-sonner-toast]" },
    "navigation-menu-open": { action: "click", selector: "li:nth-child(2) > button", wait: "[data-slot=navigation-menu-content]" },
    "menubar-open": { action: "click", selector: "[data-slot=menubar-trigger]", away: true, wait: "[data-slot=menubar-content]" },
    "command-filtered": { action: "type", selector: "[data-slot=command-input]", text: "se", away: true, wait: "[data-slot=command-item]" },
    "command-dialog-open": { action: "press", selector: "p", key: "Control+j", viewport: [640, 480], wait: "[data-slot=dialog-content]" },
    "combobox-open": { action: "click", selector: "button", away: true, wait: "[data-slot=popover-content]" },
    "date-picker-open": { action: "click", selector: "button", away: true, wait: "[data-slot=popover-content]" },
    "date-picker-range-open": { action: "click", selector: "button", away: true, wait: "[data-slot=popover-content]" },
    "date-picker-presets-open": { action: "click", selector: "button", away: true, wait: "[data-slot=popover-content]" },
    "data-table-selected": { action: "steps", selector: "table", away: true, steps: [["click", "tbody tr:nth-child(1) [data-slot=checkbox]"], ["click", "tbody tr:nth-child(2) [data-slot=checkbox]"], ["click", "thead th:nth-child(3) button"]] },
  }
}

export const overlayProbe = () => `(() => {
  const pick = ${pick}
  return [...document.querySelectorAll("#fixture button, [data-slot], [data-sonner-toast], [data-sonner-toast] [data-title], [data-sonner-toast] [data-description], [data-sonner-toast] [data-icon], [data-sonner-toast] [data-button], [data-sonner-toast] [data-close-button]")]
    .filter((el) => el.getClientRects().length > 0).map((el) => ({ ...pick(el), slot: el.getAttribute("data-slot") ?? (el.hasAttribute("data-sonner-toast") ? "toast" : el.hasAttribute("data-title") ? "toast-title" : el.hasAttribute("data-description") ? "toast-description" : el.hasAttribute("data-icon") ? "toast-icon" : el.hasAttribute("data-button") ? "toast-action" : el.hasAttribute("data-close-button") ? "toast-close" : "") }))
})()`

/** Opens a fixture's overlay as a user does, and waits for it to stand still. */
export async function openOverlay(page, overlay, selector) {
  const target = await page.$("#fixture " + (selector ?? overlay.selector))
  const box = target ? await target.boundingBox() : null
  if (overlay.action === "click") await target.click()
  if (overlay.action === "hover") await target.hover()
  if (overlay.action === "type") { await target.click(); await page.keyboard.type(overlay.text) }
  if (overlay.action === "press") await page.keyboard.press(overlay.key)
  if (overlay.action === "steps") for (const [step, where] of overlay.steps) if (step === "click") await page.click("#fixture " + where)
  if (overlay.action === "contextmenu") await page.mouse.click(box.x + box.width / 2, box.y + box.height / 2, { button: "right" })
  if (overlay.wait) await page.waitForSelector(overlay.wait, { state: "visible" })
  if (overlay.away) {
    const fixture = await (await page.$("#fixture")).boundingBox()
    await page.mouse.move(fixture.x + 2, fixture.y + fixture.height - 2)
  }
  await page.waitForFunction(() => document.getAnimations().every((animation) => animation.playState !== "running"))
  await page.evaluate(() => document.activeElement?.blur?.())
}

export const interactive = {
  "badge-links": ["a"],
  "tabs-line": ["button"],
  "button-invalid": ["button"],
  "input-states": ["[data-slot=input]", "input"],
  "field-form": ["[data-slot=input]", "input:not([type=checkbox])"],
  "textarea-states": ["[data-slot=textarea]", "textarea"],
  "checkbox-states": ["[data-slot=checkbox]", "input[type=checkbox], .tri"],
  "radio-group": ["[data-slot=radio-group-item]", "input[type=radio]"],
  "switch-states": ["[data-slot=switch]", "input[role=switch]"],
  "native-select": ["[data-slot=native-select]", "select"],
  "select-trigger": ["[data-slot=select-trigger]", "select"],
  "slider-states": ["[data-slot=slider-thumb]", "input[type=range]"],
  "toggle-states": ["[data-slot=toggle]", "label"],
  "toggle-group": ["[data-slot=toggle-group-item]", "label"],
  "input-group": ["[data-slot=input-group-control]", "input, textarea"],
  "tabs-list": ["[data-slot=tabs-trigger]", "label"],
  "accordion-demo": ["[data-slot=accordion-trigger]", "summary"],
  "breadcrumb-demo": ["[data-slot=breadcrumb-link]", "a"],
}
export const colors = ["neutral", "zinc", "slate", "stone", "gray", "red", "rose", "orange", "green", "blue", "yellow", "violet"]

export function serve() {
  const server = createServer(async (request, response) => {
    const url = new URL(request.url, "http://localhost")
    const file = path.join(dist, url.pathname === "/" ? "index.html" : decodeURIComponent(url.pathname))
    try {
      const body = await readFile(file)
      response.writeHead(200, { "content-type": types[path.extname(file)] ?? "application/octet-stream" })
      response.end(body)
    } catch {
      response.writeHead(404)
      response.end()
    }
  })
  return new Promise((resolve) => server.listen(0, "127.0.0.1", () => resolve(server)))
}

export const pick = `(el) => {
    const s = getComputedStyle(el), r = el.getBoundingClientRect(), root = document.getElementById("fixture").getBoundingClientRect()
    return { tag: el.tagName.toLowerCase(), text: el.textContent.trim(), x: r.x - root.x, y: r.y - root.y, width: r.width, height: r.height,
      background: s.backgroundColor, color: s.color, borderColor: s.borderTopColor, borderWidth: s.borderTopWidth, radius: s.borderTopLeftRadius,
      shadow: s.boxShadow, fontSize: s.fontSize, fontWeight: s.fontWeight, lineHeight: s.lineHeight, opacity: s.opacity, decoration: s.textDecorationLine,
      outline: s.outlineStyle + " " + s.outlineWidth, cursor: s.cursor, paddingLeft: s.paddingLeft, paddingTop: s.paddingTop,
      paddingRight: s.paddingRight, paddingBottom: s.paddingBottom, slot: el.getAttribute("data-slot") ?? "", state: el.getAttribute("data-state") ?? "" }
  }`

export const probe = `(() => {
  const pick = ${pick}
  return [...document.querySelectorAll("#fixture button, #fixture [data-slot=badge], #fixture [data-slot^=card], #fixture [data-cosmo]")].map(pick)
})()`

export const formProbe = `(() => {
  const pick = ${pick}
  return [...document.querySelectorAll("#fixture button, #fixture [data-slot], [data-slot=select-content], [data-slot=select-content] [data-slot], #fixture [data-cosmo]")].map(pick)
})()`

export const targetProbe = (selector, index) => `(() => {
  const pick = ${pick}
  return pick(document.querySelectorAll(${JSON.stringify("#fixture " + selector)})[${index}])
})()`

export const pixel = (png, width, x, y) => {
  const i = (Math.round(y) * width + Math.round(x)) * 4
  return [png[i], png[i + 1], png[i + 2]]
}

async function main() {
  const server = await serve()
  const base = `http://127.0.0.1:${server.address().port}/`
  await mkdir(out, { recursive: true })
  const browser = await chromium.launch({ executablePath: process.env.CHROMIUM ?? "/usr/bin/chromium", args: ["--no-sandbox", "--force-color-profile=srgb", "--disable-lcd-text"] })
  const page = await browser.newPage({ viewport: { width: 1000, height: 900 }, deviceScaleFactor: 1 })
  const shellPage = await browser.newPage({ viewport: { width: 1280, height: 800 }, deviceScaleFactor: 1 })
  await shellPage.clock.setFixedTime(new Date(2026, 9, 16, 12))
  const metrics = {}
  const only = process.argv[2]?.split(",")
  const previous = only ? JSON.parse(await readFile(path.join(out, "metrics.json"), "utf8")) : {}
  for (const theme of ["light", "dark"]) {
    for (const fixture of fixtures.filter((name) => (!only || only.includes(name)) && (!recharts2 || chartFixtures().includes(name)))) {
      const shot = recharts2 ? `${fixture}.recharts2.${theme}` : `${fixture}.${theme}`
      const overlay = overlays()[fixture]
      if (overlay) {
        const [width, height] = overlay.viewport ?? [1000, 900]
        const context = await browser.newContext({ viewport: { width, height }, deviceScaleFactor: 1, colorScheme: theme })
        const shown = await context.newPage()
        await shown.goto(`${base}?fixture=${fixture}&theme=${theme}`)
        await shown.waitForSelector("body[data-ready=true]")
        await openOverlay(shown, overlay)
        await (await shown.$("#fixture")).screenshot({ path: path.join(out, `${fixture}.${theme}.png`), animations: await still(shown) })
        metrics[`${fixture}.${theme}`] = { normal: await shown.evaluate(overlayProbe()) }
        if (fixture === "sonner-close") {
          await shown.hover("[data-close-button]")
          await shown.waitForFunction(() => document.getAnimations().every((animation) => animation.playState !== "running"))
          metrics[`${fixture}.${theme}`].hover = (await shown.evaluate(overlayProbe())).filter((element) => element.slot === "toast-close")
        }
        await context.close()
        continue
      }
      const current = shellFixtures().includes(fixture) ? shellPage : page
      await current.goto(`${base}?fixture=${fixture}&theme=${theme}`)
      await current.waitForSelector("body[data-ready=true]")
      if (chartFixtures().includes(fixture) || fixture === "dashboard-01") await settleCharts(current)
      if (["code-block-demo", "code-block-highlighting"].includes(fixture)) await current.waitForSelector("#fixture [style*='--shiki']")
      const element = await current.$("#fixture")
      await element.screenshot({ path: path.join(out, `${shot}.png`), animations: await still(current) })
      await current.addStyleTag({ content: "*,*::before,*::after{transition:none!important;animation:none!important}" })
      const form = formFixtures().includes(fixture) || displayFixtures().includes(fixture) || compositeFixtures().includes(fixture) || dateFixtures().includes(fixture) || chartFixtures().includes(fixture) || blockFixtures().includes(fixture) || shellFixtures().includes(fixture) || chatFixtures().includes(fixture) || rheaFixtures().includes(fixture)
      const entry = { normal: await current.evaluate(form ? formProbe : probe) }
      if (interactive[fixture]) {
        entry.hover = []
        entry.focus = []
        const selector = interactive[fixture][0]
        const targets = await current.$$("#fixture " + selector)
        for (const [index, target] of targets.entries()) {
          await target.hover({ force: true })
          entry.hover.push(await current.evaluate(targetProbe(selector, index)))
        }
        await current.mouse.move(990, 890)
        for (const [index, target] of targets.entries()) {
          await target.evaluate((el) => el.focus({ focusVisible: true }))
          entry.focus.push(await current.evaluate(targetProbe(selector, index)))
        }
        await current.evaluate(() => document.activeElement?.blur())
      }
      if (fixture.startsWith("button-variants")) {
        entry.hover = []
        entry.focus = []
        const buttons = await current.$$("#fixture button")
        for (const [index, button] of buttons.entries()) {
          await button.hover()
          entry.hover.push((await current.evaluate(probe))[index])
          await button.screenshot({ path: path.join(out, `${fixture}.${theme}.hover-${index}.png`), animations: "disabled" })
        }
        await current.mouse.move(700, 880)
        await current.keyboard.press("Tab")
        for (const [index, button] of buttons.entries()) {
          entry.focus.push((await current.evaluate(probe))[index])
          await current.keyboard.press("Tab")
        }
      }
      metrics[shot] = entry
    }
    for (const color of only || recharts2 ? [] : colors) {
      await page.goto(`${base}?fixture=tokens&theme=${theme}&color=${color}`)
      await page.waitForSelector("body[data-ready=true]")
      metrics[`tokens.${color}.${theme}`] = await page.evaluate(() => {
        const canvas = document.createElement("canvas")
        canvas.width = canvas.height = 1
        const context = canvas.getContext("2d", { colorSpace: "srgb", willReadFrequently: true })
        const root = getComputedStyle(document.documentElement)
        return Object.fromEntries([...document.querySelectorAll("[data-token]")].map((el) => {
          const value = root.getPropertyValue(`--${el.dataset.token}`).trim()
          context.clearRect(0, 0, 1, 1)
          context.fillStyle = value
          context.fillRect(0, 0, 1, 1)
          return [el.dataset.token, { css: value, rgba: [...context.getImageData(0, 0, 1, 1).data] }]
        }))
      })
    }
  }
  await writeFile(path.join(out, "metrics.json"), JSON.stringify({ ...previous, ...metrics }, null, 1))
  await browser.close()
  server.close()
}

if (process.argv[1] === fileURLToPath(import.meta.url)) await main()
