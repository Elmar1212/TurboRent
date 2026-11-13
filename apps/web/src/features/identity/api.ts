import { ApiError, send } from '../../shared/api'

export { ApiError }

/** POST to an /api/v1/auth endpoint. */
export function submit(path: string, body: unknown) {
  return send('POST', '/api/v1/auth/' + path, body)
}

export function submitTo(path: string, body: unknown) {
  return send('POST', path, body)
}

export function submitMultipart(path: string, body: FormData) {
  return send('POST', path, body)
}
