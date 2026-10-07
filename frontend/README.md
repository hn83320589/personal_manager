# Personal Manager — 前端

Vue 3 + TypeScript SPA。專案說明與文件索引見[根目錄 README](../README.md)。

## 執行

需要 Node.js `^20.19.0 || >=22.12.0`，以及執行中的後端（`http://localhost:5037`）。

```bash
cd frontend
npm install
npm run dev     # → http://localhost:5173
```

`/api` 與 `/files` 由 Vite 轉送到後端，瀏覽器看到的是同一個網站，登入狀態（refresh token cookie）才能保存。
API 位址預設為 `/api`，不需要 `.env` 檔；要指向其他位址時可設定環境變數 `VITE_API_BASE_URL`。

## 指令

| 指令                              | 用途                                                  |
| --------------------------------- | ----------------------------------------------------- |
| `npm run dev`                     | 開發伺服器                                            |
| `npm run build`                   | 型別檢查 + 建置到 `dist/`                             |
| `npm run preview`                 | 預覽建置結果（同樣轉送 `/api`）                       |
| `npm run lint` / `npm run format` | ESLint / Prettier                                     |
| `npm run api:types`               | 由 `../backend/openapi.json` 產生 `src/api/schema.ts` |
| `npx vitest run`                  | 單元測試                                              |
| `npx playwright test`             | E2E：自動啟動後端與前端，使用暫存的 SQLite            |

## 延伸閱讀

- 架構、路由與寫法：[`CLAUDE.md`](CLAUDE.md)
- 本地開發與測試：[`docs/development-guide.md`](../docs/development-guide.md)
