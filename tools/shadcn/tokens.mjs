import { readFile, writeFile, mkdir } from "node:fs/promises"
import path from "node:path"
import { fileURLToPath } from "node:url"
import { parse, toSrgb, hex } from "./oklch.mjs"

const here = path.dirname(fileURLToPath(import.meta.url))
const root = path.resolve(here, "../..")
const theming = path.join(root, "src/Aioros.Cosmo/Theming")

export const tokens = ["background", "foreground", "card", "card-foreground", "popover", "popover-foreground", "primary", "primary-foreground", "secondary", "secondary-foreground", "muted", "muted-foreground", "accent", "accent-foreground", "destructive", "border", "input", "ring", "chart-1", "chart-2", "chart-3", "chart-4", "chart-5", "sidebar", "sidebar-foreground", "sidebar-primary", "sidebar-primary-foreground", "sidebar-accent", "sidebar-accent-foreground", "sidebar-border", "sidebar-ring"]
export const bases = ["neutral", "zinc", "slate", "stone", "gray"]
export const accents = ["red", "rose", "orange", "green", "blue", "yellow", "violet"]
export const radii = [["0", 0, "None"], ["0.3", 0.3, "Small"], ["0.5", 0.5, "Medium"], ["0.625", 0.625, "Default"], ["0.75", 0.75, "Large"], ["1", 1, "ExtraLarge"]]
export const scale = [["Sm", 0.6], ["Md", 0.8], ["Lg", 1], ["Xl", 1.4], ["2Xl", 1.8], ["3Xl", 2.2], ["4Xl", 2.6]]

