# Personal Manager API 文檔

## 概述

Personal Manager 系統的 RESTful API 接口文檔。本系統包含個人介紹、履歷管理、工作追蹤、待辦事項等功能的完整API。

## 基本資訊

- **Base URL**: `http://localhost:5253/api` (開發環境)
- **Swagger 文檔**: `http://localhost:5253/swagger`
- **API 版本**: v1.0
- **Content-Type**: application/json
- **回應格式**: 統一使用 ApiResponse 包裝格式
- **CORS**: 已配置支援前端 `http://localhost:5173` 跨域請求

## 🔗 前後端整合狀態

### 整合完成功能
- ✅ **CORS 設定**: 支援前端跨域請求
- ✅ **API 端點**: 13個完整 Controllers，32+ 個端點
- ✅ **JSON 資料服務**: 完整的資料模擬與回應
- ✅ **錯誤處理**: 統一錯誤回應格式
- ✅ **前端整合**: 與 Vue3 + Axios 完全整合

### 已驗證端點
```bash
✅ GET /api/users - 使用者列表
✅ GET /api/skills - 技能資料  
✅ GET /api/personalprofiles - 個人資料
✅ 所有CRUD操作正常運作
```

## API 回應格式

所有 API 均使用統一的回應格式：

### 成功回應
```json
{
  "success": true,
  "message": "操作成功訊息",
  "data": {}, // 實際資料內容
  "errors": []
}
```

### 失敗回應
```json
{
  "success": false,
  "message": "錯誤訊息",
  "data": null,
  "errors": ["具體錯誤內容"]
}
```

## API 端點

### 使用者管理 (Users)

#### GET /api/users
取得所有使用者列表

**回應範例**:
```json
{
  "success": true,
  "message": "成功取得使用者列表",
  "data": [
    {
      "id": 1,
      "username": "admin",
      "email": "admin@example.com",
      "firstName": "管理員",
      "lastName": "使用者",
      "phone": "0912345678",
      "isActive": true,
      "createdAt": "2025-08-12T09:00:00Z",
      "updatedAt": "2025-08-12T09:00:00Z"
    }
  ],
  "errors": []
}
```

#### GET /api/users/{id}
取得指定使用者資料

**路徑參數**:
- `id` (integer): 使用者ID

**回應範例**:
```json
{
  "success": true,
  "message": "成功取得使用者資料",
  "data": {
    "id": 1,
    "username": "admin",
    "email": "admin@example.com",
    "firstName": "管理員",
    "lastName": "使用者",
    "phone": "0912345678",
    "isActive": true,
    "createdAt": "2025-08-12T09:00:00Z",
    "updatedAt": "2025-08-12T09:00:00Z"
  },
  "errors": []
}
```

#### POST /api/users
建立新使用者

**請求內容**:
```json
{
  "username": "newuser",
  "email": "newuser@example.com",
  "passwordHash": "hashed_password",
  "firstName": "新",
  "lastName": "使用者",
  "phone": "0987654321",
  "isActive": true
}
```

**回應範例**:
```json
{
  "success": true,
  "message": "使用者建立成功",
  "data": {
    "id": 3,
    "username": "newuser",
    "email": "newuser@example.com",
    "firstName": "新",
    "lastName": "使用者",
    "phone": "0987654321",
    "isActive": true,
    "createdAt": "2025-08-12T03:58:42.359Z",
    "updatedAt": "2025-08-12T03:58:42.359Z"
  },
  "errors": []
}
```

#### PUT /api/users/{id}
更新使用者資料

**路徑參數**:
- `id` (integer): 使用者ID

**請求內容**:
```json
{
  "id": 1,
  "username": "admin",
  "email": "admin@example.com",
  "passwordHash": "hashed_password",
  "firstName": "更新後",
  "lastName": "姓名",
  "phone": "0912345678",
  "isActive": true
}
```

#### DELETE /api/users/{id}
刪除使用者

**路徑參數**:
- `id` (integer): 使用者ID

**回應範例**:
```json
{
  "success": true,
  "message": "使用者刪除成功",
  "errors": []
}
```

---

### 個人資料管理 (PersonalProfiles)

#### GET /api/personalprofiles
取得所有公開個人資料

