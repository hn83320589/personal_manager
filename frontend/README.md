# Personal Manager Frontend

個人展示與管理平台前端 UI，使用 Vue 3 + TypeScript + Vite 開發。

## 如何執行

### 前置需求

- Node.js `^20.19.0 || >=22.12.0`

### 開發模式

```bash
npm install   # 首次或 package.json 有變動時
npm run dev
```

- 前端：`http://localhost:5173`
- 需後端 API 同時運行於 `http://localhost:5037`
- Demo 登入：帳號 `admin`，密碼 `demo123`

## 環境變數

### `.env.development`（已存在，不提交至 git）

```env
VITE_API_BASE_URL=http://localhost:5037/api
VITE_APP_TITLE=Personal Manager
VITE_DEBUG=true
```

### `.env.production`（部署時設定）

```env
VITE_API_BASE_URL=https://your-api-domain.com/api
VITE_DEBUG=false
```

> 所有變數須以 `VITE_` 為前綴，才可在程式碼中透過 `import.meta.env.VITE_XXX` 存取。

## 建置與部署（Zeabur）

```bash
# 生產建置
npm run build

# 預覽建置結果
npm run preview
```

建置產出在 `dist/` 目錄，為純靜態檔案。

在 Zeabur 連接 `PersonalManagerFrontend` 倉庫，設定環境變數 `VITE_API_BASE_URL`，推送 `main` branch 即自動部署。

## 測試

```bash
# 型別檢查
npm run type-check

# 單元測試（Vitest）
npm run test:unit

# E2E 測試（Playwright）
npx playwright install   # 首次安裝瀏覽器
npm run test:e2e
```

## 相關連結

- [主專案](https://github.com/hn83320589/personal_manager)
- [後端專案](https://github.com/hn83320589/PersonalManagerBackend)
