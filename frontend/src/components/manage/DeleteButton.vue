<template>
  <button
    type="button"
    :class="[
      'rounded-md px-2.5 py-1 text-sm',
      confirming
        ? 'bg-danger font-medium text-white'
        : 'text-muted hover:bg-danger/10 hover:text-danger',
    ]"
    :disabled="disabled"
    :aria-label="
      confirming
        ? `確定刪除${itemName ? `「${itemName}」` : ''}`
        : `刪除${itemName ? `「${itemName}」` : ''}`
    "
    @click="onClick"
    @blur="reset"
  >
    {{ confirming ? '確定刪除？' : label }}
  </button>
</template>

<script setup lang="ts">
// 兩段式刪除：第一次點擊要求確認，幾秒內沒有確認就恢復，不需要彈出對話框
import { onUnmounted, ref } from 'vue'

withDefaults(defineProps<{ itemName?: string; label?: string; disabled?: boolean }>(), {
  label: '刪除',
  disabled: false,
})
const emit = defineEmits<{ confirm: [] }>()

const confirming = ref(false)
let timer: ReturnType<typeof setTimeout> | undefined

function reset() {
  confirming.value = false
  clearTimeout(timer)
}

function onClick() {
  if (confirming.value) {
    reset()
    emit('confirm')
    return
  }
  confirming.value = true
  timer = setTimeout(reset, 4000)
}

onUnmounted(() => clearTimeout(timer))
</script>
