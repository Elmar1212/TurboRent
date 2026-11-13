import { retryAfterSeconds } from '../features/identity/retryAfter'

/** An API failure carrying the server's machine-readable code. */
export class ApiError extends Error {
  code: string
  fields: Record<string, string[]>
  retryAfterSeconds: number | null
  status: number
  constructor(code: string, fields: Record<string, string[]> = {}, retry: number | null = null, status = 0) {
    super(code)
    this.code = code
    this.fields = fields
    this.retryAfterSeconds = retry
    this.status = status
  }
}

async function failure(response: Response): Promise<ApiError> {
  const result = await response.json().catch(() => ({}))
  const code = result.code ?? (response.status === 429 ? 'rate_limited' : response.status === 404 ? 'notFound' : 'generic')
  return new ApiError(code, result.errors ?? {},
    response.status === 429 ? retryAfterSeconds(response.headers.get('Retry-After')) : null, response.status)
}

/** GET a JSON resource with the session cookie. */
export async function getJson<T>(path: string, signal?: AbortSignal): Promise<T> {
  let response: Response
  try {
    response = await fetch(path, { credentials: 'same-origin', cache: 'no-store', signal })
  } catch (error) {
    if (signal?.aborted) throw error
    throw new ApiError('network')
  }
  if (!response.ok) throw await failure(response)
  return await response.json() as T
}

/** Sends a CSRF-protected write. Bodies are JSON unless a FormData is given. */
export async function send<T = Record<string, unknown>>(method: 'POST' | 'PUT' | 'DELETE', path: string,
  body?: unknown): Promise<T> {
  if (location.protocol !== 'https:') throw new ApiError('secure')
  let response: Response
  try {
    const csrf = await fetch('/api/v1/auth/csrf', { credentials: 'same-origin', cache: 'no-store' })
    if (!csrf.ok) throw new ApiError('invalid_csrf')
    const { requestToken } = await csrf.json()
    const multipart = body instanceof FormData
    response = await fetch(path, {
      method, credentials: 'same-origin', cache: 'no-store',
      headers: multipart ? { 'X-CSRF-TOKEN': requestToken }
        : { 'Content-Type': 'application/json', 'X-CSRF-TOKEN': requestToken },
      body: multipart ? body : method === 'DELETE' && body === undefined ? undefined : JSON.stringify(body ?? {}),
    })
  } catch (error) {
    if (error instanceof ApiError) throw error
    throw new ApiError('network')
  }
  if (!response.ok) throw await failure(response)
  if (response.status === 204) return {} as T
  return await response.json().catch(() => ({})) as T
}
