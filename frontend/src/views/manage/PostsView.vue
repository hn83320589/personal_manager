<template>
  <AdminPage title="文章" description="點文章進入編輯，修改會自動儲存。">
    <template #actions>
      <form class="flex gap-2" @submit.prevent="create">
        <label for="new-post-title" class="sr-only">新文章標題</label>
        <input
          id="new-post-title"
          v-model.trim="newTitle"
          class="input py-1.5 text-sm"
          placeholder="新文章的標題"
          maxlength="200"
          required
        />
        <button type="submit" class="btn btn-primary btn-small shrink-0" :disabled="creating">
          寫新文章
        </button>
      </form>
    </template>

    <div class="flex flex-wrap items-center gap-3">
      <SegmentedControl
        v-model="filter"
        :options="filters"
        label="依狀態篩選"
        class="w-full max-w-sm"
      />
      <label for="post-search" class="sr-only">搜尋標題</label>
      <input
        id="post-search"
        v-model="search"
        type="search"
        class="input max-w-60 py-1.5 text-sm"
        placeholder="搜尋標題"
      />
    </div>

    <PageState :loading="loading && !result" :error="error" @retry="reload">
      <template v-if="result">
        <p
          v-if="!result.items.length"
          class="rounded-xl border border-rule bg-surface p-8 text-center text-muted"
        >
          {{
            query || filter !== 'all'
              ? '沒有符合的文章。'
              : '還沒有文章。在上方輸入標題即可開始寫。'
          }}
        </p>
        <ul v-else class="flex flex-col overflow-hidden rounded-xl border border-rule bg-surface">
          <li
            v-for="post in result.items"
            :key="post.id"
            class="flex flex-wrap items-center gap-x-3 gap-y-1 border-rule px-4 py-3 [&+&]:border-t"
          >
            <RouterLink :to="`/admin/blog/${post.id}`" class="min-w-0 flex-1 no-underline">
              <span class="block truncate font-medium hover:text-accent">{{ post.title }}</span>
              <span class="flex flex-wrap gap-x-2 text-xs text-muted">
                <span :class="['rounded px-1.5', badge(post).class]">{{ badge(post).label }}</span>
                <span v-if="post.publishedAt">{{ formatDate(post.publishedAt) }}</span>
                <span v-if="post.category">{{ post.category }}</span>
                <span>{{ post.viewCount }} 次瀏覽</span>
              </span>
            </RouterLink>
            <DeleteButton :item-name="post.title" @confirm="remove(post.id)" />
          </li>
        </ul>
        <PaginationNav
          v-model:page="page"
          :result="result"
          prev-label="← 上一頁"
          next-label="下一頁 →"
          small
        />
      </template>
    </PageState>
  </AdminPage>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import { postsApi } from '@/api/posts'
import type { Schemas } from '@/api/types'
import { useAsyncAction } from '@/composables/useAsyncAction'
import { useAsyncData } from '@/composables/useAsyncData'
import { formatDate } from '@/lib/format'
import AdminPage from '@/components/manage/AdminPage.vue'
import DeleteButton from '@/components/manage/DeleteButton.vue'
import SegmentedControl from '@/components/manage/SegmentedControl.vue'
import PageState from '@/components/public/PageState.vue'
import PaginationNav from '@/components/public/PaginationNav.vue'

type Summary = Schemas['MyPostSummaryDto']
type Filter = 'all' | 'Draft' | 'Published'

const filters: { value: Filter; label: string }[] = [
  { value: 'all', label: '全部' },
  { value: 'Draft', label: '草稿' },
  { value: 'Published', label: '已發佈與排程' },
]

const router = useRouter()
const filter = ref<Filter>('all')
const search = ref('')
const query = ref('')
const page = ref(1)

let debounce: ReturnType<typeof setTimeout> | undefined
watch(search, (value) => {
  clearTimeout(debounce)
  debounce = setTimeout(() => {
    query.value = value.trim()
    page.value = 1
  }, 300)
})
watch(filter, () => (page.value = 1))

const {
  data: result,
  loading,
  error,
  reload,
} = useAsyncData(
  () =>
    postsApi.list({
      q: query.value || undefined,
      status: filter.value === 'all' ? undefined : filter.value,
      page: page.value,
      pageSize: 20,
    }),
  { watch: [query, filter, page] },
)

function badge(post: Summary) {
  if (post.status !== 'Published') return { label: '草稿', class: 'bg-soft' }
  if (post.publishedAt && new Date(post.publishedAt) > new Date())
    return { label: '排程', class: 'bg-accent/10 text-accent' }
  return { label: '已發佈', class: 'bg-success/10 text-success' }
}

const { run, running: creating } = useAsyncAction()
const newTitle = ref('')

async function create() {
  const created = await run(() =>
    postsApi.create({ title: newTitle.value, status: 'Draft', content: '' }),
  )
  if (created) router.push(`/admin/blog/${created.id}`)
}

async function remove(id: number) {
  if (await run(async () => (await postsApi.remove(id), true), { success: '已刪除文章' }))
    await reload()
}
</script>
