# 資料庫設計文檔

## 概述

Personal Manager 使用 MariaDB（生產環境）或本地 JSON 檔案（開發 fallback）。Schema 由 EF Core `EnsureCreated()` 自動建立，不需要手動 migration。

---

## 實體關係圖

```
Users (1) ──── (1) PersonalProfiles
Users (1) ──── (N) Educations
Users (1) ──── (N) WorkExperiences
Users (1) ──── (N) Skills
Users (1) ──── (N) Portfolios
Users (1) ──── (N) CalendarEvents
Users (1) ──── (N) TodoItems
Users (1) ──── (N) WorkTasks
Users (1) ──── (N) BlogPosts
Users (1) ──── (N) ContactMethods
GuestBookEntries.TargetUserId ──── (N:1) Users
```

---

## 資料表設計

### Users（使用者）

| 欄位 | 型別 | 限制 | 說明 |
|------|------|------|------|
| Id | int | PK, AI | 使用者 ID |
| Username | varchar(50) | NOT NULL, UNIQUE | 帳號名稱 |
| Email | varchar(100) | NOT NULL, UNIQUE | 電子郵件 |
| PasswordHash | varchar(255) | NOT NULL | BCrypt 雜湊密碼 |
| FullName | varchar(100) | | 顯示名稱 |
| Role | varchar(20) | 預設 `User` | 角色（User / Admin） |
| IsActive | bool | 預設 true | 帳號啟用狀態 |
| CreatedAt | datetime | NOT NULL | 建立時間 |
| UpdatedAt | datetime | NOT NULL | 更新時間 |

### PersonalProfiles（個人資料）

| 欄位 | 型別 | 限制 | 說明 |
|------|------|------|------|
| Id | int | PK, AI | |
| UserId | int | FK → Users | 所屬用戶 |
| Title | varchar(100) | | 職稱 |
| Summary | varchar(500) | | 一行摘要 |
| Description | text | | 詳細自介 |
| ProfileImageUrl | varchar(500) | | 大頭照 URL |
| Website | varchar(200) | | 個人網站 |
| Location | varchar(100) | | 所在地 |
| ThemeColor | varchar(50) | 預設 `blue` | 主題色（blue/green/purple/rose/slate） |
| CreatedAt | datetime | | |
| UpdatedAt | datetime | | |

### Educations（學歷）

| 欄位 | 型別 | 限制 | 說明 |
|------|------|------|------|
| Id | int | PK, AI | |
| UserId | int | FK → Users | |
| School | varchar(200) | NOT NULL | 學校名稱 |
| Degree | varchar(100) | | 學位 |
| FieldOfStudy | varchar(200) | | 科系 |
| StartYear | int? | | 入學年 |
| EndYear | int? | | 畢業年（null = 就讀中） |
| Description | text | | 說明 |
| IsPublic | bool | 預設 true | 是否公開 |
| SortOrder | int | | 排序 |
| CreatedAt / UpdatedAt | datetime | | |

### WorkExperiences（工作經歷）

| 欄位 | 型別 | 限制 | 說明 |
|------|------|------|------|
| Id | int | PK, AI | |
| UserId | int | FK → Users | |
| Company | varchar(200) | NOT NULL | 公司名稱 |
| Position | varchar(200) | NOT NULL | 職位 |
| StartDate | datetime? | | 到職日 |
| EndDate | datetime? | | 離職日 |
| IsCurrent | bool | | 是否在職 |
| Description | text | | 職責說明 |
| IsPublic | bool | 預設 true | |
| SortOrder | int | | |
| CreatedAt / UpdatedAt | datetime | | |

### Skills（技能）

| 欄位 | 型別 | 限制 | 說明 |
|------|------|------|------|
| Id | int | PK, AI | |
| UserId | int | FK → Users | |
| Name | varchar(100) | NOT NULL | 技能名稱 |
| Category | varchar(50) | | 分類 |
| Level | enum | Beginner/Intermediate/Advanced/Expert | 等級 |
| YearsOfExperience | int | | 年資 |
| IsPublic | bool | 預設 true | |
| SortOrder | int | | |
| CreatedAt / UpdatedAt | datetime | | |

### Portfolios（作品集）

| 欄位 | 型別 | 限制 | 說明 |
|------|------|------|------|
| Id | int | PK, AI | |
| UserId | int | FK → Users | |
| Title | varchar(200) | NOT NULL | 專案名稱 |
| Description | text | | 專案說明 |
| ImageUrl | varchar(500) | | 封面圖 URL |
| ProjectUrl | varchar(500) | | 專案連結 |
| RepositoryUrl | varchar(500) | | 原始碼連結 |
| Technologies | text | | 使用技術（逗號分隔） |
| IsFeatured | bool | | 是否精選 |
| IsPublic | bool | 預設 true | |
| SortOrder | int | | |
| CreatedAt / UpdatedAt | datetime | | |

### CalendarEvents（行事曆事件）

