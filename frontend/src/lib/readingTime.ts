const cjk = /[぀-ヿ㐀-䶿一-鿿豈-﫿가-힯]/g
const latinWord = /[A-Za-z0-9]+(?:['’-][A-Za-z0-9]+)*/g

/** 字數：中日韓文字每字算一個，其他語言以單字計算。 */
export function countWords(text: string): number {
  const cjkCount = text.match(cjk)?.length ?? 0
  const otherWords = text.replace(cjk, ' ').match(latinWord)?.length ?? 0
  return cjkCount + otherWords
}

/** 預估閱讀分鐘數，與後端 RichTextSanitizer.EstimateReadingMinutes 相同：中文每分鐘 400 字、其他 200 字，最少 1 分鐘。 */
export function readingMinutes(text: string): number {
  const cjkCount = text.match(cjk)?.length ?? 0
  const otherWords = text.replace(cjk, ' ').match(latinWord)?.length ?? 0
  return Math.max(1, Math.ceil(cjkCount / 400 + otherWords / 200))
}
