import { useEffect, useRef, useState } from 'react'
import type { FormEvent } from 'react'
import { useLocale } from '../../shared/i18n/messages'
import { AuthLayout } from './AuthLayout'
import { FormField } from './FormField'
import { ApiError, submit } from './api'
import { parseVerificationFragment } from './verificationLink'
import { useResendCooldown } from './useResendCooldown'

export default function VerifyEmailPage() {
  const language = useLocale(); const { t } = language
  const [verification, setVerification] = useState(() => parseVerificationFragment(location.hash))
  const version = useRef({ value: 0 })
  const [busy, setBusy] = useState(false)
  const [verified, setVerified] = useState(false)
  const [requiresListingSetup, setRequiresListingSetup] = useState(false)
  const [notice, setNotice] = useState('')
  const [email, setEmail] = useState('')
  const cooldown = useResendCooldown()

  useEffect(() => {
    const counter = version.current
    function capture(hash: string) {
      counter.value++
      setVerification(parseVerificationFragment(hash))
      setVerified(false); setRequiresListingSetup(false); setNotice(''); setBusy(false)
      if (location.hash) history.replaceState(history.state, '', location.pathname + location.search)
    }
    const hashChanged = (event: HashChangeEvent) => capture(new URL(event.newURL).hash)
    const navigated = () => capture(location.hash)
    // No side effects during render. The second StrictMode effect sees no fragment
    // and preserves the captured token, while future navigation can replace it.
    if (location.hash) capture(location.hash)
    window.addEventListener('hashchange', hashChanged)
    window.addEventListener('popstate', navigated)
    return () => {
      counter.value++ // Ignore completion of a request belonging to an unmounted page.
      window.removeEventListener('hashchange', hashChanged)
      window.removeEventListener('popstate', navigated)
    }
  }, [])

  async function verify() {
    if (!verification) return
    const attempt = version.current.value
    setBusy(true); setNotice('')
    try {
      const result = await submit('email/verify', verification)
      if (attempt !== version.current.value) return
      setRequiresListingSetup(result.requiresListingSetup === true)
      setVerification(null); setVerified(true)
    } catch (error) {
      if (attempt === version.current.value)
        setNotice(error instanceof ApiError && error.code !== 'invalid_verification' ? error.code : 'invalidLink')
    } finally { if (attempt === version.current.value) setBusy(false) }
  }
  async function resend(event: FormEvent) {
    event.preventDefault()
    if (cooldown.remaining > 0) return
    setBusy(true); setNotice('')
    try { await submit('email/resend', { email }); setNotice('resendSent') }
    catch (error) {
      setNotice(error instanceof ApiError ? error.code : 'generic')
      if (error instanceof ApiError && error.code === 'rate_limited') cooldown.start(error.retryAfterSeconds)
    } finally { setBusy(false) }
  }
  return <AuthLayout language={language}><div className="verify-panel">
    <div className="success-symbol" aria-hidden="true">{verified ? '✓' : '↗'}</div>
    <h2>{t(verified ? 'verifiedTitle' : 'verifyTitle')}</h2>
    <p>{t(verified ? 'verified' : verification ? 'verifyIntro' : 'invalidLink')}</p>
    {!verified && verification && <button className="primary-button" disabled={busy} onClick={verify}>{busy ? t('loading') : t('verify')}</button>}
    {notice && <p id="verify-notice" role={notice === 'resendSent' ? 'status' : 'alert'}>{t(notice)}</p>}
    {!verified && !verification && <form onSubmit={resend} className="resend-form">
      <FormField id="email" label={t('email')} value={email} onChange={setEmail} type="email" autoComplete="email" />
      <button type="submit" className="secondary-button" disabled={busy || cooldown.remaining > 0}>{t('resend')}</button>
      {cooldown.remaining > 0 && <p role="status" data-testid="resend-countdown">{t('retryIn').replace('{seconds}', String(cooldown.remaining))}</p>}
    </form>}
    {verified && requiresListingSetup && <p>{t('verifiedOwnerNext')}</p>}
    {verified && <a className="primary-button verify-login-link"
      href={requiresListingSetup ? '/login?returnTo=%2Fowner%2Flistings%2Fnew' : '/login'}>
      {t('login')}
    </a>}
    <a className="back-link" href="/register">{t('back')}</a>
  </div></AuthLayout>
}
