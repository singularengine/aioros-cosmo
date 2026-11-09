import "../../app/src/index.css"

import { themeCss } from "../../app/src/themes"

const css =
  themeCss("neutral", "0.625rem") +
  `:root { --font-reference: Geist, "Cosmo Symbols", "Cosmo Emoji", "Noto Naskh Arabic", ui-sans-serif, system-ui, sans-serif; }`

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en" style={{ colorScheme: "light" }}>
      <head>
        <style dangerouslySetInnerHTML={{ __html: css }} />
      </head>
      <body className="bg-background font-sans text-foreground antialiased">{children}</body>
    </html>
  )
}
