<template>
  <div class="min-h-screen bg-gray-50">
    <!-- Hero Section -->
    <section class="bg-white">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
        <div class="text-center">
          <h1 class="text-4xl font-bold text-gray-900 sm:text-5xl lg:text-6xl">
            公開行事曆
          </h1>
          <p class="mt-6 text-xl text-gray-600 max-w-3xl mx-auto">
            瞭解我的公開活動、行程安排與即將到來的活動。
          </p>
        </div>
      </div>
    </section>

    <!-- Calendar Controls -->
    <section class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div class="bg-white rounded-lg shadow-md p-6 mb-8">
        <div
          class="flex flex-col md:flex-row md:items-center justify-between space-y-4 md:space-y-0"
        >
          <!-- View Toggle -->
          <div class="flex items-center space-x-4">
            <label class="text-sm font-medium text-gray-700">檢視：</label>
            <div class="flex rounded-lg border border-gray-300 overflow-hidden">
              <button
                v-for="view in viewOptions"
                :key="view.value"
                :class="[
                  'px-4 py-2 text-sm font-medium transition-colors',
                  currentView === view.value
                    ? 'bg-primary-600 text-white'
                    : 'bg-white text-gray-700 hover:bg-gray-50',
                ]"
                @click="currentView = view.value as 'month' | 'week' | 'list'"
              >
                {{ view.label }}
              </button>
            </div>
          </div>

          <!-- Date Navigation -->
          <div class="flex items-center space-x-4">
            <button
              @click="previousPeriod"
              class="p-2 text-gray-600 hover:text-gray-900 hover:bg-gray-100 rounded-lg transition-colors"
            >
              <ChevronLeftIcon class="w-5 h-5" />
            </button>

            <div
              class="text-lg font-semibold text-gray-900 min-w-48 text-center"
            >
              {{ currentPeriodLabel }}
            </div>

            <button
              @click="nextPeriod"
              class="p-2 text-gray-600 hover:text-gray-900 hover:bg-gray-100 rounded-lg transition-colors"
            >
              <ChevronRightIcon class="w-5 h-5" />
            </button>

            <button
              @click="goToToday"
              class="px-4 py-2 text-sm font-medium text-primary-600 hover:text-primary-700 border border-primary-300 rounded-lg hover:bg-primary-50 transition-colors"
            >
              今天
            </button>
          </div>
        </div>
      </div>

      <!-- Calendar Content -->
      <div v-if="isLoading" class="flex justify-center py-12">
        <LoadingSpinner />
      </div>

      <div v-else class="grid grid-cols-1 lg:grid-cols-4 gap-8">
        <!-- Calendar Main View -->
        <div class="lg:col-span-3">
          <!-- Month View -->
          <div
            v-if="currentView === 'month'"
            class="bg-white rounded-lg shadow-md overflow-hidden"
          >
            <div class="grid grid-cols-7 gap-0 border-b border-gray-200">
              <div
                v-for="day in weekDays"
                :key="day"
                class="p-4 text-center text-sm font-medium text-gray-500 bg-gray-50"
              >
                {{ day }}
              </div>
            </div>

            <div class="grid grid-cols-7 gap-0">
              <div
                v-for="date in calendarDates"
                :key="date.dateString"
                :class="[
                  'min-h-32 p-2 border-r border-b border-gray-200 transition-colors',
                  date.isCurrentMonth ? 'bg-white' : 'bg-gray-50',
                  date.isToday ? 'bg-blue-50' : '',
                  'hover:bg-gray-50',
                ]"
              >
                <div
                  :class="[
                    'text-sm font-medium mb-2',
                    date.isCurrentMonth ? 'text-gray-900' : 'text-gray-400',
                    date.isToday ? 'text-blue-600' : '',
                  ]"
                >
                  {{ date.day }}
                </div>

                <div class="space-y-1">
                  <div
                    v-for="event in date.events"
                    :key="event.id"
                    :class="[
                      'text-xs p-1 rounded truncate cursor-pointer',
                      getEventColor(event.color),
                    ]"
                    @click="selectedEvent = event"
                  >
                    {{ event.title }}
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Week View -->
          <div
            v-else-if="currentView === 'week'"
            class="bg-white rounded-lg shadow-md"
          >
            <div class="p-6">
              <div class="grid grid-cols-8 gap-4">
                <div class="text-sm font-medium text-gray-500">Time</div>
                <div
                  v-for="day in weekDates"
                  :key="day.dateString"
                  class="text-center"
                >
                  <div class="text-sm font-medium text-gray-900">
                    {{ day.dayName }}
                  </div>
                  <div
                    :class="[
                      'text-lg',
                      day.isToday ? 'text-blue-600 font-bold' : 'text-gray-700',
                    ]"
                  >
                    {{ day.day }}
                  </div>
                </div>
              </div>

              <div class="mt-4 space-y-2">
                <div
                  v-for="hour in dayHours"
                  :key="hour"
                  class="grid grid-cols-8 gap-4 py-2 border-t border-gray-100"
                >
                  <div class="text-xs text-gray-500">{{ hour }}:00</div>
                  <div
                    v-for="day in weekDates"
                    :key="`${day.dateString}-${hour}`"
                    class="min-h-8 relative"
                  >
                    <div
                      v-for="event in getEventsForDayHour(day.dateString, hour)"
                      :key="event.id"
                      :class="[
                        'absolute left-0 right-0 text-xs p-1 rounded truncate cursor-pointer',
                        getEventColor(event.color),
                      ]"
                      @click="selectedEvent = event"
                    >
                      {{ event.title }}
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- List View -->
          <div v-else class="space-y-4">
            <div
              v-for="event in upcomingEvents"
              :key="event.id"
              class="bg-white rounded-lg shadow-md p-6 cursor-pointer hover:shadow-lg transition-shadow"
              @click="selectedEvent = event"
            >
              <div class="flex items-start justify-between">
                <div class="flex-1">
                  <h3 class="text-lg font-semibold text-gray-900 mb-2">
                    {{ event.title }}
                  </h3>
                  <p v-if="event.description" class="text-gray-600 mb-3">
                    {{ event.description }}
                  </p>
                  <div
                    class="flex items-center space-x-4 text-sm text-gray-500"
                  >
                    <span class="flex items-center">
                      <CalendarIcon class="w-4 h-4 mr-1" />
                      {{ formatEventDate(event) }}
                    </span>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Sidebar -->
        <div class="lg:col-span-1 space-y-6">
          <!-- Upcoming Events -->
          <BaseCard title="即將到來的活動">
            <div
              v-if="todayEvents.length === 0 && upcomingEvents.length === 0"
              class="text-center py-8"
            >
              <CalendarIcon class="mx-auto h-12 w-12 text-gray-400" />
              <h3 class="mt-2 text-sm font-medium text-gray-900">
                無即將到來的活動
              </h3>
              <p class="mt-1 text-sm text-gray-500">請稍後再來查看更新。</p>
            </div>

            <div v-else class="space-y-4">
              <!-- Today's Events -->
              <div v-if="todayEvents.length > 0">
                <h4 class="text-sm font-semibold text-gray-900 mb-2">今天</h4>
                <div class="space-y-2">
                  <div
                    v-for="event in todayEvents"
                    :key="event.id"
                    class="p-3 rounded-lg border cursor-pointer hover:bg-gray-50 transition-colors"
                    @click="selectedEvent = event"
                  >
                    <div class="font-medium text-sm text-gray-900">
                      {{ event.title }}
                    </div>
                    <div class="text-xs text-gray-500">
                      {{ event.startTime }} -
                      {{ event.endTime || "結束時間待定" }}
                    </div>
                  </div>
                </div>
              </div>

              <!-- Upcoming Events Preview -->
              <div v-if="nextFewEvents.length > 0">
                <h4 class="text-sm font-semibold text-gray-900 mb-2">
                  即將到來
                </h4>
                <div class="space-y-2">
                  <div
                    v-for="event in nextFewEvents"
                    :key="event.id"
                    class="p-3 rounded-lg border cursor-pointer hover:bg-gray-50 transition-colors"
                    @click="selectedEvent = event"
                  >
                    <div class="font-medium text-sm text-gray-900">
                      {{ event.title }}
                    </div>
                    <div class="text-xs text-gray-500">
                      {{ formatShortDate(event.startTime) }}
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </BaseCard>
        </div>
      </div>
    </section>

    <!-- Event Detail Modal -->
    <BaseModal v-model:show="showEventModal" title="活動詳情" size="medium">
      <div v-if="selectedEvent" class="space-y-4">
        <div>
          <h3 class="text-lg font-semibold text-gray-900">
            {{ selectedEvent.title }}
          </h3>
        </div>

        <div v-if="selectedEvent.description" class="text-gray-700">
          {{ selectedEvent.description }}
        </div>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm">
          <div>
            <span class="font-medium text-gray-900">日期：</span>
            <div class="text-gray-600">
              {{ formatEventDate(selectedEvent) }}
            </div>
          </div>
        </div>
      </div>

      <template #footer>
        <BaseButton variant="primary" @click="showEventModal = false">
          關閉
        </BaseButton>
      </template>
    </BaseModal>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed, watch } from "vue";
