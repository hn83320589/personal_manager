const pad = (n: number) => String(n).padStart(2, '0')

/** 'YYYY-MM-DD' 或年份 → '2024.07'／'2014'。 */
function periodPart(value: string | number | null | undefined): string {
  if (value === null || value === undefined || value === '') return ''
  if (typeof value === 'number') return String(value)
  const [year, month] = value.split('-')
  return month ? `${year}.${month}` : (year ?? '')
}

/** 經歷、學歷的期間，例如「2021.03 — 2024.06」「2024.07 — 現在」。 */
export function formatPeriod(period: {
  start: string | number | null | undefined
  end: string | number | null | undefined
  isCurrent?: boolean
}): string {
  const start = periodPart(period.start)
  const end = period.isCurrent ? '現在' : periodPart(period.end)
  if (!start && !end) return ''
  return [start, end].filter(Boolean).join(' — ')
}

/** 以訪客的當地時間顯示日期，例如「2026.09.30」。 */
export function formatDate(iso: string): string {
  const date = new Date(iso)
  return `${date.getFullYear()}.${pad(date.getMonth() + 1)}.${pad(date.getDate())}`
}

export function formatFileSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${Math.round(bytes / 1024)} KB`
  return `${Math.round((bytes / 1024 / 1024) * 10) / 10} MB`
}
