import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { ApiError, getJson, send } from '../../shared/api'
import { addDays, bakuToday, cityKey, daysBetweenInclusive, formatDay, formatListedDate, formatPrice } from '../../shared/format'
import { useLocale } from '../../shared/i18n/messages'
import { SiteLayout } from '../../shared/layout/SiteLayout'
import { Link } from '../../shared/Link'
import { useSession } from '../../shared/session'
import { VehiclePlaceholder } from './VehicleCard'
import './marketplace.css'

type ListingDetails = {
  id: string; make: string; model: string; year: number; city: string; dailyPrice: number
  transmission: string; fuelType: string; seats: number; description: string; createdAtUtc: string
  ownerName: string | null; ownerIsCompany: boolean; images: string[]
}
type Availability = { today: string; booked: { startDate: string; endDate: string }[] }

function Gallery({ listing }: { listing: ListingDetails }) {
  const { t } = useLocale()
  const [active, setActive] = useState(0)
  const name = `${listing.make} ${listing.model}`
  if (!listing.images.length) return <div className="gallery"><VehiclePlaceholder make={listing.make} model={listing.model} /></div>
  return <div className="gallery">
    <div className="gallery-main">
      <img src={listing.images[active]} alt={`${name} ${t('vehiclePhoto')} ${active + 1}`} />
      {listing.images.length > 1 && <>
        <button type="button" className="gallery-nav prev" aria-label="Previous photo"
          onClick={() => setActive(index => (index - 1 + listing.images.length) % listing.images.length)}>‹</button>
        <button type="button" className="gallery-nav next" aria-label="Next photo"
          onClick={() => setActive(index => (index + 1) % listing.images.length)}>›</button>
        <span className="gallery-count">{active + 1} / {listing.images.length}</span>
      </>}
    </div>
    {listing.images.length > 1 && <div className="gallery-thumbs">
      {listing.images.map((image, index) => <button key={image} type="button" aria-pressed={index === active}
        aria-label={`${t('vehiclePhoto')} ${index + 1}`} onClick={() => setActive(index)}>
        <img src={image} alt="" loading="lazy" />
      </button>)}
    </div>}
  </div>
}

function OwnerCard({ listing }: { listing: ListingDetails }) {
  const { t } = useLocale()
  const [phone, setPhone] = useState<string | null>(null)
  const [error, setError] = useState('')
  async function reveal() {
    setError('')
    try { setPhone((await getJson<{ phone: string }>(`/api/v1/catalog/listings/${listing.id}/contact`)).phone) }
    catch (failure) { setError(failure instanceof ApiError && failure.code === 'rate_limited' ? 'rate_limited' : 'phoneError') }
  }
  return <section className="side-card owner-card" aria-label={t('ownerLabel')}>
    <p className="side-label">{t('ownerLabel')}</p>
    <p className="owner-name">{listing.ownerName ?? 'TurboRent'}</p>
    <span className={'owner-type ' + (listing.ownerIsCompany ? 'company' : 'private')}>
      {t(listing.ownerIsCompany ? 'company' : 'owner')}
    </span>
    {phone
      ? <a className="btn btn-phone" href={`tel:${phone}`}>☎ {phone}</a>
      : <button type="button" className="btn btn-phone" onClick={reveal}>☎ {t('showPhone')}</button>}
    {error && <p className="field-error" role="alert">{t(error)}</p>}
  </section>
}

function BookingCard({ listing, availability }: { listing: ListingDetails; availability: Availability | null }) {
  const { locale, t } = useLocale()
  const { session, status } = useSession()
  const today = availability?.today ?? bakuToday()
  const [start, setStart] = useState(() => addDays(today, 1))
  const [end, setEnd] = useState(() => addDays(today, 3))
  const [message, setMessage] = useState('')
  const [busy, setBusy] = useState(false)
  const [notice, setNotice] = useState('')
  const [sent, setSent] = useState(false)
  const valid = start >= today && end >= start && daysBetweenInclusive(start, end) <= 60
  const days = valid ? daysBetweenInclusive(start, end) : 0
  const clash = availability?.booked.some(range => range.startDate <= end && range.endDate >= start) ?? false

  async function request(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!valid) { setNotice('invalid_booking_dates'); return }
    if (clash) { setNotice('dates_unavailable'); return }
    setBusy(true); setNotice('')
    try {
      await send('POST', '/api/v1/bookings', { listingId: listing.id, startDate: start, endDate: end, message: message.trim() || null })
      setSent(true)
    } catch (error) {
      setNotice(error instanceof ApiError ? error.code : 'generic')
    } finally { setBusy(false) }
  }

  return <section className="side-card booking-card" aria-labelledby="booking-title">
    <h2 id="booking-title">{t('bookTitle')}</h2>
    {sent ? <div className="notice notice-success" role="status">
      <p>{t('bookingSent')}</p>
      <Link className="btn btn-ghost" href="/account">{t('account')}</Link>
    </div> : status !== 'loading' && !session ? <>
      <p className="muted">{t('paymentNote')}</p>
      <Link className="btn btn-primary btn-block" href={`/login?returnTo=${encodeURIComponent('/cars/' + listing.id)}`}>{t('bookingSignIn')}</Link>
    </> : <form onSubmit={request} noValidate>
      <div className="field-range">
        <label className="field"><span>{t('startDate')}</span>
          <input type="date" value={start} min={today} max={addDays(today, 365)} required
            onChange={event => { setStart(event.target.value); if (event.target.value > end) setEnd(event.target.value) }} />
        </label>
        <label className="field"><span>{t('endDate')}</span>
          <input type="date" value={end} min={start} max={addDays(start, 59)} required onChange={event => setEnd(event.target.value)} />
        </label>
      </div>
      <p className="field-hint">{t('bookingDatesHint')}</p>
      <label className="field"><span>{t('bookingMessage')}</span>
        <textarea value={message} maxLength={500} rows={3} onChange={event => setMessage(event.target.value)} />
      </label>
      <dl className="price-breakdown">
        <div><dt>{formatPrice(listing.dailyPrice, locale)} AZN × {days} {t('daysLabel')}</dt>
          <dd>{formatPrice(listing.dailyPrice * days, locale)} AZN</dd></div>
        <div className="total"><dt>{t('total')}</dt><dd>{formatPrice(listing.dailyPrice * days, locale)} AZN</dd></div>
      </dl>
      {clash && <p className="field-error" role="alert">{t('dates_unavailable')}</p>}
      {notice && <p className="notice notice-error" role="alert">{t(notice)}</p>}
      <button type="submit" className="btn btn-primary btn-block" disabled={busy || !valid || clash}>
        {busy ? t('loading') : t('requestBooking')}
      </button>
      <p className="muted small">{t('paymentNote')}</p>
    </form>}
    {availability && availability.booked.length > 0 && <div className="booked-dates">
      <p className="side-label">{t('bookedDates')}</p>
      <ul>{availability.booked.map(range => <li key={range.startDate}>
        {formatDay(range.startDate, locale)} – {formatDay(range.endDate, locale)}
      </li>)}</ul>
    </div>}
  </section>
}

