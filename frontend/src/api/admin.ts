import { http } from './http'
import type { PageQuery } from './public'
import type { Schemas } from './types'

/** 管理員專用（/api/admin，限 Admin 角色）。 */
export const adminUsersApi = {
  list: (query: { q?: string } & PageQuery = {}) =>
    http.get<Schemas['AdminUserDtoPagedResult']>('/admin/users', query),
  setStatus: (id: number, isActive: boolean) => http.put(`/admin/users/${id}/status`, { isActive }),
  setRole: (id: number, role: string) => http.put(`/admin/users/${id}/role`, { role }),
}
