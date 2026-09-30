<template>
  <div class="overflow-hidden border border-gray-200 rounded-lg">
    <!-- Table Header (if showHeader is true) -->
    <div v-if="showHeader" class="bg-gray-50 px-6 py-4 border-b border-gray-200">
      <div class="flex items-center justify-between">
        <div>
          <h3 v-if="title" class="text-lg font-medium text-gray-900">{{ title }}</h3>
          <p v-if="description" class="text-sm text-gray-500">{{ description }}</p>
        </div>
        <div class="flex items-center space-x-3">
          <slot name="header-actions" />
        </div>
      </div>
    </div>

    <!-- Search and Filters -->
    <div v-if="showFilters" class="bg-white px-6 py-4 border-b border-gray-200">
      <div class="flex items-center justify-between space-x-4">
        <div class="flex-1">
          <BaseInput
            v-if="searchable"
            v-model="searchTerm"
            type="search"
            placeholder="Search..."
            class="max-w-sm"
          />
        </div>
        <div class="flex items-center space-x-3">
          <slot name="filters" />
        </div>
      </div>
    </div>

    <!-- Table Container -->
    <div class="overflow-x-auto">
      <table class="w-full">
        <!-- Table Head -->
        <thead class="bg-gray-50">
          <tr>
            <th
              v-if="selectable"
              class="w-12 px-6 py-3 text-left"
            >
              <input
                type="checkbox"
                :checked="allSelected"
                :indeterminate="someSelected"
                @change="toggleSelectAll"
                class="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300 rounded"
              />
            </th>
            
            <th
              v-for="column in columns"
              :key="column.key"
              :class="getHeaderClasses(column)"
              @click="handleSort(column)"
            >
              <div class="flex items-center space-x-1">
                <span>{{ column.title }}</span>
                <div v-if="column.sortable" class="flex flex-col">
                  <ChevronUpIcon
                    :class="getSortIconClasses(column, 'asc')"
                    class="h-3 w-3 -mb-1"
                  />
                  <ChevronDownIcon
                    :class="getSortIconClasses(column, 'desc')"
                    class="h-3 w-3"
                  />
                </div>
              </div>
            </th>
            
            <th v-if="hasActions" class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
              Actions
            </th>
          </tr>
        </thead>

        <!-- Table Body -->
        <tbody class="bg-white divide-y divide-gray-200">
          <tr v-if="loading" >
            <td :colspan="totalColumns" class="px-6 py-12 text-center">
              <LoadingSpinner />
            </td>
          </tr>
          
          <tr v-else-if="filteredData.length === 0">
            <td :colspan="totalColumns" class="px-6 py-12 text-center text-gray-500">
              {{ emptyMessage }}
            </td>
          </tr>
          
          <tr
            v-else
            v-for="(row, index) in paginatedData"
            :key="getRowKey(row, index)"
            :class="getRowClasses(row, index)"
            @click="handleRowClick(row, index)"
          >
            <td v-if="selectable" class="w-12 px-6 py-4">
              <input
                type="checkbox"
                :checked="isRowSelected(row)"
                @change="toggleRowSelection(row)"
                @click.stop
                class="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300 rounded"
              />
            </td>
            
            <td
              v-for="column in columns"
              :key="column.key"
              :class="getCellClasses(column)"
            >
              <slot
                :name="`cell-${column.key}`"
                :row="row"
                :value="getColumnValue(row, column)"
                :column="column"
                :index="index"
              >
                {{ formatCellValue(row, column) }}
              </slot>
            </td>
            
            <td v-if="hasActions" class="px-6 py-4 text-right text-sm font-medium space-x-2">
              <slot name="actions" :row="row" :index="index" />
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Pagination -->
    <div v-if="showPagination && totalPages > 1" class="bg-gray-50 px-6 py-3 border-t border-gray-200">
      <div class="flex items-center justify-between">
        <div class="text-sm text-gray-500">
          Showing {{ (currentPage - 1) * pageSize + 1 }} to {{ Math.min(currentPage * pageSize, filteredData.length) }} of {{ filteredData.length }} results
        </div>
        
        <div class="flex items-center space-x-2">
          <BaseButton
            size="small"
            variant="outline"
            :disabled="currentPage === 1"
            @click="goToPage(currentPage - 1)"
          >
            Previous
          </BaseButton>
          
          <span v-for="page in visiblePages" :key="page">
            <BaseButton
              v-if="page !== '...'"
              size="small"
              :variant="page === currentPage ? 'primary' : 'outline'"
              @click="goToPage(page as number)"
            >
              {{ page }}
            </BaseButton>
            <span v-else class="px-2 text-gray-500">...</span>
          </span>
          
          <BaseButton
            size="small"
            variant="outline"
            :disabled="currentPage === totalPages"
            @click="goToPage(currentPage + 1)"
          >
            Next
          </BaseButton>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { ChevronUpIcon, ChevronDownIcon } from '@heroicons/vue/24/outline'
