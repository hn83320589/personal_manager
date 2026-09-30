<template>
  <div
    class="flex min-w-52 flex-col rounded-card border border-rule bg-surface p-1.5 text-ink shadow-[0_8px_30px_rgba(0,0,0,.14)]"
    role="listbox"
    aria-label="插入內容"
  >
    <button
      v-for="(item, i) in items"
      :key="item.label"
      type="button"
      role="option"
      :aria-selected="i === selected"
      :class="[
        'flex justify-between gap-3 rounded-md px-2.5 py-1.5 text-left text-[0.88rem]',
        i === selected ? 'bg-soft' : 'hover:bg-soft',
      ]"
      @mousedown.prevent="choose(i)"
    >
      {{ item.label }}
      <span class="text-[0.78rem] text-muted">{{ item.hint }}</span>
    </button>
    <p v-if="!items.length" class="px-2.5 py-1.5 text-sm text-muted">沒有符合的項目</p>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import type { SlashItem } from './slashItems'

const props = defineProps<{ items: SlashItem[]; command: (item: SlashItem) => void }>()

const selected = ref(0)
watch(
  () => props.items,
  () => (selected.value = 0),
)

function choose(index: number) {
  const item = props.items[index]
  if (item) props.command(item)
}

/** 由 suggestion 外掛轉送的按鍵：上下選擇、Enter 確認。回傳 true 表示已處理。 */
function onKeyDown(event: KeyboardEvent): boolean {
  if (!props.items.length) return false
  if (event.key === 'ArrowDown') selected.value = (selected.value + 1) % props.items.length
  else if (event.key === 'ArrowUp')
    selected.value = (selected.value - 1 + props.items.length) % props.items.length
  else if (event.key === 'Enter') choose(selected.value)
  else return false
  return true
}

defineExpose({ onKeyDown })
</script>
