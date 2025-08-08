# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Status

This appears to be a new/empty repository for a personal manager application. The repository currently contains no source files, configuration files, or documentation.

## Development Setup

Since this is a new project, you will need to:

1. Initialize the project with the appropriate technology stack based on user requirements
2. Set up build tools, package management, and testing frameworks as needed
3. Create the initial project structure

## Notes

- No existing codebase to analyze
- No build/test commands currently available
- Architecture will be determined based on initial implementation requirements
- This CLAUDE.md should be updated once the project structure is established

---

# 開發注意事項說明

- 此區會記錄開發的內容與項目規劃，包含系統架構與功能以及開發紀錄都會放在這裡。
- 待辦事項也會陸續新增(內容來源包含使用者需求、開發過程中的問題與解決方案等)。
- 溝通紀錄也會在這裡紀錄。
- 這個專案會使用GitHub來進行版本控制。(前後端會有各自的Git庫)
- 每次開發都要檢查專案底下的CLAUDE.md的內容。
- 每次異動完畢，都要更新此文件。
- 每個功能開發完成都需要做測試，包含單元測試與整合測試。才可以結束開發。
- 前端跟後端也都會各有自己的CLAUDE.md文件記錄各自詳細的開發內容。這個主要只在監控整個專案的開發進度與規劃。

## 專案說明

這一個個人服務系統personal manager，裡面包含著個人介紹、履歷，但也會有使用者登入後可以追蹤工作與待辦事項……等等，不同服務。

### 系統功能
1. 個人介紹
2. 學/經歷
3. 專長
4. 作品
5. 行事曆(可以自己寫也可以串GOOGLE)(分公開行事曆與個人行事曆，個人需登入)
6. 工作追蹤(需登入)
7. 個人待辦事項(需登入)
8. 文章/網誌(需登入，公開文章可直接瀏覽)
9. 留言板
10. 與我聯絡(社群網站/E-Mail/手機)

## 專案技術

- Server OS：Linux
- 部署服務：zeabur
- 資料庫：mariaDB
- 後端：C# .Net Core(for Linux)
- 前端：Vue.js(Vue3)
- 開發IDE：vscode
- 開發OS：Windows、MacOS都會
- 版本控制：GitHub

## 專案架構說明

此Personal Manager系統採用**分離倉庫架構**，包含三個獨立的Git倉庫：

### 主專案倉庫 (personal-manager)
負責整體專案規劃、文檔管理和部署協調
```
personal-manager/
├── CLAUDE.md                    # 主專案文檔與開發記錄
├── README.md                    # 專案說明
├── .gitignore                   # 排除本地的前後端資料夾
├── docs/                        # 共用文檔
│   ├── database-design.md       # 資料庫設計文檔
│   ├── api-documentation.md     # API文檔
│   └── deployment-guide.md      # 部署指南
├── docker-compose.yml           # 整合部署設定
└── local-development/           # 本地開發相關檔案
    ├── PersonalManagerBackend/  # 本地後端開發 (不上傳)
    └── PersonalManagerFrontend/ # 本地前端開發 (不上傳)
```

### 後端專案倉庫 (PersonalManagerBackend)
獨立的後端API專案
- **倉庫地址**: `https://github.com/hn83320589/PersonalManagerBackend`
- **本地路徑**: `./local-development/PersonalManagerBackend/`
- **技術棧**: C# .NET Core Web API + Entity Framework Core + MariaDB

### 前端專案倉庫 (PersonalManagerFrontend)  
獨立的前端UI專案
- **倉庫地址**: `https://github.com/hn83320589/PersonalManagerFrontend`
- **本地路徑**: `./local-development/PersonalManagerFrontend/`
- **技術棧**: Vue3 + TypeScript + Vite + Pinia

## 本地開發環境設定

### 初始化步驟
1. **Clone 主專案倉庫**
   ```bash
   git clone https://github.com/hn83320589/personal_manager.git
   cd personal-manager
   ```

2. **Clone 後端專案** (在主專案目錄下)
   ```bash
   git clone https://github.com/hn83320589/PersonalManagerBackend.git PersonalManagerBackend
   ```

3. **Clone 前端專案** (在主專案目錄下)
   ```bash
   git clone https://github.com/hn83320589/PersonalManagerFrontend.git PersonalManagerFrontend
   ```

