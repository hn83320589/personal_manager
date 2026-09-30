# CLAUDE.md - PersonalManager Frontend

This file provides guidance to Claude Code when working with the frontend codebase.

---

## 給 AI 的指示

每次任務完成後，你必須：
1. 更新下方「進度」區塊的 checkbox
2. 若有新的技術債，加入「已知問題」
3. 若做了未預期的架構決策，加入「架構決策」並說明原因
4. 回報你更新了哪些區塊

以上未完成，任務視為未完成。

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

開發環境：`.env.development`（已存在，**不提交至 git**）
```
VITE_API_BASE_URL=http://localhost:5037/api
VITE_APP_TITLE=Personal Manager
```

生產環境：Zeabur 環境變數設定 `VITE_API_BASE_URL`。

> 後端 API port 為 `5037`

---

## 架構說明

### 技術棧

| 項目 | 工具/版本 |
|------|-----------|
| 框架 | Vue 3.5 Composition API + `<script setup>` |
| 語言 | TypeScript（嚴格模式） |
| 路由 | Vue Router 4 |
| 狀態管理 | Pinia + pinia-plugin-persistedstate v4 |
| HTTP | Axios（含攔截器） |
| 樣式 | Tailwind CSS 3 |
| 圖示 | Heroicons v2 |
| 建置 | Vite 7 |
| 測試 | Vitest（單元）+ Playwright（E2E） |

### 資料流

```
View（.vue）
  → Store（Pinia）
  → Service（API calls）
  → HttpService（Axios + 攔截器）
  → 後端 API（http://localhost:5037/api）
```

### HTTP 攔截器（`src/services/http.ts`）

- **請求**：自動從 `authStore` 取得 JWT token，加入 `Authorization: Bearer ...` header
- **回應**：解包 `ApiResponse<T>` wrapper，回傳 `.data`
- **錯誤**：401 自動登出並導向 `/login`

### API 回應格式

後端統一回傳 camelCase JSON：
```typescript
interface ApiResponse<T> {
  success: boolean
  message: string
  data: T
  errors: string[] | null
}
```

### Store 持久化

`pinia-plugin-persistedstate` v4 語法（composition API store 需要三參數形式）：
```typescript
export const useTaskStore = defineStore('task', () => {
  // ...
  return { timeEntries }
}, { persist: { pick: ['timeEntries'] } })  // 只持久化指定欄位
```

---

## 專案結構

