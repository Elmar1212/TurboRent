import { useEffect, useState } from 'react'
import { ApiError, submitTo } from '../identity/api'
import { useLocale } from '../../shared/i18n/messages'
import { SiteLayout } from '../../shared/layout/SiteLayout'
import './admin.css'

type AdminListing = {
  id: string
  make: string
  model: string
  year: number
  city: string
  dailyPrice: number
  transmission: string
  fuelType: string
  seats: number
  descriptionAz: string
  descriptionEn: string
  ownerName: string | null
  ownerEmail: string | null
  createdAtUtc: string
  images: string[]
}

type ListingPage = { items: AdminListing[]; total: number; page: number; pageSize: number }
const reasons = ['missing_information', 'invalid_vehicle_details', 'image_quality', 'other'] as const

async function fetchPendingListings(page: number, locale: string, signal?: AbortSignal): Promise<ListingPage | null> {
  const response = await fetch(`/api/v1/admin/listings?page=${page}&pageSize=20&locale=${locale}`, {
    credentials: 'same-origin',
    cache: 'no-store',
    signal,
  })
  if (response.status === 401) {
    location.assign('/login?returnTo=%2Fadmin')
    return null
  }
  if (!response.ok) throw new Error('adminLoadError')
  return await response.json() as ListingPage
}

