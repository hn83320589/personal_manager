import { describe, it, expect, vi } from 'vitest'
import type { AxiosAdapter, AxiosResponse, InternalAxiosRequestConfig } from 'axios'
import { AxiosError } from 'axios'
import { ApiError, createHttpClient, type AuthHooks } from '../http'

type Reply = { status: number; body?: unknown } | 'network-error'

/** 依序回傳預先排好的回應，並記錄每一個送出的請求。 */
function fakeServer(handler: (config: InternalAxiosRequestConfig) => Reply) {
  const requests: InternalAxiosRequestConfig[] = []
  const adapter: AxiosAdapter = async (config) => {
    requests.push(config)
    const reply = handler(config)
    if (reply === 'network-error') throw new AxiosError('Network Error', 'ERR_NETWORK', config)
    const response: AxiosResponse = {
      data: reply.body,
      status: reply.status,
      statusText: '',
      headers: {},
      config,
    }
    if (reply.status >= 400) {
      throw new AxiosError('failed', 'ERR_BAD_RESPONSE', config, null, response)
    }
    return response
  }
  return { adapter, requests }
}

const ok = (data: unknown): Reply => ({
  status: 200,
  body: { success: true, message: '', data, errors: [] },
})
const fail = (status: number, message = '失敗', errors: string[] = []): Reply => ({
  status,
  body: { success: false, message, errors },
})

function client(
  handler: (config: InternalAxiosRequestConfig) => Reply,
  hooks?: Partial<AuthHooks>,
) {
  const server = fakeServer(handler)
  const http = createHttpClient({ baseURL: '/api', adapter: server.adapter, retryDelayMs: 0 })
  const auth: AuthHooks = {
    getAccessToken: () => null,
    refreshAccessToken: vi.fn(async () => 'new-token'),
    onSessionExpired: vi.fn(),
    ...hooks,
  }
  http.setAuthHooks(auth)
  return { http, auth, requests: server.requests }
}

const bearer = (config: InternalAxiosRequestConfig) => config.headers.get('Authorization')

describe('http client', () => {
  it('returns the data inside the ApiResponse wrapper', async () => {
    const { http } = client(() => ok({ id: 1 }))

    await expect(http.get('/things/1')).resolves.toEqual({ id: 1 })
  })

  it('sends the in-memory access token as a bearer token', async () => {
    const { http, requests } = client(() => ok(null), { getAccessToken: () => 'abc' })

    await http.get('/me/things')

    expect(bearer(requests[0]!)).toBe('Bearer abc')
  })

  it('sends cookies so the refresh token cookie reaches the API', async () => {
    const { http, requests } = client(() => ok(null))

    await http.post('/auth/refresh')

    expect(requests[0]!.withCredentials).toBe(true)
  })

  it('refreshes once and retries the request with the new token after a 401', async () => {
    let token = 'expired'
    const { http, requests } = client(
      (config) => (bearer(config) === 'Bearer expired' ? fail(401) : ok('secret')),
      {
        getAccessToken: () => token,
        refreshAccessToken: vi.fn(async () => (token = 'fresh')),
      },
    )

    await expect(http.get('/me/things')).resolves.toBe('secret')
    expect(bearer(requests[1]!)).toBe('Bearer fresh')
  })

  it('shares a single refresh between requests that fail with 401 at the same time', async () => {
    let token = 'expired'
    const refreshAccessToken = vi.fn(async () => (token = 'fresh'))
    const { http } = client((config) => (bearer(config) === 'Bearer expired' ? fail(401) : ok(1)), {
      getAccessToken: () => token,
      refreshAccessToken,
    })

    await Promise.all([http.get('/me/a'), http.get('/me/b'), http.get('/me/c')])

    expect(refreshAccessToken).toHaveBeenCalledTimes(1)
  })

  it('ends the session when the refresh fails', async () => {
    const { http, auth } = client(() => fail(401), {
      getAccessToken: () => 'expired',
      refreshAccessToken: vi.fn(async () => {
        throw new Error('refresh token revoked')
      }),
    })

    await expect(http.get('/me/things')).rejects.toMatchObject({ status: 401 })
    expect(auth.onSessionExpired).toHaveBeenCalledTimes(1)
  })

  it('does not refresh when an auth request itself returns 401', async () => {
    const { http, auth } = client(() => fail(401, '帳號或密碼錯誤'))

    await expect(http.post('/auth/login', {})).rejects.toMatchObject({ message: '帳號或密碼錯誤' })
    expect(auth.refreshAccessToken).not.toHaveBeenCalled()
  })

  it('does not refresh again when the retried request is still unauthorized', async () => {
    const { http, auth, requests } = client(() => fail(401), { getAccessToken: () => 'token' })

    await expect(http.get('/me/things')).rejects.toBeInstanceOf(ApiError)
    expect((auth.refreshAccessToken as ReturnType<typeof vi.fn>).mock.calls.length).toBe(1)
    expect(requests).toHaveLength(2)
  })

  it('retries GET requests after a network error or server error', async () => {
    const replies: Reply[] = ['network-error', fail(503), ok('finally')]
    const { http } = client(() => replies.shift()!)

    await expect(http.get('/public/users')).resolves.toBe('finally')
  })

  it('gives up on GET after the retry limit', async () => {
    const { http, requests } = client(() => fail(500))

    await expect(http.get('/public/users')).rejects.toMatchObject({ status: 500 })
    expect(requests).toHaveLength(3)
  })

  it('never retries writes, so a request is not submitted twice', async () => {
    const { http, requests } = client(() => fail(503))

    await expect(http.post('/me/todos', { title: 'x' })).rejects.toMatchObject({ status: 503 })
    expect(requests).toHaveLength(1)
  })

  it('does not retry client errors', async () => {
    const { http, requests } = client(() => fail(404, '找不到'))

    await expect(http.get('/public/users/nobody')).rejects.toMatchObject({
      status: 404,
      message: '找不到',
    })
    expect(requests).toHaveLength(1)
  })

  it('exposes validation messages from the server', async () => {
    const { http } = client(() => fail(400, '資料格式有誤', ['請輸入標題']))

    await expect(http.put('/me/posts/1', {})).rejects.toMatchObject({
      status: 400,
      message: '資料格式有誤',
      errors: ['請輸入標題'],
    })
  })

  it('reports a readable message when the server cannot be reached', async () => {
    const { http } = client(() => 'network-error')

    await expect(http.post('/auth/login', {})).rejects.toMatchObject({
      status: 0,
      message: '無法連線到伺服器，請稍後再試',
    })
  })
})

describe('http client uploads', () => {
  it('sends the form without a timeout so large files can finish', async () => {
    const { http, requests } = client(() => ok({ id: 1 }))
    const form = new FormData()

    await http.upload('/me/files', form)

    expect([requests[0]!.data, requests[0]!.timeout]).toEqual([form, 0])
  })
})
