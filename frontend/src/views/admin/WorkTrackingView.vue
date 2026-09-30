<template>
  <AdminLayout>
    <!-- Page Header -->
    <div class="mb-8">
      <div class="flex justify-between items-center">
        <div>
          <h2 class="text-2xl font-bold text-gray-900">工作追蹤</h2>
          <p class="mt-1 text-sm text-gray-600">
            追蹤工作時間、管理專案進度和分析工作效率
          </p>
        </div>
        <div class="flex space-x-3">
          <BaseButton variant="outline" @click="showTimeTrackerModal = true">
            <ClockIcon class="w-4 h-4 mr-2" />
            時間追蹤器
          </BaseButton>
          <BaseButton variant="primary" @click="showCreateModal = true">
            <PlusIcon class="w-4 h-4 mr-2" />
            新增工作任務
          </BaseButton>
        </div>
      </div>
    </div>

    <!-- Active Timer -->
    <div v-if="activeTimer" class="mb-6">
      <div class="bg-blue-50 border border-blue-200 rounded-lg p-4">
        <div class="flex items-center justify-between">
          <div class="flex items-center space-x-4">
            <div class="flex items-center space-x-2">
              <div
                class="w-3 h-3 bg-green-500 rounded-full animate-pulse"
              ></div>
              <span class="text-sm font-medium text-blue-900"
                >正在追蹤時間</span
              >
            </div>
            <div>
              <h3 class="text-lg font-semibold text-blue-900">
                {{ activeTimer.task?.title }}
              </h3>
              <p class="text-sm text-blue-700">
                {{ activeTimer.project || "無專案" }}
              </p>
            </div>
          </div>
          <div class="flex items-center space-x-4">
            <div class="text-right">
              <div class="text-2xl font-bold text-blue-900">
                {{ formatDuration(activeTimer.elapsed) }}
              </div>
              <div class="text-sm text-blue-700">
                開始時間: {{ formatTime(activeTimer.startTime) }}
              </div>
            </div>
            <div class="flex space-x-2">
              <BaseButton
                variant="outline"
                size="small"
                @click="pauseTimer"
                :disabled="activeTimer.isPaused"
              >
                <PauseIcon class="w-4 h-4" />
              </BaseButton>
              <BaseButton
                variant="outline"
                size="small"
                @click="resumeTimer"
                :disabled="!activeTimer.isPaused"
              >
                <PlayIcon class="w-4 h-4" />
              </BaseButton>
              <BaseButton variant="primary" size="small" @click="stopTimer">
                <StopIcon class="w-4 h-4" />
              </BaseButton>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Stats Cards -->
    <div class="grid grid-cols-1 md:grid-cols-4 gap-6 mb-6">
      <BaseCard>
        <div class="flex items-center">
          <div class="p-3 rounded-full bg-blue-100">
            <ClockIcon class="w-6 h-6 text-blue-600" />
          </div>
          <div class="ml-4">
            <h3 class="text-sm font-medium text-gray-500">今日工作時間</h3>
            <p class="text-2xl font-semibold text-gray-900">
              {{ todayWorkedHours }}h
            </p>
          </div>
        </div>
      </BaseCard>

      <BaseCard>
        <div class="flex items-center">
          <div class="p-3 rounded-full bg-green-100">
            <CheckCircleIcon class="w-6 h-6 text-green-600" />
          </div>
          <div class="ml-4">
            <h3 class="text-sm font-medium text-gray-500">完成任務</h3>
            <p class="text-2xl font-semibold text-gray-900">
              {{ completedTasksCount }}
            </p>
          </div>
        </div>
      </BaseCard>

      <BaseCard>
        <div class="flex items-center">
          <div class="p-3 rounded-full bg-yellow-100">
            <ExclamationTriangleIcon class="w-6 h-6 text-yellow-600" />
          </div>
          <div class="ml-4">
            <h3 class="text-sm font-medium text-gray-500">逾期任務</h3>
            <p class="text-2xl font-semibold text-gray-900">
              {{ overdueTasks.length }}
            </p>
          </div>
        </div>
      </BaseCard>

      <BaseCard>
        <div class="flex items-center">
          <div class="p-3 rounded-full bg-purple-100">
            <FolderIcon class="w-6 h-6 text-purple-600" />
          </div>
          <div class="ml-4">
            <h3 class="text-sm font-medium text-gray-500">活躍專案</h3>
            <p class="text-2xl font-semibold text-gray-900">
              {{ activeProjectsCount }}
            </p>
          </div>
        </div>
      </BaseCard>
    </div>

    <!-- View Toggle -->
    <div class="mb-6">
      <div class="flex justify-between items-center">
        <div class="flex space-x-1 bg-gray-100 p-1 rounded-lg">
          <button
            v-for="view in workViews"
            :key="view.value"
            @click="
              currentView = view.value as
                | 'tasks'
                | 'projects'
                | 'timesheet'
                | 'reports'
            "
            :class="[
              'px-3 py-2 text-sm font-medium rounded-md transition-colors',
              currentView === view.value
                ? 'bg-white text-gray-900 shadow'
                : 'text-gray-600 hover:text-gray-900',
            ]"
          >
            <component :is="view.icon" class="w-4 h-4 mr-2 inline" />
            {{ view.label }}
          </button>
        </div>

        <!-- Date Range Picker -->
        <div class="flex items-center space-x-2">
          <input
            v-model="dateRange.start"
            type="date"
            class="px-3 py-2 border border-gray-300 rounded-md text-sm focus:ring-blue-500 focus:border-blue-500"
          />
          <span class="text-gray-500">至</span>
          <input
            v-model="dateRange.end"
            type="date"
            class="px-3 py-2 border border-gray-300 rounded-md text-sm focus:ring-blue-500 focus:border-blue-500"
          />
        </div>
      </div>
    </div>

    <!-- Filters -->
    <div class="mb-6">
      <div class="flex flex-col sm:flex-row gap-4">
        <div class="flex-1">
          <div class="relative">
            <MagnifyingGlassIcon
              class="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-gray-400"
            />
            <input
              v-model="searchQuery"
              type="text"
              placeholder="搜尋任務、專案..."
              class="pl-10 pr-4 py-2 w-full border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
            />
          </div>
        </div>
        <div class="flex space-x-2">
          <select
            v-model="selectedProject"
            class="px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
          >
            <option value="">所有專案</option>
            <option v-for="project in projects" :key="project" :value="project">
              {{ project }}
            </option>
          </select>
          <select
            v-model="selectedStatus"
            class="px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
          >
            <option value="">所有狀態</option>
            <option
              v-for="status in taskStatuses"
              :key="status.value"
              :value="status.value"
            >
              {{ status.label }}
            </option>
          </select>
          <select
            v-model="selectedPriority"
            class="px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
          >
            <option value="">所有優先級</option>
            <option
              v-for="priority in taskPriorities"
              :key="priority.value"
              :value="priority.value"
            >
              {{ priority.label }}
            </option>
          </select>
        </div>
      </div>
    </div>

    <!-- Content Views -->
    <div class="bg-white shadow rounded-lg overflow-hidden">
      <!-- Tasks View -->
      <div v-if="currentView === 'tasks'">
        <TasksView
          :tasks="filteredTasks"
          :loading="loading"
          @start-timer="startTaskTimer"
          @edit-task="editTask"
          @delete-task="deleteTask"
          @toggle-complete="toggleTaskComplete"
        />
      </div>

      <!-- Projects View -->
      <div v-else-if="currentView === 'projects'">
        <ProjectsView
          :projects="projectStats"
          :loading="loading"
          @view-project="viewProject"
          @edit-project="editProject"
        />
      </div>

      <!-- Timesheet View -->
      <div v-else-if="currentView === 'timesheet'">
        <TimesheetView
          :time-entries="timeEntries"
          :date-range="dateRange"
          :loading="loading"
          @edit-entry="editTimeEntry"
          @delete-entry="deleteTimeEntry"
          @add-entry="showTimeEntryModal = true"
        />
      </div>

      <!-- Reports View -->
      <div v-else-if="currentView === 'reports'">
        <ReportsView
          :tasks="tasks"
          :time-entries="timeEntries"
          :date-range="dateRange"
        />
      </div>
    </div>

    <!-- Create/Edit Task Modal -->
    <BaseModal
      :show="showCreateModal"
      @close="showCreateModal = false"
      title="工作任務管理"
      max-width="3xl"
    >
      <WorkTaskForm
        :task="editingTask"
        :projects="projectList"
        @save="handleSave"
        @cancel="handleCancel"
      />
    </BaseModal>

    <!-- Time Tracker Modal -->
    <BaseModal
      :show="showTimeTrackerModal"
      @close="showTimeTrackerModal = false"
      title="時間追蹤器"
      max-width="2xl"
    >
      <TimeTrackerForm
        :tasks="tasks"
        @start-timer="handleStartTimer"
        @cancel="showTimeTrackerModal = false"
      />
    </BaseModal>

    <!-- Time Entry Edit Modal -->
    <BaseModal
      :show="showTimeEntryModal"
      @close="showTimeEntryModal = false"
      title="編輯時間記錄"
      max-width="2xl"
    >
      <TimeEntryForm
        :entry="editingTimeEntry"
        :available-tasks="tasks"
        @save="handleTimeEntrySave"
        @cancel="showTimeEntryModal = false"
      />
    </BaseModal>

    <!-- Edit Project Modal -->
    <BaseModal
      :show="showEditProjectModal"
      @close="showEditProjectModal = false"
      title="重新命名專案"
      max-width="sm"
    >
      <div class="space-y-4">
        <div>
          <p class="text-sm text-gray-600 mb-3">
            將「{{ editingProjectName }}」底下的所有任務移至新專案名稱。
          </p>
          <label class="block text-sm font-medium text-gray-700 mb-1">新專案名稱</label>
          <input
            v-model="newProjectName"
            type="text"
            class="form-input w-full"
            placeholder="輸入新的專案名稱"
            @keyup.enter="confirmEditProject"
          />
        </div>
      </div>
      <div class="mt-6 flex justify-end space-x-3">
        <BaseButton variant="outline" @click="showEditProjectModal = false">取消</BaseButton>
        <BaseButton variant="primary" :disabled="!newProjectName.trim() || newProjectName === editingProjectName" @click="confirmEditProject">
          儲存
        </BaseButton>
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
          您確定要刪除這個{{ deleteType }}嗎？此操作無法復原。
        </p>
      </div>
      <div class="mt-5 flex justify-end space-x-3">
        <BaseButton variant="outline" @click="showDeleteModal = false">
          取消
        </BaseButton>
        <BaseButton variant="danger" @click="confirmDelete"> 刪除 </BaseButton>
      </div>
    </BaseModal>
  </AdminLayout>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from "vue";
