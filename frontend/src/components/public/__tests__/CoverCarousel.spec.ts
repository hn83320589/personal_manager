import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { mount } from '@vue/test-utils'
import CoverCarousel from '../CoverCarousel.vue'

const image = (n: number) => ({ url: `/files/${n}.jpg`, width: 800, height: 600, caption: '', alt: `封面 ${n}`, fileId: n })

function reducedMotion(matches: boolean) {
  vi.mocked(window.matchMedia).mockImplementation(
    (query: string) =>
      ({
        matches: query.includes('reduce') ? matches : false,
        media: query,
        addEventListener: vi.fn(),
        removeEventListener: vi.fn(),
      }) as unknown as MediaQueryList,
  )
}

const visibleSlide = (wrapper: ReturnType<typeof mount>) =>
  wrapper.findAll('img').findIndex((img) => img.attributes('data-active') === 'true')

describe('CoverCarousel', () => {
  beforeEach(() => {
    vi.useFakeTimers()
    reducedMotion(false)
  })
  afterEach(() => vi.useRealTimers())

  it('shows the first cover without dots when there is only one', () => {
    const wrapper = mount(CoverCarousel, { props: { images: [image(1)], title: '作品' } })

    expect([visibleSlide(wrapper), wrapper.findAll('button').length]).toEqual([0, 0])
  })

  it('advances automatically', async () => {
    const wrapper = mount(CoverCarousel, { props: { images: [image(1), image(2)], title: '作品', interval: 1000 } })

    await vi.advanceTimersByTimeAsync(1000)

    expect(visibleSlide(wrapper)).toBe(1)
  })

  it('switches to the chosen cover when a dot is clicked', async () => {
    const wrapper = mount(CoverCarousel, { props: { images: [image(1), image(2), image(3)], title: '作品' } })

    await wrapper.findAll('button')[2]!.trigger('click')

    expect(visibleSlide(wrapper)).toBe(2)
  })

  it('pauses while the pointer is over it', async () => {
    const wrapper = mount(CoverCarousel, { props: { images: [image(1), image(2)], title: '作品', interval: 1000 } })

    await wrapper.trigger('mouseenter')
    await vi.advanceTimersByTimeAsync(3000)

    expect(visibleSlide(wrapper)).toBe(0)
  })

  it('does not play automatically when the visitor prefers reduced motion', async () => {
    reducedMotion(true)
    const wrapper = mount(CoverCarousel, { props: { images: [image(1), image(2)], title: '作品', interval: 1000 } })

    await vi.advanceTimersByTimeAsync(3000)

    expect(visibleSlide(wrapper)).toBe(0)
  })
})
