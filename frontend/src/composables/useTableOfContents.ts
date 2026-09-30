import { nextTick, onUnmounted, ref, watch, type Ref } from 'vue'

export interface TocItem {
  id: string
  text: string
  level: 2 | 3
}

/**
 * 由文章內容的 h2／h3 產生目錄，並追蹤目前讀到的段落。
 * 內容變更後（例如切換文章）會重新建立。
 */
export function useTableOfContents(container: Ref<HTMLElement | undefined>, source: Ref<unknown>) {
  const items = ref<TocItem[]>([])
  const activeId = ref<string | null>(null)
  let observer: IntersectionObserver | undefined

  async function build() {
    observer?.disconnect()
    await nextTick()
    const root = container.value
    if (!root) return

    const headings = [...root.querySelectorAll<HTMLHeadingElement>('h2, h3')]
    items.value = headings.map((heading, i) => {
      heading.id ||= `section-${i + 1}`
      return {
        id: heading.id,
        text: heading.textContent?.trim() ?? '',
        level: heading.tagName === 'H2' ? 2 : 3,
      }
    })

    if (!('IntersectionObserver' in window) || !headings.length) return
    observer = new IntersectionObserver(
      (entries) => {
        const visible = entries.find((entry) => entry.isIntersecting)
        if (visible) activeId.value = visible.target.id
      },
      { rootMargin: '0px 0px -70% 0px' },
    )
    headings.forEach((heading) => observer!.observe(heading))
  }

  watch(source, build, { immediate: true })
  onUnmounted(() => observer?.disconnect())

  return { items, activeId }
}
