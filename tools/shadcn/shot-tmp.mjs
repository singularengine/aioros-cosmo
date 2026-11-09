import { chromium } from "playwright-core"
const [url, out, width] = process.argv.slice(2)
const browser = await chromium.launch({ executablePath: "/usr/bin/chromium", args: ["--no-sandbox"] })
const page = await browser.newPage({ viewport: { width: Number(width ?? 1440), height: 900 } })
await page.goto(url)
await page.evaluate(() => document.fonts.ready)
await page.screenshot({ path: out, fullPage: Number(width) > 500 })
console.log(await page.evaluate(() => document.documentElement.scrollWidth))
await browser.close()
