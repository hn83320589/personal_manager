import httpService from './http'
import type { ApiResponse, Portfolio } from '@/types/api'

class PortfolioService {
  async getPortfolios(): Promise<ApiResponse<Portfolio[]>> {
    return httpService.get<Portfolio[]>('/portfolios')
  }

  async getPortfolioById(id: number): Promise<ApiResponse<Portfolio>> {
    return httpService.get<Portfolio>(`/portfolios/${id}`)
  }

  async getPortfoliosByUserId(userId: number): Promise<ApiResponse<Portfolio[]>> {
    return httpService.get<Portfolio[]>(`/portfolios/user/${userId}`)
  }

  async getPublicPortfoliosByUserId(userId: number): Promise<ApiResponse<Portfolio[]>> {
    return httpService.get<Portfolio[]>(`/portfolios/user/${userId}/public`)
  }

  async getFeaturedPortfoliosByUserId(userId: number): Promise<ApiResponse<Portfolio[]>> {
    return httpService.get<Portfolio[]>(`/portfolios/user/${userId}/featured`)
  }

  async createPortfolio(portfolio: Partial<Portfolio>): Promise<ApiResponse<Portfolio>> {
    return httpService.post<Portfolio>('/portfolios', portfolio)
  }

  async updatePortfolio(id: number, portfolio: Partial<Portfolio>): Promise<ApiResponse<Portfolio>> {
    return httpService.put<Portfolio>(`/portfolios/${id}`, portfolio)
  }

  async deletePortfolio(id: number): Promise<ApiResponse<void>> {
    return httpService.delete<void>(`/portfolios/${id}`)
  }
}

export const portfolioService = new PortfolioService()
export default portfolioService
