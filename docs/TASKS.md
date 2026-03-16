# Personal Manager — 任務清單

> 最後更新：2026-03-13 | 依優先度排列

---

## 範例

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

- [x] **[BE] 公開端點 Rate Limiting**
  - 現況：POST `/api/guestbookentries` 等公開端點無任何速率限制
  - 做法：在 `Program.cs` 加入 `AddRateLimiter`（.NET 7+ 內建）

### 技術債

- [x] **[BE] DB Migration 策略** (`EnsureCreated` 限制)
  - 現況：使用 `EnsureCreated()`，新增欄位需手動 `ALTER TABLE`
  - 做法：改用 EF Core Migrations（`dotnet ef migrations add`），支援增量 schema 變更
  - 注意：此改動需要協調 Zeabur 生產 DB

---

## 🟡 中優先度

### 功能補完

- [x] **[BE] TimeEntry API 實作** (`TD-01`)
  - 後端：新增 `Models/TimeEntry.cs`、DTOs、Mappings、Service、`TimeEntriesController.cs`、EF Migration `AddTimeEntry`
  - 前端：新增 `services/timeEntryService.ts`；`types/api.ts` 新增 `TimeEntry`、`CreateTimeEntryDto`、`UpdateTimeEntryDto`；`stores/task.ts` 改為 API 呼叫（移除 localStorage 持久化）

- [x] **[BE/FE] Refresh Token 機制** (`TD-04`)
  - 現況：JWT 24h 過期後強制重新登入，長時間工作會被中斷
  - 做法：後端加入 `RefreshToken` 資料表；前端 HttpService 在 401 時自動 refresh 而非直接登出

- [x] **[FE] 分頁（Pagination）**
  - 後端新增 `PagedResult<T>` DTO；`GET /api/blogposts/user/{id}/public/paged` 與 `GET /api/guestbookentries/user/{id}/paged` 支援 `page`/`pageSize`
  - 前端 `UserBlogListView` / `UserGuestbookView` 改為 server-side 分頁

- [x] **[FE] Dashboard 統計數據實作**
  - 修復 `/portfolios` → `/portfolios/user/${uid}`（原本拉全部用戶資料）
  - 修復 `/guestbookentries` → `/guestbookentries/all`（才能看到當前用戶 pending 留言）

- [x] **[FE] 部落格文章搜尋**
  - 後端 `/public/paged` 端點支援 `?keyword=` 過濾 `Title`、`Content`、`Tags`、`Summary`
  - 前端搜尋框改為 debounce 呼叫 API（300ms），並支援同時套用 tag + category 過濾

- [x] **[FE] 作品集篩選功能**
  - 現況已完整：`UserPortfolioView` 已有 searchTerm + selectedTech 下拉篩選 + filteredPortfolios computed

- [x] **[FE] 文章瀏覽計數（viewCount）**
  - 後端 `BlogPostsController` 新增 `POST /api/blogposts/{id}/view` 公開端點
  - `IBlogPostService` / `BlogPostService` 新增 `IncrementViewCountAsync(int id)`
  - 前端 `UserBlogDetailView.vue` 載入文章後 fire-and-forget 呼叫計數端點

### 優化

- [x] **[FE] SEO / Open Graph 標籤**
  - 新增 `src/composables/useSeo.ts`（`setPageSeo` + `stripHtml`，無需外部 library）
  - `UserLayout.vue`：用戶基礎 SEO（fullName、summary、profileImage）+ route watcher 重新套用
  - `UserBlogDetailView.vue`：文章層級 SEO（title、description/summary、og:type=article）

- [x] **[BE] 檔案儲存改用 Object Storage** (`TD-05`)
  - 新增 `IFileStorageProvider` 介面 + `LocalFileStorageProvider`（本地，dev fallback）+ `S3FileStorageProvider`（生產）
  - 新增 `Settings/FileStorageSettings.cs` `S3StorageSettings` 巢狀設定（BucketName、ServiceUrl、AccessKey、SecretKey、PublicBaseUrl、ForcePathStyle）
  - `Program.cs`：啟動時讀取 `FileStorage:S3` 設定，若已設定則使用 S3，否則 fallback 本地
  - 新增 NuGet `AWSSDK.S3` v3.7.414.3（支援 Zeabur Object Storage / Cloudflare R2 / AWS S3 所有 S3 相容服務）
  - `appsettings.json` 新增 `FileStorage.S3` section（空占位符，生產環境填入 Zeabur 環境變數）
  - ⚠️ 生產部署：在 Zeabur 設定以下環境變數：`FileStorage__S3__BucketName`、`FileStorage__S3__ServiceUrl`、`FileStorage__S3__AccessKey`、`FileStorage__S3__SecretKey`、`FileStorage__S3__PublicBaseUrl`

