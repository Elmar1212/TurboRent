import { useState } from 'react'
import type { FormEvent } from 'react'
import { useLocale } from '../../shared/i18n/messages'
import { Link } from '../../shared/Link'
import { AuthLayout } from './AuthLayout'
import { FormField } from './FormField'
import { ApiError, submit } from './api'
import { useResendCooldown } from './useResendCooldown'

export default function ForgotPasswordPage() {
  const language = useLocale()
  const { t } = language
  const [email, setEmail] = useState('')
  const [notice, setNotice] = useState('')
  const [sent, setSent] = useState(false)
  const [busy, setBusy] = useState(false)
  const cooldown = useResendCooldown()

  async function request(event: FormEvent) {
    event.preventDefault()
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) { setNotice('invalid_email'); return }
    setBusy(true); setNotice('')
    try { await submit('password/forgot', { email }); setSent(true) }
    catch (error) {
      setNotice(error instanceof ApiError ? error.code : 'generic')
      if (error instanceof ApiError && error.code === 'rate_limited') cooldown.start(error.retryAfterSeconds)
    } finally { setBusy(false) }
  }

  return <AuthLayout language={language}>
    <h2>{t('forgotTitle')}</h2>
    {sent ? <div className="success-panel" role="status">
      <div className="success-symbol" aria-hidden="true">✓</div>
      <p>{t('resetSent')}</p>
      <Link className="back-link" href="/login">{t('login')}</Link>
    </div> : <>
      <p className="card-subtitle">{t('forgotIntro')}</p>
      <form onSubmit={request} noValidate aria-busy={busy}>
        <FormField id="email" label={t('email')} value={email} onChange={setEmail} type="email" autoComplete="email" />
        {notice && <p className="form-notice" role="alert">{t(notice)}</p>}
        {cooldown.remaining > 0 && <p role="status">{t('retryIn').replace('{seconds}', String(cooldown.remaining))}</p>}
        <button className="primary-button" type="submit" disabled={busy || cooldown.remaining > 0}>
          {busy ? t('loading') : t('sendResetLink')}
        </button>
      </form>
      <Link className="back-link" href="/login">{t('login')}</Link>
    </>}
  </AuthLayout>
}
