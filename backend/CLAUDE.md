# CLAUDE.md - PersonalManager Backend

This file provides guidance to Claude Code when working with the backend codebase.

---

## 給 AI 的指示

每次任務完成時：
1. 在主專案的 `docs/TASKS.md` 勾選對應項目的 checkbox
2. 有新的技術債時，加入主專案 `docs/TASKS.md` 的「技術債」區塊
3. 做了未預期的架構決策時，記錄到主專案的 `docs/system-specification.md` §12 架構決策紀錄（ADR），附上原因
4. 回報更新了哪些檔案與區塊

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
- **Schema 由 EF Core Migrations 管理**：啟動時 `db.Database.Migrate()` 自動套用。兩種 provider 各有一組 migration，Model 異動後兩組都要產生（在 `src/PersonalManager.Api` 下執行）：
  ```bash
  dotnet ef migrations add <Name> --context SqliteApplicationDbContext --output-dir Migrations/Sqlite
  dotnet ef migrations add <Name> --context MySqlApplicationDbContext --output-dir Migrations/MySql
  ```
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
  → Service（業務邏輯，使用 DTO）
  → IRepository<T> → EfRepository<T>（重構中：Phase 2 將改為 service 直接使用 DbContext，見 ADR-011）
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

## 專案結構

```
backend/
├── PersonalManager.sln
├── tests/PersonalManager.Tests/  # xUnit
└── src/PersonalManager.Api/      # 以下皆位於此目錄
├── Program.cs                    # 進入點：只串接 Setup/ 裡的註冊與 pipeline
├── appsettings.json              # 設定（包含 DB 連線字串與 JWT）
├── appsettings.Development.json  # 開發環境補充設定
├── PersonalManager.Api.csproj    # 專案檔
│
├── Setup/                        # 服務註冊與 middleware pipeline（依關注點分檔）
│   ├── ApiSetup.cs               # Controller、JSON、Swagger、CORS、流量限制、健康檢查
│   ├── InfrastructureSetup.cs    # 寄信、檔案儲存
│   ├── ApplicationSetup.cs       # 業務 service 與 ICurrentUser
│   └── PipelineSetup.cs          # 資料庫初始化、middleware 順序
│
├── Common/
│   ├── AppExceptions.cs          # NotFound／Forbidden／Unauthenticated／Conflict／DomainValidation
│   └── CurrentUser.cs            # ICurrentUser：service 取得目前登入者
│
├── Auth/
│   ├── AuthService.cs            # JWT token 產生、使用者驗證
│   ├── JwtSettings.cs            # JWT 設定 model（無預設金鑰）
│   └── JwtSetup.cs               # 金鑰驗證與 JWT 驗證註冊
│
├── Controllers/                  # API 控制器（皆繼承 BaseApiController）
│   ├── BaseApiController.cs      # GetCurrentUserId() 等共用 helper
│   ├── AuthController.cs         # /api/auth（login、register、me、refresh、logout、密碼重設）
│   ├── UsersController.cs        # /api/users
│   ├── ProfilesController.cs     # /api/profiles
│   ├── EducationsController.cs   # /api/educations
│   ├── WorkExperiencesController.cs # /api/workexperiences
│   ├── SkillsController.cs       # /api/skills
│   ├── PortfoliosController.cs   # /api/portfolios
│   ├── PortfolioAttachmentsController.cs # /api/portfolioattachments
│   ├── CalendarEventsController.cs  # /api/calendarevents
│   ├── TodoItemsController.cs    # /api/todoitems
│   ├── WorkTasksController.cs    # /api/worktasks
│   ├── ProjectsController.cs     # /api/projects
│   ├── TimeEntriesController.cs  # /api/timeentries
│   ├── BlogPostsController.cs    # /api/blogposts
│   ├── GuestBookEntriesController.cs # /api/guestbookentries
│   ├── ContactMethodsController.cs  # /api/contactmethods
│   └── FileUploadsController.cs  # /api/fileuploads
│
├── DTOs/
│   ├── ApiResponse.cs            # 統一回應格式 ApiResponse<T>
│   ├── AuthDtos.cs               # LoginRequest, RegisterRequest, AuthResponse
│   └── EntityDtos.cs             # 所有實體的 Create/Update/Response DTOs
│
├── Data/
│   ├── ApplicationDbContext.cs   # EF Core 資料模型
│   ├── ProviderDbContexts.cs     # Sqlite／MySql 子類別與 design-time factory
│   ├── PersistenceSetup.cs       # AddPersistence()：依設定選擇 provider
│   └── DatabaseSeeder.cs         # 初始資料種子（索引由 ApplicationDbContext 定義）
│
├── Mappings/
│   └── MappingExtensions.cs      # Model ↔ DTO 手動映射擴展方法
│
├── Middleware/
│   └── ErrorHandlingMiddleware.cs # AppException → 對應狀態碼；其他例外 → 500 通用訊息（不外洩細節）
│
├── Migrations/Sqlite、Migrations/MySql # 各 provider 的 migration（啟動時自動套用）
│
├── Models/                       # EF Core 實體（以資料夾內容為準）
│
├── Repositories/
│   ├── IRepository.cs            # 通用 CRUD 介面
│   ├── EfRepository.cs           # EF Core 實作（以 open generic 註冊）
│   └── BlogPostRepository.cs     # BlogPost 專用（含 Tag 關聯同步）
│
├── Services/
│   ├── CrudService.cs            # 通用 CRUD 業務邏輯基礎類別
│   ├── EntityServices.cs         # 各實體的具體服務類別
│   ├── EmailService.cs           # 寄信（SMTP / 未設定時 NoOp）
│   ├── FileStorageProviders.cs   # 檔案儲存（本地 / S3 相容 Object Storage）
│   └── DbHealthCheck.cs          # DB 連線健康檢查
│
└── Settings/                     # EmailSettings、FileStorageSettings
```

