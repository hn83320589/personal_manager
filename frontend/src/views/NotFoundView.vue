<template>
  <main class="flex min-h-screen flex-col items-center justify-center gap-4 px-4 text-center">
    <p class="font-mono text-sm text-muted">404</p>
    <h1 class="font-hand text-3xl font-bold">找不到這個頁面</h1>
    <p class="max-w-sm text-muted">網址可能打錯了，或是頁面已經移除。</p>
    <div class="mt-2 flex gap-2">
      <RouterLink to="/" class="btn btn-primary">回到首頁</RouterLink>
      <button type="button" class="btn" @click="$router.back()">返回上一頁</button>
    </div>
  </main>
</template>

<script setup lang="ts">
import { onMounted } from 'vue'
import { setPageSeo } from '@/composables/useSeo'

onMounted(() => setPageSeo({ title: '找不到頁面' }))
</script>