import {
  PlusIcon,
  ClockIcon,
  CheckCircleIcon,
  ExclamationTriangleIcon,
  FolderIcon,
  MagnifyingGlassIcon,
  PlayIcon,
  PauseIcon,
  StopIcon,
  ListBulletIcon,
  ChartBarIcon,
  CalendarDaysIcon,
  DocumentChartBarIcon,
} from "@heroicons/vue/24/outline";
import { useTaskStore } from "@/stores/task";
import { useAuthStore } from "@/stores/auth";
import { projectService } from "@/services/projectService";
import type { WorkTask, WorkTaskStatus, WorkTaskPriority, Project } from "@/types/api";
import AdminLayout from "@/components/layout/AdminLayout.vue";
import BaseButton from "@/components/ui/BaseButton.vue";
import BaseCard from "@/components/ui/BaseCard.vue";
import BaseModal from "@/components/ui/BaseModal.vue";
import WorkTaskForm from "@/components/admin/WorkTaskForm.vue";
import TimeTrackerForm from "@/components/admin/TimeTrackerForm.vue";
import TimeEntryForm from "@/components/admin/TimeEntryForm.vue";
import TasksView from "@/components/work/TasksView.vue";
import ProjectsView from "@/components/work/ProjectsView.vue";
import TimesheetView from "@/components/work/TimesheetView.vue";
import ReportsView from "@/components/work/ReportsView.vue";