---

## API 路由總覽

| Controller | 路由前綴 | 說明 |
|------------|----------|------|
| AuthController | `/api/auth` | 登入、註冊、refresh、登出、密碼重設 |
| UsersController | `/api/users` | 使用者管理 |
| ProfilesController | `/api/profiles` | 個人資料 |
| EducationsController | `/api/educations` | 學歷 |
| WorkExperiencesController | `/api/workexperiences` | 工作經歷 |
| PublicSkillsController／MySkillsController | `/api/public/users/{username}/skills`、`/api/me/skills` | 技能（已重建） |
| PortfoliosController | `/api/portfolios` | 作品集 |
| CalendarEventsController | `/api/calendarevents` | 行事曆 |
| TodoItemsController | `/api/todoitems` | 待辦事項 |
| WorkTasksController | `/api/worktasks` | 工作追蹤 |
| BlogPostsController | `/api/blogposts` | 部落格文章 |
| GuestBookEntriesController | `/api/guestbookentries` | 留言板 |
| ContactMethodsController | `/api/contactmethods` | 聯絡方式 |
| ProjectsController | `/api/projects` | 工作追蹤專案 |
| TimeEntriesController | `/api/timeentries` | 時間記錄 |
| FileUploadsController | `/api/fileuploads` | 檔案上傳 |
| PortfolioAttachmentsController | `/api/portfolioattachments` | 作品集附件 |

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
| Zeabur 環境變數 | — | 生產環境覆寫，格式為雙底線（`__`）分隔層級 |

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
    "ExpiryHours": 24
  }
}
```

### `appsettings.Development.json`（gitignored，本地自行建立）

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=personal_manager;User=...;Password=...;"
  },
  "Jwt": {
    "SecretKey": "your_local_secret_key_at_least_32_chars"
  }
}
```

### 生產環境（Zeabur 環境變數）

```
ConnectionStrings__DefaultConnection = <MariaDB 連線字串>
Jwt__SecretKey = <隨機密鑰>
```

