import { describe, it, expect } from 'vitest'
import type { Schemas } from '@/api/types'
import { imageFromFile, newBlock, toEditable, toRequest, type EditableWork } from '../workDocument'

const uploaded = (id: number, url = `/files/${id}.jpg`): Schemas['PortfolioImage'] => ({
  url,
  width: 800,
  height: 600,
  caption: `說明 ${id}`,
  alt: `替代 ${id}`,
  fileId: id,
})

const dto = (overrides: Partial<Schemas['PortfolioDto']> = {}): Schemas['PortfolioDto'] => ({
  id: 1,
  title: '山茶行',
  slug: 'shancha',
  summary: '',
  category: '品牌識別',
  year: 2025,
  role: '設計',
  period: '',
  tags: ['品牌'],
  isFeatured: false,
  isPublic: true,
  sortOrder: 0,
  covers: [
    uploaded(1),
    { url: 'https://images.example.com/a.jpg', caption: '', alt: '', fileId: null },
  ],
  coverFocus: '50% 30%',
  fields: [{ label: '客戶', value: '山茶行' }],
  links: [],
  blocks: [
    { type: 'text', title: '委託', html: '<p>內容</p>' },
    { type: 'gallery', layout: 'masonry', items: [uploaded(2)] },
    {
      type: 'files',
      title: '文件',
      items: [
        {
          fileId: 9,
          url: '/files/9.pdf',
          fileName: 'guide.pdf',
          kind: 'Pdf',
          size: 10,
          description: '手冊',
        },
      ],
    },
  ],
  updatedAt: '2026-09-30T00:00:00Z',
  ...overrides,
})

const blocksOf = (work: EditableWork) => toRequest(work).blocks ?? []

describe('workDocument', () => {
  it('round-trips a saved document back to the same request', () => {
    const request = toRequest(toEditable(dto()))

    expect(request).toMatchObject({
      title: '山茶行',
      slug: 'shancha',
      coverFocus: '50% 30%',
      covers: [
        { fileId: 1, url: null, caption: '說明 1', alt: '替代 1' },
        { fileId: null, url: 'https://images.example.com/a.jpg', caption: '', alt: '' },
      ],
      blocks: [
        { type: 'text', title: '委託', html: '<p>內容</p>' },
        {
          type: 'gallery',
          layout: 'masonry',
          items: [{ fileId: 2, url: null, caption: '說明 2', alt: '替代 2' }],
        },
        { type: 'files', title: '文件', items: [{ fileId: 9, description: '手冊' }] },
      ],
    })
  })

  it('gives every block a stable key for the editor', () => {
    const work = toEditable(dto())

    expect(new Set(work.blocks.map((b) => b.key)).size).toBe(3)
  })

  it('leaves out an image block that has no image yet', () => {
    const work = toEditable(dto({ blocks: [] }))
    work.blocks.push(newBlock('image'))

    expect(blocksOf(work)).toEqual([])
  })

  it('leaves out an embed until its address is from a supported site', () => {
    const work = toEditable(dto({ blocks: [] }))
    const embed = newBlock('embed')
    work.blocks.push(embed)
    if (embed.type === 'embed') embed.url = 'https://evil.example/player'

    expect(blocksOf(work)).toEqual([])
  })

  it('skips empty metrics, fields and links while they are being filled in', () => {
    const work = toEditable(dto({ blocks: [], fields: [] }))
    const metrics = newBlock('metrics')
    work.blocks.push(metrics)
    work.fields.push({ label: '', value: '尚未命名' })
    work.links.push({ label: 'Behance', url: '' })

    const request = toRequest(work)

    expect([request.blocks, request.fields, request.links]).toEqual([[], [], []])
  })

  it('turns an uploaded file into an image the editor can show', () => {
    const file: Schemas['FileDto'] = {
      id: 5,
      fileName: 'poster.png',
      url: '/files/poster.png',
      kind: 'Image',
      mimeType: 'image/png',
      size: 100,
      width: 1200,
      height: 1600,
      createdAt: '2026-09-30T00:00:00Z',
    }

    expect(imageFromFile(file)).toEqual({
      fileId: 5,
      url: '/files/poster.png',
      width: 1200,
      height: 1600,
      caption: '',
      alt: '',
    })
  })
})
