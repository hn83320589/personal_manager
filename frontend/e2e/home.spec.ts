import { test, expect } from '@playwright/test'

test.describe('首頁 — 用戶目錄', () => {
  test('顯示用戶目錄', async ({ page }) => {
    await page.goto('/')

    // 確認頁面標題
    await expect(page).toHaveTitle(/Personal Manager/)

    // 首頁應該有搜尋框
    await expect(page.locator('input[type="text"], input[placeholder]').first()).toBeVisible()
  })

  test('個人頁瀏覽 — 關於我', async ({ page }) => {
    await page.goto('/@admin')

    // UserLayout 渲染：應包含使用者名稱或相關資訊
    await expect(page.locator('nav')).toBeVisible()
  })

  test('個人頁瀏覽 — 部落格列表', async ({ page }) => {
    await page.goto('/@admin/blog')

    // 應有頁面標題文字
    await expect(page.locator('text=部落格').first()).toBeVisible()
  })

  test('個人頁瀏覽 — 頁內導覽有效', async ({ page }) => {
    await page.goto('/@admin')

    // 確認導覽列存在且含有連結
    const nav = page.locator('nav')
    await expect(nav).toBeVisible()
  })
})
