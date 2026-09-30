import { expect, test } from '@playwright/test'
import { login } from './helpers'

test.describe('登入', () => {
  test('未登入時進入後台會先到登入頁，登入後回到原本的頁面', async ({ page }) => {
    await page.goto('/admin/works')

    await expect(page).toHaveURL(/\/login\?redirect=/)
    await login(page, '/admin/works')
    await expect(page.getByRole('heading', { level: 1, name: '作品' })).toBeVisible()
  })

  test('重新整理後仍保持登入（以 httpOnly cookie 還原）', async ({ page }) => {
    await login(page)

    await page.reload()

    await expect(page).toHaveURL(/\/admin\/dashboard$/)
    await expect(page.getByRole('heading', { level: 1, name: /你好/ })).toBeVisible()
  })

  test('登出後無法再進入後台', async ({ page }) => {
    await login(page)

    await page.getByRole('button', { name: '登出' }).click()
    await page.goto('/admin/dashboard')

    await expect(page).toHaveURL(/\/login/)
  })
})