import {
  ChevronLeftIcon,
  ChevronRightIcon,
  CalendarIcon,
} from "@heroicons/vue/24/outline";
import { useCalendarStore } from "@/stores/calendar";
import LoadingSpinner from "@/components/common/LoadingSpinner.vue";
import BaseCard from "@/components/ui/BaseCard.vue";
import BaseModal from "@/components/ui/BaseModal.vue";
import BaseButton from "@/components/ui/BaseButton.vue";
import type { CalendarEvent } from "@/types/api";

// Stores
const calendarStore = useCalendarStore();

// State
const isLoading = ref(true);
const currentView = ref<"month" | "week" | "list">("month");
const currentDate = ref(new Date());
const selectedEvent = ref<CalendarEvent | null>(null);

// Computed
const showEventModal = computed({
  get: () => !!selectedEvent.value,
  set: (value: boolean) => {
    if (!value) selectedEvent.value = null;
  },
});

const publicEvents = computed(() => calendarStore.publicEvents);
const todayEvents = computed(() => calendarStore.todayEvents);
const upcomingEvents = computed(() => calendarStore.upcomingEvents);

const nextFewEvents = computed(() =>
  upcomingEvents.value
    .filter(
      (event) => !todayEvents.value.some((today) => today.id === event.id),
    )
    .slice(0, 3),
);

