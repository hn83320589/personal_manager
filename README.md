# Personal Manager System

個人管理系統 - 一個包含個人介紹、履歷展示、工作追蹤和待辦事項管理的全端應用程式。

## 🎯 專案概述

Personal Manager 是一個現代化的個人管理系統，提供以下功能：

- **公開展示**: 個人介紹、學經歷、專長技能、作品集展示
- **內容管理**: 部落格文章撰寫與管理  
- **行事曆**: 公開行程展示與個人行程管理
- **任務管理**: 工作追蹤與待辦事項管理
- **互動功能**: 訪客留言板與聯絡表單

## 🏗️ 系統架構

此專案採用**分離倉庫架構**，將前後端獨立開發與部署：

### 📚 倉庫結構

| 倉庫 | 用途 | 技術棧 |
|------|------|--------|
| **personal-manager** (主專案) | 專案規劃、文檔、部署協調 | Markdown, Docker |
| **personal-manager-backend** | 後端API服務 | C# .NET Core, Entity Framework, MariaDB |
| **personal-manager-frontend** | 前端使用者介面 | Vue3, TypeScript, Vite, Pinia |

### 🔗 相關倉庫

- 🖥️ **後端倉庫**: [personal-manager-backend](https://github.com/hn83320589/PersonalManagerBackend)
- 🎨 **前端倉庫**: [personal-manager-frontend](https://github.com/hn83320589/PersonalManagerFrontend)

## 🚀 快速開始

### 前置需求

- Git
- .NET 9.0 SDK
- Node.js (18+)
- MariaDB/MySQL

### 本地開發設定

1. **Clone 主專案**
   ```bash
   git clone https://github.com/hn83320589/personal_manager.git
   cd personal-manager
   ```

2. **設定後端專案**
   ```bash
   git clone https://github.com/hn83320589/PersonalManagerBackend.git PersonalManagerBackend
   cd PersonalManagerBackend
   dotnet restore
   dotnet run
   ```

3. **設定前端專案**
   ```bash
   git clone https://github.com/hn83320589/PersonalManagerFrontend.git PersonalManagerFrontend
   cd PersonalManagerFrontend
   npm install
   npm run dev
   ```

4. **存取應用程式**
   - 前端: http://localhost:5173
   - 後端API: http://localhost:5253/api
   - Swagger文檔: http://localhost:5253/swagger

## 📋 開發進度

- ✅ 專案架構規劃
- ✅ 前後端專案框架建立  
- ✅ 資料庫設計與建立 (JSON 模擬資料)
- ✅ 後端API開發 (13個完整 Controllers + 文件 + 測試)
- ✅ 前端核心架構 (Vue3 + TypeScript + Tailwind CSS)
- ✅ 前端狀態管理 (9個 Pinia Stores)
- ✅ 前端API服務層 (10個完整服務)
- ✅ 前端UI元件庫 (8個核心元件) 
- ✅ 前端公開頁面開發 (9/9 完成)
- ✅ 前端認證系統 (登入、JWT、路由守衛)
- ✅ **前後端API整合完成** (CORS、HTTP客戶端、環境配置)
- 🔄 前端管理頁面開發 (11個管理介面)
- ⏳ 系統功能完善與測試
- ⏳ 部署上線

**整體開發進度: ~85% 完成** 🚀

## 📖 文檔

- [專案開發記錄](./CLAUDE.md) - 詳細的開發過程與架構說明
- [資料庫設計](./docs/database-design.md) - 資料庫架構與關聯設計
- [API文檔](./docs/api-documentation.md) - 後端API接口說明
- [部署指南](./docs/deployment-guide.md) - 生產環境部署說明

## 🛠️ 技術棧

### 後端
- **框架**: ASP.NET Core 9.0
- **資料庫**: JSON 模擬資料 (JsonDataService)
- **ORM**: Entity Framework Core 9.0.8 (已設定，未來可連接 MariaDB)
- **文檔**: Swagger/OpenAPI + 完整API文檔
- **測試**: xUnit + 手動測試驗證

### 前端  
- **框架**: Vue 3.5 with Composition API
- **語言**: TypeScript 嚴格模式
- **建置**: Vite 7.1 + PostCSS
- **樣式**: Tailwind CSS + 自訂主題設計
- **狀態管理**: Pinia (9個完整 Stores)
- **API客戶端**: Axios + 統一攔截器 + CORS整合
- **路由**: Vue Router 4 + 認證守衛
- **UI元件**: 8個自建核心元件 + Heroicons
- **認證**: JWT Token + 自動刷新 + 路由保護
- **測試**: Vitest + Playwright (已設定)

### 部署
- **容器化**: Docker
- **編排**: Docker Compose
- **平台**: Zeabur

## 🤝 開發規範

### Git 工作流程
1. 各倉庫獨立開發與測試
2. 使用語義化版本標籤 (Semantic Versioning)
3. API 變更需同步更新所有相關文檔
4. 重大更新需在主專案記錄版本相容性

### 版本同步
- 後端API版本: `v1.x.x`
- 前端UI版本: `v1.x.x`  
- 整體系統版本: `v1.x.x`

## 🔗 API 整合狀態

### 已整合服務
- ✅ 後端 API 服務: http://localhost:5253
- ✅ 前端 UI 服務: http://localhost:5173  
- ✅ CORS 跨域設定完成
- ✅ HTTP 客戶端配置完成
- ✅ 環境變數管理完成
- ✅ 認證系統整合完成

### API 測試
開發環境提供 API 測試介面，可在首頁測試以下功能：
- Users API (使用者管理)
- Skills API (技能管理)  
- PersonalProfiles API (個人資料)
- Demo 登入功能

### 開發服務啟動
```bash
# 啟動後端 (終端1)
cd PersonalManagerBackend
dotnet run

# 啟動前端 (終端2)  
cd PersonalManagerFrontend
npm run dev
```

## 📄 授權條款

此專案採用 MIT 授權條款 - 詳見 [LICENSE](LICENSE) 檔案。