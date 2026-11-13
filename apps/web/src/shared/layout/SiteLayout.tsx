import { useState } from 'react'
import type { ReactNode } from 'react'
import { useLocale } from '../i18n/messages'
import { Link } from '../Link'
import { navigate } from '../router'
import { isOwnerAccount, useSession } from '../session'
import SystemStatus from '../../features/system-status/SystemStatus'
import './layout.css'

const copyrightYear = new Date().getFullYear()

export function Brand() {
  return <Link className="brand-logo" href="/" aria-label="TurboRent">
    <span className="brand-mark" aria-hidden="true">T</span>
    <span className="brand-text">TURBO<b>RENT</b></span>
  </Link>
}

export function LocaleSwitch() {
  const { locale, setLocale } = useLocale()
  return <div className="locale-switch" role="group" aria-label="Language">
    <button type="button" aria-pressed={locale === 'az'} onClick={() => setLocale('az')}>AZ</button>
    <button type="button" aria-pressed={locale === 'en'} onClick={() => setLocale('en')}>EN</button>
  </div>
}

function SiteHeader() {
  const { t } = useLocale()
  const { session, status, signOut } = useSession()
  const [open, setOpen] = useState(false)
  const [error, setError] = useState('')
  const owner = isOwnerAccount(session)

  async function logout() {
    setError('')
    try {
      await signOut()
      navigate('/')
    } catch {
      setError('logoutError')
    }
  }

  return <>
    <header className="site-header">
      <div className="site-header-inner">
        <Brand />
        <button type="button" className="menu-toggle" aria-expanded={open} aria-controls="site-nav"
          onClick={() => setOpen(value => !value)}>
          <span aria-hidden="true">☰</span><span className="sr-only">{t('menu')}</span>
        </button>
        <nav id="site-nav" className={open ? 'site-nav open' : 'site-nav'} aria-label="Main navigation"
          onClick={event => { if ((event.target as HTMLElement).closest('a')) setOpen(false) }}>
          <Link href="/cars">{t('navCars')}</Link>
          <Link href="/#how-it-works">{t('navHowItWorks')}</Link>
          <span className="nav-spacer" />
          <LocaleSwitch />
          {session ? <>
            {session.isAdmin && <Link href="/admin">{t('adminPanel')}</Link>}
            {owner && <Link href="/owner/listings">{t('myListings')}</Link>}
            {owner && <Link href="/owner/bookings">{t('ownerBookings')}</Link>}
            <Link href="/account">{t('account')}</Link>
            <span className="nav-user">{t('signedInAs')} <strong>{session.displayName}</strong></span>
            <button className="nav-button" type="button" onClick={logout}>{t('logout')}</button>
          </> : status !== 'loading' && <Link href="/login">{t('homeLogin')}</Link>}
          <Link className="nav-cta" href="/owner/listings/new"><span aria-hidden="true">+</span> {t('postListing')}</Link>
        </nav>
      </div>
    </header>
    {(error || status === 'error') && <p className="site-alert" role="alert">{t(error || 'sessionError')}</p>}
  </>
}

function SiteFooter() {
  const { t } = useLocale()
  return <footer className="site-footer">
    <div className="site-footer-inner">
      <div className="footer-about">
        <Brand />
        <p>{t('footerAbout')}</p>
      </div>
      <nav aria-label="Footer">
        <Link href="/cars">{t('navCars')}</Link>
        <Link href="/owner/listings/new">{t('postListing')}</Link>
        <Link href="/register">{t('homeRegister')}</Link>
        <Link href="/account">{t('account')}</Link>
      </nav>
      <SystemStatus />
    </div>
    <p className="footer-legal">© {copyrightYear} TurboRent · AZN · Asia/Baku · {t('footerRights')}</p>
  </footer>
}

export function SiteLayout({ children, wide = false }: { children: ReactNode; wide?: boolean }) {
  return <div className="site-shell">
    <a className="skip-link" href="#main">Skip to content</a>
    <SiteHeader />
    <main id="main" className={wide ? 'site-main wide' : 'site-main'}>{children}</main>
    <SiteFooter />
  </div>
}