- JWT 設定區段名稱為 `Jwt`（非 `JwtSettings`）
- **只在 Development 發生的行為**：示範資料 seeder（含 `admin/password123`）、Swagger、未設定時預設允許 `localhost:5173`／`4173` 的 CORS
- **`Cors:AllowedOrigins`**：正式環境以 `Cors__AllowedOrigins__0` 等環境變數設定前端網址；`appsettings.json` 刻意留空陣列，避免依索引合併時殘留 localhost
- **`Jwt:SecretKey` 在啟動時驗證**（`Auth/JwtSetup.cs`）：需至少 32 字元且不可為占位字串。Development 未設定時會產生臨時金鑰（重啟後需重新登入）；其他環境未設定則拒絕啟動。程式碼中沒有預設金鑰
- `DefaultConnection` 為空字串且 provider 為 Sqlite 時，使用 `App_Data/personal_manager.db`

---

## Feature 寫法（ADR-011，重構中）

已重建的 feature 放在 `Features/<名稱>/`，範本為 `Features/Skills/`。尚未重建的仍是下方「如何新增一個實體」的舊寫法，重建完成後舊寫法會移除。

| 項目 | 規則 |
|------|------|
| 路由 | 公開頁面 `api/public/users/{username}/<資源>`（`[AllowAnonymous]`）；後台 `api/me/<資源>`（`[Authorize]`）；管理員 `api/admin/<資源>` |
| 資料範圍 | 後台查詢一律 `.OwnedBy(currentUser.RequireUserId())`；公開查詢先 `db.RequirePublicUserIdAsync(username)` 再篩選公開資料 |
| 存取別人的資料 | 查不到 → `NotFoundException`（404），不回 403 |
| Service | 直接使用 `ApplicationDbContext` 與 `ICurrentUser`；讀取加 `AsNoTracking()`，以 `Select` 投影成 DTO |
| DTO | `record`：`SaveXxxRequest`（新增與更新共用，含 DataAnnotations）、`XxxDto`（後台）、`PublicXxxDto`（公開，不含管理欄位） |
| 錯誤 | 丟 `AppException` 子類別；驗證失敗由 `[ApiController]` 自動回 400 + `ApiResponse` |
| 排序 | `PUT api/me/<資源>/order`，body 為 `ReorderRequest`，必須剛好包含自己的全部項目 |
| 實體 | 屬於使用者的實體實作 `IOwnedByUser` |
| 測試 | `tests/.../Features/<名稱>ApiTests.cs`，以 `ApiFactory` + `CreateUserAsync()` 打真實 HTTP；至少涵蓋匿名存取、存取他人資料、公開資料過濾 |

## 如何新增一個實體（舊寫法，重建中）

1. **新增 Model**（`Models/NewEntity.cs`）：
   ```csharp
   public class NewEntity
   {
       public int Id { get; set; }
       public int UserId { get; set; }
       public string Name { get; set; } = string.Empty;
       public DateTime CreatedAt { get; set; }
       public DateTime UpdatedAt { get; set; }
   }
   ```

2. **新增 DTO**（`DTOs/EntityDtos.cs`）：
   ```csharp
   public class CreateNewEntityDto { ... }
   public class UpdateNewEntityDto { ... }
   public class NewEntityResponseDto { ... }
   ```

3. **新增映射**（`Mappings/MappingExtensions.cs`）：
   ```csharp
   public static NewEntityResponseDto ToResponseDto(this NewEntity entity) { ... }
   public static NewEntity ToModel(this CreateNewEntityDto dto) { ... }
   ```

4. **新增 DbSet**（`Data/ApplicationDbContext.cs`）：
   ```csharp
   public DbSet<NewEntity> NewEntities { get; set; }
   ```

