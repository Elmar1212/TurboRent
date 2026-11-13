import { useCallback, useEffect, useState } from 'react'
import { ApiError, getJson, send } from '../../shared/api'
import { cityKey, formatListedDate, formatPrice } from '../../shared/format'
import { useLocale } from '../../shared/i18n/messages'
import { SiteLayout } from '../../shared/layout/SiteLayout'
import { Link } from '../../shared/Link'
import { useSession } from '../../shared/session'
import { VehiclePlaceholder } from '../marketplace/VehicleCard'
import { RequireSignIn } from './RequireSignIn'
import './owner.css'
import '../marketplace/marketplace.css'

type OwnerListing = {
  id: string; make: string; model: string; year: number; city: string; dailyPrice: number
  status: string; createdAtUtc: string; updatedAtUtc: string | null; rejectionReason: string | null
  pendingBookings: number; images: string[]
}

export default function OwnerListingsPage() {
  const { locale, t } = useLocale()
  const { session } = useSession()
  const [items, setItems] = useState<OwnerListing[] | null>(null)
  const [notice, setNotice] = useState<{ kind: 'error' | 'success'; code: string } | null>(null)
  const [busyId, setBusyId] = useState('')

  const load = useCallback(async (signal?: AbortSignal) => {
    try { setItems((await getJson<{ items: OwnerListing[] }>(`/api/v1/owner/listings?locale=${locale}`, signal)).items) }
    catch (error) { if (!signal?.aborted) setNotice({ kind: 'error', code: error instanceof ApiError ? error.code : 'network' }) }
  }, [locale])

  useEffect(() => {
    if (!session) return
    const controller = new AbortController()
    void load(controller.signal)
    return () => controller.abort()
  }, [session, load])

  async function act(listing: OwnerListing, action: 'archive' | 'restore' | 'delete') {
    const question = action === 'delete' ? t('confirmDelete') : action === 'archive' ? t('confirmArchive') : null
    if (question && !window.confirm(question)) return
    setBusyId(listing.id); setNotice(null)
    try {
      if (action === 'delete') await send('DELETE', `/api/v1/owner/listings/${listing.id}`)
      else await send('POST', `/api/v1/owner/listings/${listing.id}/${action}`, {})
      setNotice({ kind: 'success', code: 'ownerActionDone' })
      await load()
    } catch (error) {
      setNotice({ kind: 'error', code: error instanceof ApiError ? error.code : 'generic' })
    } finally { setBusyId('') }
  }

  return <SiteLayout>
    <RequireSignIn returnTo="/owner/listings">
      <div className="page-head row">
        <div>
          <h1>{t('myListingsTitle')}</h1>
          <p className="page-intro">{t('myListingsIntro')}</p>
        </div>
        {session?.canCreateListing && <Link className="btn btn-primary" href="/owner/listings/new">+ {t('newListing')}</Link>}
      </div>
      {session && !session.canCreateListing && <p className="notice notice-warning">
        {session.accountApprovalStatus ? t(`accountApproval_${session.accountApprovalStatus.toLowerCase()}`) : t('owner_account_required')}
      </p>}
      {notice && <p className={`notice notice-${notice.kind}`} role={notice.kind === 'error' ? 'alert' : 'status'}>{t(notice.code)}</p>}
      {items === null && !notice && <p role="status">{t('loading')}</p>}
      {items?.length === 0 && <div className="empty-state">
        <h2>{t('noOwnerListings')}</h2>
        {session?.canCreateListing && <Link className="btn btn-primary" href="/owner/listings/new">{t('newListing')}</Link>}
      </div>}
      {items && items.length > 0 && <ul className="owner-list">
        {items.map(listing => <li key={listing.id} className="owner-item">
          <div className="owner-item-media">
            {listing.images[0] ? <img src={listing.images[0]} alt="" loading="lazy" />
              : <VehiclePlaceholder make={listing.make} model={listing.model} />}
          </div>
          <div className="owner-item-body">
            <div className="owner-item-title">
              <h2>{listing.make} {listing.model}, {listing.year}</h2>
              <span className={`status-badge status-${listing.status.toLowerCase()}`}>{t('status_' + listing.status)}</span>
            </div>
            <p className="owner-item-meta">
              <b>{formatPrice(listing.dailyPrice, locale)} AZN / {t('perDay')}</b> · {t(cityKey(listing.city))} ·{' '}
              {formatListedDate(listing.updatedAtUtc ?? listing.createdAtUtc, locale, t('today'))}
            </p>
            {listing.rejectionReason && <p className="owner-item-reason">
              {t('rejectionReason')}: {t('adminReason_' + listing.rejectionReason)}
            </p>}
            {listing.pendingBookings > 0 && <Link className="owner-item-requests" href="/owner/bookings">
              {listing.pendingBookings} {t('pendingRequests')} →
            </Link>}
          </div>
          <div className="owner-item-actions">
            {listing.status === 'Approved' && <Link className="btn btn-ghost btn-small" href={`/cars/${listing.id}`}>{t('view')}</Link>}
            {listing.status !== 'Archived' && listing.status !== 'Suspended' &&
              <Link className="btn btn-ghost btn-small" href={`/owner/listings/${listing.id}/edit`}>{t('edit')}</Link>}
            {['Pending', 'Approved', 'Rejected'].includes(listing.status) &&
              <button type="button" className="btn btn-ghost btn-small" disabled={busyId === listing.id}
                onClick={() => act(listing, 'archive')}>{t('archive')}</button>}
            {listing.status === 'Archived' &&
              <button type="button" className="btn btn-ghost btn-small" disabled={busyId === listing.id}
                onClick={() => act(listing, 'restore')}>{t('restore')}</button>}
            <button type="button" className="btn btn-danger btn-small" disabled={busyId === listing.id}
              onClick={() => act(listing, 'delete')}>{t('deleteListing')}</button>
          </div>
        </li>)}
      </ul>}
    </RequireSignIn>
  </SiteLayout>
}
