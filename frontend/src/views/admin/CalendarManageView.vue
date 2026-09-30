<template>
  <AdminLayout>
    <!-- Page Header -->
    <div class="mb-8">
      <div class="flex justify-between items-center">
        <div>
          <h2 class="text-2xl font-bold text-gray-900">行事曆管理</h2>
          <p class="mt-1 text-sm text-gray-600">
            管理您的行事曆事件、會議安排和重要提醒
          </p>
        </div>
        <BaseButton
          variant="primary"
          @click="showCreateModal = true"
        >
          <PlusIcon class="w-4 h-4 mr-2" />
          新增事件
        </BaseButton>
      </div>
    </div>

    <!-- Stats Cards -->
    <div class="grid grid-cols-1 md:grid-cols-4 gap-6 mb-6">
      <BaseCard>
        <div class="flex items-center">
          <div class="p-3 rounded-full bg-blue-100">
            <CalendarDaysIcon class="w-6 h-6 text-blue-600" />
          </div>
          <div class="ml-4">
            <h3 class="text-sm font-medium text-gray-500">總事件數</h3>
            <p class="text-2xl font-semibold text-gray-900">{{ events.length }}</p>
          </div>
        </div>
      </BaseCard>

      <BaseCard>
        <div class="flex items-center">
          <div class="p-3 rounded-full bg-green-100">
            <ClockIcon class="w-6 h-6 text-green-600" />
          </div>
          <div class="ml-4">
            <h3 class="text-sm font-medium text-gray-500">本週事件</h3>
            <p class="text-2xl font-semibold text-gray-900">{{ thisWeekEventsCount }}</p>
          </div>
        </div>
      </BaseCard>

      <BaseCard>
        <div class="flex items-center">
          <div class="p-3 rounded-full bg-purple-100">
            <EyeIcon class="w-6 h-6 text-purple-600" />
          </div>
          <div class="ml-4">
            <h3 class="text-sm font-medium text-gray-500">公開事件</h3>
            <p class="text-2xl font-semibold text-gray-900">{{ publicEventsCount }}</p>
          </div>
        </div>
      </BaseCard>

      <BaseCard>
        <div class="flex items-center">
          <div class="p-3 rounded-full bg-yellow-100">
            <UsersIcon class="w-6 h-6 text-yellow-600" />
          </div>
          <div class="ml-4">
            <h3 class="text-sm font-medium text-gray-500">會議數</h3>
            <p class="text-2xl font-semibold text-gray-900">{{ meetingEventsCount }}</p>
          </div>
        </div>
      </BaseCard>
    </div>

    <!-- Calendar View Toggle -->
    <div class="mb-6">
      <div class="flex justify-between items-center">
        <div class="flex space-x-1 bg-gray-100 p-1 rounded-lg">
          <button
            v-for="view in calendarViews"
            :key="view.value"
            @click="currentView = view.value as 'month' | 'week' | 'list'"
            :class="[
              'px-3 py-2 text-sm font-medium rounded-md transition-colors',
              currentView === view.value
                ? 'bg-white text-gray-900 shadow'
                : 'text-gray-600 hover:text-gray-900'
            ]"
          >
            <component :is="view.icon" class="w-4 h-4 mr-2 inline" />
            {{ view.label }}
          </button>
        </div>
        
        <!-- Date Navigation -->
        <div class="flex items-center space-x-2">
          <BaseButton
            variant="outline"
            size="small"
            @click="navigateDate(-1)"
          >
            <ChevronLeftIcon class="w-4 h-4" />
          </BaseButton>
          <span class="text-sm font-medium text-gray-900 min-w-32 text-center">
            {{ formatCurrentPeriod() }}
          </span>
          <BaseButton
            variant="outline" 
            size="small"
            @click="navigateDate(1)"
          >
            <ChevronRightIcon class="w-4 h-4" />
          </BaseButton>
          <BaseButton
            variant="outline"
            size="small"
            @click="goToToday"
          >
            今天
          </BaseButton>
        </div>
      </div>
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
              placeholder="搜尋事件標題、描述、地點..."
              class="pl-10 pr-4 py-2 w-full border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
            />
          </div>
        </div>
        <div class="flex space-x-2">
          <select
            v-model="selectedEventType"
            class="px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
          >
            <option value="">所有類型</option>
            <option v-for="type in eventTypes" :key="type.value" :value="type.value">
              {{ type.label }}
            </option>
          </select>
          <select
            v-model="statusFilter"
            class="px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
          >
            <option value="">所有狀態</option>
            <option value="upcoming">即將到來</option>
            <option value="ongoing">進行中</option>
            <option value="past">已結束</option>
            <option value="public">公開事件</option>
            <option value="private">私人事件</option>
          </select>
          <select
            v-model="sortBy"
            class="px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
          >
            <option value="startDate">開始時間</option>
            <option value="title">標題</option>
            <option value="eventType">類型</option>
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

    <!-- Calendar Content -->
    <div class="bg-white shadow rounded-lg overflow-hidden">
      <!-- Month View -->
      <div v-if="currentView === 'month'" class="p-6">
        <CalendarGrid
          :events="filteredEvents"
          :current-date="currentDate"
          @event-click="viewEvent"
          @date-click="createEventOnDate"
        />
      </div>

      <!-- Week View -->
      <div v-else-if="currentView === 'week'" class="p-6">
        <WeekView
          :events="filteredEvents"
          :current-date="currentDate"
          @event-click="viewEvent"
          @time-slot-click="createEventOnDateTime"
        />
      </div>

      <!-- List View -->
      <div v-else>
        <div v-if="filteredEvents.length === 0" class="p-8 text-center">
          <CalendarDaysIcon class="mx-auto h-12 w-12 text-gray-400" />
          <h3 class="mt-2 text-sm font-medium text-gray-900">沒有事件資料</h3>
          <p class="mt-1 text-sm text-gray-500">開始新增您的行事曆事件。</p>
          <div class="mt-6">
            <BaseButton
              variant="primary"
              @click="showCreateModal = true"
            >
              <PlusIcon class="w-4 h-4 mr-2" />
              新增事件
            </BaseButton>
          </div>
        </div>

        <div v-else>
          <!-- Events Table -->
          <table class="min-w-full divide-y divide-gray-200">
            <thead class="bg-gray-50">
              <tr>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  事件
                </th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  時間
                </th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  類型
                </th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  地點
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
              <tr v-for="event in filteredEvents" :key="event.id" class="hover:bg-gray-50">
                <td class="px-6 py-4 whitespace-nowrap">
                  <div class="flex items-center">
                    <div
                      class="flex-shrink-0 h-3 w-3 rounded-full mr-3"
                      :style="{ backgroundColor: event.color || '#3B82F6' }"
                    ></div>
                    <div>
                      <div class="text-sm font-medium text-gray-900">{{ event.title }}</div>
                      <div class="text-sm text-gray-500 line-clamp-1">{{ event.description }}</div>
                    </div>
                  </div>
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                  <div>
                    <div class="font-medium">{{ formatEventDate(event) }}</div>
                    <div class="text-xs">{{ formatEventTime(event) }}</div>
                  </div>
                </td>
                <td class="px-6 py-4 whitespace-nowrap">
                  <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
                    事件
                  </span>
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                  -
                </td>
                <td class="px-6 py-4 whitespace-nowrap">
                  <div class="flex flex-col space-y-1">
                    <span :class="[
                      'inline-flex px-2 py-1 text-xs font-semibold rounded-full',
                      event.isPublic
                        ? 'bg-green-100 text-green-800'
                        : 'bg-gray-100 text-gray-800'
                    ]">
                      {{ event.isPublic ? '公開' : '私人' }}
                    </span>
                    <span :class="[
                      'inline-flex px-2 py-1 text-xs font-semibold rounded-full',
                      getEventStatusStyle(event)
                    ]">
                      {{ getEventStatus(event) }}
                    </span>
                  </div>
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                  <div class="flex justify-end space-x-2">
                    <BaseButton
                      variant="outline"
                      size="small"
                      @click="viewEvent(event)"
                      title="查看詳細"
                    >
                      <EyeIcon class="w-4 h-4" />
                    </BaseButton>
                    <BaseButton
                      variant="outline"
                      size="small"
                      @click="editEvent(event)"
                    >
                      <PencilIcon class="w-4 h-4" />
                    </BaseButton>
                    <BaseButton
                      variant="outline"
                      size="small"
                      @click="deleteEvent(event.id)"
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
      title="事件管理"
      max-width="3xl"
    >
      <CalendarEventForm
        :event="editingEvent"
        @save="handleSave"
        @cancel="handleCancel"
      />
    </BaseModal>

    <!-- Event Detail Modal -->
    <BaseModal
      :show="showDetailModal"
      @close="showDetailModal = false"
      title="事件詳細"
      max-width="2xl"
    >
      <CalendarEventDetail
        v-if="viewingEvent"
        :event="viewingEvent"
        @edit="editEventFromDetail"
        @delete="deleteEventFromDetail"
        @close="showDetailModal = false"
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
          您確定要刪除這個事件嗎？此操作無法復原。
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
  CalendarDaysIcon,
  ClockIcon,
  EyeIcon,
  UsersIcon,
  MagnifyingGlassIcon,
  PencilIcon,
  TrashIcon,
  ChevronLeftIcon,
  ChevronRightIcon,
  Squares2X2Icon,
  ListBulletIcon,
  TableCellsIcon
} from '@heroicons/vue/24/outline'
import { useCalendarStore } from '@/stores/calendar'
import type { CalendarEvent } from '@/types/api'
import AdminLayout from '@/components/layout/AdminLayout.vue'
import BaseButton from '@/components/ui/BaseButton.vue'
import BaseCard from '@/components/ui/BaseCard.vue'
import BaseModal from '@/components/ui/BaseModal.vue'
import CalendarEventForm from '@/components/admin/CalendarEventForm.vue'
import CalendarEventDetail from '@/components/admin/CalendarEventDetail.vue'
import CalendarGrid from '@/components/calendar/CalendarGrid.vue'
import WeekView from '@/components/calendar/WeekView.vue'

