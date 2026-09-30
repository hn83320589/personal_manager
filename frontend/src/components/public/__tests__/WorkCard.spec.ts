import { describe, it, expect } from 'vitest'
import { mount, RouterLinkStub } from '@vue/test-utils'
import WorkCard from '../WorkCard.vue'
import type { Schemas } from '@/api/types'
import type { CardStyle } from '../workCards'

const work = (overrides: Partial<Schemas['PortfolioCardDto']> = {}): Schemas['PortfolioCardDto'] => ({
  slug: 'shancha',
  title: '山茶行 品牌識別',
  summary: '為經營四十年的茶行重新整理品牌。',
  category: '品牌識別',
  year: 2025,
  tags: ['品牌', '包裝'],
  isFeatured: false,
  covers: [{ url: '/files/cover.png', width: 1600, height: 1200, caption: '', alt: '', fileId: 1 }],
  coverFocus: '50% 30%',
  metrics: [{ value: '+38%', label: '回購率' }],
  ...overrides,
})

const render = (variant: CardStyle, overrides: Partial<Schemas['PortfolioCardDto']> = {}) =>
  mount(WorkCard, {
    props: { work: work(overrides), username: 'dada', variant, ratio: 'Portrait' },
    global: { stubs: { RouterLink: RouterLinkStub } },
  })

describe('WorkCard', () => {
  it('links to the case study', () => {
    const wrapper = render('Info')

    expect(wrapper.getComponent(RouterLinkStub).props('to')).toEqual({
      name: 'public-work',
      params: { username: 'dada', slug: 'shancha' },
    })
  })

  it('keeps visual cards image-first without the summary', () => {
    const text = render('Visual').text()

    expect([text.includes('品牌識別'), text.includes('四十年')]).toEqual([true, false])
  })

  it('shows the summary and tags on info cards', () => {
    const text = render('Info').text()

    expect([text.includes('四十年'), text.includes('包裝')]).toEqual([true, true])
  })

  it('leads with the key metrics on tech cards', () => {
    expect(render('Tech').text()).toContain('+38%回購率')
  })

  it('generates a text cover when the work has no images', () => {
    const wrapper = render('Info', { covers: [] })

    expect([wrapper.find('img').exists(), wrapper.text()]).toEqual([false, expect.stringContaining('山茶行')])
  })
})
