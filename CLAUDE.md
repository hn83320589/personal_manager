# CLAUDE.md - Personal Manager 主專案

This file provides guidance to Claude Code when working in this repository.

---

## 專案說明

**Personal Manager** — 現代化個人展示與管理平台，包含公開展示網站與個人管理後台。

### 功能模組

| 功能 | 說明 | 需要登入 |
|------|------|----------|
| 個人介紹 | 基本資料、個人簡介 | 否（公開） |
| 學/經歷 | 教育背景、工作經歷 | 否（公開） |
| 專長技能 | 技能分類與等級 | 否（公開） |
| 作品集 | 專案展示 | 否（公開） |
| 公開行事曆 | 公開行程 | 否（公開） |
| 部落格 | 公開文章 | 否（公開） |
| 留言板 | 訪客留言 | 否（公開） |
| 聯絡我 | 社群/Email/手機 | 否（公開） |
| 管理後台 | 所有內容管理 | **是** |
| 行事曆管理 | 完整行事曆 | **是** |
| 工作追蹤 | 任務計時、進度 | **是** |
| 待辦事項 | 個人任務管理 | **是** |

---

## 倉庫架構（分離倉庫）

此專案採用三個獨立 Git 倉庫：

```
personal_manager/                         # 主專案（本倉庫）
├── CLAUDE.md                             # 主專案文件
├── README.md
├── docs/                                 # 共用文件
└── local-development/                    # 本地開發（不提交至主倉庫）
    ├── PersonalManagerBackend/           # 後端倉庫 clone
    └── PersonalManagerFrontend/          # 前端倉庫 clone
```

| 倉庫 | GitHub | 本地路徑 |
|------|--------|----------|
| 主專案 | `hn83320589/personal_manager` | `.` |
| 後端 | `hn83320589/PersonalManagerBackend` | `./local-development/PersonalManagerBackend` |
| 前端 | `hn83320589/PersonalManagerFrontend` | `./local-development/PersonalManagerFrontend` |

---

## 快速啟動（本地開發）

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

> 後端若無法連接 MariaDB，會自動 fallback 至本地 JSON 資料（`Data/JsonData/*.json`），不影響開發。

---

## 技術架構

| 端 | 技術 |
|----|------|
| 後端 | C# .NET 9.0 Web API + EF Core 9 + MariaDB |
| 前端 | Vue 3 + TypeScript + Pinia + Axios + Tailwind CSS |
| 部署 | Zeabur |
| 資料庫 | MariaDB（Zeabur 雲端） |
| 認證 | JWT Bearer Token |

---

## 重要規則

**每次異動後：**
- 後端有異動 → 更新 `local-development/PersonalManagerBackend/CLAUDE.md`
- 前端有異動 → 更新 `local-development/PersonalManagerFrontend/CLAUDE.md`
- 重大進度 → 同步更新本檔案

**開發規範：**
- 不使用 `--no-verify`
- commit 前確認可正確建置
- 不 disable 測試，修復它

---

## 最新異動記錄

### 2026/02/22
- **JSON 序列化修復**：後端 camelCase 輸出修正，前後端欄位對齊
- **DB 自動 fallback**：後端啟動時自動偵測 DB 連線，無法連線時使用本地 JSON
- **移除 `sql/` 資料夾**：由 EF Core `EnsureCreated()` + `DatabaseSeeder` 取代
- **前後端 CLAUDE.md 全面重寫**：移除過時內容，補充實際架構與操作指南

### 2026/02/20
- 前後端 README.md 全面重寫，移除過時 Docker/CI/CD 內容

### 2025/10/01
- RBAC 權限管理系統完整實作完成

### 2025/08/08 — 2025/09/19
- Phase 1 & 2：後端 + 前端完整開發
- 企業級功能整合：JWT、EF Core、RBAC、Rate Limiting
- CI/CD、測試框架、文件體系建立
