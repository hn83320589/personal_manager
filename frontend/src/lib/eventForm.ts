import type { Schemas } from '@/api/types'
import { formatTime, localDate } from './format'

/** 行程表單：日期與時間分開編輯（當地時間），送出時轉成 UTC。 */
export interface EventForm {
  title: string
  description: string
  isAllDay: boolean
  startDate: string
  startTime: string
  endDate: string
  endTime: string
  isPublic: boolean
  color: string
  recurrence: Schemas['Recurrence']
  recurrenceUntil: string
}

/** 在指定日期新增一小時的行程（預設上午九點）。 */
export function newEventForm(date: string): EventForm {
  return {
    title: '',
    description: '',
    isAllDay: false,
    startDate: date,
    startTime: '09:00',
    endDate: date,
    endTime: '10:00',
    isPublic: false,
    color: '',
    recurrence: 'None',
    recurrenceUntil: '',
  }
}

export function toEventForm(event: Schemas['EventDto']): EventForm {
  const start = new Date(event.startTime)
  const end = new Date(event.endTime)
  // 全天行程以「結束日隔天 00:00」儲存，表單顯示最後一天
  const lastDay = event.isAllDay ? new Date(end.getTime() - 1) : end
  return {
    title: event.title,
    description: event.description,
    isAllDay: event.isAllDay,
    startDate: localDate(start),
    startTime: formatTime(start),
    endDate: localDate(lastDay),
    endTime: formatTime(end),
    isPublic: event.isPublic,
    color: event.color,
    recurrence: event.recurrence,
    recurrenceUntil: event.recurrenceUntil ?? '',
  }
}

export function toEventRequest(form: EventForm): Schemas['SaveEventRequest'] {
  const local = (date: string, time: string) => new Date(`${date}T${time}`)
  const start = form.isAllDay
    ? local(form.startDate, '00:00')
    : local(form.startDate, form.startTime)
  const end = form.isAllDay
    ? new Date(local(form.endDate, '00:00').getTime() + 24 * 60 * 60 * 1000)
    : local(form.endDate, form.endTime)
  return {
    title: form.title,
    description: form.description,
    startTime: start.toISOString(),
    endTime: end.toISOString(),
    isAllDay: form.isAllDay,
    isPublic: form.isPublic,
    color: form.color || null,
    recurrence: form.recurrence,
    recurrenceUntil:
      form.recurrence !== 'None' && form.recurrenceUntil ? form.recurrenceUntil : null,
  }
}
