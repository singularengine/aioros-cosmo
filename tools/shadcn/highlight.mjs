// Highlights code as the radix base's highlighter does (lib/highlighter.tsx: shiki's JavaScript engine, github-light and
// github-dark), for Cosmo's code blocks to carry the tokens in their XAML: reads {"name": {"code", "language"}} on
// stdin and writes {"name": [[[text, light, dark, style?], ...], ...]} (one array of tokens per line, null for a language the
// highlighter does not list, which stays plain text) on stdout.
import { createHighlighterCore } from "shiki/core"
import { createJavaScriptRegexEngine } from "shiki/engine/javascript"

const languages = {
  bash: () => import("shiki/langs/shellscript.mjs"),
  css: () => import("shiki/langs/css.mjs"),
  diff: () => import("shiki/langs/diff.mjs"),
  html: () => import("shiki/langs/html.mjs"),
  javascript: () => import("shiki/langs/javascript.mjs"),
  json: () => import("shiki/langs/json.mjs"),
  jsx: () => import("shiki/langs/jsx.mjs"),
  markdown: () => import("shiki/langs/markdown.mjs"),
  python: () => import("shiki/langs/python.mjs"),
  sql: () => import("shiki/langs/sql.mjs"),
  tsx: () => import("shiki/langs/tsx.mjs"),
  typescript: () => import("shiki/langs/typescript.mjs"),
  yaml: () => import("shiki/langs/yaml.mjs"),
}
const aliases = { js: "javascript", md: "markdown", py: "python", sh: "bash", shell: "bash", shellscript: "bash", ts: "typescript", yml: "yaml", zsh: "bash" }

const shiki = await createHighlighterCore({
  themes: [import("shiki/themes/github-light.mjs"), import("shiki/themes/github-dark.mjs")],
  langs: Object.values(languages),
  engine: createJavaScriptRegexEngine({ forgiving: true }),
})

let input = ""
for await (const chunk of process.stdin) input += chunk
const out = {}
for (const [name, { code, language }] of Object.entries(JSON.parse(input))) {
  const name2 = language.toLowerCase()
  const lang = name2 === "ansi" || name2 in languages ? name2 : aliases[name2]
  if (!lang) {
    out[name] = null
    continue
  }
  const tokenize = (text) => shiki.codeToTokens(text, { lang, themes: { light: "github-light", dark: "github-dark" }, defaultColor: false }).tokens
  // ANSI has no grammar state: each line is tokenized on its own, as the radix base's highlighter does.
  const tokens = lang === "ansi" ? code.split("\n").flatMap((line) => tokenize(line)) : tokenize(code)
  out[name] = tokens.map((line) => line.map((token) => {
    const { "--shiki-light": light, "--shiki-dark": dark, ...rest } = token.htmlStyle
    return Object.keys(rest).length ? [token.content, light, dark, rest] : [token.content, light, dark]
  }))
}
process.stdout.write(JSON.stringify(out))
