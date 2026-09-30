<template>
  <section class="mx-auto flex w-full max-w-read flex-col gap-3">
    <h2 v-if="block.title" class="font-hand text-[1.35rem] font-bold">{{ block.title }}</h2>
    <ul class="m-0 flex list-none flex-col overflow-hidden rounded-card border border-rule bg-surface p-0">
      <li
        v-for="file in block.items"
        :key="file.fileId"
        class="grid grid-cols-[auto_1fr] items-center gap-3.5 border-rule px-4 py-3.5 [&+&]:border-t min-[521px]:grid-cols-[auto_1fr_auto]"
      >
        <span
          :class="[
            'grid h-12 w-10 place-items-end justify-center rounded-md pb-1.5 font-mono text-[0.62rem] font-medium text-white',
            kindStyle[file.kind].color,
          ]"
          aria-hidden="true"
        >
          {{ kindStyle[file.kind].label }}
        </span>
        <div class="flex min-w-0 flex-col">
          <b class="break-words text-[0.95rem] font-medium">{{ file.fileName }}</b>
          <span class="text-[0.82rem] text-muted">
            {{ [file.description, kindStyle[file.kind].label, formatFileSize(file.size)].filter(Boolean).join(' · ') }}
          </span>
        </div>
        <div class="col-span-full flex flex-wrap gap-1.5 min-[521px]:col-span-1 min-[521px]:justify-end">
          <a
            v-if="file.kind === 'Pdf'"
            :href="file.url"
            target="_blank"
            rel="noopener"
            class="btn btn-small"
          >
            線上預覽
          </a>
          <a :href="file.url" :download="file.fileName" class="btn btn-small">下載</a>
        </div>
      </li>
    </ul>
  </section>
</template>

<script setup lang="ts">
import type { Schemas } from '@/api/types'
import { formatFileSize } from '@/lib/format'

defineProps<{ block: Schemas['FilesBlock'] }>()

const kindStyle: Record<Schemas['FileKind'], { label: string; color: string }> = {
  Pdf: { label: 'PDF', color: 'bg-[#c9392f]' },
  Word: { label: 'DOC', color: 'bg-[#2b5797]' },
  PowerPoint: { label: 'PPT', color: 'bg-[#c55a1c]' },
  Excel: { label: 'XLS', color: 'bg-[#1f7a45]' },
  Archive: { label: 'ZIP', color: 'bg-[#5b6270]' },
  Image: { label: 'IMG', color: 'bg-[#5b6270]' },
}
</script>
