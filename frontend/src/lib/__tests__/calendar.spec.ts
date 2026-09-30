import { describe, it, expect } from 'vitest'
import { localDate } from '../format'
import { groupByDay, monthGrid } from '../calendar'

describe('monthGrid', () => {
  it('starts on the Sunday before the first day and always shows six weeks', () => {
    const days = monthGrid(2026, 9) // 2026 年 10 月 1 日是週四

    expect([days.length, localDate(days[0]!), localDate(days[41]!)]).toEqual([
      42,
      '2026-09-27',
      '2026-11-07',
    ])
  })
})

describe('groupByDay', () => {
  it('puts an event on its local day', () => {
    const days = groupByDay([{ start: '2026-09-30T23:30:00Z', end: '2026-10-01T00:30:00Z' }])

    expect([...days.keys()]).toEqual(['2026-10-01'])
  })

  it('shows a multi-day event on every day it spans', () => {
    const days = groupByDay([{ start: '2026-10-01T01:00:00Z', end: '2026-10-03T10:00:00Z' }])

    expect([...days.keys()]).toEqual(['2026-10-01', '2026-10-02', '2026-10-03'])
  })

  it('does not spill into the next day when an event ends at midnight', () => {
    const days = groupByDay([{ start: '2026-10-01T01:00:00Z', end: '2026-10-01T16:00:00Z' }])

    expect([...days.keys()]).toEqual(['2026-10-01'])
  })
})
