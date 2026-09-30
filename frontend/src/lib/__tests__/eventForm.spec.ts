import { describe, it, expect } from 'vitest'
import { newEventForm, toEventForm, toEventRequest } from '../eventForm'

describe('eventForm', () => {
  it('sends local times as UTC', () => {
    const request = toEventRequest({ ...newEventForm('2026-10-01'), title: '會議' })

    expect([request.startTime, request.endTime]).toEqual([
      '2026-10-01T01:00:00.000Z',
      '2026-10-01T02:00:00.000Z',
    ])
  })

  it('stores an all-day event as whole local days', () => {
    const request = toEventRequest({
      ...newEventForm('2026-10-01'),
      isAllDay: true,
      endDate: '2026-10-02',
    })

    expect([request.startTime, request.endTime]).toEqual([
      '2026-09-30T16:00:00.000Z',
      '2026-10-02T16:00:00.000Z',
    ])
  })

  it('shows the last day of an all-day event when editing it again', () => {
    const form = toEventForm({
      id: 1,
      title: '展覽',
      description: '',
      startTime: '2026-09-30T16:00:00Z',
      endTime: '2026-10-02T16:00:00Z',
      isAllDay: true,
      isPublic: true,
      color: '',
      recurrence: 'None',
      recurrenceUntil: null,
    })

    expect([form.startDate, form.endDate]).toEqual(['2026-10-01', '2026-10-02'])
  })

  it('only sends an end date for repeating events', () => {
    const request = toEventRequest({ ...newEventForm('2026-10-01'), recurrenceUntil: '2026-12-31' })

    expect(request.recurrenceUntil).toBeNull()
  })
})
