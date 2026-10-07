import { describe, it, expect } from 'vitest'
import { formatDate, formatFileSize, formatPeriod, formatTime, localDate } from '../format'

describe('formatPeriod', () => {
  it('shows year and month for a finished job', () => {
    expect(formatPeriod({ start: '2021-03-01', end: '2024-06-30' })).toBe('2021.03 — 2024.06')
  })

  it('shows "now" for a current job', () => {
    expect(formatPeriod({ start: '2024-07-01', end: null, isCurrent: true })).toBe('2024.07 — 現在')
  })

  it('accepts plain years for education', () => {
    expect(formatPeriod({ start: 2014, end: 2018 })).toBe('2014 — 2018')
  })

  it('returns an empty string when nothing is known', () => {
    expect(formatPeriod({ start: null, end: null })).toBe('')
  })
})

describe('formatDate', () => {
  it('formats an ISO timestamp in local time', () => {
    expect(formatDate('2026-09-30T04:00:00Z')).toBe('2026.09.30')
  })
})

describe('formatFileSize', () => {
  it.each([
    [512, '512 B'],
    [2048, '2 KB'],
    [3.5 * 1024 * 1024, '3.5 MB'],
  ])('%d bytes → %s', (bytes, text) => {
    expect(formatFileSize(bytes)).toBe(text)
  })
})

describe('localDate', () => {
  it('uses the local calendar date, not the UTC one', () => {
    // 台北時間 2026-10-01 07:00 在 UTC 仍是 9 月 30 日
    expect(localDate(new Date('2026-09-30T23:00:00Z'))).toBe('2026-10-01')
  })
})

describe('formatTime', () => {
  it('shows local hours and minutes with leading zeros', () => {
    expect(formatTime(new Date(2026, 9, 7, 9, 5))).toBe('09:05')
  })
})
