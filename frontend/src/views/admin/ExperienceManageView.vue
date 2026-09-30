<template>
  <AdminLayout>
    <!-- Page Header -->
    <div class="mb-8">
      <div class="flex justify-between items-center">
        <div>
          <h2 class="text-2xl font-bold text-gray-900">學經歷管理</h2>
          <p class="mt-1 text-sm text-gray-600">
            管理您的教育背景和工作經歷資訊
          </p>
        </div>
        <BaseButton
          variant="primary"
          @click="showCreateModal = true"
        >
          <PlusIcon class="w-4 h-4 mr-2" />
          新增學經歷
        </BaseButton>
      </div>
    </div>

    <!-- Tabs -->
    <div class="mb-6">
      <div class="border-b border-gray-200">
        <nav class="-mb-px flex space-x-8">
          <button
            @click="activeTab = 'education'"
            :class="[
              'py-2 px-1 border-b-2 font-medium text-sm',
              activeTab === 'education'
                ? 'border-blue-500 text-blue-600'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
            ]"
          >
            <AcademicCapIcon class="w-5 h-5 mr-2 inline" />
            教育背景 ({{ educations.length }})
          </button>
          <button
            @click="activeTab = 'work'"
            :class="[
              'py-2 px-1 border-b-2 font-medium text-sm',
              activeTab === 'work'
                ? 'border-blue-500 text-blue-600'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
            ]"
          >
            <BriefcaseIcon class="w-5 h-5 mr-2 inline" />
            工作經歷 ({{ workExperiences.length }})
          </button>
        </nav>
      </div>
    </div>

    <!-- Search and Filters -->
    <div class="mb-6">
      <div class="flex flex-col sm:flex-row gap-4">
        <div class="flex-1">
          <div class="relative">
            <MagnifyingGlassIcon class="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-gray-400" />
            <input
              v-model="searchQuery"
              type="text"
              placeholder="搜尋學校、公司、職位..."
              class="pl-10 pr-4 py-2 w-full border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
            />
          </div>
        </div>
        <div class="flex space-x-2">
          <select
            v-model="sortBy"
            class="px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
          >
            <option value="startDate">開始日期</option>
            <option value="endDate">結束日期</option>
            <option value="title">標題</option>
          </select>
          <select
            v-model="sortOrder"
            class="px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
          >
            <option value="desc">降序</option>
            <option value="asc">升序</option>
          </select>
        </div>
      </div>
    </div>

    <!-- Content -->
    <div class="bg-white shadow rounded-lg">
      <!-- Education Tab -->
      <div v-if="activeTab === 'education'">
        <div v-if="filteredEducations.length === 0" class="p-8 text-center">
          <AcademicCapIcon class="mx-auto h-12 w-12 text-gray-400" />
          <h3 class="mt-2 text-sm font-medium text-gray-900">沒有教育背景資料</h3>
          <p class="mt-1 text-sm text-gray-500">開始新增您的教育背景資訊。</p>
          <div class="mt-6">
            <BaseButton
              variant="primary"
              @click="showCreateModal = true; newItemType = 'education'"
            >
              <PlusIcon class="w-4 h-4 mr-2" />
              新增教育背景
            </BaseButton>
          </div>
        </div>

        <div v-else class="overflow-hidden">
          <table class="min-w-full divide-y divide-gray-200">
            <thead class="bg-gray-50">
              <tr>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  學校/機構
                </th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  學位/課程
                </th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  期間
                </th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  狀態
                </th>
                <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                  操作
                </th>
              </tr>
            </thead>
            <tbody class="bg-white divide-y divide-gray-200">
              <tr v-for="education in filteredEducations" :key="education.id" class="hover:bg-gray-50">
                <td class="px-6 py-4 whitespace-nowrap">
                  <div class="flex items-center">
                    <div class="flex-shrink-0 h-10 w-10">
                      <div class="h-10 w-10 rounded-full bg-blue-100 flex items-center justify-center">
                        <AcademicCapIcon class="h-5 w-5 text-blue-600" />
                      </div>
                    </div>
                    <div class="ml-4">
                      <div class="text-sm font-medium text-gray-900">{{ education.school }}</div>
                      <div class="text-sm text-gray-500">{{ education.startYear }}-{{ education.endYear || '至今' }}</div>
                    </div>
                  </div>
                </td>
                <td class="px-6 py-4 whitespace-nowrap">
                  <div class="text-sm text-gray-900">{{ education.degree }}</div>
                  <div class="text-sm text-gray-500">{{ education.fieldOfStudy }}</div>
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                  {{ education.startYear }}{{ education.endYear ? ` - ${education.endYear}` : ' - 至今' }}
                </td>
                <td class="px-6 py-4 whitespace-nowrap">
                  <span :class="[
                    'inline-flex px-2 py-1 text-xs font-semibold rounded-full',
                    education.isPublic
                      ? 'bg-green-100 text-green-800'
                      : 'bg-gray-100 text-gray-800'
                  ]">
                    {{ education.isPublic ? '公開' : '私人' }}
                  </span>
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                  <div class="flex justify-end space-x-2">
                    <BaseButton
                      variant="outline"
                      size="small"
                      @click="editItem(education, 'education')"
                    >
                      <PencilIcon class="w-4 h-4" />
                    </BaseButton>
                    <BaseButton
                      variant="outline"
                      size="small"
                      @click="deleteItem(education.id, 'education')"
                      class="text-red-600 hover:text-red-900"
                    >
                      <TrashIcon class="w-4 h-4" />
                    </BaseButton>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Work Experience Tab -->
      <div v-if="activeTab === 'work'">
        <div v-if="filteredWorkExperiences.length === 0" class="p-8 text-center">
          <BriefcaseIcon class="mx-auto h-12 w-12 text-gray-400" />
          <h3 class="mt-2 text-sm font-medium text-gray-900">沒有工作經歷資料</h3>
          <p class="mt-1 text-sm text-gray-500">開始新增您的工作經歷資訊。</p>
          <div class="mt-6">
            <BaseButton
              variant="primary"
              @click="showCreateModal = true; newItemType = 'work'"
            >
              <PlusIcon class="w-4 h-4 mr-2" />
              新增工作經歷
            </BaseButton>
          </div>
        </div>

        <div v-else class="overflow-hidden">
          <table class="min-w-full divide-y divide-gray-200">
            <thead class="bg-gray-50">
              <tr>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  公司
                </th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  職位
                </th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  期間
                </th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  狀態
                </th>
                <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                  操作
                </th>
              </tr>
            </thead>
            <tbody class="bg-white divide-y divide-gray-200">
              <tr v-for="work in filteredWorkExperiences" :key="work.id" class="hover:bg-gray-50">
                <td class="px-6 py-4 whitespace-nowrap">
                  <div class="flex items-center">
                    <div class="flex-shrink-0 h-10 w-10">
                      <div class="h-10 w-10 rounded-full bg-green-100 flex items-center justify-center">
                        <BriefcaseIcon class="h-5 w-5 text-green-600" />
                      </div>
                    </div>
                    <div class="ml-4">
                      <div class="text-sm font-medium text-gray-900">{{ work.company }}</div>
                      <div class="text-sm text-gray-500">{{ work.position }}</div>
                    </div>
                  </div>
                </td>
                <td class="px-6 py-4 whitespace-nowrap">
                  <div class="text-sm text-gray-900">{{ work.position }}</div>
                  <div class="text-sm text-gray-500">{{ work.isCurrent ? '在職' : '離職' }}</div>
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                  {{ formatDateRange(work.startDate, work.endDate) }}
                  <span v-if="work.isCurrent" class="ml-2 text-green-600 font-medium">(目前)</span>
                </td>
                <td class="px-6 py-4 whitespace-nowrap">
                  <span :class="[
                    'inline-flex px-2 py-1 text-xs font-semibold rounded-full',
                    work.isPublic
                      ? 'bg-green-100 text-green-800'
                      : 'bg-gray-100 text-gray-800'
                  ]">
                    {{ work.isPublic ? '公開' : '私人' }}
                  </span>
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                  <div class="flex justify-end space-x-2">
                    <BaseButton
                      variant="outline"
                      size="small"
                      @click="editItem(work, 'work')"
                    >
                      <PencilIcon class="w-4 h-4" />
                    </BaseButton>
                    <BaseButton
                      variant="outline"
                      size="small"
                      @click="deleteItem(work.id, 'work')"
                      class="text-red-600 hover:text-red-900"
                    >
                      <TrashIcon class="w-4 h-4" />
                    </BaseButton>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- Create/Edit Modal -->
    <BaseModal
      :show="showCreateModal"
      @close="showCreateModal = false"
      title="學經歷管理"
    >
      <ExperienceForm
        :item="editingItem"
        :type="newItemType"
        @save="handleSave"
        @cancel="handleCancel"
      />
    </BaseModal>

    <!-- Delete Confirmation Modal -->
    <BaseModal
      :show="showDeleteModal"
      @close="showDeleteModal = false"
      title="確認刪除"
    >
      <div class="mt-2">
        <p class="text-sm text-gray-500">
          您確定要刪除這筆學經歷資料嗎？此操作無法復原。
        </p>
      </div>
      <div class="mt-5 flex justify-end space-x-3">
        <BaseButton
          variant="outline"
          @click="showDeleteModal = false"
        >
          取消
        </BaseButton>
        <BaseButton
          variant="danger"
          @click="confirmDelete"
        >
          刪除
        </BaseButton>
      </div>
    </BaseModal>
  </AdminLayout>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import {
  PlusIcon,
  AcademicCapIcon,
  BriefcaseIcon,
  MagnifyingGlassIcon,
  PencilIcon,
  TrashIcon
} from '@heroicons/vue/24/outline'
import { useExperienceStore } from '@/stores/experience'
import type { Education, WorkExperience } from '@/types/api'
import AdminLayout from '@/components/layout/AdminLayout.vue'
import BaseButton from '@/components/ui/BaseButton.vue'
import BaseModal from '@/components/ui/BaseModal.vue'
import ExperienceForm from '@/components/admin/ExperienceForm.vue'

