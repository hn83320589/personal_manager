import { defineStore } from "pinia";
import { ref, computed } from "vue";
import type {
  TodoItem,
  WorkTask,
  TimeEntry,
  CreateTimeEntryDto,
  UpdateTimeEntryDto,
  TodoPriority,
  WorkTaskStatus,
  WorkTaskPriority,
} from "@/types/api";
import taskService from "@/services/taskService";
import timeEntryService from "@/services/timeEntryService";


export const useTaskStore = defineStore(
  "task",
  () => {
  // State
  const todoItems = ref<TodoItem[]>([]);
  const workTasks = ref<WorkTask[]>([]);
  const timeEntries = ref<TimeEntry[]>([]);
  const currentTodo = ref<TodoItem | null>(null);
  const currentWorkTask = ref<WorkTask | null>(null);
  const isLoading = ref(false);
  const error = ref<string | null>(null);

  // Todo Items Getters
  const completedTodos = computed(() =>
    todoItems.value.filter((todo) => todo.status === "Completed"),
  );

  const pendingTodos = computed(() =>
    todoItems.value.filter((todo) => todo.status !== "Completed"),
  );

  const todosByPriority = computed(() => {
    const priorityMap: Record<TodoPriority, TodoItem[]> = {
      Low: [],
      Medium: [],
      High: [],
    };
    todoItems.value.forEach((todo) => {
      priorityMap[todo.priority].push(todo);
    });
    return priorityMap;
  });

  const overdueTodos = computed(() => {
    const today = new Date().toISOString().split("T")[0];
    return todoItems.value.filter(
      (todo) =>
        todo.dueDate && todo.dueDate < today && todo.status !== "Completed",
    );
  });

  // Work Tasks Getters
  const workTasksByStatus = computed(() => {
    const statusMap: Record<WorkTaskStatus, WorkTask[]> = {
      Pending: [],
      Planning: [],
      InProgress: [],
      Testing: [],
      Completed: [],
      OnHold: [],
      Cancelled: [],
    };
    workTasks.value.forEach((task) => {
      statusMap[task.status].push(task);
    });
    return statusMap;
  });

  const activeWorkTasks = computed(() =>
    workTasks.value.filter(
      (task) =>
        task.status === "InProgress" ||
        task.status === "Planning" ||
        task.status === "Testing",
    ),
  );

  const workTasksByPriority = computed(() => {
    const priorityMap: Record<WorkTaskPriority, WorkTask[]> = {
      Low: [],
      Medium: [],
      High: [],
      Urgent: [],
    };
    workTasks.value.forEach((task) => {
      priorityMap[task.priority].push(task);
    });
    return priorityMap;
  });

  const overdueWorkTasks = computed(() => {
    const today = new Date().toISOString().split("T")[0];
    return workTasks.value.filter(
      (task) =>
        task.dueDate && task.dueDate < today && task.status !== "Completed",
    );
  });

  // Todo Items Actions
  async function fetchTodoItems() {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await taskService.getTodoItems();
      todoItems.value = response.data;
    } catch (err) {
      error.value = "Failed to fetch todo items";
      console.error(err);
    } finally {
      isLoading.value = false;
    }
  }

  async function fetchTodoById(id: number) {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await taskService.getTodoItemById(id);
      currentTodo.value = response.data;
    } catch (err) {
      error.value = "Failed to fetch todo item";
      console.error(err);
    } finally {
      isLoading.value = false;
    }
  }

  async function createTodo(todoData: Partial<TodoItem>) {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await taskService.createTodoItem(todoData);
      todoItems.value.push(response.data);
      return response.data;
    } catch (err) {
      error.value = "Failed to create todo item";
      console.error(err);
      return null;
    } finally {
      isLoading.value = false;
    }
  }

  async function updateTodo(id: number, todoData: Partial<TodoItem>) {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await taskService.updateTodoItem(id, todoData);
      const index = todoItems.value.findIndex((todo) => todo.id === id);
      if (index !== -1) {
        todoItems.value[index] = response.data;
      }
      return response.data;
    } catch (err) {
      error.value = "Failed to update todo item";
      console.error(err);
      return null;
    } finally {
      isLoading.value = false;
    }
  }

  async function toggleTodoCompletion(id: number) {
    const todo = todoItems.value.find((t) => t.id === id);
    if (todo) {
      const isCompleted = todo.status === "Completed";
      const updatedData: Partial<TodoItem> = {
        status: isCompleted ? "Pending" : "Completed",
        completedAt: isCompleted ? undefined : new Date().toISOString(),
      };
      return await updateTodo(id, updatedData);
    }
    return null;
  }

  async function deleteTodo(id: number) {
    isLoading.value = true;
    error.value = null;

    try {
      await taskService.deleteTodoItem(id);
      todoItems.value = todoItems.value.filter((todo) => todo.id !== id);
      return true;
    } catch (err) {
      error.value = "Failed to delete todo item";
      console.error(err);
      return false;
    } finally {
      isLoading.value = false;
    }
  }

  // Work Tasks Actions
  async function fetchWorkTasks() {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await taskService.getWorkTasks();
      workTasks.value = response.data;
    } catch (err) {
      error.value = "Failed to fetch work tasks";
      console.error(err);
    } finally {
      isLoading.value = false;
    }
  }

  async function fetchWorkTaskById(id: number) {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await taskService.getWorkTaskById(id);
      currentWorkTask.value = response.data;
    } catch (err) {
      error.value = "Failed to fetch work task";
      console.error(err);
    } finally {
      isLoading.value = false;
    }
  }

  async function createWorkTask(taskData: Partial<WorkTask>) {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await taskService.createWorkTask(taskData);
      workTasks.value.push(response.data);
      return response.data;
    } catch (err) {
      error.value = "Failed to create work task";
      console.error(err);
      return null;
    } finally {
      isLoading.value = false;
    }
  }

  async function updateWorkTask(id: number, taskData: Partial<WorkTask>) {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await taskService.updateWorkTask(id, taskData);
      const index = workTasks.value.findIndex((task) => task.id === id);
      if (index !== -1) {
        workTasks.value[index] = response.data;
      }
      return response.data;
    } catch (err) {
      error.value = "Failed to update work task";
      console.error(err);
      return null;
    } finally {
      isLoading.value = false;
    }
  }

  async function updateWorkTaskStatus(id: number, status: WorkTaskStatus) {
    return await updateWorkTask(id, { status });
  }

  async function deleteWorkTask(id: number) {
    isLoading.value = true;
    error.value = null;

    try {
      await taskService.deleteWorkTask(id);
      workTasks.value = workTasks.value.filter((task) => task.id !== id);
      return true;
    } catch (err) {
      error.value = "Failed to delete work task";
      console.error(err);
      return false;
    } finally {
      isLoading.value = false;
    }
  }

  function clearError() {
    error.value = null;
  }

  function clearCurrentTodo() {
    currentTodo.value = null;
  }

  function clearCurrentWorkTask() {
    currentWorkTask.value = null;
  }

  // Time Entries Actions (API-backed)
  async function fetchTimeEntries() {
    try {
      const response = await timeEntryService.getAll();
      if (response.success) timeEntries.value = response.data || [];
    } catch (err) {
      console.error("Failed to fetch time entries", err);
    }
  }

  async function createTimeEntry(entryData: Partial<TimeEntry>) {
    try {
      const dto: CreateTimeEntryDto = {
        workTaskId: entryData.workTaskId,
        task: entryData.task || "",
        project: entryData.project,
        date: entryData.date || new Date().toISOString().split("T")[0],
        startTime: entryData.startTime,
        endTime: entryData.endTime,
        duration: entryData.duration || 0,
        description: entryData.description,
      };
      const response = await timeEntryService.create(dto);
      if (response.success && response.data) {
        timeEntries.value.push(response.data);
        return response.data;
      }
    } catch (err) {
      console.error("Failed to create time entry", err);
    }
    return null;
  }

  async function updateTimeEntry(id: number, entryData: Partial<TimeEntry>) {
    try {
      const dto: UpdateTimeEntryDto = {
        workTaskId: entryData.workTaskId,
        task: entryData.task,
        project: entryData.project,
        date: entryData.date,
        startTime: entryData.startTime,
        endTime: entryData.endTime,
        duration: entryData.duration,
        description: entryData.description,
      };
      const response = await timeEntryService.update(id, dto);
      if (response.success && response.data) {
        const index = timeEntries.value.findIndex((e) => e.id === id);
        if (index !== -1) timeEntries.value[index] = response.data;
        return response.data;
      }
    } catch (err) {
      console.error("Failed to update time entry", err);
    }
    return null;
  }

  async function deleteTimeEntry(id: number) {
    try {
      await timeEntryService.delete(id);
      timeEntries.value = timeEntries.value.filter((e) => e.id !== id);
      return true;
    } catch (err) {
      console.error("Failed to delete time entry", err);
      return false;
    }
  }

  return {
    // State
    todoItems,
    workTasks,
    timeEntries,
    currentTodo,
    currentWorkTask,
    isLoading,
    error,
    // Todo Getters
    completedTodos,
    pendingTodos,
    todosByPriority,
    overdueTodos,
    // Work Task Getters
    workTasksByStatus,
    activeWorkTasks,
    workTasksByPriority,
    overdueWorkTasks,
    // Todo Actions
    fetchTodoItems,
    fetchTodoById,
    createTodo,
    updateTodo,
    toggleTodoCompletion,
    deleteTodo,
    // Work Task Actions
    fetchWorkTasks,
    fetchWorkTaskById,
    createWorkTask,
    updateWorkTask,
    updateWorkTaskStatus,
    deleteWorkTask,
    // Time Entry Actions
    fetchTimeEntries,
    createTimeEntry,
    updateTimeEntry,
    deleteTimeEntry,
    // Utilities
    clearError,
    clearCurrentTodo,
    clearCurrentWorkTask,
  };
  },
);
