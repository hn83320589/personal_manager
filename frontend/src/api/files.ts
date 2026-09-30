import { http } from './http'
import type { Schemas } from './types'
import type { PageQuery } from './public'

export const filesApi = {
  list: (query: { kind?: Schemas['FileKind'] } & PageQuery = {}) =>
    http.get<Schemas['FileDtoPagedResult']>('/me/files', query),
  upload: (file: File, onProgress?: (ratio: number) => void) => {
    const form = new FormData()
    form.append('file', file)
    return http.upload<Schemas['FileDto']>('/me/files', form, onProgress)
  },
  usages: (id: number) => http.get<Schemas['FileUsageDto'][]>(`/me/files/${id}/usages`),
  remove: (id: number) => http.delete(`/me/files/${id}`),
}
