import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { getJson } from '../../shared/api'
import { cities, cityKey, fuelTypes, transmissions } from '../../shared/format'
import { useLocale } from '../../shared/i18n/messages'
import { SiteLayout } from '../../shared/layout/SiteLayout'
import { navigate, useLocation } from '../../shared/router'
import { useCatalogMakes } from './useCatalogMakes'
import type { VehicleMake } from './useCatalogMakes'
import { VehicleCard } from './VehicleCard'
import type { ListingSummary } from './VehicleCard'
import './marketplace.css'

type ListingPage = { items: ListingSummary[]; total: number; page: number; pageSize: number }
const filterKeys = ['city', 'make', 'model', 'minPrice', 'maxPrice', 'minYear', 'maxYear', 'transmission', 'fuelType', 'seats'] as const
type FilterKey = typeof filterKeys[number]
type Filters = Record<FilterKey, string>
const sorts = ['newest', 'price_asc', 'price_desc', 'year_desc'] as const
const pageSize = 16

function readFilters(params: URLSearchParams): Filters {
  return Object.fromEntries(filterKeys.map(key => [key, params.get(key) ?? ''])) as Filters
}

function catalogUrl(filters: Filters, sort: string, page: number) {
  const params = new URLSearchParams()
  for (const key of filterKeys) if (filters[key]) params.set(key, filters[key])
  if (sort !== 'newest') params.set('sort', sort)
  if (page > 1) params.set('page', String(page))
  return '/cars' + (params.size ? '?' + params.toString() : '')
}

function FilterPanel({ initial, makes, sort, onApply }: {
  initial: Filters; makes: VehicleMake[]; sort: string; onApply: (filters: Filters) => void
}) {
  const { t } = useLocale()
  const [filters, setFilters] = useState(initial)
  const [expanded, setExpanded] = useState(() =>
    !!(initial.minYear || initial.maxYear || initial.transmission || initial.fuelType || initial.seats))
  const selectedMake = makes.find(make => make.slug === filters.make)
  const update = (key: FilterKey, value: string) => setFilters(current => ({ ...current, [key]: value }))

  function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    onApply(filters)
  }

  return <form className="filter-panel" onSubmit={submit} aria-label={t('filters')}>
    <div className="filter-grid">
      <label className="field"><span>{t('searchCity')}</span>
        <select value={filters.city} onChange={event => update('city', event.target.value)}>
          <option value="">{t('allCities')}</option>
          {cities.map(city => <option key={city} value={city}>{t(cityKey(city))}</option>)}
        </select>
      </label>
      <label className="field"><span>{t('make')}</span>
        <select value={filters.make} onChange={event => setFilters(current => ({ ...current, make: event.target.value, model: '' }))}>
          <option value="">{t('allMakes')}</option>
          {makes.map(make => <option key={make.slug} value={make.slug}>{make.name}</option>)}
        </select>
      </label>
      <label className="field"><span>{t('model')}</span>
        <select value={filters.model} onChange={event => update('model', event.target.value)} disabled={!selectedMake}>
          <option value="">{t('allModels')}</option>
          {selectedMake?.models.map(model => <option key={model.slug} value={model.slug}>{model.name}</option>)}
        </select>
      </label>
      <div className="field"><span>{t('searchPrice')}</span>
        <div className="field-range">
          <input aria-label={t('minPrice')} type="number" inputMode="numeric" min="1" max="99999"
            placeholder="min" value={filters.minPrice} onChange={event => update('minPrice', event.target.value)} />
          <input aria-label={t('maxPrice')} type="number" inputMode="numeric" min="1" max="99999"
            placeholder="max" value={filters.maxPrice} onChange={event => update('maxPrice', event.target.value)} />
        </div>
      </div>
      {expanded && <>
        <div className="field"><span>{t('minYear')} / {t('maxYear')}</span>
          <div className="field-range">
            <input aria-label={t('minYear')} type="number" inputMode="numeric" min="1980" max="2035"
              placeholder="1980" value={filters.minYear} onChange={event => update('minYear', event.target.value)} />
            <input aria-label={t('maxYear')} type="number" inputMode="numeric" min="1980" max="2035"
              placeholder="2035" value={filters.maxYear} onChange={event => update('maxYear', event.target.value)} />
          </div>
        </div>
        <label className="field"><span>{t('transmission')}</span>
          <select value={filters.transmission} onChange={event => update('transmission', event.target.value)}>
            <option value="">{t('allTransmissions')}</option>
            {transmissions.map(item => <option key={item} value={item}>{t(item)}</option>)}
          </select>
        </label>
        <label className="field"><span>{t('fuelType')}</span>
          <select value={filters.fuelType} onChange={event => update('fuelType', event.target.value)}>
            <option value="">{t('allFuelTypes')}</option>
            {fuelTypes.map(item => <option key={item} value={item}>{t(item)}</option>)}
          </select>
        </label>
        <label className="field"><span>{t('seats')}</span>
          <select value={filters.seats} onChange={event => update('seats', event.target.value)}>
            <option value="">—</option>
            {[2, 4, 5, 6, 7, 8, 9].map(item => <option key={item} value={item}>{item}+</option>)}
          </select>
        </label>
      </>}
    </div>
    <div className="filter-actions">
      <button type="button" className="btn btn-link" aria-expanded={expanded} onClick={() => setExpanded(value => !value)}>
        {expanded ? t('fewerFilters') : t('moreFilters')}
      </button>
      <span className="nav-spacer" />
      <button type="button" className="btn btn-ghost" onClick={() => { setFilters(readFilters(new URLSearchParams())); navigate(catalogUrl(readFilters(new URLSearchParams()), sort, 1), { keepScroll: true }) }}>
        {t('clearFilters')}
      </button>
      <button type="submit" className="btn btn-primary">{t('applyFilters')}</button>
    </div>
  </form>
}

