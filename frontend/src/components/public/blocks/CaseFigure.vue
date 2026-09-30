<template>
  <figure class="m-0 flex flex-col gap-2.5">
    <button
      type="button"
      :class="[
        'block w-full cursor-zoom-in overflow-hidden border-rule bg-soft p-0',
        bleed ? 'rounded-none border-y' : 'rounded-card border',
        cropped ? 'aspect-[4/3]' : '',
      ]"
      :aria-label="`放大：${image.alt || image.caption || `圖 ${number}`}`"
      @click="$emit('open')"
    >
      <img
        :src="image.url"
        :alt="image.alt"
        :width="image.width ?? undefined"
        :height="image.height ?? undefined"
        :class="['w-full', cropped ? 'h-full object-cover' : 'h-auto']"
        loading="lazy"
      />
    </button>
    <figcaption
      v-if="image.caption"
      :class="['flex max-w-read gap-2.5 text-[0.87rem] text-muted', captionClass]"
    >
      <span class="whitespace-nowrap pt-0.5 font-mono text-[0.78rem] text-accent">圖 {{ number }}</span>
      <span>{{ image.caption }}</span>
    </figcaption>
  </figure>
</template>

<script setup lang="ts">
import type { Schemas } from '@/api/types'

withDefaults(
  defineProps<{
    image: Schemas['PortfolioImage']
    /** 圖片在整件作品中的編號（「圖 3」） */
    number: number
    /** 圖庫的格狀版型統一裁成 4:3 */
    cropped?: boolean
    bleed?: boolean
    captionClass?: string
  }>(),
  { cropped: false, bleed: false, captionClass: '' },
)
defineEmits<{ open: [] }>()
</script>