5. **新增 Service**（`Services/EntityServices.cs`）：
   ```csharp
   public interface INewEntityService : ICrudService<NewEntity, CreateNewEntityDto, UpdateNewEntityDto, NewEntityResponseDto> { }
   public class NewEntityService : CrudService<NewEntity, CreateNewEntityDto, UpdateNewEntityDto, NewEntityResponseDto>, INewEntityService
   {
       public NewEntityService(IRepository<NewEntity> repo) : base(repo) { }
       protected override NewEntityResponseDto ToResponseDto(NewEntity entity) => entity.ToResponseDto();
       protected override NewEntity ToModel(CreateNewEntityDto dto) => dto.ToModel();
       protected override void UpdateModel(NewEntity entity, UpdateNewEntityDto dto) => dto.ApplyTo(entity);
   }
   ```

6. **在 `Setup/ApplicationSetup.cs` 註冊 Service**（repository 已由 open generic 自動註冊）：
   ```csharp
   builder.Services.AddScoped<INewEntityService, NewEntityService>();
   ```

7. **新增 Controller**（`Controllers/NewEntitiesController.cs`）：
   參考 `SkillsController.cs` 的結構，繼承 `BaseApiController`，注入 `INewEntityService`。
   以 `GetCurrentUserId()`（回傳 `int?`）取得目前使用者：Create 時強制寫入 `dto.UserId`，Update/Delete 先驗證資源擁有者。

8. **新增 migration**：Sqlite 與 MySql 各產生一次（指令見「注意事項」），並 commit 產生的檔案。

---

## 開發注意事項

- **永遠不要使用 `--no-verify`** 繞過 commit hooks
- **不要 disable 測試**，修復它
- **commit 前先確認 `dotnet build` 通過**
- **Model 異動後**需新增 migration（`dotnet ef migrations add <Name>`），並 commit `Migrations/` 下產生的檔案

---

## 最新異動記錄

### 2026/03/16（密碼重設功能）
- **密碼重設功能實作完成**：
  - 新增 `Models/PasswordResetToken.cs`（Id、UserId、Token、ExpiresAt、IsUsed、CreatedAt）
  - 新增 `Settings/EmailSettings.cs`（SmtpHost、SmtpPort、UseSsl、Username、Password、FromAddress、FromName、FrontendBaseUrl、`IsConfigured` 計算屬性）
  - 新增 `Services/EmailService.cs`：`IEmailService` 介面 + `SmtpEmailService`（System.Net.Mail.SmtpClient）+ `NoOpEmailService`（SMTP 未設定時 log 至 Warning）
  - 新增 `Data/JsonData/PasswordResetTokens.json`（空陣列）
  - `DTOs/AuthDtos.cs`：新增 `ForgotPasswordRequest`（`[EmailAddress]`）、`ResetPasswordRequest`（`[StringLength(100, MinimumLength = 6)]`）
  - `Data/ApplicationDbContext.cs`：新增 `DbSet<PasswordResetToken>`
  - `Auth/AuthService.cs`：介面加 `ForgotPasswordAsync`/`ResetPasswordAsync`；實作注入 `IRepository<PasswordResetToken>` + `IEmailService`；`ForgotPasswordAsync` 防枚舉攻擊（user 不存在也回 true）；token 使用 URL-safe Base64（`+→-`、`/→_`、去除 `=`）；`ResetPasswordAsync` 驗證 `!IsUsed && ExpiresAt > UtcNow` 後 BCrypt 重設密碼並標記 token 已使用
  - `Controllers/AuthController.cs`：注入 `IOptions<EmailSettings>`；新增 `POST /api/auth/forgot-password`（rate-limited "auth"，永遠回 200）；`POST /api/auth/reset-password`（rate-limited "auth"）
  - `appsettings.json`：新增 `Email` section（空占位符，生產填 Zeabur 環境變數 `Email__SmtpHost` 等）
  - `Program.cs`：`Configure<EmailSettings>`；讀取 `Email` config 判斷 IsConfigured → `SmtpEmailService`（SMTP 模式）或 `NoOpEmailService`（NoOp 模式）；EF + JSON 兩個區塊各加 `IRepository<PasswordResetToken>`
  - EF Migration：`AddPasswordResetToken`（新增 `PasswordResetTokens` 資料表）
  - **Zeabur 生產環境變數**（SMTP 才需要設定）：`Email__SmtpHost`、`Email__SmtpPort`、`Email__UseSsl`、`Email__Username`、`Email__Password`、`Email__FromAddress`、`Email__FrontendBaseUrl`

