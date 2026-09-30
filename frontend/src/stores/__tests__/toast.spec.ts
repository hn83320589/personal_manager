import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useToastStore } from '../toast'

describe('toast store', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.useFakeTimers()
  })
  afterEach(() => vi.useRealTimers())

  it('shows a message', () => {
    const toasts = useToastStore()

    toasts.success('已儲存')

    expect(toasts.items.map((t) => [t.kind, t.message])).toEqual([['success', '已儲存']])
  })

  it('hides success messages automatically', () => {
    const toasts = useToastStore()
    toasts.success('已儲存')

    vi.advanceTimersByTime(4000)

    expect(toasts.items).toHaveLength(0)
  })

  it('keeps errors until dismissed so they are not missed', () => {
    const toasts = useToastStore()
    toasts.error('儲存失敗')
    vi.advanceTimersByTime(60_000)

    toasts.dismiss(toasts.items[0]!.id)

    expect(toasts.items).toHaveLength(0)
  })
})
