import { usePromiseWrapper } from '@/modules/shared/composables/use-promise-wrapper'
import { createAuthHeaders } from '@/utils/auth'
import { useManagementStore } from '../../stores/management/management'
import { MANAGEMENT_API_ENDPOINTS, ERROR_MESSAGES, SUCCESS_MESSAGES } from '../../constants'
import type { ExampleManagement, ExampleForm, GetExamplesQuery } from '../../types'

export function useExamplesManagement() {
  const managementStore = useManagementStore()

  // Load examples with pagination and search
  const { execute: loadExamples, isLoading: isLoadingExamples } = usePromiseWrapper({
    key: 'examples-management',
    promiseFn: async (query: GetExamplesQuery = {}, signal?: AbortSignal) => {
      managementStore.setExamplesLoading(true)
      
      try {
        const params = new URLSearchParams()
        params.append('PageNumber', (query.pageNumber || managementStore.examplesGrid.currentPage || 1).toString())
        params.append('PageSize', (query.pageSize || managementStore.examplesGrid.pageSize || 10).toString())
        params.append('IncludeInactive', (query.includeInactive || false).toString())
        
        if (query.searchTerm || managementStore.examplesGrid.searchTerm) {
          params.append('SearchTerm', query.searchTerm || managementStore.examplesGrid.searchTerm)
        }
        
        if (query.vocabularyId || managementStore.examplesGrid.filters.vocabularyId) {
          params.append('VocabularyId', (query.vocabularyId || managementStore.examplesGrid.filters.vocabularyId).toString())
        }
        
        if (query.sortBy || managementStore.examplesGrid.sortBy) {
          params.append('SortBy', query.sortBy || managementStore.examplesGrid.sortBy)
          params.append('SortOrder', query.sortOrder || managementStore.examplesGrid.sortOrder || 'asc')
        }

        const response = await fetch(`${MANAGEMENT_API_ENDPOINTS.EXAMPLES}?${params}`, {
          headers: createAuthHeaders(),
          signal
        })

        if (!response.ok) {
          throw new Error(`${ERROR_MESSAGES.LOAD_EXAMPLES}: ${response.status}`)
        }

        const result = await response.json()
        const examples = result.items || result || []
        
        managementStore.setExamples(examples)
        managementStore.setExamplesPagination(
          query.pageNumber || managementStore.examplesGrid.currentPage,
          query.pageSize || managementStore.examplesGrid.pageSize,
          result.totalCount || examples.length
        )
        
        return examples
      } finally {
        managementStore.setExamplesLoading(false)
      }
    }
  })

  // Get example by ID
  const { execute: loadExampleById, isLoading: isLoadingExample } = usePromiseWrapper({
    key: 'example-detail',
    promiseFn: async (exampleId: number, signal?: AbortSignal) => {
      const response = await fetch(`${MANAGEMENT_API_ENDPOINTS.EXAMPLES}/${exampleId}`, {
        headers: createAuthHeaders(),
        signal
      })

      if (!response.ok) {
        throw new Error(`${ERROR_MESSAGES.LOAD_EXAMPLES}: ${response.status}`)
      }

      const example = await response.json()
      managementStore.setCurrentExample(example)
      return example
    }
  })

  // Create example
  const { execute: createExample, isLoading: isCreatingExample } = usePromiseWrapper({
    key: 'create-example',
    promiseFn: async (exampleData: ExampleForm) => {
      const response = await fetch(MANAGEMENT_API_ENDPOINTS.EXAMPLES, {
        method: 'POST',
        headers: createAuthHeaders(),
        body: JSON.stringify(exampleData)
      })

      if (!response.ok) {
        throw new Error(`${ERROR_MESSAGES.CREATE_EXAMPLE}: ${response.status}`)
      }

      const newExample = await response.json()
      managementStore.addExample(newExample)
      return newExample
    },
    successMessage: SUCCESS_MESSAGES.CREATE_EXAMPLE
  })

  // Update example
  const { execute: updateExampleInternal, isLoading: isUpdatingExample } = usePromiseWrapper({
    key: 'update-example',
    promiseFn: async (params: { exampleId: number; exampleData: ExampleForm }) => {
      const { exampleId, exampleData } = params
      const response = await fetch(`${MANAGEMENT_API_ENDPOINTS.EXAMPLES}/${exampleId}`, {
        method: 'PUT',
        headers: createAuthHeaders(),
        body: JSON.stringify(exampleData)
      })

      if (!response.ok) {
        throw new Error(`${ERROR_MESSAGES.UPDATE_EXAMPLE}: ${response.status}`)
      }

      const updatedExample = { id: exampleId, ...exampleData, createdAt: new Date().toISOString() }
      managementStore.updateExample(updatedExample)
      return updatedExample
    },
    successMessage: SUCCESS_MESSAGES.UPDATE_EXAMPLE
  })

  const updateExample = (exampleId: number, exampleData: ExampleForm) => {
    return updateExampleInternal({ exampleId, exampleData })
  }

  // Delete example
  const { execute: deleteExample, isLoading: isDeletingExample } = usePromiseWrapper({
    key: 'delete-example',
    promiseFn: async (exampleId: number) => {
      const response = await fetch(`${MANAGEMENT_API_ENDPOINTS.EXAMPLES}/${exampleId}`, {
        method: 'DELETE',
        headers: createAuthHeaders()
      })

      if (!response.ok) {
        throw new Error(`${ERROR_MESSAGES.DELETE_EXAMPLE}: ${response.status}`)
      }

      managementStore.removeExample(exampleId)
      return true
    },
    successMessage: SUCCESS_MESSAGES.DELETE_EXAMPLE
  })

  // Upload Excel
  const { execute: uploadExcel, isLoading: isUploadingExcel } = usePromiseWrapper({
    key: 'upload-examples-excel',
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

      const response = await fetch(MANAGEMENT_API_ENDPOINTS.EXAMPLES_UPLOAD, {
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
        refreshExamples()
        managementStore.setUploadProgress(null)
      }, 1000)
      
      return result
    },
    successMessage: SUCCESS_MESSAGES.UPLOAD_SUCCESS
  })

  // Download Excel template
  const downloadExcelTemplate = async () => {
    try {
      const response = await fetch(MANAGEMENT_API_ENDPOINTS.EXAMPLES_TEMPLATE, {
        headers: createAuthHeaders()
      })

      if (!response.ok) {
        throw new Error('Không thể tải template Excel')
      }

      const blob = await response.blob()
      const url = window.URL.createObjectURL(blob)
      const a = document.createElement('a')
      a.href = url
      a.download = 'ExamplesTemplate.xlsx'
      document.body.appendChild(a)
      a.click()
      document.body.removeChild(a)
      window.URL.revokeObjectURL(url)
    } catch (error) {
      console.error('Error downloading template:', error)
      throw error
    }
  }

  // Search examples
  const searchExamples = (term: string) => {
    managementStore.setExamplesSearch(term)
    return loadExamples({ 
      searchTerm: term,
      pageNumber: 1,
      pageSize: managementStore.examplesGrid.pageSize 
    })
  }

  // Filter by vocabulary
  const filterByVocabulary = (vocabularyId: number | null) => {
    const filters = { ...managementStore.examplesGrid.filters, vocabularyId }
    managementStore.setExamplesFilters(filters)
    return loadExamples({ 
      vocabularyId: vocabularyId || undefined,
      pageNumber: 1,
      pageSize: managementStore.examplesGrid.pageSize 
    })
  }

  // Sort examples
  const sortExamples = (sortBy: string, sortOrder: 'asc' | 'desc' = 'asc') => {
    managementStore.setExamplesSort(sortBy, sortOrder)
    return loadExamples({ 
      sortBy, 
      sortOrder,
      pageNumber: managementStore.examplesGrid.currentPage,
      pageSize: managementStore.examplesGrid.pageSize 
    })
  }

  // Pagination
  const goToPage = (page: number) => {
    return loadExamples({ 
      pageNumber: page,
      pageSize: managementStore.examplesGrid.pageSize 
    })
  }

  const changePageSize = (size: number) => {
    return loadExamples({ 
      pageNumber: 1,
      pageSize: size 
    })
  }

  // Refresh data
  const refreshExamples = () => {
    return loadExamples({
      pageNumber: managementStore.examplesGrid.currentPage,
      pageSize: managementStore.examplesGrid.pageSize,
      searchTerm: managementStore.examplesGrid.searchTerm,
      sortBy: managementStore.examplesGrid.sortBy,
      sortOrder: managementStore.examplesGrid.sortOrder,
      vocabularyId: managementStore.examplesGrid.filters.vocabularyId
    })
  }

  return {
    // State
    examples: computed(() => managementStore.examples),
    currentExample: computed(() => managementStore.currentExample),
    isLoading: computed(() => managementStore.examplesLoading || isLoadingExamples.value),
    isLoadingExample: readonly(isLoadingExample),
    isCreatingExample: readonly(isCreatingExample),
    isUpdatingExample: readonly(isUpdatingExample),
    isDeletingExample: readonly(isDeletingExample),
    isUploadingExcel: readonly(isUploadingExcel),
    
    // Grid state
    grid: computed(() => managementStore.examplesGrid),
    uploadProgress: computed(() => managementStore.uploadProgress),
    
    // Computed
    totalExamples: computed(() => managementStore.totalExamples),
    
    // Methods
    loadExamples,
    loadExampleById,
    createExample,
    updateExample,
    deleteExample,
    uploadExcel,
    downloadExcelTemplate,
    searchExamples,
    filterByVocabulary,
    sortExamples,
    goToPage,
    changePageSize,
    refreshExamples
  }
}