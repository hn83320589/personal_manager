<template>
  <FormField v-slot="{ id }" label="網址">
    <input
      :id="id"
      v-model.trim="block.url"
      type="url"
      class="input"
      placeholder="https://youtu.be/…"
      :aria-describedby="`${id}-status`"
    />
    <p
      :id="`${id}-status`"
      :class="['text-xs', embed ? 'text-success' : block.url ? 'text-danger' : 'text-muted']"
    >
      <template v-if="embed">✓ 已辨識為 {{ embed.provider }}</template>
      <template v-else-if="block.url">不支援這個網站或網址格式。可嵌入：{{ providers }}</template>
      <template v-else
        >影片請上傳到 YouTube 或 Vimeo 後貼上網址；也支援 Figma、Sketchfab、SoundCloud
        等。</template
      >
    </p>
  </FormField>
  <FormField v-slot="{ id }" label="說明">
    <input :id="id" v-model="block.caption" class="input" maxlength="300" />
  </FormField>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { toEmbed } from '@/lib/embeds'
import type { EditableBlock } from '@/lib/workDocument'
import FormField from '../../FormField.vue'

const block = defineModel<Extract<EditableBlock, { type: 'embed' }>>({ required: true })

const embed = computed(() => toEmbed(block.value.url))
const providers = 'YouTube、Vimeo、Figma、Sketchfab、SoundCloud、Google 簡報、CodePen、GitHub Gist'
</script>
