<template>
  <div class="flex min-h-screen flex-col items-center justify-center gap-6 px-4 py-12">
    <RouterLink to="/" class="font-hand text-2xl font-bold no-underline">Personal Manager</RouterLink>
    <main class="flex w-full max-w-sm flex-col gap-5 rounded-2xl border border-rule bg-surface p-6 sm:p-8">
      <div class="flex flex-col gap-1">
        <h1 class="text-xl font-bold">{{ title }}</h1>
        <p v-if="description" class="text-sm text-muted">{{ description }}</p>
      </div>
      <slot />
    </main>
    <div class="text-sm text-muted"><slot name="footer" /></div>
  </div>
</template>

<script setup lang="ts">
defineProps<{ title: string; description?: string }>()
</script>
