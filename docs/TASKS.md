# Personal Manager — 任務清單

> 最後更新：2026-10-07

---

## 待辦

目前沒有待辦項目。

- [x] F-01：訪客開啟網站時 `POST /api/auth/refresh` 回 401、console 出現錯誤 → 沒有 refresh cookie 時改回 204，前端視為未登入（2026-10-07）

---

## 2026 重構（已完成）

2026-09～10 的全面重構紀錄。之後移除了 GitHub Actions CI（下方提到 CI 的項目為當時狀態），重構前的任務清單已刪除，歷史見 git log。

完整計畫：重構計畫頁面（claude.ai artifact「Personal Manager 重構計畫」）。已確認的決策：
- D1 移除 JSON fallback，本地與暫時的執行環境一律使用 SQLite，保留 Pomelo（MySQL/MariaDB）套件以便日後切換
- D2 合併為 monorepo（`backend/`、`frontend/`、`docs/`）
- D3 refresh token 改用 httpOnly cookie，access token 只放記憶體
- D4 引入 DOMPurify、HtmlSanitizer、ESLint + Prettier、openapi-typescript、GitHub Actions；部落格編輯器另加 `@tiptap/extension-code-block-lowlight` + `lowlight`、`@tiptap/extension-character-count`、`@tiptap/suggestion`（2026-09-30 核准）
- D5 後端改為 feature folder
- 目前沒有雲端環境（Zeabur 已停用），不使用 Docker

### Phase 0 — 安全網
- [x] 合併前後端 repo 為 monorepo（git subtree，保留歷史）
- [x] 修復後端測試專案編譯（29 個測試恢復執行）
- [x] 修復前端 8 個失敗的單元測試（改為驗證行為）
- [x] 新增 GitHub Actions CI（後端 build/test、前端 test/type-check/build）
- [x] 整理 git 追蹤範圍（`.gitignore` 改為 monorepo 版本）
- [x] `.sln` 與測試專案搬遷（於 Phase 1 完成）

### Phase 1 — 後端基礎
- [x] 後端改為 `src/`、`tests/` 同層並建立 `PersonalManager.sln`
- [x] 預設 SQLite、`Database:Provider` 切換、兩組 migration；移除 JSON fallback
- [x] 整合測試基礎（WebApplicationFactory + 暫存 SQLite）
- [x] 索引改由 `HasIndex` 管理，移除 `DatabaseSeeder.CreateIndexesAsync` 的 raw SQL；補上 token、Tag、留言板 TargetUserId 等缺少的索引
- [x] 移除寫死的 JWT 金鑰，啟動時驗證 `Jwt:SecretKey`（背景安全審查發現原始碼內有預設金鑰）
- [x] seeder 只在 Development 執行；CORS 來源改從設定讀取；Swagger 只在 Development 開啟
- [x] `Program.cs` 拆成 `Setup/` 下的擴充方法（250 行 → 30 行）；啟動訊息改用 `ILogger`
- [x] `ICurrentUser` 與統一錯誤處理（不外洩例外訊息）
- [-] feature folder 骨架 → 併入 Phase 2，各 feature 重建時直接建立於 `Features/`

