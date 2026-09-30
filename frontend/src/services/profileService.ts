import httpService from './http'
import type { ApiResponse, PersonalProfile } from '@/types/api'

class ProfileService {
  async getProfiles(): Promise<ApiResponse<PersonalProfile[]>> {
    return httpService.get<PersonalProfile[]>('/profiles')
  }

  async getProfileById(id: number): Promise<ApiResponse<PersonalProfile>> {
    return httpService.get<PersonalProfile>(`/profiles/${id}`)
  }

  async getProfileByUserId(userId: number): Promise<ApiResponse<PersonalProfile>> {
    return httpService.get<PersonalProfile>(`/profiles/user/${userId}`)
  }

  async createProfile(profile: Partial<PersonalProfile>): Promise<ApiResponse<PersonalProfile>> {
    return httpService.post<PersonalProfile>('/profiles', profile)
  }

  async updateProfile(id: number, profile: Partial<PersonalProfile>): Promise<ApiResponse<PersonalProfile>> {
    return httpService.put<PersonalProfile>(`/profiles/${id}`, profile)
  }

  async deleteProfile(id: number): Promise<ApiResponse<void>> {
    return httpService.delete<void>(`/profiles/${id}`)
  }
}

export const profileService = new ProfileService()
export default profileService
