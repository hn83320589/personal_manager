<template>
  <AdminLayout>
    <!-- Page Header -->
    <div class="mb-8">
      <div class="flex justify-between items-center">
        <div>
          <h2 class="text-2xl font-bold text-gray-900">文章管理</h2>
          <p class="mt-1 text-sm text-gray-600">
            管理部落格文章、分類和發布狀態
          </p>
        </div>
        <div class="flex space-x-3">
          <BaseButton
            variant="outline"
            @click="showCategoryModal = true"
          >
            <TagIcon class="w-4 h-4 mr-2" />
            管理分類
          </BaseButton>
          <BaseButton
            variant="outline"
            @click="showBatchModal = true"
            :disabled="selectedPosts.length === 0"
          >
            <Squares2X2Icon class="w-4 h-4 mr-2" />
            批量操作 ({{ selectedPosts.length }})
          </BaseButton>
          <BaseButton
            variant="primary"
            @click="createNewPost"
          >
            <PlusIcon class="w-4 h-4 mr-2" />
            撰寫文章
          </BaseButton>
        </div>
      </div>
    </div>

    <!-- Stats Cards -->
    <div class="grid grid-cols-1 md:grid-cols-4 gap-6 mb-6">
      <BaseCard>
        <div class="flex items-center">
          <div class="p-3 rounded-full bg-blue-100">
            <DocumentTextIcon class="w-6 h-6 text-blue-600" />
          </div>
          <div class="ml-4">
            <h3 class="text-sm font-medium text-gray-500">總文章數</h3>
            <p class="text-2xl font-semibold text-gray-900">{{ totalPosts }}</p>
          </div>
        </div>
      </BaseCard>

      <BaseCard>
        <div class="flex items-center">
          <div class="p-3 rounded-full bg-green-100">
            <EyeIcon class="w-6 h-6 text-green-600" />
          </div>
          <div class="ml-4">
            <h3 class="text-sm font-medium text-gray-500">已發布</h3>
            <p class="text-2xl font-semibold text-gray-900">{{ publishedPosts }}</p>
            <p class="text-xs text-gray-500">{{ publishedRate.toFixed(1) }}%</p>
          </div>
        </div>
      </BaseCard>

      <BaseCard>
        <div class="flex items-center">
          <div class="p-3 rounded-full bg-yellow-100">
            <DocumentIcon class="w-6 h-6 text-yellow-600" />
          </div>
          <div class="ml-4">
            <h3 class="text-sm font-medium text-gray-500">草稿</h3>
            <p class="text-2xl font-semibold text-gray-900">{{ draftPosts }}</p>
          </div>
        </div>
      </BaseCard>

      <BaseCard>
        <div class="flex items-center">
          <div class="p-3 rounded-full bg-purple-100">
            <HeartIcon class="w-6 h-6 text-purple-600" />
          </div>
          <div class="ml-4">
            <h3 class="text-sm font-medium text-gray-500">總觀看數</h3>
            <p class="text-2xl font-semibold text-gray-900">{{ totalViews.toLocaleString() }}</p>
          </div>
        </div>
      </BaseCard>
    </div>

    <!-- Filters and Controls -->
    <div class="mb-6">
      <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <!-- Search and Filters -->
        <div class="flex flex-col sm:flex-row gap-4 flex-1">
          <div class="relative flex-1 max-w-md">
            <MagnifyingGlassIcon class="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-gray-400" />
            <input
              v-model="searchQuery"
              type="text"
              placeholder="搜尋文章標題、內容..."
              class="pl-10 pr-4 py-2 w-full border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
            />
          </div>
          
          <div class="flex gap-2">
            <select
              v-model="selectedStatus"
              class="px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
            >
              <option value="">所有狀態</option>
              <option value="Draft">草稿</option>
              <option value="Published">已發布</option>
              <option value="Archived">已封存</option>
            </select>

            <select
              v-model="selectedCategory"
              class="px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
            >
              <option value="">所有分類</option>
              <option v-for="category in categories" :key="category" :value="category">
                {{ category }}
              </option>
            </select>

            <!-- Author filter removed: author field no longer exists on BlogPost -->
          </div>
        </div>

        <!-- Tag filter chips -->
        <div v-if="allTags.length > 0" class="flex flex-wrap gap-2 pt-1">
          <button
            v-for="tag in allTags"
            :key="tag"
            @click="selectedTag = selectedTag === tag ? '' : tag"
            :class="['text-xs px-3 py-1 rounded-full font-medium transition-colors', selectedTag === tag ? 'bg-blue-600 text-white' : 'bg-gray-100 text-gray-600 hover:bg-gray-200']"
          >#{{ tag }}</button>
        </div>

        <!-- View and Sort Options -->
        <div class="flex items-center space-x-4">
          <div class="flex items-center space-x-2">
            <label class="text-sm text-gray-700">排序:</label>
            <select
              v-model="sortBy"
              class="px-2 py-1 text-sm border border-gray-300 rounded focus:ring-blue-500 focus:border-blue-500"
            >
              <option value="createdAt">建立時間</option>
              <option value="updatedAt">更新時間</option>
              <option value="publishedAt">發布時間</option>
              <option value="title">標題</option>
              <option value="viewCount">觀看數</option>
            </select>
          </div>

          <div class="flex space-x-1 bg-gray-100 p-1 rounded-lg">
            <button
              @click="viewMode = 'table'"
              :class="[
                'px-3 py-1 text-sm font-medium rounded transition-colors',
                viewMode === 'table'
                  ? 'bg-white text-gray-900 shadow'
                  : 'text-gray-600 hover:text-gray-900'
              ]"
            >
              <TableCellsIcon class="w-4 h-4" />
            </button>
            <button
              @click="viewMode = 'grid'"
              :class="[
                'px-3 py-1 text-sm font-medium rounded transition-colors',
                viewMode === 'grid'
                  ? 'bg-white text-gray-900 shadow'
                  : 'text-gray-600 hover:text-gray-900'
              ]"
            >
              <Squares2X2Icon class="w-4 h-4" />
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Posts Content -->
    <div class="bg-white shadow rounded-lg overflow-hidden">
      <!-- Table View -->
      <div v-if="viewMode === 'table'">
        <BlogTableView
          :posts="filteredAndSortedPosts"
          :loading="loading"
          :selectedPosts="selectedPosts"
          @toggle-select="togglePostSelection"
          @edit-post="editPost"
          @delete-post="deletePost"
          @duplicate-post="duplicatePost"
          @toggle-publish="togglePublish"
          @preview-post="previewPost"
        />
      </div>

      <!-- Grid View -->
      <div v-else-if="viewMode === 'grid'">
        <BlogGridView
          :posts="filteredAndSortedPosts"
          :loading="loading"
          :selectedPosts="selectedPosts"
          @toggle-select="togglePostSelection"
          @edit-post="editPost"
          @delete-post="deletePost"
          @duplicate-post="duplicatePost"
          @toggle-publish="togglePublish"
          @preview-post="previewPost"
        />
      </div>
    </div>

    <!-- Category Management Modal -->
    <BaseModal
      :show="showCategoryModal"
      @close="showCategoryModal = false"
      title="管理文章分類"
      max-width="2xl"
    >
      <CategoryManagement
        :categories="categoriesWithStats"
        @save="handleCategorySave"
        @delete="handleCategoryDelete"
      />
    </BaseModal>

    <!-- Batch Operations Modal -->
    <BaseModal
      :show="showBatchModal"
      @close="showBatchModal = false"
      title="批量操作"
      max-width="md"
    >
      <div class="space-y-4">
        <p class="text-sm text-gray-600">
          已選擇 {{ selectedPosts.length }} 篇文章
        </p>
        
        <div class="space-y-3">
          <BaseButton
            variant="outline"
            @click="batchUpdateStatus('Published')"
            class="w-full justify-start"
          >
            <EyeIcon class="w-4 h-4 mr-2" />
            批量發布
          </BaseButton>
          
          <BaseButton
            variant="outline"
            @click="batchUpdateStatus('Draft')"
            class="w-full justify-start"
          >
            <DocumentIcon class="w-4 h-4 mr-2" />
            轉為草稿
          </BaseButton>
          
          <BaseButton
            variant="outline"
            @click="batchUpdateCategory"
            class="w-full justify-start"
          >
            <TagIcon class="w-4 h-4 mr-2" />
            更改分類
          </BaseButton>
          
          <BaseButton
            variant="outline"
            @click="batchExport"
            class="w-full justify-start"
          >
            <ArrowDownTrayIcon class="w-4 h-4 mr-2" />
            匯出選中文章
          </BaseButton>
          
          <BaseButton
            variant="outline"
            @click="batchDelete"
            class="w-full justify-start text-red-600 hover:text-red-700"
          >
            <TrashIcon class="w-4 h-4 mr-2" />
            刪除選中文章
          </BaseButton>
        </div>
      </div>
    </BaseModal>

    <!-- Batch Update Category Modal -->
    <BaseModal
      :show="showBatchCategoryModal"
      @close="showBatchCategoryModal = false"
      title="批量更改分類"
      max-width="sm"
    >
      <div class="space-y-3">
        <p class="text-sm text-gray-600">將 {{ selectedPosts.length }} 篇文章的分類改為：</p>
        <input
          v-model="batchCategoryValue"
          type="text"
          list="batch-categories"
          class="form-input w-full"
          placeholder="輸入或選擇分類名稱"
        />
        <datalist id="batch-categories">
          <option v-for="cat in categories" :key="cat" :value="cat">{{ cat }}</option>
        </datalist>
      </div>
      <div class="mt-6 flex justify-end space-x-3">
        <BaseButton variant="outline" @click="showBatchCategoryModal = false">取消</BaseButton>
        <BaseButton variant="primary" :disabled="!batchCategoryValue.trim()" @click="confirmBatchUpdateCategory">確認</BaseButton>
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
          您確定要刪除這{{ deleteType === 'single' ? '篇' : `${selectedPosts.length}篇` }}文章嗎？此操作無法復原。
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
import { useRouter } from 'vue-router'
import {
  PlusIcon,
  MagnifyingGlassIcon,
  DocumentTextIcon,
  EyeIcon,
  DocumentIcon,
  HeartIcon,
  TagIcon,
  Squares2X2Icon,
  TableCellsIcon,
  TrashIcon,
  ArrowDownTrayIcon
} from '@heroicons/vue/24/outline'
import { useBlogStore } from '@/stores/blog'
import { useAuthStore } from '@/stores/auth'
import type { BlogPost } from '@/types/api'
import AdminLayout from '@/components/layout/AdminLayout.vue'
import BaseButton from '@/components/ui/BaseButton.vue'
import BaseCard from '@/components/ui/BaseCard.vue'
import BaseModal from '@/components/ui/BaseModal.vue'
import BlogTableView from '@/components/blog/BlogTableView.vue'
import BlogGridView from '@/components/blog/BlogGridView.vue'
import CategoryManagement from '@/components/blog/CategoryManagement.vue'

