import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import BaseButton from '../BaseButton.vue'

describe('BaseButton', () => {
  describe('渲染', () => {
    it('應該渲染按鈕文字', () => {
      const wrapper = mount(BaseButton, {
        slots: {
          default: 'Click Me'
        }
      })
      
      expect(wrapper.text()).toContain('Click Me')
      expect(wrapper.element.tagName).toBe('BUTTON')
    })

    it('應該套用正確的預設樣式', () => {
      const wrapper = mount(BaseButton)
      
      expect(wrapper.classes()).toContain('inline-flex')
      expect(wrapper.classes()).toContain('items-center')
      expect(wrapper.classes()).toContain('justify-center')
      expect(wrapper.classes()).toContain('px-4')
      expect(wrapper.classes()).toContain('py-2')
      expect(wrapper.classes()).toContain('border')
      expect(wrapper.classes()).toContain('border-transparent')
      expect(wrapper.classes()).toContain('text-sm')
      expect(wrapper.classes()).toContain('font-medium')
      expect(wrapper.classes()).toContain('rounded-md')
    })
  })

  describe('variants 變體', () => {
    it('primary 變體應該套用正確樣式', () => {
      const wrapper = mount(BaseButton, {
        props: { variant: 'primary' }
      })
      
      expect(wrapper.classes()).toContain('text-white')
      expect(wrapper.classes()).toContain('bg-blue-600')
      expect(wrapper.classes()).toContain('hover:bg-blue-700')
      expect(wrapper.classes()).toContain('focus:ring-blue-500')
    })

    it('secondary 變體應該套用正確樣式', () => {
      const wrapper = mount(BaseButton, {
        props: { variant: 'secondary' }
      })
      
      expect(wrapper.classes()).toContain('text-gray-700')
      expect(wrapper.classes()).toContain('bg-white')
      expect(wrapper.classes()).toContain('border-gray-300')
      expect(wrapper.classes()).toContain('hover:bg-gray-50')
    })

    it('outline 變體應該套用正確樣式', () => {
      const wrapper = mount(BaseButton, {
        props: { variant: 'outline' }
      })
      
      expect(wrapper.classes()).toContain('text-blue-700')
      expect(wrapper.classes()).toContain('bg-transparent')
      expect(wrapper.classes()).toContain('border-blue-300')
      expect(wrapper.classes()).toContain('hover:bg-blue-50')
    })

    it('outline 變體應該套用正確樣式', () => {
      const wrapper = mount(BaseButton, {
        props: { variant: 'outline' }
      })
      
      expect(wrapper.classes()).toContain('text-blue-600')
      expect(wrapper.classes()).toContain('bg-transparent')
      expect(wrapper.classes()).toContain('border-blue-600')
      expect(wrapper.classes()).toContain('hover:bg-blue-50')
    })

    it('danger 變體應該套用正確樣式', () => {
      const wrapper = mount(BaseButton, {
        props: { variant: 'danger' }
      })
      
      expect(wrapper.classes()).toContain('text-white')
      expect(wrapper.classes()).toContain('bg-red-600')
      expect(wrapper.classes()).toContain('hover:bg-red-700')
      expect(wrapper.classes()).toContain('focus:ring-red-500')
    })
  })

  describe('sizes 尺寸', () => {
    it('small 尺寸應該套用正確樣式', () => {
      const wrapper = mount(BaseButton, {
        props: { size: 'small' }
      })
      
      expect(wrapper.classes()).toContain('px-3')
      expect(wrapper.classes()).toContain('py-1.5')
      expect(wrapper.classes()).toContain('text-xs')
    })

    it('medium 尺寸應該套用正確樣式', () => {
      const wrapper = mount(BaseButton, {
        props: { size: 'medium' }
      })
      
      expect(wrapper.classes()).toContain('px-4')
      expect(wrapper.classes()).toContain('py-2')
      expect(wrapper.classes()).toContain('text-sm')
    })

    it('large 尺寸應該套用正確樣式', () => {
      const wrapper = mount(BaseButton, {
        props: { size: 'large' }
      })
      
      expect(wrapper.classes()).toContain('px-6')
      expect(wrapper.classes()).toContain('py-3')
      expect(wrapper.classes()).toContain('text-base')
    })
  })

  describe('狀態', () => {
    it('disabled 狀態應該套用正確樣式和屬性', () => {
      const wrapper = mount(BaseButton, {
        props: { disabled: true }
      })
      
      expect(wrapper.classes()).toContain('opacity-50')
      expect(wrapper.classes()).toContain('cursor-not-allowed')
      expect(wrapper.element.disabled).toBe(true)
    })

    it('loading 狀態應該顯示載入指示器', () => {
      const wrapper = mount(BaseButton, {
        props: { loading: true }
      })
      
      expect(wrapper.classes()).toContain('opacity-75')
      expect(wrapper.classes()).toContain('cursor-not-allowed')
      expect(wrapper.element.disabled).toBe(true)
      
      // 檢查載入指示器 SVG
      const loadingIcon = wrapper.find('svg')
      expect(loadingIcon.exists()).toBe(true)
      expect(loadingIcon.classes()).toContain('animate-spin')
    })

    it('loading 狀態不應該發送點擊事件', async () => {
      const wrapper = mount(BaseButton, {
        props: { loading: true }
      })
      
      await wrapper.trigger('click')
      
      expect(wrapper.emitted()).not.toHaveProperty('click')
    })
  })

  describe('事件', () => {
    it('應該發送點擊事件', async () => {
      const wrapper = mount(BaseButton)
      
      await wrapper.trigger('click')
      
      expect(wrapper.emitted()).toHaveProperty('click')
      expect(wrapper.emitted().click).toHaveLength(1)
    })

    it('disabled 狀態不應該發送點擊事件', async () => {
      const wrapper = mount(BaseButton, {
        props: { disabled: true }
      })
      
      await wrapper.trigger('click')
      
      expect(wrapper.emitted()).not.toHaveProperty('click')
    })
  })

  describe('完整寬度', () => {
    it('fullWidth 屬性應該套用 w-full 類別', () => {
      const wrapper = mount(BaseButton, {
        props: { fullWidth: true }
      })
      
      expect(wrapper.classes()).toContain('w-full')
    })
  })

  describe('自訂類別', () => {
    it('應該允許自訂 class', () => {
      const wrapper = mount(BaseButton, {
        props: { class: 'custom-class' }
      })
      
      expect(wrapper.classes()).toContain('custom-class')
    })
  })

  describe('type 屬性', () => {
    it('應該設定正確的 type 屬性', () => {
      const wrapper = mount(BaseButton, {
        props: { type: 'submit' }
      })
      
      expect(wrapper.element.type).toBe('submit')
    })

    it('預設 type 應該是 button', () => {
      const wrapper = mount(BaseButton)
      
      expect(wrapper.element.type).toBe('button')
    })
  })
})