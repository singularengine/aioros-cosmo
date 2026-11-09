// shadcn/ui's blocks served by Next.js, the stack shadcn/ui's own site runs on, for Cosmo's lab bench (bench/gallery.sh).
// NEXT_STATIC=1 builds the prerendered variant into .next-static (each block rendered once at build time, as a page with
// no dynamic data is by default); unset, every request is rendered into .next, as Aioros renders every request.
import path from "node:path"

const prerendered = process.env.NEXT_STATIC === "1"
const repository = path.resolve(import.meta.dirname, "../../..")

export default {
  poweredByHeader: false,
  distDir: prerendered ? ".next-static" : ".next",
  pageExtensions: [prerendered ? "static.tsx" : "live.tsx", "tsx"],
  outputFileTracingRoot: repository,
  turbopack: { root: repository },
  typescript: { ignoreBuildErrors: true },
}
