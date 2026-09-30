import { describe, it, expect, vi, beforeEach } from 'vitest'
import { flushPromises, mount, RouterLinkStub } from '@vue/test-utils'
import { setActivePinia, createPinia } from 'pinia'
import { filesApi } from '@/api/files'
import FileCard from '../FileCard.vue'

vi.mock('@/api/files', () => ({ filesApi: { usages: vi.fn() } }))

const file = {
  id: 7,
  fileName: 'poster.png',
  url: '/files/poster.png',
  kind: 'Image' as const,
  mimeType: 'image/png',
  size: 2048,
  width: 600,
  height: 800,
  createdAt: '2026-09-30T00:00:00Z',
}

function render() {
  return mount(FileCard, { props: { file }, global: { stubs: { RouterLink: RouterLinkStub } } })
}

async function clickDelete(wrapper: ReturnType<typeof render>) {
  await wrapper.get('button[aria-label="刪除「poster.png」"]').trigger('click')
  await flushPromises()
}

describe('FileCard', () => {
  beforeEach(() => setActivePinia(createPinia()))

  it('lists the content that still uses the file before deleting it', async () => {
    vi.mocked(filesApi.usages).mockResolvedValue([{ kind: 'Portfolio', id: 3, title: '山茶行' }])
    const wrapper = render()

    await clickDelete(wrapper)

    expect([
      wrapper.text().includes('仍在使用中'),
      wrapper.text().includes('作品「山茶行」'),
    ]).toEqual([true, true])
  })

  it('only deletes after the second confirmation', async () => {
    vi.mocked(filesApi.usages).mockResolvedValue([])
    const wrapper = render()

    await clickDelete(wrapper)
    const beforeConfirm = wrapper.emitted('remove')
    await wrapper
      .findAll('button')
      .find((b) => b.text() === '刪除')!
      .trigger('click')

    expect([beforeConfirm, wrapper.emitted('remove')]).toEqual([undefined, [[]]])
  })
})
