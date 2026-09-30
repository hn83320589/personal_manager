<template>
  <div class="w-full">
    <label v-if="label" :for="textareaId" class="block text-sm font-medium text-gray-700 mb-1">
      {{ label }}
      <span v-if="required" class="text-red-500 ml-1">*</span>
    </label>
    
    <div class="relative">
      <textarea
        :id="textareaId"
        :value="modelValue"
        :placeholder="placeholder"
        :required="required"
        :disabled="disabled"
        :readonly="readonly"
        :rows="rows"
        :maxlength="maxLength"
        :class="textareaClasses"
        @input="handleInput"
        @blur="handleBlur"
        @focus="handleFocus"
      />
      
      <div v-if="hasError" class="absolute top-3 right-3 pointer-events-none">
        <ExclamationCircleIcon class="h-5 w-5 text-red-500" />
      </div>
    </div>
    
    <div v-if="showCharacterCount || hasError || helpText" class="mt-1 flex justify-between items-start">
      <div class="flex-1">
        <p v-if="hasError" class="text-sm text-red-600">{{ errorMessage }}</p>
        <p v-else-if="helpText" class="text-sm text-gray-500">{{ helpText }}</p>
      </div>
      
      <div v-if="showCharacterCount && maxLength" class="text-xs text-gray-400 ml-2">
        {{ characterCount }}/{{ maxLength }}
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { ExclamationCircleIcon } from '@heroicons/vue/24/outline'

interface Props {
  modelValue: string
  label?: string
  placeholder?: string
  required?: boolean
  disabled?: boolean
  readonly?: boolean
  error?: string
  helpText?: string
  rows?: number
  maxLength?: number
  showCharacterCount?: boolean
  resize?: 'none' | 'vertical' | 'horizontal' | 'both'
}

const props = withDefaults(defineProps<Props>(), {
  required: false,
  disabled: false,
  readonly: false,
  rows: 4,
  showCharacterCount: false,
  resize: 'vertical'
})

const emit = defineEmits<{
  'update:modelValue': [value: string]
  blur: [event: FocusEvent]
  focus: [event: FocusEvent]
}>()

const textareaId = ref(`textarea-${Math.random().toString(36).substr(2, 9)}`)
const hasError = computed(() => !!props.error)
const errorMessage = computed(() => props.error)
const characterCount = computed(() => props.modelValue.length)

const textareaClasses = computed(() => {
  const baseClasses = 'block w-full px-3 py-2 border rounded-lg text-sm placeholder-gray-400 transition-colors duration-200 focus:outline-none focus:ring-2'
  
  const stateClasses = hasError.value
    ? 'border-red-300 focus:border-red-500 focus:ring-red-500'
    : 'border-gray-300 focus:border-primary-500 focus:ring-primary-500'
    
  const disabledClasses = props.disabled
    ? 'bg-gray-100 text-gray-500 cursor-not-allowed'
    : 'bg-white text-gray-900'
    
  const readonlyClasses = props.readonly
    ? 'bg-gray-50 cursor-default'
    : ''
    
  const resizeClasses = {
    none: 'resize-none',
    vertical: 'resize-y',
    horizontal: 'resize-x',
    both: 'resize'
  }
  
  return [
    baseClasses,
    stateClasses,
    disabledClasses,
    readonlyClasses,
    resizeClasses[props.resize]
  ].join(' ')
})

function handleInput(event: Event) {
  const target = event.target as HTMLTextAreaElement
  emit('update:modelValue', target.value)
}

function handleBlur(event: FocusEvent) {
  emit('blur', event)
}

function handleFocus(event: FocusEvent) {
  emit('focus', event)
}
</script>