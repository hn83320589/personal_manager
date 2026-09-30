import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import TagInput from '../TagInput.vue'

const render = (tags: string[] = []) => mount(TagInput, { props: { modelValue: tags, id: 'tags' } })

async function type(wrapper: ReturnType<typeof render>, text: string, key: string) {
  const input = wrapper.get('input')
  await input.setValue(text)
  await input.trigger('keydown', { key })
}

describe('TagInput', () => {
  it('adds a tag on Enter', async () => {
    const wrapper = render(['品牌'])

    await type(wrapper, '包裝', 'Enter')

    expect(wrapper.emitted('update:modelValue')).toEqual([[['品牌', '包裝']]])
  })

  it('adds a tag when a comma is typed', async () => {
    const wrapper = render()

    await type(wrapper, 'Vue', ',')

    expect(wrapper.emitted('update:modelValue')).toEqual([[['Vue']]])
  })

  it('does not add the same tag twice, ignoring case', async () => {
    const wrapper = render(['Vue'])

    await type(wrapper, 'vue', 'Enter')

    expect(wrapper.emitted('update:modelValue')).toBeUndefined()
  })

  it('ignores Enter while an input method is still composing', async () => {
    const wrapper = render()
    const input = wrapper.get('input')

    await input.setValue('設計')
    await input.trigger('keydown', { key: 'Enter', isComposing: true })

    expect(wrapper.emitted('update:modelValue')).toBeUndefined()
  })

  it('removes the last tag with Backspace in an empty field', async () => {
    const wrapper = render(['品牌', '包裝'])

    await wrapper.get('input').trigger('keydown', { key: 'Backspace' })

    expect(wrapper.emitted('update:modelValue')).toEqual([[['品牌']]])
  })
})