// Router
const router = useRouter()

// Stores
const blogStore = useBlogStore()
const authStore = useAuthStore()

// State
const searchQuery = ref('')
const selectedStatus = ref<'Draft' | 'Published' | 'Archived' | ''>('')
const selectedCategory = ref('')
const selectedTag = ref('')
const sortBy = ref<'createdAt' | 'updatedAt' | 'publishedAt' | 'title' | 'viewCount'>('updatedAt')
const viewMode = ref<'table' | 'grid'>('table')
const loading = ref(false)
const showCategoryModal = ref(false)
const showBatchModal = ref(false)
const showDeleteModal = ref(false)
const showBatchCategoryModal = ref(false)
const batchCategoryValue = ref('')
const selectedPosts = ref<number[]>([])
const deletingId = ref<number | null>(null)
const deleteType = ref<'single' | 'batch'>('single')

// Computed
const posts = computed(() => blogStore.posts)

const totalPosts = computed(() => posts.value.length)

const publishedPosts = computed(() => {
  return posts.value.filter(post => post.status === 'Published').length
})

const publishedRate = computed(() => {
  return totalPosts.value > 0 ? (publishedPosts.value / totalPosts.value) * 100 : 0
})

const draftPosts = computed(() => {
  return posts.value.filter(post => post.status === 'Draft').length
})

