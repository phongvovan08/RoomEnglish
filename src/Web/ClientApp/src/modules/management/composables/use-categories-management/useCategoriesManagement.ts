import { usePromiseWrapper } from '@/modules/shared/composables/use-promise-wrapper'
import { createAuthHeaders } from '@/utils/auth'
import { useManagementStore } from '../../stores/management/management'
import { MANAGEMENT_API_ENDPOINTS, ERROR_MESSAGES, SUCCESS_MESSAGES } from '../../constants'
import type { CategoryManagement, CategoryForm, GetCategoriesQuery } from '../../types'

export function useCategoriesManagement() {
  const managementStore = useManagementStore()

  // Load categories with pagination and search
  const { execute: loadCategories, isLoading: isLoadingCategories } = usePromiseWrapper({
    key: 'categories-management',
    promiseFn: async (query: GetCategoriesQuery = {}, signal?: AbortSignal) => {
      managementStore.setCategoriesLoading(true)
      
      try {
        const params = new URLSearchParams()
        params.append('PageNumber', (query.pageNumber || managementStore.categoriesGrid.currentPage || 1).toString())
        params.append('PageSize', (query.pageSize || managementStore.categoriesGrid.pageSize || 10).toString())
        params.append('IncludeInactive', (query.includeInactive || false).toString())
        
        if (query.searchTerm || managementStore.categoriesGrid.searchTerm) {
          params.append('SearchTerm', query.searchTerm || managementStore.categoriesGrid.searchTerm)
        }
        
        if (query.sortBy || managementStore.categoriesGrid.sortBy) {
          params.append('SortBy', query.sortBy || managementStore.categoriesGrid.sortBy)
          params.append('SortOrder', query.sortOrder || managementStore.categoriesGrid.sortOrder || 'asc')
        }

        const response = await fetch(`${MANAGEMENT_API_ENDPOINTS.CATEGORIES}?${params}`, {
          headers: createAuthHeaders(),
          signal
        })

        if (!response.ok) {
          throw new Error(`${ERROR_MESSAGES.LOAD_CATEGORIES}: ${response.status}`)
        }

        const result = await response.json()
        const categories = result.items || result || []
        
        managementStore.setCategories(categories)
        managementStore.setCategoriesPagination(
          query.pageNumber || managementStore.categoriesGrid.currentPage,
          query.pageSize || managementStore.categoriesGrid.pageSize,
          result.totalCount || categories.length
        )
        
        return categories
      } finally {
        managementStore.setCategoriesLoading(false)
      }
    }
  })

  // Get category by ID
  const { execute: loadCategoryById, isLoading: isLoadingCategory } = usePromiseWrapper({
    key: 'category-detail',
    promiseFn: async (categoryId: number, signal?: AbortSignal) => {
      const response = await fetch(`${MANAGEMENT_API_ENDPOINTS.CATEGORIES}/${categoryId}`, {
        headers: createAuthHeaders(),
        signal
      })

      if (!response.ok) {
        throw new Error(`${ERROR_MESSAGES.LOAD_CATEGORIES}: ${response.status}`)
      }

      const category = await response.json()
      managementStore.setCurrentCategory(category)
      return category
    }
  })

  // Create category
  const { execute: createCategory, isLoading: isCreatingCategory } = usePromiseWrapper({
    key: 'create-category',
    promiseFn: async (categoryData: CategoryForm) => {
      const response = await fetch(MANAGEMENT_API_ENDPOINTS.CATEGORIES, {
        method: 'POST',
        headers: createAuthHeaders(),
        body: JSON.stringify(categoryData)
      })

      if (!response.ok) {
        throw new Error(`${ERROR_MESSAGES.CREATE_CATEGORY}: ${response.status}`)
      }

      const newCategory = await response.json()
      managementStore.addCategory(newCategory)
      return newCategory
    },
    successMessage: SUCCESS_MESSAGES.CREATE_CATEGORY
  })

  // Update category
  const { execute: updateCategoryInternal, isLoading: isUpdatingCategory } = usePromiseWrapper({
    key: 'update-category',
    promiseFn: async (params: { categoryId: number; categoryData: CategoryForm }) => {
      const { categoryId, categoryData } = params
      const response = await fetch(`${MANAGEMENT_API_ENDPOINTS.CATEGORIES}/${categoryId}`, {
        method: 'PUT',
        headers: createAuthHeaders(),
        body: JSON.stringify(categoryData)
      })

      if (!response.ok) {
        throw new Error(`${ERROR_MESSAGES.UPDATE_CATEGORY}: ${response.status}`)
      }

      const updatedCategory = { id: categoryId, ...categoryData, createdAt: new Date().toISOString() }
      managementStore.updateCategory(updatedCategory)
      return updatedCategory
    },
    successMessage: SUCCESS_MESSAGES.UPDATE_CATEGORY
  })

  const updateCategory = (categoryId: number, categoryData: CategoryForm) => {
    return updateCategoryInternal({ categoryId, categoryData })
  }

  // Delete category
  const { execute: deleteCategory, isLoading: isDeletingCategory } = usePromiseWrapper({
    key: 'delete-category',
    promiseFn: async (categoryId: number) => {
      const response = await fetch(`${MANAGEMENT_API_ENDPOINTS.CATEGORIES}/${categoryId}`, {
        method: 'DELETE',
        headers: createAuthHeaders()
      })

      if (!response.ok) {
        throw new Error(`${ERROR_MESSAGES.DELETE_CATEGORY}: ${response.status}`)
      }

      managementStore.removeCategory(categoryId)
      return true
    },
    successMessage: SUCCESS_MESSAGES.DELETE_CATEGORY
  })

  // Delete multiple categories
  const { execute: deleteMultipleCategories, isLoading: isDeletingMultiple } = usePromiseWrapper({
    key: 'delete-multiple-categories',
    promiseFn: async (categoryIds: number[]) => {
      const promises = categoryIds.map(id => 
        fetch(`${MANAGEMENT_API_ENDPOINTS.CATEGORIES}/${id}`, {
          method: 'DELETE',
          headers: createAuthHeaders()
        })
      )

      const responses = await Promise.allSettled(promises)
      const failedDeletes = responses.filter(result => result.status === 'rejected').length
      
      if (failedDeletes > 0) {
        throw new Error(`Không thể xóa ${failedDeletes} danh mục`)
      }

      categoryIds.forEach(id => managementStore.removeCategory(id))
      return categoryIds.length
    },
    successMessage: 'Xóa các danh mục đã chọn thành công'
  })

  // Search categories
  const searchCategories = (term: string) => {
    managementStore.setCategoriesSearch(term)
    return loadCategories({ 
      searchTerm: term,
      pageNumber: 1,
      pageSize: managementStore.categoriesGrid.pageSize 
    })
  }

  // Sort categories
  const sortCategories = (sortBy: string, sortOrder: 'asc' | 'desc' = 'asc') => {
    managementStore.setCategoriesSort(sortBy, sortOrder)
    return loadCategories({ 
      sortBy, 
      sortOrder,
      pageNumber: managementStore.categoriesGrid.currentPage,
      pageSize: managementStore.categoriesGrid.pageSize 
    })
  }

  // Pagination
  const goToPage = (page: number) => {
    return loadCategories({ 
      pageNumber: page,
      pageSize: managementStore.categoriesGrid.pageSize 
    })
  }

  const changePageSize = (size: number) => {
    return loadCategories({ 
      pageNumber: 1,
      pageSize: size 
    })
  }

  // Refresh data
  const refreshCategories = () => {
    return loadCategories({
      pageNumber: managementStore.categoriesGrid.currentPage,
      pageSize: managementStore.categoriesGrid.pageSize,
      searchTerm: managementStore.categoriesGrid.searchTerm,
      sortBy: managementStore.categoriesGrid.sortBy,
      sortOrder: managementStore.categoriesGrid.sortOrder
    })
  }

  // Selection management
  const selectCategory = (categoryId: number) => {
    const currentSelection = [...managementStore.categoriesGrid.selectedIds]
    if (!currentSelection.includes(categoryId)) {
      currentSelection.push(categoryId)
      managementStore.setCategoriesSelection(currentSelection)
    }
  }

  const deselectCategory = (categoryId: number) => {
    const currentSelection = managementStore.categoriesGrid.selectedIds.filter(id => id !== categoryId)
    managementStore.setCategoriesSelection(currentSelection)
  }

  const selectAllCategories = () => {
    const allIds = managementStore.categories.map(cat => cat.id)
    managementStore.setCategoriesSelection(allIds)
  }

  const clearSelection = () => {
    managementStore.setCategoriesSelection([])
  }

  // Export categories
  const { execute: exportCategories, isLoading: isExporting } = usePromiseWrapper({
    key: 'export-categories',
    promiseFn: async (format: 'excel' | 'csv' = 'excel') => {
      const response = await fetch(`${MANAGEMENT_API_ENDPOINTS.CATEGORIES}/export?format=${format}`, {
        headers: createAuthHeaders()
      })

      if (!response.ok) {
        throw new Error('Không thể xuất dữ liệu danh mục')
      }

      const blob = await response.blob()
      const url = window.URL.createObjectURL(blob)
      const a = document.createElement('a')
      a.href = url
      a.download = `categories.${format}`
      document.body.appendChild(a)
      a.click()
      document.body.removeChild(a)
      window.URL.revokeObjectURL(url)
    },
    successMessage: 'Xuất dữ liệu danh mục thành công'
  })

  return {
    // State
    categories: computed(() => managementStore.categories),
    currentCategory: computed(() => managementStore.currentCategory),
    isLoading: computed(() => managementStore.categoriesLoading || isLoadingCategories.value),
    isLoadingCategory: readonly(isLoadingCategory),
    isCreatingCategory: readonly(isCreatingCategory),
    isUpdatingCategory: readonly(isUpdatingCategory),
    isDeletingCategory: readonly(isDeletingCategory),
    isDeletingMultiple: readonly(isDeletingMultiple),
    isExporting: readonly(isExporting),
    
    // Grid state
    grid: computed(() => managementStore.categoriesGrid),
    
    // Computed
    totalCategories: computed(() => managementStore.totalCategories),
    activeCategories: computed(() => managementStore.activeCategories),
    hasSelection: computed(() => managementStore.categoriesGrid.selectedIds.length > 0),
    
    // Methods
    loadCategories,
    loadCategoryById,
    createCategory,
    updateCategory,
    deleteCategory,
    deleteMultipleCategories,
    searchCategories,
    sortCategories,
    goToPage,
    changePageSize,
    refreshCategories,
    selectCategory,
    deselectCategory,
    selectAllCategories,
    clearSelection,
    exportCategories
  }
}