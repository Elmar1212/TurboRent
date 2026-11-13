import type { ReactNode } from 'react'
import { cityKey, formatDay, formatPrice } from '../../shared/format'
import { useLocale } from '../../shared/i18n/messages'
import { Link } from '../../shared/Link'

export type Booking = {
  id: string; listingId: string; startDate: string; endDate: string; days: number
  dailyPrice: number; totalPrice: number; status: string; message: string | null; createdAtUtc: string
  vehicle: string | null; city: string | null; image: string | null
  contact: { name: string; phone?: string | null; email?: string | null } | null
}

/** One booking, shown to the renter (party = owner) or to the owner (party = renter). */
export function BookingRow({ booking, party, children }: { booking: Booking; party: 'owner' | 'renter'; children?: ReactNode }) {
  const { locale, t } = useLocale()
  return <li className="booking-row">
    <div className="booking-media">
      {booking.image ? <img src={booking.image} alt="" loading="lazy" /> : <span aria-hidden="true">TR</span>}
    </div>
    <div className="booking-body">
      <div className="owner-item-title">
        <h2><Link href={`/cars/${booking.listingId}`}>{booking.vehicle ?? 'TurboRent'}</Link></h2>
        <span className={`status-badge booking-${booking.status.toLowerCase()}`}>{t('bookingStatus_' + booking.status)}</span>
      </div>
      <p className="booking-dates">
        {formatDay(booking.startDate, locale)} – {formatDay(booking.endDate, locale)} · {booking.days} {t('daysLabel')}
        {booking.city && <> · {t(cityKey(booking.city))}</>}
      </p>
      <p className="booking-total">{t('total')}: <b>{formatPrice(booking.totalPrice, locale)} AZN</b></p>
      {booking.contact && <p className="booking-contact">
        {t(party === 'owner' ? 'ownerLabel' : 'renter')}: <b>{booking.contact.name}</b>
        {booking.contact.phone && <> · <a href={`tel:${booking.contact.phone}`}>{booking.contact.phone}</a></>}
        {booking.contact.email && <> · <a href={`mailto:${booking.contact.email}`}>{booking.contact.email}</a></>}
      </p>}
      {booking.message && <p className="booking-message">“{booking.message}”</p>}
    </div>
    {children && <div className="owner-item-actions">{children}</div>}
  </li>
}