export const components = [
  ["ButtonPrimaryHover", ["primary", 0.9], ["primary", 0.9]],
  ["MutedForegroundThird", ["muted-foreground", 0.3], ["muted-foreground", 0.3]],
  ["ButtonDestructive", ["destructive", 1], ["destructive", 0.6]],
  ["ButtonDestructiveHover", ["destructive", 0.9], ["destructive", 0.6]],
  ["ButtonDestructiveFocusRing", ["destructive", 0.2], ["destructive", 0.4]],
  ["ButtonOutline", ["background", 1], ["input", 0.3]],
  ["ButtonOutlineBorder", ["border", 1], ["input", 1]],
  ["ButtonOutlineHover", ["accent", 1], ["input", 0.5]],
  ["ButtonOutlineFocusBorder", ["ring", 1], ["input", 1]],
  ["ButtonOutlineInvalidBorder", ["destructive", 1], ["input", 1]],
  ["ButtonSecondaryHover", ["secondary", 0.8], ["secondary", 0.8]],
  ["ButtonGhostHover", ["accent", 1], ["accent", 0.5]],
  ["FocusRing", ["ring", 0.5], ["ring", 0.5]],
  ["BadgeDestructive", ["destructive", 1], ["destructive", 0.6]],
  ["BadgeSecondaryHover", ["secondary", 0.9], ["secondary", 0.9]],
  ["KbdTooltip", ["background", 0.2], ["background", 0.1]],
  ["BadgeDestructiveHover", ["destructive", 0.9], ["destructive", 0.9]],
  ["InputBackground", ["background", 0], ["input", 0.3]],
  ["InputHoverBackground", ["background", 0], ["input", 0.5]],
  ["SwitchOff", ["input", 1], ["input", 0.8]],
  ["SwitchThumb", ["background", 1], ["foreground", 1]],
  ["SwitchThumbOn", ["background", 1], ["primary-foreground", 1]],
  ["ProgressTrack", ["primary", 0.2], ["primary", 0.2]],
  ["ChoiceChecked", ["primary", 0.05], ["primary", 0.1]],
  ["MenuDestructiveFocus", ["destructive", 0.1], ["destructive", 0.2]],
  ["MutedHalf", ["muted", 0.5], ["muted", 0.5]],
  ["TabsTrigger", ["foreground", 0.6], ["muted-foreground", 1]],
  ["TabsActive", ["background", 1], ["input", 0.3]],
  ["TabsActiveBorder", ["background", 0], ["input", 1]],
  ["TabsLineFocusBorder", ["ring", 1], ["ring", 0]],
  ["TabsActiveFocusBorder", ["ring", 1], ["input", 1]],
  ["AccentHalf", ["accent", 0.5], ["accent", 0.5]],
  ["AlertDestructiveDescription", ["destructive", 0.9], ["destructive", 0.9]],
  ["ChartTooltipBorder", ["border", 0.5], ["border", 0.5]],
  ["BubbleDestructive", ["destructive", 0.1], ["destructive", 0.2]],
  ["BubbleHover", ["primary", 0.8], ["primary", 0.8]],
  ["BubbleOutlineHover", ["muted", 1], ["input", 0.3]],
  ["BubbleGhostHover", ["muted", 1], ["muted", 0.5]],
  ["BubbleDestructiveHover", ["destructive", 0.2], ["destructive", 0.3]],
  ["AttachmentErrorBorder", ["destructive", 0.3], ["destructive", 0.3]],
  ["AttachmentErrorMedia", ["destructive", 0.1], ["destructive", 0.1]],
  ["AttachmentErrorDescription", ["destructive", 0.8], ["destructive", 0.8]],
  ["CodeBlockRing", ["foreground", 0.05], ["foreground", 0.1]],
  ["MarkdownQuote", ["muted-foreground", 0.3], ["muted-foreground", 0.3]],
  ["CodeBlockButtonHover", ["foreground", 0.05], ["foreground", 0.1]],
  ["CodeBlockDestructive", ["destructive", 0.05], ["destructive", 0.1]],
  ["CodeBlockDestructiveRing", ["destructive", 0.15], ["destructive", 0.2]],
  ["CodeBlockLineHighlighted", ["foreground", 0.05], ["foreground", 0.05]],
  ["CodeBlockLineRemoved", ["destructive", 0.1], ["destructive", 0.1]],
  ["TraceDescription", ["muted-foreground", 0.7], ["muted-foreground", 0.7]],
  ["QuestionnaireChoiceHover", ["input", 0.4], ["input", 0.4]],
  ["QuestionnaireChoiceChecked", ["primary", 0.1], ["primary", 0.1]],
  ["QuestionnaireChoiceCheckedBorder", ["primary", 0.4], ["primary", 0.4]],
  ["QuestionnaireIndicator", ["input", 0.9], ["input", 0.9]],
  ["QuestionnaireInput", ["input", 0.5], ["input", 0.5]],
  ["QuestionnaireShortcut", ["background", 0.8], ["background", 0.8]],
  ["QuestionnaireShortcutBorder", ["primary", 0.1], ["primary", 0.1]],
]

// Brushes shadcn/ui derives with CSS relative colors: the tinted bubble is oklch(from var(--primary) 0.93 calc(c*0.4) h),
// 0.3 in the dark (registry/ui/bubble.tsx). Sonner's close button is gray12 on a gray4 border in the light, the toast's
// text and border in the dark (sonner 2.0.7's styles.css).
// CSS Color 5's color-mix(in oklch, a, b share): lightness and chroma premultiplied by alpha, the hue the shorter way
// round (an achromatic color's missing hue takes the other's).
const mixed = (a, b, share) => {
  const alpha = a.alpha * (1 - share) + b.alpha * share
  const hueA = a.c === 0 ? b.h : a.h, hueB = b.c === 0 ? a.h : b.h
  let delta = hueB - hueA
  delta -= 360 * Math.round(delta / 360)
  return {
    l: (a.l * a.alpha * (1 - share) + b.l * b.alpha * share) / alpha,
    c: (a.c * a.alpha * (1 - share) + b.c * b.alpha * share) / alpha,
    h: (hueA + delta * share + 360) % 360,
    alpha,
  }
}

