import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { nextTick, ref } from 'vue'
import { portfoliosApi } from '@/api/portfolios'
import { ApiError } from '@/api/http'
import type { Schemas } from '@/api/types'
import { useWorkEditor } from '../useWorkEditor'

vi.mock('@/api/portfolios', () => ({ portfoliosApi: { get: vi.fn(), update: vi.fn() } }))

const dto = (overrides: Partial<Schemas['PortfolioDto']> = {}): Schemas['PortfolioDto'] => ({
  id: 1,
  title: '作品',
  slug: 'work',
  summary: '',
  category: '',
  year: null,
  role: '',
  period: '',
  tags: [],
  isFeatured: false,
  isPublic: true,
  sortOrder: 0,
  covers: [],
  coverFocus: '50% 50%',
  fields: [],
  links: [],
  blocks: [],
  updatedAt: '2026-09-30T00:00:00Z',
  ...overrides,
})

async function loaded() {
  const editor = useWorkEditor(ref(1), { delay: 1000 })
  await vi.advanceTimersByTimeAsync(0)
  return editor
}

type Editor = Awaited<ReturnType<typeof loaded>>

async function edit(editor: Editor, title: string) {
  editor.work.value!.title = title
  await nextTick()
}

const updateCalls = () => vi.mocked(portfoliosApi.update).mock.calls

describe('useWorkEditor', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.useFakeTimers()
    vi.mocked(portfoliosApi.get).mockResolvedValue(dto())
    vi.mocked(portfoliosApi.update).mockImplementation(async (_id, body) =>
      dto({ title: body.title }),
    )
  })
  afterEach(() => {
    vi.useRealTimers()
    vi.clearAllMocks()
  })

  it('does not save just because the work was loaded', async () => {
    await loaded()

    await vi.advanceTimersByTimeAsync(5000)

    expect(portfoliosApi.update).not.toHaveBeenCalled()
  })

  it('saves automatically once editing pauses', async () => {
    const editor = await loaded()

    await edit(editor, '新標題')
    await vi.advanceTimersByTimeAsync(1000)

    expect([updateCalls()[0]?.[1].title, editor.status.value]).toEqual(['新標題', 'saved'])
  })

  it('waits for typing to stop before saving', async () => {
    const editor = await loaded()

    await edit(editor, 'a')
    await vi.advanceTimersByTimeAsync(600)
    await edit(editor, 'ab')
    await vi.advanceTimersByTimeAsync(1000)

    expect(portfoliosApi.update).toHaveBeenCalledTimes(1)
  })

  it('saves again after the current save when more changes arrive meanwhile', async () => {
    let finish!: () => void
    vi.mocked(portfoliosApi.update).mockImplementationOnce(
      (_id, body) => new Promise((resolve) => (finish = () => resolve(dto({ title: body.title })))),
    )
    const editor = await loaded()
    await edit(editor, '第一次')
    await vi.advanceTimersByTimeAsync(1000)

    await edit(editor, '第二次')
    await vi.advanceTimersByTimeAsync(1000)
    const callsWhileSaving = updateCalls().length
    finish()
    await vi.advanceTimersByTimeAsync(0)

    expect([callsWhileSaving, updateCalls()[1]?.[1].title]).toEqual([1, '第二次'])
  })

  it('keeps the changes and reports the problem when saving fails', async () => {
    vi.mocked(portfoliosApi.update).mockRejectedValue(
      new ApiError('資料格式有誤', 400, ['不支援嵌入這個網站的內容']),
    )
    const editor = await loaded()

    await edit(editor, '新標題')
    await vi.advanceTimersByTimeAsync(1000)

    expect([editor.status.value, editor.error.value, editor.dirty.value]).toEqual([
      'error',
      '資料格式有誤：不支援嵌入這個網站的內容',
      true,
    ])
  })

  it('saves right away when asked, for example before leaving the page', async () => {
    const editor = await loaded()
    await edit(editor, '新標題')

    await editor.flush()

    expect(portfoliosApi.update).toHaveBeenCalledTimes(1)
  })

  it('picks up the address chosen by the server without saving again', async () => {
    vi.mocked(portfoliosApi.update).mockResolvedValue(dto({ slug: 'work-2' }))
    const editor = await loaded()

    await edit(editor, '新標題')
    await vi.advanceTimersByTimeAsync(1000)
    await vi.advanceTimersByTimeAsync(5000)

    expect([editor.work.value!.slug, updateCalls().length]).toEqual(['work-2', 1])
  })

  describe('statusText', () => {
    it('says changes are saved automatically before anything happens', async () => {
      const editor = await loaded()
      expect(editor.statusText.value).toBe('修改會自動儲存')
    })

    it('mentions unsaved changes while waiting to save', async () => {
      const editor = await loaded()
      await edit(editor, '新標題')
      expect(editor.statusText.value).toBe('有尚未儲存的修改')
    })

    it('shows when it was last saved', async () => {
      vi.setSystemTime(new Date(2026, 9, 7, 9, 5))
      const editor = await loaded()
      await edit(editor, '新標題')
      await vi.advanceTimersByTimeAsync(1000)
      expect(editor.statusText.value).toBe('已自動儲存 09:05')
    })

    it('shows the reason when saving fails', async () => {
      vi.mocked(portfoliosApi.update).mockRejectedValue(new ApiError('標題太長', 400))
      const editor = await loaded()
      await edit(editor, '新標題')
      await vi.advanceTimersByTimeAsync(1000)
      expect(editor.statusText.value).toBe('儲存失敗：標題太長')
    })
  })
})
