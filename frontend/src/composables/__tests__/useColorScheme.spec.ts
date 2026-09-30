import { describe, it, expect, beforeEach } from 'vitest'
import { useColorScheme } from '../useColorScheme'

const root = document.documentElement

describe('useColorScheme', () => {
  beforeEach(() => {
    localStorage.clear()
    delete root.dataset.theme
    useColorScheme().setScheme('system')
  })

  it('follows the system setting by default', () => {
    expect(root.dataset.theme).toBeUndefined()
  })

  it('applies and remembers an explicit choice', () => {
    useColorScheme().setScheme('dark')

    expect([root.dataset.theme, localStorage.getItem('pm-color-scheme')]).toEqual(['dark', 'dark'])
  })

  it('returns to the system setting', () => {
    const { setScheme } = useColorScheme()
    setScheme('light')

    setScheme('system')

    expect([root.dataset.theme, localStorage.getItem('pm-color-scheme')]).toEqual([undefined, null])
  })

  it('shares the choice between components', () => {
    useColorScheme().setScheme('dark')

    expect(useColorScheme().scheme.value).toBe('dark')
  })
})
