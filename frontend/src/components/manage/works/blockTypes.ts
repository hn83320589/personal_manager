import type { BlockType } from '@/lib/workDocument'

export const typeLabel: Record<BlockType, string> = {
  text: '文字',
  image: '單張圖片',
  gallery: '圖庫',
  files: '附件',
  embed: '嵌入（影片等）',
  metrics: '重點數字',
  code: '程式碼',
}

export const blockTypes = Object.keys(typeLabel) as BlockType[]
