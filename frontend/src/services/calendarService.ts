import httpService from './http'
import type { ApiResponse, CalendarEvent } from '@/types/api'

class CalendarService {
  async getCalendarEvents(): Promise<ApiResponse<CalendarEvent[]>> {
    return httpService.get<CalendarEvent[]>('/calendarevents')
  }

  async getCalendarEventById(id: number): Promise<ApiResponse<CalendarEvent>> {
    return httpService.get<CalendarEvent>(`/calendarevents/${id}`)
  }

  async getCalendarEventsByUserId(userId: number): Promise<ApiResponse<CalendarEvent[]>> {
    return httpService.get<CalendarEvent[]>(`/calendarevents/user/${userId}`)
  }

  async getPublicCalendarEventsByUserId(userId: number): Promise<ApiResponse<CalendarEvent[]>> {
    return httpService.get<CalendarEvent[]>(`/calendarevents/user/${userId}/public`)
  }

  async getCalendarEventsByDateRange(userId: number, start: string, end: string): Promise<ApiResponse<CalendarEvent[]>> {
    return httpService.get<CalendarEvent[]>(`/calendarevents/user/${userId}/range`, { start, end })
  }

  async createCalendarEvent(event: Partial<CalendarEvent>): Promise<ApiResponse<CalendarEvent>> {
    return httpService.post<CalendarEvent>('/calendarevents', event)
  }

  async updateCalendarEvent(id: number, event: Partial<CalendarEvent>): Promise<ApiResponse<CalendarEvent>> {
    return httpService.put<CalendarEvent>(`/calendarevents/${id}`, event)
  }

  async deleteCalendarEvent(id: number): Promise<ApiResponse<void>> {
    return httpService.delete<void>(`/calendarevents/${id}`)
  }
}

export const calendarService = new CalendarService()
export default calendarService
