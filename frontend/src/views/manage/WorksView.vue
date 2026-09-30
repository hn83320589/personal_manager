<template>
  <AdminPage
    title="作品"
    description="清單順序就是公開頁面的顯示順序。點作品進入編輯，修改會自動儲存。"
  >
    <template #actions>
      <form class="flex gap-2" @submit.prevent="create">
        <label for="new-work-title" class="sr-only">新作品標題</label>
        <input
          id="new-work-title"
          v-model.trim="newTitle"
          class="input py-1.5 text-sm"
          placeholder="新作品的標題"
          maxlength="200"
          required
        />
        <button type="submit" class="btn btn-primary btn-small shrink-0" :disabled="creating">
          新增作品
        </button>
      </form>
    </template>

    <PageState :loading="loading" :error="error" @retry="reload">
      <p
        v-if="!items.length"
        class="rounded-xl border border-rule bg-surface p-8 text-center text-muted"
      >
        還沒有作品。在上方輸入標題即可新增。
      </p>
      <ManageList
        v-else
        :items="items"
        :label-of="(w) => w.title"
        @edit="edit"
        @move="move"
        @remove="remove"
      >
        <template #default="{ item }">
          <RouterLink :to="`/admin/works/${item.id}`" class="flex items-center gap-3 no-underline">
            <span
              class="grid h-12 w-16 shrink-0 place-items-center overflow-hidden rounded-md border border-rule bg-soft text-xs text-muted"
            >
              <img
                v-if="item.coverUrl"
                :src="item.coverUrl"
                alt=""
                class="h-full w-full object-cover"
              />
              <span v-else>無封面</span>
            </span>
            <span class="min-w-0">
              <span class="block truncate font-medium hover:text-accent">{{ item.title }}</span>
              <span class="flex flex-wrap gap-x-2 text-xs text-muted">
                <span>{{
                  [item.category, item.year].filter(Boolean).join(' · ') || '未分類'
                }}</span>
                <span v-if="item.isFeatured" class="text-accent">精選</span>
                <span v-if="!item.isPublic" class="rounded bg-soft px-1.5">不公開</span>
                <span>更新於 {{ formatDate(item.updatedAt) }}</span>
              </span>
            </span>
          </RouterLink>
        </template>
      </ManageList>
    </PageState>
  </AdminPage>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { portfoliosApi } from '@/api/portfolios'
import type { OwnedCollectionApi } from '@/api/collections'
import type { Schemas } from '@/api/types'
import { useAsyncAction } from '@/composables/useAsyncAction'
import { useOwnedList } from '@/composables/useOwnedList'
import { formatDate } from '@/lib/format'
import AdminPage from '@/components/manage/AdminPage.vue'
import ManageList from '@/components/manage/ManageList.vue'
import PageState from '@/components/public/PageState.vue'

type Summary = Schemas['PortfolioSummaryDto']

// 作品在編輯器中修改；清單只需要列表、刪除與排序
const listApi: OwnedCollectionApi<Summary, never> = {
  list: portfoliosApi.list,
  create: () => Promise.reject(new Error('請從編輯器建立作品')),
  update: () => Promise.reject(new Error('請從編輯器修改作品')),
  remove: portfoliosApi.remove,
  reorder: portfoliosApi.reorder,
}

const router = useRouter()
const { items, loading, error, reload, remove, move } = useOwnedList(listApi, {
  removed: '已刪除作品',
})
const { run, running: creating } = useAsyncAction()
const newTitle = ref('')

/** 只需要標題就能建立（預設不公開），建立後直接進入編輯器補完內容。 */
async function create() {
  const created = await run(() =>
    portfoliosApi.create({ title: newTitle.value, isFeatured: false, isPublic: false }),
  )
  if (created) router.push(`/admin/works/${created.id}`)
}

function edit(work: Summary) {
  router.push(`/admin/works/${work.id}`)
}
</script>
