<template>
  <SidePanel
    :open="form !== null"
    :title="eventId ? '編輯行程' : '新增行程'"
    :saving="saving"
    @close="$emit('close')"
    @submit="$emit('submit')"
  >
    <template v-if="form">
      <FormField v-slot="{ id }" label="標題" required>
        <input :id="id" v-model.trim="form.title" class="input" maxlength="200" required />
      </FormField>
      <label class="flex items-center gap-2 text-sm">
        <input v-model="form.isAllDay" type="checkbox" class="h-4 w-4 accent-accent" />
        全天
      </label>
      <div class="grid grid-cols-2 gap-3">
        <FormField v-slot="{ id }" label="開始日期">
          <input
            :id="id"
            v-model="form.startDate"
            type="date"
            class="input"
            required
            @change="keepEndAfterStart"
          />
        </FormField>
        <FormField v-if="!form.isAllDay" v-slot="{ id }" label="開始時間">
          <input :id="id" v-model="form.startTime" type="time" class="input" required />
        </FormField>
        <FormField v-slot="{ id }" label="結束日期">
          <input
            :id="id"
            v-model="form.endDate"
            type="date"
            class="input"
            :min="form.startDate"
            required
          />
        </FormField>
        <FormField v-if="!form.isAllDay" v-slot="{ id }" label="結束時間">
          <input :id="id" v-model="form.endTime" type="time" class="input" required />
        </FormField>
      </div>
      <div class="grid grid-cols-2 gap-3">
        <FormField v-slot="{ id }" label="重複">
          <select :id="id" v-model="form.recurrence" class="input">
            <option v-for="option in recurrences" :key="option.value" :value="option.value">
              {{ option.label }}
            </option>
          </select>
        </FormField>
        <FormField
          v-if="form.recurrence !== 'None'"
          v-slot="{ id }"
          label="重複到"
          hint="留空表示一直重複"
        >
          <input
            :id="id"
            v-model="form.recurrenceUntil"
            type="date"
            class="input"
            :min="form.startDate"
          />
        </FormField>
      </div>
      <FormField v-slot="{ id }" label="說明">
        <textarea :id="id" v-model="form.description" class="input min-h-20" maxlength="5000" />
      </FormField>
      <div class="flex flex-col gap-1.5">
        <span class="text-sm font-medium">顏色</span>
        <div class="flex gap-2" role="radiogroup" aria-label="顏色">
          <button
            v-for="color in colors"
            :key="color.value"
            type="button"
            role="radio"
            :aria-checked="form.color === color.value"
            :aria-label="color.label"
            class="h-7 w-7 rounded-full ring-offset-2 ring-offset-surface aria-checked:ring-2 aria-checked:ring-ink"
            :style="{ background: color.value || 'rgb(var(--accent))' }"
            @click="form.color = color.value"
          />
        </div>
      </div>
      <label class="flex items-center gap-2 text-sm">
        <input v-model="form.isPublic" type="checkbox" class="h-4 w-4 accent-accent" />
        顯示在公開行事曆
      </label>
    </template>
    <template v-if="eventId" #footer-start>
      <DeleteButton item-name="這個行程" @confirm="$emit('remove')" />
    </template>
  </SidePanel>
</template>

<script setup lang="ts">
import type { Schemas } from '@/api/types'
import type { EventForm } from '@/lib/eventForm'
import DeleteButton from '../DeleteButton.vue'
import FormField from '../FormField.vue'
import SidePanel from '../SidePanel.vue'

defineProps<{ eventId: number | null; saving: boolean }>()
defineEmits<{ close: []; submit: []; remove: [] }>()
const form = defineModel<EventForm | null>({ required: true })

const recurrences: { value: Schemas['Recurrence']; label: string }[] = [
  { value: 'None', label: '不重複' },
  { value: 'Daily', label: '每天' },
  { value: 'Weekly', label: '每週' },
  { value: 'Monthly', label: '每月' },
  { value: 'Yearly', label: '每年' },
]

const colors = [
  { value: '', label: '主題色' },
  { value: '#1d7550', label: '綠' },
  { value: '#bd3259', label: '紅' },
  { value: '#c55a1c', label: '橘' },
  { value: '#6541c0', label: '紫' },
  { value: '#5b6270', label: '灰' },
]

function keepEndAfterStart() {
  if (form.value && form.value.endDate < form.value.startDate)
    form.value.endDate = form.value.startDate
}
</script>
