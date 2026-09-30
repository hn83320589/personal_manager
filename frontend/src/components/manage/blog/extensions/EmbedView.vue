<template>
  <NodeViewWrapper
    :class="[
      'my-4 flex items-center gap-3 rounded-card border bg-[#111318] px-4 py-5 text-[#d7dae0]',
      selected ? 'border-accent ring-2 ring-accent/30' : 'border-rule',
    ]"
    data-drag-handle
  >
    <span class="rounded-full bg-white/10 px-2.5 py-0.5 font-mono text-[0.74rem]">
      {{ embed?.provider ?? '不支援的網址' }}
    </span>
    <span class="min-w-0 flex-1 truncate text-sm text-[#9aa1ad]">{{ node.attrs.src }}</span>
  </NodeViewWrapper>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { NodeViewWrapper, nodeViewProps } from '@tiptap/vue-3'
import { toEmbed } from '@/lib/embeds'

const props = defineProps(nodeViewProps)

const embed = computed(() => toEmbed(String(props.node.attrs.src ?? '')))
</script>
