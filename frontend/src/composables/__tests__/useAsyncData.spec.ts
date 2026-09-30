import { describe, it, expect } from 'vitest'
import { ref, nextTick } from 'vue'
import { ApiError } from '@/api/http'
import { useAsyncData } from '../useAsyncData'

const flush = () => new Promise((resolve) => setTimeout(resolve))

describe('useAsyncData', () => {
  it('loads data', async () => {
    const { data, loading } = useAsyncData(async () => 'hello')

    expect(loading.value).toBe(true)
    await flush()
    expect([data.value, loading.value]).toEqual(['hello', false])
  })

  it('exposes the error message', async () => {
    const { error, notFound } = useAsyncData(async () => {
      throw new ApiError('伺服器忙碌中', 503)
    })

    await flush()

    expect([error.value, notFound.value]).toEqual(['伺服器忙碌中', false])
  })

  it('reports not found separately so pages can show a 404', async () => {
    const { notFound } = useAsyncData(async () => {
      throw new ApiError('找不到這件作品', 404)
    })

    await flush()

    expect(notFound.value).toBe(true)
  })

  it('reloads when a watched source changes', async () => {
    const slug = ref('a')
    const { data } = useAsyncData(async () => `work-${slug.value}`, { watch: [slug] })
    await flush()

    slug.value = 'b'
    await nextTick()
    await flush()

    expect(data.value).toBe('work-b')
  })

  it('ignores a slower earlier response that arrives after a newer one', async () => {
    const slug = ref('slow')
    const { data } = useAsyncData(
      () =>
        new Promise<string>((resolve) =>
          setTimeout(() => resolve(slug.value), slug.value === 'slow' ? 30 : 0),
        ),
      { watch: [slug] },
    )

    slug.value = 'fast'
    await new Promise((resolve) => setTimeout(resolve, 50))

    expect(data.value).toBe('fast')
  })
})