import BaseInput from './BaseInput.vue'
import BaseButton from './BaseButton.vue'
import LoadingSpinner from '../common/LoadingSpinner.vue'

export interface TableColumn {
  key: string
  title: string
  sortable?: boolean
  width?: string
  align?: 'left' | 'center' | 'right'
  formatter?: (value: any, row: any) => string
}

interface Props {
  data: any[]
  columns: TableColumn[]
  title?: string
  description?: string
  loading?: boolean
  selectable?: boolean
  searchable?: boolean
  showHeader?: boolean
  showFilters?: boolean
  showPagination?: boolean
  pageSize?: number
  emptyMessage?: string
  rowKey?: string | ((row: any) => string | number)
  clickableRows?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  loading: false,
  selectable: false,
  searchable: false,
  showHeader: true,
  showFilters: true,
  showPagination: true,
  pageSize: 10,
  emptyMessage: 'No data available',
  rowKey: 'id',
  clickableRows: false
})

const emit = defineEmits<{
  rowClick: [row: any, index: number]
  rowSelect: [selectedRows: any[]]
  sort: [column: string, direction: 'asc' | 'desc']
}>()

// State
const searchTerm = ref('')
const currentPage = ref(1)
const sortColumn = ref<string | null>(null)
const sortDirection = ref<'asc' | 'desc'>('asc')
const selectedRows = ref<any[]>([])

// Computed
const hasActions = computed(() => !!slots.actions)
const totalColumns = computed(() => {
  let count = props.columns.length
  if (props.selectable) count++
  if (hasActions.value) count++
  return count
})

const filteredData = computed(() => {
  let data = [...props.data]
  
  // Apply search filter
  if (searchTerm.value) {
    const term = searchTerm.value.toLowerCase()
    data = data.filter(row =>
      props.columns.some(column => {
        const value = getColumnValue(row, column)
        return String(value).toLowerCase().includes(term)
      })
    )
  }
  
  // Apply sorting
  if (sortColumn.value) {
    data.sort((a, b) => {
      const column = props.columns.find(col => col.key === sortColumn.value)
      if (!column) return 0
      
      const aValue = getColumnValue(a, column)
      const bValue = getColumnValue(b, column)
      
      let result = 0
      if (aValue < bValue) result = -1
      else if (aValue > bValue) result = 1
      
      return sortDirection.value === 'desc' ? -result : result
    })
  }
  
  return data
})

const totalPages = computed(() => Math.ceil(filteredData.value.length / props.pageSize))

const paginatedData = computed(() => {
  if (!props.showPagination) return filteredData.value
  
  const start = (currentPage.value - 1) * props.pageSize
  const end = start + props.pageSize
  return filteredData.value.slice(start, end)
})

const visiblePages = computed(() => {
  const pages: (number | string)[] = []
  const total = totalPages.value
  const current = currentPage.value
  
  if (total <= 7) {
    for (let i = 1; i <= total; i++) {
      pages.push(i)
    }
  } else {
    if (current <= 4) {
      for (let i = 1; i <= 5; i++) pages.push(i)
      pages.push('...')
      pages.push(total)
    } else if (current >= total - 3) {
      pages.push(1)
      pages.push('...')
      for (let i = total - 4; i <= total; i++) pages.push(i)
    } else {
      pages.push(1)
      pages.push('...')
      for (let i = current - 1; i <= current + 1; i++) pages.push(i)
      pages.push('...')
      pages.push(total)
    }
  }
  
  return pages
})