export const derived = [
  ["BubbleTinted", (mode, side) => { const primary = parse(mode.primary); return hex(toSrgb({ ...primary, l: side === 1 ? 0.93 : 0.3, c: primary.c * 0.4 })) }],
  ["BubbleTintedHover", (mode, side) => { const primary = parse(mode.primary); return hex(toSrgb({ ...primary, l: side === 1 ? 0.88 : 0.35, c: primary.c * 0.5 })) }],
  ["BubbleSecondaryHover", (mode) => hex(toSrgb(mixed(parse(mode.secondary), parse(mode.foreground), 0.05)))],
  ["BubbleMutedHover", (mode) => hex(toSrgb(mixed(parse(mode.muted), parse(mode.foreground), 0.05)))],
  ["ToastCloseForeground", (mode, side) => side === 1 ? "#171717" : faded(mode["popover-foreground"], 1)],
  ["ToastCloseBorder", (mode, side) => side === 1 ? "#EDEDED" : faded(mode.border, 1)],
]

const states = ["", "PointerOver", "Pressed", "Disabled"]
const checkBox = (part, state, light, dark) => states.map((suffix) => [`CheckBox${part}${state}${suffix}`, light, dark])
const keyed = (prefix, light, dark) => states.map((suffix) => [`${prefix}${suffix}`, light, dark])
const transparent = ["background", 0]

export const fluent = [
  ["SystemControlErrorTextForegroundBrush", ["destructive", 1], ["destructive", 1]],
  ...checkBox("CheckBackgroundStroke", "Unchecked", ["input", 1], ["input", 1]),
  ...checkBox("CheckBackgroundFill", "Unchecked", ["background", 0], ["input", 0.3]),
  ...checkBox("CheckGlyphForeground", "Unchecked", ["foreground", 1], ["foreground", 1]),
  ...checkBox("CheckBackgroundStroke", "Checked", ["primary", 1], ["primary", 1]),
  ...checkBox("CheckBackgroundFill", "Checked", ["primary", 1], ["primary", 1]),
  ...checkBox("CheckGlyphForeground", "Checked", ["primary-foreground", 1], ["primary-foreground", 1]),
  ...checkBox("CheckBackgroundStroke", "Indeterminate", ["input", 1], ["input", 1]),
  ...checkBox("CheckBackgroundFill", "Indeterminate", ["background", 0], ["input", 0.3]),
  ...checkBox("CheckGlyphForeground", "Indeterminate", ["foreground", 1], ["foreground", 1]),
  ...checkBox("Foreground", "Unchecked", ["foreground", 1], ["foreground", 1]),
  ...checkBox("Foreground", "Checked", ["foreground", 1], ["foreground", 1]),
  ...checkBox("Foreground", "Indeterminate", ["foreground", 1], ["foreground", 1]),
  ...keyed("RadioButtonOuterEllipseStroke", ["input", 1], ["input", 1]),
  ...keyed("RadioButtonOuterEllipseFill", transparent, ["input", 0.3]),
  ...keyed("RadioButtonOuterEllipseCheckedStroke", ["input", 1], ["input", 1]),
  ...keyed("RadioButtonOuterEllipseCheckedFill", ["background", 1], ["background", 1]),
  ...keyed("RadioButtonCheckGlyphFill", ["primary", 1], ["primary", 1]),
  ...keyed("RadioButtonForeground", ["foreground", 1], ["foreground", 1]),
  ...keyed("ToggleSwitchFillOff", ["input", 1], ["input", 0.8]),
  ...keyed("ToggleSwitchStrokeOff", transparent, transparent),
  ...keyed("ToggleSwitchFillOn", ["primary", 1], ["primary", 1]),
  ...keyed("ToggleSwitchStrokeOn", transparent, transparent),
  ...keyed("ToggleSwitchKnobFillOff", ["background", 1], ["foreground", 1]),
  ...keyed("ToggleSwitchKnobFillOn", ["background", 1], ["primary-foreground", 1]),
  ["ToggleSwitchContentForeground", ["foreground", 1], ["foreground", 1]],
  ["ToggleButtonBackgroundPointerOver", ["muted", 1], ["muted", 1]],
  ["ToggleButtonBackgroundPressed", ["muted", 1], ["muted", 1]],
  ["ToggleButtonBackgroundDisabled", transparent, transparent],
  ["ToggleButtonForegroundPointerOver", ["muted-foreground", 1], ["muted-foreground", 1]],
  ["ToggleButtonForegroundPressed", ["muted-foreground", 1], ["muted-foreground", 1]],
  ["ToggleButtonForegroundDisabled", ["foreground", 1], ["foreground", 1]],
  ...["Checked", "CheckedPointerOver", "CheckedPressed", "CheckedDisabled"].flatMap((state) => [
    [`ToggleButtonBackground${state}`, ["accent", 1], ["accent", 1]],
    [`ToggleButtonForeground${state}`, ["accent-foreground", 1], ["accent-foreground", 1]],
  ]),
  ...["PointerOver", "Pressed", "Disabled", "Checked", "CheckedPointerOver", "CheckedPressed", "CheckedDisabled"].map((state) => [`ToggleButtonBorderBrush${state}`, ["input", 1], ["input", 1]]),
  ["FlyoutPresenterBackground", ["popover", 1], ["popover", 1]],
  ["FlyoutBorderThemeBrush", ["border", 1], ["border", 1]],
  ["MenuFlyoutPresenterBackground", ["popover", 1], ["popover", 1]],
  ["MenuFlyoutPresenterBorderBrush", ["border", 1], ["border", 1]],
  ["MenuFlyoutItemForeground", ["popover-foreground", 1], ["popover-foreground", 1]],
  ["MenuFlyoutItemBackgroundPointerOver", ["accent", 1], ["accent", 1]],
  ["MenuFlyoutItemForegroundPointerOver", ["accent-foreground", 1], ["accent-foreground", 1]],
  ["MenuFlyoutItemKeyboardAcceleratorTextForeground", ["muted-foreground", 1], ["muted-foreground", 1]],
  ["MenuFlyoutSubItemChevron", ["muted-foreground", 1], ["muted-foreground", 1]],
  ["ToolTipBackground", ["foreground", 1], ["foreground", 1]],
  ["ToolTipForeground", ["background", 1], ["background", 1]],
  ["ToolTipBorderBrush", ["foreground", 1], ["foreground", 1]],
  ["NotificationCardBackgroundBrush", ["popover", 1], ["popover", 1]],
  ["NotificationCardForegroundBrush", ["popover-foreground", 1], ["popover-foreground", 1]],
]

