import { useResendCooldown } from './useResendCooldown'
import { useState } from 'react'
import type { FormEvent } from 'react'
import { useLocale } from '../../shared/i18n/messages'
import { AuthLayout } from './AuthLayout'
import { FormField } from './FormField'
import { ApiError, submit } from './api'

export default function RegisterPage() {
  const language = useLocale()
  const { t, locale } = language
  const [accountType, setAccountType] = useState('customer')
  const [form, setForm] = useState({ displayName: '', email: '', password: '', confirm: '', contactPhone: '', legalName: '', voen: '' })
  const [errors, setErrors] = useState<Record<string, string>>({})
  const [notice, setNotice] = useState('')
  const [busy, setBusy] = useState(false)
  const [sent, setSent] = useState(false)
  const cooldown = useResendCooldown()
  function change(key: keyof typeof form, value: string) { setForm(previous => ({ ...previous, [key]: value })); setErrors(previous => ({ ...previous, [key]: '' })) }
  async function register(event: FormEvent) {
    event.preventDefault()
    const next: Record<string, string> = {}
    if (!form.displayName.trim() || form.displayName.trim().length > 100) next.displayName = 'invalid_name'
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email)) next.email = 'invalid_email'
    if (form.password.length < 15 || form.password.length > 128) next.password = 'password_length'
    if (form.password !== form.confirm) next.confirm = 'mismatch'
    if (accountType !== 'customer' && !/^\+994[0-9]{9}$/.test(form.contactPhone)) next.contactPhone = 'invalid_phone'
    if (accountType === 'company' && !form.legalName.trim()) next.legalName = 'invalid_company_name'
    if (accountType === 'company' && !/^[0-9]{10}$/.test(form.voen)) next.voen = 'invalid_voen'
    setErrors(next); setNotice('')
    if (Object.keys(next).length) { document.getElementById(Object.keys(next)[0])?.focus(); return }
    setBusy(true)
    try {
      const { confirm: _confirm, ...payload } = form
      await submit('register', { ...payload, locale, accountType })
      setSent(true); setForm(previous => ({ ...previous, password: '', confirm: '' }))
    } catch (error) {
      const api = error instanceof ApiError ? error : new ApiError('generic')
      setNotice(api.code)
      if (api.code === 'local_mailbox_unavailable') {
        setSent(true)
        setForm(previous => ({ ...previous, password: '', confirm: '' }))
      }
      const fields = Object.fromEntries(Object.entries(api.fields).map(([key, codes]) => [key, codes[0]]))
      if (api.code !== 'local_mailbox_unavailable') {
        setErrors(fields)
        document.getElementById(Object.keys(fields)[0] ?? 'form-notice')?.focus()
      }
    } finally { setBusy(false) }
  }
  async function resend() {
    if (cooldown.remaining > 0) return
    setBusy(true); setNotice('')
    try { await submit('email/resend', { email: form.email }); setNotice('resendSent') }
    catch (error) {
      setNotice(error instanceof ApiError ? error.code : 'generic')
      if (error instanceof ApiError && error.code === 'rate_limited') cooldown.start(error.retryAfterSeconds)
    }
    finally { setBusy(false) }
  }
  const field = (id: keyof typeof form, label: string, type = 'text', autoComplete?: string, maxLength?: number) =>
    <FormField key={id} id={id} label={t(label)} value={form[id]} onChange={value => change(id, value)}
      type={type} autoComplete={autoComplete} maxLength={maxLength} error={errors[id] ? t(errors[id]) : undefined}
      hint={id === 'password' ? t('passwordHint') : undefined} showLabel={t('show')} hideLabel={t('hide')} />
  return <AuthLayout language={language}>
    {sent ? <div className="success-panel">
      <div className="success-symbol" aria-hidden="true">✓</div><h2>{t('sentTitle')}</h2>
      <p className="sent-to">{t('sentTo').replace('{email}', form.email)}</p>
      {notice !== 'local_mailbox_unavailable' && <p>{t('localMailboxHint')}</p>}
      <button className="primary-button" disabled={busy || cooldown.remaining > 0} onClick={resend}>{busy ? t('loading') : t('resend')}</button>
      {notice && <p role={notice === 'local_mailbox_unavailable' ? 'alert' : 'status'}>{t(notice)}</p>}
      {cooldown.remaining > 0 && <p role="status" data-testid="resend-countdown">{t("retryIn").replace("{seconds}", String(cooldown.remaining))}</p>}
    </div> : <>
      <p className="card-step">01 / TURBORENT</p><h2>{t('register')}</h2><p className="card-subtitle">{t('subtitle')}</p>
      <form onSubmit={register} noValidate aria-busy={busy}>
        <fieldset className="account-types"><legend className="sr-only">{t('subtitle')}</legend>
          {['customer', 'owner', 'company'].map(type => <label key={type} className={accountType === type ? 'selected' : ''}>
            <input type="radio" name="accountType" value={type} checked={accountType === type} onChange={() => setAccountType(type)} />{t(type)}
          </label>)}
        </fieldset>
        {accountType !== 'customer' && <p className="pending-note">{t('pending')}</p>}
        {field('displayName', 'name', 'text', 'name', 100)}
        {field('email', 'email', 'email', 'email')}
        {accountType !== 'customer' && field('contactPhone', 'phone', 'tel', 'tel', 13)}
        {accountType === 'company' && <div className="company-fields">{field('legalName', 'legalName', 'text', 'organization', 200)}{field('voen', 'voen', 'text', 'off', 10)}</div>}
        {field('password', 'password', 'password', 'new-password', 128)}
        {field('confirm', 'confirm', 'password', 'new-password', 128)}
        {notice && <p className="form-notice" id="form-notice" role="alert" tabIndex={-1}>{t(notice)}</p>}
        <button className="primary-button" disabled={busy} type="submit">{busy ? t('loading') : t('submit')}<span aria-hidden="true">↗</span></button>
        <p className="privacy-note">{t('privacy')}</p>
      </form>
      <a className="back-link" href="/login">{t('login')}</a>
    </>}
  </AuthLayout>
}