| 欄位 | 型別 | 限制 | 說明 |
|------|------|------|------|
| Id | int | PK, AI | |
| UserId | int | FK → Users | |
| Title | varchar(200) | NOT NULL | 事件標題 |
| Description | text | | 說明 |
| StartTime | datetime | NOT NULL | 開始時間 |
| EndTime | datetime | NOT NULL | 結束時間 |
| IsAllDay | bool | | 是否全天事件 |
| IsPublic | bool | | 是否公開 |
| Color | varchar(20) | | 顯示顏色（hex） |
| CreatedAt / UpdatedAt | datetime | | |

### TodoItems（待辦事項）

| 欄位 | 型別 | 限制 | 說明 |
|------|------|------|------|
| Id | int | PK, AI | |
| UserId | int | FK → Users | |
| Title | varchar(200) | NOT NULL | 標題 |
| Description | text | | 說明 |
| Priority | enum | Low/Medium/High | 優先度 |
| Status | enum | Pending/InProgress/Completed | 狀態 |
| DueDate | datetime? | | 截止日 |
| CompletedAt | datetime? | | 完成時間 |
| CreatedAt / UpdatedAt | datetime | | |

### WorkTasks（工作任務）

| 欄位 | 型別 | 限制 | 說明 |
|------|------|------|------|
| Id | int | PK, AI | |
| UserId | int | FK → Users | |
| Title | varchar(200) | NOT NULL | 任務名稱 |
| Description | text | | 說明 |
| Project | varchar(100) | | 所屬專案名稱（字串，非外鍵） |
| Priority | enum | Low/Medium/High/Urgent | 優先度 |
| Status | enum | Pending/Planning/InProgress/Testing/Completed/OnHold/Cancelled | 狀態 |
| EstimatedHours | double | | 預估工時 |
| ActualHours | double | | 實際工時 |
| Tags | text | | 標籤（逗號分隔） |
| DueDate | datetime? | | 截止日 |
| CompletedAt | datetime? | | 完成時間 |
| CreatedAt / UpdatedAt | datetime | | |

> 注意：WorkTask 沒有獨立的 Project 實體，project 只是任務上的字串欄位。時間計時記錄（TimeEntry）存在前端 localStorage，後端無對應資料表。

### BlogPosts（部落格文章）

| 欄位 | 型別 | 限制 | 說明 |
|------|------|------|------|
| Id | int | PK, AI | |
| UserId | int | FK → Users | |
| Title | varchar(200) | NOT NULL | 文章標題 |
| Slug | varchar(200) | UNIQUE | URL 識別碼 |
| Content | text | | Markdown 內容 |
| Summary | varchar(500) | | 摘要 |
| Category | varchar(50) | | 分類（字串，無獨立分類表） |
| Tags | text | | 標籤（逗號分隔） |
| Status | enum | Draft/Published/Archived | 狀態 |
| IsPublic | bool | 預設 true | |
| ViewCount | int | 預設 0 | 瀏覽次數 |
| PublishedAt | datetime? | | 發布時間 |
| CreatedAt / UpdatedAt | datetime | | |

### GuestBookEntries（訪客留言）

| 欄位 | 型別 | 限制 | 說明 |
|------|------|------|------|
| Id | int | PK, AI | |
| TargetUserId | int | FK → Users | 留言目標用戶（每人獨立留言板） |
| Name | varchar(100) | NOT NULL | 留言者姓名 |
| Email | varchar(200) | | 留言者 Email |
| Message | text | NOT NULL | 留言內容 |
| IsApproved | bool | 預設 false | 是否通過審核 |
| AdminReply | text | | 管理員回覆 |
| CreatedAt / UpdatedAt | datetime | | |

### ContactMethods（聯絡方式）

| 欄位 | 型別 | 限制 | 說明 |
|------|------|------|------|
| Id | int | PK, AI | |
| UserId | int | FK → Users | |
| Type | enum | Email/Phone/LinkedIn/GitHub/Facebook/Twitter/Instagram/Discord/Other | 類型 |
| Label | varchar(50) | | 自訂標籤 |
| Value | varchar(500) | NOT NULL | 聯絡值（URL / Email / 電話） |
| Icon | varchar(50) | | 圖示名稱（預留） |
| IsPublic | bool | 預設 true | |
| SortOrder | int | | |
| CreatedAt / UpdatedAt | datetime | | |

---

## 索引設計

以下索引由 `DatabaseSeeder.cs` 在啟動時自動建立：

| 資料表 | 欄位 | 說明 |
|--------|------|------|
| Users | Username | UNIQUE |
| Users | Email | UNIQUE |
| PersonalProfiles | UserId | |
| BlogPosts | Slug | UNIQUE |
| BlogPosts | UserId, Status | 複合索引 |
| GuestBookEntries | TargetUserId, IsApproved | 複合索引 |
| Skills | UserId | |
| CalendarEvents | UserId, StartTime | 複合索引 |

---

## Schema 異動注意事項

- **本地開發（JSON 模式）**：不受 schema 影響，直接讀寫 `Data/JsonData/*.json`
- **本地開發（DB 模式）**：`EnsureCreated()` 只在 DB 不存在時建立，若已存在不會異動。欄位新增需手動 `ALTER TABLE` 或 DROP + 重建
- **生產環境（Zeabur）**：同上，schema 異動需在 Zeabur 的 MariaDB 控制台手動執行 SQL
