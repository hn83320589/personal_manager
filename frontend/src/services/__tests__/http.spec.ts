import { describe, it, expect, beforeEach, vi } from 'vitest'
import { httpService } from '../http'
import axios from 'axios'

// Mock axios
vi.mock('axios', () => ({
  default: {
    create: vi.fn()
  }
}))
const mockedAxios = vi.mocked(axios)

describe('HttpService', () => {
  beforeEach(() => {
    vi.resetAllMocks()
    
    // Mock axios.create
    ;(mockedAxios.create as any).mockReturnValue({
      interceptors: {
        request: { use: vi.fn() },
        response: { use: vi.fn() }
      },
      get: vi.fn(),
      post: vi.fn(),
      put: vi.fn(),
      delete: vi.fn()
    } as any)
  })

  describe('HTTP Methods', () => {
    it('應該能夠執行 GET 請求', async () => {
      const mockResponse = {
        data: { success: true, data: 'test data' },
        status: 200
      }
      
      const mockGet = vi.fn().mockResolvedValue(mockResponse)
      ;(httpService as any).client = { get: mockGet }
      
      const result = await httpService.get('/test')
      
      expect(mockGet).toHaveBeenCalledWith('/test')
      expect(result).toEqual(mockResponse)
    })

    it('應該能夠執行 POST 請求', async () => {
      const mockResponse = {
        data: { success: true, data: 'created' },
        status: 201
      }
      const postData = { name: 'test' }
      
      const mockPost = vi.fn().mockResolvedValue(mockResponse)
      ;(httpService as any).client = { post: mockPost }
      
      const result = await httpService.post('/test', postData)
      
      expect(mockPost).toHaveBeenCalledWith('/test', postData)
      expect(result).toEqual(mockResponse)
    })

    it('應該能夠執行 PUT 請求', async () => {
      const mockResponse = {
        data: { success: true, data: 'updated' },
        status: 200
      }
      const putData = { id: 1, name: 'updated' }
      
      const mockPut = vi.fn().mockResolvedValue(mockResponse)
      ;(httpService as any).client = { put: mockPut }
      
      const result = await httpService.put('/test/1', putData)
      
      expect(mockPut).toHaveBeenCalledWith('/test/1', putData)
      expect(result).toEqual(mockResponse)
    })

    it('應該能夠執行 DELETE 請求', async () => {
      const mockResponse = {
        data: { success: true, message: 'deleted' },
        status: 200
      }
      
      const mockDelete = vi.fn().mockResolvedValue(mockResponse)
      ;(httpService as any).client = { delete: mockDelete }
      
      const result = await httpService.delete('/test/1')
      
      expect(mockDelete).toHaveBeenCalledWith('/test/1')
      expect(result).toEqual(mockResponse)
    })
  })

  describe('錯誤處理', () => {
    it('應該處理網路錯誤', async () => {
      const networkError = new Error('Network Error')
      
      const mockGet = vi.fn().mockRejectedValue(networkError)
      ;(httpService as any).client = { get: mockGet }
      
      await expect(httpService.get('/test')).rejects.toThrow('Network Error')
    })

    it('應該處理 HTTP 錯誤', async () => {
      const httpError = {
        response: {
          status: 404,
          data: { success: false, message: 'Not Found' }
        }
      }
      
      const mockGet = vi.fn().mockRejectedValue(httpError)
      ;(httpService as any).client = { get: mockGet }
      
      await expect(httpService.get('/test')).rejects.toEqual(httpError)
    })
  })

  describe('設定檢查', () => {
    it('應該正確初始化 HttpService', () => {
      expect(mockedAxios.create).toHaveBeenCalledWith({
        baseURL: import.meta.env.VITE_API_BASE_URL,
        timeout: 10000,
        headers: {
          'Content-Type': 'application/json',
        },
      })
    })
  })
})