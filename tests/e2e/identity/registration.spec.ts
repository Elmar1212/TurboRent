import { test, expect } from '../../../apps/web/node_modules/@playwright/test'
import { readdir, readFile } from 'node:fs/promises'
import { join } from 'node:path'

test('public car catalog filters and reports when no approved cars exist', async ({ page }) => {
  await page.goto('/')
  await page.getByRole('link', { name: 'Bütün avtomobillərə bax' }).click()
  await expect(page.getByRole('heading', { name: 'İcarə avtomobilləri' })).toBeVisible()
  await page.getByRole('combobox').nth(0).selectOption('baku')
  await page.getByRole('combobox').nth(1).selectOption('toyota')
  await page.getByLabel('Maks. AZN / gün').fill('1')
  await page.getByRole('button', { name: 'Nəticələri göstər' }).click()
  await expect(page.getByRole('heading', { name: 'Bu filtrlərlə uyğun təsdiqlənmiş elan yoxdur.' })).toBeVisible()
  await expect(page).toHaveURL(/city=baku/)
  await page.getByRole('button', { name: 'EN', exact: true }).click()
  await expect(page.getByRole('heading', { name: 'No approved listings match these filters.' })).toBeVisible()
})

async function verificationUrl(email: string) {
  const root = process.env.TURBORENT_E2E_MAILBOX!
  for (const file of await readdir(root)) {
    if (!file.endsWith('.json')) continue
    const message = JSON.parse(await readFile(join(root, file), 'utf8'))
    if (message.email === email) return message.verificationUrl as string
  }
  throw new Error('Expected local development message was not delivered.')
}
for (const kind of ['customer', 'owner', 'company']) {
  test('register and verify ' + kind, async ({ page }) => {
    const email = 'e2e-' + kind + '-' + Date.now() + '@example.test'
    await page.goto('/register')
    await page.getByRole('button', { name: 'EN', exact: true }).click()
    const labels = { customer: 'Customer', owner: 'Individual owner', company: 'Rental company' }
    await page.getByRole('radio', { name: labels[kind as keyof typeof labels], exact: true }).check()
    await page.getByLabel('Full name', { exact: true }).fill('Safe Test Person')
    await page.getByLabel('Email address', { exact: true }).fill(email)
    if (kind !== 'customer') await page.getByLabel('Contact number', { exact: true }).fill('+994501234567')
    if (kind === 'company') {
      await page.getByLabel('Company legal name').fill('Example E2E Company')
      await page.getByLabel('Tax number (VOEN)').fill('1234567890')
    }
    await page.getByLabel('Password', { exact: true }).fill('Safe test passphrase 2026')
    await page.getByLabel('Confirm password', { exact: true }).fill('Safe test passphrase 2026')
    if (kind === 'company') await page.screenshot({ path: join(process.env.TURBORENT_E2E_MAILBOX!, '..', 'register-desktop.png'), fullPage: true })
    await page.getByRole('button', { name: 'Create account', exact: true }).click()
    await expect(page.getByRole('heading', { name: 'Check your email' })).toBeVisible()
    await expect(page.getByText(email, { exact: false })).toBeVisible()
    await expect(page.getByLabel('Email address')).toHaveCount(0)
    const url = await verificationUrl(email)
    try { await page.goto(url) } catch { throw new Error('Verification navigation failed; sensitive URL suppressed.') }
    await expect.poll(() => page.evaluate(() => location.pathname === '/verify-email' && location.hash === '')).toBe(true)
    await expect(page.getByLabel('Email address')).toHaveCount(0)
    await page.getByRole('button', { name: 'Verify email', exact: true }).click()
    await expect(page.getByRole('heading', { name: 'Email verified' })).toBeVisible()
    await page.getByRole('link', { name: 'Sign in', exact: true }).click()
    await page.getByLabel('Email address', { exact: true }).fill(email)
    await page.getByLabel('Password', { exact: true }).fill('Safe test passphrase 2026')
    await page.getByRole('button', { name: 'Sign in', exact: true }).click()
    if (kind === 'customer') {
      await expect(page.getByText('Signed in as:', { exact: false })).toContainText('Safe Test Person')
      await page.getByRole('button', { name: 'Sign out', exact: true }).click()
      await expect(page.getByRole('link', { name: 'Sign in', exact: true })).toBeVisible()
    } else {
      await expect(page).toHaveURL(/\/owner\/listings\/new$/)
      await expect(page.getByRole('heading', { name: 'Create a vehicle listing' })).toBeVisible()
      await expect(page.getByText('Vehicle photos', { exact: true })).toBeVisible()
    }
  })
}
test('Azerbaijani mobile validation and invalid verification recovery', async ({ page }) => {
  await page.setViewportSize({ width: 390, height: 844 })
  await page.goto('/register')
  await page.getByRole('button', { name: 'AZ', exact: true }).click()
  await page.getByRole('button', { name: 'Hesab yarat', exact: true }).click()
  await expect(page.getByText('Ad və soyad 1–100 simvol olmalıdır.')).toBeVisible()
  await expect(page.locator('#displayName')).toBeFocused()
  await expect(page.locator('html')).toHaveAttribute('lang', 'az')
  await expect(page.locator('body')).toHaveJSProperty('scrollWidth', 390)
  await page.screenshot({ path: join(process.env.TURBORENT_E2E_MAILBOX!, '..', 'register-mobile.png'), fullPage: true })
  await page.goto('/verify-email#userId=00000000-0000-0000-0000-000000000001&token=invalid')
  await page.getByRole('button', { name: 'Emaili təsdiqlə', exact: true }).click()
  await expect(page.getByRole('alert')).toContainText('etibarsızdır')
  await expect.poll(() => page.evaluate(() => location.pathname === '/verify-email' && location.hash === '')).toBe(true)
})
test('network failure is accessible and form values remain available', async ({ page }) => {
  await page.goto('/register')
  await page.getByRole('button', { name: 'EN', exact: true }).click()
  await page.route('**/api/v1/auth/csrf', route => route.abort())
  await page.getByLabel('Full name', { exact: true }).fill('Safe Test')
  await page.getByLabel('Email address', { exact: true }).fill('network@example.test')
  await page.getByLabel('Password', { exact: true }).fill('Safe test passphrase 2026')
  await page.getByLabel('Confirm password', { exact: true }).fill('Safe test passphrase 2026')
  await page.getByRole('button', { name: 'Create account', exact: true }).click()
  await expect(page.getByRole('alert')).toContainText('Could not connect')
  await expect(page.getByLabel('Email address', { exact: true })).toHaveValue('network@example.test')
})
