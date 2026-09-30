<template>
  <div class="min-h-screen bg-gray-50 flex flex-col justify-center py-12 sm:px-6 lg:px-8">
    <div class="sm:mx-auto sm:w-full sm:max-w-md">
      <div class="text-center">
        <h1 class="text-3xl font-bold text-gray-900">Personal Manager</h1>
        <h2 class="mt-6 text-2xl font-semibold text-gray-900">忘記密碼</h2>
        <p class="mt-2 text-sm text-gray-600">輸入您的電子信箱，我們將寄送重設連結</p>
      </div>
    </div>

    <div class="mt-8 sm:mx-auto sm:w-full sm:max-w-md">
      <div class="bg-white py-8 px-4 shadow sm:rounded-lg sm:px-10">
        <!-- Success -->
        <div v-if="submitted" class="text-center">
          <div class="mx-auto flex items-center justify-center h-12 w-12 rounded-full bg-green-100">
            <CheckIcon class="h-6 w-6 text-green-600" />
          </div>
          <h3 class="mt-4 text-lg font-medium text-gray-900">信件已寄出</h3>
          <p class="mt-2 text-sm text-gray-600">
            若此電子信箱有對應帳號，重設連結已寄出，請在 1 小時內完成重設。
          </p>
          <div class="mt-6">
            <router-link
              to="/login"
              class="text-sm font-medium text-primary-600 hover:text-primary-500"
            >
              ← 返回登入
            </router-link>
          </div>
        </div>

        <!-- Form -->
        <template v-else>
          <div v-if="errorMsg" class="mb-6 bg-red-50 border border-red-200 rounded-lg p-4">
            <p class="text-sm text-red-700">{{ errorMsg }}</p>
          </div>

          <BaseForm @submit="handleSubmit">
            <BaseInput
              v-model="email"
              type="email"
              label="電子信箱"
              placeholder="請輸入您的電子信箱"
              required
              :disabled="isLoading"
            />

            <BaseButton
              type="submit"
              class="w-full"
              :loading="isLoading"
              :disabled="!email.trim() || isLoading"
            >
              <span v-if="!isLoading">寄送重設連結</span>
              <span v-else>寄送中...</span>
            </BaseButton>
          </BaseForm>

          <div class="mt-6 text-center">
            <router-link
              to="/login"
              class="text-sm font-medium text-primary-600 hover:text-primary-500"
            >
              ← 返回登入
            </router-link>
          </div>
        </template>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { CheckIcon } from '@heroicons/vue/24/outline'
import authService from '@/services/authService'
import BaseButton from '@/components/ui/BaseButton.vue'
import BaseInput from '@/components/ui/BaseInput.vue'
import BaseForm from '@/components/ui/BaseForm.vue'

const email = ref('')
const isLoading = ref(false)
const submitted = ref(false)
const errorMsg = ref('')

async function handleSubmit() {
  if (!email.value.trim() || isLoading.value) return

  isLoading.value = true
  errorMsg.value = ''

  try {
    await authService.forgotPassword(email.value.trim())
    submitted.value = true
  } catch {
    errorMsg.value = '操作失敗，請稍後再試'
  } finally {
    isLoading.value = false
  }
}
</script>
