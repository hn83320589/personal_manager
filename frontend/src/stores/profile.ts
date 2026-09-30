import { defineStore } from 'pinia'
import { ref } from 'vue'
import profileService from '@/services/profileService'
import type { PersonalProfile } from '@/types/api'

export const useProfileStore = defineStore('profile', () => {
  // State
  const profiles = ref<PersonalProfile[]>([])
  const currentProfile = ref<PersonalProfile | null>(null)
  const publicProfile = ref<PersonalProfile | null>(null)
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  // Actions
  async function fetchProfiles() {
    isLoading.value = true
    error.value = null

    try {
      const response = await profileService.getProfiles()
      if (response.success) {
        profiles.value = response.data || []
      } else {
        error.value = response.message
      }
    } catch (err) {
      error.value = 'Failed to fetch profiles'
      console.error(err)
    } finally {
      isLoading.value = false
    }
  }

  async function fetchProfileById(id: number) {
    isLoading.value = true
    error.value = null

    try {
      const response = await profileService.getProfileById(id)
      if (response.success && response.data) {
        currentProfile.value = response.data
      } else {
        error.value = response.message
      }
    } catch (err) {
      error.value = 'Failed to fetch profile'
      console.error(err)
    } finally {
      isLoading.value = false
    }
  }

  async function fetchPublicProfile(userId: number) {
    isLoading.value = true
    error.value = null

    try {
      const response = await profileService.getProfileByUserId(userId)
      if (response.success && response.data) {
        publicProfile.value = response.data
      } else {
        error.value = response.message
      }
    } catch (err) {
      error.value = 'Failed to fetch public profile'
      console.error(err)
    } finally {
      isLoading.value = false
    }
  }

  async function fetchCurrentUserProfile(userId: number) {
    isLoading.value = true
    error.value = null

    try {
      const response = await profileService.getProfileByUserId(userId)
      if (response.success && response.data) {
        currentProfile.value = response.data
      } else {
        error.value = response.message || 'No profile found for current user'
      }
    } catch (err) {
      error.value = 'Failed to fetch current user profile'
      console.error(err)
    } finally {
      isLoading.value = false
    }
  }

  async function createProfile(profileData: Partial<PersonalProfile>) {
    isLoading.value = true
    error.value = null

    try {
      const response = await profileService.createProfile(profileData)
      if (response.success && response.data) {
        profiles.value.push(response.data)
        return response.data
      } else {
        error.value = response.message
        return null
      }
    } catch (err) {
      error.value = 'Failed to create profile'
      console.error(err)
      return null
    } finally {
      isLoading.value = false
    }
  }

  async function updateProfile(id: number, profileData: Partial<PersonalProfile>) {
    isLoading.value = true
    error.value = null

    try {
      const response = await profileService.updateProfile(id, profileData)
      if (response.success && response.data) {
        const index = profiles.value.findIndex(p => p.id === id)
        if (index !== -1) {
          profiles.value[index] = response.data
        }
        if (currentProfile.value && currentProfile.value.id === id) {
          currentProfile.value = response.data
        }
        return response.data
      } else {
        error.value = response.message
        return null
      }
    } catch (err) {
      error.value = 'Failed to update profile'
      console.error(err)
      return null
    } finally {
      isLoading.value = false
    }
  }

  async function deleteProfile(id: number) {
    isLoading.value = true
    error.value = null

    try {
      const response = await profileService.deleteProfile(id)
      if (response.success) {
        profiles.value = profiles.value.filter(p => p.id !== id)
        if (currentProfile.value && currentProfile.value.id === id) {
          currentProfile.value = null
        }
        return true
      } else {
        error.value = response.message
        return false
      }
    } catch (err) {
      error.value = 'Failed to delete profile'
      console.error(err)
      return false
    } finally {
      isLoading.value = false
    }
  }

  function clearError() {
    error.value = null
  }

  function clearCurrentProfile() {
    currentProfile.value = null
  }

  function clearPublicProfile() {
    publicProfile.value = null
  }

  return {
    // State
    profiles,
    currentProfile,
    publicProfile,
    isLoading,
    error,
    // Actions
    fetchProfiles,
    fetchProfileById,
    fetchPublicProfile,
    fetchCurrentUserProfile,
    createProfile,
    updateProfile,
    deleteProfile,
    clearError,
    clearCurrentProfile,
    clearPublicProfile,
  }
})