<template>
  <div
    class="relative flex justify-center py-1.5 before:absolute before:inset-x-0 before:top-1/2 before:border-t before:border-dashed before:border-rule"
  >
    <div
      v-if="open"
      class="relative flex flex-wrap justify-center gap-1.5 bg-paper px-1.5"
      role="group"
      aria-label="選擇區塊類型"
    >
      <button
        v-for="type in blockTypes"
        :key="type"
        type="button"
        class="rounded-lg border border-rule bg-surface px-3 py-1 text-[0.83rem] hover:border-accent hover:text-accent"
        @click="$emit('add', type)"
      >
        {{ typeLabel[type] }}
      </button>
      <button
        type="button"
        class="px-2 text-[0.83rem] text-muted hover:text-ink"
        @click="$emit('close')"
      >
        取消
      </button>
    </div>
    <button
      v-else
      type="button"
      class="relative rounded-full border border-rule bg-paper px-3 py-0.5 text-[0.8rem] text-muted hover:border-accent hover:text-accent"
      @click="$emit('open')"
    >
      {{ label }}
    </button>
  </div>
</template>

<script setup lang="ts">
import type { BlockType } from '@/lib/workDocument'
import { blockTypes, typeLabel } from './blockTypes'

withDefaults(defineProps<{ open: boolean; label?: string }>(), { label: '＋ 在這裡新增區塊' })
defineEmits<{ open: []; close: []; add: [type: BlockType] }>()
</script>
