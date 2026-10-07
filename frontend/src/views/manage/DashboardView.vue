<template>
  <AdminPage :title="`你好，${auth.userDisplayName}`" description="需要你處理的事情與常用的操作。">
    <template #actions>
      <RouterLink
        v-if="auth.user"
        :to="{ name: 'public-home', params: { username: auth.user.username } }"
        target="_blank"
        class="btn btn-small"
      >
        查看公開頁面 ↗
      </RouterLink>
    </template>

    <PageState :loading="loading" :error="error" @retry="reload">
      <template v-if="data">
        <div class="grid grid-cols-2 gap-3 min-[721px]:grid-cols-4">
          <RouterLink
            v-for="stat in stats"
            :key="stat.label"
            :to="stat.to"
            class="flex flex-col gap-1 rounded-xl border border-rule bg-surface p-4 no-underline hover:border-ink"
          >
            <span class="text-sm text-muted">{{ stat.label }}</span>
            <b :class="['font-latin text-2xl tabular-nums', stat.attention ? 'text-accent' : '']">{{
              stat.value
            }}</b>
          </RouterLink>
        </div>

        <div class="grid gap-6 min-[861px]:grid-cols-2">
          <section class="flex flex-col gap-2" aria-labelledby="upcoming">
            <h2 id="upcoming" class="text-sm font-medium text-muted">接下來 7 天的行程</h2>
            <p
              v-if="!data.events.length"
              class="rounded-xl border border-rule bg-surface p-4 text-sm text-muted"
            >
              沒有行程。
            </p>
            <ul
              v-else
              class="flex flex-col overflow-hidden rounded-xl border border-rule bg-surface"
            >
              <li
                v-for="event in data.events"
                :key="`${event.eventId}-${event.start}`"
                class="flex items-center gap-3 border-rule px-4 py-2.5 text-sm [&+&]:border-t"
              >
                <i
                  class="h-2 w-2 shrink-0 rounded-full"
                  :style="{ background: event.color || 'rgb(var(--accent))' }"
                />
                <span class="min-w-0 flex-1 truncate">{{ event.title }}</span>
                <span class="font-mono text-xs text-muted">{{ when(event) }}</span>
              </li>
            </ul>
          </section>

          <section class="flex flex-col gap-2" aria-labelledby="quick">
            <h2 id="quick" class="text-sm font-medium text-muted">快速開始</h2>
            <div class="grid grid-cols-2 gap-2">
              <RouterLink
                v-for="action in actions"
                :key="action.label"
                :to="action.to"
                class="btn justify-center"
              >
                {{ action.label }}
              </RouterLink>
            </div>
          </section>
        </div>
      </template>
    </PageState>
  </AdminPage>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { guestbookApi } from '@/api/guestbook'
import { postsApi } from '@/api/posts'
import { calendarApi, timeEntriesApi, todosApi } from '@/api/tools'
import type { Schemas } from '@/api/types'
import { useAsyncData } from '@/composables/useAsyncData'
import { formatTime, localDate } from '@/lib/format'
import { formatMinutes } from '@/lib/timer'
import { useAuthStore } from '@/stores/auth'
import AdminPage from '@/components/manage/AdminPage.vue'
import PageState from '@/components/public/PageState.vue'

const auth = useAuthStore()

const { data, loading, error, reload } = useAsyncData(async () => {
  const now = new Date()
  const weekAgo = new Date(now.getFullYear(), now.getMonth(), now.getDate() - 6)
  const inAWeek = new Date(now.getTime() + 7 * 24 * 60 * 60 * 1000)
  const [pending, drafts, todos, time, events] = await Promise.all([
    guestbookApi.list({ status: 'Pending', pageSize: 1 }),
    postsApi.list({ status: 'Draft', pageSize: 1 }),
    todosApi.list(),
    timeEntriesApi.summary({ from: localDate(weekAgo), to: localDate(now) }),
    calendarApi.occurrences({ from: now.toISOString(), to: inAWeek.toISOString() }),
  ])
  return {
    pendingMessages: pending.totalCount,
    drafts: drafts.totalCount,
    openTodos: todos.filter((t) => t.status !== 'Completed').length,
    weekMinutes: time.totalMinutes,
    events: events.slice(0, 6),
  }
})

const stats = computed(() => {
  const d = data.value!
  return [
    {
      label: '待審核留言',
      value: d.pendingMessages,
      to: '/admin/comments',
      attention: d.pendingMessages > 0,
    },
    { label: '草稿文章', value: d.drafts, to: '/admin/blog', attention: false },
    { label: '未完成待辦', value: d.openTodos, to: '/admin/tasks', attention: false },
    {
      label: '最近 7 天工時',
      value: formatMinutes(d.weekMinutes),
      to: '/admin/work-tracking',
      attention: false,
    },
  ]
})

const actions = [
  { label: '新增作品', to: '/admin/works' },
  { label: '寫文章', to: '/admin/blog' },
  { label: '編輯個人資料', to: '/admin/profile' },
  { label: '新增行程', to: '/admin/calendar' },
]

function when(event: Schemas['OccurrenceDto']) {
  const start = new Date(event.start)
  const day = `${start.getMonth() + 1}/${start.getDate()}`
  return event.isAllDay ? `${day} 全天` : `${day} ${formatTime(start)}`
}
</script>
