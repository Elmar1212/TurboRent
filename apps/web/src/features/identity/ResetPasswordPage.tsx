import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { useLocale } from '../../shared/i18n/messages'
import { Link } from '../../shared/Link'
import { AuthLayout } from './AuthLayout'
import { FormField } from './FormField'
import { ApiError, submit } from './api'
import { parseVerificationFragment } from './verificationLink'

export default function ResetPasswordPage() {
  const language = useLocale()
  const { t } = language
  // The link uses the same userId/token fragment format as email verification.
  const [link] = useState(() => parseVerificationFragment(location.hash))
  const [password, setPassword] = useState('')
  const [confirm, setConfirm] = useState('')
  const [notice, setNotice] = useState('')
  const [busy, setBusy] = useState(false)
  const [done, setDone] = useState(false)

  // Remove the token from the address bar and browser history once it has been read.
  useEffect(() => {
    if (location.hash) history.replaceState(history.state, '', location.pathname + location.search)
  }, [])

  async function reset(event: FormEvent) {
    event.preventDefault()
    if (!link) return
    if (password.length < 15 || password.length > 128) { setNotice('password_length'); return }
    if (password !== confirm) { setNotice('mismatch'); return }
    setBusy(true); setNotice('')
    try {
      await submit('password/reset', { ...link, password })
      setDone(true)
    } catch (error) {
      setNotice(error instanceof ApiError ? error.code : 'generic')
    } finally { setBusy(false) }
  }

  return <AuthLayout language={language}>
    <h2>{t('resetTitle')}</h2>
    {done ? <div className="success-panel" role="status">
      <div className="success-symbol" aria-hidden="true">✓</div>
      <p>{t('resetDone')}</p>
      <Link className="primary-button verify-login-link" href="/login">{t('login')}</Link>
    </div> : !link ? <div>
      <p className="form-notice" role="alert">{t('invalid_reset_link')}</p>
      <Link className="back-link" href="/forgot-password">{t('forgotPassword')}</Link>
    </div> : <form onSubmit={reset} noValidate aria-busy={busy}>
      <FormField id="password" label={t('newPassword')} value={password} onChange={setPassword} type="password"
        autoComplete="new-password" maxLength={128} hint={t('passwordHint')} showLabel={t('show')} hideLabel={t('hide')} />
      <FormField id="confirm" label={t('confirm')} value={confirm} onChange={setConfirm} type="password"
        autoComplete="new-password" maxLength={128} showLabel={t('show')} hideLabel={t('hide')} />
      {notice && <p className="form-notice" role="alert">{t(notice)}</p>}
      {notice === 'invalid_reset_link' && <Link className="back-link" href="/forgot-password">{t('forgotPassword')}</Link>}
      <button className="primary-button" type="submit" disabled={busy}>{busy ? t('loading') : t('resetSubmit')}</button>
    </form>}
  </AuthLayout>
}
