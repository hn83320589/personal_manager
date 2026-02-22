# Personal Manager

個人展示與管理平台 — 包含公開展示網站與個人管理後台的全端應用程式。

## 功能概覽

| 功能 | 公開 | 需登入 |
|------|------|--------|
| 個人介紹、學經歷、技能、作品集 | ✅ | — |
| 部落格、留言板、聯絡方式 | ✅ | — |
| 公開行事曆 | ✅ | — |
| 管理後台（所有內容管理） | — | ✅ |
| 行事曆、工作追蹤、待辦事項 | — | ✅ |

## 技術架構

| 端 | 技術 |
|----|------|
| 後端 | C# .NET 9.0 Web API + EF Core 9 + MariaDB |
| 前端 | Vue 3 + TypeScript + Pinia + Tailwind CSS + Vite |
| 資料庫 | MariaDB（Zeabur 雲端）/ 本地 JSON 檔案（開發 fallback） |
| 認證 | JWT Bearer Token |
| 部署 | Zeabur |

## 倉庫架構（分離倉庫）

```
personal_manager/                     # 主專案（本倉庫）
├── CLAUDE.md
├── README.md
├── docs/
└── local-development/                # 本地開發（不提交至主倉庫）
    ├── PersonalManagerBackend/       # 後端 clone
    └── PersonalManagerFrontend/      # 前端 clone
```

| 倉庫 | GitHub |
|------|--------|
| 主專案 | [hn83320589/personal_manager](https://github.com/hn83320589/personal_manager) |
| 後端 | [hn83320589/PersonalManagerBackend](https://github.com/hn83320589/PersonalManagerBackend) |
| 前端 | [hn83320589/PersonalManagerFrontend](https://github.com/hn83320589/PersonalManagerFrontend) |

## 本地開發

### 前置需求

- .NET 9.0 SDK
- Node.js 20+ (LTS)
- Git

### 啟動步驟

```bash
# 1. Clone 前後端到 local-development/
cd local-development
git clone https://github.com/hn83320589/PersonalManagerBackend.git
git clone https://github.com/hn83320589/PersonalManagerFrontend.git

# 2. 啟動後端（終端 1）
cd PersonalManagerBackend
dotnet run
# → http://localhost:5037
# → Swagger: http://localhost:5037/swagger

# 3. 啟動前端（終端 2）
cd PersonalManagerFrontend
npm install
npm run dev
# → http://localhost:5173
```

> **Demo 登入**：帳號 `admin`，密碼 `demo123`

### 無 DB 連線時

後端啟動時自動偵測 MariaDB 連線：
- **有 DB**：使用 EF Core + MariaDB，自動建表
- **無 DB**：自動 fallback 至 `Data/JsonData/*.json`，不影響開發

## 部署（Zeabur）

1. 後端：連接 `PersonalManagerBackend` 倉庫至 Zeabur，設定環境變數：
   ```
   ConnectionStrings__DefaultConnection=<MariaDB 連線字串>
   Jwt__SecretKey=<至少 32 字元的密鑰>
   ```

2. 前端：連接 `PersonalManagerFrontend` 倉庫至 Zeabur，設定：
   ```
   VITE_API_BASE_URL=https://<後端網域>/api
   ```

## 文件

- [後端 CLAUDE.md](./local-development/PersonalManagerBackend/CLAUDE.md) — 後端架構與開發指南
- [前端 CLAUDE.md](./local-development/PersonalManagerFrontend/CLAUDE.md) — 前端架構與開發指南
- [主專案 CLAUDE.md](./CLAUDE.md) — 整體規劃與開發紀錄