const totalViews = computed(() => {
  return posts.value.reduce((sum, post) => sum + (post.viewCount || 0), 0)
})

const categories = computed(() => {
  const categorySet = new Set(posts.value.map(post => post.category).filter(Boolean))
  return Array.from(categorySet).sort()
})

const allTags = computed(() => {
  const tags = new Set<string>()
  posts.value.forEach(p => {
    if (p.tags) p.tags.forEach(t => { const s = t.trim(); if (s) tags.add(s) })
  })
  return Array.from(tags).sort()
})

const categoriesWithStats = computed(() => {
  return categories.value.map((category: string) => ({
    name: category,
    count: posts.value.filter(post => post.category === category).length,
    publishedCount: posts.value.filter(post => post.category === category && post.status === 'Published').length
  }))
})

const filteredAndSortedPosts = computed(() => {
  let filtered = posts.value

  // Search filter
  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    filtered = filtered.filter(post =>
      post.title.toLowerCase().includes(query) ||
      post.content?.toLowerCase().includes(query) ||
      post.summary?.toLowerCase().includes(query) ||
      post.tags?.join(',').toLowerCase().includes(query)
    )
  }

  // Status filter
  if (selectedStatus.value) {
    filtered = filtered.filter(post => post.status === selectedStatus.value)
  }

  // Category filter
  if (selectedCategory.value) {
    filtered = filtered.filter(post => post.category === selectedCategory.value)
  }

  // Tag filter
  if (selectedTag.value) {
    filtered = filtered.filter(post =>
      post.tags?.includes(selectedTag.value)
    )
  }

  // Sort
  return filtered.sort((a, b) => {
    switch (sortBy.value) {
      case 'createdAt':
        return new Date(b.createdAt || '').getTime() - new Date(a.createdAt || '').getTime()
      case 'updatedAt':
        return new Date(b.updatedAt || '').getTime() - new Date(a.updatedAt || '').getTime()
      case 'publishedAt':
        if (!a.publishedAt) return 1
        if (!b.publishedAt) return -1
        return new Date(b.publishedAt).getTime() - new Date(a.publishedAt).getTime()
      case 'title':
        return a.title.localeCompare(b.title)
      case 'viewCount':
        return (b.viewCount || 0) - (a.viewCount || 0)
      default:
        return 0
    }
  })
})

