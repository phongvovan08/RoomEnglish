import { ref } from 'vue'
import type { UserProgress, UserCategoryProgress } from '../types/vocabulary.types'
import { createAuthHeaders } from '@/utils/auth'

const API_BASE = '/api'

export function useUserProgress() {
  const progress = ref<UserProgress | null>(null)
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  const getUserProgress = async (categoryId?: number) => {
    isLoading.value = true
    error.value = null

    try {
      const url = categoryId 
        ? `${API_BASE}/user-progress/category/${categoryId}`
        : `${API_BASE}/user-progress`
      
      const response = await fetch(url, {
        headers: createAuthHeaders()
      })

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`)
      }

      const result = await response.json()
      progress.value = result
      return result
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to load progress'
      console.error('Failed to load user progress:', err)
      return null
    } finally {
      isLoading.value = false
    }
  }

  const getCategoryProgress = (categoryId: number): UserCategoryProgress | null => {
    if (!progress.value) return null
    return progress.value.categoryProgress.find(p => p.categoryId === categoryId) || null
  }

  const updateWordProgress = async (wordId: number, isCorrect: boolean) => {
    try {
      const response = await fetch(`${API_BASE}/user-progress/word/${wordId}`, {
        method: 'POST',
        headers: createAuthHeaders(),
        body: JSON.stringify({ isCorrect })
      })
      
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`)
      }
    } catch (err) {
      console.error('Failed to update word progress:', err)
    }
  }

  const updateExampleProgress = async (exampleId: number, accuracyPercentage: number) => {
    try {
      const response = await fetch(`${API_BASE}/user-progress/example/${exampleId}`, {
        method: 'POST',
        headers: createAuthHeaders(),
        body: JSON.stringify({ accuracyPercentage })
      })
      
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`)
      }
    } catch (err) {
      console.error('Failed to update example progress:', err)
    }
  }

  const recalculateCategoryProgress = async (categoryId: number) => {
    try {
      const response = await fetch(`${API_BASE}/user-progress/category/${categoryId}/recalculate`, {
        method: 'POST',
        headers: createAuthHeaders(),
        body: JSON.stringify({})
      })
      
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`)
      }
      
      // Reload progress after recalculation
      await getUserProgress(categoryId)
    } catch (err) {
      console.error('Failed to recalculate category progress:', err)
    }
  }

  const getExampleProgress = (exampleId: number) => {
    if (!progress.value) return null
    return progress.value.exampleProgress.find(p => p.exampleId === exampleId) || null
  }

  const getWordProgress = (wordId: number) => {
    if (!progress.value) return null
    return progress.value.wordProgress.find(p => p.wordId === wordId) || null
  }

  return {
    progress,
    isLoading,
    error,
    getUserProgress,
    getCategoryProgress,
    updateWordProgress,
    updateExampleProgress,
    recalculateCategoryProgress,
    getExampleProgress,
    getWordProgress
  }
}
