import { computed, ref, watch, type Ref } from 'vue'
import { portfoliosApi } from '@/api/portfolios'
import { toEditable, toRequest, type EditableWork } from '@/lib/workDocument'
import { errorMessage } from './useAsyncAction'
import { useAsyncData } from './useAsyncData'

export type SaveStatus = 'idle' | 'saving' | 'saved' | 'error'

/**
 * 作品編輯器：載入作品，編輯停頓後自動儲存整份作品。
 * 同一時間只送出一個儲存請求；儲存期間有新的修改，完成後會再存一次。
 */
export function useWorkEditor(id: Ref<number>, options: { delay?: number } = {}) {
  const delay = options.delay ?? 1200
  const {
    data,
    loading,
    error: loadError,
    notFound,
    reload,
  } = useAsyncData(() => portfoliosApi.get(id.value), { watch: [id] })

  const work = ref<EditableWork | null>(null)
  const status = ref<SaveStatus>('idle')
  const error = ref<string | null>(null)
  const savedAt = ref<Date | null>(null)
  /** 最後一次成功儲存（或載入）時送出的內容，用來判斷是否有未儲存的修改。 */
  const savedSnapshot = ref('')
  let timer: ReturnType<typeof setTimeout> | undefined
  let inFlight: Promise<void> | null = null

  const snapshot = () => JSON.stringify(work.value ? toRequest(work.value) : null)
  const dirty = computed(() => !!work.value && snapshot() !== savedSnapshot.value)

  watch(data, (loaded) => {
    if (!loaded) return
    work.value = toEditable(loaded)
    savedSnapshot.value = snapshot()
    status.value = 'idle'
  })

  watch(
    work,
    () => {
      if (!dirty.value) return
      clearTimeout(timer)
      timer = setTimeout(() => void save(), delay)
    },
    { deep: true },
  )

  async function send(): Promise<void> {
    const body = toRequest(work.value!)
    const sent = JSON.stringify(body)
    status.value = 'saving'
    try {
      const saved = await portfoliosApi.update(id.value, body)
      savedSnapshot.value = sent
      // 伺服器可能調整網址代稱（例如重複時加上 -2），同步回編輯內容但不再觸發儲存
      if (work.value && work.value.slug !== saved.slug) {
        const unchangedSince = snapshot() === sent
        work.value.slug = saved.slug
        if (unchangedSince) savedSnapshot.value = snapshot()
      }
      status.value = 'saved'
      error.value = null
      savedAt.value = new Date()
    } catch (e) {
      status.value = 'error'
      error.value = errorMessage(e)
    }
  }

  async function save(): Promise<void> {
    if (inFlight) {
      await inFlight
      return save()
    }
    if (!work.value || !dirty.value) return
    inFlight = send().finally(() => (inFlight = null))
    await inFlight
    // 儲存期間又有修改：以最新內容再存一次（失敗時等使用者下一次修改或手動重試）
    if (dirty.value && status.value === 'saved') return save()
  }

  /** 立即儲存尚未存的修改（離開頁面前、按下儲存時）。 */
  async function flush() {
    clearTimeout(timer)
    await save()
  }

  return { work, loading, loadError, notFound, reload, status, error, savedAt, dirty, flush }
}
