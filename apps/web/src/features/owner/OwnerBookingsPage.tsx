import { useCallback, useEffect, useState } from 'react'
import { ApiError, getJson, send } from '../../shared/api'
import { useLocale } from '../../shared/i18n/messages'
import { SiteLayout } from '../../shared/layout/SiteLayout'
import { useSession } from '../../shared/session'
import { BookingRow } from './BookingRow'
import type { Booking } from './BookingRow'
import { RequireSignIn } from './RequireSignIn'
import './owner.css'

export default function OwnerBookingsPage() {
  const { locale, t } = useLocale()
  const { session } = useSession()
  const [items, setItems] = useState<Booking[] | null>(null)
  const [notice, setNotice] = useState('')
  const [busyId, setBusyId] = useState('')

  const load = useCallback(async (signal?: AbortSignal) => {
    try { setItems((await getJson<{ items: Booking[] }>(`/api/v1/owner/bookings?locale=${locale}`, signal)).items) }
    catch (error) { if (!signal?.aborted) setNotice(error instanceof ApiError ? error.code : 'network') }
  }, [locale])

  useEffect(() => {
    if (!session) return
    const controller = new AbortController()
    void load(controller.signal)
    return () => controller.abort()
  }, [session, load])

  async function decide(booking: Booking, decision: 'confirm' | 'decline') {
    if (decision === 'decline' && !window.confirm(t('confirmDecline'))) return
    setBusyId(booking.id); setNotice('')
    try {
      await send('POST', `/api/v1/owner/bookings/${booking.id}/${decision}`, {})
      await load()
    } catch (error) {
      setNotice(error instanceof ApiError ? error.code : 'generic')
    } finally { setBusyId('') }
  }

  return <SiteLayout>
    <RequireSignIn returnTo="/owner/bookings">
      <div className="page-head">
        <h1>{t('ownerBookingsTitle')}</h1>
        <p className="page-intro">{t('ownerBookingsIntro')}</p>
      </div>
      {notice && <p className="notice notice-error" role="alert">{t(notice)}</p>}
      {items === null && !notice && <p role="status">{t('loading')}</p>}
      {items?.length === 0 && <div className="empty-state"><h2>{t('noOwnerBookings')}</h2></div>}
      {items && items.length > 0 && <ul className="booking-list">
        {items.map(booking => <BookingRow key={booking.id} booking={booking} party="renter">
          {booking.status === 'Requested' && <>
            <button type="button" className="btn btn-primary btn-small" disabled={busyId === booking.id}
              onClick={() => decide(booking, 'confirm')}>{t('confirmBooking')}</button>
            <button type="button" className="btn btn-ghost btn-small" disabled={busyId === booking.id}
              onClick={() => decide(booking, 'decline')}>{t('declineBooking')}</button>
          </>}
        </BookingRow>)}
      </ul>}
    </RequireSignIn>
  </SiteLayout>
}
