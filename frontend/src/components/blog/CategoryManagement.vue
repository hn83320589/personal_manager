<template>
  <div class="space-y-6">
    <!-- Categories List -->
    <div>
      <h3 class="text-lg font-medium text-gray-900 mb-4">現有分類</h3>
      
      <div v-if="categories.length === 0" class="text-center py-8">
        <TagIcon class="mx-auto h-12 w-12 text-gray-400" />
        <h3 class="mt-2 text-sm font-medium text-gray-900">還沒有分類</h3>
        <p class="mt-1 text-sm text-gray-500">
          建立您的第一個分類吧
        </p>
      </div>

      <div v-else class="space-y-3">
        <div
          v-for="category in categories"
          :key="category.name"
          class="flex items-center justify-between p-4 bg-gray-50 rounded-lg hover:bg-gray-100 transition-colors"
        >
          <div class="flex items-center space-x-4">
            <div class="flex-shrink-0">
              <TagIcon class="h-5 w-5 text-purple-500" />
            </div>
            <div class="flex-1 min-w-0">
              <div class="flex items-center space-x-3">
                <h4 class="text-sm font-medium text-gray-900">
                  {{ category.name }}
                </h4>
                <div class="flex space-x-4 text-xs text-gray-500">
                  <span class="flex items-center">
                    <DocumentTextIcon class="w-3 h-3 mr-1" />
                    {{ category.count }} 篇文章
                  </span>
                  <span class="flex items-center">
                    <EyeIcon class="w-3 h-3 mr-1" />
                    {{ category.publishedCount }} 已發布
                  </span>
                </div>
              </div>
            </div>
          </div>

          <div class="flex items-center space-x-2">
            <button
              @click="editCategory(category)"
              class="p-2 text-gray-400 hover:text-gray-600 transition-colors"
              title="編輯分類"
            >
              <PencilIcon class="w-4 h-4" />
            </button>
            <button
              @click="confirmDeleteCategory(category.name)"
              class="p-2 text-red-400 hover:text-red-600 transition-colors"
              :disabled="category.count > 0"
              :title="category.count > 0 ? '此分類下還有文章，無法刪除' : '刪除分類'"
            >
              <TrashIcon class="w-4 h-4" />
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Add/Edit Category Form -->
    <div class="border-t pt-6">
      <h3 class="text-lg font-medium text-gray-900 mb-4">
        {{ editingCategory ? '編輯分類' : '新增分類' }}
      </h3>

      <form @submit.prevent="handleSubmit" class="space-y-4">
        <div>
          <label for="categoryName" class="block text-sm font-medium text-gray-700">
            分類名稱 <span class="text-red-500">*</span>
          </label>
          <BaseInput
            id="categoryName"
            v-model="formData.name"
            type="text"
            placeholder="輸入分類名稱"
            required
            class="mt-1"
            :class="{ 'border-red-500': errors.name }"
          />
          <p v-if="errors.name" class="mt-1 text-sm text-red-600">{{ errors.name }}</p>
        </div>

        <div>
          <label for="categoryDescription" class="block text-sm font-medium text-gray-700">
            分類描述
          </label>
          <BaseTextarea
            id="categoryDescription"
            v-model="formData.description"
            :rows="3"
            placeholder="簡要描述這個分類的用途..."
            class="mt-1"
          />
        </div>

        <div>
          <label for="categoryColor" class="block text-sm font-medium text-gray-700">
            分類顏色
          </label>
          <div class="mt-1 flex items-center space-x-3">
            <div class="flex space-x-2">
              <button
                v-for="color in colorOptions"
                :key="color.value"
                type="button"
                @click="formData.color = color.value"
                :class="[
                  'w-8 h-8 rounded-full border-2 transition-all',
                  color.bgClass,
                  formData.color === color.value
                    ? 'ring-2 ring-gray-400 ring-offset-2'
                    : 'hover:scale-110'
                ]"
                :title="color.name"
              />
            </div>
            <div class="flex items-center space-x-2">
              <div 
                :class="[
                  'w-4 h-4 rounded-full',
                  getColorClass(formData.color || 'gray')
                ]"
              ></div>
              <span class="text-sm text-gray-600">
                {{ getColorName(formData.color || 'gray') }}
              </span>
            </div>
          </div>
        </div>

        <div class="flex items-center">
          <input
            id="isDefault"
            v-model="formData.isDefault"
            type="checkbox"
            class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
          />
          <label for="isDefault" class="ml-2 text-sm font-medium text-gray-700">
            設為預設分類
          </label>
          <p class="ml-2 text-xs text-gray-500">（新文章預設使用此分類）</p>
        </div>

        <div class="flex justify-end space-x-3 pt-4">
          <BaseButton
            type="button"
            variant="outline"
            @click="cancelEdit"
          >
            取消
          </BaseButton>
          <BaseButton
            type="submit"
            variant="primary"
            :disabled="!isFormValid"
          >
            {{ editingCategory ? '更新分類' : '建立分類' }}
          </BaseButton>
        </div>
      </form>
    </div>

    <!-- Statistics -->
    <div class="border-t pt-6">
      <h3 class="text-lg font-medium text-gray-900 mb-4">分類統計</h3>
      <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div class="bg-blue-50 rounded-lg p-4">
          <div class="flex items-center">
            <TagIcon class="h-8 w-8 text-blue-500" />
            <div class="ml-4">
              <p class="text-sm font-medium text-blue-900">總分類數</p>
              <p class="text-2xl font-bold text-blue-900">{{ categories.length }}</p>
            </div>
          </div>
        </div>

        <div class="bg-green-50 rounded-lg p-4">
          <div class="flex items-center">
            <DocumentTextIcon class="h-8 w-8 text-green-500" />
            <div class="ml-4">
              <p class="text-sm font-medium text-green-900">總文章數</p>
              <p class="text-2xl font-bold text-green-900">{{ totalArticles }}</p>
            </div>
          </div>
        </div>

        <div class="bg-purple-50 rounded-lg p-4">
          <div class="flex items-center">
            <EyeIcon class="h-8 w-8 text-purple-500" />
            <div class="ml-4">
              <p class="text-sm font-medium text-purple-900">已發布文章</p>
              <p class="text-2xl font-bold text-purple-900">{{ totalPublished }}</p>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Delete Confirmation Modal -->
    <BaseModal
      :show="showDeleteConfirm"
      @close="showDeleteConfirm = false"
      title="確認刪除分類"
    >
      <div class="mt-2">
        <p class="text-sm text-gray-500">
          您確定要刪除分類「{{ deletingCategoryName }}」嗎？此操作無法復原。
        </p>
        <p class="mt-2 text-xs text-red-600">
          注意：刪除分類後，該分類下的文章將變為無分類狀態。
        </p>
      </div>
      <div class="mt-5 flex justify-end space-x-3">
        <BaseButton
          variant="outline"
          @click="showDeleteConfirm = false"
        >
          取消
        </BaseButton>
        <BaseButton
          variant="danger"
          @click="deleteCategory"
        >
          確認刪除
        </BaseButton>
      </div>
    </BaseModal>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import {
  TagIcon,
  PencilIcon,
  TrashIcon,
  DocumentTextIcon,
  EyeIcon
} from '@heroicons/vue/24/outline'
import BaseInput from '@/components/ui/BaseInput.vue'
import BaseTextarea from '@/components/ui/BaseTextarea.vue'
import BaseButton from '@/components/ui/BaseButton.vue'
import BaseModal from '@/components/ui/BaseModal.vue'

