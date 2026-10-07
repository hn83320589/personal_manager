import { http } from './http'
import type { Schemas } from './types'

type AccessToken = Schemas['AccessTokenDto']

/** /api/auth 與 /api/me/password。refresh token 由瀏覽器以 httpOnly cookie 自動送出。 */
export const authApi = {
  login: (body: Schemas['LoginRequest']) => http.post<AccessToken>('/auth/login', body),
  register: (body: Schemas['RegisterRequest']) => http.post<AccessToken>('/auth/register', body),
  /** 沒有 refresh cookie（從未登入或已登出）時伺服器回 204，結果為 undefined。 */
  refresh: () => http.post<AccessToken | undefined>('/auth/refresh'),
  logout: () => http.post('/auth/logout'),
  forgotPassword: (body: Schemas['ForgotPasswordRequest']) =>
    http.post('/auth/forgot-password', body),
  resetPassword: (body: Schemas['ResetPasswordRequest']) => http.post('/auth/reset-password', body),
  changePassword: (body: Schemas['ChangePasswordRequest']) => http.put('/me/password', body),
}
