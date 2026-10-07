# 優化計畫

> 建立日期：2026-10-07。來源：全專案掃描（後端、前端、倉庫與文件三路平行檢查，關鍵項目再以 grep 驗證）。

掃描結論：專案整體乾淨。所有 `.vue` 都有被使用、每個 CSS class 與 Tailwind token 都有用到，所有 NuGet 與 npm 套件都有用到（下列已移除者除外），文件連結全部有效。
主要的改善空間是**重複的程式碼**，以及少數**過時的文件與設定**。

## 已完成（2026-10-07）

確定沒有使用、刪除後行為不變的項目已直接清除，每批都通過建置與全部測試：

| Commit | 內容 |
| --- | --- |
| `dcf9b82` | 刪除 `PersonalManager.Api.http`（範本留下的 `/weatherforecast`）；`backend/.gitignore` 由 470 行精簡為 27 行；修正根目錄 `.gitignore` 誤忽略已提交的 `frontend/.vscode/extensions.json`、`frontend/.env.production`；移除 Cypress／Nightwatch 殘留設定；`robots.txt` 改為只擋後台與 API |
| `b166edc` | 後端：移除未使用的 `Moq`、`Microsoft.Extensions.Options` 套件，以及沒有呼叫端的 `Slugs.IsValid`、`FileInspector.AllowedExtensions`、`ApiResponse<T>.Fail` 與多餘的 `using` |
| `1d59ca2` | 前端：移除未使用的 `@tiptap/extension-text-align`、`@tiptap/extension-character-count`；移除沒有呼叫端的匯出（`HttpClient`、`SaveStatus` 轉出、`portfolioFacets`、`userRole`）；`vite.config`、`vitest.config` 移除預設值與需要未安裝套件的 coverage 設定 |
| `2b524d1` | 文件：修正與現況不符的說明（`appsettings.Development.json` 非必要、Settings 清單、migration 指令、E2E 瀏覽器、api 檔案清單） |

## 待辦

