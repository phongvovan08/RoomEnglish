import { ref, computed } from 'vue'
import { createAuthHeaders, getAuthToken } from '@/utils/auth'
import { API_CONFIG } from '@/config/api.config'
import type { 
  VocabularyCategory, 
  VocabularyWord, 
  LearningSession,
  GetVocabularyCategoriesQuery,
  GetVocabularyWordsQuery,
  StartLearningSessionCommand,
  CompleteLearningSessionCommand,
  PaginatedList
} from '../types/vocabulary.types'

const API_BASE = '/api'

export const useVocabulary = () => {
  const categories = ref<VocabularyCategory[]>([])
  const words = ref<VocabularyWord[]>([])
  const currentWord = ref<VocabularyWord | null>(null)
  const currentSession = ref<LearningSession | null>(null)
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  // Computed properties
  const totalWords = computed(() => words.value.length)
  const masteredWords = computed(() => 
    words.value.filter(w => w.userProgress?.isMastered).length
  )
  const studiedWords = computed(() => 
    words.value.filter(w => w.userProgress && w.userProgress.totalAttempts > 0).length
  )

  // API calls
  const getCategories = async (query: GetVocabularyCategoriesQuery = {}): Promise<PaginatedList<VocabularyCategory>> => {
    try {
      isLoading.value = true
      error.value = null

      const params = new URLSearchParams()
      params.append('PageNumber', (query.pageNumber || 1).toString())
      params.append('PageSize', (query.pageSize || 10).toString())
      params.append('IncludeInactive', (query.includeInactive || false).toString())

      const response = await fetch(`${API_BASE}/vocabulary-categories?${params}`, {
        headers: createAuthHeaders()
      })

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`)
      }

      const result = await response.json()
      categories.value = result.items || result
      return result
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to fetch categories'
      throw err
    } finally {
      isLoading.value = false
    }
  }

  const getWords = async (query: GetVocabularyWordsQuery = {}): Promise<PaginatedList<VocabularyWord>> => {
    try {
      isLoading.value = true
      error.value = null

      const token = getAuthToken()
      console.log('GetWords - Token value:', token)

      const params = new URLSearchParams()
      if (query.categoryId) params.append('CategoryId', query.categoryId.toString())
      if (query.difficultyLevel) params.append('DifficultyLevel', query.difficultyLevel.toString())
      if (query.searchTerm) params.append('SearchTerm', query.searchTerm)
      params.append('PageNumber', (query.pageNumber || 1).toString())
      params.append('PageSize', (query.pageSize || 10).toString())
      params.append('IncludeInactive', (query.includeInactive || false).toString())
      params.append('IncludeExamples', (query.includeExamples || false).toString())
      params.append('IncludeUserProgress', (query.includeUserProgress || false).toString())

      console.log('GetWords - Request URL:', `${API_BASE}/vocabulary-words?${params}`)
      console.log('GetWords - PageSize:', query.pageSize || 10)

      const response = await fetch(`${API_BASE}/vocabulary-words?${params}`, {
        headers: createAuthHeaders()
      })

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`)
      }

      const result = await response.json()
      words.value = result.items || result
      return result
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to fetch words'
      throw err
    } finally {
      isLoading.value = false
    }
  }

  const getWordDetail = async (id: number): Promise<VocabularyWord> => {
    try {
      isLoading.value = true
      error.value = null

      const response = await fetch(`${API_BASE}/vocabulary-words/${id}`, {
        headers: createAuthHeaders()
      })

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`)
      }

      const word = await response.json()
      currentWord.value = word
      return word
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to fetch word detail'
      throw err
    } finally {
      isLoading.value = false
    }
  }

  const startLearningSession = async (command: StartLearningSessionCommand): Promise<LearningSession> => {
    try {
      isLoading.value = true
      error.value = null

      const response = await fetch(`${API_BASE}/vocabulary-learning/sessions/start`, {
        method: 'POST',
        headers: createAuthHeaders(),
        body: JSON.stringify(command)
      })

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`)
      }

      const session = await response.json()
      currentSession.value = session
      return session
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to start learning session'
      throw err
    } finally {
      isLoading.value = false
    }
  }

  const completeLearningSession = async (command: CompleteLearningSessionCommand): Promise<LearningSession> => {
    try {
      isLoading.value = true
      error.value = null

      const response = await fetch(`${API_BASE}/vocabulary-learning/sessions/${command.sessionId}/complete`, {
        method: 'PUT',
        headers: createAuthHeaders(),
        body: JSON.stringify({
          correctAnswers: command.correctAnswers,
          score: command.score
        })
      })

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`)
      }

      const session = await response.json()
      currentSession.value = session
      return session
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to complete learning session'
      throw err
    } finally {
      isLoading.value = false
    }
  }

  return {
    // State
    categories: readonly(categories),
    words: readonly(words),
    currentWord: readonly(currentWord),
    currentSession: readonly(currentSession),
    isLoading: readonly(isLoading),
    error: readonly(error),

    // Computed
    totalWords,
    masteredWords,
    studiedWords,

    // Actions
    getCategories,
    getWords,
    getWordDetail,
    startLearningSession,
    completeLearningSession,

    // Utilities
    clearError: () => { error.value = null },
    clearCurrentWord: () => { currentWord.value = null },
    clearCurrentSession: () => { currentSession.value = null }
  }
}