export default function ListingDetailPage({ id }: { id: string }) {
  const { locale, t } = useLocale()
  const key = `${id}:${locale}`
  const [state, setState] = useState<{ key: string; listing: ListingDetails | null; error: string } | null>(null)
  const [availability, setAvailability] = useState<Availability | null>(null)
  const current = state?.key === key ? state : null
  const listing = current?.listing ?? null

  useEffect(() => {
    const controller = new AbortController()
    getJson<ListingDetails>(`/api/v1/catalog/listings/${id}?locale=${locale}`, controller.signal)
      .then(value => setState({ key, listing: value, error: '' }))
      .catch(error => {
        if (!controller.signal.aborted)
          setState({ key, listing: null, error: error instanceof ApiError && error.status === 404 ? 'notFound' : 'catalogError' })
      })
    return () => controller.abort()
  }, [id, locale, key])

  useEffect(() => {
    const controller = new AbortController()
    getJson<Availability>(`/api/v1/catalog/listings/${id}/availability`, controller.signal)
      .then(setAvailability).catch(() => { /* the booking form still validates on the server */ })
    return () => controller.abort()
  }, [id])

  useEffect(() => {
    if (listing) document.title = `${listing.year} ${listing.make} ${listing.model} · TurboRent`
    return () => { document.title = 'TurboRent' }
  }, [listing])

  return <SiteLayout wide>
    <p className="breadcrumbs"><Link href="/cars">← {t('backToCars')}</Link></p>
    {!current && <p role="status">{t('loading')}</p>}
    {current?.error && <div className="empty-state" role="alert"><h1>{t(current.error)}</h1>
      <Link className="btn btn-primary" href="/cars">{t('backToCars')}</Link></div>}
    {listing && <div className="detail-layout">
      <div className="detail-main">
        <Gallery listing={listing} />
        <section className="detail-section">
          <h2>{t('specs')}</h2>
          <dl className="spec-table">
            <div><dt>{t('searchCity')}</dt><dd>{t(cityKey(listing.city))}</dd></div>
            <div><dt>{t('make')}</dt><dd>{listing.make}</dd></div>
            <div><dt>{t('model')}</dt><dd>{listing.model}</dd></div>
            <div><dt>{t('vehicleYear')}</dt><dd>{listing.year}</dd></div>
            <div><dt>{t('transmission')}</dt><dd>{t(listing.transmission)}</dd></div>
            <div><dt>{t('fuelType')}</dt><dd>{t(listing.fuelType)}</dd></div>
            <div><dt>{t('seatsSpec')}</dt><dd>{listing.seats}</dd></div>
            <div><dt>{t('listedOn')}</dt><dd>{formatListedDate(listing.createdAtUtc, locale, t('today'))}</dd></div>
          </dl>
        </section>
        <section className="detail-section">
          <h2>{t('vehicleDescription')}</h2>
          <p className="detail-description">{listing.description}</p>
          <p className="muted small">{t('listingNumber')} {listing.id.slice(0, 8).toUpperCase()}</p>
        </section>
      </div>
      <aside className="detail-side">
        <div className="side-card price-card">
          <h1>{listing.make} {listing.model}, {listing.year}</h1>
          <p className="detail-price">{formatPrice(listing.dailyPrice, locale)} <span>AZN / {t('perDay')}</span></p>
          <p className="muted">{t(cityKey(listing.city))} · {t(listing.transmission)} · {t(listing.fuelType)}</p>
        </div>
        <OwnerCard listing={listing} />
        <BookingCard listing={listing} availability={availability} />
      </aside>
    </div>}
  </SiteLayout>
}
