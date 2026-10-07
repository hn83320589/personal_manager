# CLAUDE.md - PersonalManager Frontend

This file provides guidance to Claude Code when working with the frontend codebase.

共通規則（給 AI 的指示、不可以動的東西、測試原則）見根目錄 [`CLAUDE.md`](../CLAUDE.md)。

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

`VITE_API_BASE_URL`：API 位址，`.env.development` 與 `.env.production` 都設為 `/api`。開發時由 Vite（`vite.config.ts` 的 proxy）把 `/api` 與 `/files` 轉送到後端 `http://localhost:5037`。
前端與 API 必須是同一個 site，refresh token cookie（`SameSite=Strict`）才會送出（ADR-010）。

### 其他指令

```bash
npm run lint          # ESLint
npm run format        # Prettier 格式化；format:check 只檢查
npm run api:types     # 由 ../backend/openapi.json 產生 src/api/schema.ts（API 變更後執行）
npx vitest run        # 單元測試
npx playwright test   # E2E（會自動啟動後端與前端，使用本機安裝的 Google Chrome）
```

E2E（`e2e/`）以真實後端執行：`playwright.config.ts` 啟動 Development 環境的後端（示範資料，帳號 `admin`／`password123`）
與前端，每次使用新的 SQLite 檔案。本機若已經開著後端或前端會直接沿用；設定環境變數 `CI=1` 時改用建置後的前端（`vite preview`）。

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

### 資料流

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
│   ├── auth.ts profile.ts files.ts posts.ts portfolios.ts guestbook.ts admin.ts tools.ts
│   ├── collections.ts                # 清單型資源（學歷、技能…）共用的 CRUD 與排序
│   └── schema.ts / types.ts          # 產生的型別與 Schemas 別名
├── lib/                              # 純函式（皆有單元測試）
│   ├── sanitizeHtml.ts highlight.ts embeds.ts format.ts readingTime.ts fileTypes.ts
│   ├── calendar.ts                   # 月曆格子、依日分組、時間範圍、星期名稱
│   └── workDocument.ts postDocument.ts   # 作品／文章：API 資料與編輯器資料的轉換
├── composables/
│   ├── useAsyncData.ts               # 頁面讀取（loading、錯誤、404 分開）
│   ├── useAsyncAction.ts             # 寫入操作（防重複送出、成功與錯誤提示）
│   ├── useOwnedList.ts               # 後台清單頁（新增／更新／刪除／排序）
│   ├── useAutosave.ts useWorkEditor.ts   # 作品與文章的自動儲存
│   ├── useColorScheme.ts useAccent.ts    # 深淺色、主題色
│   ├── useUnsavedChangesGuard.ts     # 編輯器離開頁面前先存、未存成功時提醒
│   ├── useDebouncedSearch.ts         # 搜尋框停頓後才查詢
│   └── useTableOfContents.ts useSeo.ts   # 頁面標題：固定標題設在路由 meta.title，依內容變動的頁面呼叫 setPageSeo
├── stores/                           # auth（登入狀態）、toast（操作提示）
├── router/index.ts                   # 路由（見下方）
├── views/
│   ├── public/                       # 前台：DirectoryView、PublicLayout 與各頁
│   ├── manage/                       # 新版後台：AdminShell 與各頁
│   └── LoginView、RegisterView、ForgotPasswordView、ResetPasswordView、NotFoundView
└── components/
    ├── public/                       # 前台元件（卡片、輪播、區塊、lightbox、分頁 PaginationNav…）
    └── manage/                       # 後台元件（SidePanel、ManageList、編輯器…；editorCommands.ts 為兩個富文本編輯器共用）
        ├── works/                    # 作品編輯器（區塊、封面、資訊欄位）
        └── blog/                     # 文章編輯器（Tiptap：figure、embed、「/」選單）
```

設計 token 在 `src/assets/tokens.css`，對應成 Tailwind 顏色（`bg-paper`、`text-ink`、`text-muted`、`border-rule`、`bg-accent/10`…）。新元件一律用 token，不寫死 `bg-blue-*`、`bg-white`。

---

## 路由結構

### 公開路由

| 路徑                                                         | 說明                                           |
| ------------------------------------------------------------ | ---------------------------------------------- |
| `/`                                                          | 使用者目錄                                     |
| `/login`、`/register`、`/forgot-password`、`/reset-password` | 登入、註冊與密碼                               |
| `/@:username`                                                | 個人首頁（介紹、作品、經歷與技能、文章、聯絡） |
| `/@:username/works`、`/works/:slug`                          | 作品列表、作品詳情                             |
| `/@:username/blog`、`/blog/:slug`                            | 文章列表、文章                                 |
| `/@:username/guestbook`、`/calendar`                         | 留言板、公開行事曆                             |

舊網址（`portfolio`、`experience`、`skills`、`contact`、`about`）會導向新頁面或首頁對應區塊。

### 後台（`/admin`，需登入）

子路由：`dashboard`、`profile`、`works`、`works/:id`、`blog`、`blog/:id`、`experience`、`skills`、`contacts`、`comments`、`files`、`calendar`、`tasks`、`work-tracking`、`account`、`users`。
所有後台頁面都是 `AdminShell` 的子路由；`users` 限管理員（`meta.requiresAdmin`）。路由規則有測試（`router/__tests__/routes.spec.ts`）。

---

## 如何新增功能

### 新增一個 API 呼叫

1. 後端 API 有變動時：`backend` 執行 `UPDATE_OPENAPI=1 dotnet test --filter OpenApiDocument`，再到 `frontend` 執行 `npm run api:types`
2. 清單型資源（可排序、`/api/me/<資源>` 加 `order`，例如技能、學歷）在 `src/api/collections.ts` 以 `ownedCollection<Dto, SaveRequest>('/me/<資源>')` 建立即可。
   其他資源在 `src/api/<資源>.ts` 新增函式，型別使用 `Schemas['XxxDto']`，不要手寫 DTO 介面：

```typescript
import { http } from './http'
import type { Schemas } from './types'

export const todosApi = {
  list: () => http.get<Schemas['TodoDto'][]>('/me/todos'),
  create: (body: Schemas['SaveTodoRequest']) => http.post<Schemas['TodoDto']>('/me/todos', body),
  update: (id: number, body: Schemas['SaveTodoRequest']) =>
    http.put<Schemas['TodoDto']>(`/me/todos/${id}`, body),
  remove: (id: number) => http.delete(`/me/todos/${id}`),
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
