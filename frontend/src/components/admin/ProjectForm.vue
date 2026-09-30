<template>
  <form @submit.prevent="handleSubmit" class="space-y-6">
    <!-- Project Title -->
    <div>
      <label for="title" class="block text-sm font-medium text-gray-700">
        作品標題 <span class="text-red-500">*</span>
      </label>
      <BaseInput
        id="title"
        v-model="formData.title"
        type="text"
        required
        placeholder="請輸入作品標題"
        class="mt-1"
      />
    </div>

    <!-- Description -->
    <div>
      <label for="description" class="block text-sm font-medium text-gray-700">
        作品描述
      </label>
      <BaseTextarea
        id="description"
        v-model="formData.description"
        :rows="4"
        placeholder="詳細描述您的作品，包括目標、挑戰、解決方案等..."
        class="mt-1"
      />
    </div>

    <!-- Technologies -->
    <div>
      <label class="block text-sm font-medium text-gray-700">
        技術棧 <span class="text-red-500">*</span>
      </label>
      <div class="mt-1">
        <div class="space-y-2">
          <div
            v-for="(tech, index) in formData.technologies"
            :key="index"
            class="flex items-center space-x-2"
          >
            <BaseInput
              v-model="tech.name"
              type="text"
              placeholder="技術名稱，如：Vue.js, Node.js, MongoDB"
              class="flex-1"
            />
            <BaseButton
              type="button"
              variant="outline"
              size="small"
              @click="removeTechnology(index)"
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
          @click="addTechnology"
          class="mt-2"
        >
          <PlusIcon class="w-4 h-4 mr-2" />
          新增技術
        </BaseButton>
      </div>
    </div>

    <!-- Project URLs -->
    <div class="space-y-4">
      <div>
        <label for="projectUrl" class="block text-sm font-medium text-gray-700">
          專案網址
        </label>
        <BaseInput
          id="projectUrl"
          v-model="formData.projectUrl"
          type="url"
          placeholder="https://your-project.com"
          class="mt-1"
        />
        <p class="mt-1 text-xs text-gray-500">
          線上展示網址或部署連結
        </p>
      </div>

      <div>
        <label for="repositoryUrl" class="block text-sm font-medium text-gray-700">
          程式碼庫
        </label>
        <BaseInput
          id="repositoryUrl"
          v-model="formData.repositoryUrl"
          type="url"
          placeholder="https://github.com/username/repository"
          class="mt-1"
        />
        <p class="mt-1 text-xs text-gray-500">
          GitHub、GitLab 或其他版本控制庫連結
        </p>
      </div>
    </div>

    <!-- Project Image -->
    <div>
      <label for="imageUrl" class="block text-sm font-medium text-gray-700">
        作品圖片
      </label>
      <div class="mt-1 space-y-4">
        <BaseInput
          id="imageUrl"
          v-model="formData.imageUrl"
          type="url"
          placeholder="https://example.com/image.jpg"
          class="w-full"
        />
        
        <!-- Image Preview -->
        <div v-if="formData.imageUrl" class="mt-4">
          <p class="text-sm text-gray-700 mb-2">圖片預覽：</p>
          <div class="relative inline-block">
            <img
              :src="formData.imageUrl"
              :alt="formData.title"
              class="h-32 w-48 object-cover rounded-lg border border-gray-300"
              @error="imageError = true"
              @load="imageError = false"
            />
            <div
              v-if="imageError"
              class="absolute inset-0 flex items-center justify-center bg-gray-100 rounded-lg border border-gray-300"
            >
              <div class="text-center">
                <PhotoIcon class="mx-auto h-8 w-8 text-gray-400" />
                <p class="text-xs text-gray-500 mt-1">圖片載入失敗</p>
              </div>
            </div>
          </div>
        </div>
        
        <p class="text-xs text-gray-500">
          建議尺寸：800x600 像素，支援 JPG、PNG 格式
        </p>
      </div>
    </div>

    <!-- Project Categories/Tags -->
    <div>
      <label class="block text-sm font-medium text-gray-700">
        專案分類
      </label>
      <select
        v-model="formData.category"
        class="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
      >
        <option value="">選擇分類</option>
        <option v-for="category in projectCategories" :key="category" :value="category">
          {{ category }}
        </option>
      </select>
    </div>

    <!-- Highlights/Features -->
    <div>
      <label class="block text-sm font-medium text-gray-700">
        專案亮點
      </label>
      <div class="mt-1">
        <div class="space-y-2">
          <div
            v-for="(highlight, index) in formData.highlights"
            :key="index"
            class="flex items-center space-x-2"
          >
            <BaseInput
              v-model="highlight.text"
              type="text"
              placeholder="專案特色或亮點，如：RWD 設計、高性能優化"
              class="flex-1"
            />
            <BaseButton
              type="button"
              variant="outline"
              size="small"
              @click="removeHighlight(index)"
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
          @click="addHighlight"
          class="mt-2"
        >
          <PlusIcon class="w-4 h-4 mr-2" />
          新增亮點
        </BaseButton>
      </div>
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

    <!-- Settings -->
    <div class="space-y-4 pt-4 border-t border-gray-200">
      <div class="flex items-center justify-between">
        <div>
          <label class="text-sm font-medium text-gray-700">公開顯示</label>
          <p class="text-sm text-gray-500">勾選後將在前台作品集頁面顯示</p>
        </div>
        <input
          id="isPublic"
          v-model="formData.isPublic"
          type="checkbox"
          class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
        />
      </div>

      <div class="flex items-center justify-between">
        <div>
          <label class="text-sm font-medium text-gray-700">精選作品</label>
          <p class="text-sm text-gray-500">精選作品會在首頁和作品集頂部顯示</p>
        </div>
        <input
          id="isFeatured"
          v-model="formData.isFeatured"
          type="checkbox"
          class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
        />
      </div>
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
        {{ project ? '更新作品' : '建立作品' }}
      </BaseButton>
    </div>
  </form>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'
