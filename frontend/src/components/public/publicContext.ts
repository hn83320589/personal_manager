import { inject, type InjectionKey, type Ref } from 'vue'
import type { Schemas } from '@/api/types'

/** 個人頁面的共用資料：由 PublicLayout 載入一次，子頁面直接取用。 */
export interface PublicContext {
  username: Ref<string>
  profile: Ref<Schemas['ProfileDto'] | null>
}

export const publicContextKey: InjectionKey<PublicContext> = Symbol('public-context')

export function usePublicContext(): PublicContext {
  const context = inject(publicContextKey)
  if (!context) throw new Error('usePublicContext 必須在 PublicLayout 之內使用')
  return context
}
