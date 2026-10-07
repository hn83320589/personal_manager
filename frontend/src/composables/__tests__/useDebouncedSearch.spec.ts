import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { nextTick } from 'vue'
import { useDebouncedSearch } from '../useDebouncedSearch'

describe('useDebouncedSearch', () => {
  beforeEach(() => vi.useFakeTimers())
  afterEach(() => vi.useRealTimers())

  it('updates the query only after typing pauses', async () => {
    const { search, query } = useDebouncedSearch()
    search.value = 'vue'
    await nextTick()
    vi.advanceTimersByTime(299)
    const before = query.value
    vi.advanceTimersByTime(1)
    expect([before, query.value]).toEqual(['', 'vue'])
  })

  it('trims spaces and reports each change once', async () => {
    const onChange = vi.fn()
    const { search, query } = useDebouncedSearch(onChange)
    search.value = ' v'
    await nextTick()
    search.value = ' vue '
    await nextTick()
    vi.advanceTimersByTime(300)
    expect([query.value, onChange.mock.calls.length]).toEqual(['vue', 1])
  })
})