import { PlusIcon, TrashIcon, PhotoIcon } from '@heroicons/vue/24/outline'
import type { Portfolio } from '@/types/api'
import BaseInput from '@/components/ui/BaseInput.vue'
import BaseTextarea from '@/components/ui/BaseTextarea.vue'
import BaseButton from '@/components/ui/BaseButton.vue'

// Props
interface Props {
  project?: Portfolio | null
}

const props = withDefaults(defineProps<Props>(), {
  project: null
})

// Emits
const emit = defineEmits<{
  save: [data: any]
  cancel: []
}>()

// State
const loading = ref(false)
const imageError = ref(false)

const formData = ref({
  title: '',
  description: '',
  technologies: [] as Array<{ name: string }>,
  projectUrl: '',
  repositoryUrl: '',
  imageUrl: '',
  category: '',
  highlights: [] as Array<{ text: string }>,
  sortOrder: 0,
  isPublic: true,
  isFeatured: false
})

// Constants
const projectCategories = [
  'Web 應用程式',
  'Mobile 應用程式',
  'Desktop 應用程式',
  '網站設計',
  'API/後端服務',
  '資料分析',
  '機器學習',
  'DevOps/部署',
  '開源專案',
  '個人專案',
  '商業專案',
  '學習專案'
]

// Watchers
watch(() => formData.value.imageUrl, () => {
  imageError.value = false
})

// Methods
function addTechnology() {
  formData.value.technologies.push({ name: '' })
}

function removeTechnology(index: number) {
  formData.value.technologies.splice(index, 1)
}

function addHighlight() {
  formData.value.highlights.push({ text: '' })
}

function removeHighlight(index: number) {
  formData.value.highlights.splice(index, 1)
}

async function handleSubmit() {
  loading.value = true
  
  try {
    const submitData = { ...formData.value }
    
    // Filter out empty technologies
    const technologies = submitData.technologies
      .filter(tech => tech.name.trim())
      .map(tech => tech.name.trim())
    
    // Filter out empty highlights
    const highlights = submitData.highlights
      .filter(highlight => highlight.text.trim())
      .map(highlight => highlight.text.trim())

    // Prepare final data for API
    const finalData = {
      title: submitData.title,
      description: submitData.description || undefined,
      technologies: technologies.join(', '), // Store as comma-separated string
      projectUrl: submitData.projectUrl || undefined,
      repositoryUrl: submitData.repositoryUrl || undefined,
      imageUrl: submitData.imageUrl || undefined,
      sortOrder: submitData.sortOrder,
      isPublic: submitData.isPublic,
      isFeatured: submitData.isFeatured
    }
    
    // Add highlights to description if they exist
    if (highlights.length > 0) {
      let extendedDescription = finalData.description || ''
      extendedDescription += (extendedDescription ? '\n\n' : '') + '專案亮點:\n' + highlights.map(h => `• ${h}`).join('\n')
      finalData.description = extendedDescription
    }
    
    emit('save', finalData)
  } catch (error) {
    console.error('Submit error:', error)
  } finally {
    loading.value = false
  }
}

function initializeForm() {
  if (props.project) {
    const project = props.project
    
    formData.value.title = project.title
    formData.value.description = project.description || ''
    formData.value.projectUrl = project.projectUrl || ''
    formData.value.repositoryUrl = project.repositoryUrl || ''
    formData.value.imageUrl = project.imageUrl || ''
    formData.value.sortOrder = project.sortOrder
    formData.value.isPublic = project.isPublic
    formData.value.isFeatured = project.isFeatured

    // Parse technologies
    const technologies = project.technologies || ''
    if (technologies) {
      formData.value.technologies = technologies
        .split(/[,;|]/)
        .map(tech => ({ name: tech.trim() }))
        .filter(tech => tech.name)
    }
    
    // Parse highlights from description
    if (project.description) {
      const description = project.description
      const highlightsMatch = description.match(/專案亮點:\n((?:• .+\n?)+)/)
      
      if (highlightsMatch) {
        const highlights = highlightsMatch[1]
          .split('\n')
          .map(line => line.replace('• ', '').trim())
          .filter(text => text)
          .map(text => ({ text }))
        
        formData.value.highlights = highlights
        
        // Clean description of parsed highlights
        formData.value.description = description.replace(/\n\n專案亮點:\n(?:• .+\n?)+/, '').trim()
      }
    }
  } else {
    // Reset form for new project
    formData.value = {
      title: '',
      description: '',
      technologies: [],
      projectUrl: '',
      repositoryUrl: '',
      imageUrl: '',
      category: '',
      highlights: [],
      sortOrder: 0,
      isPublic: true,
      isFeatured: false
    }
  }
}

// Lifecycle
onMounted(() => {
  initializeForm()
})

// Watch for project changes
watch(() => props.project, () => {
  initializeForm()
}, { immediate: true })
</script>