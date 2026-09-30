<template>
  <AdminPage
    title="留言"
    description="訪客的留言經你公開後才會顯示在留言板；回覆會顯示在留言下方。"
  >
    <SegmentedControl v-model="filter" :options="filters" label="依狀態篩選" class="max-w-sm" />

    <PageState :loading="loading && !result" :error="error" @retry="reload">
      <template v-if="result">
        <p
          v-if="!result.items.length"
          class="rounded-xl border border-rule bg-surface p-8 text-center text-muted"
        >
          {{ filter === 'Pending' ? '沒有待審核的留言。' : '沒有留言。' }}
        </p>
        <div v-else class="flex flex-col gap-3">
          <GuestbookEntryCard
            v-for="entry in result.items"
            :key="entry.id"
            :entry="entry"
            :busy="running"
            @approve="(value) => setApproval(entry, value)"
            @reply="(text) => reply(entry, text)"
            @remove="remove(entry)"
          />
        </div>
        <nav
          v-if="result.totalPages > 1"
          class="flex items-center justify-between gap-4"
          aria-label="分頁"
        >
          <button
            type="button"
            class="btn btn-small"
            :disabled="!result.hasPreviousPage"
            @click="page--"
          >
            ← 較新
          </button>
          <span class="font-mono text-sm text-muted"
            >{{ result.page }} / {{ result.totalPages }}</span
          >
          <button
            type="button"
            class="btn btn-small"
            :disabled="!result.hasNextPage"
            @click="page++"
          >
            較舊 →
          </button>
        </nav>
      </template>
    </PageState>
  </AdminPage>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { guestbookApi, type GuestbookFilter } from '@/api/guestbook'
import type { Schemas } from '@/api/types'
import { useAsyncAction } from '@/composables/useAsyncAction'
import { useAsyncData } from '@/composables/useAsyncData'
import AdminPage from '@/components/manage/AdminPage.vue'
import GuestbookEntryCard from '@/components/manage/GuestbookEntryCard.vue'
import SegmentedControl from '@/components/manage/SegmentedControl.vue'
import PageState from '@/components/public/PageState.vue'

type Entry = Schemas['GuestbookEntryDto']

const filters: { value: GuestbookFilter; label: string }[] = [
  { value: 'Pending', label: '待審核' },
  { value: 'Approved', label: '已公開' },
  { value: 'All', label: '全部' },
]

const filter = ref<GuestbookFilter>('Pending')
const page = ref(1)
watch(filter, () => (page.value = 1))

const {
  data: result,
  loading,
  error,
  reload,
} = useAsyncData(
  () => guestbookApi.list({ status: filter.value, page: page.value, pageSize: 20 }),
  {
    watch: [filter, page],
  },
)
const { run, running } = useAsyncAction()

/** 資料以 shallowRef 保存，更新單筆時整份替換，畫面才會更新。 */
function patch(id: number, changes: Partial<Entry>) {
  if (!result.value) return
  result.value = {
    ...result.value,
    items: result.value.items.map((e) => (e.id === id ? { ...e, ...changes } : e)),
  }
}

async function setApproval(entry: Entry, isApproved: boolean) {
  const ok = await run(async () => (await guestbookApi.setApproval(entry.id, isApproved), true), {
    success: isApproved ? '已公開留言' : '已隱藏留言',
  })
  if (!ok) return
  // 在「待審核」或「已公開」篩選中，狀態改變的留言不再屬於這個清單
  if (filter.value === 'All') patch(entry.id, { isApproved })
  else await reload()
}

async function reply(entry: Entry, text: string) {
  const ok = await run(async () => (await guestbookApi.reply(entry.id, text.trim()), true), {
    success: text.trim() ? '已儲存回覆' : '已移除回覆',
  })
  if (ok) patch(entry.id, { reply: text.trim() })
}

async function remove(entry: Entry) {
  if (await run(async () => (await guestbookApi.remove(entry.id), true), { success: '已刪除留言' }))
    await reload()
}
</script>
