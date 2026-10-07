# CLAUDE.md - PersonalManager Backend

This file provides guidance to Claude Code when working with the backend codebase.

共通規則（給 AI 的指示、不可以動的東西、測試原則）見根目錄 [`CLAUDE.md`](../CLAUDE.md)。

---

## 快速啟動

```bash
# 在 backend/ 目錄下
dotnet run --project src/PersonalManager.Api   # 啟動 API
dotnet test PersonalManager.sln                # 執行測試

# API 服務預設跑在:
# http://localhost:5037
# Swagger UI: http://localhost:5037/swagger
```

### 注意事項

- **.NET SDK 版本**：需要 .NET 9.0。若出現 `Roll Forward` 錯誤，執行：
  ```bash
  DOTNET_ROLL_FORWARD=LatestMajor dotnet run --project src/PersonalManager.Api
  ```
- **資料庫**：由 `Database:Provider` 決定，預設 `Sqlite`，資料庫檔在 `src/PersonalManager.Api/App_Data/personal_manager.db`（git-ignored）。改為 `MySql` 時必須設定 `ConnectionStrings:DefaultConnection`。設定值無法辨識時會直接中止啟動。
- **Schema 由 EF Core Migrations 管理**：啟動時自動套用。兩種 provider 各有一組 migration，Model 異動後兩組都要產生，提交前以 `dotnet ef migrations has-pending-model-changes --context <兩種 context>` 檢查（`dotnet-ef` 版本鎖在 `.config/dotnet-tools.json`，首次請先 `dotnet tool restore`；在 `src/PersonalManager.Api` 下執行）：
  ```bash
  dotnet ef migrations add <Name> --context SqliteApplicationDbContext --output-dir Migrations/Sqlite
  dotnet ef migrations add <Name> --context MySqlApplicationDbContext --output-dir Migrations/MySql
  ```
- **產生 migration 後要檢查內容**：一個欄位刪除、另一個同型別欄位新增時，EF 可能判斷成「改名」而沿用舊資料（例如 `IsPublic` → `ReadingMinutes`），需手動改成刪除後新增。
  會轉換既有資料的 migration，請在 `tests/.../Data/DataMigrationTests.cs` 補上「遷移到前一版 → 寫入舊資料 → 遷移到最新版」的測試。
  新增「以字串儲存的 enum」且不可為 null 的欄位時，EF 產生的預設值是空字串，既有資料讀取時會無法轉換，需改成合法的 enum 值（例如 `"None"`）。
- **重設本地資料庫**：刪除 `App_Data/` 後重新啟動即可。

---

## 架構說明

### 技術棧

| 項目 | 版本/工具 |
|------|-----------|
| 框架 | .NET 9.0 Web API |
| ORM | Entity Framework Core 9（Sqlite provider + Pomelo MySQL provider） |
| 資料庫 | SQLite（預設）/ MySQL、MariaDB（`Database:Provider` 切換） |
| 認證 | JWT Bearer Token |
| API 文件 | Swagger / OpenAPI |
| 密碼雜湊 | BCrypt.Net-Next |

### 請求流程

```
HTTP 請求
  → ErrorHandlingMiddleware（統一錯誤處理）
  → JWT 認證/授權
  → Controller（接收請求，回傳 ApiResponse<T>）
  → Feature Service（業務邏輯，直接使用 ApplicationDbContext，見 ADR-011）
```

### 資料庫設定（`Data/PersistenceSetup.cs`）

- `AddPersistence()` 依 `Database:Provider` 註冊 `SqliteApplicationDbContext` 或 `MySqlApplicationDbContext`，兩者都以 `ApplicationDbContext` 注入使用
- 兩個子類別只負責區分 migration 目錄，模型定義全部在 `ApplicationDbContext`
- SQLite 的相對路徑一律以專案根目錄為基準

### JSON 序列化規則

- **camelCase**：所有 JSON 欄位名稱使用 camelCase（`PropertyNamingPolicy.CamelCase`）
- **Enum 為字串**：Enum 序列化為字串（如 `"Published"`、`"Expert"`）
- 前端 TypeScript 介面與此完全對應

