<template>
  <div class="flex flex-col gap-12">
    <template v-for="(block, i) in blocks" :key="i">
      <div v-if="block.type === 'text'" class="mx-auto flex w-full max-w-read flex-col gap-3.5">
        <h2 v-if="block.title" class="font-hand text-[1.6rem] font-bold">{{ block.title }}</h2>
        <div class="prose text-[1.02rem]" v-html="sanitizeHtml(block.html)"></div>
      </div>

      <CaseFigure
        v-else-if="block.type === 'image'"
        :image="block.image"
        :number="numberOf(i, 0)"
        :bleed="block.layout === 'bleed'"
        :class="imageLayoutClass[block.layout] ?? imageLayoutClass.wide"
        :caption-class="block.layout === 'bleed' ? 'mx-auto w-full px-4 sm:px-6 lg:px-10' : 'mx-auto w-full'"
        @open="$emit('open-image', numberOf(i, 0) - 1)"
      />

      <div
        v-else-if="block.type === 'gallery' && block.items.length"
        :class="galleryClass[block.layout] ?? galleryClass['cols-2']"
      >
        <CaseFigure
          v-for="(image, k) in block.items"
          :key="k"
          :image="image"
          :number="numberOf(i, k)"
          :cropped="block.layout === 'cols-2' || block.layout === 'cols-3'"
          :class="block.layout === 'masonry' ? 'mb-5 break-inside-avoid' : ''"
          @open="$emit('open-image', numberOf(i, k) - 1)"
        />
      </div>

      <FilesBlock v-else-if="block.type === 'files' && block.items.length" :block="block" />

      <EmbedBlock v-else-if="block.type === 'embed'" :block="block" />

      <section
        v-else-if="block.type === 'metrics' && block.items.length"
        class="mx-auto flex w-full max-w-read flex-col gap-3.5"
      >
        <h2 v-if="block.title" class="font-hand text-[1.35rem] font-bold">{{ block.title }}</h2>
        <dl
          class="m-0 grid grid-cols-[repeat(auto-fit,minmax(10rem,1fr))] gap-px overflow-hidden rounded-card border border-rule bg-rule"
        >
          <div v-for="(metric, k) in block.items" :key="k" class="flex flex-col gap-1 bg-surface p-[18px]">
            <dt class="order-2 text-[0.88rem] text-muted">{{ metric.label }}</dt>
            <dd class="m-0 font-latin text-[clamp(1.6rem,3vw,2.1rem)] font-bold leading-tight tabular-nums text-accent">
              {{ metric.value }}
            </dd>
          </div>
        </dl>
      </section>

      <CodeBlock v-else-if="block.type === 'code'" :block="block" />
    </template>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { Schemas } from '@/api/types'
import { sanitizeHtml } from '@/lib/sanitizeHtml'
import CaseFigure from './CaseFigure.vue'
import CodeBlock from './CodeBlock.vue'
import EmbedBlock from './EmbedBlock.vue'
import FilesBlock from './FilesBlock.vue'

type Block = Schemas['PublicPortfolioDto']['blocks'][number]

const props = defineProps<{ blocks: Block[] }>()
defineEmits<{ 'open-image': [index: number] }>()

const imageLayoutClass: Record<string, string> = {
  narrow: 'mx-auto w-full max-w-read',
  wide: 'w-full',
  // 滿版出血：延伸到版面邊緣（抵銷外層的左右留白）
  bleed: '-mx-4 sm:-mx-6 lg:-mx-10 xl:mx-[calc((100vw-70rem)/-2)]',
}

const galleryClass: Record<string, string> = {
  masonry: 'block columns-[3_16rem] gap-5',
  'cols-2': 'grid grid-cols-1 gap-5 min-[721px]:grid-cols-2',
  'cols-3': 'grid grid-cols-1 gap-5 min-[721px]:grid-cols-3',
  stack: 'grid grid-cols-1 gap-7',
}

/** 每個區塊第一張圖的全域編號（從 1 開始），圖說與 lightbox 共用同一套編號。 */
const firstNumber = computed(() => {
  const numbers: number[] = []
  let next = 1
  for (const block of props.blocks) {
    numbers.push(next)
    if (block.type === 'image') next += 1
    else if (block.type === 'gallery') next += block.items.length
  }
  return numbers
})

const numberOf = (blockIndex: number, imageIndex: number) => firstNumber.value[blockIndex]! + imageIndex
</script>
