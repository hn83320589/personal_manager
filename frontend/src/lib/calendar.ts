import { formatTime, localDate } from './format'

export const WEEKDAYS = ['日', '一', '二', '三', '四', '五', '六'] as const

/** 月曆的格子：從該月第一天所在那週的週日開始，固定 6 週 42 天，月份切換時高度不跳動。 */
export function monthGrid(year: number, month: number): Date[] {
  const first = new Date(year, month, 1)
  const start = new Date(year, month, 1 - first.getDay())
  return Array.from(
    { length: 42 },
    (_, i) => new Date(start.getFullYear(), start.getMonth(), start.getDate() + i),
  )
}

/** 依當地日期（YYYY-MM-DD）分組；跨日的行程會出現在經過的每一天。 */
export function groupByDay<T extends { start: string; end: string }>(items: T[]): Map<string, T[]> {
  const days = new Map<string, T[]>()
  for (const item of items) {
    const start = new Date(item.start)
    const end = new Date(item.end)
    const cursor = new Date(start.getFullYear(), start.getMonth(), start.getDate())
    // 結束時間剛好在午夜時，不算進下一天
    const last = new Date(end.getTime() - 1)
    do {
      const key = localDate(cursor)
      days.set(key, [...(days.get(key) ?? []), item])
      cursor.setDate(cursor.getDate() + 1)
    } while (cursor <= last)
  }
  return days
}

/** 行程的時間範圍，例如「09:00–10:30」；全天行程顯示「全天」。 */
export function timeRange(
  event: { isAllDay: boolean; start: string; end: string },
  separator = '–',
): string {
  if (event.isAllDay) return '全天'
  return `${formatTime(new Date(event.start))}${separator}${formatTime(new Date(event.end))}`
}