// Aioros.Pictor's theme keys (Theming/PictorKeys.cs), from shadcn/ui's chart tokens and ChartContainer's overrides of
// Recharts' defaults (registry/ui/chart.tsx): the series palette, the grid at border/50, the ticks muted, the cursors,
// the radial bars' background muted, the white strokes of dots and sectors transparent, and ChartTooltipContent's box.
export const pictor = [
  ["PictorPalette1", ["chart-1", 1], ["chart-1", 1]],
  ["PictorPalette2", ["chart-2", 1], ["chart-2", 1]],
  ["PictorPalette3", ["chart-3", 1], ["chart-3", 1]],
  ["PictorPalette4", ["chart-4", 1], ["chart-4", 1]],
  ["PictorPalette5", ["chart-5", 1], ["chart-5", 1]],
  ["PictorSeriesColor", ["chart-1", 1], ["chart-1", 1]],
  ["PictorPrimary", ["primary", 1], ["primary", 1]],
  ["PictorGridStroke", ["border", 0.5], ["border", 0.5]],
  ["PictorTickText", ["muted-foreground", 1], ["muted-foreground", 1]],
  ["PictorPolarTickText", ["muted-foreground", 1], ["muted-foreground", 1]],
  ["PictorCursorStroke", ["border", 1], ["border", 1]],
  ["PictorCursorFill", ["muted", 1], ["muted", 1]],
  ["PictorActiveDotStroke", ["background", 0], ["background", 0]],
  ["PictorDotFill", "#FFFFFF", "#FFFFFF"],
  ["PictorPolarGridStroke", ["border", 1], ["border", 1]],
  ["PictorReferenceLineStroke", ["border", 1], ["border", 1]],
  ["PictorBarBackground", ["muted", 1], ["muted", 1]],
  ["PictorPieStroke", ["background", 0], ["background", 0]],
  ["PictorTooltipBackground", ["background", 1], ["background", 1]],
  ["PictorTooltipBorder", ["border", 0.5], ["border", 0.5]],
  ["PictorTooltipLabel", ["foreground", 1], ["foreground", 1]],
  ["PictorTooltipText", ["foreground", 1], ["foreground", 1]],
  ["CosmoChartForeground", ["foreground", 1], ["foreground", 1]],
  ["CosmoChartMutedForeground", ["muted-foreground", 1], ["muted-foreground", 1]],
]

