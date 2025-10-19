import { defineStore } from 'pinia'
import { ref, computed, reactive } from 'vue'
import type { 
  CategoryManagement, 
  VocabularyManagement, 
  ExampleManagement,
  UploadProgress,
  GridState
} from '../../types'
import { GRID_CONFIG } from '../../constants'

export const useManagementStore = defineStore('management', () => {
  // Categories State
  const categories = ref<CategoryManagement[]>([])
  const categoriesLoading = ref(false)
  const categoriesError = ref<string | null>(null)
  const currentCategory = ref<CategoryManagement | null>(null)

  // Vocabularies State  
  const vocabularies = ref<VocabularyManagement[]>([])
  const vocabulariesLoading = ref(false)
  const vocabulariesError = ref<string | null>(null)
  const currentVocabulary = ref<VocabularyManagement | null>(null)

  // Examples State
  const examples = ref<ExampleManagement[]>([])
  const examplesLoading = ref(false)
  const examplesError = ref<string | null>(null)
  const currentExample = ref<ExampleManagement | null>(null)

  // Grid States for each entity
  const categoriesGrid = reactive<GridState>({
    items: [],
    loading: false,
    currentPage: 1,
    pageSize: GRID_CONFIG.DEFAULT_PAGE_SIZE,
    totalItems: 0,
    totalPages: 0,
    searchTerm: '',
    sortBy: 'name',
    sortOrder: 'asc',
    selectedIds: [],
    filters: {}
  })

  const vocabulariesGrid = reactive<GridState>({
    items: [],
    loading: false,
    currentPage: 1,
    pageSize: GRID_CONFIG.DEFAULT_PAGE_SIZE,
    totalItems: 0,
    totalPages: 0,
    searchTerm: '',
    sortBy: 'word',
    sortOrder: 'asc',
    selectedIds: [],
    filters: {}
  })

  const examplesGrid = reactive<GridState>({
    items: [],
    loading: false,
    currentPage: 1,
    pageSize: GRID_CONFIG.DEFAULT_PAGE_SIZE,
    totalItems: 0,
    totalPages: 0,
    searchTerm: '',
    sortBy: 'exampleText',
    sortOrder: 'asc',
    selectedIds: [],
    filters: {}
  })

  // UI State
  const activeTab = ref<'categories' | 'vocabularies' | 'examples'>('categories')
  const sidebarOpen = ref(true)
  const dialogOpen = ref(false)
  const uploadProgress = ref<UploadProgress | null>(null)

  // Computed
  const totalCategories = computed(() => categories.value.length)
  const totalVocabularies = computed(() => vocabularies.value.length)
  const totalExamples = computed(() => examples.value.length)
  
  const activeCategories = computed(() => 
    categories.value.filter(cat => cat.isActive)
  )
  
  const isLoading = computed(() => 
    categoriesLoading.value || vocabulariesLoading.value || examplesLoading.value
  )

  const hasError = computed(() => 
    categoriesError.value || vocabulariesError.value || examplesError.value
  )

  // Categories Actions
  function setCategories(newCategories: CategoryManagement[]) {
    categories.value = newCategories
    categoriesGrid.items = newCategories
  }

  function addCategory(category: CategoryManagement) {
    categories.value.unshift(category)
    categoriesGrid.items.unshift(category)
    categoriesGrid.totalItems++
  }

  function updateCategory(updatedCategory: CategoryManagement) {
    const index = categories.value.findIndex(cat => cat.id === updatedCategory.id)
    if (index !== -1) {
      categories.value[index] = updatedCategory
      categoriesGrid.items[index] = updatedCategory
    }
  }

  function removeCategory(categoryId: number) {
    categories.value = categories.value.filter(cat => cat.id !== categoryId)
    categoriesGrid.items = categoriesGrid.items.filter(cat => cat.id !== categoryId)
    categoriesGrid.totalItems--
  }

  function setCategoriesLoading(loading: boolean) {
    categoriesLoading.value = loading
    categoriesGrid.loading = loading
  }

  function setCategoriesError(error: string | null) {
    categoriesError.value = error
  }

  function setCurrentCategory(category: CategoryManagement | null) {
    currentCategory.value = category
  }

  // Vocabularies Actions
  function setVocabularies(newVocabularies: VocabularyManagement[]) {
    vocabularies.value = newVocabularies
    vocabulariesGrid.items = newVocabularies
  }

  function addVocabulary(vocabulary: VocabularyManagement) {
    vocabularies.value.unshift(vocabulary)
    vocabulariesGrid.items.unshift(vocabulary)
    vocabulariesGrid.totalItems++
  }

  function updateVocabulary(updatedVocabulary: VocabularyManagement) {
    const index = vocabularies.value.findIndex(voc => voc.id === updatedVocabulary.id)
    if (index !== -1) {
      vocabularies.value[index] = updatedVocabulary
      vocabulariesGrid.items[index] = updatedVocabulary
    }
  }

  function removeVocabulary(vocabularyId: number) {
    vocabularies.value = vocabularies.value.filter(voc => voc.id !== vocabularyId)
    vocabulariesGrid.items = vocabulariesGrid.items.filter(voc => voc.id !== vocabularyId)
    vocabulariesGrid.totalItems--
  }

  function setVocabulariesLoading(loading: boolean) {
    vocabulariesLoading.value = loading
    vocabulariesGrid.loading = loading
  }

  function setVocabulariesError(error: string | null) {
    vocabulariesError.value = error
  }

  function setCurrentVocabulary(vocabulary: VocabularyManagement | null) {
    currentVocabulary.value = vocabulary
  }

  // Examples Actions
  function setExamples(newExamples: ExampleManagement[]) {
    examples.value = newExamples
    examplesGrid.items = newExamples
  }

  function addExample(example: ExampleManagement) {
    examples.value.unshift(example)
    examplesGrid.items.unshift(example)
    examplesGrid.totalItems++
  }

  function updateExample(updatedExample: ExampleManagement) {
    const index = examples.value.findIndex(ex => ex.id === updatedExample.id)
    if (index !== -1) {
      examples.value[index] = updatedExample
      examplesGrid.items[index] = updatedExample
    }
  }

  function removeExample(exampleId: number) {
    examples.value = examples.value.filter(ex => ex.id !== exampleId)
    examplesGrid.items = examplesGrid.items.filter(ex => ex.id !== exampleId)
    examplesGrid.totalItems--
  }

  function setExamplesLoading(loading: boolean) {
    examplesLoading.value = loading
    examplesGrid.loading = loading
  }

  function setExamplesError(error: string | null) {
    examplesError.value = error
  }

  function setCurrentExample(example: ExampleManagement | null) {
    currentExample.value = example
  }

  // Grid Management Actions
  function setCategoriesPagination(page: number, pageSize: number, totalItems: number) {
    categoriesGrid.currentPage = page
    categoriesGrid.pageSize = pageSize
    categoriesGrid.totalItems = totalItems
    categoriesGrid.totalPages = Math.ceil(totalItems / pageSize)
  }

  function setVocabulariesPagination(page: number, pageSize: number, totalItems: number) {
    vocabulariesGrid.currentPage = page
    vocabulariesGrid.pageSize = pageSize
    vocabulariesGrid.totalItems = totalItems
    vocabulariesGrid.totalPages = Math.ceil(totalItems / pageSize)
  }

  function setExamplesPagination(page: number, pageSize: number, totalItems: number) {
    examplesGrid.currentPage = page
    examplesGrid.pageSize = pageSize
    examplesGrid.totalItems = totalItems
    examplesGrid.totalPages = Math.ceil(totalItems / pageSize)
  }

  function setCategoriesSearch(searchTerm: string) {
    categoriesGrid.searchTerm = searchTerm
  }

  function setVocabulariesSearch(searchTerm: string) {
    vocabulariesGrid.searchTerm = searchTerm
  }

  function setExamplesSearch(searchTerm: string) {
    examplesGrid.searchTerm = searchTerm
  }

  function setCategoriesSort(sortBy: string, sortOrder: 'asc' | 'desc') {
    categoriesGrid.sortBy = sortBy
    categoriesGrid.sortOrder = sortOrder
  }

  function setVocabulariesSort(sortBy: string, sortOrder: 'asc' | 'desc') {
    vocabulariesGrid.sortBy = sortBy
    vocabulariesGrid.sortOrder = sortOrder
  }

  function setExamplesSort(sortBy: string, sortOrder: 'asc' | 'desc') {
    examplesGrid.sortBy = sortBy
    examplesGrid.sortOrder = sortOrder
  }

  function setCategoriesFilters(filters: Record<string, any>) {
    categoriesGrid.filters = { ...filters }
  }

  function setVocabulariesFilters(filters: Record<string, any>) {
    vocabulariesGrid.filters = { ...filters }
  }

  function setExamplesFilters(filters: Record<string, any>) {
    examplesGrid.filters = { ...filters }
  }

  // Selection Actions
  function setCategoriesSelection(selectedIds: number[]) {
    categoriesGrid.selectedIds = [...selectedIds]
  }

  function setVocabulariesSelection(selectedIds: number[]) {
    vocabulariesGrid.selectedIds = [...selectedIds]
  }

  function setExamplesSelection(selectedIds: number[]) {
    examplesGrid.selectedIds = [...selectedIds]
  }

  // UI Actions
  function setActiveTab(tab: 'categories' | 'vocabularies' | 'examples') {
    activeTab.value = tab
  }

  function setSidebarOpen(open: boolean) {
    sidebarOpen.value = open
  }

  function setDialogOpen(open: boolean) {
    dialogOpen.value = open
  }

  function setUploadProgress(progress: UploadProgress | null) {
    uploadProgress.value = progress
  }

  // Reset Actions
  function resetCategoriesState() {
    categories.value = []
    categoriesLoading.value = false
    categoriesError.value = null
    currentCategory.value = null
    Object.assign(categoriesGrid, {
      items: [],
      loading: false,
      currentPage: 1,
      pageSize: GRID_CONFIG.DEFAULT_PAGE_SIZE,
      totalItems: 0,
      totalPages: 0,
      searchTerm: '',
      sortBy: 'name',
      sortOrder: 'asc',
      selectedIds: [],
      filters: {}
    })
  }

  function resetVocabulariesState() {
    vocabularies.value = []
    vocabulariesLoading.value = false
    vocabulariesError.value = null
    currentVocabulary.value = null
    Object.assign(vocabulariesGrid, {
      items: [],
      loading: false,
      currentPage: 1,
      pageSize: GRID_CONFIG.DEFAULT_PAGE_SIZE,
      totalItems: 0,
      totalPages: 0,
      searchTerm: '',
      sortBy: 'word',
      sortOrder: 'asc',
      selectedIds: [],
      filters: {}
    })
  }

  function resetExamplesState() {
    examples.value = []
    examplesLoading.value = false
    examplesError.value = null
    currentExample.value = null
    Object.assign(examplesGrid, {
      items: [],
      loading: false,
      currentPage: 1,
      pageSize: GRID_CONFIG.DEFAULT_PAGE_SIZE,
      totalItems: 0,
      totalPages: 0,
      searchTerm: '',
      sortBy: 'exampleText',
      sortOrder: 'asc',
      selectedIds: [],
      filters: {}
    })
  }

  function resetAllStates() {
    resetCategoriesState()
    resetVocabulariesState()
    resetExamplesState()
    activeTab.value = 'categories'
    sidebarOpen.value = true
    dialogOpen.value = false
    uploadProgress.value = null
  }

  return {
    // State
    categories: readonly(categories),
    vocabularies: readonly(vocabularies),
    examples: readonly(examples),
    categoriesLoading: readonly(categoriesLoading),
    vocabulariesLoading: readonly(vocabulariesLoading),
    examplesLoading: readonly(examplesLoading),
    categoriesError: readonly(categoriesError),
    vocabulariesError: readonly(vocabulariesError),
    examplesError: readonly(examplesError),
    currentCategory: readonly(currentCategory),
    currentVocabulary: readonly(currentVocabulary),
    currentExample: readonly(currentExample),
    
    // Grid States
    categoriesGrid: readonly(categoriesGrid),
    vocabulariesGrid: readonly(vocabulariesGrid),
    examplesGrid: readonly(examplesGrid),
    
    // UI State
    activeTab: readonly(activeTab),
    sidebarOpen: readonly(sidebarOpen),
    dialogOpen: readonly(dialogOpen),
    uploadProgress: readonly(uploadProgress),
    
    // Computed
    totalCategories,
    totalVocabularies,
    totalExamples,
    activeCategories,
    isLoading,
    hasError,
    
    // Actions
    setCategories,
    addCategory,
    updateCategory,
    removeCategory,
    setCategoriesLoading,
    setCategoriesError,
    setCurrentCategory,
    
    setVocabularies,
    addVocabulary,
    updateVocabulary,
    removeVocabulary,
    setVocabulariesLoading,
    setVocabulariesError,
    setCurrentVocabulary,
    
    setExamples,
    addExample,
    updateExample,
    removeExample,
    setExamplesLoading,
    setExamplesError,
    setCurrentExample,
    
    setCategoriesPagination,
    setVocabulariesPagination,
    setExamplesPagination,
    setCategoriesSearch,
    setVocabulariesSearch,
    setExamplesSearch,
    setCategoriesSort,
    setVocabulariesSort,
    setExamplesSort,
    setCategoriesFilters,
    setVocabulariesFilters,
    setExamplesFilters,
    setCategoriesSelection,
    setVocabulariesSelection,
    setExamplesSelection,
    
    setActiveTab,
    setSidebarOpen,
    setDialogOpen,
    setUploadProgress,
    
    resetCategoriesState,
    resetVocabulariesState,
    resetExamplesState,
    resetAllStates
  }
})