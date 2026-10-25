import { useEffect, useState } from 'react'
import './App.css'

type Status = 'loading' | 'healthy' | 'unhealthy'
export default function App() {
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
  const labels = { loading: 'Yoxlanılır…', healthy: 'İşləyir', unhealthy: 'Əlçatan deyil' }
  return (
    <main>
      <p className="eyebrow">TURBORENT · FOUNDATION</p>
      <h1>Yolumuz buradan başlayır.</h1>
      <p>Azərbaycan üçün avtomobil icarəsi platformasının texniki təməli.</p>
      <section aria-label="Sistem vəziyyəti" aria-live="polite">
        <h2>Sistem vəziyyəti</h2>
        <p>API: <strong data-testid="api-status">{labels[api]}</strong></p>
        <p>PostgreSQL: <strong data-testid="database-status">{labels[database]}</strong></p>
      </section>
      <footer>AZN · Asia/Baku · Milestone 1</footer>
    </main>
  )
}
