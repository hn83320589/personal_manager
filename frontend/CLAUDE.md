# CLAUDE.md - PersonalManager Frontend

This file provides guidance to Claude Code when working with the frontend codebase.

---

## 給 AI 的指示

每次任務完成時：

1. 在主專案的 `docs/TASKS.md` 勾選對應項目的 checkbox
2. 有新的技術債時，加入主專案 `docs/TASKS.md` 的「技術債」區塊
3. 做了未預期的架構決策時，記錄到主專案的 `docs/system-specification.md` §12 架構決策紀錄（ADR），附上原因
4. 回報更新了哪些檔案與區塊

---

## Constraints（不可以動的東西）

通用限制（所有專案適用）：

- 未告知不得引入新的 library
- 不得修改未被要求的現有功能
- 不得動資料庫 schema，除非任務明確要求

---

## 學習現有程式碼的方式

在開始任何新功能前：

- 找 3 個類似的現有功能或元件作為參考
- 確認常用的 pattern 和 utility
- 使用專案已有的 library，不自行發明

---

## 測試原則

- 測試行為，不測實作細節
- 一個 test case 一個 assertion（可能時）
- test 名稱要能描述情境，看名字就知道在測什麼
- 使用專案現有的 test utilities / helpers
- 測試必須是 deterministic，禁止依賴時間或隨機值

---

## 快速啟動

```bash
# 安裝依賴（首次或 package.json 有變動時）
npm install

# 開發模式啟動
npm run dev
# → http://localhost:5173

# 建置生產版本（含型別檢查）
npm run build

# 型別檢查
npm run type-check
```

### 環境變數

`VITE_API_BASE_URL`：API 位址，未設定時為 `/api`。本機開發在 `.env.development` 設為 `http://localhost:5037/api`（後端 port 5037）。
前端與 API 必須是同一個 site（相同的註冊網域，port 可不同），refresh token cookie 才會送出（ADR-010）。

### 其他指令

```bash
npm run lint          # ESLint（CI 會跑）
npm run format        # Prettier 格式化；CI 以 format:check 檢查
npm run api:types     # 由 ../backend/openapi.json 產生 src/api/schema.ts（CI 檢查是否最新）
npx vitest run        # 單元測試
npx playwright test   # E2E（會自動啟動後端與前端，首次需 npx playwright install chromium）
```

E2E（`e2e/`）以真實後端執行：`playwright.config.ts` 啟動 Development 環境的後端（示範資料，帳號 `admin`／`password123`）
與前端，每次使用新的 SQLite 檔案。本機若已經開著後端或前端會直接沿用；CI 以建置後的前端（`vite preview`）執行。

---

## 架構說明

### 技術棧

| 項目     | 工具/版本                                  |
| -------- | ------------------------------------------ |
| 框架     | Vue 3.5 Composition API + `<script setup>` |
| 語言     | TypeScript（strict）                       |
| 路由     | Vue Router 4                               |
| 狀態管理 | Pinia（不做持久化）                        |
| HTTP     | Axios，型別由 openapi-typescript 產生      |
| 樣式     | Tailwind CSS 3                             |
| 圖示     | Heroicons v2                               |
| 建置     | Vite 7                                     |
| 測試     | Vitest（單元）+ Playwright（E2E）          |

### 資料流（新架構，Phase 4 起）

```
View（.vue）
  → Store（Pinia，需要共用狀態時）
  → src/api/<資源>.ts（依 public／me／admin 分組的 API 模組，型別取自 Schemas）
  → src/api/http.ts（Axios：token、refresh、重試、錯誤轉換）
  → 後端 API
```


### HTTP 層（`src/api/http.ts`）

- 成功時回傳 `ApiResponse<T>` 裡的 `data`；失敗一律丟 `ApiError`（`status`、後端 `message`、驗證錯誤 `errors`；連不到伺服器時 status 為 0）
- access token 由 auth store 提供（只在記憶體），refresh token 是 httpOnly cookie，請求一律帶 credentials
- 401 時以 refresh cookie 續期後重試一次；同時多個 401 共用同一次 refresh；`/auth/*` 本身的 401 不續期
- 只重試 GET（網路錯誤、5xx，最多 2 次），寫入請求不重送

### 登入狀態（`src/stores/auth.ts`）

- 不寫入 localStorage；App 啟動時 `restoreSession()` 以 refresh cookie 還原
- 路由 `meta.requiresAuth`／`requiresGuest` 的守衛會等待還原完成
- session 過期時清除狀態並導向 `/login?redirect=…`；登入後只接受站內 redirect 路徑

