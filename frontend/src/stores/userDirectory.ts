import { defineStore } from 'pinia'
import { ref } from 'vue'
import userDirectoryService from '@/services/userDirectoryService'
import type { ProfileDirectory, PublicUser } from '@/types/api'

export const useUserDirectoryStore = defineStore('userDirectory', () => {
  const profiles = ref<ProfileDirectory[]>([])
  const currentUser = ref<PublicUser | null>(null)
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  async function fetchDirectory() {
    isLoading.value = true
    error.value = null
    try {
      const response = await userDirectoryService.getDirectory()
      if (response.success) {
        profiles.value = response.data || []
      } else {
        error.value = response.message
      }
    } catch (err) {
      error.value = 'Failed to fetch directory'
      console.error(err)
    } finally {
      isLoading.value = false
    }
  }

  async function resolveUsername(username: string): Promise<PublicUser | null> {
    try {
      const response = await userDirectoryService.getPublicUser(username)
      if (response.success && response.data) {
        currentUser.value = response.data
        return response.data
      }
      return null
    } catch (err) {
      console.error('Failed to resolve username:', err)
      return null
    }
  }

  function clearCurrentUser() {
    currentUser.value = null
  }

  return {
    profiles,
    currentUser,
    isLoading,
    error,
    fetchDirectory,
    resolveUsername,
    clearCurrentUser,
  }
})
