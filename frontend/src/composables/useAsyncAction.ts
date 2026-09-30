import { ref } from 'vue'
import { ApiError } from '@/api/http'
import { useToastStore } from '@/stores/toast'

/** 錯誤訊息：後端訊息加上各欄位的驗證錯誤。 */
export function errorMessage(e: unknown): string {
  if (e instanceof ApiError)
    return e.errors.length ? `${e.message}：${e.errors.join('、')}` : e.message
  return e instanceof Error ? e.message : '操作失敗，請稍後再試'
}

/**
 * 後台的寫入操作（儲存、刪除…）：處理中狀態、成功提示與錯誤訊息。
 * 執行中再次觸發會被忽略，避免重複送出。失敗時回傳 undefined。
 */
export function useAsyncAction() {
  const running = ref(false)
  const error = ref<string | null>(null)
  const toasts = useToastStore()

  async function run<T>(
    action: () => Promise<T>,
    options: { success?: string } = {},
  ): Promise<T | undefined> {
    if (running.value) return undefined
    running.value = true
    error.value = null
    try {
      const result = await action()
      if (options.success) toasts.success(options.success)
      return result
    } catch (e) {
      error.value = errorMessage(e)
      toasts.error(error.value)
      return undefined
    } finally {
      running.value = false
    }
  }

  return { run, running, error }
}
