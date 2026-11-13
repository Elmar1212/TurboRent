import { useState } from 'react'
import type { FormEvent } from 'react'
import { useLocale } from '../../shared/i18n/messages'
import { AuthLayout } from './AuthLayout'
import { FormField } from './FormField'
import { ApiError, submit } from './api'
import { Link } from '../../shared/Link'
import { safeReturnTo } from '../../shared/router'

export default function LoginPage() {
  const language = useLocale()
  const { t } = language
  const localHttp = location.protocol !== 'https:' && ['localhost', '127.0.0.1'].includes(location.hostname)
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [totpCode, setTotpCode] = useState('')
  const [requiresTotp, setRequiresTotp] = useState(false)
  const [notice, setNotice] = useState('')
  const [busy, setBusy] = useState(false)

  async function login(event: FormEvent) {
    event.preventDefault()
    setBusy(true)
    setNotice('')
    try {
      const result = await submit('login', { email, password, ...(requiresTotp ? { totpCode } : {}) })
      const returnTo = safeReturnTo(new URLSearchParams(location.search).get('returnTo'))
      // A full load refreshes the shared session state for the whole app.
      if (result.isAdmin === true) location.assign(returnTo ?? '/admin')
      else if (result.accountApprovalStatus)
        location.assign(returnTo ?? (result.canCreateListing === true ? '/owner/listings' : '/owner/listings/new'))
      else location.assign(returnTo ?? '/')
    } catch (error) {
      const code = error instanceof ApiError ? error.code : 'generic'
      if (code === 'mfa_required') setRequiresTotp(true)
      setNotice(code)
    } finally {
      setBusy(false)
    }
  }

  return <AuthLayout language={language}>
    <p className="card-step">02 / TURBORENT</p>
    <h2>{t('loginTitle')}</h2>
    <p className="card-subtitle">{t('loginSubtitle')}</p>
    <form onSubmit={login} aria-busy={busy}>
      <FormField id="email" label={t('email')} value={email} onChange={setEmail}
        type="email" autoComplete="username" maxLength={254} />
      <FormField id="password" label={t('password')} value={password} onChange={setPassword}
        type="password" autoComplete="current-password" maxLength={128}
        showLabel={t('show')} hideLabel={t('hide')} />
      {requiresTotp && <div className="form-field">
        <label htmlFor="admin-totp">{t('adminTotpLabel')}</label>
        <input id="admin-totp" inputMode="numeric" autoComplete="one-time-code" pattern="[0-9]{6}"
          maxLength={6} value={totpCode} onChange={event => setTotpCode(event.target.value.replace(/\D/g, '').slice(0, 6))}
          required aria-describedby="admin-totp-hint" />
        <small id="admin-totp-hint">{t('adminTotpHint')}</small>
      </div>}
      {notice && <p className="form-notice" id="login-notice" role="alert">{t(notice)}</p>}
      {notice === 'secure' && localHttp && <a className="back-link" href="https://localhost:5174/login">{t('openSecureLogin')}</a>}
      <button className="primary-button" disabled={busy} type="submit">
        {busy ? t('loading') : t('loginSubmit')}<span aria-hidden="true">↗</span>
      </button>
    </form>
    <div className="auth-links">
      <Link className="back-link" href="/forgot-password">{t('forgotPassword')}</Link>
      <Link className="back-link" href="/register">{t('homeRegister')}</Link>
    </div>
  </AuthLayout>
}
