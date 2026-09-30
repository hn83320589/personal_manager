import { describe, it, expect, beforeEach, vi } from 'vitest'
import { httpService } from '../http'

// httpService 是模組載入時就建立的單例，mock 的 axios.create 必須在 import 當下就回傳可用的 client
vi.mock('axios', () => ({
  default: {
    create: vi.fn(() => ({
      interceptors: {
        request: { use: vi.fn() },
        response: { use: vi.fn() },
      },
    })),
  },
}))

type FakeClient = Record<'get' | 'post' | 'put' | 'delete', ReturnType<typeof vi.fn>>

function useFakeClient(): FakeClient {
  const client: FakeClient = { get: vi.fn(), post: vi.fn(), put: vi.fn(), delete: vi.fn() }
  ;(httpService as unknown as { client: FakeClient }).client = client
  return client
}

const body = { success: true, message: '', data: 'payload', errors: null }

describe('HttpService', () => {
  let client: FakeClient

  beforeEach(() => {
    client = useFakeClient()
  })

  describe('成功回應', () => {
    it('GET 回傳 ApiResponse 本體，並把查詢參數放進 params', async () => {
      client.get.mockResolvedValue({ data: body, status: 200 })

      const result = await httpService.get('/skills', { page: 2 })

      expect(client.get).toHaveBeenCalledWith('/skills', { params: { page: 2 } })
      expect(result).toEqual(body)
    })

    it('POST 送出資料並回傳 ApiResponse 本體', async () => {
      client.post.mockResolvedValue({ data: body, status: 201 })

      const result = await httpService.post('/skills', { name: 'Vue' })

      expect(client.post).toHaveBeenCalledWith('/skills', { name: 'Vue' })
      expect(result).toEqual(body)
    })

    it('PUT 送出資料並回傳 ApiResponse 本體', async () => {
      client.put.mockResolvedValue({ data: body, status: 200 })

      const result = await httpService.put('/skills/1', { name: 'Vue 3' })

      expect(client.put).toHaveBeenCalledWith('/skills/1', { name: 'Vue 3' })
      expect(result).toEqual(body)
    })

    it('DELETE 回傳 ApiResponse 本體', async () => {
      client.delete.mockResolvedValue({ data: body, status: 200 })

      const result = await httpService.delete('/skills/1')

      expect(client.delete).toHaveBeenCalledWith('/skills/1')
      expect(result).toEqual(body)
    })
  })

  describe('錯誤處理', () => {
    it('4xx 錯誤直接拋出，不重試', async () => {
      const notFound = Object.assign(new Error('Not Found'), { response: { status: 404 } })
      client.get.mockRejectedValue(notFound)

      await expect(httpService.get('/skills/999')).rejects.toBe(notFound)
      expect(client.get).toHaveBeenCalledTimes(1)
    })
  })
})
