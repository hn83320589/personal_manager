<template>
  <dialog
    ref="dialog"
    class="m-0 ml-auto h-full max-h-none w-full max-w-none border-0 bg-transparent p-0 text-ink backdrop:bg-black/40 sm:w-[30rem]"
    :aria-labelledby="titleId"
    @close="$emit('close')"
    @click.self="$emit('close')"
  >
    <form
      class="flex h-full flex-col border-l border-rule bg-surface pt-[env(safe-area-inset-top,0px)]"
      @submit.prevent="$emit('submit')"
    >
      <header class="flex items-center justify-between gap-3 border-b border-rule px-5 py-4">
        <h2 :id="titleId" class="text-lg font-bold">{{ title }}</h2>
        <button
          type="button"
          class="grid h-8 w-8 place-items-center rounded-md text-muted hover:bg-soft hover:text-ink"
          aria-label="關閉"
          @click="$emit('close')"
        >
          ✕
        </button>
      </header>
      <div class="flex flex-1 flex-col gap-4 overflow-y-auto px-5 py-5">
        <slot />
      </div>
      <footer
        class="flex flex-wrap items-center justify-between gap-2 border-t border-rule px-5 py-4 pb-[calc(env(safe-area-inset-bottom,0px)+16px)]"
      >
        <div><slot name="footer-start" /></div>
        <div class="flex gap-2">
          <button type="button" class="btn" @click="$emit('close')">取消</button>
          <button type="submit" class="btn btn-primary" :disabled="saving">
            {{ saving ? '儲存中…' : submitLabel }}
          </button>
        </div>
      </footer>
    </form>
  </dialog>
</template>

<script setup lang="ts">
// 側邊編輯面板：原生 dialog 處理 Esc 關閉與焦點鎖定；手機上佔滿畫面
import { onMounted, ref, useId, watch } from 'vue'

const props = withDefaults(
  defineProps<{ open: boolean; title: string; saving?: boolean; submitLabel?: string }>(),
  { saving: false, submitLabel: '儲存' },
)
defineEmits<{ close: []; submit: [] }>()

const dialog = ref<HTMLDialogElement>()
const titleId = useId()

function sync(open: boolean) {
  const el = dialog.value
  if (!el) return
  if (open && !el.open) el.showModal?.()
  if (!open && el.open) el.close()
}

watch(() => props.open, sync)
onMounted(() => sync(props.open))
</script>
