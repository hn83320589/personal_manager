<template>
  <PageHead eyebrow="Works" title="作品">每件作品都附上背景、負責的部分與成果。</PageHead>

  <PageState :loading="loading" :error="error" @retry="reload">
    <template v-if="works && profile">
      <FilterChips v-model="category" :options="categories" label="依分類篩選" />
      <p v-if="!works.length" class="mt-9 text-muted">還沒有公開的作品。</p>
      <WorkGrid
        v-else
        class="mt-9"
        :works="visibleWorks"
        :username="username"
        :variant="profile.cardStyle"
        :ratio="profile.cardRatio"
      />
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
import WorkGrid from '@/components/public/WorkGrid.vue'
import { usePublicContext } from '@/components/public/publicContext'

const { username, profile } = usePublicContext()
const route = useRoute()
const router = useRouter()

// 作品數量不多，一次載入全部再於前端篩選，切換分類不需等待
const { data: works, loading, error, reload } = useAsyncData(() => publicApi.portfolios(username.value), {
  watch: [username],
})

const categories = computed(() => [...new Set((works.value ?? []).map((w) => w.category).filter(Boolean))])

/** 選擇的分類放在網址（?category=），可以直接分享。 */
const category = computed({
  get: () => (typeof route.query.category === 'string' ? route.query.category : null),
  set: (value) => router.replace({ query: { ...route.query, category: value ?? undefined } }),
})

const visibleWorks = computed(() =>
  (works.value ?? []).filter((w) => !category.value || w.category === category.value),
)

watchEffect(() => {
  if (profile.value) setPageSeo({ title: `作品 · ${profile.value.fullName || profile.value.username}` })
})
</script>
