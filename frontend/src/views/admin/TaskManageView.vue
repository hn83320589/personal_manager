<template>
  <AdminLayout>
    <!-- Page Header -->
    <div class="mb-8">
      <div class="flex justify-between items-center">
        <div>
          <h2 class="text-2xl font-bold text-gray-900">待辦事項管理</h2>
          <p class="mt-1 text-sm text-gray-600">
            管理個人待辦事項，設定優先級和追蹤完成狀態
          </p>
        </div>
        <div class="flex space-x-3">
          <BaseButton
            variant="outline"
            @click="showBatchModal = true"
            :disabled="selectedTasks.length === 0"
          >
            <Squares2X2Icon class="w-4 h-4 mr-2" />
            批量操作 ({{ selectedTasks.length }})
          </BaseButton>
          <BaseButton
            variant="primary"
            @click="showCreateModal = true"
          >
            <PlusIcon class="w-4 h-4 mr-2" />
            新增待辦事項
          </BaseButton>
        </div>
      </div>
    </div>

    <!-- Stats Cards -->
    <div class="grid grid-cols-1 md:grid-cols-4 gap-6 mb-6">
      <BaseCard>
        <div class="flex items-center">
          <div class="p-3 rounded-full bg-blue-100">
            <ListBulletIcon class="w-6 h-6 text-blue-600" />
          </div>
          <div class="ml-4">
            <h3 class="text-sm font-medium text-gray-500">總待辦事項</h3>
            <p class="text-2xl font-semibold text-gray-900">{{ totalTasks }}</p>
          </div>
        </div>
      </BaseCard>

      <BaseCard>
        <div class="flex items-center">
          <div class="p-3 rounded-full bg-green-100">
            <CheckCircleIcon class="w-6 h-6 text-green-600" />
          </div>
          <div class="ml-4">
            <h3 class="text-sm font-medium text-gray-500">已完成</h3>
            <p class="text-2xl font-semibold text-gray-900">{{ completedTasks }}</p>
            <p class="text-xs text-gray-500">{{ completionRate.toFixed(1) }}%</p>
          </div>
        </div>
      </BaseCard>

      <BaseCard>
        <div class="flex items-center">
          <div class="p-3 rounded-full bg-red-100">
            <ExclamationTriangleIcon class="w-6 h-6 text-red-600" />
          </div>
          <div class="ml-4">
            <h3 class="text-sm font-medium text-gray-500">逾期任務</h3>
            <p class="text-2xl font-semibold text-gray-900">{{ overdueTasks }}</p>
          </div>
        </div>
      </BaseCard>

      <BaseCard>
        <div class="flex items-center">
          <div class="p-3 rounded-full bg-yellow-100">
            <ClockIcon class="w-6 h-6 text-yellow-600" />
          </div>
          <div class="ml-4">
            <h3 class="text-sm font-medium text-gray-500">今日任務</h3>
            <p class="text-2xl font-semibold text-gray-900">{{ todayTasks }}</p>
          </div>
        </div>
      </BaseCard>
    </div>

    <!-- Filters and View Options -->
    <div class="mb-6">
      <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <!-- Search and Filters -->
        <div class="flex flex-col sm:flex-row gap-4 flex-1">
          <div class="relative flex-1 max-w-md">
            <MagnifyingGlassIcon class="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-gray-400" />
            <input
              v-model="searchQuery"
              type="text"
              placeholder="搜尋待辦事項..."
              class="pl-10 pr-4 py-2 w-full border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
            />
          </div>
          
          <div class="flex gap-2">
            <select
              v-model="selectedStatus"
              class="px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
            >
              <option value="">所有狀態</option>
              <option value="Pending">待處理</option>
              <option value="InProgress">進行中</option>
              <option value="Completed">已完成</option>
            </select>

            <select
              v-model="selectedPriority"
              class="px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
            >
              <option value="">所有優先級</option>
              <option value="Low">低</option>
              <option value="Medium">中</option>
              <option value="High">高</option>
            </select>

          </div>
        </div>

        <!-- View Options -->
        <div class="flex items-center space-x-4">
          <div class="flex items-center space-x-2">
            <label class="text-sm text-gray-700">排序:</label>
            <select
              v-model="sortBy"
              class="px-2 py-1 text-sm border border-gray-300 rounded focus:ring-blue-500 focus:border-blue-500"
            >
              <option value="createdAt">建立時間</option>
              <option value="dueDate">到期日</option>
              <option value="priority">優先級</option>
              <option value="title">標題</option>
              <option value="status">狀態</option>
            </select>
          </div>

          <div class="flex space-x-1 bg-gray-100 p-1 rounded-lg">
            <button
              @click="viewMode = 'list'"
              :class="[
                'px-3 py-1 text-sm font-medium rounded transition-colors',
                viewMode === 'list'
                  ? 'bg-white text-gray-900 shadow'
                  : 'text-gray-600 hover:text-gray-900'
              ]"
            >
              <ListBulletIcon class="w-4 h-4" />
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
            <button
              @click="viewMode = 'kanban'"
              :class="[
                'px-3 py-1 text-sm font-medium rounded transition-colors',
                viewMode === 'kanban'
                  ? 'bg-white text-gray-900 shadow'
                  : 'text-gray-600 hover:text-gray-900'
              ]"
            >
              <RectangleStackIcon class="w-4 h-4" />
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Tasks Content -->
    <div class="bg-white shadow rounded-lg overflow-hidden">
      <!-- List View -->
      <div v-if="viewMode === 'list'">
        <TaskListView
          :tasks="filteredAndSortedTasks"
          :loading="loading"
          :selectedTasks="selectedTasks"
          @toggle-select="toggleTaskSelection"
          @toggle-complete="toggleTaskComplete"
          @edit-task="editTask"
          @delete-task="deleteTask"
          @duplicate-task="duplicateTask"
        />
      </div>

      <!-- Grid View -->
      <div v-else-if="viewMode === 'grid'">
        <TaskGridView
          :tasks="filteredAndSortedTasks"
          :loading="loading"
          :selectedTasks="selectedTasks"
          @toggle-select="toggleTaskSelection"
          @toggle-complete="toggleTaskComplete"
          @edit-task="editTask"
          @delete-task="deleteTask"
          @duplicate-task="duplicateTask"
        />
      </div>

      <!-- Kanban View -->
      <div v-else-if="viewMode === 'kanban'">
        <TaskKanbanView
          :tasks="filteredAndSortedTasks"
          :loading="loading"
          @toggle-complete="toggleTaskComplete"
          @edit-task="editTask"
          @delete-task="deleteTask"
          @move-task="moveTask"
        />
      </div>
    </div>

    <!-- Create/Edit Task Modal -->
    <BaseModal
      :show="showCreateModal"
      @close="showCreateModal = false"
      title="待辦事項管理"
      max-width="3xl"
    >
      <TaskForm
        :task="editingTask"
        @save="handleSave"
        @cancel="handleCancel"
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
          已選擇 {{ selectedTasks.length }} 個待辦事項
        </p>
        
        <div class="space-y-3">
          <BaseButton
            variant="outline"
            @click="batchUpdateStatus('Completed')"
            class="w-full justify-start"
          >
            <CheckCircleIcon class="w-4 h-4 mr-2" />
            標記為已完成
          </BaseButton>
          
          <BaseButton
            variant="outline"
            @click="batchUpdatePriority('High')"
            class="w-full justify-start"
          >
            <ExclamationTriangleIcon class="w-4 h-4 mr-2" />
            設為高優先級
          </BaseButton>
          
          <BaseButton
            variant="outline"
            @click="batchDelete"
            class="w-full justify-start text-red-600 hover:text-red-700"
          >
            <TrashIcon class="w-4 h-4 mr-2" />
            刪除選中項目
          </BaseButton>
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
          您確定要刪除這{{ deleteType === 'single' ? '個' : `${selectedTasks.length}個` }}待辦事項嗎？此操作無法復原。
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
  MagnifyingGlassIcon,
  ListBulletIcon,
  Squares2X2Icon,
  RectangleStackIcon,
  CheckCircleIcon,
  ExclamationTriangleIcon,
  ClockIcon,
  TrashIcon,
  TagIcon
} from '@heroicons/vue/24/outline'
import { useTaskStore } from '@/stores/task'
import type { TodoItem, TodoStatus } from '@/types/api'
import AdminLayout from '@/components/layout/AdminLayout.vue'
import BaseButton from '@/components/ui/BaseButton.vue'
import BaseCard from '@/components/ui/BaseCard.vue'
import BaseModal from '@/components/ui/BaseModal.vue'
import TaskForm from '@/components/admin/TaskForm.vue'
import TaskListView from '@/components/task/TaskListView.vue'
import TaskGridView from '@/components/task/TaskGridView.vue'
import TaskKanbanView from '@/components/task/TaskKanbanView.vue'

