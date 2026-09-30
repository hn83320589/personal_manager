import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import BaseInput from '../BaseInput.vue'

describe('BaseInput', () => {
  describe('渲染', () => {
    it('應該渲染輸入框', () => {
      const wrapper = mount(BaseInput, {
        props: { modelValue: '' }
      })
      
      expect(wrapper.find('input').exists()).toBe(true)
    })

    it('應該套用預設樣式', () => {
      const wrapper = mount(BaseInput, {
        props: { modelValue: '' }
      })
      const input = wrapper.find('input')
      
      expect(input.classes()).toContain('block')
      expect(input.classes()).toContain('w-full')
      expect(input.classes()).toContain('px-3')
      expect(input.classes()).toContain('py-2')
      expect(input.classes()).toContain('border')
      expect(input.classes()).toContain('rounded-lg')
      expect(input.classes()).toContain('text-sm')
    })
  })

  describe('標籤', () => {
    it('應該顯示標籤', () => {
      const wrapper = mount(BaseInput, {
        props: { 
          modelValue: '',
          label: 'Test Label' 
        }
      })
      
      const label = wrapper.find('label')
      expect(label.exists()).toBe(true)
      expect(label.text()).toBe('Test Label')
    })

    it('應該連結標籤與輸入框', () => {
      const wrapper = mount(BaseInput, {
        props: { 
          modelValue: '',
          label: 'Test Label'
        }
      })
      
      const label = wrapper.find('label')
      const input = wrapper.find('input')
      
      expect(label.attributes('for')).toBe(input.attributes('id'))
    })

    it('必填標籤應該顯示星號', () => {
      const wrapper = mount(BaseInput, {
        props: { 
          modelValue: '',
          label: 'Required Field', 
          required: true 
        }
      })
      
      const label = wrapper.find('label')
      expect(label.html()).toContain('*')
      expect(label.html()).toContain('text-red-500')
    })
  })

  describe('輸入類型', () => {
    it('應該設定正確的輸入類型', () => {
      const wrapper = mount(BaseInput, {
        props: { 
          modelValue: '',
          type: 'email' 
        }
      })
      
      expect(wrapper.find('input').attributes('type')).toBe('email')
    })

    it('預設類型應該是 text', () => {
      const wrapper = mount(BaseInput, {
        props: { modelValue: '' }
      })
      
      expect(wrapper.find('input').attributes('type')).toBe('text')
    })
  })

  describe('v-model', () => {
    it('應該正確綁定 modelValue', () => {
      const wrapper = mount(BaseInput, {
        props: { modelValue: 'test value' }
      })
      
      expect(wrapper.find('input').element.value).toBe('test value')
    })

    it('應該發送 update:modelValue 事件', async () => {
      const wrapper = mount(BaseInput, {
        props: { modelValue: '' }
      })
      const input = wrapper.find('input')
      
      await input.setValue('new value')
      
      expect(wrapper.emitted('update:modelValue')).toBeTruthy()
      expect(wrapper.emitted('update:modelValue')?.[0]).toEqual(['new value'])
    })
  })

  describe('佔位符', () => {
    it('應該設定 placeholder', () => {
      const wrapper = mount(BaseInput, {
        props: { 
          modelValue: '',
          placeholder: 'Enter text here' 
        }
      })
      
      expect(wrapper.find('input').attributes('placeholder')).toBe('Enter text here')
    })
  })

  describe('錯誤狀態', () => {
    it('錯誤狀態應該套用錯誤樣式', () => {
      const wrapper = mount(BaseInput, {
        props: { 
          modelValue: '',
          error: 'This field is required' 
        }
      })
      
      const input = wrapper.find('input')
      expect(input.classes()).toContain('border-red-300')
      expect(input.classes()).toContain('focus:border-red-500')
      expect(input.classes()).toContain('focus:ring-red-500')
    })

    it('應該顯示錯誤訊息', () => {
      const wrapper = mount(BaseInput, {
        props: { 
          modelValue: '',
          error: 'This field is required' 
        }
      })
      
      const errorMessage = wrapper.find('.text-red-600')
      expect(errorMessage.exists()).toBe(true)
      expect(errorMessage.text()).toBe('This field is required')
    })
  })

  describe('禁用狀態', () => {
    it('禁用狀態應該套用正確樣式和屬性', () => {
      const wrapper = mount(BaseInput, {
        props: { 
          modelValue: '',
          disabled: true 
        }
      })
      
      const input = wrapper.find('input')
      expect(input.attributes('disabled')).toBeDefined()
      expect(input.classes()).toContain('bg-gray-100')
      expect(input.classes()).toContain('text-gray-500')
      expect(input.classes()).toContain('cursor-not-allowed')
    })
  })

  describe('幫助文字', () => {
    it('應該顯示幫助文字', () => {
      const wrapper = mount(BaseInput, {
        props: { 
          modelValue: '',
          helpText: 'This is help text' 
        }
      })
      
      const helpText = wrapper.find('.text-gray-500')
      expect(helpText.exists()).toBe(true)
      expect(helpText.text()).toBe('This is help text')
    })

    it('有錯誤時不應該顯示幫助文字', () => {
      const wrapper = mount(BaseInput, {
        props: { 
          modelValue: '',
          helpText: 'This is help text',
          error: 'This field is required'
        }
      })
      
      const helpText = wrapper.find('.text-gray-500')
      expect(helpText.exists()).toBe(false)
    })
  })

  describe('必填狀態', () => {
    it('必填欄位應該有 required 屬性', () => {
      const wrapper = mount(BaseInput, {
        props: { 
          modelValue: '',
          required: true 
        }
      })
      
      expect(wrapper.find('input').attributes('required')).toBeDefined()
    })
  })

  describe('聚焦狀態', () => {
    it('聚焦時應該套用聚焦樣式', () => {
      const wrapper = mount(BaseInput, {
        props: { modelValue: '' }
      })
      const input = wrapper.find('input')
      
      // 聚焦樣式在 CSS 類別中定義，我們檢查是否包含聚焦相關的類別
      expect(input.classes()).toContain('focus:ring-2')
      expect(input.classes()).toContain('focus:ring-primary-500')
      expect(input.classes()).toContain('focus:border-primary-500')
    })
  })

  describe('其他屬性', () => {
    it('應該支援 readonly 屬性', () => {
      const wrapper = mount(BaseInput, {
        props: { 
          modelValue: '',
          readonly: true 
        }
      })
      
      expect(wrapper.find('input').attributes('readonly')).toBeDefined()
    })
  })

  describe('事件', () => {
    it('應該發送 focus 事件', async () => {
      const wrapper = mount(BaseInput, {
        props: { modelValue: '' }
      })
      const input = wrapper.find('input')
      
      await input.trigger('focus')
      
      expect(wrapper.emitted('focus')).toBeTruthy()
    })

    it('應該發送 blur 事件', async () => {
      const wrapper = mount(BaseInput, {
        props: { modelValue: '' }
      })
      const input = wrapper.find('input')
      
      await input.trigger('blur')
      
      expect(wrapper.emitted('blur')).toBeTruthy()
    })
  })
})