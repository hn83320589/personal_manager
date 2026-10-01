# CLAUDE.md - Personal Manager 主專案

This file provides guidance to Claude Code when working in this repository.

---

## 給 AI 的指示

每次任務完成時：
1. 在 `docs/TASKS.md` 勾選對應項目的 checkbox
2. 有新的技術債時，加入 `docs/TASKS.md` 的「技術債」區塊
3. 做了未預期的架構決策時，記錄到 `docs/system-specification.md` §12 架構決策紀錄（ADR），附上原因
4. 回報更新了哪些檔案與區塊

---

## 專案說明

**Personal Manager** — 現代化個人展示與管理平台，包含公開展示網站與個人管理後台。

### 功能模組

| 功能 | 說明 | 需要登入 |
|------|------|----------|
| 個人介紹 | 基本資料、個人簡介 | 否（公開） |
| 學/經歷 | 教育背景、工作經歷 | 否（公開） |
| 專長技能 | 技能分類與等級 | 否（公開） |
| 作品集 | 區塊式作品（設計／前端／後端三種模式） | 否（公開） |
| 公開行事曆 | 公開行程 | 否（公開） |
| 部落格 | 公開文章 | 否（公開） |
| 留言板 | 訪客留言 | 否（公開） |
| 聯絡我 | 社群/Email/手機（在個人首頁） | 否（公開） |
| 管理後台 | 所有內容管理 | **是** |
| 行事曆管理 | 完整行事曆 | **是** |
| 工作追蹤 | 任務計時、進度 | **是** |
| 待辦事項 | 個人任務管理 | **是** |

---

## 倉庫架構（monorepo）

```
personal_manager/
├── CLAUDE.md                     # 本檔案
├── CHANGELOG.md                  # 重要變更
├── backend/
│   ├── PersonalManager.sln
│   ├── src/PersonalManager.Api/  # .NET 9 Web API
│   └── tests/PersonalManager.Tests/
├── frontend/                     # Vue 3 SPA
└── docs/
    ├── TASKS.md                  # 任務清單與技術債
    ├── system-specification.md   # 系統規格與架構決策紀錄（ADR）
    ├── development-guide.md      # 本地開發流程
    ├── database-design.md        # 資料表
    └── deployment-guide.md       # 部署條件（目前沒有正式環境）
```

`local-development/` 是合併前的舊 repo clone，已不再使用（git-ignored）。

---

## 快速啟動（本地開發）

```bash
# 終端 1 — 後端
cd backend
dotnet run --project src/PersonalManager.Api
# → http://localhost:5037
# → Swagger: http://localhost:5037/swagger

# 測試
dotnet test PersonalManager.sln

# 終端 2 — 前端
cd frontend
npm install   # 首次或 package.json 有變動
npm run dev
# → http://localhost:5173（/api 與 /files 由 Vite 轉送到後端）
```

示範帳號（只在 Development 建立）：`admin` / `password123`，公開頁面 `http://localhost:5173/@admin`。

> 後端預設使用 SQLite，資料庫檔在 `backend/src/PersonalManager.Api/App_Data/`，刪除後重新啟動即可重建（ADR-008）。

---

## 技術架構

| 端 | 技術 |
|----|------|
| 後端 | C# .NET 9 Web API + EF Core 9（feature folder，ADR-011） |
| 前端 | Vue 3 + TypeScript（strict）+ Pinia + Tailwind CSS + Tiptap |
| 部署 | 暫無，目前僅本地開發（見 `docs/deployment-guide.md`） |
| 資料庫 | SQLite（預設）；可由 `Database:Provider` 切換為 MySQL/MariaDB |
| 認證 | JWT access token（前端記憶體）+ httpOnly cookie refresh token（ADR-010） |
| 測試 | xUnit 整合測試、Vitest、Playwright E2E |

---

## 重要規則

**每次異動後：**
- 後端有異動 → 更新 `backend/CLAUDE.md`（規則、結構、設定有變時）
- 前端有異動 → 更新 `frontend/CLAUDE.md`（同上）
- 重要的功能或架構變更 → 記錄在根目錄 `CHANGELOG.md`；逐次的細節由 git log 承擔，CLAUDE.md 不寫異動記錄

**開發規範：**
- 不使用 `--no-verify`
- commit 前確認可正確建置
- 不 disable 測試，修復它
