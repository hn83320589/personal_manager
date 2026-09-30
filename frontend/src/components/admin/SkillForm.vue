<template>
  <form @submit.prevent="handleSubmit" class="space-y-6">
    <!-- Skill Name -->
    <div>
      <label for="name" class="block text-sm font-medium text-gray-700">
        技能名稱 <span class="text-red-500">*</span>
      </label>
      <BaseInput
        id="name"
        v-model="formData.name"
        type="text"
        required
        placeholder="請輸入技能名稱，如：JavaScript, Python"
        class="mt-1"
      />
    </div>

    <!-- Category -->
    <div>
      <label for="category" class="block text-sm font-medium text-gray-700">
        技能分類
      </label>
      <div class="mt-1 flex rounded-md shadow-sm">
        <select
          id="category"
          v-model="formData.category"
          class="flex-1 block w-full px-3 py-2 border border-gray-300 rounded-l-md focus:ring-blue-500 focus:border-blue-500"
        >
          <option value="">選擇分類</option>
          <option v-for="cat in predefinedCategories" :key="cat" :value="cat">
            {{ cat }}
          </option>
          <option value="custom">+ 自訂分類</option>
        </select>
        <BaseInput
          v-if="formData.category === 'custom'"
          v-model="customCategory"
          type="text"
          placeholder="輸入新分類"
          class="flex-1 rounded-l-none border-l-0"
        />
      </div>
    </div>

    <!-- Skill Level -->
    <div>
      <label for="level" class="block text-sm font-medium text-gray-700">
        技能等級 <span class="text-red-500">*</span>
      </label>
      <div class="mt-1 space-y-3">
        <div class="grid grid-cols-2 gap-4">
          <div
            v-for="level in skillLevels"
            :key="level.value"
            @click="formData.level = level.value"
            :class="[
              'relative cursor-pointer rounded-lg p-4 border-2 transition-all',
              formData.level === level.value
                ? 'border-blue-500 bg-blue-50'
                : 'border-gray-200 hover:border-gray-300'
            ]"
          >
            <div class="flex items-center justify-between">
              <div>
                <h4 class="text-sm font-medium text-gray-900">{{ level.label }}</h4>
                <p class="text-xs text-gray-500">{{ level.description }}</p>
              </div>
              <div class="flex space-x-1">
                <div
                  v-for="i in 4"
                  :key="i"
                  :class="[
                    'w-2 h-2 rounded-full',
                    i <= skillLevelIndex(level.value) + 1 ? level.color : 'bg-gray-200'
                  ]"
                ></div>
              </div>
            </div>
            <input
              type="radio"
              :value="level.value"
              v-model="formData.level"
              class="sr-only"
            />
          </div>
        </div>
      </div>
    </div>

    <!-- Years of Experience -->
    <div>
      <label for="yearsOfExperience" class="block text-sm font-medium text-gray-700">
        使用年數
      </label>
      <div class="mt-1 relative">
        <input
          id="yearsOfExperience"
          v-model.number="formData.yearsOfExperience"
          type="number"
          min="0"
          max="50"
          step="0.5"
          placeholder="0"
          class="block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
        />
        <div class="absolute inset-y-0 right-0 pr-3 flex items-center pointer-events-none">
          <span class="text-gray-500 text-sm">年</span>
        </div>
      </div>
      <p class="mt-1 text-xs text-gray-500">
        可以使用小數，如 1.5 年
      </p>
    </div>

    <!-- Competencies (Skills breakdown) -->
    <div>
      <label class="block text-sm font-medium text-gray-700">
        具體能力
      </label>
      <div class="mt-1">
        <div class="space-y-2">
          <div
            v-for="(competency, index) in formData.competencies"
            :key="index"
            class="flex items-center space-x-2"
          >
            <BaseInput
              v-model="competency.name"
              type="text"
              placeholder="具體技能，如：React Hooks, API 設計"
              class="flex-1"
            />
            <BaseButton
              type="button"
              variant="outline"
              size="small"
              @click="removeCompetency(index)"
              class="text-red-600 hover:text-red-900"
            >
              <TrashIcon class="w-4 h-4" />
            </BaseButton>
          </div>
        </div>
        <BaseButton
          type="button"
          variant="outline"
          size="small"
          @click="addCompetency"
          class="mt-2"
        >
          <PlusIcon class="w-4 h-4 mr-2" />
          新增能力
        </BaseButton>
      </div>
    </div>

    <!-- Related Projects/Certifications -->
    <div>
      <label for="projects" class="block text-sm font-medium text-gray-700">
        相關專案或證照
      </label>
      <BaseTextarea
        id="projects"
        v-model="formData.projects"
        :rows="2"
        placeholder="列出相關專案或獲得的證照..."
        class="mt-1"
      />
    </div>

    <!-- Sort Order -->
    <div>
      <label for="sortOrder" class="block text-sm font-medium text-gray-700">
        排序順序
      </label>
      <input
        id="sortOrder"
        v-model.number="formData.sortOrder"
        type="number"
        min="0"
        placeholder="數字越小排序越前面"
        class="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
      />
    </div>

    <!-- Is Public -->
    <div class="flex items-center justify-between">
      <div>
        <label class="text-sm font-medium text-gray-700">公開顯示</label>
        <p class="text-sm text-gray-500">勾選後將在前台頁面顯示</p>
      </div>
      <input
        id="isPublic"
        v-model="formData.isPublic"
        type="checkbox"
        class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
      />
    </div>

    <!-- Form Actions -->
    <div class="flex justify-end space-x-3 pt-6 border-t border-gray-200">
      <BaseButton
        type="button"
        variant="outline"
        @click="$emit('cancel')"
      >
        取消
      </BaseButton>
      <BaseButton
        type="submit"
        variant="primary"
        :loading="loading"
      >
        {{ skill ? '更新' : '建立' }}
      </BaseButton>
    </div>
  </form>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'
