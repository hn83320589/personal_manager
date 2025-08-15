# JWT認證系統 API 文檔

## 🔐 JWT認證系統概述

Personal Manager 採用企業級JWT認證系統，提供安全的使用者認證與授權機制。

### 🎯 功能特色

- **BCrypt密碼雜湊**: 強度12的安全密碼保護
- **JWT Bearer Token**: 1小時存取令牌 + 7天重新整理令牌
- **角色權限控制**: User/Admin權限管理
- **自動令牌刷新**: 無縫的認證體驗
- **完整安全日誌**: 認證活動追蹤

## 🔗 API端點說明

### 1. 使用者註冊
```http
POST /api/auth/register
Content-Type: application/json

{
  "username": "testuser",
  "email": "test@example.com",
  "password": "password123",
  "confirmPassword": "password123",
  "fullName": "Test User"
}
```

**成功回應 (201):**
```json
{
  "isSuccess": true,
  "message": "註冊成功",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "UaMCHFUHSDTq5WhHaEiep2GkXRe/+dE7FuZNzw8YkDQ=",
    "tokenType": "Bearer",
    "expiresIn": 1755243075,
    "user": {
      "id": 4,
      "username": "testuser",
      "email": "test@example.com",
      "fullName": "Test User",
      "role": "User",
      "isActive": true
    }
  },
  "errors": []
}
```

### 2. 使用者登入
```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "testuser",
  "password": "password123"
}
```

**成功回應 (200):**
```json
{
  "isSuccess": true,
  "message": "登入成功",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "HVDwfax94njwBlaZkvt+oGJmW7RlY8xw5AfF77rHAA0=",
    "tokenType": "Bearer",
    "expiresIn": 1755243135,
    "user": {
      "id": 4,
      "username": "testuser",
      "email": "test@example.com",
      "fullName": "Test User",
      "role": "User",
      "isActive": true
    }
  },
  "errors": []
}
```

### 3. 取得當前使用者資訊
```http
GET /api/auth/me
Authorization: Bearer {accessToken}
```

**成功回應 (200):**
```json
{
  "isSuccess": true,
  "message": "成功取得使用者資訊",
  "data": {
    "id": 4,
    "username": "testuser",
    "email": "test@example.com",
    "fullName": "Test User",
    "role": "User",
    "isActive": true
  },
  "errors": []
}
```

### 4. 令牌驗證
```http
GET /api/auth/validate
Authorization: Bearer {accessToken}
```

**成功回應 (200):**
```json
{
  "isSuccess": true,
  "message": "令牌驗證成功",
  "data": {
    "valid": true,
    "userId": "4",
    "username": "testuser",
    "message": "令牌有效"
  },
  "errors": []
}
```

### 5. 重新整理令牌
```http
POST /api/auth/refresh
Content-Type: application/json

{
  "refreshToken": "HVDwfax94njwBlaZkvt+oGJmW7RlY8xw5AfF77rHAA0="
}
```

**成功回應 (200):**
```json
{
  "isSuccess": true,
  "message": "令牌重新整理成功",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "NewRefreshTokenValue...",
    "tokenType": "Bearer",
    "expiresIn": 1755246735,
    "user": {
      "id": 4,
      "username": "testuser",
      "email": "test@example.com",
      "fullName": "Test User",
      "role": "User",
      "isActive": true
    }
  },
  "errors": []
}
```

### 6. 撤銷令牌 (登出)
```http
POST /api/auth/revoke
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "refreshToken": "HVDwfax94njwBlaZkvt+oGJmW7RlY8xw5AfF77rHAA0="
}
```

**成功回應 (200):**
```json
{
  "isSuccess": true,
  "message": "登出成功",
  "data": null,
  "errors": []
}
```

### 7. 測試受保護端點
```http
GET /api/auth/protected
Authorization: Bearer {accessToken}
```

