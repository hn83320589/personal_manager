<template>
  <AdminPage
    title="使用者管理"
    description="停用的帳號無法登入，公開頁面也會隱藏。你不能停用自己或取消自己的管理員身分。"
  >
    <label for="user-search" class="sr-only">搜尋帳號、姓名或 Email</label>
    <input
      id="user-search"
      v-model="search"
      type="search"
      class="input max-w-sm"
      placeholder="搜尋帳號、姓名或 Email"
    />

    <PageState :loading="loading && !result" :error="error" @retry="reload">
      <template v-if="result">
        <p
          v-if="!result.items.length"
          class="rounded-xl border border-rule bg-surface p-8 text-center text-muted"
        >
          找不到使用者。
        </p>
        <ul v-else class="flex flex-col overflow-hidden rounded-xl border border-rule bg-surface">
          <li
            v-for="user in result.items"
            :key="user.id"
            class="flex flex-wrap items-center gap-x-3 gap-y-2 border-rule px-4 py-3 [&+&]:border-t"
          >
            <div class="min-w-0 flex-1">
              <p class="truncate font-medium">
                {{ user.fullName || user.username }}
                <span class="font-normal text-muted">@{{ user.username }}</span>
                <span
                  v-if="!user.isActive"
                  class="ml-1 rounded bg-danger/10 px-1.5 text-xs text-danger"
                  >已停用</span
                >
              </p>
              <p class="truncate text-xs text-muted">
                {{ user.email }} · 註冊於 {{ formatDate(user.createdAt) }}
              </p>
            </div>
            <template v-if="user.id !== auth.user?.id">
              <label :for="`role-${user.id}`" class="sr-only">{{ user.username }} 的角色</label>
              <select
                :id="`role-${user.id}`"
                class="input w-auto py-1 text-sm"
                :value="user.role"
                :disabled="running"
                @change="setRole(user, ($event.target as HTMLSelectElement).value)"
              >
                <option value="User">一般使用者</option>
                <option value="Admin">管理員</option>
              </select>
              <button
                type="button"
                class="btn btn-small"
                :disabled="running"
                @click="setStatus(user, !user.isActive)"
              >
                {{ user.isActive ? '停用' : '啟用' }}
              </button>
            </template>
            <span v-else class="text-xs text-muted">這是你</span>
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
import { adminUsersApi } from '@/api/admin'
import type { Schemas } from '@/api/types'
import { useAsyncAction } from '@/composables/useAsyncAction'
import { useAsyncData } from '@/composables/useAsyncData'
import { formatDate } from '@/lib/format'
import { useAuthStore } from '@/stores/auth'
import AdminPage from '@/components/manage/AdminPage.vue'
import PageState from '@/components/public/PageState.vue'
import PaginationNav from '@/components/public/PaginationNav.vue'

type User = Schemas['AdminUserDto']

const auth = useAuthStore()
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

const {
  data: result,
  loading,
  error,
  reload,
} = useAsyncData(
  () => adminUsersApi.list({ q: query.value || undefined, page: page.value, pageSize: 30 }),
  {
    watch: [query, page],
  },
)
const { run, running } = useAsyncAction()

async function setStatus(user: User, isActive: boolean) {
  const ok = await run(async () => (await adminUsersApi.setStatus(user.id, isActive), true), {
    success: isActive ? `已啟用 ${user.username}` : `已停用 ${user.username}`,
  })
  if (ok) await reload()
}

async function setRole(user: User, role: string) {
  await run(async () => (await adminUsersApi.setRole(user.id, role), true), {
    success: `已更新 ${user.username} 的角色`,
  })
  await reload() // 失敗時也重新載入，讓下拉選單回到實際的角色
}
</script>
