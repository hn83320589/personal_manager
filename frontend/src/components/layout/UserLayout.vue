<template>
  <div v-if="isLoading" class="min-h-screen flex items-center justify-center bg-gray-50">
    <LoadingSpinner size="large" text="載入中..." />
  </div>

  <div v-else-if="!publicUser" class="min-h-screen flex items-center justify-center bg-gray-50">
    <div class="text-center">
      <h1 class="text-2xl font-bold text-gray-900 mb-2">找不到使用者</h1>
      <p class="text-gray-500 mb-4">@{{ username }} 不存在</p>
      <RouterLink to="/" class="text-sky-600 hover:text-sky-800">← 回首頁</RouterLink>
    </div>
  </div>

  <div v-else :style="dynamicThemeVars" class="min-h-screen bg-gray-50">
    <!-- Profile Header -->
    <div class="bg-white border-b border-gray-200 shadow-sm">
      <div class="max-w-5xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
        <div class="flex flex-col sm:flex-row items-center sm:items-start gap-4">
          <!-- Avatar -->
          <div class="flex-shrink-0">
            <img
              v-if="profile?.profileImageUrl"
              :src="profile.profileImageUrl"
              :alt="publicUser.fullName"
              class="h-20 w-20 rounded-full object-cover border-4 border-white shadow-md"
            />
            <div
              v-else
              class="h-20 w-20 rounded-full flex items-center justify-center text-white text-2xl font-bold shadow-md"
              :style="{ backgroundColor: 'var(--color-primary)' }"
            >
              {{ initials }}
            </div>
          </div>
          <!-- Info -->
          <div class="text-center sm:text-left flex-1">
            <h1 class="text-2xl font-bold text-gray-900">{{ publicUser.fullName || publicUser.username }}</h1>
            <p v-if="profile?.title" class="text-gray-600 mt-0.5">{{ profile.title }}</p>
            <p v-if="profile?.location" class="text-sm text-gray-500 mt-1 flex items-center justify-center sm:justify-start gap-1">
              <MapPinIcon class="h-4 w-4" />
              {{ profile.location }}
            </p>
          </div>

          <!-- Auth button -->
          <div class="flex-shrink-0 self-start">
            <RouterLink
              v-if="isAuthenticated"
              to="/admin/dashboard"
              class="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-medium border transition-colors"
              :style="{ color: 'var(--color-primary,#0ea5e9)', borderColor: 'var(--color-primary,#0ea5e9)' }"
            >
              <Squares2X2Icon class="h-3.5 w-3.5" />後台管理
            </RouterLink>
            <RouterLink
              v-else
              to="/login"
              class="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-medium text-gray-500 border border-gray-300 hover:border-gray-400 hover:text-gray-700 transition-colors"
            >
              <ArrowRightEndOnRectangleIcon class="h-3.5 w-3.5" />登入
            </RouterLink>
          </div>
        </div>
      </div>

      <!-- Navigation -->
      <nav class="max-w-5xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex overflow-x-auto space-x-1 pb-0 scrollbar-hide">
          <RouterLink
            v-for="item in navItems"
            :key="item.name"
            :to="`/@${username}${item.path}`"
            :class="[
              'flex-shrink-0 px-4 py-3 text-sm font-medium border-b-2 transition-colors whitespace-nowrap',
              isActiveNav(item.path)
                ? 'border-[var(--color-primary)] text-[var(--color-primary)]'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
            ]"
          >
            {{ item.name }}
          </RouterLink>
        </div>
      </nav>
    </div>

    <!-- Page content -->
    <main class="max-w-5xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <RouterView />
    </main>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, provide, watch } from 'vue'
import { useRoute, RouterLink, RouterView } from 'vue-router'
import { MapPinIcon, Squares2X2Icon, ArrowRightEndOnRectangleIcon } from '@heroicons/vue/24/outline'
import { useTheme } from '@/composables/useTheme'
import { setPageSeo } from '@/composables/useSeo'
import { useAuthStore } from '@/stores/auth'
import userDirectoryService from '@/services/userDirectoryService'
import profileService from '@/services/profileService'
import LoadingSpinner from '@/components/common/LoadingSpinner.vue'
import type { PublicUser, PersonalProfile } from '@/types/api'

const route = useRoute()
const authStore = useAuthStore()
const isAuthenticated = computed(() => authStore.isAuthenticated)
const username = computed(() => route.params.username as string)

const isLoading = ref(true)
const publicUser = ref<PublicUser | null>(null)
const profile = ref<PersonalProfile | null>(null)

// Reactive themeVars based on profile themeColor
const dynamicThemeVars = computed(() => {
  const color = profile.value?.themeColor ?? 'blue'
  const { themeVars: vars } = useTheme(color)
  return vars.value
})

const initials = computed(() => {
  const name = publicUser.value?.fullName || publicUser.value?.username || ''
  return name.split(/\s+/).map(w => w[0] ?? '').slice(0, 2).join('').toUpperCase() || 'U'
})

const navItems = [
  { name: '關於我', path: '' },
  { name: '學經歷', path: '/experience' },
  { name: '技能', path: '/skills' },
  { name: '作品集', path: '/portfolio' },
  { name: '部落格', path: '/blog' },
  { name: '行事曆', path: '/calendar' },
  { name: '留言板', path: '/guestbook' },
  { name: '聯絡我', path: '/contact' },
]

function isActiveNav(path: string) {
  const currentPath = route.path
  const base = `/@${username.value}`
  if (path === '') {
    return currentPath === base || currentPath === base + '/'
  }
  return currentPath.startsWith(base + path)
}

function applyUserSeo() {
  if (!publicUser.value) return
  setPageSeo({
    title: publicUser.value.fullName || publicUser.value.username,
    description: profile.value?.summary || undefined,
    ogImage: profile.value?.profileImageUrl || undefined,
    ogType: 'profile',
  })
}

async function loadUser(uname: string) {
  isLoading.value = true
  try {
    const response = await userDirectoryService.getPublicUser(uname)
    if (response.success && response.data) {
      publicUser.value = response.data
      // Load profile
      const profileResponse = await profileService.getProfileByUserId(response.data.id)
      if (profileResponse.success && profileResponse.data) {
        profile.value = profileResponse.data
      }
      applyUserSeo()
    } else {
      publicUser.value = null
    }
  } catch {
    publicUser.value = null
  } finally {
    isLoading.value = false
  }
}

// Provide userId to child views
provide('userId', computed(() => publicUser.value?.id ?? null))
provide('username', username)

onMounted(() => loadUser(username.value))

watch(username, (newName) => {
  if (newName) loadUser(newName)
})

// Re-apply user-level SEO on intra-user navigation (so child-specific overrides get reset)
watch(() => route.path, () => applyUserSeo())
</script>