### 安全規則

- `v-html` 只能接 `sanitizeHtml(...)`（DOMPurify）或 `highlightCode(...)`（所有文字已跳脫）的結果
- 不在 localStorage／sessionStorage 存放 token

---

## 專案結構


```
src/
├── main.ts / App.vue                 # 進入點；App 只有 RouterView 與全域 ToastHost
├── api/                              # 後端 API（型別由 schema.ts 產生）
│   ├── http.ts                       # HTTP 層（token、refresh、重試、上傳）
│   ├── public.ts                     # /api/public/users/{username}/…
│   ├── auth.ts profile.ts files.ts posts.ts portfolios.ts collections.ts
│   └── schema.ts / types.ts          # 產生的型別與 Schemas 別名
├── lib/                              # 純函式（皆有單元測試）
│   ├── sanitizeHtml.ts highlight.ts embeds.ts format.ts readingTime.ts fileTypes.ts
│   └── workDocument.ts postDocument.ts   # 作品／文章：API 資料與編輯器資料的轉換
├── composables/
│   ├── useAsyncData.ts               # 頁面讀取（loading、錯誤、404 分開）
│   ├── useAsyncAction.ts             # 寫入操作（防重複送出、成功與錯誤提示）
│   ├── useOwnedList.ts               # 後台清單頁（新增／更新／刪除／排序）
│   ├── useAutosave.ts useWorkEditor.ts   # 作品與文章的自動儲存
│   ├── useColorScheme.ts useAccent.ts    # 深淺色、主題色
│   └── useTableOfContents.ts useSeo.ts
├── stores/                           # auth（登入狀態）、toast（操作提示）
├── router/index.ts                   # 路由（見下方）
├── views/
│   ├── public/                       # 前台：DirectoryView、PublicLayout 與各頁
│   ├── manage/                       # 新版後台：AdminShell 與各頁
│   └── LoginView、ForgotPasswordView、ResetPasswordView、NotFoundView
└── components/
    ├── public/                       # 前台元件（卡片、輪播、區塊、lightbox…）
    └── manage/                       # 後台元件（SidePanel、ManageList、編輯器…）
        ├── works/                    # 作品編輯器（區塊、封面、資訊欄位）
        └── blog/                     # 文章編輯器（Tiptap：figure、embed、「/」選單）
```

設計 token 在 `src/assets/tokens.css`，對應成 Tailwind 顏色（`bg-paper`、`text-ink`、`text-muted`、`border-rule`、`bg-accent/10`…）。新元件一律用 token，不寫死 `bg-blue-*`、`bg-white`。

---

## 路由結構

### 公開路由

| 路徑                                            | 說明                                           |
| ----------------------------------------------- | ---------------------------------------------- |
| `/`                                             | 使用者目錄                                     |
| `/login`、`/forgot-password`、`/reset-password` | 登入與密碼                                     |
| `/@:username`                                   | 個人首頁（介紹、作品、經歷與技能、文章、聯絡） |
| `/@:username/works`、`/works/:slug`             | 作品列表、作品詳情                             |
| `/@:username/blog`、`/blog/:slug`               | 文章列表、文章                                 |
| `/@:username/guestbook`、`/calendar`            | 留言板、公開行事曆                             |

舊網址（`portfolio`、`experience`、`skills`、`contact`、`about`）會導向新頁面或首頁對應區塊。

### 後台（`/admin`，需登入）

子路由：`dashboard`、`profile`、`works`、`works/:id`、`blog`、`blog/:id`、`experience`、`skills`、`contacts`、`comments`、`files`、`calendar`、`tasks`、`work-tracking`、`account`、`users`。
所有後台頁面都是 `AdminShell` 的子路由；`users` 限管理員（`meta.requiresAdmin`）。路由規則有測試（`router/__tests__/routes.spec.ts`）。

---

## 如何新增功能

### 新增一個 API 呼叫

1. 後端 API 有變動時：`backend` 執行 `UPDATE_OPENAPI=1 dotnet test --filter OpenApiDocument`，再到 `frontend` 執行 `npm run api:types`
2. 在 `src/api/<資源>.ts` 新增函式，型別使用 `Schemas['XxxDto']`，不要手寫 DTO 介面：

