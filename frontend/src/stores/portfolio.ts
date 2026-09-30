import { defineStore } from 'pinia'
import { ref } from 'vue'
import portfolioService from '@/services/portfolioService'
import type { Portfolio } from '@/types/api'

export const usePortfolioStore = defineStore('portfolio', () => {
  // State
  const portfolios = ref<Portfolio[]>([])
  const currentPortfolio = ref<Portfolio | null>(null)
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  // Actions
  async function fetchPortfolios() {
    isLoading.value = true
    error.value = null

    try {
      const response = await portfolioService.getPortfolios()
      if (response.success) {
        portfolios.value = response.data || []
      } else {
        error.value = response.message
      }
    } catch (err) {
      error.value = 'Failed to fetch portfolios'
      console.error(err)
    } finally {
      isLoading.value = false
    }
  }

  async function fetchPortfolioById(id: number) {
    isLoading.value = true
    error.value = null

    try {
      const response = await portfolioService.getPortfolioById(id)
      if (response.success && response.data) {
        currentPortfolio.value = response.data
      } else {
        error.value = response.message
      }
    } catch (err) {
      error.value = 'Failed to fetch portfolio'
      console.error(err)
    } finally {
      isLoading.value = false
    }
  }

  async function createPortfolio(data: Partial<Portfolio>) {
    isLoading.value = true
    error.value = null

    try {
      const response = await portfolioService.createPortfolio(data)
      if (response.success && response.data) {
        portfolios.value.push(response.data)
        return response.data
      } else {
        error.value = response.message
        throw new Error(response.message)
      }
    } catch (err) {
      error.value = 'Failed to create portfolio'
      console.error(err)
      throw err
    } finally {
      isLoading.value = false
    }
  }

  async function updatePortfolio(id: number, data: Partial<Portfolio>) {
    isLoading.value = true
    error.value = null

    try {
      const response = await portfolioService.updatePortfolio(id, data)
      if (response.success && response.data) {
        const index = portfolios.value.findIndex(p => p.id === id)
        if (index !== -1) {
          portfolios.value[index] = response.data
        }
        if (currentPortfolio.value?.id === id) {
          currentPortfolio.value = response.data
        }
        return response.data
      } else {
        error.value = response.message
        throw new Error(response.message)
      }
    } catch (err) {
      error.value = 'Failed to update portfolio'
      console.error(err)
      throw err
    } finally {
      isLoading.value = false
    }
  }

  async function deletePortfolio(id: number) {
    isLoading.value = true
    error.value = null

    try {
      const response = await portfolioService.deletePortfolio(id)
      if (response.success) {
        portfolios.value = portfolios.value.filter(p => p.id !== id)
        if (currentPortfolio.value?.id === id) {
          currentPortfolio.value = null
        }
      } else {
        error.value = response.message
        throw new Error(response.message)
      }
    } catch (err) {
      error.value = 'Failed to delete portfolio'
      console.error(err)
      throw err
    } finally {
      isLoading.value = false
    }
  }

  function clearError() {
    error.value = null
  }

  function clearCurrentPortfolio() {
    currentPortfolio.value = null
  }

  return {
    // State
    portfolios,
    currentPortfolio,
    isLoading,
    error,
    // Actions
    fetchPortfolios,
    fetchPortfolioById,
    createPortfolio,
    updatePortfolio,
    deletePortfolio,
    clearError,
    clearCurrentPortfolio,
  }
})