// Stores
const taskStore = useTaskStore();
const authStore = useAuthStore();

// Projects
const projectList = ref<Project[]>([]);

// State
const searchQuery = ref("");
const selectedProject = ref("");
const selectedStatus = ref<WorkTaskStatus | "">("");
const selectedPriority = ref<WorkTaskPriority | "">("");
const currentView = ref<"tasks" | "projects" | "timesheet" | "reports">(
  "tasks",
);
const dateRange = ref({
  start: new Date().toISOString().split("T")[0],
  end: new Date().toISOString().split("T")[0],
});
const loading = ref(false);
const showCreateModal = ref(false);
const showTimeTrackerModal = ref(false);
const showTimeEntryModal = ref(false);
const showDeleteModal = ref(false);
const showEditProjectModal = ref(false);
const editingProjectName = ref('');
const newProjectName = ref('');
const editingTask = ref<WorkTask | null>(null);
const editingTimeEntry = ref<any>(null);
const deletingId = ref<number | null>(null);
const deleteType = ref<string>("");

// Active Timer State
const activeTimer = ref<{
  task: WorkTask | null;
  project: string | null;
  startTime: Date;
  elapsed: number;
  isPaused: boolean;
  pausedAt?: Date;
  totalPaused: number;
} | null>(null);

const timerInterval = ref<ReturnType<typeof setInterval> | null>(null);

