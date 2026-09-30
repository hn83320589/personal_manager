import { http } from './http'
import type { Schemas } from './types'

/** 個人工具：待辦、行事曆、工作追蹤（只有自己看得到，/api/me）。 */

export const todosApi = {
  list: (status?: Schemas['TodoStatus']) => http.get<Schemas['TodoDto'][]>('/me/todos', { status }),
  create: (body: Schemas['SaveTodoRequest']) => http.post<Schemas['TodoDto']>('/me/todos', body),
  update: (id: number, body: Schemas['SaveTodoRequest']) =>
    http.put<Schemas['TodoDto']>(`/me/todos/${id}`, body),
  remove: (id: number) => http.delete(`/me/todos/${id}`),
}

export const calendarApi = {
  occurrences: (range: { from: string; to: string }) =>
    http.get<Schemas['OccurrenceDto'][]>('/me/calendar', range),
  get: (id: number) => http.get<Schemas['EventDto']>(`/me/calendar/events/${id}`),
  create: (body: Schemas['SaveEventRequest']) =>
    http.post<Schemas['EventDto']>('/me/calendar/events', body),
  update: (id: number, body: Schemas['SaveEventRequest']) =>
    http.put<Schemas['EventDto']>(`/me/calendar/events/${id}`, body),
  remove: (id: number) => http.delete(`/me/calendar/events/${id}`),
}

export const projectsApi = {
  list: () => http.get<Schemas['ProjectDto'][]>('/me/projects'),
  create: (body: Schemas['SaveProjectRequest']) =>
    http.post<Schemas['ProjectDto']>('/me/projects', body),
  update: (id: number, body: Schemas['SaveProjectRequest']) =>
    http.put<Schemas['ProjectDto']>(`/me/projects/${id}`, body),
  remove: (id: number) => http.delete(`/me/projects/${id}`),
  reorder: (ids: number[]) => http.put('/me/projects/order', { ids }),
}

export const workTasksApi = {
  list: (filter: { projectId?: number; status?: Schemas['WorkTaskStatus'] } = {}) =>
    http.get<Schemas['WorkTaskDto'][]>('/me/work-tasks', filter),
  create: (body: Schemas['SaveWorkTaskRequest']) =>
    http.post<Schemas['WorkTaskDto']>('/me/work-tasks', body),
  update: (id: number, body: Schemas['SaveWorkTaskRequest']) =>
    http.put<Schemas['WorkTaskDto']>(`/me/work-tasks/${id}`, body),
  remove: (id: number) => http.delete(`/me/work-tasks/${id}`),
}

export const timeEntriesApi = {
  list: (filter: { from?: string; to?: string; workTaskId?: number } = {}) =>
    http.get<Schemas['TimeEntryDto'][]>('/me/time-entries', filter),
  summary: (range: { from: string; to: string }) =>
    http.get<Schemas['TimeSummaryDto']>('/me/time-entries/summary', range),
  create: (body: Schemas['SaveTimeEntryRequest']) =>
    http.post<Schemas['TimeEntryDto']>('/me/time-entries', body),
  update: (id: number, body: Schemas['SaveTimeEntryRequest']) =>
    http.put<Schemas['TimeEntryDto']>(`/me/time-entries/${id}`, body),
  remove: (id: number) => http.delete(`/me/time-entries/${id}`),
}