4. **啟動開發環境**
   ```bash
   # 啟動後端 (終端1)
   cd PersonalManagerBackend
   dotnet run

   # 啟動前端 (終端2)
   cd PersonalManagerFrontend
   npm install
   npm run dev
   ```

### Git 工作流程
- **主專案**: 管理整體規劃、文檔、部署設定
- **後端專案**: 獨立開發、測試、發布後端API
- **前端專案**: 獨立開發、測試、發布前端UI

### 版本同步策略
- 使用 Git Tags 標記相容的版本
- 在主專案的 docker-compose.yml 中指定前後端的版本
- API 變更時同步更新三個倉庫的文檔

## 開發進度規劃

1.建立前端與後端專案框架 ✅
2.先開發後端與DB設計  
3.前端設計與開發

## 待辦事項追蹤

**2025/08/08 新增的待辦事項**
    - [x] 建立專案
    - [x] 建立系統基本架構
    - [ ] 先建立好資料庫設計
            _在後端專案裡面開一個DB的資料夾，裡面放資料庫設計SQL檔案，之後直接寫進資料庫或是使用migration_
    - [ ] 建立Models(含關聯)
    - [ ] 建立API接口
            _請參考後端專案(PersonalManagerBackend)的CLAUDE.md文件_
    - [ ] 撰寫單元測試
    - [ ] 整合測試
    - [ ] 撰寫API文件
    - [ ] 開發前端
            _請參考前端專案(PersonalManagerFrontend)的CLAUDE.md文件_
    - [ ] 整合前後端(接API)
    - [ ] 撰寫前端測試
    - [ ] 撰寫使用者手冊
    - [ ] 撰寫開發文件
    - [ ] 發布

>附註：
>>如果是同一天都集中在一起撰寫，可以在這裡記錄下來。
>>不同天的則是在開一個新的待辦事項清單內容，這樣可以方便查詢與追蹤。
>>之後如果有比較詳細的待辦事項內容，再新增或是修改舊的

>附註2：
>>已完成的待辦事項可以在前面加上 - [x] 來劃掉，這樣可以清楚知道已完成的項目。

## 溝通紀錄(與AI的對話紀錄或是重大事項跟程式邏輯紀錄)

待補

## 開發紀錄(每次修改時的異動紀錄)

### 2025/08/08 - 專案初始化與基本架構建立

#### 1. 建立專案
- 建立前端與後端專案目錄結構
- 調整資料夾名稱為 PersonalManagerBackend、PersonalManagerFrontend
- 移除多餘的內層資料夾，簡化專案結構

#### 2. 建立系統基本架構

**後端架構 (.NET Core Web API):**
- 建立基本目錄結構：Controllers, Models, Services, Data, DTOs, Middleware, Configuration
- 安裝必要的NuGet套件：
  - Microsoft.EntityFrameworkCore.Design
  - Microsoft.EntityFrameworkCore.Tools  
  - Pomelo.EntityFrameworkCore.MySql
  - Swashbuckle.AspNetCore (Swagger文件)
- 建立 ApplicationDbContext.cs 資料庫上下文
- 設定 Program.cs：
  - API控制器註冊
  - Swagger文件設定
  - CORS設定 (允許前端存取)
  - Entity Framework與MySQL連接設定
- 建立 BaseController.cs 控制器基礎類別
- 設定 appsettings.json 資料庫連接字串
- 建置測試成功 ✅

**前端架構 (Vue3 + TypeScript):**
- 建立Vue3專案，包含功能：
  - TypeScript支援
  - Vue Router路由管理
  - Pinia狀態管理
  - Vitest單元測試框架
  - Playwright端到端測試
- 開發伺服器啟動成功 (localhost:5173) ✅
- 專案結構包含：src/components、views、router、stores、assets等

**遇到的問題與解決方案:**
1. **Pomelo.EntityFrameworkCore.MySql版本相容性警告** - 已知問題，不影響功能
2. **Swagger套件缺失錯誤** - 已安裝 Swashbuckle.AspNetCore 解決
3. **ApplicationDbContext命名空間錯誤** - 已移除不存在的Models引用

**下一步:**
- 開始進行資料庫設計
- 建立Models與關聯