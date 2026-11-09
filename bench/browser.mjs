// Browser metrics of one stack in Chromium (bench/gallery.sh): each page's first load in a fresh context (bytes, script
// bytes, TTFB, FCP, LCP, CLS, long-task blocking time, JS heap; medians of <runs>), then dashboard-01's interactions, each
// from the click to the state it shows (the chart redrawn, the pager's page, the selection count, the table hidden), its
// INP, and on Aioros the post's status and server time (Playwright's request timing). Prints one JSON object.
// usage: node bench/browser.mjs <base url> <aioros|nextjs> <runs> <dashboard path> <page path>...
// One load of the first page is made and discarded first, as Aioros's bench/realtime/app-metrics.mjs does.
import { createRequire } from "node:module"
import path from "node:path"

const require = createRequire(path.resolve(import.meta.dirname, "../tools/shadcn/package.json"))
const { chromium } = require("playwright-core")
const [base, stack, runsArg, dashboard, ...pages] = process.argv.slice(2)
const runs = +runsArg
const root = stack === "aioros" ? "#fixture" : "body"
const settle = +(process.env.BROWSER_SETTLE_MS || 1500)
const browser = await chromium.launch({ executablePath: process.env.CHROMIUM ?? "/usr/bin/chromium", args: ["--no-sandbox", "--disable-gpu"] })

async function open() {
  const context = await browser.newContext({ viewport: { width: 1400, height: 900 } })
  const page = await context.newPage()
  const cdp = await context.newCDPSession(page)
  await cdp.send("Network.enable")
  await cdp.send("Performance.enable")
  const net = { bytes: 0, js: 0 }, types = new Map()
  cdp.on("Network.responseReceived", (e) => types.set(e.requestId, e.type))
  cdp.on("Network.loadingFinished", (e) => { net.bytes += e.encodedDataLength; if (types.get(e.requestId) === "Script") net.js += e.encodedDataLength })
  await page.addInitScript(() => {
    window.__lcp = 0; window.__cls = 0; window.__tbt = 0; window.__ix = {}
    new PerformanceObserver((l) => { for (const e of l.getEntries()) window.__lcp = e.startTime }).observe({ type: "largest-contentful-paint", buffered: true })
    new PerformanceObserver((l) => { for (const e of l.getEntries()) if (!e.hadRecentInput) window.__cls += e.value }).observe({ type: "layout-shift", buffered: true })
    new PerformanceObserver((l) => { for (const e of l.getEntries()) window.__tbt += Math.max(0, e.duration - 50) }).observe({ type: "longtask", buffered: true })
    const ix = (l) => { for (const e of l.getEntries()) if (e.interactionId) window.__ix[e.interactionId] = Math.max(window.__ix[e.interactionId] ?? 0, e.duration) }
    new PerformanceObserver(ix).observe({ type: "event", durationThreshold: 16, buffered: true })
  })
  return { context, page, cdp, net }
}

const median = (a) => { const s = a.filter((x) => x != null).sort((x, y) => x - y); return s.length ? s[Math.floor(s.length / 2)] : null }
const result = { stack, runs, pages: {}, interactions: {} }

{
  const { context, page } = await open()
  await page.goto(base + pages[0], { waitUntil: "load" })
  await context.close()
}

for (const p of pages) {
  const samples = []
  for (let k = 0; k < runs; k++) {
    const { context, page, cdp, net } = await open()
    const response = await page.goto(base + p, { waitUntil: "load" })
    await page.waitForTimeout(settle)
    const m = await page.evaluate(() => ({
      ttfb: performance.getEntriesByType("navigation")[0].responseStart,
      fcp: performance.getEntriesByName("first-contentful-paint")[0]?.startTime ?? null,
      lcp: window.__lcp, cls: window.__cls, tbt: window.__tbt,
      dcl: performance.getEntriesByType("navigation")[0].domContentLoadedEventEnd,
    }))
    const heap = (await cdp.send("Performance.getMetrics")).metrics.find((x) => x.name === "JSHeapUsedSize").value
    samples.push({ ...m, status: response.status(), bytes: net.bytes, js: net.js, heap })
    await context.close()
  }
  const pick = (k, digits = 0) => +(median(samples.map((r) => r[k])) ?? NaN).toFixed(digits)
  result.pages[p] = { status: samples[0].status, kb: +(pick("bytes") / 1024).toFixed(1), jsKb: +(pick("js") / 1024).toFixed(1), ttfb: pick("ttfb"),
    fcp: pick("fcp"), lcp: pick("lcp"), cls: pick("cls", 3), tbt: pick("tbt"), dcl: pick("dcl"), heapMb: +(pick("heap") / 1048576).toFixed(1) }
}