// Props
interface CategoryWithStats {
  name: string
  count: number
  publishedCount: number
  description?: string
  color?: string
  isDefault?: boolean
}

interface Props {
  categories: CategoryWithStats[]
}

const props = defineProps<Props>()

// Emits
const emit = defineEmits<{
  save: [categoryData: {
    name: string
    description?: string
    color?: string
    isDefault?: boolean
    oldName?: string // For editing
  }]
  delete: [categoryName: string]
}>()

// State
const editingCategory = ref<CategoryWithStats | null>(null)
const showDeleteConfirm = ref(false)
const deletingCategoryName = ref('')

const formData = ref({
  name: '',
  description: '',
  color: 'blue',
  isDefault: false
})

const errors = ref({
  name: ''
})

// Color options
const colorOptions = [
  { name: '藍色', value: 'blue', bgClass: 'bg-blue-500' },
  { name: '綠色', value: 'green', bgClass: 'bg-green-500' },
  { name: '紫色', value: 'purple', bgClass: 'bg-purple-500' },
  { name: '紅色', value: 'red', bgClass: 'bg-red-500' },
  { name: '黃色', value: 'yellow', bgClass: 'bg-yellow-500' },
  { name: '粉色', value: 'pink', bgClass: 'bg-pink-500' },
  { name: '青色', value: 'cyan', bgClass: 'bg-cyan-500' },
  { name: '橙色', value: 'orange', bgClass: 'bg-orange-500' },
  { name: '灰色', value: 'gray', bgClass: 'bg-gray-500' }
]

