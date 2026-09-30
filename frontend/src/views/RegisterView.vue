<template>
  <AuthCard title="建立帳號" description="註冊後就有自己的公開頁面，網址是 /@帳號。">
    <form class="flex flex-col gap-4" @submit.prevent="submit">
      <FormField
        v-slot="{ id }"
        label="帳號"
        hint="3–30 個英文字母、數字、底線或連字號，註冊後無法修改。"
      >
        <input
          :id="id"
          v-model.trim="form.username"
          class="input font-mono"
          autocomplete="username"
          pattern="[A-Za-z0-9_\-]{3,30}"
          required
        />
      </FormField>
      <FormField v-slot="{ id }" label="姓名">
        <input
          :id="id"
          v-model.trim="form.fullName"
          class="input"
          autocomplete="name"
          maxlength="100"
          required
        />
      </FormField>
      <FormField v-slot="{ id }" label="Email" hint="用於找回密碼，不會公開。">
        <input
          :id="id"
          v-model.trim="form.email"
          type="email"
          class="input"
          autocomplete="email"
          required
        />
      </FormField>
      <FormField v-slot="{ id }" label="密碼" hint="至少 8 個字元">
        <input
          :id="id"
          v-model="form.password"
          type="password"
          class="input"
          autocomplete="new-password"
          minlength="8"
          required
        />
      </FormField>
      <p v-if="auth.error" class="text-sm text-danger" role="alert">{{ auth.error }}</p>
      <button type="submit" class="btn btn-primary justify-center" :disabled="auth.isLoading">
        {{ auth.isLoading ? '建立中…' : '建立帳號' }}
      </button>
    </form>
    <template #footer>
      已經有帳號？<RouterLink
        :to="{ name: 'login', query: route.query }"
        class="text-accent hover:underline"
        >登入</RouterLink
      >
    </template>
  </AuthCard>
</template>

<script setup lang="ts">
import { onMounted, reactive } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { setPageSeo } from '@/composables/useSeo'
import { safeRedirect } from '@/lib/safeRedirect'
import { useAuthStore } from '@/stores/auth'
import FormField from '@/components/manage/FormField.vue'
import AuthCard from '@/components/public/AuthCard.vue'

const route = useRoute()
const router = useRouter()
const auth = useAuthStore()
const form = reactive({ username: '', fullName: '', email: '', password: '' })

async function submit() {
  // 新帳號先到個人資料頁，把公開頁面填起來
  if (await auth.register({ ...form }))
    router.push(safeRedirect(route.query.redirect, '/admin/profile'))
}

onMounted(() => {
  auth.clearError()
  setPageSeo({ title: '建立帳號' })
})
</script>