```typescript
import { http } from './http'
import type { Schemas } from './types'

export const mySkillsApi = {
  list: () => http.get<Schemas['SkillDto'][]>('/me/skills'),
  create: (body: Schemas['SaveSkillRequest']) => http.post<Schemas['SkillDto']>('/me/skills', body),
  update: (id: number, body: Schemas['SaveSkillRequest']) =>
    http.put<Schemas['SkillDto']>(`/me/skills/${id}`, body),
  remove: (id: number) => http.delete(`/me/skills/${id}`),
}
```

3. 錯誤以 `ApiError` 處理，畫面顯示 `error.message`

### 新增一個後台清單頁

參考 `views/manage/SkillsView.vue`：`useOwnedList(xxxApi)` 提供清單與寫入，`ManageList` 顯示可排序的列，
`SidePanel` 為新增／編輯面板，`FormField` 包住每個欄位。

---

## 設計規範

- **視覺**：依前台 prototype（ADR-012）。顏色、字體、間距一律使用 `tokens.css` 與 Tailwind 設定中的 token，支援深淺色與 5 種主題色
- **元件**：前台元件在 `components/public/`、後台元件在 `components/manage/`；按鈕用 `.btn`／`.btn-primary`／`.btn-small`，欄位用 `.input` 搭配 `FormField`
- **無障礙**：可點的東西用 `<button>`／`<a>`，圖示按鈕要有 `aria-label`，圖片要有替代文字；尊重「減少動態」
- **TypeScript**：strict，不使用 `any`，所有 prop 需要型別定義
- **元件根層不要放 HTML 註解**：開發模式下會變成 fragment，外部傳入的 class 不會套用（註解寫在 `<script>` 裡）

---

## 開發注意事項

- **所有 API 欄位用 camelCase**，與後端 JSON 序列化一致
- **build 前確認 TypeScript 無錯誤**：`npm run type-check`
- **Admin 頁面取得 userId**：從 `authStore.user?.id` 取得，不可 hardcode `1`

---

## 最新異動記錄

### 2026/03/16（密碼重設功能）

- **密碼重設功能實作完成**：
  - `src/services/authService.ts`：新增 `forgotPassword(email)` (`POST /auth/forgot-password`)、`resetPassword(token, newPassword)` (`POST /auth/reset-password`)
  - 新增 `src/views/ForgotPasswordView.vue`：Email 輸入表單 → 呼叫 `authService.forgotPassword` → 顯示確認畫面（永遠成功，防枚舉）
  - 新增 `src/views/ResetPasswordView.vue`：從 `?token=` query param 讀取 token → 新密碼 + 確認密碼驗證 → 呼叫 `authService.resetPassword` → 成功顯示導向登入按鈕
  - `src/router/index.ts`：新增 `/forgot-password`（`requiresGuest: true`）、`/reset-password` 路由（lazy load）
  - `src/views/LoginView.vue`：「忘記密碼？」從 `<a href="#">` 改為 `<router-link to="/forgot-password">`

### 2026/03/16（DB Schema 正規化前端對應）

- **三項 DB Schema 正規化前端同步**：
  - **CalendarEvent.recurrenceRule**：
    - `src/types/api.ts`：`CalendarEvent` 新增 `recurrenceRule: string`
    - `CalendarEventForm.vue`：`formData.recurrence` 改為 `recurrenceRule`；`finalData` 加入 `recurrenceRule`；`initializeForm()` 從 `event.recurrenceRule` 初始化
  - **WorkTask.Project 正規化**：
    - `src/types/api.ts`：`WorkTask.project: string` 改為 `projectId?: number | null`、新增 `projectName?: string | null`；新增 `Project` interface
    - 新增 `src/services/projectService.ts`（getAll/getByUserId/getById/create/update/delete）
    - `WorkTaskForm.vue`：移除文字輸入 + 彈窗選擇器；改為 `<select>` 綁定 `projectId`；接受 `projects: Project[]` prop；`handleSubmit` emit `projectId`
    - `WorkTrackingView.vue`：import `projectService`；新增 `projectList` ref；`onMounted` 載入專案清單；`projects` computed 改用 `projectList`；所有 `task.project` 改為 `task.projectName`；`projectStats` 改用 `task.projectId`；`confirmEditProject` 改呼叫 `projectService.update(id, { name })`；傳 `:projects="projectList"` 給 `WorkTaskForm`
    - `TasksView.vue`、`TimeEntryForm.vue`、`TimeTrackerForm.vue`：`task.project` → `task.projectName`
  - **BlogPost.tags 正規化**：
    - `src/types/api.ts`：`BlogPost.tags: string` 改為 `tags: string[]`
    - `BlogEditorView.vue`：載入時 `(existingPost.tags || []).join(',')`（內部仍用逗號字串）；`saveDraft`/`publishPost` 傳送 `tags: tagsList.value`（陣列）
    - `BlogManageView.vue`：`allTags` computed、搜尋過濾、tag 過濾全部改為直接操作 `string[]`
    - `BlogGridView.vue`、`BlogTableView.vue`：`getTagsList` 接受 `string[] | string | undefined`
    - `UserBlogDetailView.vue`、`UserBlogListView.vue`：`parseTags` 接受 `string[] | string`
  - `src/stores/__tests__/blog.spec.ts`：mock post `tags: ''` → `tags: []`