### Phase 2 — 後端功能重建（public／me／admin）
- [x] 範本：技能（熟練度與年資改為選填、排序 API、13 個整合測試）
- [x] 個人資料／使用者目錄（新增作品集模式、卡片版型與比例、技能顯示方式、目前狀態；目錄支援搜尋與分頁）
- [x] 經歷、學歷（日期驗證、在職中忽略結束日期、經歷日期改為 DateOnly）
- [x] 聯絡方式（新增 Behance、Dribbble、YouTube、Threads、LINE、個人網站；拒絕 javascript:／data: 等非 http(s) 連結）
- [x] 部落格（封面圖、HtmlSanitizer 清洗、嵌入白名單、每位使用者範圍內唯一的 slug、排程、閱讀時間、DB 端分頁與篩選、原子遞增瀏覽數；標籤改用 Tag 資料表並提供 `/api/me/tags`）
- [x] 留言板（公開不含 Email、只回傳已審核、回覆時間、移除 TargetUserId 預設值 1；流量限制改為可設定）
- [x] 行事曆（重複規則由後端展開、查詢區間最長一年、顏色格式驗證；所有時間一律以 UTC 存取）
- [x] 待辦、工作追蹤（實際時數改由時間紀錄加總、時間紀錄改用 DateOnly／TimeOnly 並自動計算時長、移除重複的任務與專案名稱、統計 API）
- [x] 檔案上傳（副檔名與 magic bytes 須一致、不收 SVG／原始檔、伺服器判定 MIME、記錄圖片寬高、50 MB 上限且超過時不先寫入暫存、nosniff、S3 改用伺服器判定的 Content-Type）
- [x] Auth（httpOnly cookie refresh token、token 雜湊、輪換與重用偵測、登入與 refresh 檢查停用帳號、重設／修改密碼後撤銷全部工作階段、帳號限英數底線連字號、密碼至少 8 碼、access token 15 分鐘）
- [x] Admin：使用者列表、停用／啟用（停用時結束工作階段）、角色變更（不可停用或降級自己）；第一位管理員以 `Admin:BootstrapEmails` 建立
- [x] 移除 `IRepository`、`CrudService`、`BaseApiController` 與舊的 DTO／Mapping 檔（Phase 3 重建作品集時一併移除）

### Phase 3 — 作品集（ADR-012）
- [x] 區塊式作品：封面輪播與裁切重點、角色／期間與自訂欄位、連結、七種內容區塊（文字、圖片、圖庫、附件、嵌入、重點數字、程式碼）
- [x] 伺服器依使用者自己上傳的檔案填入網址、尺寸與檔名；文字區塊清洗 HTML；嵌入白名單；外部圖片限 https
- [x] 公開卡片列表（分類、標籤篩選與 facets）、以 slug 取得單件作品；後台整份儲存與排序
- [x] 移除 `PortfolioAttachment`；migration 保留既有作品的標題並產生不重複的 slug（含資料遷移測試）

### Phase 4 — 前端基礎
- [x] 刪除死碼與未使用的套件（7 個元件／頁面／store、8 個套件、未使用的測試輔助與腳本）
- [x] TypeScript 恢復 `strict`
- [x] ESLint + Prettier、`lint`／`format` script，納入 CI（順帶修正按逗號無法新增標籤的 bug）
- [x] openapi-typescript：後端提交 `backend/openapi.json`（測試檢查與 API 一致），前端 `npm run api:types` 產生型別（CI 檢查是否最新）
- [x] 新 HTTP 層：access token 只放記憶體、refresh token 走 httpOnly cookie、多個 401 共用同一次 refresh、只有 GET 會重試
- [x] auth store 與登入、忘記密碼、重設密碼頁改接新 API（瀏覽器實測：登入、重新整理後還原、登出後需重新登入）
- [-] 其他資源的 API 模組 → 移到 Phase 5，與使用它的畫面一起建立
- [x] `v-html` 一律經過 DOMPurify
- 說明：其他舊頁面在 Phase 5 依新設計重寫時，才改用新 API 並刪除對應的舊 service／store，避免為即將淘汰的畫面做轉接；
  重寫時同步從 `eslint.config.js` 的 legacy 清單移除，最後刪除 `services/`、`types/api.ts`
- 部落格編輯器要用的 Tiptap 套件（code-block-lowlight、character-count、suggestion）在 Phase 5 升級編輯器時才安裝

### Phase 5 — 前台重新設計與後台簡化（依 prototype）
基礎
- [x] 設計 token（紙、面、墨、次要文字、分隔線、主題色）以 CSS 變數定義並對應 Tailwind 顏色；深淺色、5 種主題色；字體
- [x] 前台 API 模組（依 username 呼叫 /api/public）

前台
- [x] 前台版面：精簡導覽（作品、文章、經歷、聯絡）、手機選單、頁尾（留言板、行事曆、登入）；舊網址導向新網址
- [x] 個人首頁：介紹 → 作品 → 經歷與技能 → 最近的文章 → 聯絡
- [x] 作品列表：分類篩選、三種卡片（圖像／資訊／技術）與圖像比例、封面輪播
- [x] 作品詳情：七種區塊、lightbox、嵌入、程式碼、附件、上一件／下一件（程式碼語法上色已加入）
- [x] 文章列表與文章頁（標籤篩選、目錄、閱讀時間）
- [x] 留言板、公開行事曆、使用者目錄首頁改用新設計

