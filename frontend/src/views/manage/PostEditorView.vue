<template>
  <PageState
    :loading="loading"
    :error="loadError"
    :not-found="notFound"
    not-found-text="找不到這篇文章"
    @retry="reload"
  >
    <div
      v-if="post"
      class="mx-auto grid max-w-[76rem] items-start gap-7 min-[901px]:grid-cols-[minmax(0,1fr)_18rem]"
    >
      <div class="col-span-full flex flex-wrap items-center justify-between gap-3">
        <RouterLink to="/admin/blog" class="text-sm text-muted hover:text-ink"
          >← 所有文章</RouterLink
        >
        <div class="flex flex-wrap items-center gap-2.5">
          <span
            class="text-sm"
            :class="status === 'error' ? 'text-danger' : 'text-muted'"
            role="status"
            >{{ statusText }}</span
          >
          <button v-if="status === 'error'" type="button" class="btn btn-small" @click="flush">
            重試
          </button>
          <RouterLink
            v-if="auth.user && post.mode !== 'draft'"
            :to="{ name: 'public-post', params: { username: auth.user.username, slug: post.slug } }"
            target="_blank"
            class="btn btn-small"
          >
            預覽 ↗
          </RouterLink>
          <button
            v-if="post.mode === 'draft'"
            type="button"
            class="btn btn-primary btn-small"
            @click="publish"
          >
            發佈
          </button>
        </div>
      </div>

      <div class="flex min-w-0 flex-col gap-2.5">
        <label for="post-title" class="sr-only">標題</label>
        <input
          id="post-title"
          v-model="post.title"
          class="w-full border-0 bg-transparent px-0 py-1 font-hand text-[clamp(1.8rem,4vw,2.5rem)] font-bold text-ink placeholder:text-muted/60 focus:outline-none"
          placeholder="文章標題"
          maxlength="200"
        />
        <PostContentEditor v-model="post.content" />
      </div>

      <PostSettingsPanel
        v-model="post"
        :username="auth.user?.username ?? ''"
        :categories="categories"
        :tag-suggestions="tags ?? []"
        class="min-[901px]:sticky min-[901px]:top-8"
      />
    </div>
  </PageState>
</template>

<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted } from 'vue'
import { onBeforeRouteLeave, useRoute } from 'vue-router'
import { postsApi } from '@/api/posts'
import { tagsApi } from '@/api/portfolios'
import { useAsyncData } from '@/composables/useAsyncData'
import { useAutosave } from '@/composables/useAutosave'
import { setPageSeo } from '@/composables/useSeo'
import { toEditablePost, toPostRequest } from '@/lib/postDocument'
import { useAuthStore } from '@/stores/auth'
import PostContentEditor from '@/components/manage/blog/PostContentEditor.vue'
import PostSettingsPanel from '@/components/manage/blog/PostSettingsPanel.vue'
import PageState from '@/components/public/PageState.vue'

const route = useRoute()
const auth = useAuthStore()
const id = computed(() => Number(route.params.id))

const {
  doc: post,
  loading,
  loadError,
  notFound,
  reload,
  status,
  error,
  savedAt,
  dirty,
  flush,
} = useAutosave({
  load: () => postsApi.get(id.value),
  save: (request) => postsApi.update(id.value, request),
  toEditable: (dto) => toEditablePost(dto),
  toRequest: toPostRequest,
  applySaved: (saved, editable) => {
    editable.slug = saved.slug
    editable.publishedAt = saved.publishedAt ?? null
  },
  watch: [id],
})

const { data: tags } = useAsyncData(tagsApi.mine)
const { data: recent } = useAsyncData(() => postsApi.list({ pageSize: 50 }))
const categories = computed(() => [
  ...new Set((recent.value?.items ?? []).map((p) => p.category).filter(Boolean)),
])

const pad = (n: number) => String(n).padStart(2, '0')
const statusText = computed(() => {
  if (status.value === 'saving') return '儲存中…'
  if (status.value === 'error') return `儲存失敗：${error.value}`
  if (dirty.value) return '有尚未儲存的修改'
  if (savedAt.value)
    return `已自動儲存 ${pad(savedAt.value.getHours())}:${pad(savedAt.value.getMinutes())}`
  return '修改會自動儲存'
})

async function publish() {
  if (!post.value) return
  post.value.mode = 'published'
  await flush()
}

onBeforeRouteLeave(async () => {
  await flush()
  return !dirty.value || window.confirm('有修改尚未儲存成功，確定要離開嗎？')
})

function warnBeforeUnload(event: BeforeUnloadEvent) {
  if (dirty.value) event.preventDefault()
}
onMounted(() => {
  window.addEventListener('beforeunload', warnBeforeUnload)
  setPageSeo({ title: '編輯文章' })
})
onBeforeUnmount(() => window.removeEventListener('beforeunload', warnBeforeUnload))
</script>
