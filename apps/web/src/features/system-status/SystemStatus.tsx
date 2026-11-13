import { useEffect, useState } from 'react'
import { useLocale } from '../../shared/i18n/messages'

type Status = 'loading' | 'healthy' | 'unhealthy'
const labels = {
  az: { loading: 'Yoxlanılır…', healthy: 'İşləyir', unhealthy: 'Əlçatan deyil' },
  en: { loading: 'Checking…', healthy: 'Working', unhealthy: 'Unavailable' },
}

/** Compact API and database health indicator shown in the footer. */
export default function SystemStatus() {
  const { locale, t } = useLocale()
  const [api, setApi] = useState<Status>('loading')
  const [database, setDatabase] = useState<Status>('loading')
  useEffect(() => {
    const controller = new AbortController()
    async function check(path: string, update: (status: Status) => void) {
      try {
        const response = await fetch(path, { signal: controller.signal })
        const body = await response.json()
        update(response.ok && body.status === 'healthy' ? 'healthy' : 'unhealthy')
      } catch {
        if (!controller.signal.aborted) update('unhealthy')
      }
    }
    void check('/api/v1/health/live', setApi)
    void check('/api/v1/health/ready', setDatabase)
    return () => controller.abort()
  }, [])
  const text = labels[locale]
  return <section className="system-status" aria-label={t('systemStatus')} aria-live="polite">
    <h2>{t('systemStatus')}</h2>
    <p><i className={'dot ' + api} aria-hidden="true" />API: <strong data-testid="api-status">{text[api]}</strong></p>
    <p><i className={'dot ' + database} aria-hidden="true" />PostgreSQL: <strong data-testid="database-status">{text[database]}</strong></p>
  </section>
}
