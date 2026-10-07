<template>
  <nav v-if="result.totalPages > 1" class="flex items-center justify-between gap-4" aria-label="分頁">
    <button
      type="button"
      class="btn"
      :class="{ 'btn-small': small }"
      :disabled="!result.hasPreviousPage"
      @click="page = result.page - 1"
    >
      {{ prevLabel }}
    </button>
    <span class="font-mono text-sm text-muted">{{ result.page }} / {{ result.totalPages }}</span>
    <button
      type="button"
      class="btn"
      :class="{ 'btn-small': small }"
      :disabled="!result.hasNextPage"
      @click="page = result.page + 1"
    >
      {{ nextLabel }}
    </button>
  </nav>
</template>

<script setup lang="ts">
// 分頁的上一頁／下一頁；只有一頁時不顯示
defineProps<{
  result: { page: number; totalPages: number; hasPreviousPage: boolean; hasNextPage: boolean }
  prevLabel: string
  nextLabel: string
  small?: boolean
}>()

const page = defineModel<number>('page')
</script>