// Constants
const workViews = [
  { value: "tasks", label: "任務", icon: ListBulletIcon },
  { value: "projects", label: "專案", icon: FolderIcon },
  { value: "timesheet", label: "時間表", icon: CalendarDaysIcon },
  { value: "reports", label: "報表", icon: DocumentChartBarIcon },
];

const taskStatuses = [
  { value: "Pending", label: "待處理" },
  { value: "Planning", label: "規劃中" },
  { value: "InProgress", label: "進行中" },
  { value: "Testing", label: "測試中" },
  { value: "Completed", label: "已完成" },
  { value: "OnHold", label: "暫停" },
  { value: "Cancelled", label: "已取消" },
];

const taskPriorities = [
  { value: "Low", label: "低" },
  { value: "Medium", label: "中" },
  { value: "High", label: "高" },
  { value: "Urgent", label: "緊急" },
];

// Computed
const tasks = computed(() => taskStore.workTasks);
const timeEntries = computed(() => taskStore.timeEntries || []);

const projects = computed(() => {
  return projectList.value.map(p => p.name).sort();
});

const todayWorkedHours = computed(() => {
  const today = new Date().toISOString().split("T")[0];
  const todayEntries = timeEntries.value.filter(
    (entry) => entry.date === today,
  );
  const totalMinutes = todayEntries.reduce(
    (sum, entry) => sum + (entry.duration || 0),
    0,
  );
  return Math.round((totalMinutes / 60) * 10) / 10;
});

const completedTasksCount = computed(() => {
  return tasks.value.filter((task) => task.status === "Completed").length;
});

const overdueTasks = computed(() => {
  const now = new Date();
  return tasks.value.filter(
    (task) =>
      task.dueDate &&
      new Date(task.dueDate) < now &&
      task.status !== "Completed",
  );
});

const activeProjectsCount = computed(() => {
  return projectList.value.length;
});