// Stores
const calendarStore = useCalendarStore()

// State
const searchQuery = ref('')
const selectedEventType = ref<string>('')
const statusFilter = ref('')
const sortBy = ref('startDate')
const sortOrder = ref<'asc' | 'desc'>('asc')
const currentView = ref<'month' | 'week' | 'list'>('list')
const currentDate = ref(new Date())
const showCreateModal = ref(false)
const showDetailModal = ref(false)
const showDeleteModal = ref(false)
const editingEvent = ref<CalendarEvent | null>(null)
const viewingEvent = ref<CalendarEvent | null>(null)
const deletingEventId = ref<number | null>(null)

// Constants
const calendarViews = [
  { value: 'month', label: '月', icon: Squares2X2Icon },
  { value: 'week', label: '週', icon: TableCellsIcon },
  { value: 'list', label: '列表', icon: ListBulletIcon }
]

const eventTypes: { value: string, label: string }[] = []

// Computed
const events = computed(() => calendarStore.events)

const thisWeekEventsCount = computed(() => {
  const now = new Date()
  const startOfWeek = new Date(now.setDate(now.getDate() - now.getDay()))
  const endOfWeek = new Date(now.setDate(startOfWeek.getDate() + 6))
  
  return events.value.filter(event => {
    const eventDate = new Date(event.startTime)
    return eventDate >= startOfWeek && eventDate <= endOfWeek
  }).length
})

