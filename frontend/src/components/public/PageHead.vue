<template>
  <div class="flex max-w-[44rem] flex-col gap-3.5 pt-16">
    <span class="font-mono text-[0.74rem] uppercase tracking-[0.08em] text-muted">{{ eyebrow }}</span>
    <h1 class="font-hand text-[clamp(2.2rem,5vw,3.2rem)] font-bold">{{ title }}</h1>
    <p v-if="$slots.default" class="text-muted"><slot /></p>
  </div>
</template>

<script setup lang="ts">
defineProps<{ eyebrow: string; title: string }>()
</script>
