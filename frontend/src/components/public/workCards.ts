import type { Schemas } from '@/api/types'

export type CardStyle = Schemas['CardStyle']
export type CardRatio = Schemas['CardRatio']

/** 圖像型卡片的圖片比例（CSS aspect-ratio）。 */
export const ratioValue: Record<CardRatio, string> = {
  Portrait: '4 / 5',
  Square: '1 / 1',
  Landscape: '4 / 3',
}

/** 各卡片版型的欄數；資訊型的精選作品會橫跨整列。 */
export const gridClass: Record<CardStyle, string> = {
  Visual: 'grid grid-cols-1 gap-x-5 gap-y-8 min-[481px]:grid-cols-2 min-[901px]:grid-cols-3',
  Info: 'grid grid-cols-1 gap-x-7 gap-y-10 min-[721px]:grid-cols-2',
  Tech: 'grid grid-cols-1 border-t border-rule',
}
