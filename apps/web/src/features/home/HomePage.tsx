import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { getJson } from '../../shared/api'
import { cities, cityKey } from '../../shared/format'
import { useLocale } from '../../shared/i18n/messages'
import { SiteLayout } from '../../shared/layout/SiteLayout'
import { Link } from '../../shared/Link'
import { navigate } from '../../shared/router'
import { useCatalogMakes } from '../marketplace/useCatalogMakes'
import { VehicleCard } from '../marketplace/VehicleCard'
import type { ListingSummary } from '../marketplace/VehicleCard'
import '../marketplace/marketplace.css'
import './home.css'

const popularMakeSlugs = ['toyota', 'hyundai', 'kia', 'mercedes-benz', 'bmw', 'chevrolet', 'nissan', 'lada', 'ford', 'byd']

export default function HomePage() {
  const { locale, t } = useLocale()
  const { makes } = useCatalogMakes(locale)
  const [make, setMake] = useState('')
  const [model, setModel] = useState('')
  const [city, setCity] = useState('')
  const [maxPrice, setMaxPrice] = useState('')
  const [latest, setLatest] = useState<{ locale: string; items: ListingSummary[]; total: number } | null>(null)
  const [latestError, setLatestError] = useState(false)
  const selectedMake = makes.find(item => item.slug === make)
  const popular = popularMakeSlugs.map(slug => makes.find(item => item.slug === slug)).filter(item => item !== undefined)

  useEffect(() => {
    const controller = new AbortController()
    getJson<{ items: ListingSummary[]; total: number }>(
      `/api/v1/catalog/listings?locale=${locale}&pageSize=8&sort=newest`, controller.signal)
      .then(result => { setLatest({ locale, ...result }); setLatestError(false) })
      .catch(() => { if (!controller.signal.aborted) setLatestError(true) })
    return () => controller.abort()
  }, [locale])

  function search(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const params = new URLSearchParams()
    if (city) params.set('city', city)
    if (make) params.set('make', make)
    if (make && model) params.set('model', model)
    if (maxPrice) params.set('maxPrice', maxPrice)
    navigate('/cars' + (params.size ? '?' + params.toString() : ''))
  }

  return <SiteLayout>
    <section className="home-hero">
      <div className="home-hero-copy">
        <p className="home-badge">{t('heroBadge')}</p>
        <h1>{t('marketTitle')}</h1>
        <p>{t('marketIntro')}</p>
      </div>
      <form className="home-search" onSubmit={search} aria-label={t('filters')}>
        <label><span>{t('make')}</span>
          <select value={make} onChange={event => { setMake(event.target.value); setModel('') }}>
            <option value="">{t('allMakes')}</option>
            {makes.map(item => <option key={item.slug} value={item.slug}>{item.name}</option>)}
          </select>
        </label>
        <label><span>{t('model')}</span>
          <select value={model} onChange={event => setModel(event.target.value)} disabled={!selectedMake}>
            <option value="">{t('allModels')}</option>
            {selectedMake?.models.map(item => <option key={item.slug} value={item.slug}>{item.name}</option>)}
          </select>
        </label>
        <label><span>{t('searchCity')}</span>
          <select value={city} onChange={event => setCity(event.target.value)}>
            <option value="">{t('allCities')}</option>
            {cities.map(value => <option key={value} value={value}>{t(cityKey(value))}</option>)}
          </select>
        </label>
        <label><span>{t('searchPrice')}</span>
          <input type="number" inputMode="numeric" min="1" max="99999" step="1" value={maxPrice}
            onChange={event => setMaxPrice(event.target.value)} placeholder="AZN" />
        </label>
        <button type="submit" className="btn btn-primary">{t('searchButton')}</button>
      </form>
      {popular.length > 0 && <div className="home-makes" aria-label={t('popularMakes')}>
        <span>{t('popularMakes')}:</span>
        {popular.map(item => <Link key={item.slug} href={`/cars?make=${item.slug}`}>{item.name}</Link>)}
      </div>}
    </section>

    <ul className="home-stats">
      <li><b aria-hidden="true">✓</b>{t('statVerified')}</li>
      <li><b aria-hidden="true">₼</b>{t('statDirect')}</li>
      <li><b aria-hidden="true">⌖</b>{t('statLocal')}</li>
    </ul>

    <section className="home-section" aria-labelledby="latest-title">
      <div className="section-head">
        <h2 id="latest-title">{t('latestListings')}</h2>
        <Link className="section-link" href="/cars">{t('browseCars')} →</Link>
      </div>
      {latestError && <p className="notice notice-error" role="alert">{t('catalogError')}</p>}
      {!latest && !latestError && <div className="vehicle-grid" aria-busy="true">
        {Array.from({ length: 4 }, (_, index) => <div key={index} className="vehicle-card skeleton" />)}
      </div>}
      {latest?.items.length === 0 && <div className="empty-state">
        <h3>{t('noListings')}</h3>
        <p>{t('ownerCtaText')}</p>
        <Link className="btn btn-primary" href="/owner/listings/new">{t('ownerCtaButton')}</Link>
      </div>}
      {latest && latest.items.length > 0 && <div className="vehicle-grid">
        {latest.items.map(item => <VehicleCard key={item.id} item={item} />)}
      </div>}
    </section>

    <section className="home-section how" id="how-it-works" aria-labelledby="how-title">
      <h2 id="how-title">{t('howTitle')}</h2>
      <ol className="how-steps">
        {[1, 2, 3].map(step => <li key={step}>
          <span className="how-number" aria-hidden="true">{step}</span>
          <h3>{t(`how${step}Title`)}</h3>
          <p>{t(`how${step}Text`)}</p>
        </li>)}
      </ol>
    </section>

    <section className="owner-cta" aria-labelledby="owner-cta-title">
      <div>
        <h2 id="owner-cta-title">{t('ownerCtaTitle')}</h2>
        <p>{t('ownerCtaText')}</p>
      </div>
      <Link className="btn btn-light" href="/owner/listings/new">{t('ownerCtaButton')}</Link>
    </section>
  </SiteLayout>
}
