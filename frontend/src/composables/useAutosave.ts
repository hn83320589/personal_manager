import { computed, ref, watch, type Ref, type WatchSource } from 'vue'
import { errorMessage } from './useAsyncAction'
import { useAsyncData } from './useAsyncData'

export type SaveStatus = 'idle' | 'saving' | 'saved' | 'error'

interface AutosaveOptions<Dto, Editable, Request> {
  load: () => Promise<Dto>
  save: (request: Request) => Promise<Dto>
  toEditable: (dto: Dto) => Editable
  toRequest: (editable: Editable) => Request
  /** 儲存成功後，把伺服器調整過的值（例如網址代稱）同步回編輯內容。 */
  applySaved?: (saved: Dto, editable: Editable) => void
  /** 這些值改變時重新載入（例如路由參數）。 */
  watch?: WatchSource[]
  delay?: number
}

/**
 * 可自動儲存的文件（作品、文章）：載入後，編輯停頓一段時間就儲存整份內容。
 * 同一時間只送出一個請求；儲存期間有新的修改，完成後以最新內容再存一次。
 */
export function useAutosave<Dto, Editable extends object, Request>(
  options: AutosaveOptions<Dto, Editable, Request>,
) {
  const delay = options.delay ?? 1200
  const {
    data,
    loading,
    error: loadError,
    notFound,
    reload,
  } = useAsyncData(options.load, { watch: options.watch })

  const doc = ref(null) as Ref<Editable | null>
  const status = ref<SaveStatus>('idle')
  const error = ref<string | null>(null)
  const savedAt = ref<Date | null>(null)
  /** 最後一次成功儲存（或載入）時送出的內容，用來判斷是否有未儲存的修改。 */
  const savedSnapshot = ref('')
  let timer: ReturnType<typeof setTimeout> | undefined
  let inFlight: Promise<void> | null = null

  const snapshot = () => JSON.stringify(doc.value ? options.toRequest(doc.value) : null)
  const dirty = computed(() => !!doc.value && snapshot() !== savedSnapshot.value)

  watch(data, (loaded) => {
    if (!loaded) return
    doc.value = options.toEditable(loaded)
    savedSnapshot.value = snapshot()
    status.value = 'idle'
  })

  watch(
    doc,
    () => {
      if (!dirty.value) return
      clearTimeout(timer)
      timer = setTimeout(() => void save(), delay)
    },
    { deep: true },
  )

  async function send(): Promise<void> {
    const request = options.toRequest(doc.value!)
    const sent = JSON.stringify(request)
    status.value = 'saving'
    try {
      const saved = await options.save(request)
      savedSnapshot.value = sent
      if (doc.value && options.applySaved) {
        // 同步伺服器調整的值；若這段期間使用者沒有其他修改，同步不算新的修改
        const unchangedSince = snapshot() === sent
        options.applySaved(saved, doc.value)
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
    if (!doc.value || !dirty.value) return
    inFlight = send().finally(() => (inFlight = null))
    await inFlight
    // 儲存期間又有修改：以最新內容再存一次（失敗時等使用者下一次修改或手動重試）
    if (dirty.value && status.value === 'saved') return save()
  }

  /** 立即儲存尚未存的修改（離開頁面前、按下發佈時）。 */
  async function flush() {
    clearTimeout(timer)
    await save()
  }

  return { doc, loading, loadError, notFound, reload, status, error, savedAt, dirty, flush }
}
