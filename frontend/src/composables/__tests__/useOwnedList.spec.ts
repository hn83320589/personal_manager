import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import type { OwnedCollectionApi } from '@/api/collections'
import { ApiError } from '@/api/http'
import { useOwnedList } from '../useOwnedList'

interface Item {
  id: number
  name: string
}

function fakeApi(initial: Item[]) {
  let items = [...initial]
  let nextId = 100
  const api: OwnedCollectionApi<Item, { name: string }> = {
    list: vi.fn(async () => [...items]),
    create: vi.fn(async (body) => {
      const item = { id: nextId++, ...body }
      items.push(item)
      return item
    }),
    update: vi.fn(async (id, body) => ({ id, ...body })),
    remove: vi.fn(async (id) => {
      items = items.filter((i) => i.id !== id)
    }),
    reorder: vi.fn(async () => undefined),
  }
  return api
}

const flush = () => new Promise((resolve) => setTimeout(resolve))
const names = (list: { items: { value: Item[] } }) => list.items.value.map((i) => i.name)

describe('useOwnedList', () => {
  beforeEach(() => setActivePinia(createPinia()))

  it('loads the items', async () => {
    const list = useOwnedList(fakeApi([{ id: 1, name: 'Vue' }]))

    await flush()

    expect(names(list)).toEqual(['Vue'])
  })

  it('adds a new item at the end', async () => {
    const list = useOwnedList(fakeApi([{ id: 1, name: 'Vue' }]))
    await flush()

    await list.save(null, { name: 'Go' })

    expect(names(list)).toEqual(['Vue', 'Go'])
  })

  it('replaces an item after updating it', async () => {
    const list = useOwnedList(fakeApi([{ id: 1, name: 'Vue' }]))
    await flush()

    await list.save(1, { name: 'Vue 3' })

    expect(names(list)).toEqual(['Vue 3'])
  })

  it('removes an item', async () => {
    const list = useOwnedList(
      fakeApi([
        { id: 1, name: 'Vue' },
        { id: 2, name: 'Go' },
      ]),
    )
    await flush()

    await list.remove(1)

    expect(names(list)).toEqual(['Go'])
  })

  it('moves an item and saves the new order', async () => {
    const api = fakeApi([
      { id: 1, name: 'A' },
      { id: 2, name: 'B' },
      { id: 3, name: 'C' },
    ])
    const list = useOwnedList(api)
    await flush()

    await list.move(3, -1)

    expect([names(list), vi.mocked(api.reorder).mock.calls[0]]).toEqual([
      ['A', 'C', 'B'],
      [[1, 3, 2]],
    ])
  })

  it('puts the order back when saving the order fails', async () => {
    const api = fakeApi([
      { id: 1, name: 'A' },
      { id: 2, name: 'B' },
    ])
    vi.mocked(api.reorder).mockRejectedValue(new ApiError('伺服器錯誤', 500))
    const list = useOwnedList(api)
    await flush()

    await list.move(2, -1)

    expect(names(list)).toEqual(['A', 'B'])
  })

  it('keeps the list unchanged when saving fails', async () => {
    const api = fakeApi([{ id: 1, name: 'Vue' }])
    vi.mocked(api.create).mockRejectedValue(new ApiError('資料格式有誤', 400))
    const list = useOwnedList(api)
    await flush()

    const saved = await list.save(null, { name: '' })

    expect([saved, names(list)]).toEqual([undefined, ['Vue']])
  })
})
