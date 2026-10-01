# 開發指南

本地開發的完整流程。架構與設計決策見 [`system-specification.md`](system-specification.md)，
各端的程式寫法見 [`backend/CLAUDE.md`](../backend/CLAUDE.md) 與 [`frontend/CLAUDE.md`](../frontend/CLAUDE.md)。

## 需要的工具

| 工具 | 版本 | 用途 |
| --- | --- | --- |
| .NET SDK | 9.0 | 後端 |
| Node.js | 20.19+ 或 22.12+ | 前端 |
| Google Chrome | 任一近期版本 | 執行 E2E |

不需要安裝資料庫：後端預設使用 SQLite，第一次啟動時自動建立並寫入示範資料。

## 第一次設定

```bash
git clone https://github.com/hn83320589/personal_manager.git
cd personal_manager

cd backend
dotnet tool restore            # 安裝鎖定版本的 dotnet-ef
cd ../frontend
npm install
```

## 啟動

```bash
# 終端 1：後端（http://localhost:5037，Swagger：/swagger）
cd backend
dotnet run --project src/PersonalManager.Api

# 終端 2：前端（http://localhost:5173）
cd frontend
npm run dev
```

- 開啟 `http://localhost:5173/@admin` 看公開頁面；`http://localhost:5173/login` 以 `admin` / `password123` 登入後台
- 前端的 `/api` 與 `/files` 由 Vite 轉送到後端，瀏覽器看到的是同一個網站，refresh token cookie 與上傳的圖片才能正常運作
- 示範資料只在 Development 環境建立

### 重設本機資料

停止後端，刪除 `backend/src/PersonalManager.Api/App_Data/`（資料庫）與 `files/`（上傳的檔案），再重新啟動。兩者都不會被提交。

## 測試與檢查

專案沒有 CI，提交前請在本機執行這些檢查，全部通過才提交。

```bash
# 後端
cd backend
dotnet test PersonalManager.sln

# 兩組 migration 都已產生（在 backend/src/PersonalManager.Api）
dotnet ef migrations has-pending-model-changes --context SqliteApplicationDbContext
dotnet ef migrations has-pending-model-changes --context MySqlApplicationDbContext

# 前端
cd frontend
npm run lint           # ESLint
npm run format:check   # Prettier（npm run format 可自動修正）
npx vitest run         # 單元測試
npm run api:types      # 重新產生 API 型別，git diff 應該沒有變化
npm run build          # 型別檢查 + 建置
npx playwright test    # E2E：自動啟動後端與前端，使用暫存的 SQLite
```

測試的寫法與原則見兩份 CLAUDE.md 的「測試原則」。

## 常見工作

### 修改資料表

1. 修改 `Models/` 與 `ApplicationDbContext`
2. 在 `backend/src/PersonalManager.Api` 為兩種資料庫各產生一組 migration：

   ```bash
   dotnet ef migrations add <名稱> --context SqliteApplicationDbContext --output-dir Migrations/Sqlite
   dotnet ef migrations add <名稱> --context MySqlApplicationDbContext --output-dir Migrations/MySql
   ```

3. 檢查產生的內容：EF 可能把「刪除一個欄位、新增另一個」誤判為改名；以字串儲存的 enum 新欄位需要合法的預設值；
   會轉換既有資料的 migration 要補上 `DataMigrationTests`
4. 啟動時會自動套用 migration

### 修改 API

1. 依 `Features/<名稱>/` 的寫法修改後端，補上整合測試
2. 更新 OpenAPI 文件並重新產生前端型別：

   ```bash
   cd backend
   UPDATE_OPENAPI=1 dotnet test PersonalManager.sln --filter OpenApiDocument
   cd ../frontend
   npm run api:types
   ```

   沒有更新 `openapi.json` 時，後端的 `OpenApiDocumentTests` 會失敗

### 新增後台清單頁

參考 `frontend/src/views/manage/SkillsView.vue`：`useOwnedList` 提供清單與寫入，`ManageList` 顯示可排序的列，
`SidePanel` 為編輯面板。

## Git

- 以可運作的小單位 commit；訊息說明「為什麼」，格式 `<type>(<scope>): <說明>`，例如 `fix(backend): …`
- 不使用 `--no-verify`，不停用測試
- 重要的功能或架構變更記錄在 [`CHANGELOG.md`](../CHANGELOG.md)，進行中的工作與技術債在 [`TASKS.md`](TASKS.md)

## 疑難排解

| 狀況 | 處理 |
| --- | --- |
| 後端啟動出現 `Roll Forward` 相關錯誤 | 安裝 .NET 9 SDK；或暫時以 `DOTNET_ROLL_FORWARD=LatestMajor` 執行 |
| 前端登入後重新整理就被登出 | 確認是透過 `http://localhost:5173` 開啟（Vite 轉送 `/api`），不要直接呼叫 5037 |
| 上傳的圖片在前端顯示不出來 | 同上，`/files` 也需要經由 Vite 轉送 |
| 登入時顯示「操作太頻繁」 | 登入每分鐘限 10 次；可在 `appsettings.Development.json` 調整 `RateLimiting:AuthPermitsPerMinute` |
| `dotnet ef` 找不到 | 在 `backend/` 執行 `dotnet tool restore` |
| 本機 E2E 找不到瀏覽器 | 安裝 Google Chrome，或執行 `npx playwright install chromium` |
