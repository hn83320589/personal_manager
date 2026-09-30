import type { Schemas } from '@/api/types'

export const taskStatusOptions: { value: Schemas['WorkTaskStatus']; label: string }[] = [
  { value: 'Pending', label: '待處理' },
  { value: 'Planning', label: '規劃中' },
  { value: 'InProgress', label: '進行中' },
  { value: 'Testing', label: '測試中' },
  { value: 'OnHold', label: '暫停' },
  { value: 'Completed', label: '已完成' },
  { value: 'Cancelled', label: '已取消' },
]

export const taskPriorityOptions: { value: Schemas['WorkTaskPriority']; label: string }[] = [
  { value: 'Urgent', label: '緊急' },
  { value: 'High', label: '高' },
  { value: 'Medium', label: '中' },
  { value: 'Low', label: '低' },
]

export const statusLabel = (status: Schemas['WorkTaskStatus']) =>
  taskStatusOptions.find((o) => o.value === status)?.label ?? status

export const priorityLabel = (priority: Schemas['WorkTaskPriority']) =>
  taskPriorityOptions.find((o) => o.value === priority)?.label ?? priority

export const projectColors = ['#2754c5', '#1d7550', '#6541c0', '#bd3259', '#c55a1c', '#5b6270']