後台
- [x] 後台版面：分組側欄、手機可用；共用「列表 + 側邊編輯面板」與 `useAsyncAction`
- [x] 個人資料與前台呈現設定（作品集模式、卡片版型與比例、技能顯示、目前狀態、主題色）
- [x] 經歷／學歷、技能（可自訂標籤，系統建議作為備援）、聯絡方式
- [x] 作品編輯器：左側作品資訊、主區區塊列表；多圖上傳、逐張說明、排序、設為封面、裁切重點；自動儲存
- [x] 部落格：文章列表、編輯器升級（程式碼上色、字數、「/」選單、自動儲存、封面圖）
- [x] 留言管理、檔案管理（刪除仍被引用的檔案前提示）
- [x] 行事曆、待辦、工作追蹤（專案、任務、時間紀錄）
- [x] 帳號：修改密碼；管理員：使用者管理；儀表板；登入／註冊／忘記與重設密碼頁（新增註冊頁）

收尾
- [x] 刪除舊的 services／stores／types/api.ts／舊元件，移除 eslint legacy 清單
- [x] E2E：前台首頁、作品詳情、留言、深色模式、登入與工作階段還原、後台作品編輯（8 個測試；CI 新增 e2e job，以 SQLite 後端與建置後的前端執行）
- 完成條件：沒有超過 400 行的 .vue（最大 285 行）；每個 store 都有單元測試（auth、toast）——已達成

### Phase 6 — 文件
- [x] 三份 CLAUDE.md 的「最新異動記錄」移出，CLAUDE.md 只保留規則與指引（曾集中到 `CHANGELOG.md`，2026-10-07 改以 git log 為準而刪除）
- [x] 刪除過時的 Postman collection 與 `api-quick-reference.md`（API 以 Swagger 與 `backend/openapi.json` 為準）
- [x] `development-guide.md` 改寫為實際的本地開發流程
- [x] `deployment-guide.md` 改寫：目前沒有正式環境，列出部署時的必要條件（同網站、環境變數、資料庫切換）
- [x] `database-design.md` 改寫為目前的資料表
- [x] `system-specification.md` 改寫第 1–11 章為目前的架構，移除 Zeabur 與已不存在的設計；保留第 12 章 ADR
- [x] 根目錄與前後端 README 更新
- [x] 所有文件不再提到 Zeabur 為現行環境
- [x] 移除 GitHub Actions CI，原本的檢查（migration、API 型別）改列在 `development-guide.md`「測試與檢查」
- [x] 新增技術教學 `technical-guide.md`（技術、方法、設計理由與實作練習）
- [x] 完整測試並撰寫 `test-report.md`（自動化 474 項 + 29 個頁面瀏覽器實測）
- [x] README 加上功能說明與畫面導覽（截圖在 `docs/images/screenshots/`）

### 技術債（重構期間發現）
- [x] 依 [`optimization-plan.md`](optimization-plan.md) 進行程式碼精簡：階段 1、2
- [x] 優化計畫階段 3（依建議：3-1、3-2、3-4～3-8；3-3、3-9 維持現狀）
- [x] 第一位管理員以 `Admin:BootstrapEmails` 建立
- [x] 【Bug】部落格「特色圖片」從未被儲存 → 後端已新增 `CoverImageUrl`，新的部落格編輯器可上傳封面
- [x] 移除 `WorkTask.Tags` 字串欄位
- [x] `dotnet-ef` 以 local tool 鎖定 9.0.13；CI 檢查兩組 migration 是否都已產生
- [x] 刪除仍被作品引用的上傳檔案時，作品中的圖片或附件會失效 → 新增 `GET /api/me/files/{id}/usages`，後台刪除前列出使用中的作品、文章與個人資料

---

*更新方式：新的工作加在「待辦」；完成後將 `[ ]` 改為 `[x]`。變更的細節見 git log*
