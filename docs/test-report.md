# 測試報告

| 項目 | 內容 |
| --- | --- |
| 測試日期 | 2026-10-01 |
| 受測版本 | `main` @ `bb4a1d2`（全面重構完成後） |
| 結論 | **通過**：自動化測試 474 項全數通過，靜態檢查無警告，29 個頁面在瀏覽器中正常運作；發現 1 項低嚴重度問題（見 [§6](#6-發現的問題)） |

## 1. 測試環境

| 項目 | 版本 |
| --- | --- |
| 作業系統 | Windows 11 Pro |
| .NET SDK | 9.0.315 |
| Node.js | 26.10.0 |
| 瀏覽器 | Google Chrome 153（Playwright 1.54.2 以 `channel: chrome` 驅動） |
| 前端 | Vue 3.5.18、Vite 7.1.1、TypeScript 5.8.3、Vitest 3.2.4 |
| 資料庫 | SQLite；每次測試使用新的暫存檔 |

## 2. 結果總覽

| 類別 | 工具 | 項目數 | 通過 | 失敗 | 耗時 |
| --- | --- | ---: | ---: | ---: | ---: |
| 後端整合／單元測試 | xUnit + WebApplicationFactory | 297 | 297 | 0 | 約 43 秒 |
| 前端單元測試 | Vitest | 169 | 169 | 0 | 約 18 秒 |
| E2E | Playwright | 8 | 8 | 0 | 約 55 秒 |
| **合計** | | **474** | **474** | **0** | |

| 靜態檢查 | 結果 |
| --- | --- |
| 後端建置 | 成功 |
| 兩組 migration 與模型一致（`has-pending-model-changes`，SQLite 與 MySQL） | 一致 |
| ESLint | 0 個問題 |
| Prettier | 全部符合格式 |
| TypeScript 型別檢查 + 正式建置（`npm run build`） | 成功 |
| 前端 API 型別與 `backend/openapi.json` 一致（`npm run api:types` 後無差異） | 一致 |
| 瀏覽器實測（29 個頁面） | 全部正常載入；僅有預期內的訪客 refresh 401（見 §6） |

## 3. 後端測試（297 項）

每個 API 測試都在記憶體中啟動完整的 API，以 HTTP 呼叫，搭配獨立的 SQLite 暫存檔。

| 分類 | 測試類別 | 項目數 | 涵蓋重點 |
| --- | --- | ---: | --- |
| 認證 | `AuthApiTests` | 22 | 登入、註冊、refresh 輪換與重用偵測、登出、忘記與重設密碼、變更密碼 |
| | `AuthRateLimitTests` | 2 | 登入超過限制回 429；refresh 使用獨立額度 |
| | `FirstAdminBootstrapTests` | 1 | 設定的 Email 註冊後成為管理員 |
| 管理員 | `AdminUsersApiTests` | 9 | 限管理員；不能停用或降級自己 |
| 作品集 | `PortfoliosApiTests` | 28 | 區塊驗證、檔案只能引用自己的、slug 唯一、公開卡片、排序 |
| 文章 | `BlogApiTests` | 29 | 草稿與排程不公開、HTML 清洗、標籤、分頁搜尋、瀏覽數 |
| 個人資料與經歷 | `ProfilesApiTests` | 12 | 公開資料不含 Email、停用帳號回 404、目錄搜尋與分頁、設定值驗證 |
| | `EducationsApiTests`、`WorkExperiencesApiTests` | 24 | 共用契約 + 日期規則 |
| | `SkillsApiTests` | 13 | 共用契約 + 選填等級與年資 |
| | `ContactMethodsApiTests` | 19 | 共用契約 + 值須符合類型、拒絕 `javascript:`／`data:` 網址 |
| 留言板 | `GuestbookApiTests` | 13 | 未審核不公開、不洩漏 Email、回覆 |
| | `GuestbookRateLimitTests` | 1 | 留言超過限制回 429 |
| 個人工具 | `CalendarApiTests` | 17 | 重複行程展開、公開行程、時間範圍 |
| | `TodosApiTests` | 7 | 所有權、狀態篩選、完成與重開時的完成時間 |
| | `ProjectsApiTests` | 8 | 共用契約 + 顏色驗證 |
| | `WorkTasksAndTimeEntriesApiTests` | 13 | 實際工時由時間紀錄加總、刪除專案時任務保留、不能掛到別人的專案或任務、依期間統計 |
| 檔案 | `FilesApiTests` | 15 | 上傳、刪除、使用狀況查詢、以 `nosniff` 提供檔案 |
| | `FileInspectorTests` | 26 | 以 magic bytes 判斷類型、偽裝的檔案被拒 |
| | `FileSizeLimitTests` | 1 | 超過 50 MB 被拒 |
| 共用元件 | `ErrorHandlingMiddlewareTests` | 8 | 例外轉狀態碼、500 不洩漏內部訊息 |
| | `CurrentUserTests` | 3 | 從 JWT 取得目前使用者 |
| 資料 | `DataMigrationTests` | 7 | 舊資料遷移後仍正確（例如作品改為區塊式內容） |
| | `ModelIndexTests` | 8 | token 查詢欄位唯一、標籤每人唯一、擁有者欄位有索引 |
| 啟動與設定 | `EnvironmentBehaviourTests` | 6 | 示範資料、Swagger、CORS 只在 Development |
| | `JwtConfigurationTests` | 3 | 金鑰缺少或太短時拒絕啟動 |
| | `HealthCheckTests` | 1 | 健康檢查端點 |
| 合約 | `OpenApiDocumentTests` | 1 | 提交的 `openapi.json` 與 API 一致 |

清單型資源（學歷、經歷、技能、聯絡方式、專案）都繼承共用的契約測試，涵蓋：未登入回 401、只列出自己的資料、
修改或刪除別人的資料回 404 且資料不變、排序，以及公開頁面只顯示公開項目。

## 4. 前端單元測試（169 項）

| 範圍 | 檔案 | 項目數 | 涵蓋重點 |
| --- | --- | ---: | --- |
| HTTP 層 | `api/http.spec.ts` | 16 | 401 自動 refresh 後重送、多個 401 共用一次 refresh、只重試 GET、錯誤轉換 |
| 登入狀態 | `stores/auth.spec.ts` | 11 | 登入、登出、還原、session 過期 |
| | `stores/toast.spec.ts` | 3 | 提示訊息 |
| 路由 | `router/routes.spec.ts` | 10 | 路由對應、舊網址導向、權限守衛 |
| Composable | `useAsyncData`、`useAsyncAction`、`useOwnedList`、`useWorkEditor`、`useColorScheme`、`useAccent` | 31 | 只採用最後一次結果、防重複送出、自動儲存、主題 |
| 純函式 | `lib/` 下 11 個檔案 | 72 | 嵌入白名單、HTML 清洗、程式碼上色、日期格式、閱讀時間、行事曆、計時器、安全導向、作品與文章資料轉換 |
| 元件 | `CaseBlocks`、`CoverCarousel`、`WorkCard`、`TagInput`、`DeleteButton`、`FileCard` | 26 | 區塊渲染、輪播、卡片版型、標籤輸入、刪除確認 |

## 5. E2E 測試（8 項）

Playwright 自動啟動後端（Development 環境、新的 SQLite 檔案與示範資料）與前端，以 Chrome 執行。

| # | 情境 | 結果 |
| ---: | --- | --- |
| 1 | 未登入進入後台會先到登入頁，登入後回到原本的頁面 | ✅ |
| 2 | 重新整理後仍保持登入（以 httpOnly cookie 還原） | ✅ |
| 3 | 登出後無法再進入後台 | ✅ |
| 4 | 個人首頁依序呈現介紹、作品與經歷，點作品可看完整內容 | ✅ |
| 5 | 舊的作品集網址會導向新的作品頁 | ✅ |
| 6 | 訪客留言後會看到等待審核的說明 | ✅ |
| 7 | 可以切換深色模式，並在重新整理後保留 | ✅ |
| 8 | 建立作品：文字與圖庫區塊、上傳圖片與說明、自動儲存後公開 | ✅ |

另以建置後的前端（`vite preview`，模擬原 CI 的設定）再執行一次，8 項同樣通過。

## 6. 發現的問題

| ID | 嚴重度 | 描述 | 影響 | 建議 |
| --- | --- | --- | --- | --- |
| F-01 | 低 | 未登入的訪客每次開啟網站，前端還原登入狀態時呼叫 `POST /api/auth/refresh`，因為沒有 cookie 而回 401，瀏覽器 console 會顯示一筆錯誤 | 功能正常；但每位訪客多一次請求，console 有錯誤訊息，容易讓人誤以為出錯 | 登入時另存一個不含機密的「曾登入」標記（例如 localStorage 的布林值），沒有標記就略過還原；或讓 refresh 在沒有 cookie 時回 204。已記錄於 `TASKS.md` 技術債 |

測試期間沒有發現功能錯誤。

## 7. 瀏覽器實測

以 Playwright 驅動 Chrome，在新的資料庫中載入示範資料，並透過 API 加入作品封面、大頭照、本月行程與時間紀錄後，逐頁開啟並截圖。
每個頁面都記錄 console 錯誤、未捕捉的例外，以及狀態碼 ≥ 400 的 API 回應。

| 範圍 | 頁面 | 結果 |
| --- | --- | --- |
| 前台（淺色，1440×900） | 使用者目錄、個人首頁、作品列表、作品詳情、文章列表、文章、留言板、行事曆 | 正常；僅 F-01 |
| 前台（深色） | 個人首頁、作品詳情 | 正常；僅 F-01 |
| 手機（390×844） | 個人首頁、作品詳情 | 正常；僅 F-01 |
| 登入 | 登入頁 | 正常；僅 F-01 |
| 後台 | 儀表板、個人資料、作品、作品編輯器、文章、文章編輯器、經歷、技能、留言、檔案、行事曆、待辦、工作追蹤、聯絡方式、修改密碼、使用者管理 | 全部正常，無任何錯誤 |

截圖收錄於 [`images/screenshots/`](images/screenshots/)，README 有完整的畫面導覽。

## 8. 未涵蓋的範圍

| 項目 | 說明 |
| --- | --- |
| 程式碼覆蓋率 | 專案沒有安裝覆蓋率工具（如 coverlet、`@vitest/coverage-v8`），本次未量測 |
| MySQL／MariaDB 實際執行 | 整合測試只在 SQLite 執行；MySQL 只確認 migration 與模型一致 |
| Playwright 內建的 Chromium、Firefox、Safari | 只以本機 Chrome 測試 |
| 效能與負載 | 未進行 |
| 正式環境（HTTPS、反向代理） | 目前沒有正式環境；部署條件見 [`deployment-guide.md`](deployment-guide.md) |
| 寄送 Email | 未設定 SMTP，重設密碼信只寫入 log；流程由整合測試以替身驗證 |

## 9. 如何重現

執行 [`development-guide.md`](development-guide.md#測試與檢查)「測試與檢查」的所有指令；瀏覽器實測需同時啟動後端與前端。
