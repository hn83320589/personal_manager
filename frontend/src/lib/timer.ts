import type { Schemas } from '@/api/types'
import { localDate } from './format'

export interface RunningTimer {
  workTaskId: number | null
  title: string
  /** ISO 時間 */
  startedAt: string
}

const pad = (n: number) => String(n).padStart(2, '0')
const clock = (date: Date) => `${pad(date.getHours())}:${pad(date.getMinutes())}`

/**
 * 停止計時後轉成時間紀錄。同一天內記錄起訖時間；跨過午夜或不滿一分鐘時只記錄時長
 * （後端要求結束時間晚於開始時間、時長介於 1 到 1440 分鐘）。
 */
export function timerToEntry(timer: RunningTimer, now: Date): Schemas['SaveTimeEntryRequest'] {
  const start = new Date(timer.startedAt)
  const minutes = Math.max(1, Math.min(1440, Math.round((now.getTime() - start.getTime()) / 60000)))
  const sameDay = localDate(start) === localDate(now)
  const base = {
    workTaskId: timer.workTaskId,
    title: timer.workTaskId ? null : timer.title,
    date: localDate(start),
    description: '',
  }
  if (sameDay && clock(now) > clock(start)) {
    return {
      ...base,
      startTime: `${clock(start)}:00`,
      endTime: `${clock(now)}:00`,
      durationMinutes: null,
    }
  }
  return { ...base, startTime: null, endTime: null, durationMinutes: minutes }
}

/** 顯示用：分鐘數 → 「2 小時 5 分」。 */
export function formatMinutes(minutes: number): string {
  const hours = Math.floor(minutes / 60)
  const rest = minutes % 60
  if (!hours) return `${rest} 分`
  return rest ? `${hours} 小時 ${rest} 分` : `${hours} 小時`
}