### 2026/03/16（TOC + 測試）

- **文章目錄（TOC）自動生成**：
  - `UserBlogDetailView.vue` template：`lg:flex` 雙欄佈局，右側 `w-52` sticky TOC sidebar（lg+ 顯示）
  - `UserBlogDetailView.vue` script：新增 `contentRef`、`TocItem` interface、`tocItems`/`activeId` refs；`buildToc()` 解析 h1~h3 並指定 ID；`scrollTo(id)` 平滑捲動；`IntersectionObserver` 追蹤 activeId；`onUnmounted` 清除 observer；`nextTick` 後呼叫 `buildToc`
- **Vitest blogStore 測試**：
  - 新增 `src/stores/__tests__/blog.spec.ts`（16 個測試：initial state、getters、searchPosts、createPost/deletePost）
  - `vitest.config.ts`：新增 `environmentOptions.jsdom.url`、`globalSetup`、`pool: 'threads'` + `execArgv: ['--localstorage-file', ...]`
  - 新增 `src/test-utils/globalSetup.ts`：修復 Node.js 25 + `@vue/devtools-kit` localStorage 衝突
- **E2E Playwright 測試改寫**：
  - `e2e/auth.spec.ts`：登入流程（錯誤帳密/正確帳密）、受保護路由重導向、登出測試
  - `e2e/home.spec.ts`：用戶目錄頁、個人頁面導覽（`/@admin`、`/@admin/blog`）
  - 新增 `e2e/guestbook.spec.ts`：留言板顯示 + 提交表單流程
  - `e2e/portfolio.spec.ts`：作品集列表 + 響應式設計
  - `e2e/vue.spec.ts`：首頁載入驗證

### 2026/03/16（SEO）

- **SEO / Open Graph 標籤**：
  - 新增 `src/composables/useSeo.ts`：`setPageSeo(meta: SeoMeta)` utility + `stripHtml()` helper（無需外部 library）；操作 `document.title`、`og:title`/`og:description`/`og:image`/`og:type`/`og:url`、Twitter Card meta
  - `src/components/layout/UserLayout.vue`：loadUser 後呼叫 `applyUserSeo()`（fullName、summary、profileImageUrl）；新增 `watch(route.path, applyUserSeo)` — 在用戶頁面內部導覽時重置 SEO（避免文章頁 SEO 殘留）
  - `src/views/user/UserBlogDetailView.vue`：post 載入後呼叫 `setPageSeo({ title, description, ogType: 'article' })`；description 優先用 `post.summary`，否則用 `stripHtml(content).slice(0, 160)`

### 2026/03/16（分頁 + 搜尋）

- **分頁 + 部落格搜尋**：
  - `src/types/api.ts` 新增 `PagedResult<T>` 介面
  - `UserBlogListView.vue`：
    - 不再一次載入所有文章；改呼叫 `GET /blogposts/user/{uid}/public/paged`（server-side 分頁 + 搜尋）
    - 新增 `loadMeta()` — 同時呼叫 `/tags` 和 `/categories` 端點取得篩選選項
    - 搜尋欄 debounce 300ms 再呼叫 API；tag/category 篩選即時呼叫
    - 換頁改呼叫 API（`goToPage()`）
    - `isFiltered` computed — 用於區分「無文章」vs「搜尋無結果」
  - `UserGuestbookView.vue`：
    - 改呼叫 `GET /guestbookentries/user/{uid}/paged`（server-side 分頁）
    - 新增 `goToPage()`；留言數顯示 `totalCount`（server 回傳）

### 2026/03/16

