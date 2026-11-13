import { useEffect, useMemo, useState } from 'react'
import type { ReactNode } from 'react'
import { LocaleContext, readStoredLocale, translate } from './messages'
import type { Language, Locale } from './messages'

export function LocaleProvider({ children }: { children: ReactNode }) {
  const [locale, setLocale] = useState<Locale>(readStoredLocale)
  useEffect(() => {
    document.documentElement.lang = locale
    try { localStorage.setItem('turborent.locale', locale) } catch { /* storage may be unavailable */ }
  }, [locale])
  const value = useMemo<Language>(() => ({ locale, setLocale, t: key => translate(locale, key) }), [locale])
  return <LocaleContext.Provider value={value}>{children}</LocaleContext.Provider>
}
