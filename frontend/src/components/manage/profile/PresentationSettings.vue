<template>
  <section
    class="flex flex-col gap-5 rounded-xl border border-rule bg-surface p-5"
    aria-labelledby="presentation-title"
  >
    <h2 id="presentation-title" class="font-mono text-xs uppercase tracking-[0.06em] text-muted">
      公開頁面呈現
    </h2>

    <div class="flex flex-col gap-1.5">
      <span class="text-sm font-medium">主題色</span>
      <div class="flex flex-wrap gap-2" role="radiogroup" aria-label="主題色">
        <button
          v-for="swatch in accentSwatches"
          :key="swatch.value"
          type="button"
          role="radio"
          :aria-checked="model.themeColor === swatch.value"
          :aria-label="swatch.label"
          :title="swatch.label"
          class="h-8 w-8 rounded-full ring-offset-2 ring-offset-surface aria-checked:ring-2 aria-checked:ring-ink"
          :style="{ background: swatch.color }"
          @click="model.themeColor = swatch.value"
        />
      </div>
    </div>

    <div class="flex flex-col gap-1.5">
      <span class="text-sm font-medium">作品集模式</span>
      <SegmentedControl
        :model-value="model.portfolioMode"
        :options="portfolioModes"
        label="作品集模式"
        @update:model-value="setMode"
      />
      <p class="text-xs text-muted">{{ modeHint }}</p>
    </div>

    <div class="flex flex-col gap-1.5">
      <span class="text-sm font-medium">作品卡片</span>
      <SegmentedControl v-model="model.cardStyle" :options="cardStyles" label="作品卡片版型" />
    </div>

    <div v-if="model.cardStyle === 'Visual'" class="flex flex-col gap-1.5">
      <span class="text-sm font-medium">圖片比例</span>
      <SegmentedControl v-model="model.cardRatio" :options="cardRatios" label="卡片圖片比例" />
    </div>

    <div class="flex flex-col gap-1.5">
      <span class="text-sm font-medium">技能顯示</span>
      <SegmentedControl
        v-model="model.skillDisplay"
        :options="skillDisplays"
        label="技能顯示方式"
      />
    </div>

    <FormField v-slot="{ id }" label="目前狀態" hint="顯示在名字上方，例如是否接案；留空則不顯示。">
      <input :id="id" v-model="model.availabilityStatus" class="input" maxlength="100" />
      <div class="flex flex-wrap gap-1.5">
        <button
          v-for="suggestion in statusSuggestions"
          :key="suggestion"
          type="button"
          class="rounded-full border border-dashed border-rule px-2.5 py-0.5 text-xs text-muted hover:border-accent hover:text-accent"
          @click="model.availabilityStatus = suggestion"
        >
          {{ suggestion }}
        </button>
      </div>
    </FormField>
  </section>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { Schemas } from '@/api/types'
import FormField from '../FormField.vue'
import SegmentedControl from '../SegmentedControl.vue'
import {
  accentSwatches,
  cardRatios,
  cardStyles,
  portfolioModes,
  skillDisplays,
  statusSuggestions,
  suggestedCardStyle,
  type ProfileForm,
} from './presentationOptions'

const model = defineModel<ProfileForm>({ required: true })

const modeHint = computed(
  () => portfolioModes.find((m) => m.value === model.value.portfolioMode)?.hint,
)

function setMode(mode: Schemas['PortfolioMode']) {
  model.value.portfolioMode = mode
  model.value.cardStyle = suggestedCardStyle[mode]
}
</script>