- **Refresh Token 機制（TD-04）**：
  - `src/types/api.ts`：`AuthResponse` 加 `refreshToken`/`refreshTokenExpiresAt`
  - `src/services/authService.ts`：login/register 時存 `refresh_token` 到 localStorage；`logout()` 呼叫 `POST /auth/logout` 撤銷伺服器端 token；新增 `getRefreshToken()`；`clearAuthData()` 補清 `refresh_token`
  - `src/services/http.ts`：401 時嘗試呼叫 `POST /auth/refresh` 換新 token 並重試原始請求；只在非 auth endpoint 且未重試過時執行（防止無限迴圈）
- **TimeEntry API 串接（TD-01）**：
  - `src/types/api.ts` 新增 `TimeEntry`、`CreateTimeEntryDto`、`UpdateTimeEntryDto` 介面
  - 新增 `src/services/timeEntryService.ts`（getAll/getById/create/update/delete）
  - `src/stores/task.ts`：移除本地 `TimeEntry` interface（改用 api.ts）；`fetchTimeEntries`/`createTimeEntry`/`updateTimeEntry`/`deleteTimeEntry` 全部改為 API 呼叫；移除 `persist: { pick: ['timeEntries'] }` 設定
  - `src/views/admin/WorkTrackingView.vue`：`stopTimer()` 中 `taskId` 改為 `workTaskId`
- **文章瀏覽計數 + Dashboard 修復**：
  - `UserBlogDetailView.vue`：文章載入後 fire-and-forget 呼叫 `POST /blogposts/{id}/view`
  - `DashboardView.vue`：修正 `/portfolios` → `/portfolios/user/${uid}`；`/guestbookentries` → `/guestbookentries/all`（修正作品集計數顯示所有用戶資料的 bug、留言待審核計數顯示錯誤的 bug）

### 2026/03/12

- **檔案管理系統**：
  - 新增 `src/services/fileUploadService.ts`、`src/services/portfolioAttachmentService.ts`
  - `src/types/api.ts` 新增 `FileUploadType`、`FileUpload`、`PortfolioAttachment`、`CreatePortfolioAttachmentDto`
  - 新增 `src/components/admin/TiptapEditor.vue`（TipTap WYSIWYG，含圖片插入 + FilePicker 整合）
  - 新增 `src/components/admin/FilePicker.vue`（兩 Tab：庫選取 / 直接上傳）
  - 新增 `src/views/admin/FileManagerView.vue`（`/admin/files`，檔案管理後台）
  - `src/router/index.ts` 新增 `/admin/files` 路由
  - `src/components/layout/AdminLayout.vue` 新增「檔案管理」側欄項目
- **部落格富文本編輯器**：
  - `package.json` 新增 `@tiptap/*` 套件 + `@tailwindcss/typography`
  - `tailwind.config.js` 加入 `@tailwindcss/typography` plugin
  - `src/views/admin/BlogEditorView.vue` 替換為 TipTap 編輯器 + tag chip input
  - `src/views/user/UserBlogDetailView.vue` 改為 `v-html` + `prose` 渲染
  - `src/views/admin/BlogManageView.vue` 新增標籤篩選 chip UI
  - `src/views/user/UserBlogListView.vue` 新增標籤篩選 chip UI（已在前一輪完成）
- **作品集附件**：
  - `src/views/admin/ProjectManageView.vue` 新增附件管理 modal（從 FilePicker 選取或直接上傳）
  - `src/views/user/UserProjectDetailView.vue` 新增附件下載列表
  - `src/views/user/UserPortfolioView.vue` 新增附件數量徽章

### 2026/03/11

- **多使用者架構大改寫**：
  - `src/types/api.ts` 新增 `PublicUser`、`ProfileDirectory`；`PersonalProfile` 加 `themeColor`；`GuestBookEntry` 加 `targetUserId`；`AuthResponse` 新增 `userId`
  - `src/services/userDirectoryService.ts` — 新增，呼叫 `/profiles/directory`、`/users/public/{username}` 等端點
  - `src/services/commentService.ts` — 新增 `getApprovedByUser(targetUserId)` 和支援 `targetUserId` 的 `createGuestBookEntry`
  - `src/stores/userDirectory.ts` — 新增，管理 profiles 目錄和 username 解析
  - `src/composables/useTheme.ts` — 新增，提供 5 套主題 CSS 變數（blue/green/purple/rose/slate）
  - `src/router/index.ts` — 路由重構：移除舊扁平路由（`/about`、`/skills` 等），新增 `/@:username` 巢狀路由架構
  - `src/components/layout/UserLayout.vue` — 新增核心 Layout，負責解析 username、套用主題、顯示個人 Header + 水平導覽
  - `src/views/HomeView.vue` — 改寫為用戶目錄頁（搜尋 + 格狀卡片）
  - `src/views/user/` — 新增 10 個頁面（UserAboutView、UserExperienceView 等）
  - `src/components/layout/AppHeader.vue` — 簡化導覽，修正 user menu 連結為 `/admin/dashboard`、`/admin/profile`
  - `src/views/admin/ProfileManageView.vue` — 移除 hardcoded `userId=1`，改用 `authStore.user?.id`；改呼叫真實 API；新增 5 色主題選擇器
  - `src/assets/main.css` — 新增 `.form-input`、`.form-select`、`.form-label` 全局 class