// Stores
const taskStore = useTaskStore()

// State
const searchQuery = ref('')
const selectedStatus = ref('')
const selectedPriority = ref('')
const selectedCategory = ref('') // kept for filter compatibility
const sortBy = ref<'createdAt' | 'dueDate' | 'priority' | 'title' | 'status'>('createdAt')
const viewMode = ref<'list' | 'grid' | 'kanban'>('list')
const loading = ref(false)
const showCreateModal = ref(false)
const showBatchModal = ref(false)
const showDeleteModal = ref(false)
const editingTask = ref<TodoItem | null>(null)
const selectedTasks = ref<number[]>([])
const deletingId = ref<number | null>(null)
const deleteType = ref<'single' | 'batch'>('single')

// Computed
const tasks = computed(() => taskStore.todoItems)

const totalTasks = computed(() => tasks.value.length)

const completedTasks = computed(() => {
  return tasks.value.filter(task => task.status === 'Completed' as TodoStatus).length
})

const completionRate = computed(() => {
  return totalTasks.value > 0 ? (completedTasks.value / totalTasks.value) * 100 : 0
})

const overdueTasks = computed(() => {
  const today = new Date()
  return tasks.value.filter(task => 
    task.dueDate && new Date(task.dueDate) < today && task.status !== 'Completed' as TodoStatus
  ).length
})

