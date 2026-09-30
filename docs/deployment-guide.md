# 部署指南

**目前沒有正式環境。** 原本使用的 Zeabur 已停止使用，新的平台尚未決定；開發一律在本機進行（見 [`development-guide.md`](development-guide.md)）。
本文件列出日後部署時必須滿足的條件，不綁定特定平台。

## 必要條件

### 1. 前端與 API 在同一個網站

refresh token 以 `SameSite=Strict` 的 httpOnly cookie 傳遞（ADR-010），前端與 API 必須屬於同一個 site（相同的註冊網域）。
最簡單的做法是同一個網址、以反向代理分流：

| 路徑 | 轉送到 |
| --- | --- |
| `/api/*` | 後端 |
| `/files/*` | 後端（使用本機檔案儲存時） |
| 其他 | 前端建置結果（`frontend/dist`），找不到的路徑回傳 `index.html`（SPA） |

前端的 `VITE_API_BASE_URL` 維持 `/api`。前後端分開網域（例如 `app.example.com` 與 `api.example.com`）時仍屬同一個 site，
需另外設定 `Cors__AllowedOrigins__0`。

### 2. HTTPS

refresh cookie 設有 `Secure`，正式環境必須使用 HTTPS。

### 3. 後端設定（環境變數）

| 變數 | 必要 | 說明 |
| --- | --- | --- |
| `ASPNETCORE_ENVIRONMENT` | 是 | `Production`（不會建立示範資料、不開放 Swagger） |
| `Jwt__SecretKey` | 是 | 至少 32 字元的隨機字串；未設定時後端拒絕啟動 |
| `Database__Provider` | 否 | `Sqlite`（預設）或 `MySql` |
| `ConnectionStrings__DefaultConnection` | MySQL 時必要 | 連線字串；SQLite 留空時使用 `App_Data/personal_manager.db` |
| `Admin__BootstrapEmails__0` | 建議 | 以此 Email 註冊的帳號直接成為管理員 |
| `Cors__AllowedOrigins__0` | 前後端不同網址時 | 前端網址 |
| `FileStorage__S3__*` | 否 | 使用 S3 相容的物件儲存時設定（`BucketName`、`ServiceUrl`、`AccessKey`、`SecretKey`、`PublicBaseUrl`） |
| `Email__SmtpHost`、`Email__FromAddress` 等 | 否 | 寄送重設密碼信；未設定時只記錄在 log |
| `Email__FrontendBaseUrl` | 寄信時必要 | 重設密碼連結使用的網址，預設為 `http://localhost:5173` |
| `RateLimiting__*PermitsPerMinute` | 否 | 流量限制（見 `backend/CLAUDE.md`） |

### 4. 資料庫

- 啟動時自動套用 migration（兩種資料庫各有一組）
- 使用 SQLite 時，`App_Data/` 必須放在持久化的磁碟上
- 從 SQLite 換到 MySQL 不會搬移資料，需另行匯出匯入

### 5. 上傳的檔案

預設存在後端的 `files/`，需要持久化的磁碟；多台主機或無狀態平台請改用 S3 相容儲存。

## 建置

```bash
# 後端
cd backend
dotnet publish src/PersonalManager.Api -c Release -o out

# 前端
cd frontend
npm ci
npm run build          # 產生 dist/
```

## 部署後檢查

- `GET /api/public/users` 回傳 200
- 以管理員帳號登入後重新整理頁面，仍保持登入（確認 cookie 設定正確）
- 上傳一張圖片並在前台看得到（確認 `/files` 或物件儲存設定）