### 2026/03/16（DB Schema 正規化）
- **三項 DB Schema 正規化完成**：
  - **CalendarEvent.RecurrenceRule**：`Models/CalendarEvent.cs` 新增 `RecurrenceRule` 欄位；DTOs、Mappings 同步更新
  - **WorkTask.Project 正規化**：
    - 新增 `Models/Project.cs`；`WorkTask.Project` (string) 改為 `WorkTask.ProjectId` (int?)
    - `Data/ApplicationDbContext.cs`：`DbSet<Project>` + FK 設定（DeleteBehavior.SetNull）
    - 新增 `Services/EntityServices.cs`：`IProjectService`/`ProjectService`
    - 新增 `Controllers/ProjectsController.cs`（GET/POST/PUT/DELETE，全部需 auth）
    - `Controllers/WorkTasksController.cs`：`GetByProject` 端點改為 `GET .../project/{projectId:int}`
    - 新增 `Data/JsonData/Projects.json`（空陣列）
    - `Program.cs`：DI 新增 `IProjectService`、`IRepository<Project>`（EF + JSON 兩區塊）
  - **BlogPost.Tags 正規化**：
    - 新增 `Models/Tag.cs`；`BlogPost` 新增 `ICollection<Tag> TagEntities`（JsonIgnore），保留 `string Tags` 供 JSON fallback
    - `Data/ApplicationDbContext.cs`：`DbSet<Tag>` + BlogPost↔Tag 多對多 `UsingEntity("BlogPostTags")`
    - 新增 `Repositories/BlogPostRepository.cs`（覆寫 GetAll/GetById/FindAsync 以 Include TagEntities）+ `SyncTagsAsync`
    - `Repositories/EfRepository.cs`：`GetAllAsync/GetByIdAsync/FindAsync` 改為 `virtual`
    - `Services/EntityServices.cs`：`BlogPostService` 在 Create/Update 時呼叫 `SyncTagsAsync`；`WorkTaskService` 批次載入 Project 名稱避免 N+1
    - `Program.cs`：DB 模式改用 `BlogPostRepository`（實作 `IRepository<BlogPost>`）；JSON 模式用 `JsonRepository<BlogPost>`
    - 新增 `Data/JsonData/Tags.json`（空陣列）
  - **EF Migration**：`20260316024850_AddRecurrenceRuleProjectTag`
    - `CalendarEvents` 新增 `RecurrenceRule` 欄位
    - `WorkTasks` drop `Project` (string)、新增 `ProjectId` (int?) + FK
    - 新增 `Projects` 資料表
    - 新增 `Tags` 資料表
    - 新增 `BlogPostTags` 多對多 join 資料表
  - **BlogPostsController** tag 相關端點更新（tags 現為 `List<string>`，不需 Split）
  - **DatabaseSeeder** 移除 `WorkTask.Project` 字串欄位

### 2026/03/16（單元測試）
- **後端單元測試專案建立**：
  - 新增 `tests/PersonalManager.Tests.csproj`（xUnit 2.9.3 + Moq 4.20.72，引用主專案）
  - 新增 `tests/GlobalUsings.cs`（`global using Xunit`）
  - `tests/AuthServiceTests.cs`：10 個測試覆蓋 `LoginAsync`（有效/錯誤密碼/用戶不存在）、`RegisterAsync`（新用戶/重複用戶名）、`RefreshAsync`（有效/過期/已撤銷/不存在）、`RevokeAsync`（有效/不存在）
  - `tests/BlogPostServiceTests.cs`：10 個測試覆蓋 `GetPublicByUserIdAsync`、`GetBySlugAsync`、`GetPublicPagedAsync`（分頁/關鍵字/分類/標籤過濾）、`IncrementViewCountAsync`
  - `tests/GuestBookEntryServiceTests.cs`：9 個測試覆蓋 `GetApprovedByTargetUserIdAsync`（過濾/排序）、`GetApprovedPagedAsync`（分頁/排除未審核/空結果）
  - 共 29 個測試，全部通過（`dotnet test`）
  - 執行：`cd tests && dotnet test`