const todayTasks = computed(() => {
  const today = new Date().toISOString().split('T')[0]
  return tasks.value.filter(task => 
    task.dueDate && task.dueDate.split('T')[0] === today
  ).length
})

const categories = computed(() => {
  return [] as string[]
})

const filteredAndSortedTasks = computed(() => {
  let filtered = tasks.value

  // Search filter
  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    filtered = filtered.filter(task =>
      task.title.toLowerCase().includes(query) ||
      task.description?.toLowerCase().includes(query)
    )
  }

  // Status filter
  if (selectedStatus.value) {
    filtered = filtered.filter(task => task.status === selectedStatus.value)
  }

  // Priority filter
  if (selectedPriority.value) {
    filtered = filtered.filter(task => task.priority === selectedPriority.value)
  }


  // Sort
  return filtered.sort((a, b) => {
    switch (sortBy.value) {
      case 'createdAt':
        return new Date(b.createdAt || '').getTime() - new Date(a.createdAt || '').getTime()
      case 'dueDate':
        if (!a.dueDate) return 1
        if (!b.dueDate) return -1
        return new Date(a.dueDate).getTime() - new Date(b.dueDate).getTime()
      case 'priority':
        return Number(a.priority) - Number(b.priority)
      case 'title':
        return a.title.localeCompare(b.title)
      case 'status':
        return Number(a.status) - Number(b.status)
      default:
        return 0
    }
  })
})

// Methods
function toggleTaskSelection(taskId: number) {
  const index = selectedTasks.value.indexOf(taskId)
  if (index > -1) {
    selectedTasks.value.splice(index, 1)
  } else {
    selectedTasks.value.push(taskId)
  }
}

async function toggleTaskComplete(task: TodoItem) {
  const newStatus = task.status === 'Completed' as TodoStatus ? 'Pending' as TodoStatus : 'Completed' as TodoStatus
  await taskStore.updateTodo(task.id, {
    status: newStatus,
    completedAt: newStatus === 'Completed' as TodoStatus ? new Date().toISOString() : undefined
  })
}

function editTask(task: TodoItem) {
  editingTask.value = { ...task }
  showCreateModal.value = true
}

function deleteTask(id: number) {
  deletingId.value = id
  deleteType.value = 'single'
  showDeleteModal.value = true
}

function duplicateTask(task: TodoItem) {
  const duplicated = {
    ...task,
    id: 0,
    title: `${task.title} (複製)`,
    status: 'Pending' as TodoStatus,
    createdAt: undefined,
    completedAt: undefined
  }
  editingTask.value = duplicated
  showCreateModal.value = true
}

function moveTask(taskId: number, newStatus: string) {
  const statusMap: Record<string, TodoStatus> = {
    'pending': 'Pending',
    'inprogress': 'InProgress',
    'completed': 'Completed',
  }

  const status = statusMap[newStatus.toLowerCase()] || 'Pending'
  taskStore.updateTodo(taskId, { status })
}

async function handleSave(data: any) {
  try {
    if (editingTask.value && editingTask.value.id) {
      await taskStore.updateTodo(editingTask.value.id, data)
    } else {
      await taskStore.createTodo(data)
    }
    handleCancel()
  } catch (error) {
    console.error('Save error:', error)
  }
}

function handleCancel() {
  showCreateModal.value = false
  editingTask.value = null
}

async function batchUpdateStatus(status: string) {
  try {
    await Promise.all(
      selectedTasks.value.map(taskId =>
        taskStore.updateTodo(taskId, {
          status: status as any,
          completedAt: status === 'Completed' ? new Date().toISOString() : undefined
        })
      )
    )
    selectedTasks.value = []
    showBatchModal.value = false
  } catch (error) {
    console.error('Batch status update error:', error)
  }
}

async function batchUpdatePriority(priority: string) {
  try {
    await Promise.all(
      selectedTasks.value.map(taskId =>
        taskStore.updateTodo(taskId, { priority: priority as any })
      )
    )
    selectedTasks.value = []
    showBatchModal.value = false
  } catch (error) {
    console.error('Batch priority update error:', error)
  }
}

function batchDelete() {
  if (selectedTasks.value.length === 0) return
  
  deleteType.value = 'batch'
  showBatchModal.value = false
  showDeleteModal.value = true
}

async function confirmDelete() {
  try {
    if (deleteType.value === 'single' && deletingId.value) {
      await taskStore.deleteTodo(deletingId.value)
      deletingId.value = null
    } else if (deleteType.value === 'batch') {
      await Promise.all(
        selectedTasks.value.map(taskId => taskStore.deleteTodo(taskId))
      )
      selectedTasks.value = []
    }
    showDeleteModal.value = false
  } catch (error) {
    console.error('Delete error:', error)
  }
}

// Lifecycle
onMounted(async () => {
  await taskStore.fetchTodoItems()
})
</script>