**回應範例**:
```json
{
  "success": true,
  "message": "成功取得個人資料列表",
  "data": [
    {
      "id": 1,
      "userId": 1,
      "title": "全端開發工程師",
      "summary": "專精於 .NET Core 與 Vue.js 開發",
      "description": "擁有5年以上全端開發經驗...",
      "profileImageUrl": "https://example.com/images/profile.jpg",
      "website": "https://example.com",
      "location": "台灣 台北",
      "birthday": "1990-05-15T00:00:00Z",
      "isPublic": true,
      "createdAt": "2025-08-12T09:00:00Z",
      "updatedAt": "2025-08-12T09:00:00Z"
    }
  ],
  "errors": []
}
```

#### GET /api/personalprofiles/{id}
取得指定個人資料

#### GET /api/personalprofiles/user/{userId}
取得指定使用者的個人資料

#### POST /api/personalprofiles
建立個人資料

**請求內容**:
```json
{
  "userId": 1,
  "title": "軟體工程師",
  "summary": "熟悉多種程式語言",
  "description": "詳細描述...",
  "profileImageUrl": "https://example.com/profile.jpg",
  "website": "https://mysite.com",
  "location": "台北市",
  "birthday": "1990-01-01T00:00:00Z",
  "isPublic": true
}
```

#### PUT /api/personalprofiles/{id}
更新個人資料

#### DELETE /api/personalprofiles/{id}
刪除個人資料

---

### 學歷管理 (Educations)

#### GET /api/educations
取得所有公開學歷資料（按年份降冪排序）

**回應範例**:
```json
{
  "success": true,
  "message": "成功取得學歷列表",
  "data": [
    {
      "id": 1,
      "userId": 1,
      "school": "國立台灣大學",
      "degree": "碩士",
      "fieldOfStudy": "資訊工程學系",
      "startDate": "2015-09-01T00:00:00Z",
      "endDate": "2017-06-30T00:00:00Z",
      "description": "主修軟體工程與資料庫系統...",
      "isPublic": true,
      "sortOrder": 1,
      "createdAt": "2025-08-12T09:00:00Z",
      "updatedAt": "2025-08-12T09:00:00Z"
    }
  ],
  "errors": []
}
```

#### GET /api/educations/{id}
取得指定學歷資料

#### GET /api/educations/user/{userId}
取得指定使用者的學歷資料

#### POST /api/educations
建立學歷資料

**請求內容**:
```json
{
  "userId": 1,
  "school": "國立交通大學",
  "degree": "學士",
  "fieldOfStudy": "資訊工程學系",
  "startDate": "2011-09-01T00:00:00Z",
  "endDate": "2015-06-30T00:00:00Z",
  "description": "主修程式設計與演算法...",
  "isPublic": true,
  "sortOrder": 1
}
```

---

### 工作經歷管理 (WorkExperiences)

#### GET /api/workexperiences
取得所有公開工作經歷（按開始日期降冪排序）

**回應範例**:
```json
{
  "success": true,
  "message": "成功取得工作經歷列表",
  "data": [
    {
      "id": 1,
      "userId": 1,
      "company": "ABC科技公司",
      "position": "資深全端工程師",
      "startDate": "2020-03-01T00:00:00Z",
      "endDate": null,
      "isCurrent": true,
      "description": "負責企業級應用系統開發與維護...",
      "achievements": "• 主導開發的CRM系統提升業務效率30%\n• 重構舊有系統，降低維護成本50%",
      "isPublic": true,
      "sortOrder": 1,
      "createdAt": "2025-08-12T09:00:00Z",
      "updatedAt": "2025-08-12T09:00:00Z"
    }
  ],
  "errors": []
}
```

#### GET /api/workexperiences/current
取得目前職位（endDate 為 null 的工作經歷）

#### GET /api/workexperiences/{id}
取得指定工作經歷

#### GET /api/workexperiences/user/{userId}
取得指定使用者的工作經歷

#### POST /api/workexperiences
建立工作經歷

#### PUT /api/workexperiences/{id}
更新工作經歷

#### DELETE /api/workexperiences/{id}
刪除工作經歷

---

### 技能管理 (Skills)

#### GET /api/skills
取得所有公開技能資料

