import { onBeforeUnmount, onMounted, type Ref } from 'vue'
import { onBeforeRouteLeave } from 'vue-router'

/**
 * 離開頁面前先存；存不起來就提醒，避免遺失修改。
 * 站內換頁時等待 flush 完成再判斷；關閉分頁或重新整理時由瀏覽器詢問。
 */
export function useUnsavedChangesGuard(dirty: Ref<boolean>, flush: () => Promise<void>) {
  onBeforeRouteLeave(async () => {
    await flush()
    return !dirty.value || window.confirm('有修改尚未儲存成功，確定要離開嗎？')
  })

  function warnBeforeUnload(event: BeforeUnloadEvent) {
    if (dirty.value) event.preventDefault()
  }
  onMounted(() => window.addEventListener('beforeunload', warnBeforeUnload))
  onBeforeUnmount(() => window.removeEventListener('beforeunload', warnBeforeUnload))
}
