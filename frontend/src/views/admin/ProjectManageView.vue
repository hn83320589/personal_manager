<template>
  <AdminLayout>
    <!-- Page Header -->
    <div class="mb-8">
      <div class="flex justify-between items-center">
        <div>
          <h2 class="text-2xl font-bold text-gray-900">作品管理</h2>
          <p class="mt-1 text-sm text-gray-600">
            管理您的作品集項目、技術棧和專案狀態
          </p>
        </div>
        <BaseButton
          variant="primary"
          @click="showCreateModal = true"
        >
          <PlusIcon class="w-4 h-4 mr-2" />
          新增作品
        </BaseButton>
      </div>
    </div>

    <!-- Stats Cards -->
    <div class="grid grid-cols-1 md:grid-cols-4 gap-6 mb-6">
      <BaseCard>
        <div class="flex items-center">
          <div class="p-3 rounded-full bg-blue-100">
            <FolderIcon class="w-6 h-6 text-blue-600" />
          </div>
          <div class="ml-4">
            <h3 class="text-sm font-medium text-gray-500">總作品數</h3>
            <p class="text-2xl font-semibold text-gray-900">{{ projects.length }}</p>
          </div>
        </div>
      </BaseCard>

      <BaseCard>
        <div class="flex items-center">
          <div class="p-3 rounded-full bg-green-100">
            <StarIcon class="w-6 h-6 text-green-600" />
          </div>
          <div class="ml-4">
            <h3 class="text-sm font-medium text-gray-500">精選作品</h3>
            <p class="text-2xl font-semibold text-gray-900">{{ featuredProjectsCount }}</p>
          </div>
        </div>
      </BaseCard>

      <BaseCard>
        <div class="flex items-center">
          <div class="p-3 rounded-full bg-purple-100">
            <EyeIcon class="w-6 h-6 text-purple-600" />
          </div>
          <div class="ml-4">
            <h3 class="text-sm font-medium text-gray-500">公開作品</h3>
            <p class="text-2xl font-semibold text-gray-900">{{ publicProjectsCount }}</p>
          </div>
        </div>
      </BaseCard>

      <BaseCard>
        <div class="flex items-center">
          <div class="p-3 rounded-full bg-yellow-100">
            <CodeBracketIcon class="w-6 h-6 text-yellow-600" />
          </div>
          <div class="ml-4">
            <h3 class="text-sm font-medium text-gray-500">技術棧</h3>
            <p class="text-2xl font-semibold text-gray-900">{{ technologiesCount }}</p>
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
              placeholder="搜尋作品標題、描述、技術..."
              class="pl-10 pr-4 py-2 w-full border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
            />
          </div>
        </div>
        <div class="flex space-x-2">
          <select
            v-model="selectedTechnology"
            class="px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
          >
            <option value="">所有技術</option>
            <option v-for="tech in technologies" :key="tech" :value="tech">
              {{ tech }}
            </option>
          </select>
          <select
            v-model="statusFilter"
            class="px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
          >
            <option value="">所有狀態</option>
            <option value="featured">精選作品</option>
            <option value="public">公開作品</option>
            <option value="private">私人作品</option>
          </select>
          <select
            v-model="sortBy"
            class="px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
          >
            <option value="title">標題</option>
            <option value="createdAt">建立日期</option>
            <option value="sortOrder">排序</option>
            <option value="createdAt">建立時間</option>
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

    <!-- Projects Grid -->
    <div class="bg-white shadow rounded-lg overflow-x-auto">
      <div v-if="filteredProjects.length === 0" class="p-8 text-center">
        <FolderIcon class="mx-auto h-12 w-12 text-gray-400" />
        <h3 class="mt-2 text-sm font-medium text-gray-900">沒有作品資料</h3>
        <p class="mt-1 text-sm text-gray-500">開始新增您的作品項目。</p>
        <div class="mt-6">
          <BaseButton
            variant="primary"
            @click="showCreateModal = true"
          >
            <PlusIcon class="w-4 h-4 mr-2" />
            新增作品
          </BaseButton>
        </div>
      </div>

      <div v-else>
        <!-- Projects Table -->
        <table class="min-w-full divide-y divide-gray-200">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                作品
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                技術棧
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                時間
              </th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                連結
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
            <tr v-for="project in filteredProjects" :key="project.id" class="hover:bg-gray-50">
              <td class="px-6 py-4 max-w-xs">
                <div class="flex items-center">
                  <div class="flex-shrink-0 h-12 w-12">
                    <div
                      v-if="project.imageUrl"
                      class="h-12 w-12 rounded-lg bg-cover bg-center"
                      :style="{ backgroundImage: `url(${project.imageUrl})` }"
                    ></div>
                    <div
                      v-else
                      class="h-12 w-12 rounded-lg bg-gradient-to-br from-blue-400 to-blue-600 flex items-center justify-center"
                    >
                      <FolderIcon class="h-6 w-6 text-white" />
                    </div>
                  </div>
                  <div class="ml-4">
                    <div class="text-sm font-medium text-gray-900">{{ project.title }}</div>
                    <div class="text-sm text-gray-500 line-clamp-2">{{ project.description }}</div>
                  </div>
                </div>
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <div class="flex flex-wrap gap-1 max-w-xs">
                  <span
                    v-for="tech in getProjectTechnologies(project)"
                    :key="tech"
                    class="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-blue-100 text-blue-800"
                  >
                    {{ tech }}
                  </span>
                </div>
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                <div>{{ formatDate(project.createdAt) }}</div>
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                <div class="flex space-x-2">
                  <a
                    v-if="project.projectUrl"
                    :href="project.projectUrl"
                    target="_blank"
                    class="text-blue-600 hover:text-blue-900"
                    title="查看專案"
                  >
                    <GlobeAltIcon class="w-4 h-4" />
                  </a>
                  <a
                    v-if="project.repositoryUrl"
                    :href="project.repositoryUrl"
                    target="_blank"
                    class="text-gray-600 hover:text-gray-900"
                    title="查看源碼"
                  >
                    <CodeBracketIcon class="w-4 h-4" />
                  </a>
                </div>
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <div class="flex flex-col space-y-1">
                  <span
                    v-if="project.isFeatured"
                    class="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-yellow-100 text-yellow-800"
                  >
                    <StarIcon class="w-3 h-3 mr-1" />
                    精選
                  </span>
                  <span :class="[
                    'inline-flex px-2 py-1 text-xs font-semibold rounded-full',
                    project.isPublic
                      ? 'bg-green-100 text-green-800'
                      : 'bg-gray-100 text-gray-800'
                  ]">
                    {{ project.isPublic ? '公開' : '私人' }}
                  </span>
                </div>
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium w-px">
                <div class="flex justify-end space-x-2">
                  <BaseButton
                    variant="outline"
                    size="small"
                    @click="openAttachments(project)"
                    title="管理附件"
                  >
                    <PaperClipIcon class="w-4 h-4" />
                    <span v-if="attachmentCounts[project.id]" class="ml-1 text-xs text-blue-600">{{ attachmentCounts[project.id] }}</span>
                  </BaseButton>
                  <BaseButton
                    variant="outline"
                    size="small"
                    @click="editProject(project)"
                  >
                    <PencilIcon class="w-4 h-4" />
                  </BaseButton>
                  <BaseButton
                    variant="outline"
                    size="small"
                    @click="deleteProject(project.id)"
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
      title="作品管理"
      max-width="4xl"
    >
      <ProjectForm
        :project="editingProject"
        @save="handleSave"
        @cancel="handleCancel"
      />
    </BaseModal>

    <!-- Attachments Modal -->
    <BaseModal
      :show="showAttachmentsModal"
      @close="showAttachmentsModal = false"
      title="附件管理"
      max-width="2xl"
    >
      <div class="space-y-4">
        <!-- Current Attachments -->
        <div v-if="currentAttachments.length > 0">
          <h4 class="text-sm font-medium text-gray-700 mb-2">目前附件</h4>
          <div class="space-y-2">
            <div
              v-for="att in currentAttachments"
              :key="att.id"
              class="flex items-center gap-3 p-2 border border-gray-200 rounded-lg"
            >
              <span class="text-lg">{{ getFileIcon(att.fileType) }}</span>
              <div class="flex-1 min-w-0">
                <p class="text-sm text-gray-900 truncate">{{ att.fileName }}</p>
                <p class="text-xs text-gray-400">{{ formatFileSize(att.fileSize) }}</p>
              </div>
              <button
                type="button"
                @click="removeAttachment(att.id)"
                class="text-red-500 hover:text-red-700 text-sm"
              >刪除</button>
            </div>
          </div>
        </div>
        <div v-else class="text-sm text-gray-400">尚無附件</div>

        <!-- Add via FilePicker -->
        <div>
          <h4 class="text-sm font-medium text-gray-700 mb-2">新增附件</h4>
          <FilePicker
            v-model="showFilePicker"
            fileType="all"
            @select="onAttachmentSelected"
          />
        </div>

        <!-- Direct Upload -->
        <div>
          <input type="file" ref="attFileInputRef" class="sr-only" @change="handleAttachmentUpload" />
          <BaseButton variant="outline" @click="attFileInputRef?.click()">直接上傳新檔案</BaseButton>
          <span v-if="isAttachmentUploading" class="ml-3 text-sm text-blue-600">上傳中...</span>
        </div>
      </div>
    </BaseModal>

    <!-- Delete Confirmation Modal -->
    <BaseModal
      :show="showDeleteModal"
      @close="showDeleteModal = false"
      title="確認刪除"
    >
      <div class="mt-2">
        <p class="text-sm text-gray-500">
          您確定要刪除這個作品嗎？此操作無法復原。
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
  FolderIcon,
  StarIcon,
  EyeIcon,
  CodeBracketIcon,
  MagnifyingGlassIcon,
  PencilIcon,
  TrashIcon,
  GlobeAltIcon,
  PaperClipIcon
} from '@heroicons/vue/24/outline'
import { usePortfolioStore } from '@/stores/portfolio'
import { useAuthStore } from '@/stores/auth'
import type { Portfolio, PortfolioAttachment } from '@/types/api'
import AdminLayout from '@/components/layout/AdminLayout.vue'
import BaseButton from '@/components/ui/BaseButton.vue'
import BaseCard from '@/components/ui/BaseCard.vue'
import BaseModal from '@/components/ui/BaseModal.vue'
import ProjectForm from '@/components/admin/ProjectForm.vue'
import FilePicker from '@/components/admin/FilePicker.vue'
import portfolioAttachmentService from '@/services/portfolioAttachmentService'
import fileUploadService from '@/services/fileUploadService'
import type { FileUpload } from '@/types/api'

