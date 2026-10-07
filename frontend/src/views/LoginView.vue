<template>
  <AuthCard title="登入" description="登入後管理你的個人頁面、作品與文章。">
    <form class="flex flex-col gap-4" @submit.prevent="submit">
      <FormField v-slot="{ id }" label="帳號或 Email">
        <input :id="id" v-model.trim="username" class="input" autocomplete="username" required />
      </FormField>
      <FormField v-slot="{ id }" label="密碼">
        <input
          :id="id"
          v-model="password"
          type="password"
          class="input"
          autocomplete="current-password"
          required
        />
      </FormField>
      <p v-if="auth.error" class="text-sm text-danger" role="alert">{{ auth.error }}</p>
      <button type="submit" class="btn btn-primary justify-center" :disabled="auth.isLoading">
        {{ auth.isLoading ? '登入中…' : '登入' }}
      </button>
      <RouterLink to="/forgot-password" class="text-center text-sm text-muted hover:text-accent"
        >忘記密碼？</RouterLink
      >
    </form>
    <template #footer>
      還沒有帳號？<RouterLink
        :to="{ name: 'register', query: route.query }"
        class="text-accent hover:underline"
        >註冊</RouterLink
      >
    </template>
  </AuthCard>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { safeRedirect } from '@/lib/safeRedirect'
import { useAuthStore } from '@/stores/auth'
import FormField from '@/components/manage/FormField.vue'
import AuthCard from '@/components/public/AuthCard.vue'

const route = useRoute()
const router = useRouter()
const auth = useAuthStore()
const username = ref('')
const password = ref('')

async function submit() {
  if (await auth.login({ username: username.value, password: password.value })) {
    router.push(safeRedirect(route.query.redirect))
  }
}

onMounted(() => {
  auth.clearError()
})
</script>
