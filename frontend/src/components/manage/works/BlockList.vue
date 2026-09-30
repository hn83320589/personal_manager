<template>
  <div class="flex flex-col">
    <template v-for="(block, i) in blocks" :key="block.key">
      <InsertPoint
        :open="openAt === i"
        @open="openAt = i"
        @close="openAt = null"
        @add="(type) => add(type, i)"
      />
      <section
        class="overflow-hidden rounded-xl border border-rule bg-surface"
        :aria-label="`${typeLabel[block.type]}區塊`"
      >
        <header
          class="flex flex-wrap items-center justify-between gap-2 border-b border-rule bg-paper py-2 pl-3.5 pr-2.5"
        >
          <span class="flex items-center gap-2 text-sm font-medium">
            {{ typeLabel[block.type] }}
            <span
              v-if="isIncomplete(block)"
              class="rounded bg-soft px-1.5 text-xs font-normal text-muted"
            >
              尚未完成，暫不會儲存
            </span>
          </span>
          <span class="flex flex-wrap gap-1">
            <button type="button" :class="toolClass" :disabled="i === 0" @click="move(i, -1)">
              ↑ 上移
            </button>
            <button
              type="button"
              :class="toolClass"
              :disabled="i === blocks.length - 1"
              @click="move(i, 1)"
            >
              ↓ 下移
            </button>
            <DeleteButton
              :item-name="`${typeLabel[block.type]}區塊`"
              @confirm="blocks.splice(i, 1)"
            />
          </span>
        </header>
        <div class="flex flex-col gap-3 p-3.5">
          <component :is="editors[block.type]" v-model="blocks[i]" />
        </div>
      </section>
    </template>
    <InsertPoint
      :open="openAt === blocks.length"
      :label="blocks.length ? '＋ 在最後新增區塊' : '＋ 新增第一個區塊'"
      @open="openAt = blocks.length"
      @close="openAt = null"
      @add="(type) => add(type, blocks.length)"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, type Component } from 'vue'
import { isIncomplete, newBlock, type BlockType, type EditableBlock } from '@/lib/workDocument'
import DeleteButton from '../DeleteButton.vue'
import InsertPoint from './InsertPoint.vue'
import { typeLabel } from './blockTypes'
import CodeBlockEditor from './blocks/CodeBlockEditor.vue'
import EmbedBlockEditor from './blocks/EmbedBlockEditor.vue'
import FilesBlockEditor from './blocks/FilesBlockEditor.vue'
import GalleryBlockEditor from './blocks/GalleryBlockEditor.vue'
import ImageBlockEditor from './blocks/ImageBlockEditor.vue'
import MetricsBlockEditor from './blocks/MetricsBlockEditor.vue'
import TextBlockEditor from './blocks/TextBlockEditor.vue'

const blocks = defineModel<EditableBlock[]>({ required: true })

const editors: Record<BlockType, Component> = {
  text: TextBlockEditor,
  image: ImageBlockEditor,
  gallery: GalleryBlockEditor,
  files: FilesBlockEditor,
  embed: EmbedBlockEditor,
  metrics: MetricsBlockEditor,
  code: CodeBlockEditor,
}

const toolClass =
  'rounded-md border border-transparent px-2 py-0.5 text-[0.8rem] text-muted hover:border-rule hover:text-ink disabled:cursor-default disabled:opacity-35 disabled:hover:border-transparent'

const openAt = ref<number | null>(null)

function add(type: BlockType, at: number) {
  blocks.value.splice(at, 0, newBlock(type))
  openAt.value = null
}

function move(i: number, delta: -1 | 1) {
  blocks.value.splice(i + delta, 0, blocks.value.splice(i, 1)[0]!)
}
</script>
