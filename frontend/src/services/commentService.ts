import httpService from './http'
import type { ApiResponse, GuestBookEntry } from '@/types/api'

class CommentService {
  // Public: get approved entries
  async getApprovedGuestBookEntries(): Promise<ApiResponse<GuestBookEntry[]>> {
    return httpService.get<GuestBookEntry[]>('/guestbookentries')
  }

  // Public: get approved entries for a specific user
  async getApprovedByUser(targetUserId: number): Promise<ApiResponse<GuestBookEntry[]>> {
    return httpService.get<GuestBookEntry[]>(`/guestbookentries/user/${targetUserId}`)
  }

  // Admin: get all entries
  async getAllGuestBookEntries(): Promise<ApiResponse<GuestBookEntry[]>> {
    return httpService.get<GuestBookEntry[]>('/guestbookentries/all')
  }

  async getGuestBookEntryById(id: number): Promise<ApiResponse<GuestBookEntry>> {
    return httpService.get<GuestBookEntry>(`/guestbookentries/${id}`)
  }

  async createGuestBookEntry(entry: { targetUserId?: number; name: string; email?: string; message: string }): Promise<ApiResponse<GuestBookEntry>> {
    return httpService.post<GuestBookEntry>('/guestbookentries', entry)
  }

  async updateGuestBookEntry(id: number, entry: { isApproved?: boolean; adminReply?: string }): Promise<ApiResponse<GuestBookEntry>> {
    return httpService.put<GuestBookEntry>(`/guestbookentries/${id}`, entry)
  }

  async deleteGuestBookEntry(id: number): Promise<ApiResponse<void>> {
    return httpService.delete<void>(`/guestbookentries/${id}`)
  }
}

export const commentService = new CommentService()
export default commentService
