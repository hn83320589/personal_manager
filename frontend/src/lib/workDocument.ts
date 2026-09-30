import type { Schemas } from '@/api/types'
import { toEmbed } from './embeds'

/**
 * 作品編輯器使用的資料形狀，以及與 API 之間的轉換。
 * 編輯中的圖片保留網址供預覽；送出時已上傳的圖片只送 fileId，網址與尺寸由伺服器填入。
 */
export interface EditableImage {
  fileId: number | null
  url: string
  width?: number | null
  height?: number | null
  caption: string
  alt: string
}

export type BlockType = Schemas['PortfolioDto']['blocks'][number]['type']

type BlockInput = NonNullable<Schemas['SavePortfolioRequest']['blocks']>[number]

type WithKey<T> = T & { key: string }

export type EditableBlock =
  | WithKey<{ type: 'text'; title: string; html: string }>
  | WithKey<{ type: 'image'; layout: string; image: EditableImage | null }>
  | WithKey<{ type: 'gallery'; layout: string; items: EditableImage[] }>
  | WithKey<{ type: 'files'; title: string; items: Schemas['PortfolioFile'][] }>
  | WithKey<{ type: 'embed'; url: string; caption: string }>
  | WithKey<{ type: 'metrics'; title: string; items: Schemas['PortfolioMetric'][] }>
  | WithKey<{ type: 'code'; language: string; code: string; caption: string }>

export interface EditableWork {
  title: string
  slug: string
  summary: string
  category: string
  year: number | null
  role: string
  period: string
  tags: string[]
  isFeatured: boolean
  isPublic: boolean
  coverFocus: string
  covers: EditableImage[]
  fields: Schemas['PortfolioField'][]
  links: Schemas['PortfolioLink'][]
  blocks: EditableBlock[]
}

let keySeed = 0
const nextKey = () => `b${++keySeed}`

const editableImage = (image: Schemas['PortfolioImage']): EditableImage => ({
  fileId: image.fileId ?? null,
  url: image.url,
  width: image.width,
  height: image.height,
  caption: image.caption,
  alt: image.alt,
})

export const imageFromFile = (file: Schemas['FileDto']): EditableImage => ({
  fileId: file.id,
  url: file.url,
  width: file.width,
  height: file.height,
  caption: '',
  alt: '',
})

function editableBlock(block: Schemas['PortfolioDto']['blocks'][number]): EditableBlock {
  const key = nextKey()
  switch (block.type) {
    case 'text':
      return { key, type: 'text', title: block.title, html: block.html }
    case 'image':
      return { key, type: 'image', layout: block.layout, image: editableImage(block.image) }
    case 'gallery':
      return { key, type: 'gallery', layout: block.layout, items: block.items.map(editableImage) }
    case 'files':
      return { key, type: 'files', title: block.title, items: block.items.map((f) => ({ ...f })) }
    case 'embed':
      return { key, type: 'embed', url: block.url, caption: block.caption }
    case 'metrics':
      return { key, type: 'metrics', title: block.title, items: block.items.map((m) => ({ ...m })) }
    case 'code':
      return {
        key,
        type: 'code',
        language: block.language,
        code: block.code,
        caption: block.caption,
      }
  }
}

export function toEditable(work: Schemas['PortfolioDto']): EditableWork {
  return {
    title: work.title,
    slug: work.slug,
    summary: work.summary,
    category: work.category,
    year: work.year ?? null,
    role: work.role,
    period: work.period,
    tags: [...work.tags],
    isFeatured: work.isFeatured,
    isPublic: work.isPublic,
    coverFocus: work.coverFocus,
    covers: work.covers.map(editableImage),
    fields: work.fields.map((f) => ({ ...f })),
    links: work.links.map((l) => ({ ...l })),
    blocks: work.blocks.map(editableBlock),
  }
}

/** 新區塊的預設內容（與 prototype 相同）。 */
export function newBlock(type: BlockType): EditableBlock {
  const key = nextKey()
  switch (type) {
    case 'text':
      return { key, type, title: '', html: '' }
    case 'image':
      return { key, type, layout: 'wide', image: null }
    case 'gallery':
      return { key, type, layout: 'masonry', items: [] }
    case 'files':
      return { key, type, title: '相關文件', items: [] }
    case 'embed':
      return { key, type, url: '', caption: '' }
    case 'metrics':
      return { key, type, title: '成果', items: [{ value: '', label: '' }] }
    case 'code':
      return { key, type, language: '', code: '', caption: '' }
  }
}

const imageInput = (image: EditableImage): Schemas['ImageInput'] =>
  image.fileId
    ? { fileId: image.fileId, url: null, caption: image.caption, alt: image.alt }
    : { fileId: null, url: image.url, caption: image.caption, alt: image.alt }

/**
 * 轉成送出的內容。還沒填完的部分（沒有圖片的圖片區塊、不支援的嵌入網址、
 * 空白的數字、欄位與連結）先不送出，自動儲存時才不會因為編輯到一半而失敗。
 */
function blockInput(block: EditableBlock): BlockInput | null {
  switch (block.type) {
    case 'text':
      return { type: 'text', title: block.title, html: block.html }
    case 'image':
      return block.image
        ? { type: 'image', layout: block.layout, image: imageInput(block.image) }
        : null
    case 'gallery':
      return { type: 'gallery', layout: block.layout, items: block.items.map(imageInput) }
    case 'files':
      return {
        type: 'files',
        title: block.title,
        items: block.items.map((f) => ({ fileId: f.fileId, description: f.description })),
      }
    case 'embed':
      return toEmbed(block.url) ? { type: 'embed', url: block.url, caption: block.caption } : null
    case 'metrics': {
      const items = block.items.filter((m) => m.value.trim())
      return items.length ? { type: 'metrics', title: block.title, items } : null
    }
    case 'code':
      return { type: 'code', language: block.language, code: block.code, caption: block.caption }
  }
}

export function toRequest(work: EditableWork): Schemas['SavePortfolioRequest'] {
  return {
    title: work.title.trim() || '未命名作品',
    slug: work.slug || null,
    summary: work.summary,
    category: work.category,
    year: work.year,
    role: work.role,
    period: work.period,
    tags: work.tags,
    isFeatured: work.isFeatured,
    isPublic: work.isPublic,
    coverFocus: work.coverFocus,
    covers: work.covers.map(imageInput),
    fields: work.fields.filter((f) => f.label.trim()),
    links: work.links.filter((l) => l.url.trim()),
    blocks: work.blocks.map(blockInput).filter((b) => b !== null),
  }
}

/** 區塊是否還缺東西、目前不會被儲存（在編輯器上提示使用者）。 */
export function isIncomplete(block: EditableBlock): boolean {
  return blockInput(block) === null
}
