<template>
  <header
    class="sticky top-[env(safe-area-inset-top,0px)] z-20 -mx-4 border-b border-rule bg-paper/90 px-4 backdrop-blur sm:-mx-6 sm:px-6 lg:-mx-10 lg:px-10"
  >
    <div class="mx-auto flex min-h-[60px] max-w-wrap items-center justify-between gap-4">
      <RouterLink :to="{ name: 'public-home', params: { username } }" class="flex items-baseline gap-2.5 no-underline">
        <b class="font-hand text-xl font-bold">{{ fullName || username }}</b>
        <span class="font-mono text-xs text-muted">@{{ username }}</span>
      </RouterLink>

      <div class="flex items-center gap-1">
        <nav
          id="site-links"
          aria-label="主要導覽"
          :class="[
            'items-center gap-1',
            menuOpen
              ? 'absolute inset-x-0 top-full flex flex-col border-b border-rule bg-paper p-3 sm:static sm:flex-row sm:border-0 sm:p-0'
              : 'hidden sm:flex',
          ]"
        >
          <RouterLink
            v-for="link in links"
            :key="link.key"
            :to="link.to"
            :aria-current="link.key === active ? 'page' : undefined"
            class="w-full rounded-md px-3 py-1.5 text-[0.93rem] text-muted no-underline hover:bg-soft hover:text-ink aria-[current=page]:bg-soft aria-[current=page]:text-ink sm:w-auto"
            @click="menuOpen = false"
          >
            {{ link.label }}
          </RouterLink>
        </nav>
        <ColorSchemeToggle />
        <button
          type="button"
          class="grid h-9 w-9 place-items-center rounded-md text-muted hover:bg-soft hover:text-ink sm:hidden"
          aria-controls="site-links"
          :aria-expanded="menuOpen"
          aria-label="選單"
          @click="menuOpen = !menuOpen"
        >
          <Bars3Icon class="h-5 w-5" aria-hidden="true" />
        </button>
      </div>
    </div>
  </header>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute } from 'vue-router'
import { Bars3Icon } from '@heroicons/vue/24/outline'
import ColorSchemeToggle from './ColorSchemeToggle.vue'

const props = defineProps<{ username: string; fullName?: string }>()

const route = useRoute()
const menuOpen = ref(false)
watch(() => route.fullPath, () => (menuOpen.value = false))

const links = computed(() => {
  const params = { username: props.username }
  return [
    { key: 'works', label: '作品', to: { name: 'public-works', params } },
    { key: 'blog', label: '文章', to: { name: 'public-blog', params } },
    { key: 'about', label: '經歷', to: { name: 'public-home', params, hash: '#about' } },
    { key: 'contact', label: '聯絡', to: { name: 'public-home', params, hash: '#contact' } },
  ]
})

/** 目前所在的區塊：作品與文章以路由判斷，首頁上的區塊以 hash 判斷。 */
const active = computed(() => {
  const name = String(route.name ?? '')
  if (name.startsWith('public-work')) return 'works'
  if (name.startsWith('public-blog') || name === 'public-post') return 'blog'
  return null
})
</script>
