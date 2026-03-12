# Personal Manager API 快速參考

## 服務資訊

```
開發環境 Base URL: http://localhost:5037/api
前端服務:         http://localhost:5173
Swagger 文檔:     http://localhost:5037/swagger
```

## 回應格式

所有端點統一使用 camelCase，回傳格式如下：

```json
{
  "success": true,
  "message": "string",
  "data": {},
  "errors": null
}
```

---

## API 端點總覽

### 認證 (Auth)

```
POST   /api/auth/login              # 登入，回傳 JWT token + userId
POST   /api/auth/register           # 註冊
```

### 使用者 (Users)

```
GET    /api/users/public            # 所有用戶公開資訊（無需認證）
GET    /api/users/public/{username} # 依 username 查詢用戶（無需認證）
GET    /api/users                   # 取得所有使用者（需認證）
GET    /api/users/{id}              # 取得指定使用者（需認證）
PUT    /api/users/{id}              # 更新使用者（需認證）
DELETE /api/users/{id}              # 刪除使用者（需認證）
```

### 個人資料 (Profiles)

```
GET    /api/profiles/directory             # 用戶目錄（fullName + title + summary，無需認證）
GET    /api/profiles/user/{userId}         # 取得指定使用者個人資料
POST   /api/profiles                       # 建立個人資料（需認證）
PUT    /api/profiles/{id}                  # 更新個人資料（需認證）
DELETE /api/profiles/{id}                  # 刪除個人資料（需認證）
```

### 學歷 (Educations)

```
GET    /api/educations/user/{userId}/public  # 公開學歷（無需認證）
GET    /api/educations/user/{userId}         # 全部學歷（需認證）
GET    /api/educations/{id}                  # 取得指定學歷
POST   /api/educations                       # 建立學歷（需認證）
PUT    /api/educations/{id}                  # 更新學歷（需認證）
DELETE /api/educations/{id}                  # 刪除學歷（需認證）
```

### 工作經歷 (WorkExperiences)

```
GET    /api/workexperiences/user/{userId}/public  # 公開工作經歷（無需認證）
GET    /api/workexperiences/user/{userId}          # 全部工作經歷（需認證）
GET    /api/workexperiences/{id}                   # 取得指定工作經歷
POST   /api/workexperiences                        # 建立工作經歷（需認證）
PUT    /api/workexperiences/{id}                   # 更新工作經歷（需認證）
DELETE /api/workexperiences/{id}                   # 刪除工作經歷（需認證）
```

### 技能 (Skills)

```
GET    /api/skills/user/{userId}/public  # 公開技能（無需認證）
GET    /api/skills/user/{userId}         # 全部技能（需認證）
GET    /api/skills/{id}                  # 取得指定技能
POST   /api/skills                       # 建立技能（需認證）
PUT    /api/skills/{id}                  # 更新技能（需認證）
DELETE /api/skills/{id}                  # 刪除技能（需認證）
```

### 作品集 (Portfolios)

```
GET    /api/portfolios/user/{userId}/public  # 公開作品集（無需認證）
GET    /api/portfolios/user/{userId}          # 全部作品集（需認證）
GET    /api/portfolios/{id}                   # 取得指定作品
POST   /api/portfolios                        # 建立作品（需認證）
PUT    /api/portfolios/{id}                   # 更新作品（需認證）
DELETE /api/portfolios/{id}                   # 刪除作品（需認證）
```

### 行事曆 (CalendarEvents)

```
GET    /api/calendarevents/user/{userId}/public  # 公開行程（無需認證）
GET    /api/calendarevents/user/{userId}          # 全部行程（需認證）
GET    /api/calendarevents/{id}                   # 取得指定行程
POST   /api/calendarevents                        # 建立行程（需認證）
PUT    /api/calendarevents/{id}                   # 更新行程（需認證）
DELETE /api/calendarevents/{id}                   # 刪除行程（需認證）
```

