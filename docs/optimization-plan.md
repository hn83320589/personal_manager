# 優化計畫

> 建立日期：2026-10-07。來源：全專案掃描（後端、前端、倉庫與文件三路平行檢查，關鍵項目再以 grep 驗證）。

掃描結論：專案整體乾淨。所有 `.vue` 都有被使用、每個 CSS class 與 Tailwind token 都有用到，所有 NuGet 與 npm 套件都有用到（下列已移除者除外），文件連結全部有效。
主要的改善空間是**重複的程式碼**，以及少數**過時的文件與設定**。

## 已清除的未使用內容（2026-10-07）

確定沒有使用、刪除後行為不變的項目已直接清除，每批都通過建置與全部測試：

| Commit | 內容 |
| --- | --- |
| `dcf9b82` | 刪除 `PersonalManager.Api.http`（範本留下的 `/weatherforecast`）；`backend/.gitignore` 由 470 行精簡為 27 行；修正根目錄 `.gitignore` 誤忽略已提交的 `frontend/.vscode/extensions.json`、`frontend/.env.production`；移除 Cypress／Nightwatch 殘留設定；`robots.txt` 改為只擋後台與 API |
| `b166edc` | 後端：移除未使用的 `Moq`、`Microsoft.Extensions.Options` 套件，以及沒有呼叫端的 `Slugs.IsValid`、`FileInspector.AllowedExtensions`、`ApiResponse<T>.Fail` 與多餘的 `using` |
| `1d59ca2` | 前端：移除未使用的 `@tiptap/extension-text-align`、`@tiptap/extension-character-count`；移除沒有呼叫端的匯出（`HttpClient`、`SaveStatus` 轉出、`portfolioFacets`、`userRole`）；`vite.config`、`vitest.config` 移除預設值與需要未安裝套件的 coverage 設定 |
| `2b524d1` | 文件：修正與現況不符的說明（`appsettings.Development.json` 非必要、Settings 清單、migration 指令、E2E 瀏覽器、api 檔案清單） |

## 優化進度

### 階段 1、2：已完成（2026-10-07）

每項一個 commit，各自通過建置、全部測試與 E2E。

| # | 項目 | Commit | 結果 |
| --- | --- | --- | --- |
| 1-1、1-3 | 移除重複的 enum `JsonConverter` 屬性與 `AddEndpointsApiExplorer()` | `d27f251` | `openapi.json` 不變 |
| 1-2 | 示範資料不再重複實體預設值，角色改用 `Roles` 常數 | `4d0745b` | 少約 33 行 |
| 1-4 | 合併重複的測試 helper（`Paged<T>`、`Id()`、`PostCreatedAsync`、`UploadAsync`） | `122546e` | 少約 30 行 |
| 1-5、1-6 | 收斂只在檔內使用的匯出；明確宣告 `@tiptap/core` | `a385752` | |
| 2-1 | 編輯器的儲存狀態文字移進 `useAutosave`，離開提醒抽成 `useUnsavedChangesGuard` | `ead83ee` | 新增 4 項測試 |
| 2-2 | 分頁改用 `PaginationNav` 元件 | `3587734` | 6 個頁面少約 66 行，新增 4 項測試 |
| 2-3 | `formatTime`、`pad`、`timeRange`、`WEEKDAYS` 集中到 `lib` | `ae29af8` | 新增 4 項測試 |
| 2-4 | 排序改用 `ApplyOrderAsync` 查詢擴充 | `c583b57` | 行數差不多，「載入完整清單再排序」只在一處保證 |
| 2-5 | 9 處 `UpdatedAt` 改用注入的 `TimeProvider` | `4eb302f` | **未採用**原計畫的 `SaveChangesAsync` 統一設定：文章瀏覽數累加也會被當成修改 |
| 2-6 | 兩個富文本編輯器共用連結與內容同步（`editorCommands.ts`） | `0339df6` | 修正作品編輯器不接受 `mailto`、無效網址沒有提示；新增 4 項測試 |
| 2-7 | 固定標題由路由 `afterEach` 統一設定 | `99f28c3` | 標題格式一致，移除未使用的 `VITE_APP_TITLE`；新增 1 項測試 |
| 2-8 | 搜尋防抖改用 `useDebouncedSearch` | `ec71981` | 新增 2 項測試 |

前端單元測試由 169 項增加到 188 項。

### 階段 3：已決定並完成（2026-10-07）

| # | 項目 | 決定 | Commit |
| --- | --- | --- | --- |
| 3-1 | `ContactMethod.Icon` 欄位沒有任何地方讀取 | 移除；兩種資料庫各一組 migration，並補資料遷移測試 | `a2145ca` |
| 3-2 | `ForbiddenException`、`ICurrentUser.IsAdmin` 沒有被使用 | 移除（角色檢查由 `[Authorize(Roles)]` 負責） | `dad948e` |
| 3-3 | 後端作品分類統計 API 與篩選參數，前端目前沒呼叫 | **保留**：作品變多時改回伺服器端篩選 | — |
| 3-4 | `Services/`、`DTOs/` 與「依功能分資料夾」的說法不一致 | 調整技術教學的說法，資料夾不動 | `80b04b0` |
| 3-5 | `TASKS.md` 約 150 行描述舊架構 | 刪除，改為「待辦」＋「2026 重構紀錄」（276 行 → 130 行） | 本次文件 commit |
| 3-6 | 同樣的內容重複出現在多份文件 | 共通原則只留在根目錄 `CLAUDE.md`；檢查清單只在開發指南；測試數量只在測試報告 | 本次文件 commit |
| 3-7 | `playwright.config.ts` 的 `CI=1` 分支 | 保留，改為「提交前測建置後的前端」，兩種模式都用本機 Chrome | `697d406` |
| 3-8 | `e2e/` 沒有被型別檢查 | 納入 `tsconfig.json` 的 references | `697d406` |
| 3-9 | 截圖共 3.5 MB | **維持 PNG**：總大小可接受，文字較清晰 | — |

### 已知問題

- **F-01**（測試報告）：訪客開頁面時 refresh 回 401 留下 console 錯誤 → 已修正：沒有 refresh cookie 時回 204（2026-10-07）。

## 不建議做的

| 建議 | 原因 |
| --- | --- |
| 回到泛型 Repository／CrudService 以消除 service 間的重複 | 違反 ADR-011；重複的部分以小型查詢擴充方法處理即可（如 2-4） |
| 合併 `PostFacetsDto` 與 `PortfolioFacetsDto` | 會改變 OpenAPI schema 名稱與前端型別，換來的程式碼很少 |
| 為了少寫型別參數，新增 `ApiResponse.Of<T>` | 只改善閱讀，64 處呼叫端都要改 |
