import { useLocale } from '../../shared/i18n/messages'
import { Link } from '../../shared/Link'
import { cityKey, formatListedDate, formatPrice } from '../../shared/format'

export type ListingSummary = {
  id: string
  make: string
  model: string
  year: number
  city: string
  dailyPrice: number
  transmission: string
  fuelType: string
  seats: number
  createdAtUtc: string
  images: string[]
}

export function VehiclePlaceholder({ make, model }: { make: string; model: string }) {
  return <div className="vehicle-placeholder" aria-hidden="true">
    <span>{(make.slice(0, 1) + model.slice(0, 1)).toUpperCase()}</span>
  </div>
}

export function VehicleCard({ item }: { item: ListingSummary }) {
  const { locale, t } = useLocale()
  const name = `${item.make} ${item.model}`
  return <article className="vehicle-card">
    <Link className="vehicle-card-link" href={`/cars/${item.id}`} aria-label={`${name}, ${item.year}`}>
      <div className="vehicle-card-media">
        {item.images?.[0]
          ? <img src={item.images[0]} alt={`${name} ${t('vehiclePhoto')}`} loading="lazy" decoding="async" />
          : <VehiclePlaceholder make={item.make} model={item.model} />}
        {(item.fuelType === 'electric' || item.fuelType === 'hybrid') &&
          <span className={'vehicle-badge ' + item.fuelType}>{t(item.fuelType)}</span>}
        {item.images?.length > 1 && <span className="vehicle-photo-count" aria-hidden="true">▣ {item.images.length}</span>}
      </div>
      <div className="vehicle-card-body">
        <p className="vehicle-card-price">{formatPrice(item.dailyPrice, locale)} <span>AZN / {t('perDay')}</span></p>
        <h3 className="vehicle-card-title">{name}</h3>
        <p className="vehicle-card-meta">{item.year}, {t(item.transmission)}, {t(item.fuelType)}, {item.seats} {t('seatsLabel')}</p>
        <p className="vehicle-card-foot">
          <span>{t(cityKey(item.city))}</span>
          <span>{item.createdAtUtc ? formatListedDate(item.createdAtUtc, locale, t('today')) : ''}</span>
        </p>
      </div>
    </Link>
  </article>
}