const allSelected = computed(() => {
  return paginatedData.value.length > 0 && 
         paginatedData.value.every(row => isRowSelected(row))
})

const someSelected = computed(() => {
  return paginatedData.value.some(row => isRowSelected(row)) && !allSelected.value
})

// Methods
function getRowKey(row: any, index: number): string | number {
  if (typeof props.rowKey === 'function') {
    return props.rowKey(row)
  }
  return row[props.rowKey] ?? index
}

function getColumnValue(row: any, column: TableColumn) {
  return row[column.key]
}

function formatCellValue(row: any, column: TableColumn) {
  const value = getColumnValue(row, column)
  if (column.formatter) {
    return column.formatter(value, row)
  }
  return value
}

function getHeaderClasses(column: TableColumn) {
  const baseClasses = 'px-6 py-3 text-xs font-medium text-gray-500 uppercase tracking-wider'
  const alignClasses = {
    left: 'text-left',
    center: 'text-center',
    right: 'text-right'
  }
  const cursorClass = column.sortable ? 'cursor-pointer hover:bg-gray-100' : ''
  
  return [
    baseClasses,
    alignClasses[column.align || 'left'],
    cursorClass
  ].join(' ')
}

function getCellClasses(column: TableColumn) {
  const baseClasses = 'px-6 py-4 text-sm text-gray-900'
  const alignClasses = {
    left: 'text-left',
    center: 'text-center',
    right: 'text-right'
  }
  
  return [baseClasses, alignClasses[column.align || 'left']].join(' ')
}

function getRowClasses(row: any, index: number) {
  const baseClasses = 'hover:bg-gray-50'
  const clickableClass = props.clickableRows ? 'cursor-pointer' : ''
  
  return [baseClasses, clickableClass].join(' ')
}

function getSortIconClasses(column: TableColumn, direction: 'asc' | 'desc') {
  const isActive = sortColumn.value === column.key && sortDirection.value === direction
  return isActive ? 'text-primary-600' : 'text-gray-300'
}

function handleSort(column: TableColumn) {
  if (!column.sortable) return
  
  if (sortColumn.value === column.key) {
    sortDirection.value = sortDirection.value === 'asc' ? 'desc' : 'asc'
  } else {
    sortColumn.value = column.key
    sortDirection.value = 'asc'
  }
  
  emit('sort', column.key, sortDirection.value)
}

function handleRowClick(row: any, index: number) {
  if (props.clickableRows) {
    emit('rowClick', row, index)
  }
}

function isRowSelected(row: any) {
  const key = getRowKey(row, 0)
  return selectedRows.value.some(selectedRow => getRowKey(selectedRow, 0) === key)
}

function toggleRowSelection(row: any) {
  const key = getRowKey(row, 0)
  const index = selectedRows.value.findIndex(selectedRow => getRowKey(selectedRow, 0) === key)
  
  if (index > -1) {
    selectedRows.value.splice(index, 1)
  } else {
    selectedRows.value.push(row)
  }
  
  emit('rowSelect', selectedRows.value)
}

function toggleSelectAll() {
  if (allSelected.value) {
    // Deselect all visible rows
    paginatedData.value.forEach(row => {
      const key = getRowKey(row, 0)
      const index = selectedRows.value.findIndex(selectedRow => getRowKey(selectedRow, 0) === key)
      if (index > -1) {
        selectedRows.value.splice(index, 1)
      }
    })
  } else {
    // Select all visible rows
    paginatedData.value.forEach(row => {
      if (!isRowSelected(row)) {
        selectedRows.value.push(row)
      }
    })
  }
  
  emit('rowSelect', selectedRows.value)
}

function goToPage(page: number) {
  if (page >= 1 && page <= totalPages.value) {
    currentPage.value = page
  }
}

// Watch for data changes to reset pagination
watch(() => props.data, () => {
  currentPage.value = 1
  selectedRows.value = []
}, { deep: true })

// Watch for search term changes to reset pagination
watch(searchTerm, () => {
  currentPage.value = 1
})

// Get slots
const slots = defineSlots()
</script>