**成功回應 (200):**
```json
{
  "isSuccess": true,
  "message": "受保護端點存取成功",
  "data": {
    "message": "您已成功存取受保護的端點",
    "username": "testuser",
    "role": "User",
    "serverTime": "2025-08-15T06:32:15.434Z"
  },
  "errors": []
}
```

## ⚠️ 錯誤處理

### 無效令牌 (401)
```json
HTTP/1.1 401 Unauthorized
WWW-Authenticate: Bearer error="invalid_token"
```

### 資料驗證失敗 (400)
```json
{
  "isSuccess": false,
  "message": "資料驗證失敗",
  "data": null,
  "errors": [
    "密碼長度須介於6-100字元",
    "確認密碼與密碼不符"
  ]
}
```

### 登入失敗 (401)
```json
{
  "isSuccess": false,
  "message": "登入失敗，請檢查使用者名稱和密碼",
  "data": null,
  "errors": []
}
```

## 🔧 JWT技術規格

### 令牌設定
```
Issuer: PersonalManagerAPI
Audience: PersonalManagerClient
Algorithm: HMAC-SHA256
Access Token 有效期: 1小時
Refresh Token 有效期: 7天
```

### 令牌結構 (Claims)
```json
{
  "nameid": "4",
  "name": "testuser",
  "email": "test@example.com",
  "role": "User",
  "userId": "4",
  "username": "testuser",
  "fullName": "Test User",
  "jti": "d5e8acf4-852e-4299-8360-2e1b41c2a773",
  "iat": 1755239475,
  "exp": 1755243075,
  "iss": "PersonalManagerAPI",
  "aud": "PersonalManagerClient"
}
```

## 🛡️ 安全特性

### 密碼安全
- **BCrypt雜湊**: 工作因子12，抗暴力破解
- **密碼要求**: 6-100字元，支援複雜密碼

### 令牌安全
- **短期Access Token**: 1小時自動過期
- **安全Refresh Token**: 7天有效期，支援撤銷
- **防重播攻擊**: 每次刷新產生新的Refresh Token

### 監控與審計
- **完整認證日誌**: 記錄所有認證活動
- **失敗次數追蹤**: 監控異常登入嘗試
- **IP位址記錄**: 安全事件分析

## 📋 使用範例

### cURL 測試指令

#### 註冊新使用者
```bash
curl -X POST "http://localhost:5253/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "newuser",
    "email": "newuser@example.com", 
    "password": "password123",
    "confirmPassword": "password123",
    "fullName": "New User"
  }'
```

#### 使用者登入
```bash
curl -X POST "http://localhost:5253/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "newuser",
    "password": "password123"
  }'
```

#### 存取受保護資源
```bash
curl -X GET "http://localhost:5253/api/auth/me" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

#### 重新整理令牌
```bash
curl -X POST "http://localhost:5253/api/auth/refresh" \
  -H "Content-Type: application/json" \
  -d '{
    "refreshToken": "YOUR_REFRESH_TOKEN"
  }'
```

## 📊 效能指標

- **註冊時間**: ~860ms (含BCrypt雜湊)
- **登入時間**: ~167ms
- **令牌驗證**: ~5ms
- **令牌重新整理**: ~19ms

## 🔄 前端整合

### AuthService 範例
```typescript
// 登入
const loginResult = await authService.login({
  username: 'testuser',
  password: 'password123'
});

// 自動令牌刷新
const refreshedToken = await authService.refreshToken(refreshToken);

// 取得當前使用者
const currentUser = await authService.getCurrentUser();

// 登出
await authService.logout();
```

### 自動認證攔截器
```typescript
// HTTP 攔截器自動添加 Authorization header
axios.interceptors.request.use((config) => {
  const token = authStore.accessToken;
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});
```

---

## 📝 更新日誌

**2025/08/15**
- ✅ JWT認證系統完整實作
- ✅ 企業級安全特性
- ✅ 完整測試驗證
- ✅ 生產級穩定性