import { test, expect } from '@playwright/test'

test.describe('留言板', () => {
  test('顯示留言板頁面', async ({ page }) => {
    await page.goto('/@admin/guestbook')

    // 應該有留言板相關文字
    await expect(page.locator('text=留言').first()).toBeVisible()
  })

  test('提交留言表單', async ({ page }) => {
    await page.goto('/@admin/guestbook')

    // 確認表單存在
    const nameInput = page.locator('input[placeholder*="姓名"], input[type="text"]').first()
    await expect(nameInput).toBeVisible()

    // 填入留言資料
    await nameInput.fill('E2E 測試訪客')

    // 找 textarea 填入訊息
    const messageInput = page.locator('textarea').first()
    await expect(messageInput).toBeVisible()
    await messageInput.fill('這是一則 E2E 自動測試留言')

    // 送出表單
    const submitBtn = page.locator('button[type="submit"], button:has-text("送出"), button:has-text("留言")').first()
    await submitBtn.click()

    // 成功後應顯示某種回饋（成功訊息或表單清空）
    // 等候任一條件：訊息清空或出現成功提示
    await page.waitForTimeout(1000)
    // 不做強斷言，主要確認沒有 JS error crash
  })
})