### 2026/03/16（SEO + Object Storage）
- **Object Storage 抽象層（TD-05）**：
  - 新增 NuGet `AWSSDK.S3` v3.7.414.3
  - `Settings/FileStorageSettings.cs` 新增 `S3StorageSettings`（BucketName、ServiceUrl、AccessKey、SecretKey、PublicBaseUrl、ForcePathStyle）
  - 新增 `Services/FileStorageProviders.cs`：`IFileStorageProvider` 介面 + `LocalFileStorageProvider`（現有本地行為）+ `S3FileStorageProvider`（S3-compatible）
  - `Services/EntityServices.cs`：`FileUploadService` 改注入 `IFileStorageProvider`，移除直接 File I/O 邏輯
  - `Program.cs`：啟動時讀取 `FileStorage:S3` 設定 → 若 IsConfigured 則用 S3，否則用本地磁碟
  - `appsettings.json`：新增 `FileStorage.S3` section（空占位符）
  - **Zeabur 生產環境變數**：`FileStorage__S3__BucketName`、`FileStorage__S3__ServiceUrl`（e.g. `https://xxxx.s3.us-east-1.amazonaws.com`）、`FileStorage__S3__AccessKey`、`FileStorage__S3__SecretKey`、`FileStorage__S3__PublicBaseUrl`（公開存取 URL）

### 2026/03/16（分頁 + 搜尋）
- **分頁（Pagination）**：
  - `DTOs/ApiResponse.cs` 新增 `PagedResult<T>`（Items、TotalCount、Page、PageSize、TotalPages、HasPreviousPage、HasNextPage）
  - `IBlogPostService` / `BlogPostService` 新增 `GetPublicPagedAsync(userId, page, pageSize, keyword?, tag?, category?)`
  - `IGuestBookEntryService` / `GuestBookEntryService` 新增 `GetApprovedPagedAsync(targetUserId, page, pageSize)`
  - `BlogPostsController` 新增 `GET /api/blogposts/user/{userId}/categories` 端點
  - `BlogPostsController` 新增 `GET /api/blogposts/user/{userId}/public/paged?page=1&pageSize=8&keyword=&tag=&category=` 端點
  - `GuestBookEntriesController` 新增 `GET /api/guestbookentries/user/{targetUserId}/paged?page=1&pageSize=10` 端點

### 2026/03/16
- **Refresh Token 機制（TD-04）**：
  - 新增 `Models/RefreshToken.cs`（UserId、Token、ExpiresAt、IsRevoked）
  - `DTOs/AuthDtos.cs`：`AuthResponse` 加 `RefreshToken`/`RefreshTokenExpiresAt`；新增 `RefreshRequest`
  - `Data/ApplicationDbContext.cs` 加 `DbSet<RefreshToken>`
  - `Auth/AuthService.cs`：`GenerateAuthResponse` 改為 async，登入/註冊時生成並持久化 refresh token（7 天）；新增 `RefreshAsync`（換新 token pair）、`RevokeAsync`
  - `Controllers/AuthController.cs`：新增 `POST /api/auth/refresh`（無需認證）、`POST /api/auth/logout`（撤銷 refresh token）
  - `dotnet ef migrations add AddRefreshToken` — 新增 migration
- **TimeEntry API 實作（TD-01）**：
  - 新增 `Models/TimeEntry.cs`（UserId、WorkTaskId?、Task、Project、Date、StartTime?、EndTime?、Duration、Description）
  - `DTOs/EntityDtos.cs` 新增 Create/Update/Response DTOs
  - `Mappings/MappingExtensions.cs` 新增 TimeEntry 映射
  - `Data/ApplicationDbContext.cs` 新增 `DbSet<TimeEntry>`
  - `Data/JsonData/TimeEntries.json` 新增（空陣列，JSON fallback 用）
  - `Services/EntityServices.cs` 新增 `ITimeEntryService`/`TimeEntryService`
  - `Program.cs` 新增 DI 注冊（EF + JSON 兩個區塊）
  - 新增 `Controllers/TimeEntriesController.cs`（全部端點需 auth，所有權驗證）
  - `dotnet ef migrations add AddTimeEntry` — 新增 migration
