import { usePromiseWrapper } from '@/modules/shared/composables/use-promise-wrapper'
import { createAuthHeaders } from '@/utils/auth'
import { useVocabularyStore } from '../../stores/vocabulary/vocabulary'
import { VOCABULARY_API_ENDPOINTS, ERROR_MESSAGES, SUCCESS_MESSAGES } from '../../constants'
import type { VocabularyCategory, GetVocabularyCategoriesQuery } from '../../types'

export function useVocabularyCategories() {
  const vocabularyStore = useVocabularyStore()

  // Get categories with pagination and search
  const { execute: loadCategories, isLoading: isLoadingCategories } = usePromiseWrapper({
    key: 'vocabulary-categories',
    promiseFn: async (query: GetVocabularyCategoriesQuery = {}, signal?: AbortSignal) => {
      vocabularyStore.setLoading(true)
      
      try {
        const params = new URLSearchParams()
        params.append('PageNumber', (query.pageNumber || vocabularyStore.currentPage || 1).toString())
        params.append('PageSize', (query.pageSize || vocabularyStore.pageSize || 10).toString())
        params.append('IncludeInactive', (query.includeInactive || false).toString())
        
        if (query.searchTerm || vocabularyStore.searchTerm) {
          params.append('SearchTerm', query.searchTerm || vocabularyStore.searchTerm)
        }
        
        if (query.sortBy || vocabularyStore.sortBy) {
          params.append('SortBy', query.sortBy || vocabularyStore.sortBy)
          params.append('SortOrder', query.sortOrder || vocabularyStore.sortOrder || 'asc')
        }

        const response = await fetch(`${VOCABULARY_API_ENDPOINTS.CATEGORIES}?${params}`, {
          headers: createAuthHeaders(),
          signal
        })

        if (!response.ok) {
          throw new Error(`${ERROR_MESSAGES.LOAD_CATEGORIES}: ${response.status}`)
        }

        const result = await response.json()
        const categories = result.items || result || []
        
        vocabularyStore.setCategories(categories)
        vocabularyStore.setPagination(
          query.pageNumber || vocabularyStore.currentPage,
          query.pageSize || vocabularyStore.pageSize,
          result.totalCount || categories.length
        )
        
        return categories
      } finally {
        vocabularyStore.setLoading(false)
      }
    }
  })

  // Get category by ID
  const { execute: loadCategoryById, isLoading: isLoadingCategory } = usePromiseWrapper({
    key: 'vocabulary-category-detail',
    promiseFn: async (categoryId: number, signal?: AbortSignal) => {
      const response = await fetch(`${VOCABULARY_API_ENDPOINTS.CATEGORIES}/${categoryId}`, {
        headers: createAuthHeaders(),
        signal
      })

      if (!response.ok) {
        throw new Error(`${ERROR_MESSAGES.LOAD_CATEGORIES}: ${response.status}`)
      }

      const category = await response.json()
      vocabularyStore.setCurrentCategory(category)
      return category
    }
  })

  // Create category
  const { execute: createCategory, isLoading: isCreatingCategory } = usePromiseWrapper({
    key: 'create-vocabulary-category',
    promiseFn: async (categoryData: Omit<VocabularyCategory, 'id' | 'createdAt' | 'vocabularyCount'>) => {
      const response = await fetch(VOCABULARY_API_ENDPOINTS.CATEGORIES, {
        method: 'POST',
        headers: createAuthHeaders(),
        body: JSON.stringify(categoryData)
      })

      if (!response.ok) {
        throw new Error(`Không thể tạo danh mục: ${response.status}`)
      }

      const newCategory = await response.json()
      vocabularyStore.addCategory(newCategory)
      return newCategory
    },
    successMessage: 'Tạo danh mục thành công'
  })

  // Update category
  const { execute: updateCategory, isLoading: isUpdatingCategory } = usePromiseWrapper({
    key: 'update-vocabulary-category',
    promiseFn: async ({ id, ...categoryData }: VocabularyCategory) => {
      const response = await fetch(`${VOCABULARY_API_ENDPOINTS.CATEGORIES}/${id}`, {
        method: 'PUT',
        headers: createAuthHeaders(),
        body: JSON.stringify(categoryData)
      })

      if (!response.ok) {
        throw new Error(`Không thể cập nhật danh mục: ${response.status}`)
      }

      const updatedCategory = { id, ...categoryData }
      vocabularyStore.updateCategory(updatedCategory)
      return updatedCategory
    },
    successMessage: 'Cập nhật danh mục thành công'
  })

  // Delete category
  const { execute: deleteCategory, isLoading: isDeletingCategory } = usePromiseWrapper({
    key: 'delete-vocabulary-category',
    promiseFn: async (categoryId: number) => {
      const response = await fetch(`${VOCABULARY_API_ENDPOINTS.CATEGORIES}/${categoryId}`, {
        method: 'DELETE',
        headers: createAuthHeaders()
      })

      if (!response.ok) {
        throw new Error(`Không thể xóa danh mục: ${response.status}`)
      }

      vocabularyStore.removeCategory(categoryId)
      return true
    },
    successMessage: 'Xóa danh mục thành công'
  })

  // Search categories
  const searchCategories = (term: string) => {
    vocabularyStore.setSearchTerm(term)
    return loadCategories({ 
      searchTerm: term,
      pageNumber: 1,
      pageSize: vocabularyStore.pageSize 
    })
  }

  // Sort categories
  const sortCategories = (sortBy: string, sortOrder: 'asc' | 'desc' = 'asc') => {
    vocabularyStore.setSorting(sortBy, sortOrder)
    return loadCategories({ 
      sortBy, 
      sortOrder,
      pageNumber: vocabularyStore.currentPage,
      pageSize: vocabularyStore.pageSize 
    })
  }

  // Pagination
  const goToPage = (page: number) => {
    vocabularyStore.setPage(page)
    return loadCategories({ 
      pageNumber: page,
      pageSize: vocabularyStore.pageSize 
    })
  }

  const changePageSize = (size: number) => {
    vocabularyStore.setPageSize(size)
    return loadCategories({ 
      pageNumber: 1,
      pageSize: size 
    })
  }

  return {
    // State
    categories: computed(() => vocabularyStore.categories),
    currentCategory: computed(() => vocabularyStore.currentCategory),
    isLoading: computed(() => vocabularyStore.isLoading || isLoadingCategories.value),
    isLoadingCategory: readonly(isLoadingCategory),
    isCreatingCategory: readonly(isCreatingCategory),
    isUpdatingCategory: readonly(isUpdatingCategory),
    isDeletingCategory: readonly(isDeletingCategory),
    
    // Pagination
    currentPage: computed(() => vocabularyStore.currentPage),
    pageSize: computed(() => vocabularyStore.pageSize),
    totalItems: computed(() => vocabularyStore.totalItems),
    totalPages: computed(() => vocabularyStore.totalPages),
    
    // Search & Sort
    searchTerm: computed(() => vocabularyStore.searchTerm),
    sortBy: computed(() => vocabularyStore.sortBy),
    sortOrder: computed(() => vocabularyStore.sortOrder),

    // Methods
    loadCategories,
    loadCategoryById,
    createCategory,
    updateCategory,
    deleteCategory,
    searchCategories,
    sortCategories,
    goToPage,
    changePageSize
  }
}