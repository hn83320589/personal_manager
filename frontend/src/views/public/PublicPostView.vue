<template>
  <RouterLink
    :to="{ name: 'public-blog', params: { username } }"
    class="mt-10 inline-flex gap-1.5 text-[0.9rem] text-muted no-underline hover:text-ink"
  >
    ← 所有文章
  </RouterLink>

  <PageState :loading="loading" :error="error" :not-found="notFound" not-found-text="找不到這篇文章" @retry="reload">
    <div v-if="post" class="mt-5 lg:grid lg:grid-cols-[minmax(0,1fr)_13rem] lg:gap-12">
      <article class="mx-auto w-full max-w-read lg:mx-0">
        <span class="font-mono text-[0.82rem] text-muted">
          {{ [formatDate(post.publishedAt), post.category, `約 ${post.readingMinutes} 分鐘`].filter(Boolean).join(' · ') }}
        </span>
        <h1 class="mt-3 font-hand text-[clamp(2rem,5vw,3rem)] font-bold leading-[1.2]">{{ post.title }}</h1>
        <div v-if="post.coverImageUrl" class="mt-7 overflow-hidden rounded-[14px] border border-rule bg-soft">
          <img :src="post.coverImageUrl" alt="" class="w-full" />
        </div>
        <div ref="content" class="prose mt-8 text-[1.02rem] leading-[1.9]" v-html="html"></div>
        <TagList class="mt-10" :tags="post.tags" />
      </article>

      <aside v-if="toc.length > 1" class="hidden lg:block" aria-label="文章目錄">
        <nav class="sticky top-24 text-sm">
          <p class="mb-3 font-mono text-[0.72rem] uppercase tracking-[0.06em] text-muted">目錄</p>
          <ul class="flex flex-col gap-1.5 border-l border-rule">
            <li v-for="item in toc" :key="item.id">
              <a
                :href="`#${item.id}`"
                :class="[
                  '-ml-px block border-l-2 py-0.5 no-underline',
                  item.level === 3 ? 'pl-6' : 'pl-3',
                  item.id === activeId ? 'border-accent text-ink' : 'border-transparent text-muted hover:text-ink',
                ]"
                @click.prevent="scrollToHeading(item.id)"
              >
                {{ item.text }}
              </a>
            </li>
          </ul>
        </nav>
      </aside>
    </div>
  </PageState>
</template>

<script setup lang="ts">
import { computed, nextTick, ref, watch } from 'vue'
import { useRoute } from 'vue-router'
import { publicApi } from '@/api/public'
import { useAsyncData } from '@/composables/useAsyncData'
import { setPageSeo, stripHtml } from '@/composables/useSeo'
import { useTableOfContents } from '@/composables/useTableOfContents'
import { formatDate } from '@/lib/format'
import { highlightIn } from '@/lib/highlight'
import { sanitizeHtml } from '@/lib/sanitizeHtml'
import PageState from '@/components/public/PageState.vue'
import TagList from '@/components/public/TagList.vue'
import { usePublicContext } from '@/components/public/publicContext'

const { username, profile } = usePublicContext()
const route = useRoute()
const slug = computed(() => String(route.params.slug))

const {
  data: post,
  loading,
  error,
  notFound,
  reload,
} = useAsyncData(() => publicApi.post(username.value, slug.value), { watch: [username, slug] })

const html = computed(() => sanitizeHtml(post.value?.content))
const content = ref<HTMLElement>()
const { items: toc, activeId } = useTableOfContents(content, html)

// 文章中的程式碼區塊在內容渲染後上色
watch(html, async () => {
  await nextTick()
  if (content.value) highlightIn(content.value)
})

function scrollToHeading(id: string) {
  document.getElementById(id)?.scrollIntoView({ behavior: 'smooth', block: 'start' })
  history.replaceState(history.state, '', `#${id}`)
}

watch(post, (value) => {
  if (!value) return
  setPageSeo({
    title: `${value.title} · ${profile.value?.fullName || username.value}`,
    description: value.summary || stripHtml(value.content).slice(0, 160),
    ogImage: value.coverImageUrl || undefined,
    ogType: 'article',
  })
  recordView(value.slug)
})

/** 同一次瀏覽只計一次，重新整理不重複累加；計數失敗不影響閱讀。 */
function recordView(postSlug: string) {
  const key = `pm-viewed:${username.value}/${postSlug}`
  try {
    if (sessionStorage.getItem(key)) return
    sessionStorage.setItem(key, '1')
  } catch {
    // 無法使用 sessionStorage 時仍照常計數
  }
  publicApi.recordPostView(username.value, postSlug).catch(() => undefined)
}
</script>