// A color as CSS writes it, which Pictor's paints take (its string properties, its theme keys): #rrggbb when it is
// opaque, rgba() when it is not, transparent when it shows nothing.
const css = (argb) => {
  const digits = argb.slice(1)
  const [alpha, rgb] = digits.length === 8 ? [parseInt(digits.slice(0, 2), 16), digits.slice(2)] : [255, digits]
  if (alpha === 0) return "transparent"
  if (alpha === 255) return `#${rgb.toLowerCase()}`
  const [r, g, b] = [0, 2, 4].map((i) => parseInt(rgb.slice(i, i + 2), 16))
  return `rgba(${r}, ${g}, ${b}, ${Math.round((alpha / 255) * 1000) / 1000})`
}

// The stops of shadcn/ui's area gradients (stopOpacity 0.8 at 5%, 0.1 at 95%), as colors a gradient stop takes; the
// primary's too, which dashboard-01's area chart fills with.
export const chartStops = [...[1, 2, 3, 4, 5].flatMap((index) => [[`CosmoChart${index}TopColor`, `chart-${index}`, 0.8], [`CosmoChart${index}BottomColor`, `chart-${index}`, 0.1]]), ["CosmoPrimaryTopColor", "primary", 0.8], ["CosmoPrimaryBottomColor", "primary", 0.1]]

// Gradient stops that differ by mode, opaque (a token at a share over another), as CSS mixes gradient stops premultiplied
// and Avalonia does not: dashboard-01's section cards fade from --primary at 5% over --background (bottom) to --card in the
// light theme and are a flat --card in the dark one.
export const gradientStops = [
  ["CosmoSectionCardBottomColor", ["primary", 0.05, "background"], ["card", 1, "card"]],
  ["CosmoSectionCardTopColor", ["card", 1, "card"], ["card", 1, "card"]],
]

