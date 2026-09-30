import { defineStore } from 'pinia'
import { ref } from 'vue'

export interface Toast {
  id: number
  kind: 'success' | 'error'
  message: string
}

/** 後台操作結果的提示。成功訊息自動消失；錯誤訊息保留到使用者關閉，避免沒看到。 */
export const useToastStore = defineStore('toast', () => {
  const items = ref<Toast[]>([])
  let nextId = 1

  function dismiss(id: number) {
    items.value = items.value.filter((t) => t.id !== id)
  }

  function push(kind: Toast['kind'], message: string) {
    const id = nextId++
    items.value = [...items.value, { id, kind, message }]
    if (kind === 'success') setTimeout(() => dismiss(id), 3500)
  }

  return {
    items,
    dismiss,
    success: (message: string) => push('success', message),
    error: (message: string) => push('error', message),
  }
})