const filteredTasks = computed(() => {
  let filtered = tasks.value;

  // Search filter
  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase();
    filtered = filtered.filter(
      (task) =>
        task.title.toLowerCase().includes(query) ||
        task.description?.toLowerCase().includes(query) ||
        task.projectName?.toLowerCase().includes(query),
    );
  }

  // Project filter
  if (selectedProject.value) {
    filtered = filtered.filter(
      (task) => task.projectName === selectedProject.value,
    );
  }

  // Status filter
  if (selectedStatus.value !== "") {
    filtered = filtered.filter((task) => task.status === selectedStatus.value);
  }

  // Priority filter
  if (selectedPriority.value !== "") {
    filtered = filtered.filter(
      (task) => task.priority === selectedPriority.value,
    );
  }

  return filtered.sort((a, b) => {
    // Sort by priority, then by due date
    if (a.priority !== b.priority) {
      const priorityOrder: Record<string, number> = {
        Urgent: 4,
        High: 3,
        Medium: 2,
        Low: 1,
      };
      return (
        (priorityOrder[b.priority] || 0) - (priorityOrder[a.priority] || 0)
      );
    }
    if (a.dueDate && b.dueDate) {
      return new Date(a.dueDate).getTime() - new Date(b.dueDate).getTime();
    }
    return 0;
  });
});

const projectStats = computed(() => {
  return projectList.value.map((project) => {
    const projectTasks = tasks.value.filter((task) => task.projectId === project.id);
    const completedTasks = projectTasks.filter(
      (task) => task.status === "Completed",
    );
    const totalHours = projectTasks.reduce(
      (sum, task) => sum + (task.actualHours || 0),
      0,
    );
    const estimatedHours = projectTasks.reduce(
      (sum, task) => sum + (task.estimatedHours || 0),
      0,
    );

    return {
      id: project.id,
      name: project.name,
      totalTasks: projectTasks.length,
      completedTasks: completedTasks.length,
      completionRate:
        projectTasks.length > 0
          ? (completedTasks.length / projectTasks.length) * 100
          : 0,
      totalHours,
      estimatedHours,
      efficiency: estimatedHours > 0 ? (totalHours / estimatedHours) * 100 : 0,
    };
  });
});

// Timer Methods
function startTaskTimer(task: WorkTask) {
  if (activeTimer.value) {
    stopTimer();
  }

  activeTimer.value = {
    task,
    project: task.projectName || null,
    startTime: new Date(),
    elapsed: 0,
    isPaused: false,
    totalPaused: 0,
  };

  startTimerInterval();
}

function startTimerInterval() {
  if (timerInterval.value) {
    clearInterval(timerInterval.value);
  }

  timerInterval.value = setInterval(() => {
    if (activeTimer.value && !activeTimer.value.isPaused) {
      const now = new Date();
      const elapsed =
        now.getTime() -
        activeTimer.value.startTime.getTime() -
        activeTimer.value.totalPaused;
      activeTimer.value.elapsed = Math.floor(elapsed / 1000);
    }
  }, 1000);
}

function pauseTimer() {
  if (activeTimer.value && !activeTimer.value.isPaused) {
    activeTimer.value.isPaused = true;
    activeTimer.value.pausedAt = new Date();
  }
}

function resumeTimer() {
  if (
    activeTimer.value &&
    activeTimer.value.isPaused &&
    activeTimer.value.pausedAt
  ) {
    const pauseDuration =
      new Date().getTime() - activeTimer.value.pausedAt.getTime();
    activeTimer.value.totalPaused += pauseDuration;
    activeTimer.value.isPaused = false;
    activeTimer.value.pausedAt = undefined;
  }
}

async function stopTimer() {
  if (activeTimer.value) {
    const timer = activeTimer.value;
    const durationMinutes = Math.round(timer.elapsed / 60);

    // Save time entry
    await taskStore.createTimeEntry({
      workTaskId: timer.task?.id,
      task: timer.task?.title,
      project: timer.project || undefined,
      date: new Date().toISOString().split("T")[0],
      startTime: timer.startTime.toTimeString().slice(0, 5),
      duration: durationMinutes,
      description: `計時器: ${timer.task?.title}`,
    });

    // Update task actual hours
    if (timer.task) {
      const updatedHours = (timer.task.actualHours || 0) + durationMinutes / 60;
      await taskStore.updateWorkTask(timer.task.id, {
        actualHours: Math.round(updatedHours * 100) / 100,
      });
    }

    clearTimer();
  }
}

