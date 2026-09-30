<template>
  <form class="flex flex-col gap-4 rounded-2xl border border-rule bg-surface p-6" @submit.prevent="submit">
    <h2 class="font-hand text-2xl font-bold">留下訊息</h2>
    <div class="grid gap-4 sm:grid-cols-2">
      <div>
        <label for="gb-name" class="field-label">名字</label>
        <input id="gb-name" v-model.trim="form.name" class="input" maxlength="50" required autocomplete="name" />
      </div>
      <div>
        <label for="gb-email" class="field-label">Email（選填，不會公開）</label>
        <input id="gb-email" v-model.trim="form.email" type="email" class="input" maxlength="200" autocomplete="email" />
      </div>
    </div>
    <div>
      <label for="gb-message" class="field-label">留言</label>
      <textarea id="gb-message" v-model.trim="form.message" class="input min-h-28" maxlength="2000" required />
    </div>
    <p v-if="sent" class="text-sm text-success" role="status">已送出，留言經過審核後會顯示在這裡。</p>
    <p v-if="error" class="text-sm text-danger" role="alert">{{ error }}</p>
    <div>
      <button type="submit" class="btn btn-primary" :disabled="sending">{{ sending ? '送出中…' : '送出留言' }}</button>
    </div>
  </form>
</template>

<script setup lang="ts">
import { reactive, ref } from 'vue'
import { publicApi } from '@/api/public'
import { ApiError } from '@/api/http'

const props = defineProps<{ username: string }>()

const form = reactive({ name: '', email: '', message: '' })
const sending = ref(false)
const sent = ref(false)
const error = ref<string | null>(null)

async function submit() {
  sending.value = true
  sent.value = false
  error.value = null
  try {
    await publicApi.leaveMessage(props.username, {
      name: form.name,
      email: form.email || null,
      message: form.message,
    })
    sent.value = true
    form.message = ''
  } catch (e) {
    error.value = e instanceof ApiError ? [e.message, ...e.errors].join('：') : '送出失敗，請稍後再試。'
  } finally {
    sending.value = false
  }
}
</script>
