import { test, expect } from '@playwright/test'

test('首頁可以正常載入', async ({ page }) => {
  await page.goto('/')
  await expect(page).toHaveTitle(/Personal Manager/)
})
