<template>
  <AdminLayout>
    <!-- Page Header -->
    <div class="mb-8">
      <div class="flex justify-between items-center">
        <div>
          <h2 class="text-2xl font-bold text-gray-900">專長管理</h2>
          <p class="mt-1 text-sm text-gray-600">
            管理您的技能專長、等級評估和分類組織
          </p>
        </div>
        <BaseButton
          variant="primary"
          @click="showCreateModal = true"
        >
          <PlusIcon class="w-4 h-4 mr-2" />
          新增技能
        </BaseButton>
      </div>
    </div>

    <!-- Stats Cards -->
    <div class="grid grid-cols-1 md:grid-cols-4 gap-6 mb-6">
      <BaseCard>
        <div class="flex items-center">
          <div class="p-3 rounded-full bg-blue-100">
            <CpuChipIcon class="w-6 h-6 text-blue-600" />
          </div>
          <div class="ml-4">
            <h3 class="text-sm font-medium text-gray-500">總技能數</h3>
            <p class="text-2xl font-semibold text-gray-900">{{ skills.length }}</p>
          </div>
        </div>
      </BaseCard>

      <BaseCard>
        <div class="flex items-center">
          <div class="p-3 rounded-full bg-green-100">
            <ChartBarIcon class="w-6 h-6 text-green-600" />
          </div>
          <div class="ml-4">
            <h3 class="text-sm font-medium text-gray-500">專家級</h3>
            <p class="text-2xl font-semibold text-gray-900">{{ expertSkillsCount }}</p>
          </div>
        </div>
      </BaseCard>

      <BaseCard>
        <div class="flex items-center">
          <div class="p-3 rounded-full bg-yellow-100">
            <FolderIcon class="w-6 h-6 text-yellow-600" />
          </div>
          <div class="ml-4">
            <h3 class="text-sm font-medium text-gray-500">分類數</h3>
            <p class="text-2xl font-semibold text-gray-900">{{ categoriesCount }}</p>
          </div>
        </div>
      </BaseCard>

      <BaseCard>
        <div class="flex items-center">
          <div class="p-3 rounded-full bg-purple-100">
            <EyeIcon class="w-6 h-6 text-purple-600" />
          </div>
          <div class="ml-4">
            <h3 class="text-sm font-medium text-gray-500">公開顯示</h3>
            <p class="text-2xl font-semibold text-gray-900">{{ publicSkillsCount }}</p>
          </div>
        </div>
      </BaseCard>
    </div>

    <!-- Filters -->
    <div class="mb-6">
      <div class="flex flex-col sm:flex-row gap-4">
        <div class="flex-1">
          <div class="relative">
            <MagnifyingGlassIcon class="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-gray-400" />
            <input
              v-model="searchQuery"
              type="text"
              placeholder="搜尋技能名稱、分類..."
              class="pl-10 pr-4 py-2 w-full border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
            />
          </div>
        </div>
        <div class="flex space-x-2">
          <select
            v-model="selectedCategory"
            class="px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
          >
            <option value="">所有分類</option>
            <option v-for="category in categories" :key="category" :value="category">
              {{ category }}
            </option>
          </select>
          <select
            v-model="selectedLevel"
            class="px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
          >
            <option value="">所有等級</option>
            <option value="Beginner">初學者</option>
            <option value="Intermediate">中級</option>
            <option value="Advanced">高級</option>
            <option value="Expert">專家</option>
          </select>
          <select
            v-model="sortBy"
            class="px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
          >
            <option value="name">技能名稱</option>
            <option value="level">等級</option>
            <option value="category">分類</option>
            <option value="yearsOfExperience">經驗年數</option>
            <option value="sortOrder">排序</option>
          </select>
          <select
            v-model="sortOrder"
            class="px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
          >
            <option value="asc">升序</option>
            <option value="desc">降序</option>
          </select>
        </div>
      </div>
    </div>

    <!-- Skills Grid -->
    <div class="bg-white shadow rounded-lg overflow-hidden">
      <div v-if="filteredSkills.length === 0" class="p-8 text-center">
        <CpuChipIcon class="mx-auto h-12 w-12 text-gray-400" />
        <h3 class="mt-2 text-sm font-medium text-gray-900">沒有技能資料</h3>
        <p class="mt-1 text-sm text-gray-500">開始新增您的技能專長。</p>
        <div class="mt-6">
          <BaseButton
            variant="primary"
            @click="showCreateModal = true"
          >
            <PlusIcon class="w-4 h-4 mr-2" />
            新增技能
          </BaseButton>
        </div>
      </div>

      <div v-else>
        <!-- Skills Table -->
        <table class="min-w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                技能
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                等級
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                分類
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                經驗
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
            <tr v-for="skill in filteredSkills" :key="skill.id" class="hover:bg-gray-50">
              <td class="px-6 py-4 whitespace-nowrap">
                <div class="flex items-center">
                  <div class="flex-shrink-0 h-10 w-10">
                    <div class="h-10 w-10 rounded-lg bg-gradient-to-br from-blue-400 to-blue-600 flex items-center justify-center">
                      <CpuChipIcon class="h-5 w-5 text-white" />
                    </div>
                  </div>
                  <div class="ml-4">
                    <div class="text-sm font-medium text-gray-900">{{ skill.name }}</div>
                    <div class="text-sm text-gray-500">{{ skill.category || '未分類' }}</div>
                  </div>
                </div>
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <div class="flex items-center">
                  <div class="flex-shrink-0 mr-2">
                    <div class="flex space-x-1">
                      <div
                        v-for="i in 4"
                        :key="i"
                        :class="[
                          'w-2 h-2 rounded-full',
                          i <= getLevelNumber(skill.level) ? getLevelColor(skill.level) : 'bg-gray-200'
                        ]"
                      ></div>
                    </div>
                  </div>
                  <span class="text-sm font-medium text-gray-900">
                    {{ getLevelText(skill.level) }}
                  </span>
                </div>
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-gray-100 text-gray-800">
                  {{ skill.category || '未分類' }}
                </span>
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                {{ skill.yearsOfExperience ? `${skill.yearsOfExperience} 年` : '-' }}
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <span :class="[
                  'inline-flex px-2 py-1 text-xs font-semibold rounded-full',
                  skill.isPublic
                    ? 'bg-green-100 text-green-800'
                    : 'bg-gray-100 text-gray-800'
                ]">
                  {{ skill.isPublic ? '公開' : '私人' }}
                </span>
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                <div class="flex justify-end space-x-2">
                  <BaseButton
                    variant="outline"
                    size="small"
                    @click="editSkill(skill)"
                  >
                    <PencilIcon class="w-4 h-4" />
                  </BaseButton>
                  <BaseButton
                    variant="outline"
                    size="small"
                    @click="deleteSkill(skill.id)"
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

    <!-- Create/Edit Modal -->
    <BaseModal
      :show="showCreateModal"
      @close="showCreateModal = false"
      title="技能管理"
    >
      <SkillForm
        :skill="editingSkill"
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
          您確定要刪除這個技能嗎？此操作無法復原。
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
  CpuChipIcon,
  ChartBarIcon,
  FolderIcon,
  EyeIcon,
  MagnifyingGlassIcon,
  PencilIcon,
  TrashIcon
} from '@heroicons/vue/24/outline'
import { useSkillStore } from '@/stores/skill'
import type { Skill, SkillLevel } from '@/types/api'
import AdminLayout from '@/components/layout/AdminLayout.vue'
import BaseButton from '@/components/ui/BaseButton.vue'
import BaseCard from '@/components/ui/BaseCard.vue'
import BaseModal from '@/components/ui/BaseModal.vue'
import SkillForm from '@/components/admin/SkillForm.vue'

