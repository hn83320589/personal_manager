import { http } from './http'
import type { Schemas } from './types'

export const profileApi = {
  get: () => http.get<Schemas['ProfileDto']>('/me/profile'),
  save: (body: Schemas['SaveProfileRequest']) =>
    http.put<Schemas['ProfileDto']>('/me/profile', body),
}