// Stores
const experienceStore = useExperienceStore()

// State
const activeTab = ref<'education' | 'work'>('education')
const searchQuery = ref('')
const sortBy = ref('startDate')
const sortOrder = ref<'asc' | 'desc'>('desc')
const showCreateModal = ref(false)
const showDeleteModal = ref(false)
const newItemType = ref<'education' | 'work'>('education')
const editingItem = ref<Education | WorkExperience | null>(null)
const deletingItemId = ref<number | null>(null)
const deletingItemType = ref<'education' | 'work'>('education')

// Computed
const educations = computed(() => experienceStore.educations)
const workExperiences = computed(() => experienceStore.workExperiences)

const filteredEducations = computed(() => {
  let filtered = educations.value

  // Search filter
  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    filtered = filtered.filter(education =>
      education.school.toLowerCase().includes(query) ||
      education.degree.toLowerCase().includes(query) ||
      education.fieldOfStudy?.toLowerCase().includes(query)
    )
  }

  // Sort
  return filtered.sort((a, b) => {
    let aValue: any, bValue: any
    
    if (sortBy.value === 'startDate') {
      aValue = a.startYear
      bValue = b.startYear
    } else if (sortBy.value === 'endDate') {
      aValue = a.endYear || 9999
      bValue = b.endYear || 9999
    } else {
      aValue = (a as any)[sortBy.value]
      bValue = (b as any)[sortBy.value]
    }

    if (sortOrder.value === 'asc') {
      return aValue > bValue ? 1 : -1
    } else {
      return aValue < bValue ? 1 : -1
    }
  })
})