// Methods
function togglePostSelection(postId: number) {
  const index = selectedPosts.value.indexOf(postId)
  if (index > -1) {
    selectedPosts.value.splice(index, 1)
  } else {
    selectedPosts.value.push(postId)
  }
}

function createNewPost() {
  router.push('/admin/blog/editor')
}

function editPost(post: BlogPost) {
  router.push(`/admin/blog/editor/${post.id}`)
}

function deletePost(id: number) {
  deletingId.value = id
  deleteType.value = 'single'
  showDeleteModal.value = true
}

function duplicatePost(post: BlogPost) {
  const duplicated = {
    ...post,
    id: undefined,
    title: `${post.title} (複製)`,
    status: 'Draft' as const,
    publishedAt: undefined,
    createdAt: undefined,
    updatedAt: undefined
  }
  
  blogStore.createPost(duplicated).then((newPost) => {
    if (newPost) router.push(`/admin/blog/editor/${newPost.id}`)
  })
}

async function togglePublish(post: BlogPost) {
  const newStatus = post.status === 'Published' ? 'Draft' : 'Published'
  await blogStore.updatePost(post.id, {
    status: newStatus,
    publishedAt: newStatus === 'Published' ? new Date().toISOString() : undefined
  })
}

function previewPost(post: BlogPost) {
  const username = authStore.user?.username
  if (!username || !post.slug) return
  window.open(`/@${username}/blog/${post.slug}`, '_blank')
}

