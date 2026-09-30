import { expect, test } from '@playwright/test'
import { demoUser } from './helpers'

test.describe('公開頁面', () => {
  test('個人首頁依序呈現介紹、作品與經歷，點作品可看完整內容', async ({ page }) => {
    await page.goto(`/@${demoUser.username}`)

    await expect(
      page.getByRole('heading', { level: 1, name: new RegExp(demoUser.fullName) }),
    ).toBeVisible()
    await expect(page.getByRole('heading', { name: '作品', exact: true })).toBeVisible()
    await expect(page.getByRole('heading', { name: '經歷與技能' })).toBeVisible()

    await page.getByRole('link', { name: 'Personal Manager 系統' }).click()

    await expect(page).toHaveURL(/\/works\/personal-manager$/)
    await expect(
      page.getByRole('heading', { level: 1, name: 'Personal Manager 系統' }),
    ).toBeVisible()
    await expect(page.getByText('300+')).toBeVisible()
  })

  test('舊的作品集網址會導向新的作品頁', async ({ page }) => {
    await page.goto(`/@${demoUser.username}/portfolio`)

    await expect(page).toHaveURL(new RegExp(`/@${demoUser.username}/works$`))
  })

  test('訪客留言後會看到等待審核的說明', async ({ page }) => {
    await page.goto(`/@${demoUser.username}/guestbook`)

    await page.getByLabel('名字').fill('E2E 訪客')
    await page.getByLabel('留言').fill('作品很棒！')
    await page.getByRole('button', { name: '送出留言' }).click()

    await expect(page.getByRole('status')).toContainText('審核後會顯示')
  })

  test('可以切換深色模式，並在重新整理後保留', async ({ page }) => {
    await page.goto(`/@${demoUser.username}`)

    await page.getByRole('button', { name: /切換為(深|淺)色/ }).click()
    const chosen = await page.locator('html').getAttribute('data-theme')
    await page.reload()

    await expect(page.locator('html')).toHaveAttribute('data-theme', chosen!)
  })
})