// Stores
const skillStore = useSkillStore()

// State
const searchQuery = ref('')
const selectedCategory = ref('')
const selectedLevel = ref('')
const sortBy = ref('name')
const sortOrder = ref<'asc' | 'desc'>('asc')
const showCreateModal = ref(false)
const showDeleteModal = ref(false)
const editingSkill = ref<Skill | null>(null)
const deletingSkillId = ref<number | null>(null)

// Computed
const skills = computed(() => skillStore.skills)

const categories = computed(() => {
  const categorySet = new Set(skills.value.map(skill => skill.category).filter(Boolean))
  return Array.from(categorySet).sort()
})

const expertSkillsCount = computed(() => {
  return skills.value.filter(skill => skill.level === 'Expert').length
})

const categoriesCount = computed(() => {
  return categories.value.length
})

const publicSkillsCount = computed(() => {
  return skills.value.filter(skill => skill.isPublic).length
})

const filteredSkills = computed(() => {
  let filtered = skills.value

  // Search filter
  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    filtered = filtered.filter(skill =>
      skill.name.toLowerCase().includes(query) ||
      skill.category?.toLowerCase().includes(query)
    )
  }

  // Category filter
  if (selectedCategory.value) {
    filtered = filtered.filter(skill => skill.category === selectedCategory.value)
  }

  // Level filter
  if (selectedLevel.value !== '') {
    filtered = filtered.filter(skill => skill.level === selectedLevel.value)
  }

  // Sort
  return filtered.sort((a, b) => {
    let aValue: any, bValue: any

    switch (sortBy.value) {
      case 'name':
        aValue = a.name.toLowerCase()
        bValue = b.name.toLowerCase()
        break
      case 'level':
        aValue = getLevelNumber(a.level)
        bValue = getLevelNumber(b.level)
        break
      case 'category':
        aValue = a.category || ''
        bValue = b.category || ''
        break
      case 'yearsOfExperience':
        aValue = a.yearsOfExperience || 0
        bValue = b.yearsOfExperience || 0
        break
      case 'sortOrder':
        aValue = a.sortOrder
        bValue = b.sortOrder
        break
      default:
        aValue = a.name
        bValue = b.name
    }

    if (sortOrder.value === 'asc') {
      return aValue > bValue ? 1 : -1
    } else {
      return aValue < bValue ? 1 : -1
    }
  })
})