async function batchUpdateStatus(status: 'Draft' | 'Published' | 'Archived') {
  try {
    await Promise.all(
      selectedPosts.value.map(postId =>
        blogStore.updatePost(postId, {
          status,
          publishedAt: status === 'Published' ? new Date().toISOString() : undefined
        })
      )
    )
    selectedPosts.value = []
    showBatchModal.value = false
  } catch (error) {
    console.error('Batch status update error:', error)
  }
}

function batchUpdateCategory() {
  batchCategoryValue.value = ''
  showBatchModal.value = false
  showBatchCategoryModal.value = true
}

async function confirmBatchUpdateCategory() {
  const category = batchCategoryValue.value.trim()
  if (!category) return
  try {
    await Promise.all(
      selectedPosts.value.map(postId => blogStore.updatePost(postId, { category }))
    )
    selectedPosts.value = []
  } catch (error) {
    console.error('Batch category update error:', error)
  }
  showBatchCategoryModal.value = false
}

function batchExport() {
  const selectedPostData = posts.value.filter(post => selectedPosts.value.includes(post.id))
  const exportData = selectedPostData.map(post => ({
    title: post.title,
    content: post.content,
    category: post.category,
    tags: post.tags,
    status: post.status,
    publishedAt: post.publishedAt,
    viewCount: post.viewCount
  }))
  
  // Create and download file
  const blob = new Blob([JSON.stringify(exportData, null, 2)], { type: 'application/json' })
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = `blog-posts-export-${new Date().toISOString().split('T')[0]}.json`
  document.body.appendChild(a)
  a.click()
  document.body.removeChild(a)
  URL.revokeObjectURL(url)
  
  showBatchModal.value = false
}

function batchDelete() {
  if (selectedPosts.value.length === 0) return
  
  deleteType.value = 'batch'
  showBatchModal.value = false
  showDeleteModal.value = true
}

async function confirmDelete() {
  try {
    if (deleteType.value === 'single' && deletingId.value) {
      await blogStore.deletePost(deletingId.value)
      deletingId.value = null
    } else if (deleteType.value === 'batch') {
      await Promise.all(
        selectedPosts.value.map(postId => blogStore.deletePost(postId))
      )
      selectedPosts.value = []
    }
    showDeleteModal.value = false
  } catch (error) {
    console.error('Delete error:', error)
  }
}

async function handleCategorySave(categoryData: { name: string; oldName?: string }) {
  if (!categoryData.oldName) return  // creating new standalone category: no-op (categories derive from posts)
  const oldName = categoryData.oldName
  const newName = categoryData.name
  if (oldName === newName) return
  try {
    const postsToRename = posts.value.filter(p => p.category === oldName)
    await Promise.all(postsToRename.map(p => blogStore.updatePost(p.id, { category: newName })))
  } catch (error) {
    console.error('Category rename error:', error)
  }
}

async function handleCategoryDelete(categoryName: string) {
  try {
    const postsToUpdate = posts.value.filter(p => p.category === categoryName)
    await Promise.all(postsToUpdate.map(p => blogStore.updatePost(p.id, { category: '' })))
  } catch (error) {
    console.error('Category delete error:', error)
  }
}

// Lifecycle
onMounted(async () => {
  await blogStore.fetchPosts()
})
</script>