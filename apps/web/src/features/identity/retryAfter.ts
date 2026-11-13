export function retryAfterSeconds(value: string | null, now = Date.now()): number | null {
  if (!value) return null
  if (/^[0-9]+$/.test(value)) {
    const seconds = Number(value)
    return Number.isSafeInteger(seconds) ? seconds : null
  }
  const date = Date.parse(value)
  return Number.isFinite(date) ? Math.max(0, Math.ceil((date - now) / 1000)) : null
}