---

## 專案結構

```
backend/
├── PersonalManager.sln
├── tests/PersonalManager.Tests/  # xUnit
└── src/PersonalManager.Api/      # 以下皆位於此目錄
├── Program.cs                    # 進入點：只串接 Setup/ 裡的註冊與 pipeline
├── appsettings.json              # 設定（包含 DB 連線字串與 JWT）
├── appsettings.Development.json  # 選用的本機設定（不提交）
├── PersonalManager.Api.csproj    # 專案檔
│
├── Setup/                        # 服務註冊與 middleware pipeline（依關注點分檔）
│   ├── ApiSetup.cs               # Controller、JSON、Swagger、CORS、流量限制、健康檢查
│   ├── InfrastructureSetup.cs    # 寄信、檔案儲存
│   ├── ApplicationSetup.cs       # 業務 service 與 ICurrentUser
│   └── PipelineSetup.cs          # 資料庫初始化、middleware 順序
│
├── Common/
│   ├── AppExceptions.cs          # NotFound／Unauthenticated／Conflict／DomainValidation
│   ├── CurrentUser.cs            # ICurrentUser：service 取得目前登入者
│   ├── QueryExtensions.cs        # OwnedBy()、RequirePublicUserIdAsync()
│   ├── Reordering.cs Paging.cs   # 排序、分頁
│   ├── RichTextSanitizer.cs      # 富文本清洗（HtmlSanitizer）
│   ├── EmbedProviders.cs         # 嵌入白名單（與前端 lib/embeds.ts 一致）
│   └── Slugs.cs                  # 網址代稱
│
├── Auth/
│   ├── JwtSettings.cs            # JWT 設定 model（無預設金鑰）
│   └── JwtSetup.cs               # 金鑰驗證與 JWT 驗證註冊（登入流程在 Features/Auth）
│
├── Features/<名稱>/               # 各功能的 Controller、Service、DTO（見下方「Feature 寫法」）
│
├── DTOs/
│   └── ApiResponse.cs            # 統一回應格式 ApiResponse<T>、PagedResult<T>
│
├── Data/
│   ├── ApplicationDbContext.cs   # EF Core 資料模型
│   ├── ProviderDbContexts.cs     # Sqlite／MySql 子類別與 design-time factory
│   ├── PersistenceSetup.cs       # AddPersistence()：依設定選擇 provider
│   ├── JsonColumns.cs            # 值物件清單以 JSON 文字儲存（作品內容）
│   └── DatabaseSeeder.cs         # 初始資料種子（索引由 ApplicationDbContext 定義）
│
├── Middleware/
│   └── ErrorHandlingMiddleware.cs # AppException → 對應狀態碼；其他例外 → 500 通用訊息（不外洩細節）
│
├── Migrations/Sqlite、Migrations/MySql # 各 provider 的 migration（啟動時自動套用）
│
├── Models/                       # EF Core 實體（以資料夾內容為準）
│
├── Services/
│   ├── EmailService.cs           # 寄信（SMTP / 未設定時 NoOp）
│   ├── FileStorageProviders.cs   # 檔案儲存（本地 / S3 相容 Object Storage）
│   └── DbHealthCheck.cs          # DB 連線健康檢查
│
└── Settings/                     # AdminSettings、EmailSettings、FileStorageSettings
```

---

## API 路由總覽

