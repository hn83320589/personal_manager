import { describe, it, expect } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import type { RouteLocationRaw } from 'vue-router'
import router from '../index'

const nameOf = (path: string) => router.resolve(path).name

/** 依路由定義計算舊網址會被導向哪裡（不實際導覽，避免觸發登入檢查與載入頁面）。 */
function redirectOf(path: string): string {
  const resolved = router.resolve(path)
  const record = resolved.matched[resolved.matched.length - 1]!
  const redirect = record.redirect
  const target = (
    typeof redirect === 'function' ? redirect(resolved) : redirect
  ) as RouteLocationRaw
  return router.resolve(target).fullPath
}

describe('routes', () => {
  it.each([
    ['/admin/works/3', 'manage-work'],
    ['/admin/blog/5', 'manage-post'],
    ['/@dada', 'public-home'],
    ['/@dada/works/shancha', 'public-work'],
    ['/@dada/blog/hello-world', 'public-post'],
  ])('%s opens %s', (path, name) => {
    expect(nameOf(path)).toBe(name)
  })

  it('does not treat a non-numeric id as an editor page', () => {
    expect(nameOf('/admin/works/abc')).toBe('not-found')
  })

  it.each([
    ['/@dada/portfolio', '/@dada/works'],
    ['/@dada/contact', '/@dada#contact'],
    ['/admin/projects', '/admin/works'],
    ['/admin/blog/editor/7', '/admin/blog/7'],
  ])('redirects the old address %s to %s', (from, to) => {
    expect(redirectOf(from)).toBe(to)
  })
})

describe('page title', () => {
  it('uses the route title with the site name', async () => {
    setActivePinia(createPinia())
    await router.push('/')
    expect(document.title).toBe('探索個人頁面 | Personal Manager')
  })
})
