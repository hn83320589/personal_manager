import { expect, test } from '@playwright/test'
import { login, png } from './helpers'

test('建立作品：文字與圖庫區塊、上傳圖片與說明、自動儲存後公開', async ({ page }) => {
  await login(page, '/admin/works')

  await page.getByLabel('新作品標題').fill('E2E 海報設計')
  await page.getByRole('button', { name: '新增作品' }).click()
  await expect(page).toHaveURL(/\/admin\/works\/\d+$/)

  // 文字區塊
  await page.getByRole('button', { name: '＋ 新增第一個區塊' }).click()
  await page.getByRole('button', { name: '文字', exact: true }).click()
  await page.getByRole('textbox', { name: '內文' }).fill('以紅色為主色的系列海報。')

  // 圖庫：上傳兩張圖並為第一張寫說明
  await page.getByRole('button', { name: '＋ 在最後新增區塊' }).click()
  await page.getByRole('button', { name: '圖庫', exact: true }).click()
  await page
    .getByRole('region', { name: '圖庫區塊' })
    .locator('input[type=file]')
    .setInputFiles([
      { name: 'poster.png', mimeType: 'image/png', buffer: png(60, 80, [158, 43, 53]) },
      { name: 'card.png', mimeType: 'image/png', buffer: png(80, 60, [39, 84, 197]) },
    ])
  const captions = page.getByPlaceholder('為這張圖寫一句說明')
  await expect(captions).toHaveCount(2)
  await captions.first().fill('主視覺海報')

  // 公開並等待自動儲存
  await page.getByRole('button', { name: '公開這件作品' }).click()
  await expect(page.getByRole('status').filter({ hasText: '已自動儲存' })).toBeVisible()

  const preview = await page.getByRole('link', { name: '預覽 ↗' }).getAttribute('href')
  await page.goto(preview!)

  await expect(page.getByRole('heading', { level: 1, name: 'E2E 海報設計' })).toBeVisible()
  await expect(page.getByText('以紅色為主色的系列海報。')).toBeVisible()
  await expect(page.getByText('主視覺海報')).toBeVisible()
  await expect(page.getByRole('button', { name: /放大/ })).toHaveCount(2)
})
