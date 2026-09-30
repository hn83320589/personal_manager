import { describe, it, expect, vi, afterEach } from 'vitest'
import { mount } from '@vue/test-utils'
import DeleteButton from '../DeleteButton.vue'

describe('DeleteButton', () => {
  afterEach(() => vi.useRealTimers())

  it('asks for confirmation before deleting', async () => {
    const wrapper = mount(DeleteButton, { props: { itemName: 'Vue.js' } })

    await wrapper.trigger('click')

    expect([wrapper.text(), wrapper.emitted('confirm')]).toEqual(['確定刪除？', undefined])
  })

  it('deletes on the second click', async () => {
    const wrapper = mount(DeleteButton)

    await wrapper.trigger('click')
    await wrapper.trigger('click')

    expect(wrapper.emitted('confirm')).toHaveLength(1)
  })

  it('goes back to normal if not confirmed in time', async () => {
    vi.useFakeTimers()
    const wrapper = mount(DeleteButton)
    await wrapper.trigger('click')

    vi.advanceTimersByTime(4000)
    await wrapper.vm.$nextTick()

    expect(wrapper.text()).toBe('刪除')
  })
})
