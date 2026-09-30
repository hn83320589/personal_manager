<template>
  <figure class="mx-auto m-0 flex w-full max-w-read flex-col gap-2.5">
    <div class="overflow-hidden rounded-card border border-rule bg-[#12151b] text-[#dfe3ea]">
      <div
        class="flex items-center justify-between border-b border-[#262a32] px-3 py-1.5 font-mono text-[0.74rem] text-[#9aa1ad]"
      >
        <span>{{ block.language || '程式碼' }}</span>
        <button type="button" class="px-1 py-0.5 hover:text-white" @click="copy">
          {{ copied ? '已複製' : '複製' }}
        </button>
      </div>
      <pre class="m-0 overflow-x-auto px-4 py-3.5 font-mono text-[0.84rem] leading-[1.7]"><code>{{ block.code }}</code></pre>
    </div>
    <figcaption v-if="block.caption" class="flex gap-2.5 text-[0.87rem] text-muted">
      <span class="whitespace-nowrap pt-0.5 font-mono text-[0.78rem] text-accent">程式碼</span>
      <span>{{ block.caption }}</span>
    </figcaption>
  </figure>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import type { Schemas } from '@/api/types'

const props = defineProps<{ block: Schemas['CodeBlock'] }>()

const copied = ref(false)

async function copy() {
  try {
    await navigator.clipboard.writeText(props.block.code)
    copied.value = true
    setTimeout(() => (copied.value = false), 2000)
  } catch {
    // 無法使用剪貼簿時保持原狀，使用者仍可手動選取
  }
}
</script>
