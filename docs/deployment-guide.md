# 部署指南

## 架構概覽

| 環境 | 後端 | 前端 | 資料庫 |
|------|------|------|--------|
| 本地開發 | http://localhost:5037 | http://localhost:5173 | JSON fallback（無需 DB） |
| 生產環境 | Zeabur（自動部署） | Zeabur（自動部署） | Zeabur MariaDB |

---

## 本地開發啟動

```bash
# 終端 1 — 後端
cd local-development/PersonalManagerBackend
dotnet run
# → http://localhost:5037
# → Swagger: http://localhost:5037/swagger

# 終端 2 — 前端
cd local-development/PersonalManagerFrontend
npm install   # 首次或 package.json 有變動
npm run dev
# → http://localhost:5173
```

> **注意**：後端若無法連接 MariaDB，會自動 fallback 至 `Data/JsonData/*.json`，不影響本地開發。啟動 log 會顯示 `啟動模式: 資料庫 (MariaDB)` 或 `啟動模式: JSON Fallback`。

---

## 環境變數設定

### 後端

| 檔案 | 提交 git | 用途 |
|------|---------|------|
| `appsettings.json` | ✅ | 只含占位符，**不放真實密碼** |
| `appsettings.Development.json` | ❌ gitignored | 本地真實連線字串與密鑰 |
| Zeabur 環境變數 | — | 生產環境覆寫 |

**`appsettings.Development.json`（本地自行建立）**：
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

**Zeabur 生產環境變數**（使用雙底線 `__` 分隔層級）：
```
ConnectionStrings__DefaultConnection = <MariaDB 連線字串>
Jwt__SecretKey                        = <隨機密鑰，至少 32 字元>
```

### 前端

**`.env.development`（本地，已存在，不提交）**：
```
VITE_API_BASE_URL=http://localhost:5037/api
VITE_APP_TITLE=Personal Manager
```

**Zeabur 生產環境變數**：
```
VITE_API_BASE_URL=https://your-backend.zeabur.app/api
```

---

## Zeabur 生產部署

### 部署方式

後端與前端各有獨立的 GitHub 倉庫，Zeabur 直接連接 GitHub repo 進行自動部署（**不使用 Docker**）。

```
hn83320589/PersonalManagerBackend  → Zeabur（.NET 自動偵測）
hn83320589/PersonalManagerFrontend → Zeabur（Node.js 自動偵測）
```

### 首次部署步驟

1. 在 Zeabur 建立 Project
2. 新增 MariaDB 服務，取得連線字串
3. 部署後端 → 設定 `ConnectionStrings__DefaultConnection` 和 `Jwt__SecretKey`
4. 部署前端 → 設定 `VITE_API_BASE_URL`（指向後端 URL）

### 每次更新

推送到 main branch 後 Zeabur 自動重新部署。

---

## 資料庫管理

- **不需要執行 migrations**：後端啟動時 EF Core `EnsureCreated()` 自動建立資料表
- **Schema 異動**（新增欄位等）：Zeabur 的 MariaDB 需手動執行 `ALTER TABLE`，或 DROP + 重建（會清空資料）
- **本地開發**：優先使用 JSON fallback，無需管理 DB schema

---

## 驗證部署

```bash
# 確認後端正常回應
curl https://your-backend.zeabur.app/api/profiles/directory

# 確認登入可用
curl -X POST https://your-backend.zeabur.app/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"..."}'
```

---

## 故障排除

| 問題 | 排查方式 |
|------|---------|
| 後端啟動顯示 `JSON Fallback` | 確認 `ConnectionStrings__DefaultConnection` 環境變數已設定 |
| 前端 API 請求 401 | 確認 `Jwt__SecretKey` 已設定且前後端使用相同密鑰 |
| 前端無法連線後端（CORS） | 確認後端 CORS 允許的 Origin 包含前端 URL |
| 前端顯示空白 | 確認 `VITE_API_BASE_URL` 指向正確後端網址 |
| DB 欄位缺失 | 參考 `Models/*.cs` 手動執行 `ALTER TABLE`，或 DROP DB 重建 |
