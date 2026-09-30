import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { User } from '@/types/api'

export const useUserStore = defineStore('user', () => {
  // State
  const users = ref<User[]>([])
  const currentUser = ref<User | null>(null)
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  // Actions
  async function fetchUsers() {
    isLoading.value = true
    error.value = null

    try {
      // API service will be implemented later
      users.value = []
    } catch (err) {
      error.value = 'Failed to fetch users'
      console.error(err)
    } finally {
      isLoading.value = false
    }
  }

  async function fetchUserById(id: number) {
    isLoading.value = true
    error.value = null

    try {
      // API service will be implemented later
      currentUser.value = null
    } catch (err) {
      error.value = 'Failed to fetch user'
      console.error(err)
    } finally {
      isLoading.value = false
    }
  }

  async function createUser(userData: Partial<User>) {
    isLoading.value = true
    error.value = null

    try {
      // API service will be implemented later
      return true
    } catch (err) {
      error.value = 'Failed to create user'
      console.error(err)
      return false
    } finally {
      isLoading.value = false
    }
  }

  async function updateUser(id: number, userData: Partial<User>) {
    isLoading.value = true
    error.value = null

    try {
      // API service will be implemented later
      return true
    } catch (err) {
      error.value = 'Failed to update user'
      console.error(err)
      return false
    } finally {
      isLoading.value = false
    }
  }

  async function deleteUser(id: number) {
    isLoading.value = true
    error.value = null

    try {
      // API service will be implemented later
      return true
    } catch (err) {
      error.value = 'Failed to delete user'
      console.error(err)
      return false
    } finally {
      isLoading.value = false
    }
  }

  function clearError() {
    error.value = null
  }

  function clearCurrentUser() {
    currentUser.value = null
  }

  return {
    // State
    users,
    currentUser,
    isLoading,
    error,
    // Actions
    fetchUsers,
    fetchUserById,
    createUser,
    updateUser,
    deleteUser,
    clearError,
    clearCurrentUser,
  }
})