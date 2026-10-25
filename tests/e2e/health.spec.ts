import { test, expect } from '../../apps/web/node_modules/@playwright/test'

test('frontend reaches the API and real PostgreSQL', async ({ page }) => {
  await page.goto('/')
  await expect(page.getByRole('heading', { name: 'Yolumuz buradan başlayır.' })).toBeVisible()
  await expect(page.getByTestId('api-status')).toHaveText('İşləyir')
  await expect(page.getByTestId('database-status')).toHaveText('İşləyir')
})

test('frontend explains API failure', async ({ page }) => {
  await page.route('**/api/v1/health/**', route => route.abort())
  await page.goto('/')
  await expect(page.getByTestId('api-status')).toHaveText('Əlçatan deyil')
  await expect(page.getByTestId('database-status')).toHaveText('Əlçatan deyil')
})
