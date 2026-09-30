import { describe, it, expect } from 'vitest'
import { formatMinutes, timerToEntry } from '../timer'

const timer = (startedAt: string, workTaskId: number | null = 3) => ({
  workTaskId,
  title: '寫測試',
  startedAt,
})

describe('timerToEntry', () => {
  it('records start and end times within the same day', () => {
    const entry = timerToEntry(timer('2026-10-01T01:00:00Z'), new Date('2026-10-01T02:30:00Z'))

    expect(entry).toMatchObject({
      date: '2026-10-01',
      startTime: '09:00:00',
      endTime: '10:30:00',
      durationMinutes: null,
    })
  })

  it('records only the duration when the timer runs past midnight', () => {
    const entry = timerToEntry(timer('2026-10-01T15:30:00Z'), new Date('2026-10-01T16:45:00Z'))

    expect(entry).toMatchObject({ date: '2026-10-01', startTime: null, durationMinutes: 75 })
  })

  it('counts at least one minute', () => {
    const entry = timerToEntry(timer('2026-10-01T01:00:00Z'), new Date('2026-10-01T01:00:20Z'))

    expect(entry.durationMinutes).toBe(1)
  })

  it('keeps the description as the title when no task is linked', () => {
    const entry = timerToEntry(
      timer('2026-10-01T01:00:00Z', null),
      new Date('2026-10-01T02:00:00Z'),
    )

    expect([entry.workTaskId, entry.title]).toEqual([null, '寫測試'])
  })
})

describe('formatMinutes', () => {
  it.each([
    [45, '45 分'],
    [60, '1 小時'],
    [125, '2 小時 5 分'],
  ])('%d → %s', (minutes, text) => {
    expect(formatMinutes(minutes)).toBe(text)
  })
})