function Pagination({ page, pageCount, onChange }: { page: number; pageCount: number; onChange: (page: number) => void }) {
  const pages = Array.from({ length: pageCount }, (_, index) => index + 1)
    .filter(item => item === 1 || item === pageCount || Math.abs(item - page) <= 2)
  return <nav className="pagination" aria-label="Pagination">
    <button type="button" disabled={page <= 1} onClick={() => onChange(page - 1)} aria-label="Previous page">‹</button>
    {pages.map((item, index) => <span key={item} className="pagination-item">
      {index > 0 && item - pages[index - 1] > 1 && <span className="pagination-gap">…</span>}
      <button type="button" aria-current={item === page ? 'page' : undefined} onClick={() => onChange(item)}>{item}</button>
    </span>)}
    <button type="button" disabled={page >= pageCount} onClick={() => onChange(page + 1)} aria-label="Next page">›</button>
  </nav>
}

export default function CarCatalogPage() {
  const { locale, t } = useLocale()
  const { params, search } = useLocation()
  const { makes, error: makesError } = useCatalogMakes(locale)
  const applied = readFilters(params)
  const sort = sorts.includes(params.get('sort') as typeof sorts[number]) ? params.get('sort')! : 'newest'
  const page = Math.max(1, Number(params.get('page')) || 1)
  const apiQuery = (() => {
    const query = new URLSearchParams({ locale, page: String(page), pageSize: String(pageSize), sort })
    for (const key of filterKeys) if (applied[key]) query.set(key, applied[key])
    return query.toString()
  })()
  const [result, setResult] = useState<{ key: string; value: ListingPage | null; error: boolean } | null>(null)
  const [retry, setRetry] = useState(0)
  const current = result?.key === apiQuery ? result : null
  const listings = current?.value ?? null

  useEffect(() => {
    const controller = new AbortController()
    getJson<ListingPage>('/api/v1/catalog/listings?' + apiQuery, controller.signal)
      .then(value => setResult({ key: apiQuery, value, error: false }))
      .catch(() => { if (!controller.signal.aborted) setResult({ key: apiQuery, value: null, error: true }) })
    return () => controller.abort()
  }, [apiQuery, retry])

  const pageCount = listings ? Math.max(1, Math.ceil(listings.total / listings.pageSize)) : 1
  return <SiteLayout wide>
    <div className="page-head">
      <p className="breadcrumbs">TurboRent › {t('navCars')}</p>
      <h1>{t('catalogTitle')}</h1>
      <p className="page-intro">{t('catalogIntro')}</p>
    </div>
    <FilterPanel key={search} initial={applied} makes={makes} sort={sort}
      onApply={filters => navigate(catalogUrl(filters, sort, 1), { keepScroll: true })} />
    {makesError && <p className="notice notice-error" role="alert">{t('catalogError')}</p>}
    <section className="results" aria-live="polite" aria-busy={!current}>
      <div className="results-bar">
        <p className="results-count">{listings ? <><b>{listings.total}</b> {t('resultCount')}</> : t('loading')}</p>
        <label className="sort-select"><span>{t('sortLabel')}</span>
          <select value={sort} onChange={event => navigate(catalogUrl(applied, event.target.value, 1), { keepScroll: true })}>
            {sorts.map(item => <option key={item} value={item}>{t('sort_' + item)}</option>)}
          </select>
        </label>
      </div>
      {!current && <div className="vehicle-grid">{Array.from({ length: 8 }, (_, index) =>
        <div key={index} className="vehicle-card skeleton" />)}</div>}
      {current?.error && <div className="notice notice-error" role="alert">
        {t('catalogError')} <button type="button" className="btn btn-link" onClick={() => setRetry(value => value + 1)}>{t('errorTryAgain')}</button>
      </div>}
      {listings?.items.length === 0 && <div className="empty-state">
        <h2>{t('noListings')}</h2><p>{t('noListingsHint')}</p>
      </div>}
      {listings && listings.items.length > 0 && <>
        <div className="vehicle-grid">{listings.items.map(item => <VehicleCard key={item.id} item={item} />)}</div>
        {pageCount > 1 && <Pagination page={page} pageCount={pageCount}
          onChange={next => navigate(catalogUrl(applied, sort, next))} />}
      </>}
    </section>
  </SiteLayout>
}