// Stores
const portfolioStore = usePortfolioStore()
const authStore = useAuthStore()

// State
const searchQuery = ref('')
const selectedTechnology = ref('')
const statusFilter = ref('')
const sortBy = ref('title')
const sortOrder = ref<'asc' | 'desc'>('asc')
const showCreateModal = ref(false)
const showDeleteModal = ref(false)
const editingProject = ref<Portfolio | null>(null)
const deletingProjectId = ref<number | null>(null)

// Attachment state
const showAttachmentsModal = ref(false)
const showFilePicker = ref(false)
const attachingProject = ref<Portfolio | null>(null)
const currentAttachments = ref<PortfolioAttachment[]>([])
const attachmentCounts = ref<Record<number, number>>({})
const isAttachmentUploading = ref(false)
const attFileInputRef = ref<HTMLInputElement>()

// Computed
const projects = computed(() => portfolioStore.portfolios)

const technologies = computed(() => {
  const techSet = new Set<string>()
  projects.value.forEach(project => {
    const techs = getProjectTechnologies(project)
    techs.forEach(tech => techSet.add(tech))
  })
  return Array.from(techSet).sort()
})

const featuredProjectsCount = computed(() => {
  return projects.value.filter(project => project.isFeatured).length
})

const publicProjectsCount = computed(() => {
  return projects.value.filter(project => project.isPublic).length
})