export const shadows = [
  ["FocusRingShadow", (mode) => `0 0 0 3 ${faded(mode.ring, 0.5)}`],
  ["AttachmentFocusShadow", (mode) => `0 0 0 1 ${faded(mode.ring, 0.5)}`],
  ["CodeBlockFloatingShadow", (mode) => `0 0 0 1 ${faded(mode.foreground, 0.1)}`],
  ["TabsFocusShadow", (mode) => `0 0 0 1 ${faded(mode.ring, 1)}, 0 0 0 3 ${faded(mode.ring, 0.5)}`],
  ["TabsActiveFocusShadow", (mode) => `0 0 0 1 ${faded(mode.ring, 1)}, 0 0 0 3 ${faded(mode.ring, 0.5)}, 0 1 3 0 #1A000000, 0 1 2 -1 #1A000000`],
  ["DestructiveFocusRingShadow", (mode, side) => `0 0 0 3 ${faded(mode.destructive, side === 1 ? 0.2 : 0.4)}`],
  ["OutlineFocusShadow", (mode) => `0 0 0 3 ${faded(mode.ring, 0.5)}, 0 1 2 0 #0D000000`],
  ["OutlineDestructiveFocusShadow", (mode, side) => `0 0 0 3 ${faded(mode.destructive, side === 1 ? 0.2 : 0.4)}, 0 1 2 0 #0D000000`],
  ["RingBackgroundShadow", (mode) => `0 0 0 2 ${faded(mode.background, 1)}`],
  ["ShadowXs", () => "0 1 2 0 #0D000000"],
  ["ShadowSm", () => "0 1 3 0 #1A000000, 0 1 2 -1 #1A000000"],
  ["ShadowMd", () => "0 4 6 -1 #1A000000, 0 2 4 -2 #1A000000"],
  ["ShadowLg", () => "0 10 15 -3 #1A000000, 0 4 6 -4 #1A000000"],
  ["ToastShadow", () => "0 4 12 0 #1A000000"],
  ["InvalidFocusShadow", (mode, side) => `0 0 0 3 ${faded(mode.destructive, side === 1 ? 0.2 : 0.4)}, 0 1 2 0 #0D000000`],
  ["SliderThumbHoverShadow", (mode) => `0 0 0 4 ${faded(mode.ring, 0.5)}, 0 1 3 0 #1A000000, 0 1 2 -1 #1A000000`],
]

export const pascal = (name) => name.split("-").map((part) => part[0].toUpperCase() + part.slice(1)).join("")
const title = (name) => name[0].toUpperCase() + name.slice(1)

