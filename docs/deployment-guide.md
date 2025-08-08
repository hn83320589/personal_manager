# 部署指南

## 概述

Personal Manager 系統的部署指南，包含開發、測試和生產環境的部署方式。

## 部署架構

### 開發環境
- 後端: 本地 .NET Core (localhost:5000)
- 前端: Vite 開發伺服器 (localhost:5173)
- 資料庫: 本地 MariaDB

### 生產環境
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