const filteredWorkExperiences = computed(() => {
  let filtered = workExperiences.value

  // Search filter
  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    filtered = filtered.filter(work =>
      work.company.toLowerCase().includes(query) ||
      work.position.toLowerCase().includes(query)
    )
  }

  // Sort
  return filtered.sort((a, b) => {
    let aValue: any, bValue: any
    
    if (sortBy.value.includes('Date')) {
      aValue = new Date((a as any)[sortBy.value] || '').getTime()
      bValue = new Date((b as any)[sortBy.value] || '').getTime()
    } else {
      aValue = (a as any)[sortBy.value]
      bValue = (b as any)[sortBy.value]
    }

    if (sortOrder.value === 'asc') {
      return aValue > bValue ? 1 : -1
    } else {
      return aValue < bValue ? 1 : -1
    }
  })
})

// Methods
function formatDateRange(startDate: string, endDate?: string): string {
  const start = new Date(startDate).toLocaleDateString('zh-TW', {
    year: 'numeric',
    month: 'short'
  })
  
  if (!endDate) {
    return `${start} - 至今`
  }
  
  const end = new Date(endDate).toLocaleDateString('zh-TW', {
    year: 'numeric',
    month: 'short'
  })
  
  return `${start} - ${end}`
}

function editItem(item: Education | WorkExperience, type: 'education' | 'work') {
  editingItem.value = { ...item }
  newItemType.value = type
  showCreateModal.value = true
}

function deleteItem(id: number, type: 'education' | 'work') {
  deletingItemId.value = id
  deletingItemType.value = type
  showDeleteModal.value = true
}

async function confirmDelete() {
  if (deletingItemId.value && deletingItemType.value) {
    try {
      if (deletingItemType.value === 'education') {
        await experienceStore.deleteEducation(deletingItemId.value)
      } else {
        await experienceStore.deleteWorkExperience(deletingItemId.value)
      }
      showDeleteModal.value = false
      deletingItemId.value = null
    } catch (error) {
      console.error('Delete error:', error)
    }
  }
}

async function handleSave(data: any) {
  try {
    if (editingItem.value) {
      // Update existing
      if (newItemType.value === 'education') {
        await experienceStore.updateEducation(editingItem.value.id, data)
      } else {
        await experienceStore.updateWorkExperience(editingItem.value.id, data)
      }
    } else {
      // Create new
      if (newItemType.value === 'education') {
        await experienceStore.createEducation(data)
      } else {
        await experienceStore.createWorkExperience(data)
      }
    }
    handleCancel()
  } catch (error) {
    console.error('Save error:', error)
  }
}

function handleCancel() {
  showCreateModal.value = false
  editingItem.value = null
  newItemType.value = 'education'
}

// Lifecycle
onMounted(async () => {
  await experienceStore.fetchEducations()
  await experienceStore.fetchWorkExperiences()
})
</script>