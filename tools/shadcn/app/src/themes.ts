import gray from "../../registry/colors/gray.json"
import neutral from "../../registry/colors/neutral.json"
import slate from "../../registry/colors/slate.json"
import stone from "../../registry/colors/stone.json"
import zinc from "../../registry/colors/zinc.json"
import { baseColorsOKLCH } from "../../registry/_legacy-base-colors"

type Vars = Record<string, string>
export type Theme = { light: Vars; dark: Vars }

const bases: Record<string, Theme> = {
  neutral: neutral.cssVarsV4,
  zinc: zinc.cssVarsV4,
  slate: slate.cssVarsV4,
  stone: stone.cssVarsV4,
  gray: gray.cssVarsV4,
}

const colors = ["red", "rose", "orange", "green", "blue", "yellow", "violet"] as const

export const themes: Record<string, Theme> = {
  ...bases,
  ...Object.fromEntries(colors.map((name) => [name, baseColorsOKLCH[name] as Theme])),
}

function block(selector: string, vars: Vars, radius: string | null) {
  const lines = Object.entries(vars)
    .filter(([key]) => key !== "radius")
    .map(([key, value]) => `  --${key}: ${value};`)
  if (radius) lines.unshift(`  --radius: ${radius};`)
  return `${selector} {\n${lines.join("\n")}\n}`
}

export function themeCss(name: string, radius: string) {
  const theme = themes[name] ?? themes.neutral
  return `${block(":root", theme.light, radius)}\n${block(".dark", theme.dark, null)}\n`
}
