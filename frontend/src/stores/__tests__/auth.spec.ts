import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useAuthStore } from '../auth'
import { authApi } from '@/api/auth'
import { ApiError, http, type AuthHooks } from '@/api/http'

vi.mock('@/api/auth', () => ({
  authApi: { login: vi.fn(), register: vi.fn(), refresh: vi.fn(), logout: vi.fn() },
}))

vi.mock('@/api/http', async (importOriginal) => {
  const actual = await importOriginal<typeof import('@/api/http')>()
  return { ...actual, http: { setAuthHooks: vi.fn() } }
})

const user = { id: 7, username: 'dada', email: 'dada@test.local', fullName: 'Dada', role: 'Admin' }
const session = (accessToken: string) => ({ accessToken, expiresAt: '2030-01-01T00:00:00Z', user })

/** store 交給 http 層的 hooks（http 用來取得 token、續期、通知過期）。 */
function hooks(): AuthHooks {
  return vi.mocked(http.setAuthHooks).mock.calls.at(-1)![0]
}

describe('auth store', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('starts signed out', () => {
    const store = useAuthStore()

    expect(store.isAuthenticated).toBe(false)
  })

  it('signs in and gives the access token to the http layer', async () => {
    vi.mocked(authApi.login).mockResolvedValue(session('token-1'))
    const store = useAuthStore()

    const success = await store.login({ username: 'dada', password: 'secret123' })

    expect(success).toBe(true)
    expect(store.user?.username).toBe('dada')
    expect(hooks().getAccessToken()).toBe('token-1')
  })

  it('shows the server message when sign in fails', async () => {
    vi.mocked(authApi.login).mockRejectedValue(new ApiError('帳號或密碼錯誤', 401))
    const store = useAuthStore()

    const success = await store.login({ username: 'dada', password: 'wrong' })

    expect(success).toBe(false)
    expect(store.error).toBe('帳號或密碼錯誤')
  })

  it('signs in right after registering', async () => {
    vi.mocked(authApi.register).mockResolvedValue(session('token-new'))
    const store = useAuthStore()

    const success = await store.register({
      username: 'dada',
      email: 'dada@test.local',
      password: 'secret123',
      fullName: 'Dada',
    })

    expect([success, store.isAuthenticated]).toEqual([true, true])
  })

  it('shows every validation problem when registration is rejected', async () => {
    vi.mocked(authApi.register).mockRejectedValue(
      new ApiError('資料格式有誤', 400, ['密碼至少 8 個字元']),
    )
    const store = useAuthStore()

    await store.register({
      username: 'dada',
      email: 'dada@test.local',
      password: 'short',
      fullName: 'Dada',
    })

    expect(store.error).toBe('資料格式有誤：密碼至少 8 個字元')
  })

  it('restores the session from the refresh cookie when the app starts', async () => {
    vi.mocked(authApi.refresh).mockResolvedValue(session('token-2'))
    const store = useAuthStore()

    await store.restoreSession()

    expect(store.isAuthenticated).toBe(true)
  })

  it('stays signed out when the visitor has never signed in', async () => {
    vi.mocked(authApi.refresh).mockResolvedValue(undefined) // 沒有 refresh cookie：204
    const store = useAuthStore()

    await store.restoreSession()

    expect([store.isAuthenticated, store.error]).toEqual([false, null])
  })

  it('treats a missing session as expired when the http layer needs a new token', async () => {
    vi.mocked(authApi.refresh).mockResolvedValue(undefined)
    useAuthStore()

    await expect(hooks().refreshAccessToken()).rejects.toBeInstanceOf(ApiError)
  })

  it('stays signed out without an error when the saved session is no longer valid', async () => {
    vi.mocked(authApi.refresh).mockRejectedValue(new ApiError('請重新登入', 401))
    const store = useAuthStore()

    await store.restoreSession()

    expect([store.isAuthenticated, store.error]).toEqual([false, null])
  })

  it('restores the session only once even when asked repeatedly', async () => {
    vi.mocked(authApi.refresh).mockResolvedValue(session('token-2'))
    const store = useAuthStore()

    await Promise.all([store.restoreSession(), store.restoreSession()])

    expect(authApi.refresh).toHaveBeenCalledTimes(1)
  })

  it('renews the access token for the http layer', async () => {
    vi.mocked(authApi.refresh).mockResolvedValue(session('token-3'))
    useAuthStore()

    await expect(hooks().refreshAccessToken()).resolves.toBe('token-3')
    expect(hooks().getAccessToken()).toBe('token-3')
  })

  it('signs out locally even when the server cannot be reached', async () => {
    vi.mocked(authApi.login).mockResolvedValue(session('token-1'))
    vi.mocked(authApi.logout).mockRejectedValue(new ApiError('無法連線到伺服器，請稍後再試', 0))
    const store = useAuthStore()
    await store.login({ username: 'dada', password: 'secret123' })

    await store.logout()

    expect([store.isAuthenticated, hooks().getAccessToken()]).toEqual([false, null])
  })

  it('clears the session and notifies the app when the session expires', async () => {
    vi.mocked(authApi.login).mockResolvedValue(session('token-1'))
    const store = useAuthStore()
    const onExpired = vi.fn()
    store.onSessionExpired(onExpired)
    await store.login({ username: 'dada', password: 'secret123' })

    hooks().onSessionExpired()

    expect([store.isAuthenticated, onExpired.mock.calls.length]).toEqual([false, 1])
  })
})
