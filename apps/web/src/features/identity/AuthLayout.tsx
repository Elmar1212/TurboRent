import type { ReactNode } from 'react'
import type { Language as SharedLanguage } from '../../shared/i18n/messages'
import { Brand, LocaleSwitch } from '../../shared/layout/SiteLayout'
import { Link } from '../../shared/Link'
import './identity.css'

export type Language = SharedLanguage

/** Two-column layout for sign-up, sign-in and password pages. */
export function AuthLayout({ language, children }: { language: Language; children: ReactNode }) {
  const { t } = language
  return <div className="auth-shell">
    <header className="auth-header">
      <Brand />
      <div className="auth-header-actions">
        <Link className="auth-header-link" href="/cars">{t('navCars')}</Link>
        <LocaleSwitch />
      </div>
    </header>
    <main className="auth-grid">
      <aside className="auth-story">
        <p className="auth-eyebrow">{t('eyebrow')}</p>
        <h1>{t('hero')}</h1>
        <p className="auth-intro">{t('intro')}</p>
        <ul className="auth-points">
          <li>{t('statVerified')}</li>
          <li>{t('statDirect')}</li>
          <li>{t('statLocal')}</li>
        </ul>
      </aside>
      <section className="auth-card">{children}</section>
    </main>
    <footer className="auth-footer"><span>© TurboRent</span><span>{t('currency')}</span></footer>
  </div>
}
