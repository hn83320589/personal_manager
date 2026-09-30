import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import BaseButton from '../BaseButton.vue'

describe('BaseButton', () => {
  describe('渲染', () => {
    it('渲染為 button 並顯示 slot 內容', () => {
      const wrapper = mount(BaseButton, { slots: { default: 'Click Me' } })

      expect(wrapper.element.tagName).toBe('BUTTON')
      expect(wrapper.text()).toContain('Click Me')
    })

    it('允許傳入自訂 class', () => {
      const wrapper = mount(BaseButton, { props: { class: 'custom-class' } })

      expect(wrapper.classes()).toContain('custom-class')
    })
  })

  describe('type 屬性', () => {
    it('預設 type 為 button，避免在表單中意外送出', () => {
      const wrapper = mount(BaseButton)

      expect(wrapper.element.type).toBe('button')
    })

    it('可設定為 submit', () => {
      const wrapper = mount(BaseButton, { props: { type: 'submit' } })

      expect(wrapper.element.type).toBe('submit')
    })
  })

  describe('點擊', () => {
    it('一般狀態點擊會發出 click 事件', async () => {
      const wrapper = mount(BaseButton)

      await wrapper.trigger('click')

      expect(wrapper.emitted('click')).toHaveLength(1)
    })

    it('disabled 時按鈕不可用且不發出 click', async () => {
      const wrapper = mount(BaseButton, { props: { disabled: true } })

      await wrapper.trigger('click')

      expect(wrapper.element.disabled).toBe(true)
      expect(wrapper.emitted('click')).toBeUndefined()
    })

    it('loading 時按鈕不可用且不發出 click', async () => {
      const wrapper = mount(BaseButton, { props: { loading: true } })

      await wrapper.trigger('click')

      expect(wrapper.element.disabled).toBe(true)
      expect(wrapper.emitted('click')).toBeUndefined()
    })
  })

  describe('loading 指示', () => {
    it('loading 時顯示載入指示器', () => {
      const wrapper = mount(BaseButton, { props: { loading: true } })

      expect(wrapper.findComponent({ name: 'LoadingSpinner' }).exists()).toBe(true)
    })

    it('非 loading 時不顯示載入指示器', () => {
      const wrapper = mount(BaseButton)

      expect(wrapper.findComponent({ name: 'LoadingSpinner' }).exists()).toBe(false)
    })
  })
})