const chart = `() => [...document.querySelectorAll('${root} svg')].filter((svg) => svg.getBoundingClientRect().width > 300).map((svg) => [...svg.querySelectorAll('path')].map((p) => p.getAttribute('d')).join()).join()`
const pager = `() => [...document.querySelectorAll('${root} *')].find((e) => !e.children.length && /^Page \\d+ of \\d+$/.test(e.textContent.trim()))?.textContent.trim()`
const selected = `() => [...document.querySelectorAll('${root} *')].find((e) => !e.children.length && /row\\(s\\) selected\\.$/.test(e.textContent.trim()))?.textContent.trim()`
const table = `() => [...document.querySelectorAll('${root} table')].some((t) => t.checkVisibility())`
const steps = [
  { name: "range", act: (page) => page.locator(`${root} :is(label, button)`, { hasText: /^Last 7 days$/ }).first().click(), state: chart },
  { name: "next-page", act: (page) => page.getByRole("button", { name: "Go to next page" }).first().click(), state: pager },
  { name: "select-row", act: (page) => page.locator(`${root} tbody :is(input[type=checkbox], [role=checkbox])`).first().click(), state: selected },
  { name: "tab", act: (page) => page.getByRole("tab", { name: "Key Personnel" }).first().click(), state: table },
]
const samples = Object.fromEntries(steps.map((s) => [s.name, []]))
for (let k = 0; k < runs && dashboard; k++) {
  const { context, page } = await open()
  const posts = []
  page.on("response", (response) => {
    if (response.request().method() === "POST") posts.push({ status: response.status(), request: response.request() })
  })
  await page.goto(base + dashboard, { waitUntil: "load" })
  await page.waitForTimeout(settle)
  for (const step of steps) {
    const before = await page.evaluate(step.state)
    const count = posts.length
    await page.evaluate(() => { window.__ix = {} })
    const t0 = await page.evaluate(() => performance.now())
    let ms = null, error = null
    try {
      await step.act(page)
      await page.waitForFunction(([state, before]) => (0, eval)(state)() !== before, [step.state, before], { polling: "raf", timeout: 60000 })
        if (stack === "aioros") await page.waitForFunction(() => !document.querySelector("form[aria-busy=true]"), null, { polling: "raf", timeout: 60000 })
      ms = +((await page.evaluate(() => performance.now())) - t0).toFixed(1)
      const failed = posts.slice(count).find((p) => p.status >= 400)
      if (failed) { ms = null; error = `post answered ${failed.status}` }
    } catch (e) {
      error = e.message.split("\n")[0]
    }
    await page.evaluate(() => new Promise((r) => requestAnimationFrame(() => requestAnimationFrame(() => setTimeout(r, 50)))))
    const inp = await page.evaluate(() => Math.max(0, ...Object.values(window.__ix)))
    const timed = posts.slice(count).map(({ status, request }) => {
      const t = request.timing()
      return { status, waitMs: +(t.responseStart - t.requestStart).toFixed(1), totalMs: +t.responseEnd.toFixed(1) }
    })
    samples[step.name].push({ ms, inp, error, posts: timed })
    if (error) break
  }
  await context.close()
}
for (const [name, list] of Object.entries(samples)) {
  if (!list.length) continue
  const posts = list.flatMap((s) => s.posts)
  result.interactions[name] = { ms: median(list.map((s) => s.ms)), inp: median(list.map((s) => s.inp)), samples: list.length,
    errors: list.filter((s) => s.error).map((s) => s.error),
    post: posts.length ? { status: [...new Set(posts.map((p) => p.status))], waitMs: median(posts.map((p) => p.waitMs)), totalMs: median(posts.map((p) => p.totalMs)) } : null }
}

await browser.close()
console.log(JSON.stringify(result))