const technologiesCount = computed(() => {
  return technologies.value.length
})

const filteredProjects = computed(() => {
  let filtered = projects.value

  // Search filter
  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    filtered = filtered.filter(project =>
      project.title.toLowerCase().includes(query) ||
      project.description?.toLowerCase().includes(query) ||
      getProjectTechnologies(project).some(tech => 
        tech.toLowerCase().includes(query)
      )
    )
  }

  // Technology filter
  if (selectedTechnology.value) {
    filtered = filtered.filter(project =>
      getProjectTechnologies(project).includes(selectedTechnology.value)
    )
  }

  // Status filter
  if (statusFilter.value) {
    switch (statusFilter.value) {
      case 'featured':
        filtered = filtered.filter(project => project.isFeatured)
        break
      case 'public':
        filtered = filtered.filter(project => project.isPublic)
        break
      case 'private':
        filtered = filtered.filter(project => !project.isPublic)
        break
    }
  }

  // Sort
  return filtered.sort((a, b) => {
    let aValue: any, bValue: any

    switch (sortBy.value) {
      case 'title':
        aValue = a.title.toLowerCase()
        bValue = b.title.toLowerCase()
        break
      case 'createdAt':
        aValue = new Date(a.createdAt || '').getTime()
        bValue = new Date(b.createdAt || '').getTime()
        break
      case 'sortOrder':
        aValue = a.sortOrder
        bValue = b.sortOrder
        break
      default:
        aValue = a.title
        bValue = b.title
    }

    if (sortOrder.value === 'asc') {
      return aValue > bValue ? 1 : -1
    } else {
      return aValue < bValue ? 1 : -1
    }
  })
})

