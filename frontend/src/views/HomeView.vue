<template>
  <div class="min-h-screen bg-gray-50">
    <!-- Header -->
    <header class="bg-white border-b border-gray-200 shadow-sm">
      <div class="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 h-16 flex items-center justify-between">
        <h1 class="text-xl font-bold text-sky-600">Personal Manager</h1>
        <div v-if="authStore.isAuthenticated" class="flex items-center gap-3">
          <RouterLink to="/admin/dashboard" class="text-sm text-gray-600 hover:text-sky-600">後台管理</RouterLink>
          <button @click="authStore.logout()" class="text-sm text-gray-400 hover:text-red-500">登出</button>
        </div>
        <RouterLink v-else to="/login" class="text-sm font-medium text-sky-600 hover:text-sky-800">登入</RouterLink>
      </div>
    </header>

    <!-- Hero -->
    <section class="bg-white py-12 border-b border-gray-100">
      <div class="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 text-center">
        <h2 class="text-3xl font-extrabold text-gray-900 sm:text-4xl">探索個人主頁</h2>
        <p class="mt-3 text-lg text-gray-500 max-w-xl mx-auto">
          選擇一位使用者，探索他們的技能、作品與部落格。
        </p>
        <!-- Search -->
        <div class="mt-6 max-w-md mx-auto">
          <input
            v-model="searchQuery"
            type="text"
            placeholder="搜尋姓名或使用者名稱..."
            class="w-full px-4 py-2 border border-gray-300 rounded-lg bg-white text-gray-900 placeholder:text-gray-400 focus:outline-none focus:ring-2 focus:ring-sky-500 focus:border-sky-500"
          />
        </div>
      </div>
    </section>

    <!-- User Grid -->
    <section class="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 py-10">
      <LoadingSpinner v-if="isLoading" size="medium" text="載入使用者..." />

      <div v-else-if="filteredProfiles.length === 0" class="text-center py-16 text-gray-400">
        <p class="text-lg">找不到符合的使用者</p>
      </div>

      <div v-else class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
        <RouterLink
          v-for="p in filteredProfiles"
          :key="p.userId"
          :to="`/@${p.username}`"
          class="bg-white rounded-xl border border-gray-200 shadow-sm hover:shadow-md transition-shadow duration-200 overflow-hidden group"
        >
          <!-- Top accent bar -->
          <div class="h-1.5" :style="{ backgroundColor: themeColorHex(p.themeColor) }"></div>
          <div class="p-6">
            <!-- Avatar + Name -->
            <div class="flex items-center gap-4 mb-4">
              <div class="flex-shrink-0">
                <img
                  v-if="p.profileImageUrl"
                  :src="p.profileImageUrl"
                  :alt="p.fullName"
                  class="h-14 w-14 rounded-full object-cover border-2 border-gray-100"
                />
                <div
                  v-else
                  class="h-14 w-14 rounded-full flex items-center justify-center text-white font-bold text-xl"
                  :style="{ backgroundColor: themeColorHex(p.themeColor) }"
                >
                  {{ getInitials(p.fullName || p.username) }}
                </div>
              </div>
              <div>
                <h3 class="font-semibold text-gray-900 group-hover:text-sky-600 transition-colors">
                  {{ p.fullName || p.username }}
                </h3>
                <p class="text-sm text-gray-400">@{{ p.username }}</p>
              </div>
            </div>
            <!-- Title -->
            <p v-if="p.title" class="text-sm font-medium text-gray-700 mb-2">{{ p.title }}</p>
            <!-- Summary -->
            <p v-if="p.summary" class="text-sm text-gray-500 line-clamp-2">{{ p.summary }}</p>
            <!-- Location -->
            <p v-if="p.location" class="mt-3 text-xs text-gray-400 flex items-center gap-1">
              <MapPinIcon class="h-3.5 w-3.5" />
              {{ p.location }}
            </p>
          </div>
        </RouterLink>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { RouterLink } from 'vue-router'
import { MapPinIcon } from '@heroicons/vue/24/outline'
import { useUserDirectoryStore } from '@/stores/userDirectory'
import { useAuthStore } from '@/stores/auth'
import LoadingSpinner from '@/components/common/LoadingSpinner.vue'

const directoryStore = useUserDirectoryStore()
const authStore = useAuthStore()

const searchQuery = ref('')
const isLoading = computed(() => directoryStore.isLoading)

const filteredProfiles = computed(() => {
  const q = searchQuery.value.toLowerCase().trim()
  if (!q) return directoryStore.profiles
  return directoryStore.profiles.filter(p =>
    p.fullName.toLowerCase().includes(q) ||
    p.username.toLowerCase().includes(q) ||
    p.title?.toLowerCase().includes(q)
  )
})

const themeHexMap: Record<string, string> = {
  blue: '#0ea5e9',
  green: '#22c55e',
  purple: '#a855f7',
  rose: '#f43f5e',
  slate: '#64748b',
}

function themeColorHex(color: string): string {
  return themeHexMap[color] ?? themeHexMap.blue
}

function getInitials(name: string): string {
  return name.split(/\s+/).map(w => w[0] ?? '').slice(0, 2).join('').toUpperCase() || 'U'
}

onMounted(() => directoryStore.fetchDirectory())
</script>
