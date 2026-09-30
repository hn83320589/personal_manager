<template>
  <AdminPage title="修改密碼" description="修改後所有裝置都會登出，需要以新密碼重新登入。">
    <form
      class="flex max-w-md flex-col gap-4 rounded-xl border border-rule bg-surface p-5"
      @submit.prevent="submit"
    >
      <FormField v-slot="{ id }" label="目前的密碼" required>
        <input
          :id="id"
          v-model="current"
          type="password"
          class="input"
          autocomplete="current-password"
          required
        />
      </FormField>
      <FormField v-slot="{ id }" label="新密碼" required hint="至少 8 個字元">
        <input
          :id="id"
          v-model="next"
          type="password"
          class="input"
          autocomplete="new-password"
          minlength="8"
          required
        />
      </FormField>
      <FormField v-slot="{ id }" label="再輸入一次新密碼" required>
        <input
          :id="id"
          v-model="confirm"
          type="password"
          class="input"
          autocomplete="new-password"
          required
        />
      </FormField>
      <p v-if="mismatch" class="text-sm text-danger" role="alert">兩次輸入的新密碼不一樣。</p>
      <div>
        <button
          type="submit"
          class="btn btn-primary"
          :disabled="running || mismatch || next.length < 8"
        >
          {{ running ? '修改中…' : '修改密碼' }}
        </button>
      </div>
    </form>
  </AdminPage>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRouter } from 'vue-router'
import { authApi } from '@/api/auth'
import { useAsyncAction } from '@/composables/useAsyncAction'
import { useAuthStore } from '@/stores/auth'
import AdminPage from '@/components/manage/AdminPage.vue'
import FormField from '@/components/manage/FormField.vue'

const current = ref('')
const next = ref('')
const confirm = ref('')
const mismatch = computed(() => confirm.value.length > 0 && confirm.value !== next.value)

const router = useRouter()
const auth = useAuthStore()
const { run, running } = useAsyncAction()

/** 後端修改密碼後會撤銷所有工作階段（包含目前這個），因此直接登出並回到登入頁。 */
async function submit() {
  const ok = await run(
    async () => (
      await authApi.changePassword({ currentPassword: current.value, newPassword: next.value }),
      true
    ),
    { success: '已修改密碼，請以新密碼重新登入' },
  )
  if (!ok) return
  auth.clearSession()
  router.push('/login')
}
</script>
