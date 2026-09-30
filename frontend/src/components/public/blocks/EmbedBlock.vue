<template>
  <figure v-if="embed" class="mx-auto m-0 flex w-full max-w-read flex-col gap-3">
    <a
      v-if="embed.kind === 'link'"
      :href="embed.src"
      target="_blank"
      rel="noopener"
      class="flex items-center justify-between gap-4 rounded-card border border-rule bg-surface px-5 py-4 hover:border-ink"
    >
      <span class="flex flex-col">
        <span class="font-mono text-[0.72rem] uppercase tracking-[0.06em] text-muted">{{ embed.provider }}</span>
        <span class="break-all">{{ block.url }}</span>
      </span>
      <span aria-hidden="true">↗</span>
    </a>
    <iframe
      v-else
      :src="embed.src"
      :title="block.caption || `${embed.provider} 嵌入內容`"
      :class="[
        'w-full rounded-card border border-rule bg-[#111318]',
        embed.kind === 'audio' ? 'h-[166px]' : 'aspect-video',
      ]"
      loading="lazy"
      referrerpolicy="strict-origin-when-cross-origin"
      allow="fullscreen; picture-in-picture; encrypted-media"
      sandbox="allow-scripts allow-same-origin allow-popups allow-presentation"
    ></iframe>
    <figcaption v-if="block.caption" class="flex gap-2.5 text-[0.87rem] text-muted">
      <span class="whitespace-nowrap pt-0.5 font-mono text-[0.78rem] text-accent">{{ embed.provider }}</span>
      <span>{{ block.caption }}</span>
    </figcaption>
  </figure>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { Schemas } from '@/api/types'
import { toEmbed } from '@/lib/embeds'

const props = defineProps<{ block: Schemas['EmbedBlock'] }>()

const embed = computed(() => toEmbed(props.block.url))
</script>