// Methods
function getProjectTechnologies(project: Portfolio): string[] {
  const technologies = project.technologies || ''
  return technologies
    .split(/[,;|]/)
    .map(tech => tech.trim())
    .filter(tech => tech.length > 0)
}

function formatDate(dateString?: string): string {
  if (!dateString) return ''
  return new Date(dateString).toLocaleDateString('zh-TW', {
    year: 'numeric',
    month: 'short'
  })
}

function editProject(project: Portfolio) {
  editingProject.value = { ...project }
  showCreateModal.value = true
}

function deleteProject(id: number) {
  deletingProjectId.value = id
  showDeleteModal.value = true
}

async function confirmDelete() {
  if (deletingProjectId.value) {
    try {
      await portfolioStore.deletePortfolio(deletingProjectId.value)
      showDeleteModal.value = false
      deletingProjectId.value = null
    } catch (error) {
      console.error('Delete error:', error)
    }
  }
}

async function handleSave(data: any) {
  try {
    if (editingProject.value) {
      // Update existing
      await portfolioStore.updatePortfolio(editingProject.value.id, data)
    } else {
      // Create new
      await portfolioStore.createPortfolio(data)
    }
    handleCancel()
  } catch (error) {
    console.error('Save error:', error)
  }
}

function handleCancel() {
  showCreateModal.value = false
  editingProject.value = null
}

// Attachment methods
function getFileIcon(fileType: string): string {
  switch (fileType) {
    case 'image': return '🖼'
    case 'pdf': return '📄'
    case 'document': return '📝'
    case 'presentation': return '📊'
    default: return '📁'
  }
}

function formatFileSize(bytes: number): string {
  if (!bytes) return ''
  if (bytes < 1024) return `${bytes}B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(0)}KB`
  return `${(bytes / (1024 * 1024)).toFixed(1)}MB`
}

async function openAttachments(project: Portfolio) {
  attachingProject.value = project
  showAttachmentsModal.value = true
  try {
    const res = await portfolioAttachmentService.getByPortfolio(project.id)
    if (res.success) currentAttachments.value = res.data || []
  } catch { currentAttachments.value = [] }
}

async function onAttachmentSelected(file: FileUpload) {
  if (!attachingProject.value) return
  const backendBase = import.meta.env.VITE_API_BASE_URL?.replace('/api', '') || 'http://localhost:5037'
  const fullUrl = file.fileUrl.startsWith('http') ? file.fileUrl : `${backendBase}${file.fileUrl}`
  try {
    const res = await portfolioAttachmentService.create({
      portfolioId: attachingProject.value.id,
      fileUploadId: file.id,
      fileName: file.fileName,
      fileUrl: fullUrl,
      fileType: file.fileType,
      fileSize: file.fileSize,
      sortOrder: currentAttachments.value.length
    })
    if (res.success && res.data) {
      currentAttachments.value.push(res.data)
      attachmentCounts.value = { ...attachmentCounts.value, [attachingProject.value.id]: currentAttachments.value.length }
    }
  } catch { /* ignore */ }
}

async function handleAttachmentUpload(event: Event) {
  const file = (event.target as HTMLInputElement).files?.[0]
  if (!file || !attachingProject.value) return
  isAttachmentUploading.value = true
  try {
    const formData = new FormData()
    formData.append('file', file)
    const uploadRes = await fileUploadService.upload(formData)
    if (uploadRes.success && uploadRes.data) {
      await onAttachmentSelected(uploadRes.data)
    }
  } catch { /* ignore */ } finally {
    isAttachmentUploading.value = false
  }
}

async function removeAttachment(id: number) {
  if (!confirm('確定刪除此附件？')) return
  try {
    await portfolioAttachmentService.delete(id)
    currentAttachments.value = currentAttachments.value.filter(a => a.id !== id)
    if (attachingProject.value) {
      const count = currentAttachments.value.length
      if (count > 0) attachmentCounts.value = { ...attachmentCounts.value, [attachingProject.value.id]: count }
      else {
        const counts = { ...attachmentCounts.value }
        delete counts[attachingProject.value.id]
        attachmentCounts.value = counts
      }
    }
  } catch { /* ignore */ }
}

// Lifecycle
onMounted(async () => {
  await portfolioStore.fetchPortfolios()
  // Load attachment counts
  for (const p of portfolioStore.portfolios) {
    try {
      const res = await portfolioAttachmentService.getByPortfolio(p.id)
      if (res.success && res.data?.length) {
        attachmentCounts.value = { ...attachmentCounts.value, [p.id]: res.data.length }
      }
    } catch { /* ignore */ }
  }
})
</script>

<style scoped>
.line-clamp-2 {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}
</style>