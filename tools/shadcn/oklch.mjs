export function parse(text) {
  const match = /^oklch\(\s*([\d.]+)(%?)\s+([\d.]+)\s+([\d.]+)\s*(?:\/\s*([\d.]+)(%?))?\s*\)$/.exec(text.trim())
  if (!match) throw new Error(`Not an oklch color: ${text}`)
  const lightness = Number(match[1]) / (match[2] ? 100 : 1)
  const alpha = match[5] === undefined ? 1 : Number(match[5]) / (match[6] ? 100 : 1)
  return { l: lightness, c: Number(match[3]), h: Number(match[4]), alpha }
}

const encode = (v) => (v <= 0.0031308 ? 12.92 * v : 1.055 * Math.pow(v, 1 / 2.4) - 0.055)

export function toSrgb({ l, c, h, alpha }) {
  const radians = (h * Math.PI) / 180
  const a = c * Math.cos(radians)
  const b = c * Math.sin(radians)
  const l1 = (l + 0.3963377773761749 * a + 0.2158037573099136 * b) ** 3
  const m1 = (l - 0.1055613458156586 * a - 0.0638541728258133 * b) ** 3
  const s1 = (l - 0.0894841775298119 * a - 1.2914855480194092 * b) ** 3
  const linear = [
    4.0767416621 * l1 - 3.3077115913 * m1 + 0.2309699292 * s1,
    -1.2684380046 * l1 + 2.6097574011 * m1 - 0.3413193965 * s1,
    -0.0041960863 * l1 - 0.7034186147 * m1 + 1.707614701 * s1,
  ]
  const exact = linear.map(encode)
  const clipped = exact.map((v) => Math.min(1, Math.max(0, v)))
  return { exact, rgb: clipped.map((v) => Math.round(v * 255)), alpha: Math.round(alpha * 255), inGamut: exact.every((v) => v >= -1e-4 && v <= 1 + 1e-4) }
}

export const hex = ({ rgb, alpha }) =>
  "#" + (alpha === 255 ? [] : [alpha]).concat(rgb).map((v) => v.toString(16).padStart(2, "0").toUpperCase()).join("")
