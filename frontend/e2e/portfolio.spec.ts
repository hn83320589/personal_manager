import { test, expect } from '@playwright/test'

test.describe('作品集頁面', () => {
  test('顯示作品集列表', async ({ page }) => {
    await page.goto('/@admin/portfolio')

    // 確認頁面有作品集相關內容
    await expect(page.locator('text=作品').first()).toBeVisible()
  })

  test('響應式設計', async ({ page }) => {
    await page.goto('/@admin/portfolio')

    // 桌面版
    await page.setViewportSize({ width: 1200, height: 800 })
    await expect(page.locator('nav')).toBeVisible()

    // 手機版
    await page.setViewportSize({ width: 375, height: 667 })
    await expect(page.locator('nav')).toBeVisible()
  })
})
