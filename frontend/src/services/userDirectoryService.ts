import httpService from './http'
import type { ApiResponse, PublicUser, ProfileDirectory } from '@/types/api'

class UserDirectoryService {
  getDirectory(): Promise<ApiResponse<ProfileDirectory[]>> {
    return httpService.get<ProfileDirectory[]>('/profiles/directory')
  }

  getPublicUser(username: string): Promise<ApiResponse<PublicUser>> {
    return httpService.get<PublicUser>(`/users/public/${username}`)
  }

  getAllPublicUsers(): Promise<ApiResponse<PublicUser[]>> {
    return httpService.get<PublicUser[]>('/users/public')
  }
}

export const userDirectoryService = new UserDirectoryService()
export default userDirectoryService