// View options
const viewOptions = [
  { value: "month", label: "月" },
  { value: "week", label: "週" },
  { value: "list", label: "清單" },
];

const weekDays = ["日", "一", "二", "三", "四", "五", "六"];

const dayHours = Array.from({ length: 24 }, (_, i) => i);

// Calendar date calculations
const currentPeriodLabel = computed(() => {
  if (currentView.value === "month") {
    return currentDate.value.toLocaleDateString("zh-TW", {
      year: "numeric",
      month: "long",
    });
  } else if (currentView.value === "week") {
    const weekStart = getWeekStart(currentDate.value);
    const weekEnd = new Date(weekStart.getTime() + 6 * 24 * 60 * 60 * 1000);
    return `${weekStart.toLocaleDateString("zh-TW", { month: "short", day: "numeric" })} - ${weekEnd.toLocaleDateString("zh-TW", { month: "short", day: "numeric", year: "numeric" })}`;
  } else {
    return currentDate.value.toLocaleDateString("zh-TW", {
      year: "numeric",
      month: "long",
    });
  }
});

const calendarDates = computed(() => {
  const year = currentDate.value.getFullYear();
  const month = currentDate.value.getMonth();

  const firstDay = new Date(year, month, 1);
  const lastDay = new Date(year, month + 1, 0);
  const startDate = new Date(
    firstDay.getTime() - firstDay.getDay() * 24 * 60 * 60 * 1000,
  );

  const dates = [];
  const today = new Date();

  for (let i = 0; i < 42; i++) {
    const date = new Date(startDate.getTime() + i * 24 * 60 * 60 * 1000);
    const dateString = date.toISOString().split("T")[0];

    dates.push({
      date,
      day: date.getDate(),
      dateString,
      isCurrentMonth: date.getMonth() === month,
      isToday: date.toDateString() === today.toDateString(),
      events: getEventsForDate(dateString),
    });
  }

  return dates;
});