- **Bug 修復**：
  - `src/services/authService.ts` — 登入/註冊時將 `id: response.data.userId` 存入 `user_data`，修復頁面重整後 userId 為 undefined 的問題
  - `src/views/admin/CommentManageView.vue` — `submitReply()` 中 `adminReply` 欄位未傳送已修正
  - `src/stores/task.ts` — 時間記錄（TimeEntry）改為本地實作 + `pinia-plugin-persistedstate` 持久化；`TimesheetView.vue` 新增 `add-entry` emit
  - `src/components/layout/AdminLayout.vue` — 移除 `/admin/files`（檔案管理）和 `/admin/settings`（系統設定）尚未實作的導覽項目及 `FolderIcon`、`Cog6ToothIcon` 匯入
- **功能 Stub 補完**：
  - `WorkTrackingView.vue` — `editProject()` 改為真實實作：打開重新命名 Modal，更新該專案下所有 WorkTask 的 `project` 欄位
  - `BlogManageView.vue` — `batchUpdateCategory()` 補實作：開啟分類輸入 Modal，批量更新選中文章的 `category`；`batchExport()` 移除 console.log（原本已有 JSON 下載邏輯）；`handleCategorySave()` 改為重新命名文章分類；`handleCategoryDelete()` 改為清空該分類文章的 `category` 欄位
  - `TaskManageView.vue` — 移除「更改分類」批次按鈕（`TodoItem` 無 `category` 欄位，功能不可行）及對應 stub function
  - `CommentManageView.vue` — `saveSettings()` 改用 `localStorage` 持久化；`onMounted` 時從 localStorage 讀取還原設定
- **聯絡方式管理補全**：
  - `src/services/contactMethodService.ts` — 新增，完整封裝 `/api/contactmethods` CRUD + 公開/私人端點
  - `src/views/admin/ContactManageView.vue` — 新增管理頁面（建立、編輯、刪除、排序）
  - `src/router/index.ts` — 新增 `/admin/contacts` 路由
  - `src/components/layout/AdminLayout.vue` — 新增「聯絡方式」側欄連結
- **Blog publishedAt 修復**：
  - 後端 `UpdateBlogPostDto` 缺少 `PublishedAt` 欄位，導致文章發布時間永遠為 null
  - 對應的前端修復：`src/views/admin/BlogEditorView.vue` `publishPost()` 已正確傳送 `publishedAt`（後端現在接受）
- **BlogEditorView 直接進入編輯路由修復**：
  - `loadPost()` 新增 API fallback：若 store 無資料則呼叫 `blogStore.fetchPostById(id)` 補載

### 2026/03/11

- **聯絡我公開頁面補全**：
  - `src/views/user/UserContactView.vue` — 新增，直接聯絡（Email/Phone）+ 社群媒體兩區塊，使用 `inject('userId')` 模式
  - `src/router/index.ts` — 新增 `/@:username/contact` 路由（`user-contact`）
  - `src/components/layout/UserLayout.vue` — 導覽列新增「聯絡我」項目
- **BlogManageView 修復**：
  - `previewPost()` — 修正預覽 URL 為 `/@{username}/blog/{slug}`（原本使用舊的 `/blog/{id}` 路徑）；新增 `useAuthStore` import
  - `duplicatePost()` — 新增 null check，避免 `createPost` 失敗時 crash
- **BlogEditorView 清理**：
  - 移除「富文本」toggle 按鈕及 WYSIWYG placeholder 區塊（功能未實作，改為僅保留 Markdown 模式）
  - 移除 `editorMode` ref

### 2026/02/22

- **API base URL 確認**：後端 port 為 `5037`（`.env.development` 已更新）
- **欄位對齊驗證**：`src/types/api.ts` 中所有介面與後端 DTO 欄位確認一致（camelCase）
- **前後端整合測試通過**：登入、技能、個人資料、部落格等核心 API 均正常回應
