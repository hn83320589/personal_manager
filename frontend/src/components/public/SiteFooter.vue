<template>
  <footer
    class="mx-auto mt-24 flex max-w-wrap flex-wrap justify-between gap-3 border-t border-rule py-8 text-sm text-muted"
  >
    <span>© {{ year }} {{ fullName || username }} · 由 Personal Manager 建立</span>
    <nav aria-label="其他頁面" class="flex flex-wrap gap-x-4 gap-y-1">
      <RouterLink :to="{ name: 'public-guestbook', params: { username } }" class="hover:text-ink">留言板</RouterLink>
      <RouterLink :to="{ name: 'public-calendar', params: { username } }" class="hover:text-ink">行事曆</RouterLink>
      <RouterLink :to="auth.isAuthenticated ? '/admin' : '/login'" class="hover:text-ink">
        {{ auth.isAuthenticated ? '管理後台' : '登入' }}
      </RouterLink>
    </nav>
  </footer>
</template>

<script setup lang="ts">
import { useAuthStore } from '@/stores/auth'

defineProps<{ username: string; fullName?: string }>()

const auth = useAuthStore()
const year = new Date().getFullYear()
</script>
