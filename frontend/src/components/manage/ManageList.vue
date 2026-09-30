<template>
  <ul class="flex flex-col overflow-hidden rounded-xl border border-rule bg-surface">
    <li
      v-for="(item, index) in items"
      :key="item.id"
      class="flex flex-wrap items-center gap-x-3 gap-y-2 border-rule px-4 py-3 [&+&]:border-t"
    >
      <div class="min-w-0 flex-1">
        <slot :item="item" />
      </div>
      <div class="flex items-center gap-1">
        <template v-if="sortable">
          <button
            type="button"
            :class="iconButton"
            :disabled="index === 0"
            :aria-label="`上移「${labelOf(item)}」`"
            @click="$emit('move', item.id, -1)"
          >
            <ChevronUpIcon class="h-4 w-4" aria-hidden="true" />
          </button>
          <button
            type="button"
            :class="iconButton"
            :disabled="index === items.length - 1"
            :aria-label="`下移「${labelOf(item)}」`"
            @click="$emit('move', item.id, 1)"
          >
            <ChevronDownIcon class="h-4 w-4" aria-hidden="true" />
          </button>
        </template>
        <button
          type="button"
          class="rounded-md px-2.5 py-1 text-sm text-muted hover:bg-soft hover:text-ink"
          @click="$emit('edit', item)"
        >
          編輯
        </button>
        <DeleteButton :item-name="labelOf(item)" @confirm="$emit('remove', item.id)" />
      </div>
    </li>
  </ul>
</template>

<script setup lang="ts" generic="T extends { id: number }">
import { ChevronDownIcon, ChevronUpIcon } from '@heroicons/vue/24/outline'
import DeleteButton from './DeleteButton.vue'

withDefaults(defineProps<{ items: T[]; labelOf: (item: T) => string; sortable?: boolean }>(), {
  sortable: true,
})
defineEmits<{ edit: [item: T]; move: [id: number, delta: -1 | 1]; remove: [id: number] }>()

const iconButton =
  'grid h-8 w-8 place-items-center rounded-md text-muted hover:bg-soft hover:text-ink disabled:opacity-30 disabled:hover:bg-transparent'
</script>
