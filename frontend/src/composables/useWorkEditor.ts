import type { Ref } from 'vue'
import { portfoliosApi } from '@/api/portfolios'
import { toEditable, toRequest } from '@/lib/workDocument'
import { useAutosave } from './useAutosave'

/** 作品編輯器：載入作品並自動儲存整份作品（見 useAutosave）。 */
export function useWorkEditor(id: Ref<number>, options: { delay?: number } = {}) {
  const { doc, ...rest } = useAutosave({
    load: () => portfoliosApi.get(id.value),
    save: (request) => portfoliosApi.update(id.value, request),
    toEditable,
    toRequest,
    // 伺服器可能調整網址代稱（例如重複時加上 -2）
    applySaved: (saved, work) => {
      work.slug = saved.slug
    },
    watch: [id],
    delay: options.delay,
  })
  return { work: doc, ...rest }
}
