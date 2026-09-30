<template>
  <div>
    <LoadingSpinner v-if="isLoading" size="medium" text="載入中..." />

    <article v-else-if="post" class="lg:flex lg:gap-6 space-y-6 lg:space-y-0">
      <!-- Main column -->
      <div class="flex-1 min-w-0 space-y-6">
        <div>
          <RouterLink :to="`/@${username}/blog`" class="text-sm text-sky-600 hover:text-sky-800">← 返回部落格</RouterLink>
        </div>

        <!-- Article Header -->
        <header class="bg-white rounded-xl border border-gray-200 shadow-sm p-6 sm:p-8">
          <div class="flex flex-wrap items-center gap-3 mb-3">
            <span
              v-if="post.category"
              class="text-xs font-medium px-2.5 py-1 rounded-full"
              :style="{ backgroundColor: 'var(--color-primary-light,#e0f2fe)', color: 'var(--color-primary-dark,#0c4a6e)' }"
            >{{ post.category }}</span>
          </div>
          <h1 class="text-2xl sm:text-3xl font-bold text-gray-900 leading-tight mb-4">{{ post.title }}</h1>
          <p v-if="post.summary" class="text-gray-500 text-base leading-relaxed mb-4">{{ post.summary }}</p>
          <div class="flex flex-wrap gap-4 text-sm text-gray-400 pb-5 border-b border-gray-100">
            <span v-if="post.publishedAt">{{ formatDate(post.publishedAt) }}</span>
            <span v-if="post.viewCount">{{ post.viewCount }} 次閱讀</span>
            <span class="flex items-center gap-1">
              <ClockIcon class="h-4 w-4" />{{ estimateReadTime(post.content) }} 分鐘閱讀
            </span>
          </div>
          <!-- Tags -->
          <div v-if="post.tags" class="flex flex-wrap gap-2 pt-4">
            <span
              v-for="tag in parseTags(post.tags)"
              :key="tag"
              class="text-xs px-3 py-1 rounded-full font-medium bg-sky-50 text-sky-600"
            >#{{ tag }}</span>
          </div>
        </header>

        <!-- Article Content -->
        <main class="bg-white rounded-xl border border-gray-200 shadow-sm p-6 sm:p-8">
          <div ref="contentRef" class="prose prose-gray max-w-none" v-html="post.content"></div>
        </main>

        <!-- Footer: share + update time -->
        <footer class="bg-white rounded-xl border border-gray-200 shadow-sm p-5">
          <div class="flex items-center justify-between flex-wrap gap-3">
            <span class="text-xs text-gray-400">最後更新：{{ formatDate(post.updatedAt) }}</span>
            <div class="flex items-center gap-3">
              <span class="text-sm text-gray-500">分享：</span>
              <button
                @click="copyLink"
                :class="copied ? 'text-green-500' : 'text-gray-400 hover:text-gray-600'"
                class="flex items-center gap-1 text-sm transition-colors"
                title="複製連結"
              >
                <LinkIcon class="h-4 w-4" />
                {{ copied ? '已複製！' : '複製連結' }}
              </button>
            </div>
          </div>
        </footer>
      </div>

      <!-- TOC Sidebar (lg+ only) -->
      <aside v-if="tocItems.length > 1" class="hidden lg:block w-52 flex-shrink-0">
        <div class="sticky top-8">
          <p class="text-xs font-semibold text-gray-500 uppercase tracking-wider mb-3">目錄</p>
          <nav class="space-y-1">
            <a
              v-for="item in tocItems"
              :key="item.id"
              :href="`#${item.id}`"
              :class="[
                'block text-sm leading-snug transition-colors rounded px-2 py-1',
                item.level === 1 ? 'font-medium' : item.level === 2 ? 'pl-4' : 'pl-7',
                activeId === item.id
                  ? 'text-[var(--color-primary,#0ea5e9)] bg-[var(--color-primary-light,#e0f2fe)]'
                  : 'text-gray-500 hover:text-gray-800'
              ]"
              @click.prevent="scrollTo(item.id)"
            >{{ item.text }}</a>
          </nav>
        </div>
      </aside>
    </article>

    <div v-else-if="!isLoading" class="text-center text-gray-400 py-12">找不到此文章</div>
  </div>