```
PersonalManagerFrontend/
├── src/
│   ├── main.ts                       # 進入點（Pinia + persistedstate + Router 掛載）
│   ├── App.vue                       # 根元件
│   │
│   ├── types/
│   │   ├── api.ts                    # 所有 API 介面定義（ApiResponse、實體介面）
│   │   └── experience.ts             # 學經歷相關型別
│   │
│   ├── services/
│   │   ├── http.ts                   # Axios 封裝，HttpService 單例
│   │   ├── authService.ts            # /api/auth（login、register、logout）
│   │   ├── profileService.ts         # /api/profiles
│   │   ├── experienceService.ts      # /api/educations、/api/workexperiences
│   │   ├── skillService.ts           # /api/skills
│   │   ├── portfolioService.ts       # /api/portfolios
│   │   ├── calendarService.ts        # /api/calendarevents
│   │   ├── taskService.ts            # /api/todoitems
│   │   ├── workTrackingService.ts    # /api/worktasks
│   │   ├── blogService.ts            # /api/blogposts
│   │   ├── commentService.ts         # /api/guestbookentries
│   │   ├── contactMethodService.ts   # /api/contactmethods
│   │   └── userDirectoryService.ts   # /api/profiles/directory、/api/users/public
│   │
│   ├── stores/
│   │   ├── auth.ts                   # 認證狀態（token、user info、isLoggedIn）
│   │   ├── user.ts                   # 使用者資料
│   │   ├── profile.ts                # 個人資料
│   │   ├── experience.ts             # 學歷 + 工作經歷
│   │   ├── skill.ts                  # 技能
│   │   ├── portfolio.ts              # 作品集
│   │   ├── calendar.ts               # 行事曆
│   │   ├── task.ts                   # 待辦事項 + 工作任務 + 時間記錄（timeEntries 持久化）
│   │   ├── blog.ts                   # 部落格文章
│   │   ├── comment.ts                # 留言
│   │   └── userDirectory.ts          # 用戶目錄與 username → userId 解析
│   │
│   ├── composables/
│   │   └── useTheme.ts               # 5 套主題 CSS 變數（blue/green/purple/rose/slate）
│   │
│   ├── router/
│   │   └── index.ts                  # 路由定義（/@:username 架構 + admin 路由守衛）
│   │
│   ├── views/
│   │   ├── HomeView.vue              # 首頁（用戶目錄，格狀卡片 + 搜尋）
│   │   ├── LoginView.vue             # 登入
│   │   ├── NotFoundView.vue          # 404
│   │   ├── user/                     # 個人頁面（/@:username 路由，由 UserLayout 包覆）
│   │   │   ├── UserAboutView.vue     # 關於我
│   │   │   ├── UserExperienceView.vue # 學經歷
│   │   │   ├── UserSkillView.vue     # 技能
│   │   │   ├── UserPortfolioView.vue # 作品集
│   │   │   ├── UserProjectDetailView.vue # 作品詳情
│   │   │   ├── UserBlogListView.vue  # 部落格列表
│   │   │   ├── UserBlogDetailView.vue # 文章詳情
│   │   │   ├── UserCalendarView.vue  # 公開行事曆
│   │   │   ├── UserGuestbookView.vue # 留言板
│   │   │   └── UserContactView.vue  # 聯絡我
│   │   └── admin/                    # 管理後台
│   │       ├── DashboardView.vue
│   │       ├── ProfileManageView.vue
│   │       ├── ExperienceManageView.vue
│   │       ├── SkillManageView.vue
│   │       ├── ProjectManageView.vue
│   │       ├── CalendarManageView.vue
│   │       ├── WorkTrackingView.vue  # 工作追蹤（任務 + 時間記錄 + 報告）
│   │       ├── TaskManageView.vue
│   │       ├── BlogManageView.vue
│   │       ├── BlogEditorView.vue
│   │       ├── CommentManageView.vue
│   │       └── ContactManageView.vue # 聯絡方式 CRUD
│   │
│   ├── components/
│   │   ├── layout/
│   │   │   ├── AppHeader.vue         # 公開頁首（首頁 + 登入頁使用）
│   │   │   ├── AppFooter.vue
│   │   │   ├── AdminLayout.vue       # 管理後台版面（側欄 + 頂列）
│   │   │   └── UserLayout.vue        # 個人頁面版面（主題、用戶 Header、水平導覽）
│   │   ├── ui/                       # BaseButton、BaseCard、BaseInput、BaseModal 等
│   │   ├── common/                   # LoadingSpinner 等共用元件
│   │   ├── admin/                    # 管理後台專用表單元件
│   │   ├── blog/                     # BlogGridView、BlogTableView
│   │   ├── calendar/                 # CalendarGrid、WeekView
│   │   ├── task/                     # TaskListView、TaskKanbanView、TaskGridView
│   │   └── work/                     # ProjectsView、TasksView、TimesheetView、ReportsView
│   │
│   ├── assets/
│   │   └── main.css                  # Tailwind 全局樣式（含 .form-input、.form-select）
│   └── test-utils/                   # 測試輔助工具
│
├── .env.development                  # 開發環境變數（不提交）
├── .env.production                   # 生產環境變數（不提交）
├── package.json
├── vite.config.ts
├── tsconfig.json
└── tailwind.config.js
```

---

## 路由結構

### 公開路由（不需登入）

