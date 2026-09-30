import os from 'node:os'
import path from 'node:path'
import process from 'node:process'
import { defineConfig, devices } from '@playwright/test'

/**
 * E2E：同時啟動後端（Development 環境，會寫入示範資料）與前端，以真實的 API 測試。
 * 每次執行使用新的 SQLite 檔案，測試之間不受上次資料影響。
 * CI 以建置後的前端（vite preview）執行；本機開發用 dev server，已在執行時直接沿用。
 */
const isCI = !!process.env.CI
const database = path.join(os.tmpdir(), `personal-manager-e2e-${Date.now()}.db`)
const frontendPort = isCI ? 4173 : 5173

export default defineConfig({
  testDir: './e2e',
  timeout: 60_000,
  expect: { timeout: 10_000 },
  forbidOnly: isCI,
  retries: isCI ? 1 : 0,
  // 測試共用同一個後端與示範帳號，依序執行較穩定
  workers: 1,
  reporter: isCI ? [['list'], ['html', { open: 'never' }]] : 'list',
  use: {
    baseURL: `http://localhost:${frontendPort}`,
    trace: 'retain-on-failure',
    locale: 'zh-TW',
    timezoneId: 'Asia/Taipei',
  },
  // CI 使用 Playwright 內建的 Chromium；本機直接使用已安裝的 Google Chrome，不必另外下載瀏覽器
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'], channel: isCI ? undefined : 'chrome' },
    },
  ],
  webServer: [
    {
      command:
        'dotnet run --project ../backend/src/PersonalManager.Api --urls http://localhost:5037',
      url: 'http://localhost:5037/api/public/users',
      env: {
        ASPNETCORE_ENVIRONMENT: 'Development',
        ConnectionStrings__DefaultConnection: `Data Source=${database}`,
        // 所有測試來自同一個 IP，放寬流量限制（限制本身由後端測試驗證）
        RateLimiting__AuthPermitsPerMinute: '1000',
        RateLimiting__PublicWritePermitsPerMinute: '1000',
        RateLimiting__SessionPermitsPerMinute: '1000',
      },
      reuseExistingServer: !isCI,
      timeout: 180_000,
    },
    {
      command: isCI ? 'npm run preview -- --port 4173 --strictPort' : 'npm run dev',
      url: `http://localhost:${frontendPort}`,
      reuseExistingServer: !isCI,
      timeout: 120_000,
    },
  ],
})
