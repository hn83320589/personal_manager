<template>
  <div v-if="loading" class="py-24 text-center text-muted" role="status">載入中…</div>
  <div v-else-if="notFound" class="py-24 text-center">
    <p class="font-hand text-2xl">{{ notFoundText }}</p>
    <slot name="not-found" />
  </div>
  <div v-else-if="error" class="py-24 text-center" role="alert">
    <p class="text-muted">{{ error }}</p>
    <button type="button" class="mt-4 rounded-lg border border-rule bg-surface px-4 py-2 hover:border-ink" @click="$emit('retry')">
      重新載入
    </button>
  </div>
  <slot v-else />
</template>

<script setup lang="ts">
withDefaults(
  defineProps<{ loading: boolean; error: string | null; notFound?: boolean; notFoundText?: string }>(),
  { notFound: false, notFoundText: '找不到這個頁面' },
)
defineEmits<{ retry: [] }>()
</script>