| 路徑 | View | 說明 |
|------|------|------|
| `/` | HomeView | 用戶目錄（格狀卡片） |
| `/login` | LoginView | 登入 |
| `/@:username` | UserAboutView | 個人頁面（UserLayout 包覆） |
| `/@:username/experience` | UserExperienceView | 學經歷 |
| `/@:username/skills` | UserSkillView | 技能 |
| `/@:username/portfolio` | UserPortfolioView | 作品集 |
| `/@:username/portfolio/:id` | UserProjectDetailView | 作品詳情 |
| `/@:username/blog` | UserBlogListView | 部落格列表 |
| `/@:username/blog/:slug` | UserBlogDetailView | 文章詳情 |
| `/@:username/calendar` | UserCalendarView | 公開行事曆 |
| `/@:username/guestbook` | UserGuestbookView | 留言板 |
| `/@:username/contact` | UserContactView | 聯絡我 |

### 管理後台路由（需要登入，路由守衛保護）

| 路徑 | View |
|------|------|
| `/admin/dashboard` | DashboardView |
| `/admin/profile` | ProfileManageView |
| `/admin/experience` | ExperienceManageView |
| `/admin/skills` | SkillManageView |
| `/admin/projects` | ProjectManageView |
| `/admin/calendar` | CalendarManageView |
| `/admin/work-tracking` | WorkTrackingView |
| `/admin/tasks` | TaskManageView |
| `/admin/blog` | BlogManageView |
| `/admin/blog/new` | BlogEditorView |
| `/admin/blog/:id/edit` | BlogEditorView |
| `/admin/comments` | CommentManageView |
| `/admin/contacts` | ContactManageView |

---

## 如何新增功能

### 新增一個 API 呼叫

1. 在 `src/types/api.ts` 定義介面（camelCase 欄位）
2. 在對應的 `src/services/xxxService.ts` 新增方法
3. 在對應的 `src/stores/xxx.ts` 新增 action
4. 在 View/Component 中呼叫 store action

### 新增一個頁面

1. 在 `src/views/` 建立 `.vue` 檔案
2. 在 `src/router/index.ts` 新增路由（使用懶載入 `() => import(...)`）
3. 若需要認證，路由 `meta` 加 `{ requiresAuth: true }`

### 新增一個 Service

```typescript
import httpService from './http'
import type { ApiResponse, Skill } from '@/types/api'

export const skillService = {
  getAll: () => httpService.get<Skill[]>('/skills'),
  getById: (id: number) => httpService.get<Skill>(`/skills/${id}`),
  create: (dto: CreateSkillDto) => httpService.post<Skill>('/skills', dto),
  update: (id: number, dto: Partial<CreateSkillDto>) => httpService.put<Skill>(`/skills/${id}`, dto),
  delete: (id: number) => httpService.delete<void>(`/skills/${id}`),
}
```

### UserLayout 與 provide/inject

`UserLayout.vue` 透過 `provide('userId', ...)` 傳遞 userId 給子頁面，子頁面用 `inject<ComputedRef<number | null>>('userId')` 取得：
```typescript
// UserLayout.vue
provide('userId', computed(() => publicUser.value?.id ?? null))  // ComputedRef<number | null>

// UserAboutView.vue
const userId = inject<ComputedRef<number | null>>('userId')
```

---

## 設計規範

- **色調**：淺灰、淺藍、白色為主，冷色調
- **元件**：基礎 UI 元件在 `src/components/ui/`，直接使用，不重複建立
- **表單樣式**：使用 `src/assets/main.css` 定義的 `.form-input`、`.form-select`、`.form-label`，確保文字顏色可見
- **TypeScript**：嚴格模式，不使用 `any`，所有 prop 需要型別定義
- **主題**：個人頁面透過 `useTheme(themeColor)` 取得 CSS 變數，套用至 UserLayout 的 `:style`

---

## 開發注意事項

- **不要修改 `http.ts` 的攔截器邏輯**，除非有明確需求
- **所有 API 欄位用 camelCase**，與後端 JSON 序列化一致
- **build 前確認 TypeScript 無錯誤**：`npm run type-check`
- **pinia-plugin-persistedstate v4**：compose store 需用三參數 `defineStore(id, setup, { persist })`，舊版 v3 語法不相容
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
