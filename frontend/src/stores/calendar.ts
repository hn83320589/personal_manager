import { defineStore } from "pinia";
import { ref, computed } from "vue";
import type { CalendarEvent } from "@/types/api";
import calendarService from "@/services/calendarService";

export const useCalendarStore = defineStore("calendar", () => {
  // State
  const events = ref<CalendarEvent[]>([]);
  const currentEvent = ref<CalendarEvent | null>(null);
  const selectedDate = ref<string>(new Date().toISOString().split("T")[0]);
  const calendarView = ref<"month" | "week" | "day">("month");
  const isLoading = ref(false);
  const error = ref<string | null>(null);

  // Getters
  const publicEvents = computed(() =>
    events.value.filter((event) => event.isPublic),
  );

  const eventsByDate = computed(() => {
    const eventMap: Record<string, CalendarEvent[]> = {};
    events.value.forEach((event) => {
      const date = event.startTime.split("T")[0];
      if (!eventMap[date]) {
        eventMap[date] = [];
      }
      eventMap[date].push(event);
    });
    return eventMap;
  });

  const todayEvents = computed(() => {
    const today = new Date().toISOString().split("T")[0];
    return eventsByDate.value[today] || [];
  });

  const upcomingEvents = computed(() => {
    const today = new Date().toISOString().split("T")[0];
    return events.value
      .filter((event) => event.startTime.split("T")[0] >= today)
      .sort((a, b) => a.startTime.localeCompare(b.startTime))
      .slice(0, 5);
  });

  const allDayEvents = computed(() =>
    events.value.filter((event) => event.isAllDay),
  );

  // Actions
  async function fetchEvents() {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await calendarService.getCalendarEvents();
      events.value = response.data;
    } catch (err) {
      error.value = "Failed to fetch events";
      console.error(err);
    } finally {
      isLoading.value = false;
    }
  }

  async function fetchEventById(id: number) {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await calendarService.getCalendarEventById(id);
      currentEvent.value = response.data;
    } catch (err) {
      error.value = "Failed to fetch event";
      console.error(err);
    } finally {
      isLoading.value = false;
    }
  }

  async function fetchEventsByDateRange(startDate: string, endDate: string) {
    // Note: Need userId - for now return filtered local events
    return events.value.filter((event) => {
      const eventDate = event.startTime.split("T")[0];
      return eventDate >= startDate && eventDate <= endDate;
    });
  }

  async function fetchPublicEvents() {
    // Return computed public events
    return publicEvents.value;
  }

  async function createEvent(eventData: Partial<CalendarEvent>) {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await calendarService.createCalendarEvent(eventData);
      events.value.push(response.data);
      return response.data;
    } catch (err) {
      error.value = "Failed to create event";
      console.error(err);
      return null;
    } finally {
      isLoading.value = false;
    }
  }

  async function updateEvent(id: number, eventData: Partial<CalendarEvent>) {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await calendarService.updateCalendarEvent(id, eventData);
      const index = events.value.findIndex((event) => event.id === id);
      if (index !== -1) {
        events.value[index] = response.data;
      }
      return response.data;
    } catch (err) {
      error.value = "Failed to update event";
      console.error(err);
      return null;
    } finally {
      isLoading.value = false;
    }
  }

  async function deleteEvent(id: number) {
    isLoading.value = true;
    error.value = null;

    try {
      await calendarService.deleteCalendarEvent(id);
      events.value = events.value.filter((event) => event.id !== id);
      return true;
    } catch (err) {
      error.value = "Failed to delete event";
      console.error(err);
      return false;
    } finally {
      isLoading.value = false;
    }
  }

  function setSelectedDate(date: string) {
    selectedDate.value = date;
  }

  function setCalendarView(view: "month" | "week" | "day") {
    calendarView.value = view;
  }

  function clearError() {
    error.value = null;
  }

  function clearCurrentEvent() {
    currentEvent.value = null;
  }

  return {
    // State
    events,
    currentEvent,
    selectedDate,
    calendarView,
    isLoading,
    error,
    // Getters
    publicEvents,
    eventsByDate,
    todayEvents,
    upcomingEvents,
    allDayEvents,
    // Actions
    fetchEvents,
    fetchEventById,
    fetchEventsByDateRange,
    fetchPublicEvents,
    createEvent,
    updateEvent,
    deleteEvent,
    setSelectedDate,
    setCalendarView,
    clearError,
    clearCurrentEvent,
  };
});
