import { ref, watch, type Ref } from 'vue'
import type { OwnedCollectionApi } from '@/api/collections'
import { useAsyncAction } from './useAsyncAction'
import { useAsyncData } from './useAsyncData'

interface Messages {
  created?: string
  updated?: string
  removed?: string
}

/**
 * 後台清單頁共用的狀態：載入、新增／更新（同一個 save）、刪除、上下移動排序。
 * 寫入成功後直接更新畫面上的清單，不重新載入。
 */
export function useOwnedList<Item extends { id: number }, Save>(
  api: OwnedCollectionApi<Item, Save>,
  messages: Messages = {},
) {
  const items = ref([]) as Ref<Item[]>
  const { data, loading, error, reload } = useAsyncData(api.list)
  const { run, running: saving } = useAsyncAction()

  // 載入（或重新載入）完成時以伺服器資料為準；之後的寫入直接更新本地清單
  watch(data, (loaded) => (items.value = loaded ?? []))

  /** id 為 null 表示新增。回傳儲存後的資料；失敗時回傳 undefined，清單不變。 */
  async function save(id: number | null, body: Save): Promise<Item | undefined> {
    const saved = await run(() => (id === null ? api.create(body) : api.update(id, body)), {
      success: id === null ? messages.created : messages.updated,
    })
    if (!saved) return undefined
    items.value =
      id === null ? [...items.value, saved] : items.value.map((i) => (i.id === id ? saved : i))
    return saved
  }

  async function remove(id: number) {
    const ok = await run(async () => (await api.remove(id), true), { success: messages.removed })
    if (ok) items.value = items.value.filter((i) => i.id !== id)
  }

  /** 樂觀更新：先移動畫面上的項目，排序儲存失敗再還原。 */
  async function move(id: number, delta: -1 | 1) {
    const from = items.value.findIndex((i) => i.id === id)
    const to = from + delta
    if (from < 0 || to < 0 || to >= items.value.length) return
    const previous = items.value
    const next = [...previous]
    next.splice(to, 0, next.splice(from, 1)[0]!)
    items.value = next
    const ok = await run(async () => (await api.reorder(next.map((i) => i.id)), true))
    if (!ok) items.value = previous
  }

  return { items, loading, error, saving, reload, save, remove, move }
}