const publicEventsCount = computed(() => {
  return events.value.filter(event => event.isPublic).length
})

const meetingEventsCount = computed(() => {
  return 0 // eventType field removed from CalendarEvent
})

const filteredEvents = computed(() => {
  let filtered = events.value

  // Search filter
  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    filtered = filtered.filter(event =>
      event.title.toLowerCase().includes(query) ||
      event.description?.toLowerCase().includes(query)
    )
  }

  // Event type filter (disabled: eventType removed from CalendarEvent)
  // if (selectedEventType.value !== '') { ... }

  // Status filter
  if (statusFilter.value) {
    const now = new Date()
    switch (statusFilter.value) {
      case 'upcoming':
        filtered = filtered.filter(event => new Date(event.startTime) > now)
        break
      case 'ongoing':
        filtered = filtered.filter(event => {
          const start = new Date(event.startTime)
          const end = event.endTime ? new Date(event.endTime) : start
          return start <= now && end >= now
        })
        break
      case 'past':
        filtered = filtered.filter(event => {
          const end = event.endTime ? new Date(event.endTime) : new Date(event.startTime)
          return end < now
        })
        break
      case 'public':
        filtered = filtered.filter(event => event.isPublic)
        break
      case 'private':
        filtered = filtered.filter(event => !event.isPublic)
        break
    }
  }

  // Sort
  return filtered.sort((a, b) => {
    let aValue: any, bValue: any

    switch (sortBy.value) {
      case 'startDate':
        aValue = new Date(a.startTime).getTime()
        bValue = new Date(b.startTime).getTime()
        break
      case 'title':
        aValue = a.title.toLowerCase()
        bValue = b.title.toLowerCase()
        break
      case 'eventType':
        aValue = a.title
        bValue = b.title
        break
      case 'createdAt':
        aValue = new Date(a.createdAt).getTime()
        bValue = new Date(b.createdAt).getTime()
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
function formatCurrentPeriod(): string {
  const year = currentDate.value.getFullYear()
  const month = currentDate.value.getMonth() + 1
  
  switch (currentView.value) {
    case 'month':
      return `${year}年 ${month}月`
    case 'week':
      const startOfWeek = new Date(currentDate.value)
      startOfWeek.setDate(currentDate.value.getDate() - currentDate.value.getDay())
      const endOfWeek = new Date(startOfWeek)
      endOfWeek.setDate(startOfWeek.getDate() + 6)
      return `${startOfWeek.getMonth() + 1}/${startOfWeek.getDate()} - ${endOfWeek.getMonth() + 1}/${endOfWeek.getDate()}`
    default:
      return `${year}年 ${month}月`
  }
}

function navigateDate(direction: number) {
  const newDate = new Date(currentDate.value)
  
  switch (currentView.value) {
    case 'month':
      newDate.setMonth(newDate.getMonth() + direction)
      break
    case 'week':
      newDate.setDate(newDate.getDate() + (direction * 7))
      break
    default:
      newDate.setMonth(newDate.getMonth() + direction)
  }
  
  currentDate.value = newDate
}

function goToToday() {
  currentDate.value = new Date()
}

function getEventStatus(event: CalendarEvent): string {
  const now = new Date()
  const start = new Date(event.startTime)
  const end = event.endTime ? new Date(event.endTime) : start
  
  if (start > now) return '即將到來'
  if (start <= now && end >= now) return '進行中'
  return '已結束'
}

function getEventStatusStyle(event: CalendarEvent): string {
  const status = getEventStatus(event)
  switch (status) {
    case '即將到來': return 'bg-blue-100 text-blue-800'
    case '進行中': return 'bg-green-100 text-green-800'
    case '已結束': return 'bg-gray-100 text-gray-800'
    default: return 'bg-gray-100 text-gray-800'
  }
}

function formatEventDate(event: CalendarEvent): string {
  const start = new Date(event.startTime)
  const end = event.endTime ? new Date(event.endTime) : null
  
  const startDate = start.toLocaleDateString('zh-TW', {
    month: 'short',
    day: 'numeric'
  })
  
  if (end && end.toDateString() !== start.toDateString()) {
    const endDate = end.toLocaleDateString('zh-TW', {
      month: 'short',
      day: 'numeric'
    })
    return `${startDate} - ${endDate}`
  }
  
  return startDate
}

function formatEventTime(event: CalendarEvent): string {
  if (event.isAllDay) return '全天'

  const start = new Date(event.startTime)
  const end = event.endTime ? new Date(event.endTime) : null
  const startTimeStr = start.toLocaleTimeString('zh-TW', { hour: '2-digit', minute: '2-digit' })

  if (end) {
    const endTimeStr = end.toLocaleTimeString('zh-TW', { hour: '2-digit', minute: '2-digit' })
    return `${startTimeStr} - ${endTimeStr}`
  }

  return startTimeStr
}

function viewEvent(event: CalendarEvent) {
  viewingEvent.value = event
  showDetailModal.value = true
}

function editEvent(event: CalendarEvent) {
  editingEvent.value = { ...event }
  showCreateModal.value = true
}

function editEventFromDetail(event: CalendarEvent) {
  showDetailModal.value = false
  editEvent(event)
}

function deleteEvent(id: number) {
  deletingEventId.value = id
  showDeleteModal.value = true
}

function deleteEventFromDetail(id: number) {
  showDetailModal.value = false
  deleteEvent(id)
}

function createEventOnDate(date: Date) {
  // 建立包含選定日期的部分事件物件，讓表單能自動填入日期
  editingEvent.value = {
    startTime: date.toISOString(),
    endTime: '',
    isAllDay: false
  } as Partial<CalendarEvent> as CalendarEvent
  showCreateModal.value = true
}

function createEventOnDateTime(date: Date, time: string) {
  // 建立包含選定日期和時間的部分事件物件
  const dateStr = date.toISOString().split('T')[0]
  editingEvent.value = {
    startTime: `${dateStr}T${time}:00`,
    endTime: '',
    isAllDay: false
  } as Partial<CalendarEvent> as CalendarEvent
  showCreateModal.value = true
}

async function confirmDelete() {
  if (deletingEventId.value) {
    try {
      await calendarStore.deleteEvent(deletingEventId.value)
      showDeleteModal.value = false
      deletingEventId.value = null
    } catch (error) {
      console.error('Delete error:', error)
    }
  }
}

async function handleSave(data: any) {
  try {
    if (editingEvent.value) {
      // Update existing
      await calendarStore.updateEvent(editingEvent.value.id, data)
    } else {
      // Create new
      await calendarStore.createEvent(data)
    }
    handleCancel()
  } catch (error) {
    console.error('Save error:', error)
  }
}

function handleCancel() {
  showCreateModal.value = false
  editingEvent.value = null
}

// Lifecycle
onMounted(async () => {
  await calendarStore.fetchEvents()
})
</script>

<style scoped>
.line-clamp-1 {
  display: -webkit-box;
  -webkit-line-clamp: 1;
  -webkit-box-orient: vertical;
  overflow: hidden;
}
</style>