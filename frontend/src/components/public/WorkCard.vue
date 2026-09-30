<template>
  <article
    v-if="variant === 'Tech'"
    :class="[
      'group relative grid items-start gap-6 border-b border-rule px-1 py-[26px]',
      thumbnail ? 'sm:grid-cols-[minmax(0,1fr)_13rem]' : '',
    ]"
  >
    <div class="flex min-w-0 flex-col gap-2.5">
      <span class="font-mono text-[0.74rem] uppercase tracking-[0.08em] text-muted">
        {{ [work.category, work.year].filter(Boolean).join(' · ') }}
      </span>
      <h3 class="text-xl font-bold group-hover:text-accent">
        <RouterLink :to="detail" class="stretched-link">
          {{ work.title }}
        </RouterLink>
      </h3>
      <p v-if="work.summary" class="max-w-[44rem] text-[0.95rem] text-muted">{{ work.summary }}</p>
      <MetricList v-if="work.metrics.length" :metrics="work.metrics" />
      <TagList :tags="work.tags" />
    </div>
    <div
      v-if="thumbnail"
      class="order-first aspect-[4/3] overflow-hidden rounded-lg border border-rule bg-soft sm:order-none"
    >
      <img
        :src="thumbnail.url"
        :alt="`${work.title} 截圖`"
        :style="{ objectPosition: work.coverFocus }"
        class="h-full w-full object-cover"
        loading="lazy"
      />
    </div>
  </article>

  <!-- 圖像型與資訊型：封面在上 -->
  <article v-else :class="['group relative flex flex-col gap-3.5', spanFull ? 'min-[721px]:col-span-full' : '']">
    <div
      :class="['relative overflow-hidden border border-rule bg-soft', variant === 'Visual' ? 'rounded-md' : 'rounded-card']"
      :style="{ aspectRatio: coverRatio }"
    >
      <CoverCarousel v-if="work.covers.length" :images="work.covers" :title="work.title" :focus="work.coverFocus" />
      <TextCover v-else :title="work.title" :category="work.category" />
    </div>
    <div class="flex flex-col gap-1.5">
      <div class="flex items-baseline justify-between gap-3">
        <h3 :class="['group-hover:text-accent', variant === 'Visual' ? 'text-base font-medium' : 'text-lg font-bold']">
          <RouterLink :to="detail" class="stretched-link after:z-[1]">
            {{ work.title }}
          </RouterLink>
        </h3>
        <span v-if="work.year" class="whitespace-nowrap font-mono text-[0.8rem] tabular-nums text-muted">
          {{ work.year }}
        </span>
      </div>
      <span v-if="variant === 'Visual' && work.category" class="text-[0.83rem] text-muted">{{ work.category }}</span>
      <template v-if="variant === 'Info'">
        <p v-if="work.summary" class="text-[0.95rem] text-muted">{{ work.summary }}</p>
        <TagList :tags="work.tags" />
      </template>
    </div>
  </article>
</template>

<script setup lang="ts">
// 技術型：文字與重點數字為主，截圖為選用的縮圖
import { computed } from 'vue'
import type { Schemas } from '@/api/types'
import CoverCarousel from './CoverCarousel.vue'
import MetricList from './MetricList.vue'
import TagList from './TagList.vue'
import TextCover from './TextCover.vue'
import { ratioValue, type CardRatio, type CardStyle } from './workCards'

const props = defineProps<{
  work: Schemas['PortfolioCardDto']
  username: string
  variant: CardStyle
  ratio: CardRatio
}>()

const detail = computed(() => ({
  name: 'public-work',
  params: { username: props.username, slug: props.work.slug },
}))
const thumbnail = computed(() => props.work.covers[0])
/** 資訊型的精選作品橫跨整列，用較寬的封面。 */
const spanFull = computed(() => props.variant === 'Info' && props.work.isFeatured)
const coverRatio = computed(() =>
  props.variant === 'Visual' ? ratioValue[props.ratio] : spanFull.value ? '21 / 9' : '4 / 3',
)
</script>
