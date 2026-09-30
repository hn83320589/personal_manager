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
- [x] `Program.cs` 拆成 `Setup/` 下的擴充方法（250 行 → 30 行）；啟動訊息改用 `ILogger`
- [x] `ICurrentUser` 與統一錯誤處理（不外洩例外訊息）
- [-] feature folder 骨架 → 併入 Phase 2，各 feature 重建時直接建立於 `Features/`

### Phase 2 — 後端功能重建（public／me／admin）
- [x] 範本：技能（熟練度與年資改為選填、排序 API、13 個整合測試）
- [x] 個人資料／使用者目錄（新增作品集模式、卡片版型與比例、技能顯示方式、目前狀態；目錄支援搜尋與分頁）
- [x] 經歷、學歷（日期驗證、在職中忽略結束日期、經歷日期改為 DateOnly）
- [x] 聯絡方式（新增 Behance、Dribbble、YouTube、Threads、LINE、個人網站；拒絕 javascript:／data: 等非 http(s) 連結）
- [x] 部落格（封面圖、HtmlSanitizer 清洗、嵌入白名單、每位使用者範圍內唯一的 slug、排程、閱讀時間、DB 端分頁與篩選、原子遞增瀏覽數；標籤改用 Tag 資料表並提供 `/api/me/tags`）
- [x] 留言板（公開不含 Email、只回傳已審核、回覆時間、移除 TargetUserId 預設值 1；流量限制改為可設定）
- [x] 行事曆（重複規則由後端展開、查詢區間最長一年、顏色格式驗證；所有時間一律以 UTC 存取）
- [x] 待辦、工作追蹤（實際時數改由時間紀錄加總、時間紀錄改用 DateOnly／TimeOnly 並自動計算時長、移除重複的任務與專案名稱、統計 API）
- [x] 檔案上傳（副檔名與 magic bytes 須一致、不收 SVG／原始檔、伺服器判定 MIME、記錄圖片寬高、50 MB 上限且超過時不先寫入暫存、nosniff、S3 改用伺服器判定的 Content-Type）
- [x] Auth（httpOnly cookie refresh token、token 雜湊、輪換與重用偵測、登入與 refresh 檢查停用帳號、重設／修改密碼後撤銷全部工作階段、帳號限英數底線連字號、密碼至少 8 碼、access token 15 分鐘）
- [x] Admin：使用者列表、停用／啟用（停用時結束工作階段）、角色變更（不可停用或降級自己）；第一位管理員以 `Admin:BootstrapEmails` 建立
- [x] 移除 `IRepository`、`CrudService`、`BaseApiController` 與舊的 DTO／Mapping 檔（Phase 3 重建作品集時一併移除）

### Phase 3 — 作品集（ADR-012）
- [x] 區塊式作品：封面輪播與裁切重點、角色／期間與自訂欄位、連結、七種內容區塊（文字、圖片、圖庫、附件、嵌入、重點數字、程式碼）
- [x] 伺服器依使用者自己上傳的檔案填入網址、尺寸與檔名；文字區塊清洗 HTML；嵌入白名單；外部圖片限 https
- [x] 公開卡片列表（分類、標籤篩選與 facets）、以 slug 取得單件作品；後台整份儲存與排序
- [x] 移除 `PortfolioAttachment`；migration 保留既有作品的標題並產生不重複的 slug（含資料遷移測試）

