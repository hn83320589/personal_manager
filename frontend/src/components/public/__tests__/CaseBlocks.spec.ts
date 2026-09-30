import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import CaseBlocks from '../blocks/CaseBlocks.vue'
import type { Schemas } from '@/api/types'

type Block = Schemas['PublicPortfolioDto']['blocks'][number]

const image = (n: number, caption = '') => ({
  url: `/files/${n}.jpg`,
  width: 800,
  height: 600,
  caption,
  alt: `圖片 ${n}`,
  fileId: n,
})

const render = (blocks: Block[]) => mount(CaseBlocks, { props: { blocks } })

describe('CaseBlocks', () => {
  it('numbers images across image and gallery blocks', () => {
    const wrapper = render([
      { type: 'image', layout: 'wide', image: image(1, '主標誌') },
      { type: 'text', title: '', html: '<p>說明</p>' },
      { type: 'gallery', layout: 'cols-2', items: [image(2, '名片'), image(3, '信封')] },
    ])

    const numbers = wrapper.findAll('figcaption').map((c) => c.text())

    expect(numbers).toEqual(['圖 1主標誌', '圖 2名片', '圖 3信封'])
  })

  it('asks to open the lightbox at the clicked image', async () => {
    const wrapper = render([
      { type: 'image', layout: 'wide', image: image(1) },
      { type: 'gallery', layout: 'masonry', items: [image(2), image(3)] },
    ])

    await wrapper.findAll('button')[2]!.trigger('click')

    expect(wrapper.emitted('open-image')).toEqual([[2]])
  })

  it('sanitizes text blocks again before rendering', () => {
    const wrapper = render([{ type: 'text', title: '', html: '<p>安全</p><img src=x onerror="alert(1)">' }])

    expect(wrapper.html()).not.toContain('onerror')
  })

  it('embeds whitelisted players', () => {
    const wrapper = render([{ type: 'embed', url: 'https://vimeo.com/123456', caption: '動態版' }])

    expect(wrapper.find('iframe').attributes('src')).toBe('https://player.vimeo.com/video/123456')
  })

  it('renders nothing for embeds from other sites', () => {
    const wrapper = render([{ type: 'embed', url: 'https://evil.example/player', caption: '' }])

    expect(wrapper.find('iframe').exists()).toBe(false)
  })

  it('offers a preview for PDF attachments', () => {
    const wrapper = render([
      {
        type: 'files',
        title: '相關文件',
        items: [
          { fileId: 1, url: '/files/a.pdf', fileName: '品牌手冊.pdf', kind: 'Pdf', size: 2048, description: '' },
          { fileId: 2, url: '/files/b.docx', fileName: '提案.docx', kind: 'Word', size: 2048, description: '' },
        ],
      },
    ])

    expect(wrapper.findAll('a').map((a) => a.text())).toEqual(['線上預覽', '下載', '下載'])
  })
})