---

## 🟢 低優先度

### 功能增強

- [-] **[BE] BlogPost.Tags 正規化**
  - 現況：Tags 以逗號字串儲存（`"vue,typescript,dotnet"`）
  - 做法：建立獨立 `Tags` 資料表 + `BlogPostTags` 多對多關聯
  - 備注：需評估改動成本，現況雖不優雅但功能正常

- [-] **[BE] WorkTask.Project 正規化**
  - 現況：`Project` 只是 `WorkTask` 上的字串欄位，前端有「專案管理」UI 但後端無對應實體
  - 做法：建立獨立 `Projects` 資料表，WorkTask 加入 FK

- [-] **[FE] 行事曆重複事件支援**
  - 現況：`CalendarEvent` 無重複規則（RRULE）欄位
  - 做法：加入 `RecurrenceRule` 欄位，前端行事曆解析並展開重複事件

- [x] **[FE] 部落格文章目錄（TOC）自動生成**
  - `UserBlogDetailView` 加入 lg+ 右側 TOC sidebar
  - 解析文章 `h1~h3`，指定 ID，IntersectionObserver 追蹤 activeId

- [x] **[FE] 文章閱讀時間估算**
  - `estimateReadTime(content)` 依字數 / 200 估算，已顯示於文章 header

- [-] **[FE] 密碼重設功能**
  - 現況：忘記密碼只能直接改 DB
  - 做法：需要 Email 發送功能（SMTP 設定），後端建立 `PasswordResetTokens` 資料表

- [x] **[BE] 後端 `/api/health` 端點**
  - 新增 `DbHealthCheck : IHealthCheck`；`app.MapHealthChecks("/api/health")` 回傳 JSON，DB 模式回 Healthy，JSON fallback 回 Degraded

### 程式碼品質

- [x] **[BE] 後端單元測試補充**
  - 新增 `tests/PersonalManager.Tests.csproj`（xUnit + Moq）
  - `tests/AuthServiceTests.cs`：10 個測試（Login 有效/無效密碼/用戶不存在、Register 新用戶/重複用戶名、Refresh 有效/過期/已撤銷/不存在、Revoke 有效/不存在）
  - `tests/BlogPostServiceTests.cs`：10 個測試（GetPublicByUserId、GetBySlug、GetPublicPaged 分頁/關鍵字/分類/標籤過濾、IncrementViewCount）
  - `tests/GuestBookEntryServiceTests.cs`：9 個測試（GetApprovedByTargetUserId 過濾/空結果/排序、GetApprovedPaged 分頁/最後一頁/排除未審核/空結果）
  - 共 29 個測試，全部通過

- [x] **[FE] Vitest 單元測試補充**
  - `src/stores/__tests__/blog.spec.ts` 新增：16 個測試覆蓋 initial state、getters（publishedPosts/publicPosts/draftPosts）、searchPosts、createPost/deletePost
  - 修復 Vitest 3.2.4 + Node.js 25 + `@vue/devtools-kit` 相容性問題（`execArgv: ['--localstorage-file', ...]`）

- [x] **[FE] Playwright E2E 測試**
  - 改寫 `e2e/auth.spec.ts`：登入流程、受保護路由重導向、登出後無法訪問後台
  - 改寫 `e2e/home.spec.ts`：用戶目錄、個人頁瀏覽（/@admin, /@admin/blog）
  - 新增 `e2e/guestbook.spec.ts`：留言板顯示 + 提交表單
  - 改寫 `e2e/portfolio.spec.ts`：作品集列表 + 響應式
  - 改寫 `e2e/vue.spec.ts`：首頁載入

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
