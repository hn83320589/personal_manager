<template>
  <div class="min-h-screen px-4 sm:px-6 lg:px-10">
    <header class="mx-auto flex min-h-[60px] max-w-wrap items-center justify-between gap-4 border-b border-rule">
      <span class="font-hand text-xl font-bold">Personal Manager</span>
      <div class="flex items-center gap-2">
        <RouterLink :to="auth.isAuthenticated ? '/admin' : '/login'" class="btn btn-small">
          {{ auth.isAuthenticated ? '管理後台' : '登入' }}
        </RouterLink>
        <ColorSchemeToggle />
      </div>
    </header>

    <main class="mx-auto max-w-wrap pb-24">
      <div class="flex max-w-[40rem] flex-col gap-4 pt-16">
        <h1 class="font-hand text-[clamp(2.2rem,5vw,3.2rem)] font-bold leading-tight">找到想認識的人</h1>
        <p class="text-muted">每個人都有自己的作品、文章與經歷頁面。</p>
        <label for="directory-search" class="sr-only">搜尋姓名、職稱或使用者名稱</label>
        <input
          id="directory-search"
          v-model="search"
          type="search"
          class="input mt-2 max-w-md"
          placeholder="搜尋姓名、職稱或使用者名稱"
        />
      </div>

      <PageState :loading="loading && !people.length" :error="error" @retry="reload">
        <p v-if="!people.length" class="mt-12 text-muted">
          {{ query ? `找不到符合「${query}」的人。` : '目前還沒有公開的個人頁面。' }}
        </p>
        <ul v-else class="mt-12 grid gap-5 min-[641px]:grid-cols-2 min-[1001px]:grid-cols-3">
          <li
            v-for="person in people"
            :key="person.username"
            class="group relative flex gap-4 rounded-card border border-rule bg-surface p-5 hover:border-ink"
          >
            <div
              class="grid h-14 w-14 shrink-0 place-items-center overflow-hidden rounded-2xl bg-soft font-hand text-2xl text-muted"
            >
              <img v-if="person.profileImageUrl" :src="person.profileImageUrl" alt="" class="h-full w-full object-cover" />
              <span v-else aria-hidden="true">{{ (person.fullName || person.username).charAt(0).toUpperCase() }}</span>
            </div>
            <div class="flex min-w-0 flex-col gap-1">
              <h2 class="font-bold group-hover:text-accent">
                <RouterLink :to="{ name: 'public-home', params: { username: person.username } }" class="stretched-link">
                  {{ person.fullName || person.username }}
                </RouterLink>
              </h2>
              <span v-if="person.title" class="text-sm text-muted">{{ person.title }}</span>
              <p v-if="person.summary" class="line-clamp-2 text-sm text-muted">{{ person.summary }}</p>
              <span v-if="person.location" class="mt-1 font-mono text-xs text-muted">{{ person.location }}</span>
            </div>
          </li>
        </ul>
        <div v-if="hasMore" class="mt-10 text-center">
          <button type="button" class="btn" :disabled="loading" @click="loadMore">
            {{ loading ? '載入中…' : '載入更多' }}
          </button>
        </div>
      </PageState>
    </main>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref, watch } from 'vue'
import { publicApi } from '@/api/public'
import type { Schemas } from '@/api/types'
import { ApiError } from '@/api/http'
import { setPageSeo } from '@/composables/useSeo'
import { useAuthStore } from '@/stores/auth'
import ColorSchemeToggle from '@/components/public/ColorSchemeToggle.vue'
import PageState from '@/components/public/PageState.vue'

const pageSize = 24

const auth = useAuthStore()
const search = ref('')
const query = ref('')
const people = ref<Schemas['DirectoryCardDto'][]>([])
const page = ref(1)
const hasMore = ref(false)
const loading = ref(false)
const error = ref<string | null>(null)
let latest = 0

async function load(reset: boolean) {
  const request = ++latest
  loading.value = true
  error.value = null
  try {
    const nextPage = reset ? 1 : page.value + 1
    const result = await publicApi.directory({ q: query.value || undefined, page: nextPage, pageSize })
    if (request !== latest) return
    people.value = reset ? result.items : [...people.value, ...result.items]
    page.value = nextPage
    hasMore.value = result.hasNextPage
  } catch (e) {
    if (request === latest) error.value = e instanceof ApiError ? e.message : '載入失敗，請稍後再試'
  } finally {
    if (request === latest) loading.value = false
  }
}

const reload = () => load(true)
const loadMore = () => load(false)

// 輸入停頓 300ms 後才查詢，避免每打一個字就送出請求
let debounce: ReturnType<typeof setTimeout> | undefined
watch(search, (value) => {
  clearTimeout(debounce)
  debounce = setTimeout(() => {
    query.value = value.trim()
    void load(true)
  }, 300)
})

onMounted(() => {
  setPageSeo({ title: '探索個人頁面' })
  void load(true)
})
</script>
