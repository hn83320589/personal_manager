import { describe, it, expect, afterEach } from 'vitest'
import { defineComponent, h, ref } from 'vue'
import { mount } from '@vue/test-utils'
import { useAccent } from '../useAccent'

const root = document.documentElement

function mountWith(accent: string | null) {
  const color = ref(accent)
  const wrapper = mount(
    defineComponent({
      setup() {
        useAccent(color)
        return () => h('div')
      },
    }),
  )
  return { wrapper, color }
}

describe('useAccent', () => {
  afterEach(() => {
    delete root.dataset.accent
  })

  it('applies the owner’s theme color to the page', () => {
    mountWith('green')

    expect(root.dataset.accent).toBe('green')
  })

  it('uses the default blue for blue, unknown or missing colors', () => {
    mountWith('orange')

    expect(root.dataset.accent).toBeUndefined()
  })

  it('updates when the color changes', async () => {
    const { color } = mountWith('rose')

    color.value = 'purple'
    await Promise.resolve()

    expect(root.dataset.accent).toBe('purple')
  })

  it('restores the default when the page is left', () => {
    const { wrapper } = mountWith('slate')

    wrapper.unmount()

    expect(root.dataset.accent).toBeUndefined()
  })
})
