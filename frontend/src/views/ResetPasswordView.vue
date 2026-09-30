<template>
  <div class="min-h-screen bg-gray-50 flex flex-col justify-center py-12 sm:px-6 lg:px-8">
    <div class="sm:mx-auto sm:w-full sm:max-w-md">
      <div class="text-center">
        <h1 class="text-3xl font-bold text-gray-900">Personal Manager</h1>
        <h2 class="mt-6 text-2xl font-semibold text-gray-900">重設密碼</h2>
      </div>
    </div>

    <div class="mt-8 sm:mx-auto sm:w-full sm:max-w-md">
      <div class="bg-white py-8 px-4 shadow sm:rounded-lg sm:px-10">
        <!-- No token -->
        <div v-if="!token" class="text-center">
          <p class="text-sm text-gray-600">無效的重設連結，請重新申請。</p>
          <div class="mt-4">
            <router-link
              to="/forgot-password"
              class="text-sm font-medium text-primary-600 hover:text-primary-500"
            >
              重新申請重設連結
            </router-link>
          </div>
        </div>

        <!-- Success -->
        <div v-else-if="success" class="text-center">
          <div class="mx-auto flex items-center justify-center h-12 w-12 rounded-full bg-green-100">
            <CheckIcon class="h-6 w-6 text-green-600" />
          </div>
          <h3 class="mt-4 text-lg font-medium text-gray-900">密碼重設成功</h3>
          <p class="mt-2 text-sm text-gray-600">請使用新密碼重新登入。</p>
          <div class="mt-6">
            <router-link
              to="/login"
              class="inline-flex justify-center py-2 px-4 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-primary-600 hover:bg-primary-700"
            >
              前往登入
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
              v-model="newPassword"
              type="password"
              label="新密碼"
              placeholder="至少 6 個字元"
              required
              :disabled="isLoading"
            />
            <BaseInput
              v-model="confirmPassword"
              type="password"
              label="確認新密碼"
              placeholder="再次輸入新密碼"
              required
              :disabled="isLoading"
            />

            <p v-if="passwordMismatch" class="text-sm text-red-600 -mt-2">
              兩次密碼輸入不一致
            </p>

            <BaseButton
              type="submit"
              class="w-full"
              :loading="isLoading"
              :disabled="!isFormValid || isLoading"
            >
              <span v-if="!isLoading">確認重設</span>
              <span v-else>重設中...</span>
            </BaseButton>
          </BaseForm>
        </template>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRoute } from 'vue-router'
import { CheckIcon } from '@heroicons/vue/24/outline'
import authService from '@/services/authService'
import BaseButton from '@/components/ui/BaseButton.vue'
import BaseInput from '@/components/ui/BaseInput.vue'
import BaseForm from '@/components/ui/BaseForm.vue'

const route = useRoute()
const token = computed(() => (route.query.token as string) || '')

const newPassword = ref('')
const confirmPassword = ref('')
const isLoading = ref(false)
const success = ref(false)
const errorMsg = ref('')

const passwordMismatch = computed(
  () => confirmPassword.value.length > 0 && newPassword.value !== confirmPassword.value
)

const isFormValid = computed(
  () =>
    newPassword.value.length >= 6 &&
    confirmPassword.value.length > 0 &&
    newPassword.value === confirmPassword.value
)

async function handleSubmit() {
  if (!isFormValid.value || isLoading.value) return

  isLoading.value = true
  errorMsg.value = ''

  try {
    const resp = await authService.resetPassword(token.value, newPassword.value)
    if (resp.success) {
      success.value = true
    } else {
      errorMsg.value = resp.message || '重設連結無效或已過期'
    }
  } catch {
    errorMsg.value = '操作失敗，請稍後再試'
  } finally {
    isLoading.value = false
  }
}
</script>
