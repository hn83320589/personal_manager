<template>
  <div>
    <!-- Tabs -->
    <div class="flex border-b border-gray-200 mb-4">
      <button
        type="button"
        @click="activeTab = 'library'"
        :class="['px-4 py-2 text-sm font-medium border-b-2 transition-colors', activeTab === 'library' ? 'border-blue-600 text-blue-600' : 'border-transparent text-gray-500 hover:text-gray-700']"
      >從檔案庫選取</button>
      <button
        type="button"
        @click="activeTab = 'upload'"
        :class="['px-4 py-2 text-sm font-medium border-b-2 transition-colors', activeTab === 'upload' ? 'border-blue-600 text-blue-600' : 'border-transparent text-gray-500 hover:text-gray-700']"
      >直接上傳</button>
    </div>

    <!-- Library Tab -->
    <div v-if="activeTab === 'library'">
      <div v-if="isLoading" class="text-center py-8 text-gray-400 text-sm">載入中...</div>
      <div v-else-if="filteredFiles.length === 0" class="text-center py-8 text-gray-400 text-sm">尚無檔案</div>
      <div v-else class="grid grid-cols-4 gap-2 max-h-64 overflow-y-auto">
        <button
          v-for="file in filteredFiles"
          :key="file.id"
          type="button"
          @click="selectFile(file)"
          class="border border-gray-200 rounded-lg p-1 hover:border-blue-500 hover:bg-blue-50 transition-colors text-left"
          :title="file.fileName"
        >
          <img
            v-if="file.fileType === 'image'"
            :src="getFileUrl(file)"
            :alt="file.fileName"
            class="w-full h-16 object-cover rounded"
          />
          <div v-else class="w-full h-16 flex flex-col items-center justify-center text-gray-400 bg-gray-50 rounded">
            <span class="text-2xl">{{ getFileIcon(file.fileType) }}</span>
          </div>
          <p class="text-xs text-gray-600 truncate mt-1 px-0.5">{{ file.fileName }}</p>
        </button>
      </div>
    </div>

    <!-- Upload Tab -->
    <div v-if="activeTab === 'upload'">
      <div
        class="border-2 border-dashed border-gray-300 rounded-lg p-8 text-center"
        @drop.prevent="handleDrop"
        @dragover.prevent
        :class="isDragging ? 'border-blue-500 bg-blue-50' : ''"
        @dragenter="isDragging = true"
        @dragleave="isDragging = false"
      >
        <input
          type="file"
          ref="fileInputRef"
          class="sr-only"
          :accept="acceptTypes"
          @change="handleFileInput"
        />
        <p class="text-sm text-gray-600">拖放檔案至此，或</p>
        <button
          type="button"
          @click="fileInputRef?.click()"
          class="mt-2 px-3 py-1.5 text-sm text-white bg-blue-600 rounded-md hover:bg-blue-700"
        >選擇檔案</button>
        <p class="mt-2 text-xs text-gray-400">最大 50MB</p>
      </div>

      <div v-if="isUploading" class="mt-3 text-center text-sm text-blue-600">上傳中...</div>
      <div v-if="uploadError" class="mt-3 text-center text-sm text-red-600">{{ uploadError }}</div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import fileUploadService from '@/services/fileUploadService'
import type { FileUpload, FileUploadType } from '@/types/api'

const props = withDefaults(defineProps<{
  modelValue: boolean
  fileType?: 'image' | 'all'
}>(), {
  fileType: 'all'
})

const emit = defineEmits<{
  (e: 'update:modelValue', value: boolean): void
  (e: 'select', file: FileUpload): void
}>()

const activeTab = ref<'library' | 'upload'>('library')
const files = ref<FileUpload[]>([])
const isLoading = ref(false)
const isUploading = ref(false)
const uploadError = ref('')
const isDragging = ref(false)
const fileInputRef = ref<HTMLInputElement>()

const backendBase = import.meta.env.VITE_API_BASE_URL?.replace('/api', '') || 'http://localhost:5037'

function getFileUrl(file: FileUpload): string {
  return file.fileUrl.startsWith('http') ? file.fileUrl : `${backendBase}${file.fileUrl}`
}

const filteredFiles = computed(() => {
  if (props.fileType === 'image') return files.value.filter(f => f.fileType === 'image')
  return files.value
})

const acceptTypes = computed(() => {
  if (props.fileType === 'image') return 'image/*'
  return '.jpg,.jpeg,.png,.gif,.webp,.pdf,.doc,.docx,.ppt,.pptx'
})

function getFileIcon(fileType: FileUploadType | string): string {
  switch (fileType) {
    case 'pdf': return '📄'
    case 'document': return '📝'
    case 'presentation': return '📊'
    default: return '📁'
  }
}

function selectFile(file: FileUpload) {
  emit('select', file)
}

async function uploadFile(file: File) {
  isUploading.value = true
  uploadError.value = ''
  try {
    const formData = new FormData()
    formData.append('file', file)
    const res = await fileUploadService.upload(formData)
    if (res.success && res.data) {
      files.value.unshift(res.data)
      emit('select', res.data)
      activeTab.value = 'library'
    }
  } catch {
    uploadError.value = '上傳失敗，請重試'
  } finally {
    isUploading.value = false
  }
}

function handleFileInput(event: Event) {
  const file = (event.target as HTMLInputElement).files?.[0]
  if (file) uploadFile(file)
}

function handleDrop(event: DragEvent) {
  isDragging.value = false
  const file = event.dataTransfer?.files?.[0]
  if (file) uploadFile(file)
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
