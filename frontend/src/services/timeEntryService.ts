import httpService from './http'
import type { TimeEntry, CreateTimeEntryDto, UpdateTimeEntryDto } from '@/types/api'

const timeEntryService = {
  getAll: () => httpService.get<TimeEntry[]>('/timeentries'),
  getById: (id: number) => httpService.get<TimeEntry>(`/timeentries/${id}`),
  create: (dto: CreateTimeEntryDto) => httpService.post<TimeEntry>('/timeentries', dto),
  update: (id: number, dto: UpdateTimeEntryDto) => httpService.put<TimeEntry>(`/timeentries/${id}`, dto),
  delete: (id: number) => httpService.delete<void>(`/timeentries/${id}`),
}

export default timeEntryService