export default function AdminListingsPage() {
  const language = useLocale()
  const { t } = language
  const [listings, setListings] = useState<AdminListing[]>([])
  const [total, setTotal] = useState(0)
  const [page, setPage] = useState(1)
  const [pageCount, setPageCount] = useState(1)
  const [authorized, setAuthorized] = useState(false)
  const [checked, setChecked] = useState(false)
  const [busyId, setBusyId] = useState('')
  const [reasonById, setReasonById] = useState<Record<string, string>>({})
  const [notice, setNotice] = useState('')

  function showListings(result: ListingPage) {
    setListings(result.items)
    setTotal(result.total)
    setPageCount(Math.max(1, Math.ceil(result.total / result.pageSize)))
    setNotice('')
  }

  useEffect(() => {
    const controller = new AbortController()
    fetch('/api/v1/auth/session', {
      credentials: 'same-origin', cache: 'no-store', signal: controller.signal,
    }).then(async response => {
      if (response.status === 401) {
        location.assign('/login?returnTo=%2Fadmin')
        return
      }
      if (!response.ok) throw new Error('adminLoadError')
      const session = await response.json()
      if (session.isAdmin !== true) {
        setChecked(true)
        return
      }
      setAuthorized(true)
      setChecked(true)
    }).catch(() => {
      if (!controller.signal.aborted) {
        setNotice('adminLoadError')
        setChecked(true)
      }
    })
    return () => controller.abort()
  }, [])

  useEffect(() => {
    if (!authorized) return
    const controller = new AbortController()
    fetchPendingListings(page, language.locale, controller.signal).then(result => {
      if (result && !controller.signal.aborted) showListings(result)
    }).catch(() => {
      if (!controller.signal.aborted) setNotice('adminLoadError')
    })
    return () => controller.abort()
  }, [authorized, page, language.locale])

  async function review(listing: AdminListing, decision: 'approve' | 'reject') {
    setBusyId(listing.id)
    setNotice('')
    try {
      const reasonCode = reasonById[listing.id]
      if (decision === 'reject' && !reasons.includes(reasonCode as typeof reasons[number])) {
        setNotice('invalid_rejection_reason')
        return
      }
      await submitTo(`/api/v1/admin/listings/${listing.id}/${decision}`,
        decision === 'reject' ? { reasonCode } : {})
      const result = await fetchPendingListings(page, language.locale)
      if (!result) return
      showListings(result)
      setNotice('adminActionSuccess')
    } catch (error) {
      setNotice(error instanceof ApiError && error.code === 'invalid_csrf'
        ? 'invalid_csrf' : 'adminActionError')
    } finally {
      setBusyId('')
    }
  }

  if (!checked) return <SiteLayout><p role="status">{t('loading')}</p></SiteLayout>
  if (checked && !authorized) return <SiteLayout><div className="admin-page">
    <h1>{t('adminPanel')}</h1>
    <p role="alert">{t('adminAccessDenied')}</p>
    <a href="/login?returnTo=%2Fadmin">{t('login')}</a>
  </div></SiteLayout>

  return <SiteLayout wide>
    <div className="admin-page">
      <div className="admin-heading">
        <div>
          <p className="card-step">TURBORENT / ADMIN</p>
          <h1>{t('adminListingsTitle')}</h1>
          <p>{t('adminListingsIntro')} · {total}</p>
          <nav className="admin-section-nav" aria-label={t('adminPanel')}>
            <span aria-current="page">{t('adminListingsLink')}</span>
            <a href="/admin/accounts">{t('adminAccountsLink')}</a>
          </nav>
        </div>
      </div>
      {notice && <p className="form-notice" role="alert">{t(notice)}</p>}
      {listings.length === 0 && !notice && <section className="admin-empty">{t('adminNoListings')}</section>}
      <div className="admin-list">
        {listings.map(listing => <article className="admin-listing" key={listing.id}>
          <div className="admin-listing-content">
            <div className="admin-listing-title">
              <h2>{listing.year} {listing.make} {listing.model}</h2>
              <span>{listing.dailyPrice.toFixed(2)} AZN / {t('perDay')}</span>
            </div>
            <p>{t(`city${listing.city[0].toUpperCase()}${listing.city.slice(1)}`)} · {t(listing.transmission)} · {t(listing.fuelType)} · {listing.seats} {t('seatsLabel')}</p>
            <p className="admin-submitted">{t('adminCreatedAt')}: {new Date(listing.createdAtUtc).toLocaleString(language.locale)}</p>
            {listing.ownerName && <p className="admin-submitted">
              {t('adminListingOwner')}: {listing.ownerName}{listing.ownerEmail ? ` · ${listing.ownerEmail}` : ''}
            </p>}
            <h3>{t('adminDescriptionAz')}</h3>
            <p className="admin-description">{listing.descriptionAz}</p>
            {listing.descriptionEn !== listing.descriptionAz && <p className="admin-description">{listing.descriptionEn}</p>}
          </div>
          {listing.images.length > 0 && <div className="admin-images">
            {listing.images.map((image, index) => <a href={image} target="_blank" rel="noreferrer" key={image}>
              <img src={image} alt={`${listing.make} ${listing.model} ${index + 1}`} loading="lazy" />
            </a>)}
          </div>}
          <div className="admin-review-actions">
            <label>
              <span>{t('adminReason')}</span>
              <select value={reasonById[listing.id] ?? ''} onChange={event =>
                setReasonById(current => ({ ...current, [listing.id]: event.target.value }))}>
                <option value="">—</option>
                {reasons.map(reason => <option value={reason} key={reason}>{t(`adminReason_${reason}`)}</option>)}
              </select>
            </label>
            <div className="admin-review-buttons">
              <button type="button" className="admin-approve" disabled={busyId === listing.id}
                onClick={() => review(listing, 'approve')}>{t('adminApprove')}</button>
              <button type="button" className="admin-reject" disabled={busyId === listing.id || !reasonById[listing.id]}
                onClick={() => review(listing, 'reject')}>{t('adminReject')}</button>
            </div>
          </div>
        </article>)}
      </div>
      {pageCount > 1 && <nav className="admin-pagination" aria-label="Pagination">
        <button type="button" disabled={page <= 1} onClick={() => setPage(current => current - 1)}>←</button>
        <span>{page} / {pageCount}</span>
        <button type="button" disabled={page >= pageCount} onClick={() => setPage(current => current + 1)}>→</button>
      </nav>}
    </div>
  </SiteLayout>
}
