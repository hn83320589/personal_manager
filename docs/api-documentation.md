# API 文檔

## 概述

Personal Manager 系統的 RESTful API 接口文檔。

## 基本資訊

- **Base URL**: `http://localhost:5000/api` (開發環境)
- **API 版本**: v1
- **認證方式**: JWT Bearer Token
- **Content-Type**: application/json

## 認證

### POST /auth/login
使用者登入

**請求內容**:
```json
{
  "username": "string",
  "password": "string"
}
```

**回應內容**:
```json
{
  "token": "jwt_token_string",
  "user": {
    "id": 1,
    "username": "string",
    "email": "string"
  }
}
```

## API 端點

### 使用者管理
*待補充 - 將在API開發階段完成*

### 個人資料管理
*待補充*

### 部落格管理
*待補充*

### 任務管理
*待補充*

### 行事曆管理
*待補充*

## 錯誤處理

### HTTP 狀態碼

| 狀態碼 | 說明 |
|-------|------|
| 200 | 請求成功 |
| 201 | 資源建立成功 |
| 400 | 請求格式錯誤 |
| 401 | 未授權 |
| 403 | 權限不足 |
| 404 | 資源不存在 |
| 500 | 伺服器內部錯誤 |

### 錯誤回應格式

```json
{
  "error": {
    "code": "ERROR_CODE",
    "message": "錯誤訊息",
    "details": "詳細錯誤說明"
  }
}
```

## 測試

使用 Swagger UI 進行 API 測試: `http://localhost:5000/swagger`

---

**注意**: 此文檔將在API開發階段詳細補充完整。