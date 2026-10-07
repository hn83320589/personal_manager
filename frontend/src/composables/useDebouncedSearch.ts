import { onScopeDispose, ref, watch } from 'vue'

/**
 * 搜尋框：search 綁定輸入框，輸入停頓後才更新 query（去掉前後空白），避免每打一個字就送出請求。
 * onChange 與 query 在同一個時間點執行，適合同時把頁碼重設為 1。
 */
export function useDebouncedSearch(onChange?: () => void, delay = 300) {
  const search = ref('')
  const query = ref('')
  let timer: ReturnType<typeof setTimeout> | undefined

  watch(search, (value) => {
    clearTimeout(timer)
    timer = setTimeout(() => {
      query.value = value.trim()
      onChange?.()
    }, delay)
  })
  onScopeDispose(() => clearTimeout(timer))

  return { search, query }
}