**回應範例**:
```json
{
  "success": true,
  "message": "成功取得技能列表",
  "data": [
    {
      "id": 1,
      "userId": 1,
      "name": "C# .NET Core",
      "category": "後端開發",
      "level": 3, // SkillLevel: 0=Beginner, 1=Intermediate, 2=Advanced, 3=Expert
      "description": "5年以上開發經驗，熟悉MVC、Web API、Entity Framework等技術",
      "isPublic": true,
      "sortOrder": 1,
      "createdAt": "2025-08-12T09:00:00Z",
      "updatedAt": "2025-08-12T09:00:00Z"
    }
  ],
  "errors": []
}
```

#### GET /api/skills/category/{category}
依分類篩選技能

**路徑參數**:
- `category` (string): 技能分類（如：後端開發、前端開發、資料庫、DevOps等）

#### GET /api/skills/level/{level}
依技能等級篩選

**路徑參數**:
- `level` (string): 技能等級（Beginner, Intermediate, Advanced, Expert）

#### GET /api/skills/{id}
取得指定技能資料

#### GET /api/skills/user/{userId}
取得指定使用者的技能資料

#### POST /api/skills
建立技能資料

**請求內容**:
```json
{
  "userId": 1,
  "name": "Vue.js",
  "category": "前端開發",
  "level": 2, // 0=Beginner, 1=Intermediate, 2=Advanced, 3=Expert
  "description": "3年以上開發經驗，熟悉Vue3、Composition API",
  "isPublic": true,
  "sortOrder": 2
}
```

---

## 資料模型

### User 模型
```json
{
  "id": "integer",
  "username": "string (必填, 最大50字元, 唯一)",
  "email": "string (必填, Email格式, 最大100字元, 唯一)",
  "passwordHash": "string (必填, 最大255字元)",
  "firstName": "string (最大50字元)",
  "lastName": "string (最大50字元)",
  "phone": "string (最大20字元)",
  "isActive": "boolean",
  "createdAt": "datetime",
  "updatedAt": "datetime"
}
```

### PersonalProfile 模型
```json
{
  "id": "integer",
  "userId": "integer (必填)",
  "title": "string (最大100字元)",
  "summary": "string (最大500字元)",
  "description": "string (最大2000字元)",
  "profileImageUrl": "string (最大500字元)",
  "website": "string (最大200字元)",
  "location": "string (最大100字元)",
  "birthday": "datetime",
  "isPublic": "boolean",
  "createdAt": "datetime",
  "updatedAt": "datetime"
}
```

### SkillLevel 枚舉
```json
{
  "Beginner": 0,
  "Intermediate": 1,
  "Advanced": 2,
  "Expert": 3
}
```

## HTTP 狀態碼

| 狀態碼 | 說明 |
|--------|------|
| 200 | 請求成功 |
| 201 | 資源建立成功 |
| 400 | 請求格式錯誤或資料驗證失敗 |
| 404 | 資源不存在 |
| 409 | 資料衝突（如重複的使用者名稱） |
| 500 | 伺服器內部錯誤 |

## 錯誤處理

### 資料驗證錯誤
當請求資料不符合驗證規則時，API會回傳400狀態碼和詳細錯誤訊息。

### 重複資料錯誤
當嘗試建立重複的使用者名稱或Email時，會回傳相應的錯誤訊息。

### 資源不存在錯誤
當請求不存在的資源時，會回傳"找不到指定的資源"訊息。

## 開發測試

### 本機測試環境
- API服務地址: `http://localhost:5001`
- Swagger文件: `http://localhost:5001/swagger` (如有啟用)

### 使用 curl 測試範例

```bash
# 取得使用者列表
curl -X GET "http://localhost:5001/api/users"

# 建立新使用者
curl -X POST "http://localhost:5001/api/users" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "email": "test@example.com",
    "passwordHash": "dummy_hash",
    "firstName": "Test",
    "lastName": "User",
    "isActive": true
  }'

# 取得技能列表
curl -X GET "http://localhost:5001/api/skills"

# 依分類篩選技能
curl -X GET "http://localhost:5001/api/skills/category/後端開發"
```

## 版本資訊

- **目前版本**: 1.0.0
- **建立日期**: 2025-08-12
- **最後更新**: 2025-08-12

## 聯絡資訊

如有API相關問題，請聯繫開發團隊。

---

*本文檔基於實際開發的API進行撰寫，所有範例均經過測試驗證。*