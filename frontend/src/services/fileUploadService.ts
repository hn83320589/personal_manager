import httpService from './http'
import type { FileUpload } from '@/types/api'

export const fileUploadService = {
  getAll: () => httpService.get<FileUpload[]>('/fileuploads'),

  upload: (formData: FormData) => httpService.uploadFile<FileUpload>('/fileuploads', formData),

  delete: (id: number) => httpService.delete<void>(`/fileuploads/${id}`)
}

export default fileUploadService
