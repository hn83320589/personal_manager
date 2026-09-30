<template>
  <div
    class="pointer-events-none fixed inset-x-4 bottom-[calc(env(safe-area-inset-bottom,0px)+16px)] z-50 flex flex-col items-end gap-2 sm:left-auto sm:right-6"
    aria-live="polite"
  >
    <div
      v-for="toast in toasts.items"
      :key="toast.id"
      :role="toast.kind === 'error' ? 'alert' : 'status'"
      :class="[
        'pointer-events-auto flex w-full max-w-sm items-start gap-3 rounded-lg border px-4 py-3 text-sm shadow-lg',
        toast.kind === 'error'
          ? 'border-danger/40 bg-surface text-danger'
          : 'border-rule bg-ink text-paper',
      ]"
    >
      <span class="flex-1">{{ toast.message }}</span>
      <button
        type="button"
        class="opacity-70 hover:opacity-100"
        aria-label="關閉"
        @click="toasts.dismiss(toast.id)"
      >
        ✕
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useToastStore } from '@/stores/toast'

const toasts = useToastStore()
</script>