| Controller | 路由前綴 | 說明 |
|------------|----------|------|
| AuthController／MyPasswordController | `/api/auth/login`、`register`、`refresh`、`logout`、`me`、`forgot-password`、`reset-password`；`/api/me/password` | 認證（已重建，ADR-010） |
| AdminUsersController | `/api/admin/users`（含 `{id}/status`、`{id}/role`） | 使用者管理（已重建，限 Admin） |
| PublicProfilesController／MyProfileController | `/api/public/users`（目錄）、`/api/public/users/{username}`、`/api/me/profile` | 個人資料與前台呈現設定（已重建） |
| PublicResumeController／MyEducationsController | `/api/public/users/{username}/educations`、`/api/me/educations` | 學歷（已重建） |
| PublicResumeController／MyWorkExperiencesController | `/api/public/users/{username}/work-experiences`、`/api/me/work-experiences` | 工作經歷（已重建） |
| PublicSkillsController／MySkillsController | `/api/public/users/{username}/skills`、`/api/me/skills` | 技能（已重建） |
| PublicPortfoliosController／MyPortfoliosController | `/api/public/users/{username}/portfolios`（卡片列表，`?category`、`?tag`；`facets`；`{slug}`）、`/api/me/portfolios`（含 `order`） | 作品集（已重建，區塊式內容，ADR-012） |
| PublicCalendarController／MyCalendarController | `/api/public/users/{username}/calendar?from&to`、`/api/me/calendar`（展開後的發生時間）、`/api/me/calendar/events` | 行事曆（已重建） |
| MyTodosController | `/api/me/todos` | 待辦事項（已重建） |
| MyWorkTasksController | `/api/me/work-tasks` | 工作任務（已重建，實際時數由時間紀錄加總） |
| PublicPostsController／MyPostsController／MyTagsController | `/api/public/users/{username}/posts`（含 `facets`、`{slug}/views`）、`/api/me/posts`、`/api/me/tags` | 部落格文章與標籤（已重建） |
| PublicGuestbookController／MyGuestbookController | `/api/public/users/{username}/guestbook`（GET、POST 限流）、`/api/me/guestbook`（含 `approval`、`reply`） | 留言板（已重建） |
| PublicContactMethodsController／MyContactMethodsController | `/api/public/users/{username}/contact-methods`、`/api/me/contact-methods` | 聯絡方式（已重建） |
| MyProjectsController | `/api/me/projects` | 工作追蹤專案（已重建） |
| MyTimeEntriesController | `/api/me/time-entries`（含 `summary`） | 時間紀錄（已重建） |
| MyFilesController | `/api/me/files` | 檔案上傳（已重建：副檔名與 magic bytes 須一致、伺服器判定 MIME、記錄圖片寬高） |

所有資料回應格式：
```json
{
  "success": true,
  "message": "...",
  "data": { ... },
  "errors": null
}
```

---

## 設定檔架構

| 檔案 | 提交至 git | 說明 |
|------|-----------|------|
| `appsettings.json` | ✅ 是 | 只含占位符與非機密設定，**不能放真實密碼** |
| `appsettings.Development.json` | ❌ 否（gitignored） | 本地開發的真實連線字串與密鑰 |
| 環境變數 | — | 正式環境覆寫設定，以雙底線（`__`）分隔層級，例如 `Jwt__SecretKey`（部署平台尚未決定，見 `docs/deployment-guide.md`） |

### `appsettings.json`（已提交，只有占位符）

```json
{
  "ConnectionStrings": {
    "DefaultConnection": ""
  },
  "Jwt": {
    "SecretKey": "",
    "Issuer": "PersonalManagerAPI",
    "Audience": "PersonalManagerClient",
    "AccessTokenMinutes": 15,
    "RefreshTokenDays": 14
  }
}
```

### `appsettings.Development.json`（gitignored，本地自行建立）

```json
{
  "Jwt": {
    "SecretKey": "your_local_secret_key_at_least_32_chars"
  }
}
```

本機開發通常不需要這個檔案：Development 未設定 JWT 金鑰時會產生臨時金鑰，資料庫預設為 SQLite。

### 正式環境（環境變數）

```
Jwt__SecretKey = <至少 32 字元的隨機字串>
Database__Provider = MySql                               # 使用 SQLite 時可省略
ConnectionStrings__DefaultConnection = <MySQL／MariaDB 連線字串>
Cors__AllowedOrigins__0 = https://<前端網址>              # 前後端同網址反向代理時可省略
Admin__BootstrapEmails__0 = <第一位管理員的 Email>
```

