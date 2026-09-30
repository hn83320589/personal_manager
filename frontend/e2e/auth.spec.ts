import { test, expect } from '@playwright/test'

test.describe('使用者認證', () => {
  test('登入流程', async ({ page }) => {
    // 未登入直接訪問管理後台，應重導向登入頁
    await page.goto('/admin/dashboard')
    await expect(page).toHaveURL(/\/login/)

    // 確認登入表單存在
    await expect(page.locator('input[type="text"]')).toBeVisible()
    await expect(page.locator('input[type="password"]')).toBeVisible()
    await expect(page.locator('button[type="submit"]')).toBeVisible()

    // 填入錯誤帳密，應該不跳轉或顯示錯誤
    await page.fill('input[type="text"]', 'wronguser')
    await page.fill('input[type="password"]', 'wrongpass')
    await page.click('button[type="submit"]')
    await expect(page).toHaveURL(/\/login/)

    // 填入正確帳密
    await page.fill('input[type="text"]', 'admin')
    await page.fill('input[type="password"]', 'password123')
    await page.click('button[type="submit"]')

    // 應跳轉到管理後台
    await expect(page).toHaveURL(/\/admin\/dashboard/)
  })

  test('受保護路由 — 未登入重導向', async ({ page }) => {
    const protectedRoutes = [
      '/admin/dashboard',
      '/admin/profile',
      '/admin/blog',
    ]
    for (const route of protectedRoutes) {
      await page.goto(route)
      await expect(page).toHaveURL(/\/login/)
    }
  })

  test('登出後無法再訪問後台', async ({ page }) => {
    // 登入
    await page.goto('/login')
    await page.fill('input[type="text"]', 'admin')
    await page.fill('input[type="password"]', 'password123')
    await page.click('button[type="submit"]')
    await expect(page).toHaveURL(/\/admin\/dashboard/)

    // 找登出按鈕並點擊
    await page.click('text=登出')

    // 後台應不可訪問
    await page.goto('/admin/dashboard')
    await expect(page).toHaveURL(/\/login/)
  })
})
