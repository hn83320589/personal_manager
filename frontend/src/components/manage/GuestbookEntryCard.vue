<template>
  <article class="flex flex-col gap-3 rounded-xl border border-rule bg-surface p-4">
    <header class="flex flex-wrap items-baseline justify-between gap-2">
      <div class="flex flex-wrap items-baseline gap-x-2">
        <b class="font-medium">{{ entry.name }}</b>
        <a
          v-if="entry.email"
          :href="`mailto:${entry.email}`"
          class="text-sm text-muted hover:text-accent"
          >{{ entry.email }}</a
        >
        <span
          :class="[
            'rounded px-1.5 text-xs',
            entry.isApproved ? 'bg-success/10 text-success' : 'bg-soft text-muted',
          ]"
        >
          {{ entry.isApproved ? '已公開' : '待審核' }}
        </span>
      </div>
      <span class="font-mono text-xs text-muted">{{ formatDate(entry.createdAt) }}</span>
    </header>

    <p class="whitespace-pre-line">{{ entry.message }}</p>

    <div
      v-if="entry.reply && !replying"
      class="rounded-lg border-l-2 border-accent bg-accent/5 px-3 py-2 text-sm"
    >
      <span class="text-xs font-medium text-accent">你的回覆</span>
      <p class="whitespace-pre-line">{{ entry.reply }}</p>
    </div>

    <form v-if="replying" class="flex flex-col gap-2" @submit.prevent="submitReply">
      <label :for="`reply-${entry.id}`" class="sr-only">回覆 {{ entry.name }}</label>
      <textarea
        :id="`reply-${entry.id}`"
        v-model="draft"
        class="input min-h-20"
        maxlength="2000"
        placeholder="回覆會公開顯示在留言下方；留空則移除回覆"
      />
      <div class="flex gap-2">
        <button type="submit" class="btn btn-primary btn-small" :disabled="busy">儲存回覆</button>
        <button type="button" class="btn btn-small" @click="replying = false">取消</button>
      </div>
    </form>

    <footer class="flex flex-wrap items-center gap-1.5">
      <button
        type="button"
        class="btn btn-small"
        :disabled="busy"
        @click="$emit('approve', !entry.isApproved)"
      >
        {{ entry.isApproved ? '改為隱藏' : '公開' }}
      </button>
      <button v-if="!replying" type="button" class="btn btn-small" @click="startReply">
        {{ entry.reply ? '修改回覆' : '回覆' }}
      </button>
      <DeleteButton
        class="ml-auto"
        :item-name="`${entry.name} 的留言`"
        @confirm="$emit('remove')"
      />
    </footer>
  </article>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import type { Schemas } from '@/api/types'
import { formatDate } from '@/lib/format'
import DeleteButton from './DeleteButton.vue'

const props = defineProps<{ entry: Schemas['GuestbookEntryDto']; busy: boolean }>()
const emit = defineEmits<{ approve: [isApproved: boolean]; reply: [reply: string]; remove: [] }>()

const replying = ref(false)
const draft = ref('')

function startReply() {
  draft.value = props.entry.reply
  replying.value = true
}

function submitReply() {
  emit('reply', draft.value)
  replying.value = false
}
</script>
