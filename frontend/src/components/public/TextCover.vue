<template>
  <div class="absolute inset-0 flex flex-col justify-between overflow-hidden bg-accent/10 p-[18px] text-ink">
    <span class="font-mono text-[0.72rem] uppercase tracking-[0.06em] text-accent">{{ category || '作品' }}</span>
    <b class="font-hand text-[clamp(1.3rem,2.4vw,1.8rem)] font-bold leading-tight">{{ title }}</b>
    <span
      aria-hidden="true"
      class="absolute -bottom-[30px] -right-[30px] h-[140px] w-[140px] rounded-full border-[18px] border-accent/20"
    />
  </div>
</template>

<script setup lang="ts">
// 沒有封面圖的作品：以分類與標題產生文字封面
defineProps<{ title: string; category?: string }>()
</script>
