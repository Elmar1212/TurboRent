import { useEffect, useState } from 'react'
import { getJson } from '../../shared/api'

export type VehicleModel = { id: number; slug: string; name: string }
export type VehicleMake = { slug: string; name: string; models: VehicleModel[] }

/** Loads makes and models for the current language; the response is cached by the browser. */
export function useCatalogMakes(locale: string) {
  const [result, setResult] = useState<{ locale: string; makes: VehicleMake[]; error: boolean } | null>(null)
  useEffect(() => {
    const controller = new AbortController()
    getJson<VehicleMake[]>('/api/v1/catalog/makes?locale=' + locale, controller.signal)
      .then(makes => setResult({ locale, makes, error: false }))
      .catch(() => { if (!controller.signal.aborted) setResult({ locale, makes: [], error: true }) })
    return () => controller.abort()
  }, [locale])
  const current = result?.locale === locale ? result : null
  return { makes: current?.makes ?? [], loading: current === null, error: current?.error ?? false }
}
