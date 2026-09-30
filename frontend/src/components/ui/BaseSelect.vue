<template>
  <div class="w-full">
    <label v-if="label" :for="selectId" class="block text-sm font-medium text-gray-700 mb-1">
      {{ label }}
      <span v-if="required" class="text-red-500 ml-1">*</span>
    </label>
    
    <div class="relative">
      <select
        :id="selectId"
        :value="modelValue"
        :required="required"
        :disabled="disabled"
        :class="selectClasses"
        @change="handleChange"
        @blur="handleBlur"
        @focus="handleFocus"
      >
        <option v-if="placeholder" value="" disabled>{{ placeholder }}</option>
        
        <template v-for="(option, index) in normalizedOptions" :key="index">
          <optgroup v-if="'options' in option" :label="option.label">
            <option
              v-for="groupOption in option.options"
              :key="groupOption.value"
              :value="groupOption.value"
              :disabled="groupOption.disabled"
            >
              {{ groupOption.label }}
            </option>
          </optgroup>
          
          <option
            v-else
            :value="option.value"
            :disabled="option.disabled"
          >
            {{ option.label }}
          </option>
        </template>
      </select>
      
      <div class="absolute inset-y-0 right-0 flex items-center pr-3 pointer-events-none">
        <ChevronDownIcon v-if="!hasError" class="h-5 w-5 text-gray-400" />
        <ExclamationCircleIcon v-else class="h-5 w-5 text-red-500" />
      </div>
    </div>
    
    <p v-if="hasError" class="mt-1 text-sm text-red-600">{{ errorMessage }}</p>
    <p v-else-if="helpText" class="mt-1 text-sm text-gray-500">{{ helpText }}</p>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { ChevronDownIcon, ExclamationCircleIcon } from '@heroicons/vue/24/outline'

export interface SelectOption {
  label: string
  value: string | number
  disabled?: boolean
}

export interface SelectOptionGroup {
  label: string
  options: SelectOption[]
  isGroup: true
}

type Option = SelectOption | SelectOptionGroup

interface Props {
  modelValue: string | number
  label?: string
  placeholder?: string
  required?: boolean
  disabled?: boolean
  error?: string
  helpText?: string
  options: (string | SelectOption | SelectOptionGroup)[]
}

const props = withDefaults(defineProps<Props>(), {
  required: false,
  disabled: false,
})

const emit = defineEmits<{
  'update:modelValue': [value: string | number]
  change: [event: Event]
  blur: [event: FocusEvent]
  focus: [event: FocusEvent]
}>()

const selectId = ref(`select-${Math.random().toString(36).substr(2, 9)}`)
const hasError = computed(() => !!props.error)
const errorMessage = computed(() => props.error)

const normalizedOptions = computed(() => {
  return props.options.map(option => {
    if (typeof option === 'string') {
      return { label: option, value: option, isGroup: false as const }
    }
    if ('isGroup' in option && option.isGroup) {
      return option
    }
    return { ...option, isGroup: false as const }
  })
})

const selectClasses = computed(() => {
  const baseClasses = 'block w-full px-3 py-2 pr-10 border rounded-lg text-sm bg-white transition-colors duration-200 focus:outline-none focus:ring-2 appearance-none'
  
  const stateClasses = hasError.value
    ? 'border-red-300 focus:border-red-500 focus:ring-red-500'
    : 'border-gray-300 focus:border-primary-500 focus:ring-primary-500'
    
  const disabledClasses = props.disabled
    ? 'bg-gray-100 text-gray-500 cursor-not-allowed'
    : 'text-gray-900 cursor-pointer'
  
  return [baseClasses, stateClasses, disabledClasses].join(' ')
})

function handleChange(event: Event) {
  const target = event.target as HTMLSelectElement
  const value = target.value
  
  // Try to convert to number if the original option value was a number
  const normalizedValue = isNaN(Number(value)) ? value : Number(value)
  
  emit('update:modelValue', normalizedValue)
  emit('change', event)
}

function handleBlur(event: FocusEvent) {
  emit('blur', event)
}

function handleFocus(event: FocusEvent) {
  emit('focus', event)
}
</script>