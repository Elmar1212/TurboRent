import { useEffect, useMemo, useState } from 'react'
import type { FormEvent } from 'react'
import { ApiError, getJson, send } from '../../shared/api'
import { cities, cityKey, fuelTypes, transmissions } from '../../shared/format'
import { useLocale } from '../../shared/i18n/messages'
import { SiteLayout } from '../../shared/layout/SiteLayout'
import { Link } from '../../shared/Link'
import { navigate } from '../../shared/router'
import { useSession } from '../../shared/session'
import { useCatalogMakes } from './useCatalogMakes'
import './marketplace.css'

const maxImageBytes = 10 * 1024 * 1024
const maxTotalImageBytes = 50 * 1024 * 1024
const maxImageEdge = 1920
const currentYear = new Date().getFullYear()

type ListingForm = {
  make: string; modelId: string; year: string; city: string; dailyPrice: string
  transmission: string; fuelType: string; seats: string; descriptionAz: string; descriptionEn: string
}
type OwnerListing = {
  makeSlug: string; modelId: number; year: number; city: string; dailyPrice: number; transmission: string
  fuelType: string; seats: number; descriptionAz: string; descriptionEn: string; status: string
}

const emptyForm: ListingForm = {
  make: '', modelId: '', year: String(currentYear), city: 'baku', dailyPrice: '',
  transmission: 'automatic', fuelType: 'petrol', seats: '5', descriptionAz: '', descriptionEn: '',
}

/**
 * Re-encodes a photo in the browser: applies the camera orientation, limits the longest edge
 * and drops all metadata (including GPS location). Falls back to the original file if the
 * browser cannot decode it; the server strips metadata as well.
 */
async function preparePhoto(file: File): Promise<File> {
  try {
    const bitmap = await createImageBitmap(file, { imageOrientation: 'from-image' })
    const scale = Math.min(1, maxImageEdge / Math.max(bitmap.width, bitmap.height))
    const canvas = document.createElement('canvas')
    canvas.width = Math.round(bitmap.width * scale)
    canvas.height = Math.round(bitmap.height * scale)
    canvas.getContext('2d')?.drawImage(bitmap, 0, 0, canvas.width, canvas.height)
    bitmap.close()
    const blob = await new Promise<Blob | null>(resolve => canvas.toBlob(resolve, 'image/jpeg', 0.85))
    return blob ? new File([blob], file.name.replace(/\.[^.]+$/, '') + '.jpg', { type: 'image/jpeg' }) : file
  } catch {
    return file
  }
}

