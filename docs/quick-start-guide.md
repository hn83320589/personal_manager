# Personal Manager 快速入門指南

## 5 分鐘上手

### 1. 瀏覽公開頁面

開啟瀏覽器，進入首頁（`/`）可以看到所有已註冊用戶的目錄卡片。

點選任一用戶卡片進入個人頁面（`/@username`），可瀏覽：

| 頁面 | 網址 |
|------|------|
| 關於我 | `/@username` |
| 學經歷 | `/@username/experience` |
| 技能專長 | `/@username/skills` |
| 作品集 | `/@username/portfolio` |
| 部落格 | `/@username/blog` |
| 公開行事曆 | `/@username/calendar` |
| 留言板 | `/@username/guestbook` |
| 聯絡我 | `/@username/contact` |

> 所有公開頁面無需登入即可瀏覽。

---

### 2. 登入管理後台

前往 `/login` 使用帳號密碼登入。登入後進入管理後台（`/admin/dashboard`）。

後台功能一覽：

| 功能 | 網址 |
|------|------|
| 儀表板 | `/admin/dashboard` |
| 個人資料 + 主題設定 | `/admin/profile` |
| 學經歷管理 | `/admin/experience` |
| 技能管理 | `/admin/skills` |
| 作品集管理 | `/admin/projects` |
| 聯絡方式管理 | `/admin/contacts` |
| 行事曆管理 | `/admin/calendar` |
| 工作追蹤 | `/admin/work-tracking` |
| 待辦事項 | `/admin/tasks` |
| 文章管理 | `/admin/blog` |
| 留言管理 | `/admin/comments` |

---

### 3. 本地開發快速啟動

```bash
# 終端 1 — 後端
cd local-development/PersonalManagerBackend
dotnet run
# → http://localhost:5037  （Swagger: /swagger）

# 終端 2 — 前端
cd local-development/PersonalManagerFrontend
npm install && npm run dev
# → http://localhost:5173
```

後端若無法連接 MariaDB，會自動 fallback 至本地 JSON 資料，可正常開發。

---

### 4. 常用指令

```bash
# 前端型別檢查
npm run type-check

# 前端建置
npm run build

# 後端建置檢查
dotnet build
```

---

更多詳情請參考：
- [API 快速參考](./api-quick-reference.md)
- [部署指南](./deployment-guide.md)
- [資料庫設計](./database-design.md)