### 待辦事項 (TodoItems)

```
GET    /api/todoitems                # 取得所有待辦（需認證）
GET    /api/todoitems/{id}           # 取得指定待辦
POST   /api/todoitems                # 建立待辦（需認證）
PUT    /api/todoitems/{id}           # 更新待辦（需認證）
DELETE /api/todoitems/{id}           # 刪除待辦（需認證）
```

### 工作任務 (WorkTasks)

```
GET    /api/worktasks                # 取得所有工作任務（需認證）
GET    /api/worktasks/{id}           # 取得指定工作任務
POST   /api/worktasks                # 建立工作任務（需認證）
PUT    /api/worktasks/{id}           # 更新工作任務（需認證）
DELETE /api/worktasks/{id}           # 刪除工作任務（需認證）
```

### 部落格文章 (BlogPosts)

```
GET    /api/blogposts/user/{userId}       # 公開已發布文章（無需認證）
GET    /api/blogposts/slug/{slug}         # 依 slug 取得文章（無需認證）
GET    /api/blogposts                     # 全部文章（需認證）
GET    /api/blogposts/{id}                # 取得指定文章
POST   /api/blogposts                     # 建立文章（需認證）
PUT    /api/blogposts/{id}                # 更新文章（需認證）
DELETE /api/blogposts/{id}                # 刪除文章（需認證）
```

### 留言板 (GuestBookEntries)

```
GET    /api/guestbookentries/user/{targetUserId}  # 指定用戶的已審核留言（無需認證）
GET    /api/guestbookentries                      # 全部留言（需認證）
POST   /api/guestbookentries                      # 新增留言（無需認證）
PUT    /api/guestbookentries/{id}                 # 更新留言（需認證）
DELETE /api/guestbookentries/{id}                 # 刪除留言（需認證）
```

### 聯絡方式 (ContactMethods)

```
GET    /api/contactmethods/user/{userId}/public  # 公開聯絡方式（無需認證）
GET    /api/contactmethods/user/{userId}          # 全部聯絡方式（需認證）
GET    /api/contactmethods/{id}                   # 取得指定聯絡方式
POST   /api/contactmethods                        # 建立聯絡方式（需認證）
PUT    /api/contactmethods/{id}                   # 更新聯絡方式（需認證）
DELETE /api/contactmethods/{id}                   # 刪除聯絡方式（需認證）
```

---

## 常用 Enum 值

### Skill Level（技能等級）
```
Beginner / Intermediate / Advanced / Expert
```

### BlogPost Status（文章狀態）
```
Draft / Published / Archived
```

### TodoItem Status / Priority
```
Status:   Pending / InProgress / Completed
Priority: Low / Medium / High / Critical
```

### CalendarEvent（全天/公開）
```
isAllDay: boolean
isPublic: boolean
```

---

## 測試指令範例

```bash
# 登入取得 token
curl -X POST http://localhost:5037/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"your_password"}'

# 取得公開用戶目錄
curl http://localhost:5037/api/profiles/directory

# 取得指定用戶公開個人資料
curl http://localhost:5037/api/users/public/admin

# 取得 admin 用戶的公開技能
curl http://localhost:5037/api/skills/user/1/public

# 建立技能（需 Authorization header）
curl -X POST http://localhost:5037/api/skills \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"userId":1,"name":"Vue 3","category":"前端框架","level":"Advanced","isPublic":true}'
```

---

## 常見錯誤

### 400 - 資料驗證失敗
```json
{ "success": false, "message": "資料驗證失敗", "data": null, "errors": ["field is required"] }
```

### 401 - 未授權
```json
{ "success": false, "message": "未授權", "data": null, "errors": [] }
```

### 404 - 資源不存在
```json
{ "success": false, "message": "找不到指定的資源", "data": null, "errors": [] }
```

---

*詳細 API 文檔請參考 Swagger UI：http://localhost:5037/swagger*
