<template>
  <RouterLink
    :to="{ name: 'public-works', params: { username } }"
    class="mt-10 inline-flex gap-1.5 text-[0.9rem] text-muted no-underline hover:text-ink"
  >
    ← 所有作品
  </RouterLink>

  <PageState :loading="loading" :error="error" :not-found="notFound" not-found-text="找不到這件作品" @retry="reload">
    <template v-if="data">
      <article>
        <header class="mt-5 flex max-w-[52rem] flex-col gap-[18px]">
          <span class="font-mono text-[0.74rem] uppercase tracking-[0.08em] text-muted">
            {{ [work.year, work.category].filter(Boolean).join(' · ') }}
          </span>
          <h1 class="font-hand text-[clamp(2.2rem,5.5vw,3.6rem)] font-bold leading-[1.15]">{{ work.title }}</h1>
          <p v-if="work.summary" class="text-[clamp(1.05rem,1.8vw,1.2rem)] text-muted">{{ work.summary }}</p>
          <TagList :tags="work.tags" />
        </header>

        <dl
          v-if="facts.length || work.links.length"
          class="mb-0 mt-9 grid grid-cols-[repeat(auto-fit,minmax(11rem,1fr))] gap-px overflow-hidden rounded-card border border-rule bg-rule"
        >
          <div v-for="fact in facts" :key="fact.label" class="flex min-w-0 flex-col gap-1 bg-surface px-4 py-3.5">
            <dt class="font-mono text-[0.72rem] tracking-[0.06em] text-muted">{{ fact.label }}</dt>
            <dd class="m-0 text-[0.93rem]">{{ fact.value }}</dd>
          </div>
          <div v-if="work.links.length" class="flex min-w-0 flex-col gap-1 bg-surface px-4 py-3.5">
            <dt class="font-mono text-[0.72rem] tracking-[0.06em] text-muted">連結</dt>
            <dd class="m-0 flex flex-wrap gap-x-3 text-[0.93rem]">
              <a
                v-for="link in work.links"
                :key="link.url"
                :href="link.url"
                target="_blank"
                rel="noopener"
                class="text-accent no-underline hover:underline"
              >
                {{ link.label }} ↗
              </a>
            </dd>
          </div>
        </dl>

        <div
          v-if="work.covers[0]"
          class="mt-8 aspect-[16/8] overflow-hidden rounded-[14px] border border-rule bg-soft"
        >
          <img
            :src="work.covers[0].url"
            :alt="work.covers[0].alt || `${work.title} 封面`"
            :style="{ objectPosition: work.coverFocus }"
            class="h-full w-full object-cover"
          />
        </div>

        <CaseBlocks class="mt-14" :blocks="work.blocks" @open-image="lightboxIndex = $event" />
      </article>

      <nav
        v-if="data.neighbours"
        class="mt-20 grid gap-4 min-[561px]:grid-cols-2"
        aria-label="其他作品"
      >
        <RouterLink
          v-for="neighbour in data.neighbours"
          :key="neighbour.slug"
          :to="{ name: 'public-work', params: { username, slug: neighbour.slug } }"
          :class="[
            'flex flex-col gap-1 rounded-card border border-rule bg-surface px-5 py-[18px] no-underline hover:border-ink',
            neighbour.direction === 'next' ? 'min-[561px]:col-start-2 min-[561px]:text-right' : '',
          ]"
        >
          <span class="font-mono text-[0.74rem] uppercase tracking-[0.08em] text-muted">
            {{ neighbour.direction === 'previous' ? '上一件' : '下一件' }}
          </span>
          <b>{{ neighbour.title }}</b>
        </RouterLink>
      </nav>

      <ImageLightbox v-model:index="lightboxIndex" :images="images" />
    </template>
  </PageState>
</template>

<script setup lang="ts">
import { computed, ref, watch, watchEffect } from 'vue'
import { useRoute } from 'vue-router'
import { publicApi } from '@/api/public'
import { useAsyncData } from '@/composables/useAsyncData'
import { setPageSeo } from '@/composables/useSeo'
import ImageLightbox from '@/components/public/ImageLightbox.vue'
import PageState from '@/components/public/PageState.vue'
import TagList from '@/components/public/TagList.vue'
import CaseBlocks from '@/components/public/blocks/CaseBlocks.vue'
import { usePublicContext } from '@/components/public/publicContext'

const { username, profile } = usePublicContext()
const route = useRoute()
const slug = computed(() => String(route.params.slug))

const { data, loading, error, notFound, reload } = useAsyncData(
  async () => {
    const [work, cards] = await Promise.all([
      publicApi.portfolio(username.value, slug.value),
      publicApi.portfolios(username.value),
    ])
    return { work, neighbours: neighboursOf(cards, work.slug) }
  },
  { watch: [username, slug] },
)

const work = computed(() => data.value!.work)

/** 依作品排序的上一件與下一件（頭尾相接）；只有兩件時兩者相同，只顯示下一件。 */
function neighboursOf(cards: { slug: string; title: string }[], current: string) {
  const i = cards.findIndex((c) => c.slug === current)
  if (i < 0 || cards.length < 2) return null
  const previous = cards[(i - 1 + cards.length) % cards.length]!
  const next = cards[(i + 1) % cards.length]!
  const list: { slug: string; title: string; direction: 'previous' | 'next' }[] = [
    { ...next, direction: 'next' },
  ]
  if (previous.slug !== next.slug) list.unshift({ ...previous, direction: 'previous' })
  return list
}

/** 角色與期間是固定欄位，接著是使用者自訂的欄位；空值不顯示。 */
const facts = computed(() =>
  [
    { label: '角色', value: work.value.role },
    { label: '期間', value: work.value.period },
    ...work.value.fields,
  ].filter((f) => f.value),
)

/** lightbox 依畫面上的順序列出所有圖片，與圖說的編號一致。 */
const images = computed(() =>
  work.value.blocks.flatMap((b) => (b.type === 'image' ? [b.image] : b.type === 'gallery' ? b.items : [])),
)

const lightboxIndex = ref<number | null>(null)
watch(slug, () => (lightboxIndex.value = null))

watchEffect(() => {
  if (!data.value) return
  const owner = profile.value?.fullName || username.value
  setPageSeo({
    title: `${data.value.work.title} · ${owner}`,
    description: data.value.work.summary,
    ogImage: data.value.work.covers[0]?.url,
    ogType: 'article',
  })
})
</script>
