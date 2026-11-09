import path from "node:path"
import { fileURLToPath, pathToFileURL } from "node:url"

const here = path.dirname(fileURLToPath(import.meta.url))
const modules = path.join(here, "node_modules")

const attributes = (props) => Object.entries(props).filter(([name]) => name !== "key").map(([name, value]) => `${name}="${value}"`).join(" ")

const circle = ({ cx, cy, r }) => `M${+cx - +r},${cy} A${r},${r} 0 1 1 ${+cx + +r},${cy} A${r},${r} 0 1 1 ${+cx - +r},${cy} Z`

const rect = ({ x = 0, y = 0, width, height, rx = 0, ry }) => {
  const [l, t, w, h, r] = [+x, +y, +width, +height, +(ry ?? rx)]
  if (!r) return `M${l},${t} L${l + w},${t} L${l + w},${t + h} L${l},${t + h} Z`
  return `M${l + r},${t} L${l + w - r},${t} A${r},${r} 0 0 1 ${l + w},${t + r} L${l + w},${t + h - r} A${r},${r} 0 0 1 ${l + w - r},${t + h} L${l + r},${t + h} A${r},${r} 0 0 1 ${l},${t + h - r} L${l},${t + r} A${r},${r} 0 0 1 ${l + r},${t} Z`
}

const points = (list, close) => {
  const values = list.trim().split(/[\s,]+/).map(Number)
  const pairs = []
  for (let i = 0; i < values.length; i += 2) pairs.push(`${values[i]},${values[i + 1]}`)
  return `M${pairs.join(" L")}${close ? " Z" : ""}`
}

const absolute = (d) => d.replace(/^\s*m\s*(-?[\d.]+)[\s,]*(-?[\d.]+)\s*(?=[-\d.])?/, (match, x, y, offset, whole) => {
  const rest = whole.slice(match.length)
  return `M${x} ${y}` + (/^[-\d.]/.test(rest) ? " l" : " ")
})

const geometry = ([tag, props]) => ({
  path: () => absolute(props.d),
  circle: () => circle(props),
  rect: () => rect(props),
  line: () => `M${props.x1},${props.y1} L${props.x2},${props.y2}`,
  polyline: () => points(props.points, false),
  polygon: () => points(props.points, true),
})[tag]()

/** The icon's node list: `lucide:popcorn` from lucide-react, `tabler:folder-code` from @tabler/icons-react. */
export async function icon(name) {
  const [set, kind] = name.split(":")
  const file = set === "tabler"
    ? path.join(modules, "@tabler/icons-react/dist/esm/icons", `Icon${kind.split("-").map((part) => part[0].toUpperCase() + part.slice(1)).join("")}.mjs`)
    : path.join(modules, "lucide-react/dist/esm/icons", `${kind}.js`)
  const { __iconNode } = await import(pathToFileURL(file).href)
  return {
    svg: __iconNode.map(([tag, props]) => `<${tag} ${attributes(props)}/>`).join(""),
    path: __iconNode.map(geometry).join(" "),
  }
}

if (process.argv[1] === fileURLToPath(import.meta.url)) {
  const result = {}
  for (const name of process.argv.slice(2)) result[name] = await icon(name)
  console.log(JSON.stringify(result, null, 1))
}