export default function OwnerListingPage({ editId }: { editId?: string }) {
  const { t, locale } = useLocale()
  const { session, status } = useSession()
  const { makes } = useCatalogMakes(locale)
  const [files, setFiles] = useState<File[]>([])
  const [notice, setNotice] = useState('')
  const [busy, setBusy] = useState(false)
  const [processing, setProcessing] = useState(false)
  const [done, setDone] = useState(false)
  const [loadedId, setLoadedId] = useState<string | null>(null)
  const [form, setForm] = useState<ListingForm>(emptyForm)
  const models = useMemo(() => makes.find(make => make.slug === form.make)?.models ?? [], [form.make, makes])
  const previews = useMemo(() => files.map(file => URL.createObjectURL(file)), [files])
  const editing = !!editId
  const approved = session?.canCreateListing === true
  const approvalStatus = session?.accountApprovalStatus ?? null

  useEffect(() => () => previews.forEach(url => URL.revokeObjectURL(url)), [previews])

  useEffect(() => {
    if (!editId || !approved) return
    const controller = new AbortController()
    getJson<OwnerListing>(`/api/v1/owner/listings/${editId}?locale=${locale}`, controller.signal).then(listing => {
      setForm({
        make: listing.makeSlug, modelId: String(listing.modelId), year: String(listing.year), city: listing.city,
        dailyPrice: String(listing.dailyPrice), transmission: listing.transmission, fuelType: listing.fuelType,
        seats: String(listing.seats), descriptionAz: listing.descriptionAz,
        descriptionEn: listing.descriptionEn === listing.descriptionAz ? '' : listing.descriptionEn,
      })
      setLoadedId(editId)
    }).catch(error => { if (!controller.signal.aborted) setNotice(error instanceof ApiError ? error.code : 'network') })
    return () => controller.abort()
  }, [editId, approved, locale])

  function change(key: keyof ListingForm, value: string) {
    setForm(previous => ({ ...previous, [key]: value }))
    setNotice('')
  }

  async function selectFiles(selected: FileList | null) {
    const chosen = Array.from(selected ?? [])
    if (chosen.length > 5) { setFiles([]); setNotice('tooManyImages'); return }
    if (chosen.some(file => !['image/jpeg', 'image/png', 'image/webp'].includes(file.type) || file.size === 0)) {
      setFiles([]); setNotice('invalidImageSelection'); return
    }
    setProcessing(true)
    const prepared = await Promise.all(chosen.map(preparePhoto))
    setProcessing(false)
    if (prepared.some(file => file.size > maxImageBytes)) { setFiles([]); setNotice('invalidImageSelection'); return }
    if (prepared.reduce((total, file) => total + file.size, 0) > maxTotalImageBytes) {
      setFiles([]); setNotice('imageTotalTooLarge'); return
    }
    setFiles(prepared)
    setNotice('')
  }

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setNotice('')
    if ((!editing && files.length === 0) || !form.modelId || !form.descriptionAz.trim()) {
      setNotice('requiredListingFields')
      return
    }
    setBusy(true)
    try {
      if (editing) {
        await send('PUT', `/api/v1/owner/listings/${editId}`, {
          modelId: Number(form.modelId), year: Number(form.year), city: form.city, dailyPrice: Number(form.dailyPrice),
          transmission: form.transmission, fuelType: form.fuelType, seats: Number(form.seats),
          descriptionAz: form.descriptionAz, descriptionEn: form.descriptionEn || null,
        })
      } else {
        const payload = new FormData()
        for (const [key, value] of Object.entries(form)) payload.append(key, value)
        files.forEach(file => payload.append('images', file))
        await send('POST', '/api/v1/owner/listings', payload)
        setFiles([])
      }
      setDone(true)
    } catch (error) {
      setNotice(error instanceof ApiError ? error.code : 'generic')
    } finally {
      setBusy(false)
    }
  }

  const title = editing ? t('editListingTitle') : t('ownerListingTitle')
  if (done) return <SiteLayout>
    <div className="form-page"><div className="panel success-panel">
      <div className="success-symbol" aria-hidden="true">✓</div>
      <h1>{editing ? t('listingSaved') : t('listingSubmittedTitle')}</h1>
      {!editing && <p>{t('listingSubmitted')}</p>}
      <div className="button-row">
        <Link className="btn btn-primary" href="/owner/listings">{t('myListings')}</Link>
        {!editing && <button type="button" className="btn btn-ghost" onClick={() => { setForm(emptyForm); setDone(false) }}>{t('newListing')}</button>}
      </div>
    </div></div>
  </SiteLayout>

  return <SiteLayout>
    <div className="form-page">
      <p className="breadcrumbs"><Link href="/owner/listings">{t('myListings')}</Link> › {title}</p>
      <div className="panel">
        <h1>{title}</h1>
        <p className="page-intro">{editing ? t('editNotice') : t('ownerListingIntro')}</p>
        {status === 'loading' && <p role="status">{t('loading')}</p>}
        {status !== 'loading' && !session && <div className="notice notice-info">
          <p>{t('ownerListingSignIn')}</p>
          <div className="button-row">
            <Link className="btn btn-primary" href="/login?returnTo=%2Fowner%2Flistings%2Fnew">{t('login')}</Link>
            <Link className="btn btn-ghost" href="/register">{t('homeRegister')}</Link>
          </div>
        </div>}
        {session && !approved && <div className="notice notice-warning" role="status">
          <p>{approvalStatus ? t(`accountApproval_${approvalStatus.toLowerCase()}`) : t('owner_account_required')}</p>
          {approvalStatus === 'Rejected' && session.accountApprovalReason &&
            <p>{t(`accountApprovalReason_${session.accountApprovalReason}`)}</p>}
        </div>}
        {session && (!editing || loadedId === editId || !approved) &&
        <form onSubmit={submit} noValidate aria-busy={busy}>
          {/* Pending owners see the form so they know what to prepare, but cannot submit it yet. */}
          <fieldset disabled={!approved || busy} className="form-fieldset">
            <div className="form-grid">
              <label className="field"><span>{t('make')}</span>
                <select value={form.make} required onChange={event => {
                  const make = event.target.value
                  setForm(previous => ({ ...previous, make, modelId: '' }))
                }}>
                  <option value="">{t('allMakes')}</option>
                  {makes.map(make => <option key={make.slug} value={make.slug}>{make.name}</option>)}
                </select>
              </label>
              <label className="field"><span>{t('model')}</span>
                <select value={form.modelId} required disabled={!form.make} onChange={event => change('modelId', event.target.value)}>
                  <option value="">{t('allModels')}</option>
                  {models.map(model => <option key={model.slug} value={model.id}>{model.name}</option>)}
                </select>
              </label>
              <label className="field"><span>{t('vehicleYear')}</span>
                <input type="number" inputMode="numeric" min="1980" max="2035" value={form.year} required onChange={event => change('year', event.target.value)} />
              </label>
              <label className="field"><span>{t('searchCity')}</span>
                <select value={form.city} onChange={event => change('city', event.target.value)}>
                  {cities.map(city => <option key={city} value={city}>{t(cityKey(city))}</option>)}
                </select>
              </label>
              <label className="field"><span>{t('dailyPriceLabel')}</span>
                <input type="number" inputMode="decimal" min="0.01" max="99999" step="0.01" value={form.dailyPrice} required onChange={event => change('dailyPrice', event.target.value)} />
              </label>
              <label className="field"><span>{t('transmission')}</span>
                <select value={form.transmission} onChange={event => change('transmission', event.target.value)}>
                  {transmissions.map(item => <option key={item} value={item}>{t(item)}</option>)}
                </select>
              </label>
              <label className="field"><span>{t('fuelType')}</span>
                <select value={form.fuelType} onChange={event => change('fuelType', event.target.value)}>
                  {fuelTypes.map(item => <option key={item} value={item}>{t(item)}</option>)}
                </select>
              </label>
              <label className="field"><span>{t('seatsSpec')}</span>
                <input type="number" inputMode="numeric" min="2" max="9" value={form.seats} required onChange={event => change('seats', event.target.value)} />
              </label>
            </div>
            <label className="field"><span>{t('vehicleDescription')}</span>
              <textarea value={form.descriptionAz} maxLength={2000} rows={5} required onChange={event => change('descriptionAz', event.target.value)} />
            </label>
            <label className="field"><span>{t('descriptionEnOptional')}</span>
              <textarea value={form.descriptionEn} maxLength={2000} rows={4} onChange={event => change('descriptionEn', event.target.value)} />
            </label>
            {!editing && <section className="image-upload">
              <label htmlFor="vehicle-images">{t('vehiclePhotos')}</label>
              <p>{t('vehiclePhotosHint')} {t('photoTip')}</p>
              <input id="vehicle-images" type="file" accept="image/jpeg,image/png,image/webp" multiple
                onChange={event => void selectFiles(event.currentTarget.files)} />
              {processing && <p role="status">{t('imageProcessing')}</p>}
              {previews.length > 0 && <div className="image-previews">
                {previews.map((url, index) => <img key={url} src={url} alt={`${t('vehiclePhoto')} ${index + 1}`} />)}
              </div>}
            </section>}
          </fieldset>
          {notice && <p className="notice notice-error" role="alert">{t(notice)}</p>}
          <p className="muted small">{t('pendingListingNotice')}</p>
          <div className="button-row">
            <button className="btn btn-primary" disabled={busy || processing || !approved} type="submit">
              {busy ? t('loading') : editing ? t('saveChanges') : t('submitListing')}
            </button>
            {editing && <button type="button" className="btn btn-ghost" onClick={() => navigate('/owner/listings')}>{t('myListings')}</button>}
          </div>
        </form>}
        {editing && approved && loadedId !== editId && !notice && <p role="status">{t('loading')}</p>}
        {editing && approved && loadedId !== editId && notice && <p className="notice notice-error" role="alert">{t(notice)}</p>}
      </div>
    </div>
  </SiteLayout>
}
