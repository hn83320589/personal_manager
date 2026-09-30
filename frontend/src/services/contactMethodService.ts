import httpService from './http'
import type { ApiResponse, ContactMethod, ContactType } from '@/types/api'

export interface CreateContactMethodDto {
  userId: number
  type: ContactType
  label: string
  value: string
  icon: string
  isPublic: boolean
  sortOrder: number
}

export interface UpdateContactMethodDto {
  type?: ContactType
  label?: string
  value?: string
  icon?: string
  isPublic?: boolean
  sortOrder?: number
}

class ContactMethodService {
  async getByUserId(userId: number): Promise<ApiResponse<ContactMethod[]>> {
    return httpService.get<ContactMethod[]>(`/contactmethods/user/${userId}`)
  }

  async getPublicByUserId(userId: number): Promise<ApiResponse<ContactMethod[]>> {
    return httpService.get<ContactMethod[]>(`/contactmethods/user/${userId}/public`)
  }

  async create(dto: CreateContactMethodDto): Promise<ApiResponse<ContactMethod>> {
    return httpService.post<ContactMethod>('/contactmethods', dto)
  }

  async update(id: number, dto: UpdateContactMethodDto): Promise<ApiResponse<ContactMethod>> {
    return httpService.put<ContactMethod>(`/contactmethods/${id}`, dto)
  }

  async delete(id: number): Promise<ApiResponse<void>> {
    return httpService.delete<void>(`/contactmethods/${id}`)
  }
}

export const contactMethodService = new ContactMethodService()
export default contactMethodService
