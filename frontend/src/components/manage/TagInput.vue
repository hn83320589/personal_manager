<template>
  <div
    class="flex flex-wrap items-center gap-1.5 rounded-lg border border-rule bg-paper px-2 py-1.5 focus-within:border-accent focus-within:ring-2 focus-within:ring-accent/20"
  >
    <span
      v-for="(tag, i) in modelValue"
      :key="tag"
      class="flex items-center gap-1 rounded-full bg-soft px-2.5 py-0.5 text-[0.8rem]"
    >
      {{ tag }}
      <button
        type="button"
        class="text-muted hover:text-danger"
        :aria-label="`移除標籤「${tag}」`"
        @click="removeAt(i)"
      >
        ×
      </button>
    </span>
    <input
      :id="id"
      v-model="draft"
      class="min-w-24 flex-1 bg-transparent py-0.5 text-[0.9rem] outline-none placeholder:text-muted/70"
      :list="`${id}-options`"
      :placeholder="modelValue.length ? '' : placeholder"
      :disabled="modelValue.length >= max"
      maxlength="50"
      @keydown="onKeydown"
      @blur="commit"
    />
    <datalist :id="`${id}-options`">
      <option v-for="option in remaining" :key="option" :value="option" />
    </datalist>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'

const props = withDefaults(
  defineProps<{
    modelValue: string[]
    id: string
    suggestions?: string[]
    max?: number
    placeholder?: string
  }>(),
  { suggestions: () => [], max: 10, placeholder: '輸入後按 Enter 或逗號' },
)
const emit = defineEmits<{ 'update:modelValue': [tags: string[]] }>()

const draft = ref('')
const remaining = computed(() => props.suggestions.filter((s) => !props.modelValue.includes(s)))

/** 名稱相同（忽略大小寫）的標籤只保留一個，與後端的規則一致。 */
function commit() {
  const tag = draft.value.replace(/,/g, '').trim()
  draft.value = ''
  if (!tag || props.modelValue.length >= props.max) return
  if (props.modelValue.some((t) => t.toLowerCase() === tag.toLowerCase())) return
  emit('update:modelValue', [...props.modelValue, tag])
}

function removeAt(index: number) {
  emit(
    'update:modelValue',
    props.modelValue.filter((_, i) => i !== index),
  )
}

function onKeydown(event: KeyboardEvent) {
  if (event.isComposing) return // 中文輸入法選字時的 Enter 不算送出
  if (event.key === 'Enter' || event.key === ',') {
    event.preventDefault()
    commit()
  } else if (event.key === 'Backspace' && !draft.value && props.modelValue.length) {
    removeAt(props.modelValue.length - 1)
  }
}
</script>