// Methods
function getLevelNumber(level: SkillLevel): number {
  const map: Record<string, number> = {
    'Beginner': 1,
    'Intermediate': 2,
    'Advanced': 3,
    'Expert': 4
  }
  return map[level] || 0
}

function getLevelText(level: SkillLevel): string {
  const map: Record<string, string> = {
    'Beginner': '初學者',
    'Intermediate': '中級',
    'Advanced': '高級',
    'Expert': '專家'
  }
  return map[level] || '未知'
}

function getLevelColor(level: SkillLevel): string {
  const map: Record<string, string> = {
    'Beginner': 'bg-gray-400',
    'Intermediate': 'bg-blue-400',
    'Advanced': 'bg-yellow-400',
    'Expert': 'bg-green-400'
  }
  return map[level] || 'bg-gray-400'
}

function editSkill(skill: Skill) {
  editingSkill.value = { ...skill }
  showCreateModal.value = true
}

function deleteSkill(id: number) {
  deletingSkillId.value = id
  showDeleteModal.value = true
}

async function confirmDelete() {
  if (deletingSkillId.value) {
    try {
      await skillStore.deleteSkill(deletingSkillId.value)
      showDeleteModal.value = false
      deletingSkillId.value = null
    } catch (error) {
      console.error('Delete error:', error)
    }
  }
}

async function handleSave(data: any) {
  try {
    if (editingSkill.value) {
      // Update existing
      await skillStore.updateSkill(editingSkill.value.id, data)
    } else {
      // Create new
      await skillStore.createSkill(data)
    }
    handleCancel()
  } catch (error) {
    console.error('Save error:', error)
  }
}

function handleCancel() {
  showCreateModal.value = false
  editingSkill.value = null
}

// Lifecycle
onMounted(async () => {
  await skillStore.fetchSkills()
})
</script>