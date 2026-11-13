import { test } from 'node:test'
import assert from 'node:assert/strict'
import { parseVerificationFragment } from '../src/features/identity/verificationLink.ts'
import { retryAfterSeconds } from '../src/features/identity/retryAfter.ts'

const id = '11111111-1111-4111-8111-111111111111'
const token = 'YWJjZA'
test('normal fragment preserves GUID and token exactly', () => {
  assert.deepEqual(parseVerificationFragment('#userId=' + id + '&token=' + token), { userId: id, token })
})
test('only the exact historical JSON separator is normalized', () => {
  assert.deepEqual(parseVerificationFragment('#userId=' + id + String.raw`\u0026token=` + token), { userId: id, token })
})
test('reversed ordinary parameters are accepted without changing bytes', () => {
  assert.deepEqual(parseVerificationFragment('#token=' + token + '&userId=' + id), { userId: id, token })
})
for (const [name, hash] of [
  ['missing token', '#userId=' + id],
  ['invalid GUID', '#userId=invalid&token=' + token],
  ['empty GUID', '#userId=00000000-0000-0000-0000-000000000000&token=' + token],
  ['padded token', '#userId=' + id + '&token=' + token + '=='],
  ['plus token', '#userId=' + id + '&token=YWJ+'],
  ['percent encoding', '#userId=' + id + '&token=%59WJjZA'],
  ['duplicate token', '#userId=' + id + '&token=' + token + '&token=' + token],
  ['duplicate GUID', '#userId=' + id + '&userId=' + id + '&token=' + token],
  ['unknown parameter', '#userId=' + id + '&token=' + token + '&role=Admin'],
  ['bad base64 length', '#userId=' + id + '&token=A'],
  ['oversized token', '#userId=' + id + '&token=' + 'A'.repeat(4097)],
  ['wrong escape case', '#userId=' + id + (String.fromCharCode(92) + 'U0026token=') + token],
  ['wrong token key case', '#userId=' + id + (String.fromCharCode(92) + 'u0026TOKEN=') + token],
  ['other JSON escape', '#userId=' + id + String.raw`\u0026token\u003d` + token],
  ['escape within token', '#userId=' + id + '&token=YWJ' + String.raw`\u0026token=` + token],
]) test(name + ' is rejected', () => assert.equal(parseVerificationFragment(hash), null))

test('Retry-After delta seconds', () => assert.equal(retryAfterSeconds('899'), 899))
test('Retry-After HTTP date', () => assert.equal(retryAfterSeconds('Thu, 01 Jan 2026 00:00:05 GMT', Date.parse('2026-01-01T00:00:00Z')), 5))
test('past HTTP date does not create a negative countdown', () => assert.equal(retryAfterSeconds('Thu, 01 Jan 2026 00:00:00 GMT', Date.parse('2026-01-01T00:01:00Z')), 0))
test('missing and malformed Retry-After are ignored', () => {
  assert.equal(retryAfterSeconds(null), null)
  assert.equal(retryAfterSeconds('nonsense'), null)
  assert.equal(retryAfterSeconds('999999999999999999999999'), null)
})