export async function themes() {
  const result = {}
  for (const base of bases) {
    result[base] = JSON.parse(await readFile(path.join(here, "registry/colors", `${base}.json`), "utf8")).cssVarsV4
  }
  const source = await readFile(path.join(here, "registry/_legacy-base-colors.ts"), "utf8")
  const oklch = source.slice(source.indexOf("export const baseColorsOKLCH"))
  for (const accent of accents) {
    const start = oklch.indexOf(`\n  ${accent}: {`)
    const block = oklch.slice(start, oklch.indexOf("\n  },", start))
    const mode = (name) => {
      const body = block.slice(block.indexOf(`${name}: {`), block.indexOf("}", block.indexOf(`${name}: {`)))
      return Object.fromEntries([...body.matchAll(/"?([a-z0-9-]+)"?: "([^"]+)"/g)].map((m) => [m[1], m[2]]))
    }
    result[accent] = { light: mode("light"), dark: mode("dark") }
  }
  return result
}

const header = (comment) => `<!-- Generated by tools/shadcn/tokens.mjs from shadcn/ui's registry (${comment}); do not edit by hand. -->\n`

const faded = (value, factor) => {
  const color = toSrgb(parse(value))
  return hex({ ...color, alpha: Math.round((color.alpha / 255) * factor * 255) })
}

const over = (value, factor, under) => {
  const top = toSrgb(parse(value)).rgb
  const bottom = toSrgb(parse(under)).rgb
  return hex({ rgb: top.map((v, i) => Math.round(v * factor + bottom[i] * (1 - factor))), alpha: 255 })
}

function colorDictionary(theme) {
  const lines = []
  for (const [key, mode, side] of [["Light", theme.light, 1], ["Dark", theme.dark, 2]]) {
    lines.push(`    <ResourceDictionary x:Key="${key}">`)
    for (const token of tokens) {
      lines.push(`      <Color x:Key="Cosmo${pascal(token)}Color">${hex(toSrgb(parse(mode[token])))}</Color>`)
    }
    for (const token of tokens) {
      lines.push(`      <SolidColorBrush x:Key="Cosmo${pascal(token)}Brush" Color="${hex(toSrgb(parse(mode[token])))}" />`)
    }
    for (const component of components) {
      const [token, factor] = component[side]
      lines.push(`      <SolidColorBrush x:Key="Cosmo${component[0]}Brush" Color="${faded(mode[token], factor)}" />`)
    }
    for (const [name, value] of derived) {
      lines.push(`      <SolidColorBrush x:Key="Cosmo${name}Brush" Color="${value(mode, side)}" />`)
    }
    for (const [name, value] of shadows) {
      lines.push(`      <BoxShadows x:Key="Cosmo${name}">${value(mode, side)}</BoxShadows>`)
    }
    for (const entry of fluent) {
      const [token, factor] = entry[side]
      lines.push(`      <SolidColorBrush x:Key="${entry[0]}" Color="${faded(mode[token], factor)}" />`)
    }
    for (const [key, token, factor] of chartStops) {
      lines.push(`      <Color x:Key="${key}">${faded(mode[token], factor)}</Color>`)
    }
    for (const entry of gradientStops) {
      const [token, factor, under] = entry[side]
      lines.push(`      <Color x:Key="${entry[0]}">${over(mode[token], factor, mode[under])}</Color>`)
    }
    for (const entry of pictor) {
      const value = entry[side]
      lines.push(`      <x:String x:Key="${entry[0]}">${css(typeof value === "string" ? value : faded(mode[value[0]], value[1]))}</x:String>`)
    }
    lines.push(`    </ResourceDictionary>`)
  }
  return lines.join("\n")
}

const dictionary = (comment, body, type) => `${header(comment)}<ResourceDictionary xmlns="https://github.com/avaloniaui" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" x:Class="Aioros.Cosmo.Theming.${type}">
${body}
</ResourceDictionary>
`

const radiusType = (label) => `Radius${radii.find((r) => r[0] === label)[2]}Tokens`

function classes() {
  const type = (name, summary) => `/// <summary>${summary}</summary>
public sealed partial class ${name} : ResourceDictionary
{
    /// <summary>Loads the dictionary's compiled XAML.</summary>
    public ${name}() => AvaloniaXamlLoader.Load(this);
}
`
  return `using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Aioros.Cosmo.Theming;

${[...bases, ...accents].map((name) => type(`${title(name)}Tokens`, `The light and dark tokens of shadcn/ui's ${name} color theme.`)).join("\n")}
${radii.map(([label]) => type(radiusType(label), `The radius scale for <c>--radius: ${label}rem</c>.`)).join("\n")}`
}

const themed = (body) => `  <ResourceDictionary.ThemeDictionaries>\n${body}\n  </ResourceDictionary.ThemeDictionaries>`

async function main() {
  const all = await themes()
  await mkdir(path.join(theming, "Colors"), { recursive: true })
  await mkdir(path.join(theming, "Radius"), { recursive: true })
  for (const name of [...bases, ...accents]) {
    const source = bases.includes(name) ? `public/r/colors/${name}.json, cssVarsV4` : `registry/_legacy-base-colors.ts, baseColorsOKLCH.${name}`
    await writeFile(path.join(theming, "Colors", `${title(name)}.axaml`), dictionary(source, themed(colorDictionary(all[name])), `${title(name)}Tokens`))
  }
  for (const [label, rem] of radii) {
    const px = rem * 16
    const lines = [`  <x:Double x:Key="CosmoRadius">${px}</x:Double>`, `  <CornerRadius x:Key="OverlayCornerRadius">${+(px * 0.8).toFixed(2)}</CornerRadius>`, `  <CornerRadius x:Key="CosmoDrawerRadius">${px},${px},0,0</CornerRadius>`]
    const md = +(px * 0.8).toFixed(2)
    lines.push(`  <CornerRadius x:Key="CosmoRadiusMdStart">${md},0,0,${md}</CornerRadius>`, `  <CornerRadius x:Key="CosmoRadiusMdEnd">0,${md},${md},0</CornerRadius>`, `  <CornerRadius x:Key="CosmoRadiusMdTop">${md},${md},0,0</CornerRadius>`, `  <CornerRadius x:Key="CosmoRadiusMdBottom">0,0,${md},${md}</CornerRadius>`)
    for (const [step, factor] of scale) {
      lines.push(`  <CornerRadius x:Key="CosmoRadius${step}">${+(px * factor).toFixed(2)}</CornerRadius>`)
    }
    await writeFile(path.join(theming, "Radius", `Radius${radii.find((r) => r[0] === label)[2]}.axaml`), dictionary(`the radius ${label}rem and the radius scale of the registry's CSS updater`, lines.join("\n"), radiusType(label)))
  }
  await writeFile(path.join(theming, "TokenDictionaries.cs"), classes())
  const row = (value) => { const c = toSrgb(parse(value)); return `\`${value}\` | \`${hex(c)}\`${c.inGamut ? "" : " (clipped)"}` }
  const table = [`| Token | Resource | Light (oklch) | Light (sRGB) | Dark (oklch) | Dark (sRGB) |`, `| --- | --- | --- | --- | --- | --- |`]
  for (const token of tokens) {
    table.push(`| \`--${token}\` | \`Cosmo${pascal(token)}Color\`, \`Cosmo${pascal(token)}Brush\` | ${row(all.neutral.light[token])} | ${row(all.neutral.dark[token])} |`)
  }
  const themesTable = [`| Theme | Source | \`--primary\` light | \`--primary\` dark | \`--background\` dark | \`--ring\` light |`, `| --- | --- | --- | --- | --- | --- |`]
  for (const name of [...bases, ...accents]) {
    const source = bases.includes(name) ? `\`public/r/colors/${name}.json\`` : `\`baseColorsOKLCH.${name}\``
    const sr = (value) => hex(toSrgb(parse(value)))
    themesTable.push(`| ${title(name)} | ${source} | \`${sr(all[name].light.primary)}\` | \`${sr(all[name].dark.primary)}\` | \`${sr(all[name].dark.background)}\` | \`${sr(all[name].light.ring)}\` |`)
  }
  const componentTable = [`| Resource | Light | Dark |`, `| --- | --- | --- |`]
  for (const [name, light, dark] of components) {
    componentTable.push(`| \`Cosmo${name}Brush\` | \`--${light[0]}\` at ${light[1] * 100}% (\`${faded(all.neutral.light[light[0]], light[1])}\`) | \`--${dark[0]}\` at ${dark[1] * 100}% (\`${faded(all.neutral.dark[dark[0]], dark[1])}\`) |`)
  }
  for (const [key, light, dark] of fluent) {
    componentTable.push(`| \`${key}\` (Fluent's key) | \`--${light[0]}\` at ${light[1] * 100}% | \`--${dark[0]}\` at ${dark[1] * 100}% |`)
  }
  for (const [name, value] of shadows) {
    componentTable.push(`| \`Cosmo${name}\` | \`${value(all.neutral.light, 1)}\` | \`${value(all.neutral.dark, 2)}\` |`)
  }
  const radiusTable = [`| \`CosmoRadius\` | rem | px | Sm | Md | Lg | Xl | 2Xl | 3Xl | 4Xl |`, `| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |`]
  for (const [label, rem, name] of radii) {
    radiusTable.push(`| ${name} | ${label} | ${rem * 16} | ${scale.map(([, f]) => +(rem * 16 * f).toFixed(2)).join(" | ")} |`)
  }
  const docs = await readFile(path.join(here, "tokens.template.md"), "utf8")
  await writeFile(path.join(root, "docs", "tokens.md"), docs
    .replace("{{tokens}}", table.join("\n"))
    .replace("{{themes}}", themesTable.join("\n"))
    .replace("{{components}}", componentTable.join("\n"))
    .replace("{{radius}}", radiusTable.join("\n")))
  const json = Object.fromEntries(Object.entries(all).map(([name, theme]) => [name, { light: Object.fromEntries(tokens.map((t) => [t, theme.light[t]])), dark: Object.fromEntries(tokens.map((t) => [t, theme.dark[t]])) }]))
  await writeFile(path.join(root, "tests/Aioros.Cosmo.Tests/Reference/themes.json"), JSON.stringify(json, null, 1) + "\n")
}

if (process.argv[1] === fileURLToPath(import.meta.url)) await main()
