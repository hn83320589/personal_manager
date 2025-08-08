# 資料庫設計文檔

## 概述

Personal Manager 系統的資料庫設計，採用關聯式資料庫 (MariaDB) 儲存所有系統資料。

## 實體關係圖 (ERD)

*待補充 - 將在資料庫設計階段完成*

## 資料表設計

### 使用者相關

#### Users (使用者)
| 欄位名稱 | 資料型別 | 限制 | 說明 |
|---------|---------|------|------|
| Id | int | PK, AI | 使用者ID |
| Username | varchar(50) | NOT NULL, UNIQUE | 使用者名稱 |
| Email | varchar(100) | NOT NULL, UNIQUE | 電子郵件 |
| PasswordHash | varchar(255) | NOT NULL | 密碼雜湊 |
| CreatedAt | datetime | NOT NULL | 建立時間 |
| UpdatedAt | datetime | NULL | 更新時間 |

#### Profiles (個人檔案)
*待設計*

### 內容管理相關

#### BlogPosts (部落格文章)
*待設計*

#### Comments (留言)
*待設計*

### 任務管理相關

#### Tasks (待辦事項)
*待設計*

#### WorkTrackings (工作追蹤)
*待設計*

### 行事曆相關

#### CalendarEvents (行事曆事件)
*待設計*

## 索引設計

*待補充*

## 資料庫約束

*待補充*

## 效能優化

*待補充*

---

**注意**: 此文檔將在資料庫設計階段詳細補充完整。