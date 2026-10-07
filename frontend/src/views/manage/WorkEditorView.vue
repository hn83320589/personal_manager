<template>
  <PageState
    :loading="loading"
    :error="loadError"
    :not-found="notFound"
    not-found-text="找不到這件作品"
    @retry="reload"
  >
    <div
      v-if="work"
      class="mx-auto grid max-w-[80rem] items-start gap-7 min-[901px]:grid-cols-[20rem_minmax(0,1fr)]"
    >
      <div class="col-span-full flex flex-wrap items-center justify-between gap-3">
        <div>
          <RouterLink to="/admin/works" class="text-sm text-muted hover:text-ink"
            >← 所有作品</RouterLink
          >
          <h1 class="mt-1 text-xl font-bold">編輯「{{ work.title || '未命名作品' }}」</h1>
        </div>
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
            v-if="auth.user"
            :to="{ name: 'public-work', params: { username: auth.user.username, slug: work.slug } }"
            target="_blank"
            class="btn btn-small"
          >
            預覽 ↗
          </RouterLink>
          <button
            type="button"
            :class="['btn btn-small', work.isPublic ? '' : 'btn-primary']"
            @click="work.isPublic = !work.isPublic"
          >
            {{ work.isPublic ? '改為不公開' : '公開這件作品' }}
          </button>
        </div>
      </div>

      <WorkInfoPanel
        v-model="work"
        :username="auth.user?.username ?? ''"
        :mode="profile?.portfolioMode ?? 'Designer'"
        :categories="categories"
        :tag-suggestions="tags ?? []"
        class="min-[901px]:sticky min-[901px]:top-8 min-[901px]:max-h-[calc(100vh-4rem)] min-[901px]:overflow-y-auto"
      />

      <section aria-label="內容區塊">
        <BlockList v-model="work.blocks" />
      </section>
    </div>
  </PageState>
</template>

<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted } from 'vue'
import { onBeforeRouteLeave, useRoute } from 'vue-router'
import { portfoliosApi, tagsApi } from '@/api/portfolios'
import { profileApi } from '@/api/profile'
import { useAsyncData } from '@/composables/useAsyncData'
import { setPageSeo } from '@/composables/useSeo'
import { useWorkEditor } from '@/composables/useWorkEditor'
import { formatTime } from '@/lib/format'
import { useAuthStore } from '@/stores/auth'
import BlockList from '@/components/manage/works/BlockList.vue'
import WorkInfoPanel from '@/components/manage/works/WorkInfoPanel.vue'
import PageState from '@/components/public/PageState.vue'

const route = useRoute()
const auth = useAuthStore()
const id = computed(() => Number(route.params.id))

const { work, loading, loadError, notFound, reload, status, error, savedAt, dirty, flush } =
  useWorkEditor(id)
const { data: profile } = useAsyncData(profileApi.get)
const { data: tags } = useAsyncData(tagsApi.mine)
const { data: others } = useAsyncData(portfoliosApi.list)
const categories = computed(() => [
  ...new Set((others.value ?? []).map((w) => w.category).filter(Boolean)),
])

const statusText = computed(() => {
  if (status.value === 'saving') return '儲存中…'
  if (status.value === 'error') return `儲存失敗：${error.value}`
  if (dirty.value) return '有尚未儲存的修改'
  if (savedAt.value) return `已自動儲存 ${formatTime(savedAt.value)}`
  return '修改會自動儲存'
})

// 離開頁面前先存；存不起來就提醒，避免遺失修改
onBeforeRouteLeave(async () => {
  await flush()
  return !dirty.value || window.confirm('有修改尚未儲存成功，確定要離開嗎？')
})

function warnBeforeUnload(event: BeforeUnloadEvent) {
  if (dirty.value) event.preventDefault()
}
onMounted(() => {
  window.addEventListener('beforeunload', warnBeforeUnload)
  setPageSeo({ title: '編輯作品' })
})
onBeforeUnmount(() => window.removeEventListener('beforeunload', warnBeforeUnload))
</script>
