<template>
  <div class="min-h-screen bg-white flex flex-col justify-center py-12 sm:px-6 lg:px-8">
    <div class="sm:mx-auto sm:w-full sm:max-w-md text-center">
      <div class="bg-gray-50 py-16 px-6 rounded-lg">
        <!-- 404 Icon -->
        <div class="flex justify-center mb-6">
          <ExclamationTriangleIcon class="h-20 w-20 text-gray-400" />
        </div>
        
        <!-- Error Message -->
        <h1 class="text-6xl font-bold text-primary-600 mb-4">404</h1>
        <h2 class="text-2xl font-semibold text-gray-900 mb-4">頁面不存在</h2>
        <p class="text-gray-600 mb-8">
          抱歉，您要找的頁面不存在或已被移動。
        </p>
        
        <!-- Action Buttons -->
        <div class="space-y-3 sm:space-y-0 sm:space-x-3 sm:flex sm:justify-center">
          <BaseButton @click="goHome" variant="primary">
            回到首頁
          </BaseButton>
          <BaseButton @click="goBack" variant="secondary">
            返回上頁
          </BaseButton>
        </div>
      </div>
      
      <!-- Helpful Links -->
      <div class="mt-8">
        <h3 class="text-sm font-medium text-gray-900 mb-4">您可能在尋找：</h3>
        <div class="grid grid-cols-2 gap-4 text-sm">
          <RouterLink
            to="/portfolio"
            class="text-primary-600 hover:text-primary-700 transition-colors duration-200"
          >
            作品集
          </RouterLink>
          <RouterLink
            to="/blog"
            class="text-primary-600 hover:text-primary-700 transition-colors duration-200"
          >
            部落格
          </RouterLink>
          <RouterLink
            to="/about"
            class="text-primary-600 hover:text-primary-700 transition-colors duration-200"
          >
            關於我
          </RouterLink>
          <RouterLink
            to="/contact"
            class="text-primary-600 hover:text-primary-700 transition-colors duration-200"
          >
            聯絡我
          </RouterLink>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router'
import { ExclamationTriangleIcon } from '@heroicons/vue/24/outline'
import BaseButton from '@/components/ui/BaseButton.vue'

const router = useRouter()

function goHome() {
  router.push('/')
}

function goBack() {
  router.go(-1)
}
</script>