import httpService from './http'
import type { PortfolioAttachment, CreatePortfolioAttachmentDto } from '@/types/api'

export const portfolioAttachmentService = {
  getByPortfolio: (portfolioId: number) =>
    httpService.get<PortfolioAttachment[]>(`/portfolioattachments/portfolio/${portfolioId}`),

  create: (dto: CreatePortfolioAttachmentDto) =>
    httpService.post<PortfolioAttachment>('/portfolioattachments', dto),

  delete: (id: number) => httpService.delete<void>(`/portfolioattachments/${id}`)
}

export default portfolioAttachmentService
