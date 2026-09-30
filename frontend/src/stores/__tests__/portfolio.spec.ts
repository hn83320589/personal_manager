import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { usePortfolioStore } from '../portfolio'
import type { Portfolio } from '@/types/api'

// Mock portfolioService
vi.mock('@/services/portfolioService', () => ({
  default: {
    getPortfolios: vi.fn(),
    getPortfolioById: vi.fn(),
    createPortfolio: vi.fn(),
    updatePortfolio: vi.fn(),
    deletePortfolio: vi.fn(),
  }
}))

const mockPortfolio: Portfolio = {
  id: 1,
  userId: 1,
  title: 'Test Portfolio',
  description: 'Test Description',
  imageUrl: '/test.jpg',
  projectUrl: 'https://test.com',
  repositoryUrl: 'https://github.com/test',
  technologies: 'Vue.js, TypeScript',
  isPublic: true,
  isFeatured: false,
  sortOrder: 1,
  createdAt: '2024-01-01T00:00:00Z',
}

describe('PortfolioStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  describe('初始狀態', () => {
    it('應該有正確的初始狀態', () => {
      const store = usePortfolioStore()

      expect(store.portfolios).toEqual([])
      expect(store.currentPortfolio).toBeNull()
      expect(store.isLoading).toBe(false)
      expect(store.error).toBeNull()
    })
  })

  describe('fetchPortfolios', () => {
    it('應該成功獲取作品集列表', async () => {
      const store = usePortfolioStore()
      const mockResponse = {
        success: true,
        data: [mockPortfolio],
        message: 'Success',
        errors: []
      }

      const portfolioService = await import('@/services/portfolioService')
      vi.mocked(portfolioService.default.getPortfolios).mockResolvedValue(mockResponse)

      await store.fetchPortfolios()

      expect(store.portfolios).toEqual([mockPortfolio])
      expect(store.isLoading).toBe(false)
      expect(store.error).toBeNull()
    })

    it('應該處理獲取失敗的情況', async () => {
      const store = usePortfolioStore()
      const mockResponse = {
        success: false,
        data: [],
        message: 'Failed to fetch',
        errors: ['Network error']
      }

      const portfolioService = await import('@/services/portfolioService')
      vi.mocked(portfolioService.default.getPortfolios).mockResolvedValue(mockResponse)

      await store.fetchPortfolios()

      expect(store.portfolios).toEqual([])
      expect(store.error).toBe('Failed to fetch')
      expect(store.isLoading).toBe(false)
    })
  })

  describe('fetchPortfolioById', () => {
    it('應該成功獲取單一作品集', async () => {
      const store = usePortfolioStore()
      const mockResponse = {
        success: true,
        data: mockPortfolio,
        message: 'Success',
        errors: []
      }

      const portfolioService = await import('@/services/portfolioService')
      vi.mocked(portfolioService.default.getPortfolioById).mockResolvedValue(mockResponse)

      await store.fetchPortfolioById(1)

      expect(store.currentPortfolio).toEqual(mockPortfolio)
      expect(store.isLoading).toBe(false)
      expect(store.error).toBeNull()
    })
  })

  describe('clearCurrentPortfolio', () => {
    it('應該清除當前作品集', () => {
      const store = usePortfolioStore()

      store.currentPortfolio = mockPortfolio
      store.clearCurrentPortfolio()

      expect(store.currentPortfolio).toBeNull()
    })
  })

  describe('clearError', () => {
    it('應該清除錯誤訊息', () => {
      const store = usePortfolioStore()

      store.error = 'Some error'
      store.clearError()

      expect(store.error).toBeNull()
    })
  })
})
