<template>
  <PageHead eyebrow="Guestbook" title="留言板">打聲招呼、給點回饋，或是分享你的想法。</PageHead>

  <div class="mt-10 grid gap-12 min-[901px]:grid-cols-[minmax(0,1fr)_22rem] min-[901px]:items-start">
    <PageState :loading="loading" :error="error" @retry="reload">
      <template v-if="result">
        <p v-if="!result.items.length" class="text-muted">還沒有留言，成為第一個留言的人吧。</p>
        <ol v-else class="flex flex-col">
          <li v-for="entry in result.items" :key="entry.id" class="border-b border-rule py-6 first:pt-0">
            <div class="flex items-baseline justify-between gap-3">
              <b class="font-medium">{{ entry.name }}</b>
              <span class="font-mono text-xs text-muted">{{ formatDate(entry.createdAt) }}</span>
            </div>
            <p class="mt-2 whitespace-pre-line">{{ entry.message }}</p>
            <div v-if="entry.reply" class="mt-4 rounded-lg border-l-2 border-accent bg-accent/5 px-4 py-3">
              <span class="text-xs font-medium text-accent">{{ ownerName }} 的回覆</span>
              <p class="mt-1 whitespace-pre-line text-[0.95rem]">{{ entry.reply }}</p>
            </div>
          </li>
        </ol>
        <nav v-if="result.totalPages > 1" class="mt-8 flex items-center justify-between gap-4" aria-label="分頁">
          <button type="button" class="btn" :disabled="!result.hasPreviousPage" @click="page--">← 較新</button>
          <span class="font-mono text-sm text-muted">{{ result.page }} / {{ result.totalPages }}</span>
          <button type="button" class="btn" :disabled="!result.hasNextPage" @click="page++">較舊 →</button>
        </nav>
      </template>
    </PageState>

    <GuestbookForm :username="username" class="min-[901px]:sticky min-[901px]:top-24" />
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watchEffect } from 'vue'
import { publicApi } from '@/api/public'
import { useAsyncData } from '@/composables/useAsyncData'
import { setPageSeo } from '@/composables/useSeo'
import { formatDate } from '@/lib/format'
import GuestbookForm from '@/components/public/GuestbookForm.vue'
import PageHead from '@/components/public/PageHead.vue'
import PageState from '@/components/public/PageState.vue'
import { usePublicContext } from '@/components/public/publicContext'

const { username, profile } = usePublicContext()
const page = ref(1)
const ownerName = computed(() => profile.value?.fullName || username.value)

const {
  data: result,
  loading,
  error,
  reload,
} = useAsyncData(() => publicApi.guestbook(username.value, { page: page.value, pageSize: 10 }), {
  watch: [username, page],
})

watchEffect(() => setPageSeo({ title: `留言板 · ${ownerName.value}` }))
</script>
