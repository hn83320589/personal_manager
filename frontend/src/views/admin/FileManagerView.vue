<template>
  <AdminLayout>
    <div class="mb-8">
      <div class="flex justify-between items-center">
        <div>
          <h2 class="text-2xl font-bold text-gray-900">檔案管理</h2>
          <p class="mt-1 text-sm text-gray-600">管理上傳的圖片與文件</p>
        </div>
        <div>
          <input type="file" ref="fileInputRef" class="sr-only" :accept="acceptTypes" @change="handleFileInput" multiple />
          <BaseButton variant="primary" @click="fileInputRef?.click()">
            <span class="mr-2">+</span>上傳檔案
          </BaseButton>
        </div>
      </div>
    </div>

    <!-- Filter Tabs -->
    <div class="flex gap-2 mb-6">
      <button
        v-for="tab in tabs"
        :key="tab.value"
        @click="activeFilter = tab.value"
        :class="['px-4 py-2 rounded-lg text-sm font-medium transition-colors', activeFilter === tab.value ? 'bg-blue-600 text-white' : 'bg-white text-gray-600 border border-gray-200 hover:bg-gray-50']"
      >{{ tab.label }}</button>
    </div>

    <!-- Uploading progress -->
    <div v-if="isUploading" class="mb-4 p-3 bg-blue-50 border border-blue-200 rounded-lg text-sm text-blue-700">
      上傳中，請稍候...
    </div>

    <!-- Files Grid -->
    <div v-if="isLoading" class="text-center py-12 text-gray-400">載入中...</div>

    <div v-else-if="filteredFiles.length > 0" class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-6 gap-4">
      <div
        v-for="file in filteredFiles"
        :key="file.id"
        class="bg-white border border-gray-200 rounded-lg overflow-hidden group hover:shadow-md transition-shadow"
      >
        <!-- Preview -->
        <div class="relative aspect-square bg-gray-50 cursor-pointer" @click="previewFile(file)">
          <img
            v-if="file.fileType === 'image'"
            :src="getFileUrl(file)"
            :alt="file.fileName"
            class="w-full h-full object-cover"
          />
          <div v-else class="w-full h-full flex items-center justify-center text-4xl">
            {{ getFileIcon(file.fileType) }}
          </div>
        </div>

        <!-- Info -->
        <div class="p-2">
          <p class="text-xs text-gray-700 truncate font-medium" :title="file.fileName">{{ file.fileName }}</p>
          <p class="text-xs text-gray-400 mt-0.5">{{ formatFileSize(file.fileSize) }}</p>
        </div>

        <!-- Actions -->
        <div class="px-2 pb-2 flex gap-1">
          <button
            @click="copyLink(file)"
            class="flex-1 text-xs py-1 text-gray-600 border border-gray-200 rounded hover:bg-gray-50 transition-colors"
            :title="copied === file.id ? '已複製！' : '複製連結'"
          >{{ copied === file.id ? '✓' : '複製' }}</button>
          <button
            @click="deleteFile(file)"
            class="text-xs py-1 px-2 text-red-600 border border-red-200 rounded hover:bg-red-50 transition-colors"
            title="刪除"
          >✕</button>
        </div>
      </div>
    </div>

    <div v-else-if="!isLoading" class="text-center py-12 text-gray-400">
      <p class="text-lg mb-2">尚無檔案</p>
      <p class="text-sm">點擊「上傳檔案」按鈕開始上傳</p>
    </div>

    <!-- Image Preview Modal -->
    <div v-if="previewImage" class="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-80" @click="previewImage = null">
      <div class="max-w-4xl max-h-screen p-4">
        <img :src="previewImage" alt="預覽" class="max-w-full max-h-full rounded-lg" />
      </div>
    </div>
  </AdminLayout>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import AdminLayout from '@/components/layout/AdminLayout.vue'
import BaseButton from '@/components/ui/BaseButton.vue'
import fileUploadService from '@/services/fileUploadService'
import type { FileUpload, FileUploadType } from '@/types/api'

const files = ref<FileUpload[]>([])
const isLoading = ref(true)
const isUploading = ref(false)
const activeFilter = ref<FileUploadType | 'all'>('all')
const fileInputRef = ref<HTMLInputElement>()
const copied = ref<number | null>(null)
const previewImage = ref<string | null>(null)

const backendBase = import.meta.env.VITE_API_BASE_URL?.replace('/api', '') || 'http://localhost:5037'

const tabs = [
  { value: 'all', label: '全部' },
  { value: 'image', label: '圖片' },
  { value: 'document', label: '文件' },
  { value: 'presentation', label: '簡報' },
  { value: 'pdf', label: 'PDF' },
] as const

const acceptTypes = '.jpg,.jpeg,.png,.gif,.webp,.pdf,.doc,.docx,.ppt,.pptx'

const filteredFiles = computed(() => {
  if (activeFilter.value === 'all') return files.value
  return files.value.filter(f => f.fileType === activeFilter.value)
})

function getFileUrl(file: FileUpload): string {
  return file.fileUrl.startsWith('http') ? file.fileUrl : `${backendBase}${file.fileUrl}`
}

function getFileIcon(fileType: FileUploadType | string): string {
  switch (fileType) {
    case 'image': return '🖼'
    case 'pdf': return '📄'
    case 'document': return '📝'
    case 'presentation': return '📊'
    default: return '📁'
  }
}

function formatFileSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
}

function previewFile(file: FileUpload) {
  if (file.fileType === 'image') {
    previewImage.value = getFileUrl(file)
  } else {
    window.open(getFileUrl(file), '_blank')
  }
}

async function copyLink(file: FileUpload) {
  try {
    await navigator.clipboard.writeText(getFileUrl(file))
    copied.value = file.id
    setTimeout(() => { copied.value = null }, 2000)
  } catch { /* ignore */ }
}

async function deleteFile(file: FileUpload) {
  if (!confirm(`確定刪除「${file.fileName}」？`)) return
  try {
    await fileUploadService.delete(file.id)
    files.value = files.value.filter(f => f.id !== file.id)
  } catch { /* ignore */ }
}

async function uploadFiles(fileList: FileList) {
  isUploading.value = true
  for (const file of Array.from(fileList)) {
    const formData = new FormData()
    formData.append('file', file)
    try {
      const res = await fileUploadService.upload(formData)
      if (res.success && res.data) files.value.unshift(res.data)
    } catch { /* ignore */ }
  }
  isUploading.value = false
}

function handleFileInput(event: Event) {
  const fileList = (event.target as HTMLInputElement).files
  if (fileList?.length) uploadFiles(fileList)
}

onMounted(async () => {
  isLoading.value = true
  try {
    const res = await fileUploadService.getAll()
    if (res.success) files.value = res.data || []
  } catch {
    files.value = []
  } finally {
    isLoading.value = false
  }
})
</script>
