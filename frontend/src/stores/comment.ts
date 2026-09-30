import { defineStore } from "pinia";
import { ref, computed } from "vue";
import type { GuestBookEntry } from "@/types/api";
import commentService from "@/services/commentService";

export const useCommentStore = defineStore("comment", () => {
  // State
  const entries = ref<GuestBookEntry[]>([]);
  const currentEntry = ref<GuestBookEntry | null>(null);
  const isLoading = ref(false);
  const error = ref<string | null>(null);

  // Getters
  const publicEntries = computed(() =>
    entries.value
      .filter((entry) => entry.isApproved)
      .sort(
        (a, b) =>
          new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime(),
      ),
  );

  const pendingEntries = computed(() =>
    entries.value.filter((entry) => !entry.isApproved),
  );

  const approvedEntries = computed(() =>
    entries.value.filter((entry) => entry.isApproved),
  );

  const topLevelEntries = computed(() => publicEntries.value);

  const recentEntries = computed(() => publicEntries.value.slice(0, 10));

  // Helper function - no longer applicable since parentId was removed
  const getRepliesForEntry = (_entryId: number) => {
    return [] as GuestBookEntry[];
  };

  // Actions
  async function fetchEntries() {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await commentService.getAllGuestBookEntries();
      entries.value = response.data;
    } catch (err) {
      error.value = "Failed to fetch guestbook entries";
      console.error(err);
    } finally {
      isLoading.value = false;
    }
  }

  async function fetchPublicEntries() {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await commentService.getApprovedGuestBookEntries();
      entries.value = response.data;
      return response.data;
    } catch (err) {
      error.value = "Failed to fetch public entries";
      console.error(err);
      return [];
    } finally {
      isLoading.value = false;
    }
  }

  async function fetchEntryById(id: number) {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await commentService.getGuestBookEntryById(id);
      currentEntry.value = response.data;
    } catch (err) {
      error.value = "Failed to fetch entry";
      console.error(err);
    } finally {
      isLoading.value = false;
    }
  }

  async function createEntry(entryData: Partial<GuestBookEntry>) {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await commentService.createGuestBookEntry({
        name: entryData.name || "",
        email: entryData.email,
        message: entryData.message || "",
      });
      entries.value.push(response.data);
      return response.data;
    } catch (err) {
      error.value = "Failed to create entry";
      console.error(err);
      return null;
    } finally {
      isLoading.value = false;
    }
  }

  async function createReply(
    _parentId: number,
    entryData: Partial<GuestBookEntry>,
  ) {
    return await createEntry(entryData);
  }

  async function updateEntry(id: number, entryData: Partial<GuestBookEntry>) {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await commentService.updateGuestBookEntry(id, {
        isApproved: entryData.isApproved,
        adminReply: entryData.adminReply,
      });
      const index = entries.value.findIndex((entry) => entry.id === id);
      if (index !== -1) {
        entries.value[index] = response.data;
      }
      return response.data;
    } catch (err) {
      error.value = "Failed to update entry";
      console.error(err);
      return null;
    } finally {
      isLoading.value = false;
    }
  }

  async function approveEntry(id: number) {
    return await updateEntry(id, { isApproved: true });
  }

  async function rejectEntry(id: number) {
    return await updateEntry(id, { isApproved: false });
  }

  async function toggleEntryApproval(id: number) {
    const entry = entries.value.find((e) => e.id === id);
    if (entry) {
      return await updateEntry(id, { isApproved: !entry.isApproved });
    }
    return null;
  }

  async function deleteEntry(id: number) {
    isLoading.value = true;
    error.value = null;

    try {
      await commentService.deleteGuestBookEntry(id);
      entries.value = entries.value.filter((entry) => entry.id !== id);
      return true;
    } catch (err) {
      error.value = "Failed to delete entry";
      console.error(err);
      return false;
    } finally {
      isLoading.value = false;
    }
  }

  async function moderateEntry(
    id: number,
    action: "approve" | "reject" | "delete",
  ) {
    switch (action) {
      case "approve":
        return await approveEntry(id);
      case "reject":
        return await rejectEntry(id);
      case "delete":
        return await deleteEntry(id);
      default:
        return false;
    }
  }

  function clearError() {
    error.value = null;
  }

  function clearCurrentEntry() {
    currentEntry.value = null;
  }

  return {
    // State
    entries,
    currentEntry,
    isLoading,
    error,
    // Getters
    publicEntries,
    pendingEntries,
    approvedEntries,
    topLevelEntries,
    recentEntries,
    // Helper functions
    getRepliesForEntry,
    // Actions
    fetchEntries,
    fetchPublicEntries,
    fetchEntryById,
    createEntry,
    createReply,
    updateEntry,
    approveEntry,
    rejectEntry,
    toggleEntryApproval,
    deleteEntry,
    moderateEntry,
    clearError,
    clearCurrentEntry,
  };
});
