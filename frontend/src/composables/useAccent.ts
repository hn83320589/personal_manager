import { onUnmounted, watchEffect, type Ref } from 'vue'

/** 可選的主題色；blue 為預設，不需要設定屬性（見 src/assets/tokens.css）。 */
const accentColors = ['blue', 'green', 'purple', 'rose', 'slate'] as const
export type AccentColor = (typeof accentColors)[number]

function isAccent(value: string | null | undefined): value is AccentColor {
  return (accentColors as readonly string[]).includes(value ?? '')
}

/** 在個人頁面套用擁有者設定的主題色，離開頁面時恢復預設。 */
export function useAccent(color: Ref<string | null | undefined>) {
  const root = document.documentElement

  watchEffect(() => {
    const value = color.value
    if (isAccent(value) && value !== 'blue') root.dataset.accent = value
    else delete root.dataset.accent
  })

  onUnmounted(() => {
    delete root.dataset.accent
  })
}
