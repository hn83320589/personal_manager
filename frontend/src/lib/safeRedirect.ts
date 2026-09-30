/** 登入後只接受站內路徑，避免被導向外部網站（例如 //evil.com 這類開放重導向）。 */
export function safeRedirect(target: unknown, fallback = '/admin/dashboard'): string {
  return typeof target === 'string' &&
    target.startsWith('/') &&
    !target.startsWith('//') &&
    !target.startsWith('/\\')
    ? target
    : fallback
}
