# Personal Manager — 任務清單

> 最後更新：2026-09-30 | 依優先度排列

---

## 🚧 2026 重構（進行中）

完整計畫：重構計畫頁面（claude.ai artifact「Personal Manager 重構計畫」）。已確認的決策：
- D1 移除 JSON fallback，本地與暫時的執行環境一律使用 SQLite，保留 Pomelo（MySQL/MariaDB）套件以便日後切換
- D2 合併為 monorepo（`backend/`、`frontend/`、`docs/`）
- D3 refresh token 改用 httpOnly cookie，access token 只放記憶體
- D4 引入 DOMPurify、HtmlSanitizer、ESLint + Prettier、openapi-typescript、GitHub Actions；部落格編輯器另加 `@tiptap/extension-code-block-lowlight` + `lowlight`、`@tiptap/extension-character-count`、`@tiptap/suggestion`（2026-09-30 核准）
- D5 後端改為 feature folder
- 目前沒有雲端環境（Zeabur 已停用），不使用 Docker

### Phase 0 — 安全網
- [x] 合併前後端 repo 為 monorepo（git subtree，保留歷史）
- [x] 修復後端測試專案編譯（29 個測試恢復執行）
- [x] 修復前端 8 個失敗的單元測試（改為驗證行為）
- [x] 新增 GitHub Actions CI（後端 build/test、前端 test/type-check/build）
- [x] 整理 git 追蹤範圍（`.gitignore` 改為 monorepo 版本）
- [x] `.sln` 與測試專案搬遷（於 Phase 1 完成）

### Phase 1 — 後端基礎
- [x] 後端改為 `src/`、`tests/` 同層並建立 `PersonalManager.sln`
- [x] 預設 SQLite、`Database:Provider` 切換、兩組 migration；移除 JSON fallback
- [x] 整合測試基礎（WebApplicationFactory + 暫存 SQLite）
- [x] 索引改由 `HasIndex` 管理，移除 `DatabaseSeeder.CreateIndexesAsync` 的 raw SQL；補上 token、Tag、留言板 TargetUserId 等缺少的索引
- [x] 移除寫死的 JWT 金鑰，啟動時驗證 `Jwt:SecretKey`（背景安全審查發現原始碼內有預設金鑰）
- [x] seeder 只在 Development 執行；CORS 來源改從設定讀取；Swagger 只在 Development 開啟
- [ ] `Program.cs` 拆成擴充方法；改用 `ILogger`
- [ ] `ICurrentUser` 與統一錯誤處理（不外洩例外訊息）
- [ ] feature folder 骨架

### 技術債（重構期間發現）
- [ ] 非 Development 環境不再自動建立 admin 帳號 → Phase 2 Admin 功能需提供建立第一個管理員的方式（例如以設定指定的 Email 註冊時授予 Admin）
- [ ] 【Bug】部落格編輯器的「特色圖片」欄位不存在於前端型別與後端資料表，上傳的圖片從未被儲存 → Phase 2 Blog 重建時補上 `CoverImage`
- [ ] `BlogPost.Tags`、`WorkTask.Tags` 字串欄位與其註解仍寫著「供 JSON fallback 使用」，JSON 模式已移除 → Phase 2 一併刪除欄位
- [ ] 本地 `dotnet ef` 工具為 9.0.8，runtime 為 9.0.13（產生 migration 時會出現警告，不影響結果）

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

- [x] **[BE/FE] BlogPost.Tags 正規化**
  - 後端：`Tags` 資料表 + `BlogPostTags` 多對多 + `BlogPostRepository`（SyncTagsAsync）+ EF Migration `AddRecurrenceRuleProjectTag`
  - 前端：`BlogPost.tags: string[]`；`BlogEditorView` 內部仍用逗號字串、提交時轉陣列；`BlogManageView/BlogGridView/BlogTableView/UserBlogDetailView/UserBlogListView` 全部改為直接使用 `string[]`

- [x] **[BE/FE] WorkTask.Project 正規化**
  - 後端：`Projects` 資料表 + `ProjectsController` + `IProjectService`/`ProjectService` + EF Migration
  - 前端：`WorkTask.projectId`/`projectName`；`WorkTaskForm` 改為下拉選單；`WorkTrackingView` 動態載入專案清單；`TasksView/TimeEntryForm/TimeTrackerForm` 改用 `projectName`

- [x] **[FE] 行事曆重複事件支援**
  - 後端：`CalendarEvent.RecurrenceRule` + EF Migration
  - 前端：`CalendarEvent.recurrenceRule`；`CalendarEventForm` 送出/讀入 `recurrenceRule`

- [x] **[FE] 部落格文章目錄（TOC）自動生成**
  - `UserBlogDetailView` 加入 lg+ 右側 TOC sidebar
  - 解析文章 `h1~h3`，指定 ID，IntersectionObserver 追蹤 activeId

- [x] **[FE] 文章閱讀時間估算**
  - `estimateReadTime(content)` 依字數 / 200 估算，已顯示於文章 header

- [x] **[BE/FE] 密碼重設功能**
  - 後端：`Models/PasswordResetToken.cs`、`Settings/EmailSettings.cs`、`Services/EmailService.cs`（`IEmailService`/`SmtpEmailService`/`NoOpEmailService`）、`Data/JsonData/PasswordResetTokens.json`、`DTOs/AuthDtos.cs` 加 `ForgotPasswordRequest`/`ResetPasswordRequest`、`Data/ApplicationDbContext.cs` 加 `DbSet<PasswordResetToken>`、`Auth/AuthService.cs` 加 `ForgotPasswordAsync`/`ResetPasswordAsync`、`Controllers/AuthController.cs` 加 `POST /auth/forgot-password`/`POST /auth/reset-password`（rate-limited）、`appsettings.json` 加 `Email` section、`Program.cs` DI（SMTP/NoOp 自動判斷）、EF Migration `AddPasswordResetToken`
  - 前端：`src/services/authService.ts` 加 `forgotPassword`/`resetPassword`；新增 `src/views/ForgotPasswordView.vue`（Email 輸入、防枚舉攻擊）；新增 `src/views/ResetPasswordView.vue`（token 從 query param 讀取、密碼確認驗證）；`src/router/index.ts` 新增 `/forgot-password`/`/reset-password` 路由；`src/views/LoginView.vue` 「忘記密碼？」改為 `<router-link to="/forgot-password">`
  - 本地開發：SMTP 未設定時，重設連結會以 Warning log 輸出至後端 console

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