import { PlusIcon, TrashIcon } from '@heroicons/vue/24/outline'
import type { Skill, SkillLevel } from '@/types/api'
import BaseInput from '@/components/ui/BaseInput.vue'
import BaseTextarea from '@/components/ui/BaseTextarea.vue'
import BaseButton from '@/components/ui/BaseButton.vue'

// Props
interface Props {
  skill?: Skill | null
}

const props = withDefaults(defineProps<Props>(), {
  skill: null
})

// Emits
const emit = defineEmits<{
  save: [data: any]
  cancel: []
}>()

// State
const loading = ref(false)
const customCategory = ref('')

const formData = ref({
  name: '',
  level: 'Beginner' as SkillLevel,
  category: '',
  yearsOfExperience: undefined as number | undefined,
  competencies: [] as Array<{ name: string }>,
  projects: '',
  sortOrder: 0,
  isPublic: true
})

// Constants
const predefinedCategories = [
  '程式語言',
  '前端開發',
  '後端開發',
  '資料庫',
  '雲端服務',
  '開發工具',
  '設計工具',
  '專案管理',
  '軟技能'
]

const skillLevels: Array<{ value: SkillLevel; label: string; description: string; color: string }> = [
  {
    value: 'Beginner',
    label: '初學者',
    description: '基礎了解，需要指導',
    color: 'bg-gray-400'
  },
  {
    value: 'Intermediate',
    label: '中級',
    description: '能獨立完成基本任務',
    color: 'bg-blue-400'
  },
  {
    value: 'Advanced',
    label: '高級',
    description: '能處理複雜問題',
    color: 'bg-yellow-400'
  },
  {
    value: 'Expert',
    label: '專家',
    description: '能指導他人，創新解決方案',
    color: 'bg-green-400'
  }
]

// Helper to get numeric index for a skill level
function skillLevelIndex(level: SkillLevel): number {
  const order: SkillLevel[] = ['Beginner', 'Intermediate', 'Advanced', 'Expert']
  return order.indexOf(level)
}

// Watchers
watch(() => formData.value.category, (newValue) => {
  if (newValue !== 'custom') {
    customCategory.value = ''
  }
})

watch(customCategory, (newValue) => {
  if (newValue && formData.value.category === 'custom') {
    // Will be handled in submit
  }
})

// Methods
function addCompetency() {
  formData.value.competencies.push({ name: '' })
}

function removeCompetency(index: number) {
  formData.value.competencies.splice(index, 1)
}

async function handleSubmit() {
  loading.value = true
  
  try {
    const submitData = { ...formData.value }
    
    // Handle custom category
    if (submitData.category === 'custom' && customCategory.value) {
      submitData.category = customCategory.value
    }
    
    // Filter out empty competencies
    submitData.competencies = submitData.competencies.filter(c => c.name.trim())
    
    // Convert competencies to string for API
    const competenciesString = submitData.competencies.map(c => c.name).join(', ')
    
    // Prepare final data
    const finalData: Record<string, any> = {
      name: submitData.name,
      level: submitData.level,
      category: submitData.category || undefined,
      yearsOfExperience: submitData.yearsOfExperience,
      sortOrder: submitData.sortOrder,
      isPublic: submitData.isPublic
    }
    
    emit('save', finalData)
  } catch (error) {
    console.error('Submit error:', error)
  } finally {
    loading.value = false
  }
}

function initializeForm() {
  if (props.skill) {
    const skill = props.skill
    
    formData.value.name = skill.name
    formData.value.level = skill.level
    formData.value.category = skill.category || ''
    formData.value.yearsOfExperience = skill.yearsOfExperience
    formData.value.sortOrder = skill.sortOrder
    formData.value.isPublic = skill.isPublic
  } else {
    // Reset form for new skill
    formData.value = {
      name: '',
      level: 'Beginner' as SkillLevel,
      category: '',
      yearsOfExperience: undefined,
      competencies: [],
      projects: '',
      sortOrder: 0,
      isPublic: true
    }
  }
}

// Lifecycle
onMounted(() => {
  initializeForm()
})

// Watch for skill changes
watch(() => props.skill, () => {
  initializeForm()
}, { immediate: true })
</script>