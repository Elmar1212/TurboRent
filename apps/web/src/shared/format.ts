export const cities = ['baku', 'ganja', 'sumgait', 'lankaran', 'shaki', 'gabala'] as const
export const transmissions = ['automatic', 'manual'] as const
export const fuelTypes = ['petrol', 'diesel', 'hybrid', 'electric'] as const

export const cityKey = (city: string) => 'city' + city[0].toUpperCase() + city.slice(1)

const intlLocale = (locale: string) => locale === 'az' ? 'az-AZ' : 'en-GB'

export function formatPrice(value: number, locale: string) {
  return new Intl.NumberFormat(intlLocale(locale), { maximumFractionDigits: 2 }).format(value)
}

/** "today 14:05" for today, otherwise a short date. */
export function formatListedDate(iso: string, locale: string, todayLabel: string) {
  const date = new Date(iso)
  const now = new Date()
  if (date.toDateString() === now.toDateString())
    return `${todayLabel}, ${date.toLocaleTimeString(intlLocale(locale), { hour: '2-digit', minute: '2-digit' })}`
  return date.toLocaleDateString(intlLocale(locale), { day: '2-digit', month: '2-digit', year: 'numeric' })
}

/** Formats a yyyy-MM-dd calendar date without shifting it through the browser time zone. */
export function formatDay(day: string, locale: string) {
  const [year, month, date] = day.split('-').map(Number)
  return new Date(year, month - 1, date).toLocaleDateString(intlLocale(locale),
    { day: 'numeric', month: 'short', year: 'numeric' })
}

/** Today's date in Baku (UTC+4) as yyyy-MM-dd, matching the server's booking calendar. */
export function bakuToday() {
  return new Date(Date.now() + 4 * 3600_000).toISOString().slice(0, 10)
}

export function addDays(day: string, count: number) {
  const date = new Date(day + 'T00:00:00Z')
  date.setUTCDate(date.getUTCDate() + count)
  return date.toISOString().slice(0, 10)
}

export function daysBetweenInclusive(start: string, end: string) {
  return Math.round((Date.parse(end + 'T00:00:00Z') - Date.parse(start + 'T00:00:00Z')) / 86_400_000) + 1
}