每項標示**效益**（減少的程式碼或風險）、**工作量**、**風險**。建議依階段順序進行，每項一個 commit，並跑完 [`development-guide.md`](development-guide.md#測試與檢查) 的檢查。

### 階段 1：小而確定（不改行為）

| # | 項目 | 位置 | 效益 | 工作量 | 風險 |
| --- | --- | --- | --- | --- | --- |
| 1-1 | 移除重複的 enum `[JsonConverter(typeof(JsonStringEnumConverter))]` 屬性：全域已註冊轉換器 | `Models/BlogPost.cs`、`ContactMethod.cs`、`Skill.cs`、`TodoItem.cs`、`WorkTask.cs` | 少 7 個屬性與 5 個 `using` | 小 | 低：`OpenApiDocumentTests` 會確認輸出不變 |
| 1-2 | `DatabaseSeeder` 移除與實體預設值重複的 `CreatedAt`／`UpdatedAt`／`IsActive`，角色改用 `Roles.Admin`／`Roles.User` 常數（`Models/User.cs` 同理） | `Data/DatabaseSeeder.cs` | 約 45 行 | 小 | 低 |
| 1-3 | 移除 `services.AddEndpointsApiExplorer()`：`AddControllers()` 已註冊 | `Setup/ApiSetup.cs` | 1 行 | 小 | 低：合約測試確認 |
| 1-4 | 測試共用 helper：`Paged<T>` 重複宣告 4 次、`Id(JsonElement)` 5 次、「POST 後確認 201 再讀 data」7 次、`FilesApiTests.UploadOk` 與 `TestFiles.UploadAsync` 重複 | `tests/…/Features/*.cs`、`Infrastructure/` | 約 60 行 | 小 | 低 |
| 1-5 | 前端去掉只在檔內使用的 `export`（`accentColors`、`AutosaveOptions`、`ColorScheme`、`SeoMeta`、`TocItem`、`resolveLanguage`、`toLocalInput`） | `composables/`、`lib/` | 讓公開 API 更清楚 | 小 | 低 |
| 1-6 | 宣告直接使用的 `@tiptap/core`：5 個檔案直接 import，目前只靠其他套件間接安裝 | `package.json` | 避免升級時意外壞掉 | 小 | 低（不是新套件，只是明確宣告） |

### 階段 2：抽出重複邏輯（效益最大）

| # | 項目 | 位置 | 效益 | 工作量 | 風險 |
| --- | --- | --- | --- | --- | --- |
| 2-1 | 自動儲存頁面的共用邏輯移進 `useAutosave`：狀態文字、離開頁面前提醒、`beforeunload` 在兩個編輯器各寫一次 | `PostEditorView.vue`、`WorkEditorView.vue` | 約 50 行，且兩邊行為不會分歧 | 中 | 中：E2E 涵蓋作品編輯器 |
| 2-2 | 分頁元件 `<Pager>`：相同的分頁按鈕寫了 6 次 | `PostsView`、`UsersView`、`GuestbookView`、`FilesView`、`PublicBlogView`、`PublicGuestbookView` | 約 100 行 | 中 | 低 |
| 2-3 | 時間格式集中到 `lib/format.ts`：`HH:MM` 寫了 7 次、`pad()` 有 11 份；行事曆的 `timeRange()` 與星期名稱各重複 2～3 次 | `lib/`、行事曆與編輯器頁面 | 約 40 行 | 小 | 低（有單元測試） |
| 2-4 | 後端排序：6 個 service 的 `ReorderAsync` 內容相同，改為 `IQueryable<T>` 擴充方法（與 `OwnedBy` 同類，不是 repository） | `Common/Reordering.cs` 與 6 個 service | 約 30 行 | 小 | 低（共用契約測試涵蓋排序） |
| 2-5 | `UpdatedAt` 統一：9 處直接用 `DateTime.UtcNow`、其他用注入的 `TimeProvider`。改為在 `SaveChangesAsync` 統一設定修改時間 | 各 service、`ApplicationDbContext` | 約 20 行，測試時鐘涵蓋所有時間 | 中 | 中 |
| 2-6 | 兩個 Tiptap 編輯器共用 `modelValue` 同步與 `setLink`；目前兩份 `setLink` 已經不一致（一份允許 `mailto:`） | `RichTextEditor.vue`、`blog/PostContentEditor.vue` | 約 30 行，並修正不一致 | 中 | 中 |
| 2-7 | 頁面標題只設定一次：路由守衛與 8 個頁面各設一次，格式不同（` - ` 與 ` \| `），NotFound 兩處文字也不同 | `router/index.ts`、8 個頁面 | 一致的標題 | 小 | 低 |
| 2-8 | 搜尋防抖與「篩選改變時回到第 1 頁」重複 3 次，可抽成 composable | `PostsView`、`UsersView`、`DirectoryView` 等 | 約 30 行 | 小 | 低 |

### 階段 3：需要先決定

| # | 項目 | 需要決定的事 |
| --- | --- | --- |
| 3-1 | `ContactMethod.Icon` 欄位只有示範資料寫入，沒有任何地方讀取 | 移除需要兩組 migration（schema 變更） |
| 3-2 | `ForbiddenException`、`ICurrentUser.IsAdmin` 沒有被使用 | 保留作為錯誤類型的完整詞彙，或移除 |
| 3-3 | 後端 `GET …/portfolios/facets` 與 `?category`、`?tag` 篩選，前端目前改在用戶端篩選、沒有呼叫 | 保留 API（作品多時改回伺服器端篩選）或移除 |
| 3-4 | `Services/`、`DTOs/` 兩個資料夾與「依功能分資料夾」的說法不一致（技術教學 3.1） | 移到 `Common/`／`Infrastructure/`，或調整文件說法 |
| 3-5 | `docs/TASKS.md` 有約 150 行描述舊架構的已完成項目（舊路由、`services/`、JSON fallback、Zeabur） | 刪除（`CHANGELOG.md` 與 git 歷史已保存）或移到封存檔 |
| 3-6 | 同樣的規則與指令重複出現在多份文件（三份 CLAUDE.md 的「給 AI 的指示」、測試原則；快速開始出現在 7 個檔案；測試數字出現在 3 個檔案） | 每類內容指定一份為主，其他改為連結 |
| 3-7 | `playwright.config.ts` 的 `CI=1` 分支：CI 已移除，但可用來測建置後的前端 | 保留並改註解，或移除分支 |
| 3-8 | `e2e/` 沒有被 `npm run build` 型別檢查 | 加入 `tsconfig.json` 的 references |
| 3-9 | 截圖共 3.5 MB | 轉成 WebP 可縮小約一半 |

### 已知問題

- **F-01**（測試報告）：訪客開頁面時 refresh 回 401 留下 console 錯誤，已記錄於 [`TASKS.md`](TASKS.md) 技術債。

## 不建議做的

| 建議 | 原因 |
| --- | --- |
| 回到泛型 Repository／CrudService 以消除 service 間的重複 | 違反 ADR-011；重複的部分以小型查詢擴充方法處理即可（如 2-4） |
| 合併 `PostFacetsDto` 與 `PortfolioFacetsDto` | 會改變 OpenAPI schema 名稱與前端型別，換來的程式碼很少 |
| 為了少寫型別參數，新增 `ApiResponse.Of<T>` | 只改善閱讀，64 處呼叫端都要改 |
