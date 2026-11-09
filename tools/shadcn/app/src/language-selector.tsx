// The part of shadcn/ui's components/language-selector.tsx (apps/v4 at the pinned commit, MIT) that the RTL examples use:
// their translations in the language they start in, with no selector shown.
import * as React from "react"

export type Language = "en" | "ar" | "he"

export type Direction = "ltr" | "rtl"

export type Translations<T extends Record<string, string> = Record<string, string>> = Record<Language, { dir: Direction; locale?: string; values: T }>

export function useTranslation<T extends Record<string, string>>(translations: Translations<T>, defaultLanguage: Language = "ar") {
  const [language, setLanguage] = React.useState<Language>(defaultLanguage)
  const { dir, locale, values: t } = translations[language]
  return { language, setLanguage, dir, locale, t }
}
