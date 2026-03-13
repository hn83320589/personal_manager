# Personal Manager — 系統規格書與架構文件

> 版本：v2.0 | 最後更新：2026-03-13 | 狀態：現行版本

---

## 目錄

1. [專案概述](#1-專案概述)
2. [系統邊界與使用者角色](#2-系統邊界與使用者角色)
3. [功能規格](#3-功能規格)
4. [系統架構](#4-系統架構)
5. [後端架構規格](#5-後端架構規格)
6. [前端架構規格](#6-前端架構規格)
7. [資料模型規格](#7-資料模型規格)
8. [API 規格摘要](#8-api-規格摘要)
9. [安全設計](#9-安全設計)
10. [部署架構](#10-部署架構)
11. [已知限制與技術債](#11-已知限制與技術債)
12. [架構決策紀錄（ADR）](#12-架構決策紀錄adr)

---

## 1. 專案概述

### 1.1 產品定義

**Personal Manager** 是一個雙模式個人品牌平台：

| 模式 | 目標用戶 | 主要功能 |
|------|---------|---------|
| **公開展示網站** | 訪客、潛在雇主、合作夥伴 | 瀏覽個人介紹、作品集、部落格、留言 |
| **個人管理後台** | 網站擁有者 | 管理所有內容、追蹤工作、管理待辦事項 |

### 1.2 倉庫結構

採用三個獨立 Git 倉庫策略：

```
personal_manager/                          ← 主專案（本倉庫）
├── CLAUDE.md
├── README.md
├── docs/                                  ← 所有技術文件
└── local-development/
    ├── PersonalManagerBackend/            ← 後端倉庫（hn83320589/PersonalManagerBackend）
    └── PersonalManagerFrontend/           ← 前端倉庫（hn83320589/PersonalManagerFrontend）
```

### 1.3 技術棧速覽

| 層級 | 技術 | 版本 |
|------|------|------|
| 後端框架 | C# .NET Web API | 9.0 |
| ORM | Entity Framework Core + Pomelo MySQL | 9.x |
| 資料庫 | MariaDB | 11.x（Zeabur 雲端） |
| 前端框架 | Vue 3 Composition API | 3.5 |
| 語言 | TypeScript | 5.x（嚴格模式） |
| 狀態管理 | Pinia + pinia-plugin-persistedstate | 2.x + 4.x |
| HTTP 客戶端 | Axios | 1.x |
| CSS 框架 | Tailwind CSS | 3.x |
| 富文本編輯 | TipTap | 2.14.0 |
| 認證 | JWT Bearer Token | HS256，24h 有效期 |
| 部署平台 | Zeabur | — |
| 建置工具 | Vite | 7.x |

---

## 2. 系統邊界與使用者角色

### 2.1 使用者角色

```
┌─────────────────────────────────────────────────────────┐
│                    Personal Manager                      │
│                                                         │
│  ┌──────────────┐           ┌──────────────────────┐   │
│  │   訪客        │           │   網站擁有者（Admin）  │   │
│  │  (匿名用戶)   │           │   (已登入)            │   │
│  │              │           │                      │   │
│  │ • 瀏覽公開頁  │           │ • 管理所有內容        │   │
│  │ • 留言       │           │ • 審核留言            │   │
│  │ • 查看部落格  │           │ • 追蹤工作/時間       │   │
│  │ • 聯絡       │           │ • 管理待辦事項        │   │
│  └──────────────┘           └──────────────────────┘   │
└─────────────────────────────────────────────────────────┘
```

### 2.2 系統上下文圖

```
        ┌──────────────┐
        │   瀏覽器      │
        └──────┬───────┘
               │ HTTPS
               ▼
  ┌────────────────────────┐
  │  Vue 3 前端 SPA         │   ← Zeabur Static Hosting
  │  http://localhost:5173 │
  └────────────┬───────────┘
               │ REST API / JSON
               ▼
  ┌────────────────────────┐
  │  .NET 9 Web API        │   ← Zeabur Container
  │  http://localhost:5037 │
  └────────────┬───────────┘
               │ EF Core
               ▼
  ┌────────────────────────┐
  │  MariaDB               │   ← Zeabur MariaDB
  └────────────────────────┘
```

---

## 3. 功能規格

### 3.1 功能模組總覽

| 模組 | 公開頁面 | 管理後台 | 登入需求 |
|------|---------|---------|---------|
| 個人展示 | ✅ `/@:username` | ✅ `/admin/profile` | 僅後台 |
| 學經歷 | ✅ `/@:username/experience` | ✅ `/admin/experience` | 僅後台 |
| 技能 | ✅ `/@:username/skills` | ✅ `/admin/skills` | 僅後台 |
| 作品集 | ✅ `/@:username/portfolio` | ✅ `/admin/projects` | 僅後台 |
| 部落格 | ✅ `/@:username/blog` | ✅ `/admin/blog` | 僅後台 |
| 行事曆 | ✅ `/@:username/calendar` | ✅ `/admin/calendar` | 僅後台 |
| 留言板 | ✅ `/@:username/guestbook` | ✅ `/admin/comments` | 僅後台 |
| 聯絡方式 | ✅ `/@:username/contact` | ✅ `/admin/contacts` | 僅後台 |
| 待辦事項 | ❌ | ✅ `/admin/tasks` | 是 |
| 工作追蹤 | ❌ | ✅ `/admin/work-tracking` | 是 |
| 檔案管理 | ❌ | ✅ `/admin/files` | 是 |
| 用戶目錄 | ✅ `/`（首頁） | ❌ | 否 |

### 3.2 公開展示模組功能需求

#### FR-01：用戶目錄（首頁）
- 顯示所有已啟用用戶的個人卡片（格狀佈局）
- 支援依姓名搜尋
- 每個卡片顯示：大頭照、姓名、職稱、所在地、主題色
- 點擊卡片導向 `/@:username`

#### FR-02：個人展示頁面（`/@:username`）
- 顯示個人基本資料、介紹、大頭照
- 支援 5 種預設主題色（blue/green/purple/rose/slate）
- 水平導覽列切換各子頁面

#### FR-03：學經歷頁面（`/@:username/experience`）
- 顯示教育背景（時間軸格式）
- 顯示工作經歷（時間軸格式）
- 僅顯示 `isPublic: true` 的資料

#### FR-04：技能頁面（`/@:username/skills`）
- 顯示技能分類（分群顯示）
- 每項技能顯示等級（Beginner/Intermediate/Advanced/Expert）
- 僅顯示 `isPublic: true` 的資料

#### FR-05：作品集頁面（`/@:username/portfolio`）
- 顯示所有公開作品（格狀卡片）
- 精選作品優先顯示
- 支援點擊進入作品詳情頁
- 詳情頁顯示：描述、技術標籤、連結、附件下載

#### FR-06：部落格（`/@:username/blog`）
- 顯示所有已發布文章（`status: "Published"`）
- 支援 Tag 篩選
- 文章詳情支援 Markdown 與 HTML 渲染
- 記錄文章瀏覽次數（viewCount）

#### FR-07：行事曆（`/@:username/calendar`）
- 顯示公開行事曆事件（`isPublic: true`）
- 月曆格狀視圖

#### FR-08：留言板（`/@:username/guestbook`）
- 訪客可提交留言（name, email, message）
- 僅顯示已審核的留言（`isApproved: true`）
- 顯示管理者回覆（`adminReply`）

#### FR-09：聯絡方式（`/@:username/contact`）
- 顯示所有公開聯絡方式（`isPublic: true`）
- 支援類型：Email、電話、社群媒體、網站等
- 提供可點擊的直接聯絡按鈕

### 3.3 管理後台模組功能需求

#### FR-10：認證
- 使用者名稱 + 密碼登入
- JWT Token 自動注入所有 API 請求
- Token 過期自動登出並導向登入頁
- 登入狀態瀏覽器重整後保持

#### FR-11：個人資料管理（`/admin/profile`）
- 編輯基本資訊（姓名、職稱、簡介、地點）
- 上傳大頭照
- 選擇主題色（5 種預設）

#### FR-12：內容管理（通用 CRUD）
所有以下模組均支援：新增、編輯、刪除、排序、公開/私人切換：
- 學歷（Education）
- 工作經歷（WorkExperience）
- 技能（Skill）
- 作品集（Portfolio）+ 附件管理
- 行事曆事件（CalendarEvent）
- 聯絡方式（ContactMethod）

#### FR-13：部落格管理（`/admin/blog`）
- TipTap 富文本編輯器（圖片、格式、連結）
- 文章狀態：草稿（Draft）→ 發布（Published）→ 封存（Archived）
- 分類管理（批次操作）
- Tag 管理
- 文章複製功能
- 預覽連結

#### FR-14：留言管理（`/admin/comments`）
- 審核待審留言（approve/reject）
- 撰寫管理員回覆（adminReply）
- 依狀態篩選

#### FR-15：待辦事項（`/admin/tasks`）
- 多視圖：列表、Kanban、格狀
- 優先級：Low / Medium / High
- 狀態：Pending → InProgress → Completed
- 截止日期設定

#### FR-16：工作追蹤（`/admin/work-tracking`）
四大模塊：
1. **專案管理**：建立/重命名/刪除工作專案
2. **工作任務**：任務 CRUD + 狀態（7 種）+ 優先級（4 種）+ 預估/實際工時
3. **時間表**：時間記錄（localStorage 持久化，後端無對應 API）
4. **報告**：工作統計報告

#### FR-17：檔案管理（`/admin/files`）
- 上傳檔案（含檔案類型與大小驗證）
- 瀏覽已上傳檔案列表
- 刪除檔案
- FilePicker 元件：可在作品集/部落格中選取已上傳圖片

---

## 4. 系統架構

### 4.1 整體架構圖

```
┌─────────────────────────────────────────────────────────────────┐
│                     前端（Vue 3 SPA）                             │
│                                                                 │
│  ┌─────────────┐  ┌─────────────┐  ┌────────────────────────┐  │
│  │ 公開頁面     │  │ 管理後台    │  │ 首頁（用戶目錄）         │  │
│  │ /@:username │  │ /admin/*   │  │ /                      │  │
│  └──────┬──────┘  └──────┬─────┘  └───────────┬────────────┘  │
│         │                │                    │               │
│  ┌──────┴────────────────┴────────────────────┴────────────┐  │
│  │              Pinia Store（11 個）                        │  │
│  │  auth / user / profile / skill / portfolio / blog / ... │  │
│  └──────────────────────────┬───────────────────────────────┘  │
│                             │                                  │
│  ┌──────────────────────────┴───────────────────────────────┐  │
│  │         API Service 層（16 個服務）                       │  │
│  │         HttpService（Axios + JWT 攔截器）                 │  │
│  └──────────────────────────┬───────────────────────────────┘  │
└─────────────────────────────┼───────────────────────────────────┘
                              │ REST API（JSON）
                              │ Authorization: Bearer {JWT}
┌─────────────────────────────┼───────────────────────────────────┐
│                     後端（.NET 9 Web API）                       │
│                             │                                  │
│  ┌──────────────────────────┴───────────────────────────────┐  │
│  │  ErrorHandlingMiddleware → JWT Auth → CORS               │  │
│  └──────────────────────────┬───────────────────────────────┘  │
│                             │                                  │
│  ┌──────────────────────────┴───────────────────────────────┐  │
│  │                Controllers（15 個）                       │  │
│  └──────────────────────────┬───────────────────────────────┘  │
│                             │                                  │
│  ┌──────────────────────────┴───────────────────────────────┐  │
│  │           Service 層（14 個，繼承 CrudService<T>）         │  │
│  └──────────────────────────┬───────────────────────────────┘  │
│                             │                                  │
│  ┌──────────────────────────┴───────────────────────────────┐  │
│  │              Repository 層（IRepository<T>）              │  │
│  │       ┌─────────────────┬──────────────────┐            │  │
│  │       │ EfRepository<T> │ JsonRepository<T>│            │  │
│  │       │  (EF Core)      │  (本地 JSON)     │            │  │
│  │       └────────┬────────┴────────┬─────────┘            │  │
│  └────────────────┼─────────────────┼──────────────────────┘  │
└───────────────────┼─────────────────┼──────────────────────────┘
                    │                 │（fallback，無 DB 時）
             ┌──────┴──────┐    ┌─────┴──────┐
             │  MariaDB    │    │ JSON 檔案  │
             │  (Zeabur)   │    │ Data/*.json│
             └─────────────┘    └────────────┘
```

### 4.2 啟動流程

```
dotnet run
  │
  ├─ 1. 讀取 appsettings.json / 環境變數
  ├─ 2. 嘗試連線 MariaDB（ConnectionStrings.DefaultConnection）
  │     ├─ 成功 → 使用 EfRepository
  │     │         EnsureCreated() 建立 schema
  │     │         DatabaseSeeder 植入初始資料
  │     └─ 失敗 → 自動 fallback → 使用 JsonRepository
  │               載入 Data/JsonData/*.json
  ├─ 3. 設定 JWT 認證中間件
  ├─ 4. 設定 CORS（允許前端 origin）
  ├─ 5. 設定 ErrorHandlingMiddleware
  └─ 6. 啟動 HTTP 監聽（port 5037）
```

---

## 5. 後端架構規格

### 5.1 專案結構

```
PersonalManagerBackend/
├── Controllers/          # 15 個 API 控制器
├── Services/             # 14 個業務邏輯服務
│   ├── CrudService.cs    # 通用 CRUD 基礎類別
│   └── EntityServices.cs # 各實體服務
├── Models/               # 14 個 EF Core 實體
├── DTOs/                 # 每實體 3 層 DTO（Create/Update/Response）
├── Repositories/
│   ├── IRepository.cs    # 倉庫介面
│   ├── EfRepository.cs   # EF Core 實作
│   └── JsonRepository.cs # JSON 檔案實作
├── Data/
│   ├── AppDbContext.cs   # EF Core DB Context
│   ├── DatabaseSeeder.cs # 初始資料植入
│   └── JsonData/         # 14 個 JSON fallback 檔案
├── Middleware/
│   └── ErrorHandlingMiddleware.cs
├── Mappings/
│   └── MappingExtensions.cs  # 手動 DTO 映射
└── Program.cs            # 應用程式進入點與 DI 設定
```

### 5.2 控制器規格

所有控制器遵守以下規範：
- 路由：`[Route("api/[controller]")]`
- 回應格式：`ApiResponse<T>` 統一包裝
- 錯誤格式：`{ success: false, message: "...", errors: [...] }`
- 認證：需認證端點加 `[Authorize]` 屬性

| 控制器 | 路由前綴 | 公開端點 | 認證端點 |
|--------|---------|---------|---------|
| AuthController | `/api/auth` | POST `/login`, `/register` | GET `/me` |
| UsersController | `/api/users` | GET `/public`, `/public/{username}` | CRUD（需 Admin） |
| ProfilesController | `/api/profiles` | GET `/directory` | GET/PUT（owner） |
| EducationsController | `/api/educations` | GET `/user/{userId}/public` | CRUD |
| WorkExperiencesController | `/api/workexperiences` | GET `/user/{userId}/public` | CRUD |
| SkillsController | `/api/skills` | GET `/user/{userId}/public` | CRUD |
| PortfoliosController | `/api/portfolios` | GET `/user/{userId}/public`, `/featured` | CRUD |
| CalendarEventsController | `/api/calendarevents` | GET `/user/{userId}/public` | CRUD |
| TodoItemsController | `/api/todoitems` | — | 全部需認證 |
| WorkTasksController | `/api/worktasks` | — | 全部需認證 |
| BlogPostsController | `/api/blogposts` | GET `/user/{userId}/published`, `/{slug}` | CRUD |
| GuestBookEntriesController | `/api/guestbookentries` | GET `/user/{userId}`, POST `/` | 審核需認證 |
| ContactMethodsController | `/api/contactmethods` | GET `/user/{userId}/public` | CRUD |
| FileUploadsController | `/api/fileuploads` | — | 全部需認證 |
| PortfolioAttachmentsController | `/api/portfolioattachments` | — | 全部需認證 |

### 5.3 Service 層架構

```csharp
// 通用 CRUD 基礎類別
public class CrudService<T> where T : class
{
    Task<T> GetByIdAsync(int id)
    Task<IEnumerable<T>> GetAllAsync()
    Task<T> CreateAsync(T entity)
    Task<T> UpdateAsync(T entity)
    Task<bool> DeleteAsync(int id)
}

// 各實體服務繼承 CrudService
public class BlogPostService : CrudService<BlogPost>
{
    // 額外業務邏輯：依 slug 查詢、發布狀態篩選、Tag 篩選等
}
```

### 5.4 Repository 模式

```csharp
public interface IRepository<T>
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(int id);
    IQueryable<T> Query();  // 支援複雜 LINQ 查詢
}

// EF Core 實作（連線 MariaDB 時）
public class EfRepository<T> : IRepository<T>

// JSON 檔案實作（無 DB 時 fallback）
public class JsonRepository<T> : IRepository<T>
```

### 5.5 統一回應格式

```json
// 成功
{
  "success": true,
  "message": "操作成功",
  "data": { /* 實際資料 */ }
}

// 失敗
{
  "success": false,
  "message": "錯誤說明",
  "errors": ["詳細錯誤 1", "詳細錯誤 2"]
}
```

### 5.6 JWT 認證規格

| 項目 | 值 |
|------|-----|
| 算法 | HS256 |
| 有效期 | 24 小時 |
| Issuer | `PersonalManagerAPI` |
| Audience | `PersonalManagerClient` |
| Payload Claims | `userId`, `username`, `email`, `role` |
| 傳遞方式 | HTTP Header: `Authorization: Bearer {token}` |

---

## 6. 前端架構規格

### 6.1 專案結構

```
PersonalManagerFrontend/
├── src/
│   ├── views/
│   │   ├── user/         # 10 個公開個人頁 View
│   │   ├── admin/        # 18 個管理後台 View
│   │   ├── HomeView.vue  # 用戶目錄首頁
│   │   └── LoginView.vue
│   ├── components/
│   │   ├── ui/           # 9 個基礎 UI 元件
│   │   ├── admin/        # 14 個後台專用元件
│   │   └── layout/       # AppHeader / AppFooter
│   ├── layouts/
│   │   ├── AdminLayout.vue   # 後台容器（側欄 + 頂列）
│   │   └── UserLayout.vue    # 個人頁容器（主題 + 導覽）
│   ├── stores/           # 11 個 Pinia Store
│   ├── services/         # 16 個 API 服務
│   ├── types/
│   │   ├── api.ts        # 所有 API 型別定義
│   │   └── experience.ts
│   ├── composables/
│   │   └── useTheme.ts   # 主題系統 composable
│   └── router/
│       └── index.ts      # 路由配置
└── vite.config.ts
```

### 6.2 路由架構

```
/                              → HomeView（用戶目錄）
/login                         → LoginView

/@:username                    → UserLayout（provide userId）
  /                            → UserAboutView
  /experience                  → UserExperienceView
  /skills                      → UserSkillView
  /portfolio                   → UserPortfolioView
    /:id                       → UserProjectDetailView
  /blog                        → UserBlogListView
    /:slug                     → UserBlogDetailView
  /calendar                    → UserCalendarView
  /guestbook                   → UserGuestbookView
  /contact                     → UserContactView

/admin                         → AdminLayout（路由守衛保護）
  /dashboard                   → DashboardView
  /profile                     → ProfileManageView
  /experience                  → ExperienceManageView
  /skills                      → SkillManageView
  /projects                    → ProjectManageView
  /calendar                    → CalendarManageView
  /work-tracking               → WorkTrackingView
  /tasks                       → TaskManageView
  /blog                        → BlogManageView
    /new                       → BlogEditorView
    /:id/edit                  → BlogEditorView
  /comments                    → CommentManageView
  /contacts                    → ContactManageView
  /files                       → FileManagerView
```

**路由守衛**：所有 `/admin/*` 路由在導航前檢查 `authStore.isAuthenticated`，未登入自動跳轉至 `/login`。

### 6.3 狀態管理（Pinia）

```typescript
// 所有 Store 遵守相同結構
export const useXxxStore = defineStore('xxx', () => {
  // State
  const items = ref<XxxType[]>([])
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  // Actions（async，呼叫對應 Service）
  async function fetchItems() { ... }
  async function createItem(dto) { ... }
  async function updateItem(id, dto) { ... }
  async function deleteItem(id) { ... }

  return { items, isLoading, error, fetchItems, ... }
}, {
  persist: true  // 僅 authStore 與 task.timeEntries 持久化
})
```

**持久化策略**：
- `authStore`：token + user_data 存入 localStorage（支援刷新頁面保持登入）
- `task.timeEntries`：時間記錄存入 localStorage（後端無對應 API）
- 其他 Store：不持久化，每次頁面載入重新 fetch

### 6.4 HTTP 服務層

```typescript
// 單例 Axios 實例
const httpService = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL  // http://localhost:5037/api
})

// 請求攔截器：自動加入 JWT
httpService.interceptors.request.use(config => {
  const token = localStorage.getItem('token')
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

// 回應攔截器：解包 ApiResponse<T>
httpService.interceptors.response.use(
  response => response.data.data,  // 直接回傳 .data 內容
  error => {
    if (error.response?.status === 401) {
      authStore.logout()
      router.push('/login')
    }
    return Promise.reject(error)
  }
)
```

### 6.5 UserLayout provide/inject 模式

```typescript
// UserLayout.vue（提供者）
const route = useRoute()
const { userId } = await userDirectoryService.getUserByUsername(route.params.username)
provide('userId', userId)

// 所有 User View（注入者）
const userId = inject<number>('userId')
```

此模式避免重複的 username → userId 解析，並讓子頁面可直接使用 userId 呼叫 API。

### 6.6 主題系統

```typescript
// 5 種預設主題色
type ThemeColor = 'blue' | 'green' | 'purple' | 'rose' | 'slate'

// useTheme composable 回傳 CSS 變數映射
const theme = useTheme(profile.themeColor)
// → { '--primary': '#3b82f6', '--primary-dark': '#2563eb', ... }

// UserLayout.vue 套用
<div :style="theme">
```

---

## 7. 資料模型規格

### 7.1 實體關係圖

```
User (1) ─────────────────────── (*) PersonalProfile
User (1) ─────────────────────── (*) Education
User (1) ─────────────────────── (*) WorkExperience
User (1) ─────────────────────── (*) Skill
User (1) ─────────────────────── (*) Portfolio
User (1) ─────────────────────── (*) CalendarEvent
User (1) ─────────────────────── (*) TodoItem
User (1) ─────────────────────── (*) WorkTask
User (1) ─────────────────────── (*) BlogPost
User (1) ─────────────────────── (*) GuestBookEntry (targetUserId)
User (1) ─────────────────────── (*) ContactMethod
Portfolio (1) ──────────────── (*) PortfolioAttachment
```

### 7.2 核心實體定義

#### User
| 欄位 | 型別 | 說明 |
|------|------|------|
| Id | int | PK |
| Username | string | 唯一，用於 URL（`@:username`） |
| Email | string | 唯一 |
| PasswordHash | string | BCrypt 雜湊 |
| FullName | string | 顯示名稱 |
| Role | string | "Admin" / "User" |
| IsActive | bool | 帳號啟用狀態 |
| CreatedAt | DateTime | 建立時間 |

#### PersonalProfile
| 欄位 | 型別 | 說明 |
|------|------|------|
| Id | int | PK |
| UserId | int | FK → User |
| Title | string | 職稱 / 角色標題 |
| Summary | string | 一行簡介 |
| Description | string | 詳細自我介紹 |
| ProfileImageUrl | string? | 大頭照 URL |
| Location | string? | 所在地 |
| ThemeColor | string | 主題色（blue/green/purple/rose/slate） |

#### BlogPost
| 欄位 | 型別 | 說明 |
|------|------|------|
| Id | int | PK |
| UserId | int | FK → User |
| Title | string | 文章標題 |
| Slug | string | 唯一，URL friendly |
| Content | string | 富文本內容（HTML） |
| Summary | string? | 文章摘要 |
| Category | string? | 分類 |
| Tags | string | 逗號分隔 tag 清單 |
| Status | BlogPostStatus | Draft / Published / Archived |
| IsPublic | bool | 公開可見 |
| ViewCount | int | 瀏覽次數 |
| PublishedAt | DateTime? | 發布時間 |

#### GuestBookEntry（多用戶支援）
| 欄位 | 型別 | 說明 |
|------|------|------|
| Id | int | PK |
| Name | string | 留言者姓名 |
| Email | string? | 留言者 Email |
| Message | string | 留言內容 |
| **TargetUserId** | int | FK → User（此留言屬於哪個用戶的留言板） |
| IsApproved | bool | 是否審核通過 |
| AdminReply | string? | 管理員回覆 |
| CreatedAt | DateTime | 留言時間 |

### 7.3 Enum 規格

```csharp
public enum BlogPostStatus   { Draft, Published, Archived }
public enum TodoStatus       { Pending, InProgress, Completed }
public enum TodoPriority     { Low, Medium, High }
public enum WorkTaskStatus   { Pending, Planning, InProgress, Testing, Completed, OnHold, Cancelled }
public enum WorkTaskPriority { Low, Medium, High, Urgent }
public enum SkillLevel       { Beginner, Intermediate, Advanced, Expert }
```

---

## 8. API 規格摘要

### 8.1 認證端點

| 方法 | 路徑 | 說明 | 認證 |
|------|------|------|------|
| POST | `/api/auth/login` | 登入，回傳 JWT | 否 |
| POST | `/api/auth/register` | 註冊新用戶 | 否 |
| GET | `/api/auth/me` | 取得當前登入用戶資訊 | 是 |

**登入請求 / 回應範例**：
```json
// POST /api/auth/login
Request: { "username": "john", "password": "secret" }

Response: {
  "success": true,
  "data": {
    "userId": 1,
    "username": "john",
    "email": "john@example.com",
    "fullName": "John Doe",
    "role": "Admin",
    "token": "eyJhbGci..."
  }
}
```

### 8.2 公開端點（無需認證）

```
GET /api/users/public                       # 所有公開用戶清單
GET /api/users/public/{username}            # 特定用戶公開資訊
GET /api/profiles/directory                 # 用戶目錄（含主題色）
GET /api/profiles/user/{userId}             # 特定用戶個人資料
GET /api/educations/user/{userId}/public    # 用戶公開學歷
GET /api/workexperiences/user/{userId}/public
GET /api/skills/user/{userId}/public
GET /api/portfolios/user/{userId}/public
GET /api/portfolios/user/{userId}/featured
GET /api/calendarevents/user/{userId}/public
GET /api/blogposts/user/{userId}/published
GET /api/blogposts/{slug}
GET /api/guestbookentries/user/{userId}     # 已審核留言
POST /api/guestbookentries                  # 提交留言
GET /api/contactmethods/user/{userId}/public
```

### 8.3 私人端點（需 JWT 認證）

所有私人端點遵守 RESTful 規範：
- `GET /api/{resource}` — 取得當前用戶的所有資源
- `GET /api/{resource}/{id}` — 取得單一資源
- `POST /api/{resource}` — 新增
- `PUT /api/{resource}/{id}` — 更新
- `DELETE /api/{resource}/{id}` — 刪除

額外私人端點：
```
GET /api/guestbookentries                   # 所有留言（含未審核）
PATCH /api/guestbookentries/{id}/approve    # 審核通過
PATCH /api/guestbookentries/{id}/reply      # 撰寫回覆
GET /api/fileuploads                        # 所有已上傳檔案
POST /api/fileuploads                       # 上傳檔案
DELETE /api/fileuploads/{id}
GET /api/portfolioattachments/{portfolioId}
POST /api/portfolioattachments
DELETE /api/portfolioattachments/{id}
```

---

## 9. 安全設計

### 9.1 認證與授權

| 機制 | 實作 |
|------|------|
| 認證方式 | JWT Bearer Token（HS256） |
| Token 有效期 | 24 小時 |
| 密碼儲存 | BCrypt 雜湊（BCrypt.Net-Next） |
| 路由保護（前端） | Vue Router beforeEach 守衛 |
| 路由保護（後端） | `[Authorize]` 屬性 + JWT 中間件 |

### 9.2 安全設定注意事項

- `appsettings.json` 中 `Jwt.SecretKey` 僅為占位符，**實際密鑰必須透過環境變數注入**
- 本地開發使用 `appsettings.Development.json`（已加入 `.gitignore`）
- 生產環境使用 Zeabur 環境變數：`Jwt__SecretKey`、`ConnectionStrings__DefaultConnection`

### 9.3 CORS 設定

後端允許來自前端 origin 的跨域請求，需在 Program.cs 中設定對應的允許 origin。

### 9.4 檔案上傳安全

- 副檔名白名單驗證
- 最大檔案大小：50MB（可透過 `FileStorage.MaxFileSizeMB` 調整）

---

## 10. 部署架構

### 10.1 Zeabur 部署設定

| 服務 | 類型 | 說明 |
|------|------|------|
| 前端 | Static Site | Vue 3 SPA |
| 後端 | Container | .NET 9 Web API |
| 資料庫 | MariaDB | Zeabur 管理服務 |

### 10.2 環境變數（後端）

| 變數名 | 說明 | 範例 |
|--------|------|------|
| `ConnectionStrings__DefaultConnection` | MariaDB 連線字串 | `Server=...;Database=...;User=...;Password=...` |
| `Jwt__SecretKey` | JWT 簽名密鑰（≥32 字元） | 隨機生成的安全字串 |
| `Jwt__Issuer` | JWT Issuer（可選） | `PersonalManagerAPI` |
| `Jwt__ExpiryHours` | Token 有效時數（可選） | `24` |

### 10.3 環境變數（前端）

| 變數名 | 說明 |
|--------|------|
| `VITE_API_BASE_URL` | 後端 API base URL |

### 10.4 本地開發快速啟動

```bash
# 後端（Terminal 1）
cd local-development/PersonalManagerBackend
dotnet run
# → API: http://localhost:5037
# → Swagger: http://localhost:5037/swagger

# 前端（Terminal 2）
cd local-development/PersonalManagerFrontend
npm install      # 首次或 package.json 有異動時
npm run dev
# → http://localhost:5173
```

> 若無 MariaDB，後端自動 fallback 至 `Data/JsonData/*.json` 本地資料

---

## 11. 已知限制與技術債

### 11.1 現有技術債

| ID | 描述 | 嚴重度 | 影響範圍 |
|----|------|--------|---------|
| TD-01 | `TimeEntry`（工作時間記錄）無後端 API，僅 localStorage 持久化 | 中 | 工作追蹤模組，清除瀏覽器資料會遺失 |
| TD-02 | 部分端點權限檢查不完整（僅驗證登入，未驗證資源所有權） | 高 | 所有 CRUD 端點 |
| TD-03 | `BlogPost.Tags` 以逗號字串儲存，非正規化 | 低 | 部落格 Tag 篩選 |
| TD-04 | 無 Refresh Token 機制，Token 過期後強制登出 | 中 | 認證流程 |
| TD-05 | `FileStorage.RootPath` 為本地路徑，生產環境需改為 CDN / Object Storage | 高 | 檔案上傳 |

### 11.2 功能限制

- 目前系統設計為**單一擁有者**（一個帳號管理整個網站），雖有多用戶架構，但無用戶自助註冊流程
- 部落格編輯器不支援 Markdown 輸入模式（已移除，僅支援 TipTap WYSIWYG）
- 行事曆無重複事件（recurring events）支援

---

## 12. 架構決策紀錄（ADR）

### ADR-001：資料庫 JSON Fallback
**決策**：後端啟動時自動偵測 DB 連線，失敗自動 fallback 至本地 JSON
**原因**：簡化本地開發環境建立，開發者不需安裝 MariaDB
**取捨**：JSON 實作不支援複雜查詢，功能有限

### ADR-002：`/@:username` URL 架構
**決策**：個人頁面採用類 GitHub 的 `/@:username` 路由格式
**原因**：直覺、易記，不與其他路由衝突（`@` 前綴區隔）
**取捨**：URL 含特殊字元，部分環境可能需要特殊處理

### ADR-003：三倉庫策略
**決策**：主專案、後端、前端各用獨立 Git 倉庫
**原因**：各服務可獨立部署與版本控制，符合微服務精神
**取捨**：本地開發需 clone 三個倉庫，協調成本略高

### ADR-004：JWT 24h 有效期（無 Refresh Token）
**決策**：JWT 有效期 24 小時，過期後強制重新登入
**原因**：簡化實作，個人管理工具使用頻率已足夠
**取捨**：長時間工作可能被強制中斷，日後可加入 Refresh Token

### ADR-005：TipTap WYSIWYG 取代純 Markdown
**決策**：部落格編輯器採用 TipTap，移除 Markdown 模式
**原因**：所見即所得，降低寫作門檻；支援圖片拖放
**取捨**：內容儲存為 HTML，Markdown 用戶可能不習慣

### ADR-006：主題系統使用 CSS 變數
**決策**：5 套主題透過 CSS 變數動態切換，而非多套 CSS 類別
**原因**：與 TailwindCSS 整合簡單，UserLayout 一個 `:style` 綁定即可套用
**取捨**：主題選項固定為 5 種，擴充需修改 composable

### ADR-007：Repository + Service 雙層架構
**決策**：明確區分 Repository（資料存取）與 Service（業務邏輯）層
**原因**：支援 EF/JSON 雙 Repository 實作切換；業務邏輯測試不依賴 DB
**取捨**：對簡單 CRUD 操作略為冗餘，增加檔案數量

---

*文件維護：每次重大架構異動後更新對應章節，並記錄於 CLAUDE.md「最新異動記錄」區塊*
