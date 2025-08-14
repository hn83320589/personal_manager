# 部署指南

## 概述

Personal Manager 系統的部署指南，包含開發、測試和生產環境的部署方式。

## 部署架構

### 開發環境 ✅ 已完成整合
- 後端: 本地 .NET Core (localhost:5253) - **運行中**
- 前端: Vite 開發伺服器 (localhost:5173) - **運行中** 
- 資料庫: JSON 模擬資料服務 (JsonDataService)
- **整合狀態**: ✅ CORS 設定完成，前後端API完全整合

### 開發環境啟動
```bash
# 啟動後端服務 (終端1)
cd PersonalManagerBackend
dotnet run
# 服務運行於 http://localhost:5253

# 啟動前端服務 (終端2)  
cd PersonalManagerFrontend
npm run dev
# 服務運行於 http://localhost:5173

# 驗證整合狀態
curl -X GET "http://localhost:5253/api/users"
```

### 整合測試結果
**API 端點驗證:**
```bash
✅ GET /api/users - 200 OK
✅ GET /api/skills - 200 OK  
✅ GET /api/personalprofiles - 200 OK
✅ CORS Headers: Access-Control-Allow-Origin: http://localhost:5173
✅ 前端 API 測試組件正常運作
```

**技術堆疊驗證:**
- ✅ Vue3 + TypeScript 前端建置成功
- ✅ .NET Core 後端服務穩定運行
- ✅ Axios HTTP 客戶端與後端完全整合
- ✅ Tailwind CSS 樣式系統運作正常
- ✅ 路由系統與認證機制完整

### 生產環境 (規劃中)
- 平台: Zeabur
- 容器化: Docker
- 資料庫: MariaDB (雲端服務)

## Docker 部署

### Docker Compose 設定

*待建立 docker-compose.yml*

### 建立映像檔

```bash
# 建立後端映像檔
cd PersonalManagerBackend
docker build -t personal-manager-backend .

# 建立前端映像檔  
cd PersonalManagerFrontend
docker build -t personal-manager-frontend .
```

### 執行容器

```bash
# 使用 Docker Compose
docker-compose up -d
```

## Zeabur 部署

### 準備工作

1. 建立 Zeabur 帳號
2. 連接 GitHub 倉庫
3. 設定環境變數

### 部署步驟

*待補充 - 將在部署階段完成*

## 環境變數設定

### 後端環境變數

```env
ASPNETCORE_ENVIRONMENT=Production
CONNECTION_STRINGS__DEFAULT=Server=xxx;Database=xxx;...
JWT_SECRET=your_jwt_secret
```

### 前端環境變數

```env
VITE_API_BASE_URL=https://your-api-domain.com/api
```

## 資料庫遷移

### Production 資料庫設定

```bash
# 執行 Migration
dotnet ef database update --connection "your_production_connection_string"
```

## 監控與維護

### 健康檢查

- **後端健康檢查**: `/health`
- **前端可用性**: 主頁載入測試

### 日誌監控

*待設定 - 建議使用結構化日誌*

### 備份策略

*待規劃 - 資料庫定期備份*

## 故障排除

### 常見問題

1. **資料庫連線失敗**
   - 檢查連線字串設定
   - 確認資料庫服務狀態

2. **API 回應 500 錯誤**
   - 查看應用程式日誌
   - 檢查環境變數設定

3. **前端無法載入**
   - 檢查 API 基礎 URL 設定
   - 確認 CORS 政策設定

---

**注意**: 此文檔將在部署階段詳細補充完整。