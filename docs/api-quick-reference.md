# Personal Manager API 快速參考

## Base URL
```
http://localhost:5001/api
```

## 回應格式
```json
{
  "success": boolean,
  "message": "string",
  "data": object|array|null,
  "errors": array
}
```

## API 端點總覽

### 使用者管理 (Users)
```
GET    /api/users              # 取得所有使用者
GET    /api/users/{id}         # 取得指定使用者
POST   /api/users              # 建立新使用者
PUT    /api/users/{id}         # 更新使用者
DELETE /api/users/{id}         # 刪除使用者
```

### 個人資料 (PersonalProfiles)
```
GET    /api/personalprofiles           # 取得所有公開個人資料
GET    /api/personalprofiles/{id}      # 取得指定個人資料
GET    /api/personalprofiles/user/{userId}  # 取得指定使用者個人資料
POST   /api/personalprofiles          # 建立個人資料
PUT    /api/personalprofiles/{id}     # 更新個人資料
DELETE /api/personalprofiles/{id}     # 刪除個人資料
```

### 學歷管理 (Educations)
```
GET    /api/educations                 # 取得所有公開學歷
GET    /api/educations/{id}            # 取得指定學歷
GET    /api/educations/user/{userId}   # 取得指定使用者學歷
POST   /api/educations                 # 建立學歷
PUT    /api/educations/{id}            # 更新學歷
DELETE /api/educations/{id}            # 刪除學歷
```

### 工作經歷 (WorkExperiences)
```
GET    /api/workexperiences            # 取得所有公開工作經歷
GET    /api/workexperiences/current    # 取得目前職位
GET    /api/workexperiences/{id}       # 取得指定工作經歷
GET    /api/workexperiences/user/{userId}  # 取得指定使用者工作經歷
POST   /api/workexperiences            # 建立工作經歷
PUT    /api/workexperiences/{id}       # 更新工作經歷
DELETE /api/workexperiences/{id}       # 刪除工作經歷
```

### 技能管理 (Skills)
```
GET    /api/skills                     # 取得所有公開技能
GET    /api/skills/category/{category} # 依分類篩選技能
GET    /api/skills/level/{level}       # 依等級篩選技能
GET    /api/skills/{id}                # 取得指定技能
GET    /api/skills/user/{userId}       # 取得指定使用者技能
POST   /api/skills                     # 建立技能
PUT    /api/skills/{id}                # 更新技能
DELETE /api/skills/{id}                # 刪除技能
```

## 測試指令範例

### 基本查詢
```bash
curl http://localhost:5001/api/users
curl http://localhost:5001/api/skills
curl http://localhost:5001/api/educations
```

### 建立資料
```bash
# 建立使用者
curl -X POST http://localhost:5001/api/users \
  -H "Content-Type: application/json" \
  -d '{"username":"test","email":"test@test.com","passwordHash":"hash","firstName":"Test","lastName":"User","isActive":true}'

# 建立技能
curl -X POST http://localhost:5001/api/skills \
  -H "Content-Type: application/json" \
  -d '{"userId":1,"name":"Python","category":"程式語言","level":2,"description":"熟悉Python開發","isPublic":true}'
```

### 技能等級對照
```
0 = Beginner (初學者)
1 = Intermediate (中級)
2 = Advanced (進階)
3 = Expert (專家)
```

## 常見錯誤

### 400 - 資料驗證失敗
```json
{
  "success": false,
  "message": "資料驗證失敗",
  "data": null,
  "errors": ["Username is required"]
}
```

### 404 - 資源不存在
```json
{
  "success": false,
  "message": "找不到指定的資源",
  "data": null,
  "errors": []
}
```

### 409 - 資料重複
```json
{
  "success": false,
  "message": "使用者名稱已存在",
  "data": null,
  "errors": []
}
```

---

*詳細API文檔請參考: [api-documentation.md](./api-documentation.md)*