# Personal Manager — 任務清單

> 最後更新：2026-03-13 | 依優先度排列

---

## 凡例

- 🔴 高優先度 — 影響安全或核心功能
- 🟡 中優先度 — 體驗改善或技術債
- 🟢 低優先度 — 錦上添花

狀態：`[ ]` 待做 | `[x]` 完成 | `[-]` 跳過/決定不做

---

## 🔴 高優先度

### 安全性

- [x] **[BE] 資源所有權驗證** (`TD-02`)
  - 現況：後端 CRUD 端點只驗證「是否已登入」，未驗證「是否為資源擁有者」
  - 影響：任何登入用戶可以刪除/修改其他用戶的資料
  - 做法：在每個 CRUD 控制器的 Update/Delete 加入 `userId == currentUserId` 驗證

- [ ] **[BE] 公開端點 Rate Limiting**
  - 現況：POST `/api/guestbookentries` 等公開端點無任何速率限制
  - 做法：在 `Program.cs` 加入 `AddRateLimiter`（.NET 7+ 內建）

### 技術債

- [ ] **[BE] DB Migration 策略** (`EnsureCreated` 限制)
  - 現況：使用 `EnsureCreated()`，新增欄位需手動 `ALTER TABLE`
  - 做法：改用 EF Core Migrations（`dotnet ef migrations add`），支援增量 schema 變更
  - 注意：此改動需要協調 Zeabur 生產 DB

---

## 🟡 中優先度

### 功能補完

- [ ] **[BE] TimeEntry API 實作** (`TD-01`)
  - 現況：工作時間記錄僅存於前端 localStorage，清除瀏覽器資料即遺失
  - 做法：新增 `TimeEntries` 資料表 + CRUD 端點，前端改為 API 呼叫
  - 影響檔案：後端 `Models/TimeEntry.cs`、`Controllers/TimeEntriesController.cs`；前端 `stores/task.ts`、`services/workTrackingService.ts`

- [ ] **[BE/FE] Refresh Token 機制** (`TD-04`)
  - 現況：JWT 24h 過期後強制重新登入，長時間工作會被中斷
  - 做法：後端加入 `RefreshToken` 資料表；前端 HttpService 在 401 時自動 refresh 而非直接登出

- [ ] **[FE] 分頁（Pagination）**
  - 現況：所有列表端點一次回傳所有資料，資料量大時效能低落
  - 影響頁面：部落格列表、作品集、留言板、待辦事項
  - 做法：後端加入 `page`/`pageSize` query param；前端加入分頁元件

- [ ] **[FE] Dashboard 統計數據實作**
  - 現況：`DashboardView.vue` 顯示的統計（文章數、留言數等）可能為硬編碼或空值
  - 做法：後端加入 `/api/dashboard/stats` 端點彙總各模組數量；前端串接顯示

- [ ] **[FE] 部落格文章搜尋**
  - 現況：前端有搜尋 UI 但後端無全文搜尋端點
  - 做法：後端加入 `?keyword=` query param 過濾 `Title`、`Content`、`Tags`

- [ ] **[FE] 作品集篩選功能**
  - 現況：`UserPortfolioView` 無技術類型篩選
  - 做法：加入 `Technologies` tag 篩選，類似部落格的 Tag 篩選實作

- [ ] **[FE] 文章瀏覽計數（viewCount）**
  - 現況：`BlogPost` 有 `ViewCount` 欄位但訪問文章詳情頁時未觸發遞增
  - 做法：`UserBlogDetailView` 載入文章時呼叫 `POST /api/blogposts/{id}/view` 端點

### 優化

- [ ] **[FE] SEO / Open Graph 標籤**
  - 現況：所有公開頁面缺乏 `<meta>` og 標籤，社群分享無預覽圖
  - 做法：使用 `vueuse/head` 或 `@unhead/vue` 動態設定每頁的 title、description、og:image

- [ ] **[BE] 檔案儲存改用 Object Storage** (`TD-05`)
  - 現況：上傳檔案存於本地 `files/` 路徑，生產環境容器重啟後遺失
  - 做法：整合 Zeabur Object Storage 或 S3 相容服務（如 Cloudflare R2）
  - 優先度：上線前必須解決

---

## 🟢 低優先度

### 功能增強

- [ ] **[BE] BlogPost.Tags 正規化**
  - 現況：Tags 以逗號字串儲存（`"vue,typescript,dotnet"`）
  - 做法：建立獨立 `Tags` 資料表 + `BlogPostTags` 多對多關聯
  - 備注：需評估改動成本，現況雖不優雅但功能正常

- [ ] **[BE] WorkTask.Project 正規化**
  - 現況：`Project` 只是 `WorkTask` 上的字串欄位，前端有「專案管理」UI 但後端無對應實體
  - 做法：建立獨立 `Projects` 資料表，WorkTask 加入 FK

- [ ] **[FE] 行事曆重複事件支援**
  - 現況：`CalendarEvent` 無重複規則（RRULE）欄位
  - 做法：加入 `RecurrenceRule` 欄位，前端行事曆解析並展開重複事件

- [ ] **[FE] 部落格文章目錄（TOC）自動生成**
  - 現況：`UserBlogDetailView` 無目錄導覽
  - 做法：解析文章 HTML，抓取 `h1~h3` 標籤生成浮動 TOC 元件

- [ ] **[FE] 文章閱讀時間估算**
  - 做法：依文章字數除以平均閱讀速度（250 字/分鐘）顯示「預計閱讀 N 分鐘」

- [ ] **[FE] 密碼重設功能**
  - 現況：忘記密碼只能直接改 DB
  - 做法：需要 Email 發送功能（SMTP 設定），後端建立 `PasswordResetTokens` 資料表

- [ ] **[BE] 後端 `/api/health` 端點**
  - 做法：加入 `HealthChecks`（DB 連線狀態），供 Zeabur 監控使用

### 程式碼品質

- [ ] **[BE] 後端單元測試補充**
  - 現況：測試專案存在但覆蓋率不明
  - 目標：`AuthService`、`BlogPostService`、`GuestBookEntryService` 各有完整單元測試

- [ ] **[FE] Vitest 單元測試補充**
  - 現況：Vitest 已設定，但 Store 與 Service 的測試可能不完整
  - 目標：`authStore`、`blogStore`、`httpService` 有基本測試

- [ ] **[FE] Playwright E2E 測試**
  - 現況：Playwright 已設定，測試腳本可能為空或最小化
  - 目標：登入流程、留言提交、個人頁瀏覽 3 個關鍵路徑有 E2E 測試

---

## 完成項目（參考）

- [x] JWT 認證系統（登入/登出/路由守衛）
- [x] 多用戶 `/@:username` 路由架構
- [x] 5 套主題色系統（CSS 變數 + useTheme composable）
- [x] UserLayout provide/inject userId 模式
- [x] 所有 15 個後端 API 控制器
- [x] 所有 28 個前端 View（10 公開 + 18 管理後台）
- [x] TipTap 富文本編輯器（含圖片上傳）
- [x] 部落格 Tag 篩選
- [x] 留言板多用戶支援（`TargetUserId`）
- [x] 聯絡方式 CRUD（`UserContactView` + `ContactManageView`）
- [x] 工作追蹤四模塊（專案/任務/時間表/報告）
- [x] 待辦事項多視圖（列表/Kanban/格狀）
- [x] 檔案管理（`FileManagerView` + `FilePicker` 元件）
- [x] DB 自動 fallback 至本地 JSON

---

*更新方式：完成項目後將 `[ ]` 改為 `[x]`，並同步更新 `CLAUDE.md` 的「最新異動記錄」*
