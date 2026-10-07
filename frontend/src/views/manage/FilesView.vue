<template>
  <AdminPage title="檔案" description="上傳的圖片與文件。作品與文章中上傳的檔案也會出現在這裡。">
    <div class="grid gap-3 sm:grid-cols-2">
      <UploadDropzone
        compact
        :accept="imageAccept"
        label="＋ 上傳圖片"
        :hint="imageHint"
        @uploaded="reload"
      />
      <UploadDropzone
        compact
        :accept="documentAccept"
        label="＋ 上傳文件"
        :hint="documentHint"
        @uploaded="reload"
      />
    </div>

    <SegmentedControl v-model="filter" :options="filters" label="依類型篩選" class="max-w-xs" />

    <PageState :loading="loading && !result" :error="error" @retry="reload">
      <template v-if="result">
        <p
          v-if="!visible.length"
          class="rounded-xl border border-rule bg-surface p-8 text-center text-muted"
        >
          沒有檔案。
        </p>
        <div v-else class="grid grid-cols-[repeat(auto-fill,minmax(11rem,1fr))] gap-3">
          <FileCard v-for="file in visible" :key="file.id" :file="file" @remove="remove(file.id)" />
        </div>
        <PaginationNav
          v-model:page="page"
          :result="result"
          prev-label="← 較新"
          next-label="較舊 →"
          small
        />
      </template>
    </PageState>
  </AdminPage>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { filesApi } from '@/api/files'
import { useAsyncAction } from '@/composables/useAsyncAction'
import { useAsyncData } from '@/composables/useAsyncData'
import { documentAccept, documentHint, imageAccept, imageHint } from '@/lib/fileTypes'
import AdminPage from '@/components/manage/AdminPage.vue'
import FileCard from '@/components/manage/FileCard.vue'
import SegmentedControl from '@/components/manage/SegmentedControl.vue'
import UploadDropzone from '@/components/manage/UploadDropzone.vue'
import PageState from '@/components/public/PageState.vue'
import PaginationNav from '@/components/public/PaginationNav.vue'

type Filter = 'all' | 'images' | 'documents'

const filters: { value: Filter; label: string }[] = [
  { value: 'all', label: '全部' },
  { value: 'images', label: '圖片' },
  { value: 'documents', label: '文件' },
]

const filter = ref<Filter>('all')
const page = ref(1)
watch(filter, () => (page.value = 1))

const {
  data: result,
  loading,
  error,
  reload,
} = useAsyncData(
  () =>
    filesApi.list({
      kind: filter.value === 'images' ? 'Image' : undefined,
      page: page.value,
      pageSize: 48,
    }),
  { watch: [filter, page] },
)

/** 文件有多種類型，後端只能依單一類型篩選，因此「文件」在前端排除圖片。 */
const visible = computed(() =>
  (result.value?.items ?? []).filter((f) => filter.value !== 'documents' || f.kind !== 'Image'),
)

const { run } = useAsyncAction()

async function remove(id: number) {
  if (await run(async () => (await filesApi.remove(id), true), { success: '已刪除檔案' }))
    await reload()
}
</script>
