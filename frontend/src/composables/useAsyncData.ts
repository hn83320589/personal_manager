import { ref, shallowRef, watch, type Ref, type WatchSource } from 'vue'
import { ApiError } from '@/api/http'

interface Options {
  /** 這些值改變時重新載入（例如路由參數）。 */
  watch?: WatchSource[]
}

/**
 * 頁面讀取資料用：提供 loading、錯誤訊息，並把 404 分開（頁面顯示「找不到」而不是錯誤）。
 * 快速切換頁面時，只採用最後一次請求的結果。
 */
export function useAsyncData<T>(load: () => Promise<T>, options: Options = {}) {
  const data = shallowRef<T | null>(null) as Ref<T | null>
  const loading = ref(true)
  const error = ref<string | null>(null)
  const notFound = ref(false)
  let latest = 0

  async function reload() {
    const request = ++latest
    loading.value = true
    error.value = null
    notFound.value = false
    try {
      const result = await load()
      if (request === latest) data.value = result
    } catch (e) {
      if (request !== latest) return
      data.value = null
      if (e instanceof ApiError && e.status === 404) notFound.value = true
      else error.value = e instanceof Error ? e.message : '載入失敗，請稍後再試'
    } finally {
      if (request === latest) loading.value = false
    }
  }

  if (options.watch?.length) watch(options.watch, reload)
  void reload()

  return { data, loading, error, notFound, reload }
}
