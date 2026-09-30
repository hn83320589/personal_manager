<template>
  <div class="mb-7 flex items-baseline justify-between gap-4 border-t-2 border-ink pt-3.5">
    <h2 :id="id" class="font-hand text-[1.7rem] font-bold">{{ title }}</h2>
    <RouterLink v-if="link" :to="link.to" class="text-sm text-muted hover:text-accent">
      {{ link.label }} →
    </RouterLink>
  </div>
</template>

<script setup lang="ts">
import type { RouteLocationRaw } from 'vue-router'

defineProps<{ id: string; title: string; link?: { to: RouteLocationRaw; label: string } }>()
</script>
