<template>
  <PageHead eyebrow="Writing" title="文章">工作筆記、做法與想法。</PageHead>

  <FilterChips v-if="facets" v-model="tag" :options="facets.tags" label="依標籤篩選" />

  <PageState :loading="loading" :error="error" @retry="reload">
    <template v-if="result">
      <p v-if="!result.items.length" class="mt-8 text-muted">
        {{ tag ? `沒有標記「${tag}」的文章。` : '還沒有發佈的文章。' }}
      </p>
      <PostList v-else class="mt-6" :posts="result.items" :username="username" />

      <nav v-if="result.totalPages > 1" class="mt-10 flex items-center justify-between gap-4" aria-label="分頁">
        <button type="button" class="btn" :disabled="!result.hasPreviousPage" @click="page = result.page - 1">
          ← 較新的文章
        </button>
        <span class="font-mono text-sm text-muted">{{ result.page }} / {{ result.totalPages }}</span>
        <button type="button" class="btn" :disabled="!result.hasNextPage" @click="page = result.page + 1">
          較舊的文章 →
        </button>
      </nav>
    </template>
  </PageState>
</template>

<script setup lang="ts">
import { computed, watchEffect } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { publicApi } from '@/api/public'
import { useAsyncData } from '@/composables/useAsyncData'
import { setPageSeo } from '@/composables/useSeo'
import FilterChips from '@/components/public/FilterChips.vue'
import PageHead from '@/components/public/PageHead.vue'
import PageState from '@/components/public/PageState.vue'
import PostList from '@/components/public/PostList.vue'
import { usePublicContext } from '@/components/public/publicContext'

const pageSize = 10

const { username, profile } = usePublicContext()
const route = useRoute()
const router = useRouter()

/** 標籤與頁碼放在網址（?tag=&page=），可以分享與返回。 */
const tag = computed({
  get: () => (typeof route.query.tag === 'string' ? route.query.tag : null),
  set: (value) => router.replace({ query: { tag: value ?? undefined } }),
})
const page = computed({
  get: () => Math.max(1, Number(route.query.page) || 1),
  set: (value) => {
    router.push({ query: { ...route.query, page: value > 1 ? String(value) : undefined } })
    window.scrollTo({ top: 0 })
  },
})

const { data: facets } = useAsyncData(() => publicApi.postFacets(username.value), { watch: [username] })
const {
  data: result,
  loading,
  error,
  reload,
} = useAsyncData(
  () => publicApi.posts(username.value, { tag: tag.value ?? undefined, page: page.value, pageSize }),
  { watch: [username, tag, page] },
)

watchEffect(() => {
  if (profile.value) setPageSeo({ title: `文章 · ${profile.value.fullName || profile.value.username}` })
})
</script>
