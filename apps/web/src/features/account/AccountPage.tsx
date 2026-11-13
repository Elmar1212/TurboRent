import { useCallback, useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { ApiError, getJson, send } from '../../shared/api'
import { bakuToday } from '../../shared/format'
import { useLocale } from '../../shared/i18n/messages'
import { SiteLayout } from '../../shared/layout/SiteLayout'
import { Link } from '../../shared/Link'
import { isOwnerAccount, useSession } from '../../shared/session'
import { FormField } from '../identity/FormField'
import { BookingRow } from '../owner/BookingRow'
import type { Booking } from '../owner/BookingRow'
import { RequireSignIn } from '../owner/RequireSignIn'
import '../owner/owner.css'
import '../identity/identity.css'

function ChangePassword() {
  const { t } = useLocale()
  const [current, setCurrent] = useState('')
  const [next, setNext] = useState('')
  const [notice, setNotice] = useState<{ kind: 'error' | 'success'; code: string } | null>(null)
  const [busy, setBusy] = useState(false)

  async function submit(event: FormEvent) {
    event.preventDefault()
    if (next.length < 15 || next.length > 128) { setNotice({ kind: 'error', code: 'password_length' }); return }
    setBusy(true); setNotice(null)
    try {
      await send('POST', '/api/v1/auth/password/change', { currentPassword: current, newPassword: next })
      setCurrent(''); setNext('')
      setNotice({ kind: 'success', code: 'passwordChanged' })
    } catch (error) {
      setNotice({ kind: 'error', code: error instanceof ApiError ? error.code : 'generic' })
    } finally { setBusy(false) }
  }

  return <section className="panel" aria-labelledby="password-title">
    <h2 id="password-title">{t('changePasswordTitle')}</h2>
    <form onSubmit={submit} aria-busy={busy}>
      <FormField id="current-password" label={t('currentPassword')} value={current} onChange={setCurrent}
        type="password" autoComplete="current-password" maxLength={128} showLabel={t('show')} hideLabel={t('hide')} />
      <FormField id="new-password" label={t('newPassword')} value={next} onChange={setNext} type="password"
        autoComplete="new-password" maxLength={128} hint={t('passwordHint')} showLabel={t('show')} hideLabel={t('hide')} />
      {notice && <p className={`notice notice-${notice.kind}`} role={notice.kind === 'error' ? 'alert' : 'status'}>{t(notice.code)}</p>}
      <button className="btn btn-primary" type="submit" disabled={busy}>{busy ? t('loading') : t('changePasswordTitle')}</button>
    </form>
  </section>
}

export default function AccountPage() {
  const { locale, t } = useLocale()
  const { session } = useSession()
  const [bookings, setBookings] = useState<Booking[] | null>(null)
  const [notice, setNotice] = useState('')
  const [busyId, setBusyId] = useState('')
  const owner = isOwnerAccount(session)
  const today = bakuToday()

  const load = useCallback(async (signal?: AbortSignal) => {
    try { setBookings((await getJson<{ items: Booking[] }>(`/api/v1/bookings?locale=${locale}`, signal)).items) }
    catch (error) { if (!signal?.aborted) setNotice(error instanceof ApiError ? error.code : 'network') }
  }, [locale])

  useEffect(() => {
    if (!session) return
    const controller = new AbortController()
    void load(controller.signal)
    return () => controller.abort()
  }, [session, load])

  async function cancel(booking: Booking) {
    if (!window.confirm(t('confirmCancel'))) return
    setBusyId(booking.id); setNotice('')
    try { await send('POST', `/api/v1/bookings/${booking.id}/cancel`, {}); await load() }
    catch (error) { setNotice(error instanceof ApiError ? error.code : 'generic') }
    finally { setBusyId('') }
  }

  return <SiteLayout>
    <RequireSignIn returnTo="/account">
      <div className="page-head"><h1>{t('accountTitle')}</h1></div>
      <div className="account-layout">
        <div>
          <section className="panel" aria-labelledby="bookings-title">
            <h2 id="bookings-title">{t('myBookingsTitle')}</h2>
            {notice && <p className="notice notice-error" role="alert">{t(notice)}</p>}
            {bookings === null && !notice && <p role="status">{t('loading')}</p>}
            {bookings?.length === 0 && <div className="empty-inline">
              <p>{t('noBookings')}</p>
              <Link className="btn btn-primary btn-small" href="/cars">{t('browseCars')}</Link>
            </div>}
            {bookings && bookings.length > 0 && <ul className="booking-list">
              {bookings.map(booking => <BookingRow key={booking.id} booking={booking} party="owner">
                {(booking.status === 'Requested' || booking.status === 'Confirmed') && booking.startDate > today &&
                  <button type="button" className="btn btn-ghost btn-small" disabled={busyId === booking.id}
                    onClick={() => cancel(booking)}>{t('cancelBooking')}</button>}
              </BookingRow>)}
            </ul>}
          </section>
        </div>
        <div className="account-side">
          <section className="panel" aria-labelledby="profile-title">
            <h2 id="profile-title">{t('profile')}</h2>
            <p className="profile-name">{session?.displayName}</p>
            <p className="muted">{session?.email}</p>
            {!owner && <p className="muted small">{t('accountType_customer')}</p>}
            {owner && <div className="button-column">
              <p className="side-label">{t('ownerTools')}</p>
              <Link className="btn btn-ghost btn-small" href="/owner/listings">{t('myListings')}</Link>
              <Link className="btn btn-ghost btn-small" href="/owner/bookings">{t('ownerBookings')}</Link>
            </div>}
          </section>
          <ChangePassword />
        </div>
      </div>
    </RequireSignIn>
  </SiteLayout>
}
