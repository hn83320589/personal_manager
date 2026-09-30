<template>
  <div id="app" class="min-h-screen bg-gray-50 flex flex-col">
    <!-- Global loading indicator -->
    <LoadingSpinner v-if="isGlobalLoading" full-screen text="載入中..." />
    <RouterView />
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { RouterView } from 'vue-router'
import LoadingSpinner from '@/components/common/LoadingSpinner.vue'
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()
const isGlobalLoading = ref(false)

onMounted(() => {
  authStore.initializeAuth()
})
</script>

<style>
/* Global styles are handled by Tailwind CSS */
/* Any additional global styles can be added here */
</style>