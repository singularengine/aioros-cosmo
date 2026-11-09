import { StrictMode, useEffect } from "react"
import { createRoot } from "react-dom/client"

import "./index.css"
import { fixtures } from "./fixtures"
import { shellFixtures } from "./blocks"
import { themeCss } from "./themes"

const query = new URLSearchParams(location.search)
const fixture = query.get("fixture") ?? "button-variants"
const dark = query.get("theme") === "dark"
const font = query.get("font") === "inter" ? "Inter" : "Geist"

const style = document.createElement("style")
style.textContent =
  themeCss(query.get("color") ?? "neutral", query.get("radius") ?? "0.625rem") +
  `:root { --font-reference: ${font}, "Cosmo Symbols", "Cosmo Emoji", "Noto Naskh Arabic", ui-sans-serif, system-ui, sans-serif; }`
document.head.appendChild(style)
document.documentElement.classList.toggle("dark", dark)
document.documentElement.style.colorScheme = dark ? "dark" : "light"

const Fixture = fixtures[fixture]
const frame = fixture in shellFixtures ? "w-[1280px] p-0" : "w-[640px] p-6"

function Ready() {
  useEffect(() => {
    document.fonts.ready.then(() => requestAnimationFrame(() => document.body.setAttribute("data-ready", "true")))
  }, [])
  return null
}

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <div id="fixture" data-fixture={fixture} className={`${frame} bg-background font-sans text-foreground antialiased`}>
      {Fixture ? <Fixture /> : <p>Unknown fixture {fixture}</p>}
      <Ready />
    </div>
  </StrictMode>
)
