# Personal Manager

多使用者的個人展示與管理平台。每位使用者有一個公開頁面 `/@username`（介紹、作品集、經歷與技能、文章、留言板、行事曆），
以及管理這些內容和個人待辦、行事曆、工作追蹤的後台。

![個人首頁](docs/images/screenshots/public-home.png)

## 功能

| 公開頁面（不需登入） | 管理後台（需登入） |
| --- | --- |
| 使用者目錄與搜尋 | 儀表板：待審留言、草稿、待辦、最近 7 天工時、近期行程 |
| 個人首頁：介紹、可接案狀態、精選作品、經歷與技能、最新文章、聯絡方式 | 區塊式作品編輯器（文字、圖片、圖庫、附件、嵌入、重點數字、程式碼），自動儲存 |
| 作品集：設計、前端、後端三種模式，多種卡片版型，封面輪播 | 文章編輯器：「/」插入選單、圖片說明、嵌入影片、程式碼上色、排程發佈 |
| 部落格：分類、標籤、搜尋、目錄、閱讀時間 | 經歷、技能、聯絡方式、留言審核與回覆、檔案管理 |
| 留言板（審核後公開）、公開行事曆 | 行事曆（重複行程）、待辦、工作追蹤（計時器、時間紀錄、統計） |
| 深淺色模式、5 種主題色、手機版 | 修改密碼；管理員可管理使用者 |

## 畫面

### 公開頁面

| 使用者目錄 | 作品列表 |
| --- | --- |
| ![使用者目錄](docs/images/screenshots/public-directory.png) | ![作品列表](docs/images/screenshots/public-works.png) |
| **文章列表** | **留言板** |
| ![文章列表](docs/images/screenshots/public-blog.png) | ![留言板](docs/images/screenshots/public-guestbook.png) |
| **公開行事曆** | **深色模式** |
| ![公開行事曆](docs/images/screenshots/public-calendar.png) | ![深色模式的個人首頁](docs/images/screenshots/public-home-dark.png) |

**作品詳情與手機版**：資訊欄位、封面、內容區塊（圖片可開 lightbox）、重點數字、程式碼，以及上一件／下一件。

<p>
  <img src="docs/images/screenshots/public-work-detail.png" alt="作品詳情" width="60%">
  <img src="docs/images/screenshots/mobile-home.png" alt="手機版個人首頁" width="19%">
  <img src="docs/images/screenshots/mobile-work-detail.png" alt="手機版作品詳情" width="19%">
</p>

<details>
<summary>文章、深色模式的作品詳情</summary>

| 文章 | 作品詳情（深色） |
| --- | --- |
| ![文章](docs/images/screenshots/public-post.png) | ![深色模式的作品詳情](docs/images/screenshots/public-work-detail-dark.png) |

</details>

### 管理後台

| 儀表板 | 作品編輯器 |
| --- | --- |
| ![儀表板](docs/images/screenshots/admin-dashboard.png) | ![作品編輯器](docs/images/screenshots/admin-work-editor.png) |
| **文章編輯器** | **行事曆** |
| ![文章編輯器](docs/images/screenshots/admin-post-editor.png) | ![行事曆](docs/images/screenshots/admin-calendar.png) |
| **工作追蹤** | **待辦** |
| ![工作追蹤](docs/images/screenshots/admin-work-tracking.png) | ![待辦](docs/images/screenshots/admin-todos.png) |

<details>
<summary>其他後台頁面</summary>

| 登入 | 個人資料 |
| --- | --- |
| ![登入](docs/images/screenshots/admin-login.png) | ![個人資料](docs/images/screenshots/admin-profile.png) |
| **作品** | **文章** |
| ![作品清單](docs/images/screenshots/admin-works.png) | ![文章清單](docs/images/screenshots/admin-posts.png) |
| **經歷** | **技能** |
| ![經歷](docs/images/screenshots/admin-experience.png) | ![技能](docs/images/screenshots/admin-skills.png) |
| **留言** | **檔案** |
| ![留言](docs/images/screenshots/admin-comments.png) | ![檔案](docs/images/screenshots/admin-files.png) |
| **聯絡方式** | **修改密碼** |
| ![聯絡方式](docs/images/screenshots/admin-contacts.png) | ![修改密碼](docs/images/screenshots/admin-account.png) |
| **使用者管理** | |
| ![使用者管理](docs/images/screenshots/admin-users.png) | |

</details>

畫面中的資料是示範資料（作品封面為測試時產生的示意圖）。

## 技術

| 端 | 技術 |
| --- | --- |
| 後端 | .NET 9 Web API、EF Core 9；SQLite（預設）或 MySQL／MariaDB |
| 前端 | Vue 3、TypeScript（strict）、Pinia、Tailwind CSS、Tiptap、Vite |
| 認證 | JWT access token（只在記憶體）+ httpOnly cookie refresh token（每次輪換） |
| 測試 | xUnit 整合測試 297 項、Vitest 169 項、Playwright E2E 8 項（本機執行，沒有 CI） |

為什麼這樣選、程式碼怎麼組織，見[技術教學](docs/technical-guide.md)。目前沒有正式環境，只在本機開發。

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

## 測試

```bash
cd backend && dotnet test PersonalManager.sln    # 後端
cd frontend && npx vitest run                    # 前端單元測試
cd frontend && npx playwright test               # E2E（自動啟動後端與前端）
```

提交前的完整檢查清單見[開發指南](docs/development-guide.md#測試與檢查)，最新結果見[測試報告](docs/test-report.md)。

## 倉庫結構

```
backend/    .NET 方案（API、測試、openapi.json）
frontend/   Vue SPA（含 E2E）
docs/       規格、教學、測試報告與指南
```

## 文件

| 文件 | 內容 |
| --- | --- |
| [技術教學](docs/technical-guide.md) | 使用的技術與方法、為什麼這樣設計，以及新增功能的實作練習 |
| [開發指南](docs/development-guide.md) | 本地開發、測試、修改資料表與 API |
| [測試報告](docs/test-report.md) | 最新一次完整測試的結果與發現 |
| [優化計畫](docs/optimization-plan.md) | 可精簡的重複程式碼與待決定的清理項目 |
| [系統規格書](docs/system-specification.md) | 功能範圍、架構、安全設計、架構決策紀錄（ADR） |
| [資料庫設計](docs/database-design.md) | 資料表與關係 |
| [部署指南](docs/deployment-guide.md) | 日後部署時必須滿足的條件 |
| [任務清單](docs/TASKS.md) | 進行中的工作與技術債 |
| [異動記錄](CHANGELOG.md) | 重要的功能與架構變更 |
| [`backend/CLAUDE.md`](backend/CLAUDE.md)、[`frontend/CLAUDE.md`](frontend/CLAUDE.md) | 各端的程式寫法與慣例 |
