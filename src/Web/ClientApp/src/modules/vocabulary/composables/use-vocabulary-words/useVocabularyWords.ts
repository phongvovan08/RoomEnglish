import { usePromiseWrapper } from '@/modules/shared/composables/use-promise-wrapper'
import { createAuthHeaders } from '@/utils/auth'
import { useVocabularyStore } from '../../stores/vocabulary/vocabulary'
import { VOCABULARY_API_ENDPOINTS, ERROR_MESSAGES, SUCCESS_MESSAGES } from '../../constants'
import type { VocabularyWord, GetVocabularyWordsQuery } from '../../types'

export function useVocabularyWords() {
  const vocabularyStore = useVocabularyStore()

  // Get words with pagination and search
  const { execute: loadWords, isLoading: isLoadingWords } = usePromiseWrapper({
    key: 'vocabulary-words',
    promiseFn: async (query: GetVocabularyWordsQuery = {}, signal?: AbortSignal) => {
      vocabularyStore.setLoading(true)
      
      try {
        const params = new URLSearchParams()
        params.append('PageNumber', (query.pageNumber || vocabularyStore.currentPage || 1).toString())
        params.append('PageSize', (query.pageSize || vocabularyStore.pageSize || 10).toString())
        params.append('IncludeInactive', (query.includeInactive || false).toString())
        
        if (query.searchTerm || vocabularyStore.searchTerm) {
          params.append('SearchTerm', query.searchTerm || vocabularyStore.searchTerm)
        }
        
        if (query.categoryId || vocabularyStore.selectedCategoryId) {
          params.append('CategoryId', (query.categoryId || vocabularyStore.selectedCategoryId)!.toString())
        }
        
        if (query.difficultyLevel || vocabularyStore.selectedDifficultyLevel) {
          params.append('DifficultyLevel', query.difficultyLevel || vocabularyStore.selectedDifficultyLevel!.toString())
        }
        
        if (query.sortBy || vocabularyStore.sortBy) {
          params.append('SortBy', query.sortBy || vocabularyStore.sortBy)
          params.append('SortOrder', query.sortOrder || vocabularyStore.sortOrder || 'asc')
        }

        const response = await fetch(`${VOCABULARY_API_ENDPOINTS.WORDS}?${params}`, {
          headers: createAuthHeaders(),
          signal
        })

        if (!response.ok) {
          throw new Error(`${ERROR_MESSAGES.LOAD_WORDS}: ${response.status}`)
        }

        const result = await response.json()
        const words = result.items || result || []
        
        vocabularyStore.setWords(words)
        vocabularyStore.setPagination(
          query.pageNumber || vocabularyStore.currentPage,
          query.pageSize || vocabularyStore.pageSize,
          result.totalCount || words.length
        )
        
        return words
      } finally {
        vocabularyStore.setLoading(false)
      }
    }
  })

  // Get word by ID
  const { execute: loadWordById, isLoading: isLoadingWord } = usePromiseWrapper({
    key: 'vocabulary-word-detail',
    promiseFn: async (wordId: number, signal?: AbortSignal) => {
      const response = await fetch(`${VOCABULARY_API_ENDPOINTS.WORDS}/${wordId}`, {
        headers: createAuthHeaders(),
        signal
      })

      if (!response.ok) {
        throw new Error(`${ERROR_MESSAGES.LOAD_WORDS}: ${response.status}`)
      }

      const word = await response.json()
      vocabularyStore.setCurrentWord(word)
      return word
    }
  })

  // Create word
  const { execute: createWord, isLoading: isCreatingWord } = usePromiseWrapper({
    key: 'create-vocabulary-word',
    promiseFn: async (wordData: Omit<VocabularyWord, 'id' | 'createdAt' | 'categoryName'>) => {
      const response = await fetch(VOCABULARY_API_ENDPOINTS.WORDS, {
        method: 'POST',
        headers: createAuthHeaders(),
        body: JSON.stringify(wordData)
      })

      if (!response.ok) {
        throw new Error(`Không thể tạo từ vựng: ${response.status}`)
      }

      const newWord = await response.json()
      vocabularyStore.addWord(newWord)
      return newWord
    },
    successMessage: 'Tạo từ vựng thành công'
  })

  // Update word
  const { execute: updateWord, isLoading: isUpdatingWord } = usePromiseWrapper({
    key: 'update-vocabulary-word',
    promiseFn: async ({ id, ...wordData }: VocabularyWord) => {
      const response = await fetch(`${VOCABULARY_API_ENDPOINTS.WORDS}/${id}`, {
        method: 'PUT',
        headers: createAuthHeaders(),
        body: JSON.stringify(wordData)
      })

      if (!response.ok) {
        throw new Error(`Không thể cập nhật từ vựng: ${response.status}`)
      }

      const updatedWord = { id, ...wordData }
      vocabularyStore.updateWord(updatedWord)
      return updatedWord
    },
    successMessage: 'Cập nhật từ vựng thành công'
  })

  // Delete word
  const { execute: deleteWord, isLoading: isDeletingWord } = usePromiseWrapper({
    key: 'delete-vocabulary-word',
    promiseFn: async (wordId: number) => {
      const response = await fetch(`${VOCABULARY_API_ENDPOINTS.WORDS}/${wordId}`, {
        method: 'DELETE',
        headers: createAuthHeaders()
      })

      if (!response.ok) {
        throw new Error(`Không thể xóa từ vựng: ${response.status}`)
      }

      vocabularyStore.removeWord(wordId)
      return true
    },
    successMessage: 'Xóa từ vựng thành công'
  })

  // Upload Excel
  const { execute: uploadExcel, isLoading: isUploadingExcel } = usePromiseWrapper({
    key: 'upload-vocabulary-excel',
    promiseFn: async (file: File) => {
      const formData = new FormData()
      formData.append('file', file)

      const response = await fetch(VOCABULARY_API_ENDPOINTS.UPLOAD_EXCEL, {
        method: 'POST',
        headers: { 'Authorization': `Bearer ${localStorage.getItem('token')}` }, // No content-type for FormData
        body: formData
      })

      if (!response.ok) {
        throw new Error(`Không thể upload file Excel: ${response.status}`)
      }

      const result = await response.json()
      
      // Reload current page after successful upload
      await loadWords({
        pageNumber: vocabularyStore.currentPage,
        pageSize: vocabularyStore.pageSize
      })
      
      return result
    },
    successMessage: 'Upload file Excel thành công'
  })

  // Upload JSON
  const { execute: importJson, isLoading: isImportingJson } = usePromiseWrapper({
    key: 'import-vocabulary-json',
    promiseFn: async (jsonData: string) => {
      const response = await fetch(VOCABULARY_API_ENDPOINTS.IMPORT_JSON, {
        method: 'POST',
        headers: createAuthHeaders(),
        body: JSON.stringify({ jsonData })
      })

      if (!response.ok) {
        throw new Error(`Không thể import JSON: ${response.status}`)
      }

      const result = await response.json()
      
      // Reload current page after successful import
      await loadWords({
        pageNumber: vocabularyStore.currentPage,
        pageSize: vocabularyStore.pageSize
      })
      
      return result
    },
    successMessage: 'Import JSON thành công'
  })

  // Download Excel template
  const downloadExcelTemplate = async () => {
    try {
      const response = await fetch(VOCABULARY_API_ENDPOINTS.EXCEL_TEMPLATE, {
        headers: createAuthHeaders()
      })

      if (!response.ok) {
        throw new Error('Không thể tải template Excel')
      }

      const blob = await response.blob()
      const url = window.URL.createObjectURL(blob)
      const a = document.createElement('a')
      a.href = url
      a.download = 'VocabularyTemplate.xlsx'
      document.body.appendChild(a)
      a.click()
      document.body.removeChild(a)
      window.URL.revokeObjectURL(url)
    } catch (error) {
      console.error('Error downloading template:', error)
      throw error
    }
  }

  // Download JSON template
  const downloadJsonTemplate = async () => {
    try {
      const response = await fetch(VOCABULARY_API_ENDPOINTS.JSON_TEMPLATE, {
        headers: createAuthHeaders()
      })

      if (!response.ok) {
        throw new Error('Không thể tải template JSON')
      }

      const jsonData = await response.text()
      const blob = new Blob([jsonData], { type: 'application/json' })
      const url = window.URL.createObjectURL(blob)
      const a = document.createElement('a')
      a.href = url
      a.download = 'VocabularyTemplate.json'
      document.body.appendChild(a)
      a.click()
      document.body.removeChild(a)
      window.URL.revokeObjectURL(url)
    } catch (error) {
      console.error('Error downloading JSON template:', error)
      throw error
    }
  }

  // Search words
  const searchWords = (term: string) => {
    vocabularyStore.setSearchTerm(term)
    return loadWords({ 
      searchTerm: term,
      pageNumber: 1,
      pageSize: vocabularyStore.pageSize 
    })
  }

  // Filter by category
  const filterByCategory = (categoryId: number | null) => {
    vocabularyStore.setFilters(categoryId, vocabularyStore.selectedDifficultyLevel)
    return loadWords({ 
      categoryId: categoryId || undefined,
      pageNumber: 1,
      pageSize: vocabularyStore.pageSize 
    })
  }

  // Filter by difficulty
  const filterByDifficulty = (difficulty: string | null) => {
    const difficultyId = difficulty === 'Easy' ? 1 : difficulty === 'Medium' ? 2 : difficulty === 'Hard' ? 3 : null
    vocabularyStore.setFilters(vocabularyStore.selectedCategoryId, difficultyId)
    return loadWords({ 
      difficultyLevel: difficulty || undefined,
      pageNumber: 1,
      pageSize: vocabularyStore.pageSize 
    })
  }

  // Sort words
  const sortWords = (sortBy: string, sortOrder: 'asc' | 'desc' = 'asc') => {
    vocabularyStore.setSorting(sortBy, sortOrder)
    return loadWords({ 
      sortBy, 
      sortOrder,
      pageNumber: vocabularyStore.currentPage,
      pageSize: vocabularyStore.pageSize 
    })
  }

  // Pagination
  const goToPage = (page: number) => {
    vocabularyStore.setPage(page)
    return loadWords({ 
      pageNumber: page,
      pageSize: vocabularyStore.pageSize 
    })
  }

  const changePageSize = (size: number) => {
    vocabularyStore.setPageSize(size)
    return loadWords({ 
      pageNumber: 1,
      pageSize: size 
    })
  }

  return {
    // State
    words: computed(() => vocabularyStore.words),
    currentWord: computed(() => vocabularyStore.currentWord),
    isLoading: computed(() => vocabularyStore.isLoading || isLoadingWords.value),
    isLoadingWord: readonly(isLoadingWord),
    isCreatingWord: readonly(isCreatingWord),
    isUpdatingWord: readonly(isUpdatingWord),
    isDeletingWord: readonly(isDeletingWord),
    isUploadingExcel: readonly(isUploadingExcel),
    isImportingJson: readonly(isImportingJson),
    
    // Pagination
    currentPage: computed(() => vocabularyStore.currentPage),
    pageSize: computed(() => vocabularyStore.pageSize),
    totalItems: computed(() => vocabularyStore.totalItems),
    totalPages: computed(() => vocabularyStore.totalPages),
    
    // Search & Filter
    searchTerm: computed(() => vocabularyStore.searchTerm),
    selectedCategoryId: computed(() => vocabularyStore.selectedCategoryId),
    selectedDifficultyLevel: computed(() => vocabularyStore.selectedDifficultyLevel),
    
    // Sort
    sortBy: computed(() => vocabularyStore.sortBy),
    sortOrder: computed(() => vocabularyStore.sortOrder),

    // Methods
    loadWords,
    loadWordById,
    createWord,
    updateWord,
    deleteWord,
    uploadExcel,
    importJson,
    downloadExcelTemplate,
    downloadJsonTemplate,
    searchWords,
    filterByCategory,
    filterByDifficulty,
    sortWords,
    goToPage,
    changePageSize
  }
}