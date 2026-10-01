# Personal Manager — 後端

.NET 9 Web API。專案說明與文件索引見[根目錄 README](../README.md)。

## 執行

```bash
cd backend
dotnet run --project src/PersonalManager.Api
```

- API：`http://localhost:5037`
- Swagger：`http://localhost:5037/swagger`（僅 Development）
- 預設使用 SQLite（`src/PersonalManager.Api/App_Data/`），首次啟動自動套用 migration 並建立示範資料（帳號 `admin` / `password123`）

本機開發需要一份 `src/PersonalManager.Api/appsettings.Development.json`（不提交），至少設定 JWT 金鑰。範例見 [`CLAUDE.md`](CLAUDE.md#設定檔架構)。

## 測試

```bash
dotnet test PersonalManager.sln
```

整合測試以 WebApplicationFactory 啟動 API，每個 `ApiFactory` 使用獨立的暫存 SQLite 檔案。

## 結構

```
src/PersonalManager.Api/
├── Features/<名稱>/    # 各功能的 Controller、Service、DTO
├── Common/            # 共用的例外、目前使用者、分頁、HTML 清洗…
├── Models/ Data/      # EF Core 實體與 DbContext
├── Migrations/        # Sqlite 與 MySql 各一組
└── Setup/             # DI 與 middleware 設定
tests/PersonalManager.Tests/
openapi.json           # API 定義（測試確保與程式一致，前端由此產生型別）
```

## 延伸閱讀

- 寫法、設定與 API 路由：[`CLAUDE.md`](CLAUDE.md)
- 修改資料表或 API 的步驟：[`docs/development-guide.md`](../docs/development-guide.md)
- 正式環境的設定：[`docs/deployment-guide.md`](../docs/deployment-guide.md)