- **文章瀏覽計數（ViewCount）**：
  - `IBlogPostService` / `BlogPostService` 新增 `IncrementViewCountAsync(int id)` 方法
  - `BlogPostsController` 新增 `POST /api/blogposts/{id}/view` 公開端點（無需認證）

### 2026/03/13
- **EF Core Migrations 策略遷移**：
  - 新增 `Data/DesignTimeDbContextFactory.cs` — 讓 `dotnet ef` 工具在 design-time 能建立 DbContext（無需真實 DB 連線）
  - 新增 `Migrations/` 目錄：`20260313084338_InitialCreate.cs`（完整初始 schema）
  - `Program.cs`：`db.Database.EnsureCreated()` 改為 `db.Database.Migrate()`
  - **⚠️ 生產 DB 切換步驟**（Zeabur 已有資料的情況）：
    ```sql
    -- 登入生產 DB，執行以下 SQL 標記 migration 已套用（不重新建表）
    CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
        `MigrationId` varchar(150) NOT NULL,
        `ProductVersion` varchar(32) NOT NULL,
        PRIMARY KEY (`MigrationId`)
    );
    INSERT INTO `__EFMigrationsHistory` VALUES ('20260313084338_InitialCreate', '9.0.13');
    ```
  - 往後新增欄位：`dotnet ef migrations add <名稱>` → `dotnet ef database update`（或 Zeabur deploy 時自動 migrate）
- **公開端點 Rate Limiting**：
  - `Program.cs` 加入 `AddRateLimiter`（.NET 內建 `Microsoft.AspNetCore.RateLimiting`）
  - 兩個 policy：`"auth"`（login/register）、`"public_write"`（guestbook POST），均為每 IP 每分鐘 10 次 fixed window
  - 超過限制回傳 HTTP 429
  - `AuthController.Login`、`AuthController.Register` 加 `[EnableRateLimiting("auth")]`
  - `GuestBookEntriesController.Create` 加 `[EnableRateLimiting("public_write")]`
  - `app.UseRateLimiter()` 加入 middleware pipeline（在 `UseAuthentication` 之前）
- **資源所有權驗證（TD-02）**：
  - 新增 `Controllers/BaseApiController.cs` — 抽象基底類別，提供 `GetCurrentUserId()` 從 JWT claim 取得當前用戶 ID
  - 所有需要身份驗證的 Controller 改繼承 `BaseApiController`（替代 `ControllerBase`）
  - 修復 `TodoItemsController.GetAll()`、`WorkTasksController.GetAll()`：原本回傳所有用戶資料，現在只回傳當前用戶自己的資料
  - 以下 Controller 的 Create/Update/Delete 全部加入所有權驗證：
    - `SkillsController`、`EducationsController`、`WorkExperiencesController`、`PortfoliosController`
    - `CalendarEventsController`、`BlogPostsController`、`TodoItemsController`、`WorkTasksController`
    - `ContactMethodsController`、`ProfilesController`
  - `GuestBookEntriesController`：Update/Delete 驗證 `TargetUserId == currentUserId`；`GetAll` 改為只回傳當前用戶 guestbook 的所有留言（含未審核）
  - Create 端點：強制 `dto.UserId = currentUserId.Value`，不信任 client 傳入的 UserId

