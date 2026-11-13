export type VerificationLink = { userId: string; token: string }
const guid = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i
const base64Url = /^[A-Za-z0-9_-]+$/

export function parseVerificationFragment(hash: string): VerificationLink | null {
  let fragment = hash.startsWith('#') ? hash.slice(1) : hash
  // Only accept the exact historical JSON separator in its known position.
  const legacy = fragment.match(/^userId=([0-9a-fA-F-]{36})\\u0026token=([A-Za-z0-9_-]{1,4096})$/)
  if (legacy) fragment = 'userId=' + legacy[1] + '&token=' + legacy[2]
  // These fields use only URL-safe ASCII. Reject percent-decoding, plus/space conversion,
  // duplicate keys and unknown fields instead of silently changing a token.
  if (fragment.includes('%') || fragment.includes('+')) return null
  const params = new URLSearchParams(fragment)
  if ([...params.keys()].length !== 2 || params.getAll('userId').length !== 1 ||
      params.getAll('token').length !== 1) return null
  const userId = params.get('userId') ?? ''
  const token = params.get('token') ?? ''
  if (!guid.test(userId) || userId === '00000000-0000-0000-0000-000000000000' ||
      token.length < 1 || token.length > 4096 || token.length % 4 === 1 || !base64Url.test(token)) return null
  return { userId, token }
}
