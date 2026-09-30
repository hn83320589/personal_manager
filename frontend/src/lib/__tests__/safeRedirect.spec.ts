import { describe, it, expect } from 'vitest'
import { safeRedirect } from '../safeRedirect'

describe('safeRedirect', () => {
  it('follows paths inside the site', () => {
    expect(safeRedirect('/admin/works/3')).toBe('/admin/works/3')
  })

  it.each(['//evil.com', '/\\evil.com', 'https://evil.com', undefined, ['/admin']])(
    'ignores %s and goes to the dashboard',
    (target) => {
      expect(safeRedirect(target)).toBe('/admin/dashboard')
    },
  )
})
