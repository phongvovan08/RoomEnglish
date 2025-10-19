import { usePromiseWrapper } from '@/modules/shared/composables/use-promise-wrapper'
import { createAuthHeaders } from '@/utils/auth'
import { useManagementStore } from '../../stores/management/management'
import { MANAGEMENT_API_ENDPOINTS, ERROR_MESSAGES, SUCCESS_MESSAGES } from '../../constants'
import type { VocabularyManagement, VocabularyForm, GetVocabulariesQuery } from '../../types'

export function useVocabulariesManagement() {
  const managementStore = useManagementStore()

  // Load vocabularies with pagination and search
  const { execute: loadVocabularies, isLoading: isLoadingVocabularies } = usePromiseWrapper({
    key: 'vocabularies-management',
    promiseFn: async (query: GetVocabulariesQuery = {}, signal?: AbortSignal) => {
      managementStore.setVocabulariesLoading(true)
      
      try {
        const params = new URLSearchParams()
        params.append('PageNumber', (query.pageNumber || managementStore.vocabulariesGrid.currentPage || 1).toString())
        params.append('PageSize', (query.pageSize || managementStore.vocabulariesGrid.pageSize || 10).toString())
        params.append('IncludeInactive', (query.includeInactive || false).toString())
        
        if (query.searchTerm || managementStore.vocabulariesGrid.searchTerm) {
          params.append('SearchTerm', query.searchTerm || managementStore.vocabulariesGrid.searchTerm)
        }
        
        if (query.categoryId || managementStore.vocabulariesGrid.filters.categoryId) {
          params.append('CategoryId', (query.categoryId || managementStore.vocabulariesGrid.filters.categoryId).toString())
        }
        
        if (query.difficultyLevel || managementStore.vocabulariesGrid.filters.difficultyLevel) {
          params.append('DifficultyLevel', query.difficultyLevel || managementStore.vocabulariesGrid.filters.difficultyLevel)
        }
        
        if (query.sortBy || managementStore.vocabulariesGrid.sortBy) {
          params.append('SortBy', query.sortBy || managementStore.vocabulariesGrid.sortBy)
          params.append('SortOrder', query.sortOrder || managementStore.vocabulariesGrid.sortOrder || 'asc')
        }

        const response = await fetch(`${MANAGEMENT_API_ENDPOINTS.VOCABULARIES}?${params}`, {
          headers: createAuthHeaders(),
          signal
        })

        if (!response.ok) {
          throw new Error(`${ERROR_MESSAGES.LOAD_VOCABULARIES}: ${response.status}`)
        }

        const result = await response.json()
        const vocabularies = result.items || result || []
        
        managementStore.setVocabularies(vocabularies)
        managementStore.setVocabulariesPagination(
          query.pageNumber || managementStore.vocabulariesGrid.currentPage,
          query.pageSize || managementStore.vocabulariesGrid.pageSize,
          result.totalCount || vocabularies.length
        )
        
        return vocabularies
      } finally {
        managementStore.setVocabulariesLoading(false)
      }
    }
  })

  // Get vocabulary by ID
  const { execute: loadVocabularyById, isLoading: isLoadingVocabulary } = usePromiseWrapper({
    key: 'vocabulary-detail',
    promiseFn: async (vocabularyId: number, signal?: AbortSignal) => {
      const response = await fetch(`${MANAGEMENT_API_ENDPOINTS.VOCABULARIES}/${vocabularyId}`, {
        headers: createAuthHeaders(),
        signal
      })

      if (!response.ok) {
        throw new Error(`${ERROR_MESSAGES.LOAD_VOCABULARIES}: ${response.status}`)
      }

      const vocabulary = await response.json()
      managementStore.setCurrentVocabulary(vocabulary)
      return vocabulary
    }
  })

  // Create vocabulary
  const { execute: createVocabulary, isLoading: isCreatingVocabulary } = usePromiseWrapper({
    key: 'create-vocabulary',
    promiseFn: async (vocabularyData: VocabularyForm) => {
      const response = await fetch(MANAGEMENT_API_ENDPOINTS.VOCABULARIES, {
        method: 'POST',
        headers: createAuthHeaders(),
        body: JSON.stringify(vocabularyData)
      })

      if (!response.ok) {
        throw new Error(`${ERROR_MESSAGES.CREATE_VOCABULARY}: ${response.status}`)
      }

      const newVocabulary = await response.json()
      managementStore.addVocabulary(newVocabulary)
      return newVocabulary
    },
    successMessage: SUCCESS_MESSAGES.CREATE_VOCABULARY
  })

  // Update vocabulary
  const { execute: updateVocabularyInternal, isLoading: isUpdatingVocabulary } = usePromiseWrapper({
    key: 'update-vocabulary',
    promiseFn: async (params: { vocabularyId: number; vocabularyData: VocabularyForm }) => {
      const { vocabularyId, vocabularyData } = params
      const response = await fetch(`${MANAGEMENT_API_ENDPOINTS.VOCABULARIES}/${vocabularyId}`, {
        method: 'PUT',
        headers: createAuthHeaders(),
        body: JSON.stringify(vocabularyData)
      })

      if (!response.ok) {
        throw new Error(`${ERROR_MESSAGES.UPDATE_VOCABULARY}: ${response.status}`)
      }

      const updatedVocabulary = { id: vocabularyId, ...vocabularyData, createdAt: new Date().toISOString() }
      managementStore.updateVocabulary(updatedVocabulary)
      return updatedVocabulary
    },
    successMessage: SUCCESS_MESSAGES.UPDATE_VOCABULARY
  })

  const updateVocabulary = (vocabularyId: number, vocabularyData: VocabularyForm) => {
    return updateVocabularyInternal({ vocabularyId, vocabularyData })
  }

  // Delete vocabulary
  const { execute: deleteVocabulary, isLoading: isDeletingVocabulary } = usePromiseWrapper({
    key: 'delete-vocabulary',
    promiseFn: async (vocabularyId: number) => {
      const response = await fetch(`${MANAGEMENT_API_ENDPOINTS.VOCABULARIES}/${vocabularyId}`, {
        method: 'DELETE',
        headers: createAuthHeaders()
      })

      if (!response.ok) {
        throw new Error(`${ERROR_MESSAGES.DELETE_VOCABULARY}: ${response.status}`)
      }

      managementStore.removeVocabulary(vocabularyId)
      return true
    },
    successMessage: SUCCESS_MESSAGES.DELETE_VOCABULARY
  })

  // Upload Excel
  const { execute: uploadExcel, isLoading: isUploadingExcel } = usePromiseWrapper({
    key: 'upload-vocabularies-excel',
    promiseFn: async (file: File) => {
      const formData = new FormData()
      formData.append('file', file)

      // Set upload progress
      managementStore.setUploadProgress({
        loaded: 0,
        total: file.size,
        percentage: 0,
        stage: 'uploading'
      })

      const response = await fetch(MANAGEMENT_API_ENDPOINTS.VOCABULARIES_UPLOAD, {
        method: 'POST',
        headers: { 'Authorization': `Bearer ${localStorage.getItem('token')}` },
        body: formData
      })

      if (!response.ok) {
        managementStore.setUploadProgress({
          loaded: file.size,
          total: file.size,
          percentage: 100,
          stage: 'error'
        })
        throw new Error(`${ERROR_MESSAGES.UPLOAD_FILE}: ${response.status}`)
      }

      const result = await response.json()
      
      // Complete upload
      managementStore.setUploadProgress({
        loaded: file.size,
        total: file.size,
        percentage: 100,
        stage: 'completed'
      })
      
      // Auto reload after successful upload
      setTimeout(() => {
        refreshVocabularies()
        managementStore.setUploadProgress(null)
      }, 1000)
      
      return result
    },
    successMessage: SUCCESS_MESSAGES.UPLOAD_SUCCESS
  })

  // Download Excel template
  const downloadExcelTemplate = async () => {
    try {
      const response = await fetch(MANAGEMENT_API_ENDPOINTS.VOCABULARIES_TEMPLATE, {
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

  // Import from JSON
  const { execute: importFromJson, isLoading: isImportingJson } = usePromiseWrapper({
    key: 'import-vocabularies-json',
    promiseFn: async (jsonData: string) => {
      const response = await fetch(MANAGEMENT_API_ENDPOINTS.VOCABULARIES_IMPORT_JSON, {
        method: 'POST',
        headers: createAuthHeaders(),
        body: JSON.stringify({ jsonData })
      })

      if (!response.ok) {
        const errorText = await response.text()
        return JSON.parse(errorText);
      }

      const result = await response.json()
      
      // Auto reload after successful import
      setTimeout(() => {
        refreshVocabularies()
      }, 1000)
      
      return result
    },
    successMessage: SUCCESS_MESSAGES.IMPORT_JSON_SUCCESS
  })

  // Import from Word List (using ChatGPT)
  const { execute: importFromWords, isLoading: isImportingWords } = usePromiseWrapper({
    key: 'import-vocabularies-words',
    promiseFn: async (words: string[]) => {
      const response = await fetch(MANAGEMENT_API_ENDPOINTS.VOCABULARIES_IMPORT_WORDS, {
        method: 'POST',
        headers: createAuthHeaders(),
        body: JSON.stringify({ words })
      })

      if (!response.ok) {
        const errorText = await response.text()
        return JSON.parse(errorText);
      }

      const result = await response.json()
      
      // Auto reload after successful import
      setTimeout(() => {
        refreshVocabularies()
      }, 1000)
      
      return result
    },
    successMessage: SUCCESS_MESSAGES.IMPORT_WORDS_SUCCESS
  })

  // Download JSON Template
  const downloadJsonTemplate = async () => {
    try {
      const response = await fetch(MANAGEMENT_API_ENDPOINTS.VOCABULARIES_JSON_TEMPLATE, {
        headers: createAuthHeaders()
      })

      if (!response.ok) {
        throw new Error('Không thể tải template JSON')
      }

      const jsonText = await response.text()
      const blob = new Blob([jsonText], { type: 'application/json' })
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

  // Search vocabularies
  const searchVocabularies = (term: string) => {
    managementStore.setVocabulariesSearch(term)
    return loadVocabularies({ 
      searchTerm: term,
      pageNumber: 1,
      pageSize: managementStore.vocabulariesGrid.pageSize 
    })
  }

  // Filter by category
  const filterByCategory = (categoryId: number | null) => {
    const filters = { ...managementStore.vocabulariesGrid.filters, categoryId }
    managementStore.setVocabulariesFilters(filters)
    return loadVocabularies({ 
      categoryId: categoryId || undefined,
      pageNumber: 1,
      pageSize: managementStore.vocabulariesGrid.pageSize 
    })
  }

  // Filter by difficulty
  const filterByDifficulty = (difficulty: 'Easy' | 'Medium' | 'Hard' | null) => {
    const filters = { ...managementStore.vocabulariesGrid.filters, difficultyLevel: difficulty }
    managementStore.setVocabulariesFilters(filters)
    return loadVocabularies({ 
      difficultyLevel: difficulty || undefined,
      pageNumber: 1,
      pageSize: managementStore.vocabulariesGrid.pageSize 
    })
  }

  // Sort vocabularies
  const sortVocabularies = (sortBy: string, sortOrder: 'asc' | 'desc' = 'asc') => {
    managementStore.setVocabulariesSort(sortBy, sortOrder)
    return loadVocabularies({ 
      sortBy, 
      sortOrder,
      pageNumber: managementStore.vocabulariesGrid.currentPage,
      pageSize: managementStore.vocabulariesGrid.pageSize 
    })
  }

  // Pagination
  const goToPage = (page: number) => {
    return loadVocabularies({ 
      pageNumber: page,
      pageSize: managementStore.vocabulariesGrid.pageSize 
    })
  }

  const changePageSize = (size: number) => {
    return loadVocabularies({ 
      pageNumber: 1,
      pageSize: size 
    })
  }

  // Refresh data
  const refreshVocabularies = () => {
    return loadVocabularies({
      pageNumber: managementStore.vocabulariesGrid.currentPage,
      pageSize: managementStore.vocabulariesGrid.pageSize,
      searchTerm: managementStore.vocabulariesGrid.searchTerm,
      sortBy: managementStore.vocabulariesGrid.sortBy,
      sortOrder: managementStore.vocabulariesGrid.sortOrder,
      categoryId: managementStore.vocabulariesGrid.filters.categoryId,
      difficultyLevel: managementStore.vocabulariesGrid.filters.difficultyLevel
    })
  }

  return {
    // State
    vocabularies: computed(() => managementStore.vocabularies),
    currentVocabulary: computed(() => managementStore.currentVocabulary),
    isLoading: computed(() => managementStore.vocabulariesLoading || isLoadingVocabularies.value),
    isLoadingVocabulary: readonly(isLoadingVocabulary),
    isCreatingVocabulary: readonly(isCreatingVocabulary),
    isUpdatingVocabulary: readonly(isUpdatingVocabulary),
    isDeletingVocabulary: readonly(isDeletingVocabulary),
    isUploadingExcel: readonly(isUploadingExcel),
    isImportingJson: readonly(isImportingJson),
    isImportingWords: readonly(isImportingWords),
    
    // Grid state
    grid: computed(() => managementStore.vocabulariesGrid),
    uploadProgress: computed(() => managementStore.uploadProgress),
    
    // Computed
    totalVocabularies: computed(() => managementStore.totalVocabularies),
    
    // Methods
    loadVocabularies,
    loadVocabularyById,
    createVocabulary,
    updateVocabulary,
    deleteVocabulary,
    uploadExcel,
    downloadExcelTemplate,
    importFromJson,
    importFromWords,
    downloadJsonTemplate,
    searchVocabularies,
    filterByCategory,
    filterByDifficulty,
    sortVocabularies,
    goToPage,
    changePageSize,
    refreshVocabularies
  }
}