const weekDates = computed(() => {
  const weekStart = getWeekStart(currentDate.value);
  const dates = [];

  for (let i = 0; i < 7; i++) {
    const date = new Date(weekStart.getTime() + i * 24 * 60 * 60 * 1000);
    const dateString = date.toISOString().split("T")[0];

    dates.push({
      date,
      day: date.getDate(),
      dayName: date.toLocaleDateString("zh-TW", { weekday: "short" }),
      dateString,
      isToday: date.toDateString() === new Date().toDateString(),
      events: getEventsForDate(dateString),
    });
  }

  return dates;
});

// Methods
function getWeekStart(date: Date): Date {
  const d = new Date(date);
  const day = d.getDay();
  const diff = d.getDate() - day;
  return new Date(d.setDate(diff));
}

function getEventsForDate(dateString: string): CalendarEvent[] {
  return publicEvents.value.filter((event) =>
    event.startTime?.startsWith(dateString),
  );
}

function getEventsForDayHour(
  dateString: string,
  hour: number,
): CalendarEvent[] {
  return getEventsForDate(dateString).filter((event) => {
    if (!event.startTime) return false;
    const eventDate = new Date(event.startTime);
    return eventDate.getHours() === hour;
  });
}

function getEventColor(color?: string): string {
  if (!color) return "bg-blue-100 text-blue-800";
  return `bg-blue-100 text-blue-800`;
}

function formatEventDate(event: CalendarEvent): string {
  const date = new Date(event.startTime);
  let formatted = date.toLocaleDateString("zh-TW", {
    weekday: "long",
    year: "numeric",
    month: "long",
    day: "numeric",
  });

  formatted += ` ${date.toLocaleTimeString("zh-TW", { hour: "2-digit", minute: "2-digit" })}`;
  if (event.endTime) {
    const endDate = new Date(event.endTime);
    formatted += ` - ${endDate.toLocaleTimeString("zh-TW", { hour: "2-digit", minute: "2-digit" })}`;
  }

  return formatted;
}

function formatShortDate(dateString: string): string {
  const date = new Date(dateString);
  return date.toLocaleDateString("zh-TW", { month: "short", day: "numeric" });
}

function previousPeriod() {
  if (currentView.value === "month") {
    currentDate.value = new Date(
      currentDate.value.getFullYear(),
      currentDate.value.getMonth() - 1,
      1,
    );
  } else if (currentView.value === "week") {
    currentDate.value = new Date(
      currentDate.value.getTime() - 7 * 24 * 60 * 60 * 1000,
    );
  }
}

function nextPeriod() {
  if (currentView.value === "month") {
    currentDate.value = new Date(
      currentDate.value.getFullYear(),
      currentDate.value.getMonth() + 1,
      1,
    );
  } else if (currentView.value === "week") {
    currentDate.value = new Date(
      currentDate.value.getTime() + 7 * 24 * 60 * 60 * 1000,
    );
  }
}

function goToToday() {
  currentDate.value = new Date();
}

// Lifecycle
onMounted(async () => {
  try {
    await calendarStore.fetchPublicEvents();
  } catch (error) {
    console.error("Failed to load calendar events:", error);
  } finally {
    isLoading.value = false;
  }
});

// Watch for view changes
watch(currentView, () => {
  // Reset to current month/week when changing views
  currentDate.value = new Date();
});
</script>

<style scoped>
.calendar-grid {
  display: grid;
  grid-template-columns: repeat(7, 1fr);
}
</style>
