# 異動記錄

重要的功能與架構變更。細節見 git log；進行中的工作見 [`docs/TASKS.md`](docs/TASKS.md)，架構決策見 [`docs/system-specification.md`](docs/system-specification.md) §12。

## 2026-09 ～ 10：全面重構（分支 `refactor/phase-0`）

### 專案與基礎（Phase 0–1）
- 前端、後端、主專案合併為 monorepo（`backend/`、`frontend/`、`docs/`），保留歷史（ADR-009）
- 預設使用 SQLite，`Database:Provider` 可切換 MySQL／MariaDB，兩種 provider 各有一組 migration；移除 JSON fallback（ADR-008）
- 移除寫死的 JWT 金鑰，啟動時驗證設定；示範資料、Swagger、localhost CORS 只在 Development
- 整合測試改用 WebApplicationFactory + 暫存 SQLite；GitHub Actions CI

### 後端重建（Phase 2–3）
- API 分為 `/api/public`、`/api/me`、`/api/admin`，移除泛型 Repository 與 CrudService，改為 feature folder（ADR-011）
- 修正多項授權與資料外洩問題：他人資料一律 404、公開端點只回傳公開資料、使用者管理限管理員
- 認證：refresh token 改為 httpOnly cookie、只存雜湊、每次輪換並偵測重用；access token 15 分鐘（ADR-010）
- 文章與作品的 HTML 以 HtmlSanitizer 清洗；檔案上傳檢查 magic bytes、記錄圖片尺寸
- 作品集改為區塊式內容（封面輪播、自訂欄位、七種區塊），支援設計、前端、後端三種作品集模式（ADR-012）
- 提交 `backend/openapi.json`，測試確保與 API 一致；新增查詢檔案使用狀況的 API
- refresh／logout 使用獨立的流量限制，多開分頁不會被登出

### 前端重建（Phase 4–5）
- TypeScript strict、ESLint + Prettier；API 型別由 openapi-typescript 產生
- 新的 HTTP 層：access token 只在記憶體、多個 401 共用一次 refresh、只重試 GET；`v-html` 一律經 DOMPurify
- 前台依 prototype 重新設計：設計 token、深淺色與 5 種主題色；個人首頁、作品列表與詳情（lightbox、嵌入、程式碼上色）、文章、留言板、行事曆、使用者目錄
- 後台全部改版：分組側欄、列表 + 側邊編輯面板；區塊式作品編輯器與升級的部落格編輯器（「/」選單、圖片說明、排程）皆自動儲存；檔案刪除前提示使用中的內容；待辦、行事曆、工作追蹤（計時器）；新增註冊頁
- 刪除所有舊的頁面、元件、store 與 service
- E2E（Playwright）以真實後端測試關鍵流程，納入 CI

### 文件（Phase 6）
- 文件改寫為目前的架構；刪除過時的 Postman collection 與 API 快速參考；異動記錄集中到本檔

## 2026-03

- 密碼重設（Email 連結）、Refresh Token、時間紀錄 API、文章瀏覽數與分頁搜尋
- Object Storage 抽象層（本地／S3 相容）、SEO／Open Graph 標籤、文章目錄
- 資料表正規化（專案、標籤、行事曆重複規則）；改用 EF Core Migrations
- 資源所有權驗證、公開端點流量限制
- 檔案管理、作品集附件、TipTap 富文本編輯器
- 後端單元測試專案、前端單元測試與 E2E

## 2026-02

- 多使用者架構：`/@:username` 個人頁面、使用者目錄、每人留言板、5 套主題色
- JSON 序列化改為 camelCase、前後端欄位對齊；移除設定檔中的機密資訊

## 2025-08 ～ 10

- 後端（.NET）與前端（Vue 3）初版開發；JWT、EF Core、RBAC
