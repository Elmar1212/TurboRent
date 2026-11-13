import { useEffect, useState } from 'react'

export function useResendCooldown() {
  const [deadline, setDeadline] = useState(0)
  const [now, setNow] = useState(Date.now)
  useEffect(() => {
    if (!deadline) return
    const tick = () => {
      const current = Date.now()
      setNow(current)
      if (current >= deadline) setDeadline(0)
    }
    const timer = setInterval(tick, 250)
    document.addEventListener('visibilitychange', tick)
    return () => { clearInterval(timer); document.removeEventListener('visibilitychange', tick) }
  }, [deadline])
  function start(seconds: number | null) {
    if (seconds === null || !Number.isFinite(seconds) || seconds <= 0) return
    const current = Date.now()
    setNow(current)
    setDeadline(current + seconds * 1000)
  }
  return { remaining: Math.max(0, Math.ceil((deadline - now) / 1000)), start }
}
