<template>
  <button
    type="button"
    class="grid h-9 w-9 place-items-center rounded-md text-muted hover:bg-soft hover:text-ink"
    :aria-label="isDark ? '切換為淺色' : '切換為深色'"
    :title="isDark ? '切換為淺色' : '切換為深色'"
    @click="toggle"
  >
    <SunIcon v-if="isDark" class="h-5 w-5" aria-hidden="true" />
    <MoonIcon v-else class="h-5 w-5" aria-hidden="true" />
  </button>
</template>

<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref } from 'vue'
import { MoonIcon, SunIcon } from '@heroicons/vue/24/outline'
import { useColorScheme } from '@/composables/useColorScheme'

const { scheme, setScheme } = useColorScheme()
const systemDark = ref(false)
let media: MediaQueryList | null = null
const onSystemChange = (e: MediaQueryListEvent) => (systemDark.value = e.matches)

onMounted(() => {
  media = window.matchMedia('(prefers-color-scheme: dark)')
  systemDark.value = media.matches
  media.addEventListener('change', onSystemChange)
})
onUnmounted(() => media?.removeEventListener('change', onSystemChange))

const isDark = computed(() => scheme.value === 'dark' || (scheme.value === 'system' && systemDark.value))

function toggle() {
  setScheme(isDark.value ? 'light' : 'dark')
}
</script>
