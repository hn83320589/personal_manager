import type { Schemas } from '@/api/types'
import type { AccentColor } from '@/composables/useAccent'

/** 表單內的欄位一律有值（空字串代表未填），送出時即為 SaveProfileRequest。 */
export type ProfileForm = {
  [K in keyof Schemas['SaveProfileRequest']]-?: NonNullable<Schemas['SaveProfileRequest'][K]>
}

export const accentSwatches: { value: AccentColor; label: string; color: string }[] = [
  { value: 'blue', label: '藍', color: '#2754c5' },
  { value: 'green', label: '綠', color: '#1d7550' },
  { value: 'purple', label: '紫', color: '#6541c0' },
  { value: 'rose', label: '玫瑰', color: '#bd3259' },
  { value: 'slate', label: '石板灰', color: '#3a4658' },
]

export const portfolioModes: { value: Schemas['PortfolioMode']; label: string; hint: string }[] = [
  {
    value: 'Designer',
    label: '設計',
    hint: '以圖像為主，適合平面、品牌、插畫、UI／UX、攝影、動態、3D 等作品。',
  },
  { value: 'Frontend', label: '前端', hint: '畫面與說明並重，適合網站、App 與互動介面。' },
  { value: 'Backend', label: '後端', hint: '以文字、架構與重點數字為主，截圖為輔。' },
]

/** 切換作品集模式時建議的卡片版型；使用者仍可以另外選擇。 */
export const suggestedCardStyle: Record<Schemas['PortfolioMode'], Schemas['CardStyle']> = {
  Designer: 'Visual',
  Frontend: 'Info',
  Backend: 'Tech',
}

export const cardStyles: { value: Schemas['CardStyle']; label: string }[] = [
  { value: 'Visual', label: '圖像' },
  { value: 'Info', label: '資訊' },
  { value: 'Tech', label: '技術' },
]

export const cardRatios: { value: Schemas['CardRatio']; label: string }[] = [
  { value: 'Portrait', label: '直式 4:5' },
  { value: 'Square', label: '方形 1:1' },
  { value: 'Landscape', label: '橫式 4:3' },
]

export const skillDisplays: { value: Schemas['SkillDisplay']; label: string }[] = [
  { value: 'NameOnly', label: '只顯示名稱' },
  { value: 'Level', label: '熟練度' },
  { value: 'Years', label: '年資' },
]

export const statusSuggestions = ['接受委託中', '開放新的工作機會', '目前不接案']
