import { http } from './http'
import type { PageQuery } from './public'
import type { Schemas } from './types'

export const postsApi = {
  list: (query: { q?: string; status?: Schemas['BlogPostStatus'] } & PageQuery = {}) =>
    http.get<Schemas['MyPostSummaryDtoPagedResult']>('/me/posts', query),
  get: (id: number) => http.get<Schemas['MyPostDto']>(`/me/posts/${id}`),
  create: (body: Schemas['SavePostRequest']) => http.post<Schemas['MyPostDto']>('/me/posts', body),
  update: (id: number, body: Schemas['SavePostRequest']) =>
    http.put<Schemas['MyPostDto']>(`/me/posts/${id}`, body),
  remove: (id: number) => http.delete(`/me/posts/${id}`),
}
