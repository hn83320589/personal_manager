<template>
  <dialog
    ref="dialog"
    class="m-0 h-screen max-h-none w-screen max-w-none border-0 bg-transparent p-0 text-[#eceef2] backdrop:bg-[rgba(8,9,12,.92)]"
    aria-label="圖片檢視"
    @close="$emit('update:index', null)"
    @keydown.left.prevent="step(-1)"
    @keydown.right.prevent="step(1)"
  >
    <div v-if="current" class="grid h-full grid-rows-[auto_1fr_auto] gap-3 px-[clamp(12px,3vw,32px)] pb-6 pt-4">
      <div class="flex items-center justify-between font-mono text-[0.82rem] text-[#9aa1ad]">
        <span>{{ index! + 1 }} / {{ images.length }}</span>
        <button type="button" :class="buttonClass" aria-label="關閉" @click="close">✕</button>
      </div>
      <div class="grid min-h-0 grid-cols-[auto_1fr_auto] items-center gap-3">
        <button type="button" :class="buttonClass" aria-label="上一張" :disabled="index === 0" @click="step(-1)">
          ‹
        </button>
        <img
          :src="current.url"
          :alt="current.alt"
          class="m-auto max-h-[calc(100vh-190px)] max-w-full rounded-lg object-contain"
        />
        <button
          type="button"
          :class="buttonClass"
          aria-label="下一張"
          :disabled="index === images.length - 1"
          @click="step(1)"
        >
          ›
        </button>
      </div>
      <p class="mx-auto max-w-[44rem] text-center text-[0.95rem] text-[#d7dae0]">{{ current.caption }}</p>
    </div>
  </dialog>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import type { Schemas } from '@/api/types'

const props = defineProps<{ images: Schemas['PortfolioImage'][]; index: number | null }>()
const emit = defineEmits<{ 'update:index': [value: number | null] }>()

const dialog = ref<HTMLDialogElement>()
const current = computed(() => (props.index === null ? null : props.images[props.index]))
const buttonClass =
  'grid h-11 w-11 place-items-center rounded-full border border-white/20 bg-white/5 text-xl text-[#eceef2] disabled:cursor-default disabled:opacity-30'

// 使用原生 <dialog>：Esc 關閉、焦點鎖在對話框內，都由瀏覽器處理
watch(
  () => props.index,
  (value) => {
    const el = dialog.value
    if (!el) return
    if (value !== null && !el.open) el.showModal()
    if (value === null && el.open) el.close()
  },
)

function step(delta: number) {
  if (props.index === null) return
  const next = props.index + delta
  if (next >= 0 && next < props.images.length) emit('update:index', next)
}

function close() {
  emit('update:index', null)
}
</script>
