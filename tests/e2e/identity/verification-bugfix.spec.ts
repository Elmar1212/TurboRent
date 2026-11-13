import { test, expect } from '../../../apps/web/node_modules/@playwright/test'
import type { Page } from '../../../apps/web/node_modules/@playwright/test'
import { readdir, readFile } from 'node:fs/promises'
import { join, resolve } from 'node:path'
import { spawn } from 'node:child_process'
import type { ChildProcess } from 'node:child_process'

async function register(page: Page, email: string) {
  await page.goto('/register')
  await page.getByRole('button', { name: 'EN', exact: true }).click()
  await page.getByLabel('Full name', { exact: true }).fill('Safe Regression Test')
  await page.getByLabel('Email address', { exact: true }).fill(email)
  await page.getByLabel('Password', { exact: true }).fill('Safe regression passphrase 2026')
  await page.getByLabel('Confirm password', { exact: true }).fill('Safe regression passphrase 2026')
  await page.getByRole('button', { name: 'Create account', exact: true }).click()
  await expect(page.getByRole('heading', { name: 'Check your email', exact: true })).toBeVisible()
}
async function mail(email: string) {
  const root = process.env.TURBORENT_E2E_MAILBOX!
  for (const file of await readdir(root)) {
    if (!file.endsWith('.json')) continue
    const raw = await readFile(join(root, file), 'utf8')
    const message = JSON.parse(raw)
    if (message.email === email) {
      const match = raw.match(/"verificationUrl"\s*:\s*"((?:\\.|[^"\\])*)"/)
      if (!match) throw new Error('Expected mailbox metadata missing.')
      const plain = await readFile(join(root, message.id + '.txt'), 'utf8')
      const textLink = plain.split(/\r?\n/).find(line => line.startsWith('https://'))!
      // Boolean assertions prevent reporter output from disclosing real token values.
      expect(textLink === message.verificationUrl).toBe(true)
      return { decoded: message.verificationUrl as string, raw: match[1], text: textLink }
    }
  }
  throw new Error('Expected local regression message missing.')
}
async function navigate(page: Page, url: string) {
  try { await page.goto(url) } catch { throw new Error('Verification navigation failed; sensitive URL suppressed.') }
  await expect(page.getByRole('button', { name: 'Verify email', exact: true })).toBeVisible()
  await expect.poll(() => page.evaluate(() => location.hash.length === 0)).toBe(true)
}
test('raw JSON copy and TXT link both reach real verification', async ({ page }) => {
  for (const kind of ['raw', 'text'] as const) {
    const email = 'bugfix-' + kind + '-' + Date.now() + '@example.test'
    await register(page, email)
    const message = await mail(email)
    if (kind === 'raw') expect(message.raw.includes(String.raw`\u0026token=`)).toBe(true)
    await navigate(page, message[kind])
    await page.getByRole('button', { name: 'Verify email', exact: true }).click()
    await expect(page.getByRole('heading', { name: 'Email verified', exact: true })).toBeVisible()
  }
})
test('same-tab hashchange and popstate replace a stale link without reload', async ({ page }) => {
  const email = 'bugfix-navigation-' + Date.now() + '@example.test'
  await register(page, email)
  const message = await mail(email)
  await page.goto('/verify-email')
  await expect(page.getByRole('button', { name: 'Verify email', exact: true })).toHaveCount(0)
  await page.evaluate(() => { location.hash = 'userId=not-a-guid&token=YWJjZA' })
  await expect(page.getByRole('button', { name: 'Verify email', exact: true })).toHaveCount(0)
  const fragment = new URL(message.raw).hash
  await page.evaluate(value => { location.hash = value }, fragment)
  await expect(page.getByRole('button', { name: 'Verify email', exact: true })).toBeVisible()
  await expect.poll(() => page.evaluate(() => location.hash === '')).toBe(true)
  // Simulate a same-document history navigation, whose popstate listener must also capture.
  const decodedFragment = new URL(message.decoded).hash
  await page.evaluate(value => {
    history.pushState(null, '', '/verify-email' + value)
    window.dispatchEvent(new PopStateEvent('popstate'))
  }, decodedFragment)
  await expect(page.getByRole('button', { name: 'Verify email', exact: true })).toBeVisible()
  await expect.poll(() => page.evaluate(() => location.hash === '')).toBe(true)
  await page.getByRole('button', { name: 'Verify email', exact: true }).click()
  await expect(page.getByRole('heading', { name: 'Email verified', exact: true })).toBeVisible()
})
for (const location of ['verify', 'check-email']) {
  test('429 countdown prevents resend until Retry-After on ' + location, async ({ page }) => {
    if (location === 'check-email') await register(page, 'bugfix-countdown-' + Date.now() + '@example.test')
    else {
      await page.goto('/verify-email')
      await page.getByRole('button', { name: 'EN', exact: true }).click()
      await page.getByLabel('Email address', { exact: true }).fill('countdown@example.test')
    }
    await page.clock.install()
    let sends = 0
    await page.route('**/api/v1/auth/email/resend', async route => {
      sends++
      await route.fulfill({ status: 429, headers: { 'Content-Type': 'application/problem+json', 'Retry-After': '5' },
        body: JSON.stringify({ code: 'rate_limited' }) })
    })
    const resend = page.getByRole('button', { name: 'Resend verification email', exact: true })
    await resend.click()
    await expect(resend).toBeDisabled()
    await expect(page.getByTestId('resend-countdown')).toContainText('5 seconds')
    await page.clock.fastForward(3000)
    await expect(page.getByTestId('resend-countdown')).toContainText('2 seconds')
    await expect(resend).toBeDisabled()
    expect(sends).toBe(1)
    await page.clock.fastForward(2000)
    await expect(resend).toBeEnabled()
    await expect(page.getByTestId('resend-countdown')).toHaveCount(0)
    expect(sends).toBe(1)
  })
}

