<template>
  <div class="mx-auto flex max-w-[64rem] flex-col gap-6">
    <header class="flex flex-wrap items-end justify-between gap-3">
      <div class="flex flex-col gap-1">
        <h1 class="text-xl font-bold">{{ title }}</h1>
        <p v-if="description" class="text-sm text-muted">{{ description }}</p>
      </div>
      <div v-if="$slots.actions" class="flex flex-wrap items-center gap-2">
        <slot name="actions" />
      </div>
    </header>
    <slot />
  </div>
</template>

<script setup lang="ts">
defineProps<{ title: string; description?: string }>()
</script>
