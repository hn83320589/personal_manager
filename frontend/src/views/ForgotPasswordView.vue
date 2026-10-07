<template>
  <AuthCard title="忘記密碼" description="輸入註冊時的 Email，我們會寄出重設密碼的連結。">
    <p v-if="sent" class="rounded-lg bg-success/10 p-3 text-sm text-success" role="status">
      如果這個 Email 有註冊，重設連結已經寄出，請到信箱查看（也看看垃圾郵件）。
    </p>
    <form v-else class="flex flex-col gap-4" @submit.prevent="submit">
      <FormField v-slot="{ id }" label="Email">
        <input
          :id="id"
          v-model.trim="email"
          type="email"
          class="input"
          autocomplete="email"
          required
        />
      </FormField>
      <p v-if="error" class="text-sm text-danger" role="alert">{{ error }}</p>
      <button type="submit" class="btn btn-primary justify-center" :disabled="running">
        {{ running ? '寄送中…' : '寄出重設連結' }}
      </button>
    </form>
    <template #footer>
      <RouterLink to="/login" class="hover:text-accent">← 回到登入</RouterLink>
    </template>
  </AuthCard>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { authApi } from '@/api/auth'
import { errorMessage } from '@/composables/useAsyncAction'
import FormField from '@/components/manage/FormField.vue'
import AuthCard from '@/components/public/AuthCard.vue'

const email = ref('')
const sent = ref(false)
const running = ref(false)
const error = ref<string | null>(null)

/** 不論 Email 是否存在，後端都回應成功（避免被用來查詢誰有註冊），畫面也用同一句說明。 */
async function submit() {
  running.value = true
  error.value = null
  try {
    await authApi.forgotPassword({ email: email.value })
    sent.value = true
  } catch (e) {
    error.value = errorMessage(e)
  } finally {
    running.value = false
  }
}
</script>