</template>

<script setup lang="ts">
import { ref, inject, onMounted, nextTick, onUnmounted } from 'vue'
import type { ComputedRef } from 'vue'
import { useRoute, RouterLink } from 'vue-router'
import { ClockIcon, LinkIcon } from '@heroicons/vue/24/outline'
import httpService from '@/services/http'
import { setPageSeo, stripHtml } from '@/composables/useSeo'
import LoadingSpinner from '@/components/common/LoadingSpinner.vue'
import type { BlogPost } from '@/types/api'

const route = useRoute()
const username = inject<ComputedRef<string>>('username')

const isLoading = ref(true)
const post = ref<BlogPost | null>(null)
const copied = ref(false)
const contentRef = ref<HTMLElement | null>(null)

interface TocItem { id: string; text: string; level: 1 | 2 | 3 }
const tocItems = ref<TocItem[]>([])
const activeId = ref('')

let observer: IntersectionObserver | null = null

function buildToc() {
  if (!contentRef.value) return
  const headings = contentRef.value.querySelectorAll('h1, h2, h3')
  const items: TocItem[] = []
  headings.forEach((el, i) => {
    if (!el.id) el.id = `heading-${i}`
    const tag = el.tagName.toLowerCase() as 'h1' | 'h2' | 'h3'
    const level = parseInt(tag[1]) as 1 | 2 | 3
    items.push({ id: el.id, text: el.textContent ?? '', level })
  })
  tocItems.value = items

  if (observer) observer.disconnect()
  observer = new IntersectionObserver(
    (entries) => {
      for (const entry of entries) {
        if (entry.isIntersecting) {
          activeId.value = entry.target.id
          break
        }
      }
    },
    { rootMargin: '0px 0px -60% 0px', threshold: 0 }
  )
  headings.forEach(el => observer!.observe(el))
}

function scrollTo(id: string) {
  document.getElementById(id)?.scrollIntoView({ behavior: 'smooth', block: 'start' })
}

onUnmounted(() => { observer?.disconnect() })

function formatDate(dateStr: string): string {
  return new Date(dateStr).toLocaleDateString('zh-TW', { year: 'numeric', month: 'long', day: 'numeric' })
}

function estimateReadTime(content: string): number {
  return Math.max(1, Math.ceil(content.trim().split(/\s+/).length / 200))
}

function parseTags(tags: string[] | string): string[] {
  if (Array.isArray(tags)) return tags.filter(Boolean)
  return tags.split(',').map(t => t.trim()).filter(Boolean)
}

async function copyLink() {
  try {
    await navigator.clipboard.writeText(window.location.href)
    copied.value = true
    setTimeout(() => { copied.value = false }, 2000)
  } catch { /* ignore */ }
}

onMounted(async () => {
  const slug = route.params.slug as string
  try {
    const res = await httpService.get<BlogPost>(`/blogposts/slug/${encodeURIComponent(slug)}`)
    if (res.success && res.data) {
      post.value = res.data
      httpService.post(`/blogposts/${res.data.id}/view`, {}).catch(() => {})
      const desc = res.data.summary || stripHtml(res.data.content).slice(0, 160)
      setPageSeo({
        title: res.data.title,
        description: desc,
        ogType: 'article',
      })
      await nextTick()
      buildToc()
    }
  } catch {
    // ignore
  } finally {
    isLoading.value = false
  }
})
</script>

<style scoped>
.prose-content {
  color: #374151;
  line-height: 1.75;
  white-space: pre-wrap;
  font-size: 0.9375rem;
}
</style>