// Computed
const isFormValid = computed(() => {
  return formData.value.name.trim().length > 0 && !errors.value.name
})

const totalArticles = computed(() => {
  return props.categories.reduce((sum, category) => sum + category.count, 0)
})

const totalPublished = computed(() => {
  return props.categories.reduce((sum, category) => sum + category.publishedCount, 0)
})

// Methods
function editCategory(category: CategoryWithStats) {
  editingCategory.value = category
  formData.value = {
    name: category.name,
    description: category.description || '',
    color: category.color || 'blue',
    isDefault: category.isDefault || false
  }
  errors.value.name = ''
}

function cancelEdit() {
  editingCategory.value = null
  formData.value = {
    name: '',
    description: '',
    color: 'blue',
    isDefault: false
  }
  errors.value.name = ''
}

function validateForm(): boolean {
  errors.value.name = ''
  
  if (!formData.value.name.trim()) {
    errors.value.name = '分類名稱為必填項'
    return false
  }
  
  // Check for duplicate names (excluding current editing category)
  const existingCategory = props.categories.find(cat => 
    cat.name.toLowerCase() === formData.value.name.trim().toLowerCase() &&
    cat.name !== editingCategory.value?.name
  )
  
  if (existingCategory) {
    errors.value.name = '此分類名稱已存在'
    return false
  }
  
  return true
}

function handleSubmit() {
  if (!validateForm()) return
  
  const categoryData = {
    name: formData.value.name.trim(),
    description: formData.value.description.trim() || undefined,
    color: formData.value.color,
    isDefault: formData.value.isDefault,
    oldName: editingCategory.value?.name // For editing
  }
  
  emit('save', categoryData)
  cancelEdit()
}

function confirmDeleteCategory(categoryName: string) {
  deletingCategoryName.value = categoryName
  showDeleteConfirm.value = true
}

function deleteCategory() {
  emit('delete', deletingCategoryName.value)
  showDeleteConfirm.value = false
  deletingCategoryName.value = ''
}

function getColorClass(color: string): string {
  const colorMap = {
    blue: 'bg-blue-500',
    green: 'bg-green-500',
    purple: 'bg-purple-500',
    red: 'bg-red-500',
    yellow: 'bg-yellow-500',
    pink: 'bg-pink-500',
    cyan: 'bg-cyan-500',
    orange: 'bg-orange-500',
    gray: 'bg-gray-500'
  }
  return colorMap[color as keyof typeof colorMap] || colorMap.gray
}

function getColorName(color: string): string {
  const colorMap = {
    blue: '藍色',
    green: '綠色',
    purple: '紫色',
    red: '紅色',
    yellow: '黃色',
    pink: '粉色',
    cyan: '青色',
    orange: '橙色',
    gray: '灰色'
  }
  return colorMap[color as keyof typeof colorMap] || '灰色'
}
</script>