function clearTimer() {
  if (timerInterval.value) {
    clearInterval(timerInterval.value);
    timerInterval.value = null;
  }
  activeTimer.value = null;
}

// Utility Methods
function formatDuration(seconds: number): string {
  const hours = Math.floor(seconds / 3600);
  const minutes = Math.floor((seconds % 3600) / 60);
  const secs = seconds % 60;

  if (hours > 0) {
    return `${hours}:${minutes.toString().padStart(2, "0")}:${secs.toString().padStart(2, "0")}`;
  }
  return `${minutes}:${secs.toString().padStart(2, "0")}`;
}

function formatTime(date: Date): string {
  return date.toLocaleTimeString("zh-TW", {
    hour: "2-digit",
    minute: "2-digit",
  });
}

// CRUD Methods
function editTask(task: WorkTask) {
  editingTask.value = { ...task };
  showCreateModal.value = true;
}

function deleteTask(id: number) {
  deletingId.value = id;
  deleteType.value = "任務";
  showDeleteModal.value = true;
}

function editTimeEntry(entry: any) {
  editingTimeEntry.value = { ...entry };
  showTimeEntryModal.value = true;
}

function deleteTimeEntry(id: number) {
  deletingId.value = id;
  deleteType.value = "時間記錄";
  showDeleteModal.value = true;
}

async function toggleTaskComplete(task: WorkTask) {
  const newStatus: WorkTaskStatus =
    task.status === "Completed" ? "InProgress" : "Completed";
  await taskStore.updateWorkTask(task.id, {
    status: newStatus,
    completedAt:
      newStatus === "Completed" ? new Date().toISOString() : undefined,
  });
}

async function handleSave(data: any) {
  try {
    if (editingTask.value) {
      await taskStore.updateWorkTask(editingTask.value.id, data);
    } else {
      await taskStore.createWorkTask(data);
    }
    handleCancel();
  } catch (error) {
    console.error("Save error:", error);
  }
}

async function handleStartTimer(data: any) {
  // Create a temporary task for timer if needed
  if (data.taskId) {
    const task = tasks.value.find((t) => t.id === data.taskId);
    if (task) {
      startTaskTimer(task);
    }
  }
  showTimeTrackerModal.value = false;
}

async function handleTimeEntrySave(data: any) {
  try {
    if (editingTimeEntry.value) {
      await taskStore.updateTimeEntry(editingTimeEntry.value.id, data);
    } else {
      await taskStore.createTimeEntry(data);
    }
    showTimeEntryModal.value = false;
    editingTimeEntry.value = null;
  } catch (error) {
    console.error("Time entry save error:", error);
  }
}

function handleCancel() {
  showCreateModal.value = false;
  editingTask.value = null;
}

async function confirmDelete() {
  if (deletingId.value) {
    try {
      if (deleteType.value === "任務") {
        await taskStore.deleteWorkTask(deletingId.value);
      } else if (deleteType.value === "時間記錄") {
        await taskStore.deleteTimeEntry(deletingId.value);
      }
      showDeleteModal.value = false;
      deletingId.value = null;
    } catch (error) {
      console.error("Delete error:", error);
    }
  }
}

function viewProject(project: any) {
  selectedProject.value = project.name;
  currentView.value = "tasks";
}

function editProject(project: any) {
  editingProjectName.value = project.name;
  newProjectName.value = project.name;
  showEditProjectModal.value = true;
}

async function confirmEditProject() {
  const oldName = editingProjectName.value;
  const newName = newProjectName.value.trim();
  if (!newName || newName === oldName) return;

  const project = projectList.value.find(p => p.name === oldName);
  if (project) {
    await projectService.update(project.id, { name: newName });
    project.name = newName;
  }

  showEditProjectModal.value = false;
}

// Lifecycle
onMounted(async () => {
  await taskStore.fetchWorkTasks();
  await taskStore.fetchTimeEntries?.();
  try {
    const resp = await projectService.getAll();
    projectList.value = resp.data ?? [];
  } catch {
    // silently fail — projects section just won't show
  }
});

onUnmounted(() => {
  clearTimer();
});
</script>