### 2026/03/12
- **檔案管理系統**：
  - 新增 `Settings/FileStorageSettings.cs`（RootPath、MaxFileSizeMB、AllowedExtensions）
  - `appsettings.json` 加入 `FileStorage` section
  - `Program.cs` 加入 `Configure<FileStorageSettings>`、`UseStaticFiles`（`/files` path）、EF/JSON repository 及 service 注冊
  - 新增 `Models/FileUpload.cs`、`Models/PortfolioAttachment.cs`
  - `Data/ApplicationDbContext.cs` 加入兩個新 DbSet
  - `DTOs/EntityDtos.cs` 新增 `FileUploadResponse`、`CreatePortfolioAttachmentDto`、`PortfolioAttachmentResponse`
  - `Mappings/MappingExtensions.cs` 新增 FileUpload、PortfolioAttachment 映射
  - `Services/EntityServices.cs` 新增 `IFileUploadService`/`FileUploadService`（含上傳驗證）、`IPortfolioAttachmentService`/`PortfolioAttachmentService`
  - 新增 `Controllers/FileUploadsController.cs`（GET/POST/DELETE，需 auth）
  - 新增 `Controllers/PortfolioAttachmentsController.cs`（GET 公開，POST/DELETE 需 auth）
  - 新增 `Data/JsonData/FileUploads.json`、`Data/JsonData/PortfolioAttachments.json`（空陣列）
- **部落格標籤端點**：
  - `Controllers/BlogPostsController.cs` `GetByUserId` 加入 `?tag=` query 過濾
  - 新增 `GET /api/blogposts/user/{userId}/tags` 端點（回傳所有已用標籤）

### 2026/02/23
- **多使用者架構補強**：
  - `PersonalProfile` 加入 `ThemeColor` 欄位（預設 `"blue"`）
  - `GuestBookEntry` 加入 `TargetUserId` 欄位（目標用戶ID）
  - `DTOs/EntityDtos.cs` 新增 `PublicUserDto`、`ProfileDirectoryDto`，更新相關 DTO
  - `Mappings/MappingExtensions.cs` 更新 PersonalProfile 和 GuestBookEntry 映射
  - `GET /api/users/public` — 公開用戶列表（無需認證）
  - `GET /api/users/public/{username}` — 依 username 查詢用戶（無需認證）
  - `GET /api/profiles/directory` — 合併 Profile + User 的目錄清單（無需認證）
  - `GET /api/guestbookentries/user/{targetUserId}` — 查詢特定用戶的已審核留言
  - JSON fallback 資料更新：`personalProfiles.json` 加 `themeColor`，`guestBookEntries.json` 加 `targetUserId`
  - **DB 模式注意**：schema 有異動，需 DROP DB 後重建（EnsureCreated）或執行 ALTER TABLE
- **AuthResponse UserId 修復**：
  - `DTOs/AuthDtos.cs` — `AuthResponse` 新增 `public int UserId { get; set; }` 欄位
  - `Auth/AuthService.cs` — `GenerateAuthResponse()` 加入 `UserId = user.Id`，確保登入/註冊回應包含 userId
- **Blog publishedAt 修復**：
  - `DTOs/EntityDtos.cs` — `UpdateBlogPostDto` 新增 `public DateTime? PublishedAt { get; set; }` 欄位
  - `Mappings/MappingExtensions.cs` — `BlogPost.ApplyUpdate()` 加入 `if (d.PublishedAt.HasValue) b.PublishedAt = d.PublishedAt`

### 2026/02/22
- **資安修復：移除 credentials 洩露**：
  - `appsettings.json` 改為只含占位符（空連線字串 + 假密鑰）
  - `appsettings.Development.json` 移入 `.gitignore`，存放本地真實密鑰
  - 生產環境改用 Zeabur 環境變數（`ConnectionStrings__DefaultConnection`、`Jwt__SecretKey`）
- **JSON 序列化修復**：`PropertyNamingPolicy` 從 `null`（PascalCase）改為 `CamelCase`
- **DB 自動偵測 fallback**：啟動時探測 DB，失敗自動使用本地 JSON 資料
- **移除 `sql/` 資料夾**：由 EF Core `EnsureCreated()` + `DatabaseSeeder.cs` 取代
- **前後端欄位對齊驗證**：所有 12 個實體的 DTO 欄位與前端 TypeScript 介面確認一致
