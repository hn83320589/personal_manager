<template>
  <div class="min-h-screen bg-paper md:grid md:grid-cols-[15rem_minmax(0,1fr)]">
    <!-- 手機：頂列 + 抽屜式選單 -->
    <header
      class="sticky top-0 z-30 flex items-center justify-between border-b border-rule bg-paper/90 px-4 pb-2.5 pt-[calc(env(safe-area-inset-top,0px)+10px)] backdrop-blur md:hidden"
    >
      <span class="font-hand text-lg font-bold">Personal Manager</span>
      <button
        type="button"
        class="grid h-9 w-9 place-items-center rounded-md text-muted hover:bg-soft"
        aria-controls="admin-nav"
        :aria-expanded="menuOpen"
        aria-label="選單"
        @click="menuOpen = !menuOpen"
      >
        <Bars3Icon class="h-5 w-5" aria-hidden="true" />
      </button>
    </header>

    <aside
      id="admin-nav"
      :class="[
        'fixed inset-y-0 left-0 z-40 w-64 flex-col border-r border-rule bg-surface md:sticky md:top-0 md:z-auto md:flex md:h-screen md:w-auto',
        menuOpen ? 'flex' : 'hidden',
      ]"
      aria-label="後台選單"
    >
      <div
        class="flex items-center justify-between px-5 pb-4 pt-[calc(env(safe-area-inset-top,0px)+20px)]"
      >
        <RouterLink to="/admin" class="font-hand text-lg font-bold no-underline"
          >Personal Manager</RouterLink
        >
        <ColorSchemeToggle />
      </div>

      <nav class="flex-1 overflow-y-auto px-3 pb-4">
        <div v-for="group in groups" :key="group.label" class="mt-4 first:mt-0">
          <p class="px-2 pb-1 font-mono text-[0.7rem] uppercase tracking-[0.06em] text-muted">
            {{ group.label }}
          </p>
          <RouterLink
            v-for="item in group.items"
            :key="item.to"
            :to="item.to"
            class="flex items-center gap-2.5 rounded-md px-2 py-1.5 text-sm text-muted no-underline hover:bg-soft hover:text-ink"
            exact-active-class="!bg-accent/10 !text-accent font-medium"
            @click="menuOpen = false"
          >
            <component :is="item.icon" class="h-4 w-4 shrink-0" aria-hidden="true" />
            {{ item.label }}
          </RouterLink>
        </div>
      </nav>

      <div
        class="border-t border-rule px-4 py-3 pb-[calc(env(safe-area-inset-bottom,0px)+12px)] text-sm"
      >
        <p class="truncate font-medium">{{ auth.userDisplayName }}</p>
        <div class="mt-2 flex gap-3">
          <RouterLink
            v-if="auth.user"
            :to="{ name: 'public-home', params: { username: auth.user.username } }"
            class="text-muted hover:text-accent"
          >
            我的頁面
          </RouterLink>
          <button type="button" class="text-muted hover:text-danger" @click="logout">登出</button>
        </div>
      </div>
    </aside>

    <div
      v-if="menuOpen"
      class="fixed inset-0 z-30 bg-black/30 md:hidden"
      aria-hidden="true"
      @click="menuOpen = false"
    />

    <main class="min-w-0 px-4 py-8 sm:px-8">
      <RouterView />
    </main>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, type Component } from 'vue'
import { useRouter } from 'vue-router'
import {
  AcademicCapIcon,
  Bars3Icon,
  BriefcaseIcon,
  CalendarDaysIcon,
  ChatBubbleLeftRightIcon,
  CheckCircleIcon,
  ClockIcon,
  DocumentTextIcon,
  FolderIcon,
  HomeIcon,
  KeyIcon,
  PhoneIcon,
  SparklesIcon,
  UserCircleIcon,
  UsersIcon,
} from '@heroicons/vue/24/outline'
import { useAuthStore } from '@/stores/auth'
import ColorSchemeToggle from '@/components/public/ColorSchemeToggle.vue'

interface NavItem {
  label: string
  to: string
  icon: Component
}

const auth = useAuthStore()
const router = useRouter()
const menuOpen = ref(false)

const groups = computed<{ label: string; items: NavItem[] }[]>(() => [
  { label: '總覽', items: [{ label: '儀表板', to: '/admin/dashboard', icon: HomeIcon }] },
  {
    label: '公開頁面內容',
    items: [
      { label: '個人資料', to: '/admin/profile', icon: UserCircleIcon },
      { label: '作品', to: '/admin/works', icon: BriefcaseIcon },
      { label: '文章', to: '/admin/blog', icon: DocumentTextIcon },
      { label: '經歷', to: '/admin/experience', icon: AcademicCapIcon },
      { label: '技能', to: '/admin/skills', icon: SparklesIcon },
      { label: '聯絡方式', to: '/admin/contacts', icon: PhoneIcon },
      { label: '留言', to: '/admin/comments', icon: ChatBubbleLeftRightIcon },
    ],
  },
  {
    label: '工具',
    items: [
      { label: '行事曆', to: '/admin/calendar', icon: CalendarDaysIcon },
      { label: '待辦', to: '/admin/tasks', icon: CheckCircleIcon },
      { label: '工作追蹤', to: '/admin/work-tracking', icon: ClockIcon },
      { label: '檔案', to: '/admin/files', icon: FolderIcon },
    ],
  },
  {
    label: '帳號',
    items: [
      { label: '修改密碼', to: '/admin/account', icon: KeyIcon },
      ...(auth.isAdmin ? [{ label: '使用者管理', to: '/admin/users', icon: UsersIcon }] : []),
    ],
  },
])

async function logout() {
  await auth.logout()
  router.push('/login')
}
</script>
