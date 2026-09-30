import { http } from './http'
import type { PageQuery } from './public'
import type { Schemas } from './types'

export type GuestbookFilter = 'All' | 'Pending' | 'Approved'

export const guestbookApi = {
  list: (query: { status?: GuestbookFilter } & PageQuery = {}) =>
    http.get<Schemas['GuestbookEntryDtoPagedResult']>('/me/guestbook', query),
  setApproval: (id: number, isApproved: boolean) =>
    http.put(`/me/guestbook/${id}/approval`, { isApproved }),
  reply: (id: number, reply: string) => http.put(`/me/guestbook/${id}/reply`, { reply }),
  remove: (id: number) => http.delete(`/me/guestbook/${id}`),
}