async function stop(child: ChildProcess) {
  if (child.exitCode !== null || child.signalCode !== null) return
  await new Promise<void>(resolve => { child.once('exit', () => resolve()); child.kill() })
}
test('verification works in the browser after a real isolated API process restart', async ({ page, request, baseURL }) => {
  test.setTimeout(60000)
  const root = resolve(process.cwd(), '../..')
  const configuration = process.env.TURBORENT_E2E_CONFIGURATION === 'Debug' ? 'Debug' : 'Release'
  const api = join(root, 'src/backend/TurboRent.Api/bin', configuration, 'net10.0/TurboRent.Api.dll')
  const origin = 'https://localhost:7082'
  const env = {
    ...process.env,
    ASPNETCORE_ENVIRONMENT: 'Testing',
    ConnectionStrings__Database: process.env.TURBORENT_TEST_DATABASE!,
    ASPNETCORE_Kestrel__Certificates__Default__Path: join(root, 'artifacts/dev-certs/turborent.pem'),
    ASPNETCORE_Kestrel__Certificates__Default__KeyPath: join(root, 'artifacts/dev-certs/turborent.key'),
    Identity__FrontendOrigin: baseURL!,
    Identity__MailboxPath: process.env.TURBORENT_E2E_MAILBOX!,
    Identity__IpPermitLimit: '1000',
  }
  let child: ChildProcess | undefined
  async function start() {
    child = spawn('dotnet', [api, '--urls', origin], { env, stdio: 'ignore', windowsHide: true })
    await expect.poll(async () => {
      if (child?.exitCode !== null) return false
      try { return (await request.get(origin + '/api/v1/health/ready')).ok() } catch { return false }
    }, { timeout: 15000 }).toBe(true)
  }
  await page.route('**/api/v1/auth/**', async route => {
    try {
      const response = await route.fetch({ url: origin + new URL(route.request().url()).pathname })
      await route.fulfill({ response })
    } catch { throw new Error('Isolated restart API request failed; sensitive details suppressed.') }
  })
  try {
    // Refuse an occupied port instead of contacting or stopping any existing server.
    let occupied = false
    try { occupied = (await request.get(origin + '/api/v1/health/live')).ok() } catch { /* no listener */ }
    if (occupied) throw new Error('Isolated restart port is already in use.')
    await start()
    const email = 'bugfix-restart-' + Date.now() + '@example.test'
    await register(page, email)
    const message = await mail(email)
    await stop(child!)
    await start()
    await navigate(page, message.text)
    await page.getByRole('button', { name: 'Verify email', exact: true }).click()
    await expect(page.getByRole('heading', { name: 'Email verified', exact: true })).toBeVisible()
  } finally { if (child) await stop(child) }
})
