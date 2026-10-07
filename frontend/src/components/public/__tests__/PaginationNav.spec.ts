import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import PaginationNav from '../PaginationNav.vue'

const result = (page: number, totalPages: number) => ({
  page,
  totalPages,
  hasPreviousPage: page > 1,
  hasNextPage: page < totalPages,
})

const render = (page: number, totalPages: number) =>
  mount(PaginationNav, {
    props: { result: result(page, totalPages), prevLabel: '← 較新', nextLabel: '較舊 →' },
  })

describe('PaginationNav', () => {
  it('is hidden when everything fits on one page', () => {
    expect(render(1, 1).find('nav').exists()).toBe(false)
  })

  it('shows the current page out of the total', () => {
    expect(render(2, 5).text()).toContain('2 / 5')
  })

  it('moves to the next and previous page', async () => {
    const wrapper = render(2, 5)
    const [prev, next] = wrapper.findAll('button')
    await next!.trigger('click')
    await prev!.trigger('click')
    expect(wrapper.emitted('update:page')).toEqual([[3], [1]])
  })

  it('disables the buttons at either end', () => {
    const [prev] = render(1, 5).findAll('button')
    const [, next] = render(5, 5).findAll('button')
    expect([prev!.attributes('disabled'), next!.attributes('disabled')]).toEqual(['', ''])
  })
})
