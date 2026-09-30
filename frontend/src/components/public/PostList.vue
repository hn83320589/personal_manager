<template>
  <ul class="flex flex-col">
    <li
      v-for="post in posts"
      :key="post.slug"
      :class="[
        'group relative grid gap-6 border-b border-rule py-[26px]',
        post.coverImageUrl ? 'sm:grid-cols-[minmax(0,1fr)_12rem]' : '',
      ]"
    >
      <div class="flex flex-col gap-2">
        <span class="font-mono text-[0.82rem] text-muted">{{ meta(post) }}</span>
        <h3 class="text-xl font-bold group-hover:text-accent">
          <RouterLink :to="{ name: 'public-post', params: { username, slug: post.slug } }" class="stretched-link">
            {{ post.title }}
          </RouterLink>
        </h3>
        <p v-if="post.summary" class="text-muted">{{ post.summary }}</p>
        <TagList :tags="post.tags" />
      </div>
      <div
        v-if="post.coverImageUrl"
        class="order-first aspect-[4/3] overflow-hidden rounded-lg border border-rule bg-soft sm:order-none"
      >
        <img :src="post.coverImageUrl" alt="" class="h-full w-full object-cover" loading="lazy" />
      </div>
    </li>
  </ul>
</template>

<script setup lang="ts">
import type { Schemas } from '@/api/types'
import { formatDate } from '@/lib/format'
import TagList from './TagList.vue'

type Post = Schemas['PublicPostSummaryDto']

defineProps<{ posts: Post[]; username: string }>()

function meta(post: Post) {
  return [formatDate(post.publishedAt), post.category, `${post.readingMinutes} 分鐘`]
    .filter(Boolean)
    .join(' · ')
}
</script>