### Phase 4 — 前端基礎
- [x] 刪除死碼與未使用的套件（7 個元件／頁面／store、8 個套件、未使用的測試輔助與腳本）
- [x] TypeScript 恢復 `strict`
- [x] ESLint + Prettier、`lint`／`format` script，納入 CI（順帶修正按逗號無法新增標籤的 bug）
- [x] openapi-typescript：後端提交 `backend/openapi.json`（測試檢查與 API 一致），前端 `npm run api:types` 產生型別（CI 檢查是否最新）
- [x] 新 HTTP 層：access token 只放記憶體、refresh token 走 httpOnly cookie、多個 401 共用同一次 refresh、只有 GET 會重試
- [x] auth store 與登入、忘記密碼、重設密碼頁改接新 API（瀏覽器實測：登入、重新整理後還原、登出後需重新登入）
- [-] 其他資源的 API 模組 → 移到 Phase 5，與使用它的畫面一起建立
- [x] `v-html` 一律經過 DOMPurify
- 說明：其他舊頁面在 Phase 5 依新設計重寫時，才改用新 API 並刪除對應的舊 service／store，避免為即將淘汰的畫面做轉接；
  重寫時同步從 `eslint.config.js` 的 legacy 清單移除，最後刪除 `services/`、`types/api.ts`
- 部落格編輯器要用的 Tiptap 套件（code-block-lowlight、character-count、suggestion）在 Phase 5 升級編輯器時才安裝

### Phase 5 — 前台重新設計與後台簡化（依 prototype）
基礎
- [x] 設計 token（紙、面、墨、次要文字、分隔線、主題色）以 CSS 變數定義並對應 Tailwind 顏色；深淺色、5 種主題色；字體
- [x] 前台 API 模組（依 username 呼叫 /api/public）

前台
- [x] 前台版面：精簡導覽（作品、文章、經歷、聯絡）、手機選單、頁尾（留言板、行事曆、登入）；舊網址導向新網址
- [x] 個人首頁：介紹 → 作品 → 經歷與技能 → 最近的文章 → 聯絡
- [x] 作品列表：分類篩選、三種卡片（圖像／資訊／技術）與圖像比例、封面輪播
- [x] 作品詳情：七種區塊、lightbox、嵌入、程式碼、附件、上一件／下一件（程式碼語法上色待部落格編輯器安裝 lowlight 時一併加入）
- [x] 文章列表與文章頁（標籤篩選、目錄、閱讀時間）
- [x] 留言板、公開行事曆、使用者目錄首頁改用新設計

後台
- [x] 後台版面：分組側欄、手機可用；共用「列表 + 側邊編輯面板」與 `useAsyncAction`
- [x] 個人資料與前台呈現設定（作品集模式、卡片版型與比例、技能顯示、目前狀態、主題色）
- [x] 經歷／學歷、技能（可自訂標籤，系統建議作為備援）、聯絡方式
- [ ] 作品編輯器：左側作品資訊、主區區塊列表；多圖上傳、逐張說明、排序、設為封面、裁切重點；自動儲存
- [ ] 部落格：文章列表、編輯器升級（程式碼上色、字數、「/」選單、自動儲存、封面圖）
- [ ] 留言管理、檔案管理（刪除仍被引用的檔案前提示）
- [ ] 行事曆、待辦、工作追蹤（專案、任務、時間紀錄）
- [ ] 帳號：修改密碼；管理員：使用者管理

收尾
- [ ] 刪除舊的 services／stores／types/api.ts／舊元件，移除 eslint legacy 清單
- [ ] E2E：前台首頁、作品詳情、後台作品編輯（CI 以 SQLite 後端執行）
- 完成條件：沒有超過 400 行的 .vue；每個 store 都有單元測試

### 技術債（重構期間發現）
- [x] 第一位管理員以 `Admin:BootstrapEmails` 建立
- [x] 【Bug】部落格「特色圖片」從未被儲存 → 後端已新增 `CoverImageUrl`（前端於 Phase 5 重寫部落格編輯器時串接）
- [x] 移除 `WorkTask.Tags` 字串欄位
- [x] `dotnet-ef` 以 local tool 鎖定 9.0.13；CI 檢查兩組 migration 是否都已產生
- [ ] 刪除仍被作品引用的上傳檔案時，作品中的圖片或附件會失效 → 刪除前檢查引用或提示使用者（Phase 5 後台檔案管理時處理）

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
