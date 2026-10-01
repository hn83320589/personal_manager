# Personal Manager

多使用者的個人展示與管理平台。每位使用者有一個公開頁面 `/@username`（介紹、作品集、經歷與技能、文章、留言板、行事曆），
以及管理這些內容和個人待辦、行事曆、工作追蹤的後台。

| 端 | 技術 |
| --- | --- |
| 後端 | .NET 9 Web API、EF Core 9；SQLite（預設）或 MySQL／MariaDB |
| 前端 | Vue 3、TypeScript、Pinia、Tailwind CSS、Vite |
| 認證 | JWT access token + httpOnly cookie refresh token |
| 測試 | xUnit、Vitest、Playwright（本機執行，沒有 CI） |

目前沒有正式環境，只在本機開發。

## 快速開始

需要 .NET 9 SDK 與 Node.js 20.19+（或 22.12+），不需要安裝資料庫。

```bash
# 終端 1：後端 → http://localhost:5037（Swagger：/swagger）
cd backend
dotnet run --project src/PersonalManager.Api

# 終端 2：前端 → http://localhost:5173
cd frontend
npm install
npm run dev
```

開啟 `http://localhost:5173/@admin` 看公開頁面，或到 `/login` 以示範帳號 `admin` / `password123` 登入後台。
第一次啟動會自動建立 SQLite 資料庫與示範資料。

## 倉庫結構

```
backend/    .NET 方案（API、測試、openapi.json）
frontend/   Vue SPA（含 E2E）
docs/       規格與指南
```

## 文件

| 文件 | 內容 |
| --- | --- |
| [開發指南](docs/development-guide.md) | 本地開發、測試、修改資料表與 API |
| [系統規格書](docs/system-specification.md) | 功能範圍、架構、安全設計、架構決策紀錄（ADR） |
| [資料庫設計](docs/database-design.md) | 資料表與關係 |
| [部署指南](docs/deployment-guide.md) | 日後部署時必須滿足的條件 |
| [任務清單](docs/TASKS.md) | 進行中的工作與技術債 |
| [異動記錄](CHANGELOG.md) | 重要的功能與架構變更 |
| [`backend/CLAUDE.md`](backend/CLAUDE.md)、[`frontend/CLAUDE.md`](frontend/CLAUDE.md) | 各端的程式寫法與慣例 |
