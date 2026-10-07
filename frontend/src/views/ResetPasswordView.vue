<template>
  <AuthCard title="重設密碼">
    <p v-if="!token" class="text-sm text-muted">
      這個重設連結無效。<RouterLink to="/forgot-password" class="text-accent hover:underline"
        >重新申請</RouterLink
      >
    </p>
    <div v-else-if="done" class="flex flex-col gap-4">
      <p class="rounded-lg bg-success/10 p-3 text-sm text-success" role="status">
        密碼已重設，請以新密碼登入。
      </p>
      <RouterLink to="/login" class="btn btn-primary justify-center">前往登入</RouterLink>
    </div>
    <form v-else class="flex flex-col gap-4" @submit.prevent="submit">
      <FormField v-slot="{ id }" label="新密碼" hint="至少 8 個字元">
        <input
          :id="id"
          v-model="password"
          type="password"
          class="input"
          autocomplete="new-password"
          minlength="8"
          required
        />
      </FormField>
      <FormField v-slot="{ id }" label="再輸入一次">
        <input
          :id="id"
          v-model="confirm"
          type="password"
          class="input"
          autocomplete="new-password"
          required
        />
      </FormField>
      <p v-if="mismatch" class="text-sm text-danger" role="alert">兩次輸入的密碼不一樣。</p>
      <p v-if="error" class="text-sm text-danger" role="alert">{{ error }}</p>
      <button
        type="submit"
        class="btn btn-primary justify-center"
        :disabled="running || mismatch || password.length < 8"
      >
        {{ running ? '重設中…' : '重設密碼' }}
      </button>
    </form>
  </AuthCard>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRoute } from 'vue-router'
import { authApi } from '@/api/auth'
import { errorMessage } from '@/composables/useAsyncAction'
import FormField from '@/components/manage/FormField.vue'
import AuthCard from '@/components/public/AuthCard.vue'

const route = useRoute()
const token = computed(() => (typeof route.query.token === 'string' ? route.query.token : ''))
const password = ref('')
const confirm = ref('')
const mismatch = computed(() => confirm.value.length > 0 && confirm.value !== password.value)
const running = ref(false)
const done = ref(false)
const error = ref<string | null>(null)

async function submit() {
  running.value = true
  error.value = null
  try {
    await authApi.resetPassword({ token: token.value, newPassword: password.value })
    done.value = true
  } catch (e) {
    error.value = errorMessage(e)
  } finally {
    running.value = false
  }
}
</script>
