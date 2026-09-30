import { describe, it, expect, beforeEach } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { ApiError } from '@/api/http'
import { useToastStore } from '@/stores/toast'
import { useAsyncAction } from '../useAsyncAction'

describe('useAsyncAction', () => {
  beforeEach(() => setActivePinia(createPinia()))

  it('returns the result and confirms success', async () => {
    const { run } = useAsyncAction()

    const result = await run(async () => 42, { success: '已儲存' })

    expect([result, useToastStore().items[0]?.message]).toEqual([42, '已儲存'])
  })

  it('reports the server message and validation errors on failure', async () => {
    const { run, error } = useAsyncAction()

    const result = await run(async () => {
      throw new ApiError('資料格式有誤', 400, ['請輸入標題'])
    })

    expect([result, error.value, useToastStore().items[0]?.message]).toEqual([
      undefined,
      '資料格式有誤：請輸入標題',
      '資料格式有誤：請輸入標題',
    ])
  })

  it('is busy while the action runs', async () => {
    const { run, running } = useAsyncAction()
    let finish!: () => void

    const pending = run(() => new Promise<void>((resolve) => (finish = resolve)))
    const whileRunning = running.value
    finish()
    await pending

    expect([whileRunning, running.value]).toEqual([true, false])
  })

  it('ignores a second submit while the first is still running', async () => {
    const { run } = useAsyncAction()
    let calls = 0
    let finish!: () => void
    const action = () => {
      calls++
      return new Promise<void>((resolve) => (finish = resolve))
    }

    const first = run(action)
    await run(action)
    finish()
    await first

    expect(calls).toBe(1)
  })
})
