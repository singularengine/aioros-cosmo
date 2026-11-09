import path from "node:path"
import react from "@vitejs/plugin-react"
import tailwindcss from "@tailwindcss/vite"
import { defineConfig } from "vite"

export default defineConfig(({ mode }) => ({
  root: __dirname,
  base: "./",
  plugins: [react(), tailwindcss()],
  resolve: {
    alias: [
      { find: /^cn$/, replacement: path.resolve(__dirname, "src/cn.ts") },
      { find: "@/registry/new-york-v4/ui", replacement: path.resolve(__dirname, "../registry/ui") },
      { find: "@/registry/new-york-v4/hooks", replacement: path.resolve(__dirname, "../registry/hooks") },
      { find: "@/registry/new-york-v4/examples", replacement: path.resolve(__dirname, "../registry/examples") },
      { find: "@/registry/new-york-v4/charts", replacement: path.resolve(__dirname, "../registry/charts") },
      { find: "@/registry/new-york-v4/blocks", replacement: path.resolve(__dirname, "../registry/blocks") },
      { find: /^@\/styles\/radix-(rhea|nova)\/ui\/(bubble|message|tabs|sheet|dialog|card)$/, replacement: path.resolve(__dirname, "../registry/ui/$2") },
      { find: /^@\/styles\/radix-(rhea|nova)\/ui(-rtl)?/, replacement: path.resolve(__dirname, "../registry/radix/ui") },
      { find: "@/components/language-selector", replacement: path.resolve(__dirname, "src/language-selector.tsx") },
      { find: "@/components/markdown", replacement: path.resolve(__dirname, "src/markdown.tsx") },
      { find: /^@\/components\/ui\/(.*)$/, replacement: path.resolve(__dirname, "../registry/ui/$1") },
      { find: "@/lib/ai", replacement: path.resolve(__dirname, "src/ai.ts") },
      { find: "@/registry/bases/radix", replacement: path.resolve(__dirname, "../registry/radix") },
      { find: "@/app/(create)/components/icon-placeholder", replacement: path.resolve(__dirname, "src/icon-placeholder.tsx") },
      ...(mode === "recharts2" ? [{ find: /^recharts$/, replacement: "recharts2" }] : []),
      { find: "@/registry", replacement: path.resolve(__dirname, "../registry") },
    ],
  },
}))