- JWT 設定區段名稱為 `Jwt`（非 `JwtSettings`）
- **第一位管理員**：`Admin:BootstrapEmails`（環境變數 `Admin__BootstrapEmails__0`）中的 Email 註冊後直接成為 Admin；正式環境不會執行示範資料 seeder
- **流量限制**（每個 IP 每分鐘）：`RateLimiting:AuthPermitsPerMinute`（登入、註冊、重設密碼，預設 10）、`RateLimiting:PublicWritePermitsPerMinute`（留言，預設 10）、`RateLimiting:SessionPermitsPerMinute`（refresh、logout，預設 60；前端每次開頁面都會 refresh，不能與登入共用額度）
- **只在 Development 發生的行為**：示範資料 seeder（含 `admin/password123`）、Swagger、未設定時預設允許 `localhost:5173`／`4173` 的 CORS
- **`Cors:AllowedOrigins`**：正式環境以 `Cors__AllowedOrigins__0` 等環境變數設定前端網址；`appsettings.json` 刻意留空陣列，避免依索引合併時殘留 localhost
- **認證（ADR-010）**：登入後回應本文只有 access token（預設 15 分鐘）；refresh token 以 `pm_refresh` cookie 傳遞（httpOnly、Secure、SameSite=Strict、Path=/api/auth，預設 14 天），資料庫只存 SHA-256 雜湊。每次 refresh 輪換；已撤銷的 token 被重用時撤銷該使用者全部工作階段。沒有帶 cookie 的 refresh 回 204（訪客開啟網站時的還原檢查，不是錯誤），cookie 無效或過期才回 401。前端與 API 必須同一個 site（相同的註冊網域），cookie 才會送出
- **`Jwt:SecretKey` 在啟動時驗證**（`Auth/JwtSetup.cs`）：需至少 32 字元且不可為占位字串。Development 未設定時會產生臨時金鑰（重啟後需重新登入）；其他環境未設定則拒絕啟動。程式碼中沒有預設金鑰
- `DefaultConnection` 為空字串且 provider 為 Sqlite 時，使用 `App_Data/personal_manager.db`

---

## Feature 寫法（ADR-011）

所有 feature 放在 `Features/<名稱>/`，範本為 `Features/Skills/`。新增資料表時，Model 放 `Models/`、在 `ApplicationDbContext` 加 `DbSet` 與索引，再產生兩組 migration（見「注意事項」）。

| 項目 | 規則 |
|------|------|
| 路由 | 公開頁面 `api/public/users/{username}/<資源>`（`[AllowAnonymous]`）；後台 `api/me/<資源>`（`[Authorize]`）；管理員 `api/admin/<資源>` |
| 資料範圍 | 後台查詢一律 `.OwnedBy(currentUser.RequireUserId())`；公開查詢先 `db.RequirePublicUserIdAsync(username)` 再篩選公開資料 |
| 存取別人的資料 | 查不到 → `NotFoundException`（404），不回 403 |
| Service | 直接使用 `ApplicationDbContext` 與 `ICurrentUser`；讀取加 `AsNoTracking()`，以 `Select` 投影成 DTO。排序、分頁必須在投影成 record DTO **之前**（EF 無法翻譯對建構子投影結果的排序）；需要關聯欄位時先投影成私有的 row class 再排序 |
| DTO | `record`：`SaveXxxRequest`（新增與更新共用，含 DataAnnotations）、`XxxDto`（後台）、`PublicXxxDto`（公開，不含管理欄位） |
| 錯誤 | 丟 `AppException` 子類別；驗證失敗由 `[ApiController]` 自動回 400 + `ApiResponse` |
| 排序 | 實體實作 `ISortable`；新增時 `NextPositionAsync()`，`PUT api/me/<資源>/order` 以 `ApplyOrder()` 套用 |
| 實體 | 屬於使用者的實體實作 `IOwnedByUser` |
| 測試 | `tests/.../Features/<名稱>ApiTests.cs`，以 `ApiFactory` + `CreateUserAsync()` 打真實 HTTP。清單型資源繼承 `OwnedCollectionContract` 取得共用的 9 個行為測試，只需另寫該資源特有的規則 |

## 開發注意事項

- **commit 前先確認 `dotnet build` 通過**
- **Model 異動後**需為兩種資料庫各新增一組 migration（指令見「注意事項」），並 commit `Migrations/` 下產生的檔案
