import { readonly, ref } from 'vue'

export type ColorScheme = 'system' | 'light' | 'dark'

/** 與 index.html 的預載腳本使用同一個 key。 */
const storageKey = 'pm-color-scheme'
const scheme = ref<ColorScheme>(readStored())

function readStored(): ColorScheme {
  try {
    const value = localStorage.getItem(storageKey)
    return value === 'light' || value === 'dark' ? value : 'system'
  } catch {
    return 'system'
  }
}

function apply(value: ColorScheme) {
  const root = document.documentElement
  if (value === 'system') delete root.dataset.theme
  else root.dataset.theme = value
}

/** 深淺色偏好：預設跟隨系統，手動選擇記在瀏覽器（只影響這位訪客）。 */
export function useColorScheme() {
  function setScheme(value: ColorScheme) {
    scheme.value = value
    apply(value)
    try {
      if (value === 'system') localStorage.removeItem(storageKey)
      else localStorage.setItem(storageKey, value)
    } catch {
      // 無法存取 localStorage（例如無痕模式）時只在這次瀏覽生效
    }
  }

  return { scheme: readonly(scheme), setScheme }
}
