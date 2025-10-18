import type { VocabularyCategory, VocabularyWord } from '../../types'

export const useVocabularyStore = defineStore('vocabulary', () => {
  // Categories
  const categories = ref<VocabularyCategory[]>([])
  const currentCategory = ref<VocabularyCategory | null>(null)
  
  // Words
  const words = ref<VocabularyWord[]>([])
  const currentWord = ref<VocabularyWord | null>(null)
  
  // Loading states
  const isLoading = ref(false)
  const isLoadingWords = ref(false)
  
  // Pagination
  const currentPage = ref(1)
  const pageSize = ref(10)
  const totalItems = ref(0)
  const totalPages = ref(0)
  
  // Search & Filters
  const searchTerm = ref('')
  const sortBy = ref('')
  const sortOrder = ref<'asc' | 'desc'>('asc')
  const selectedCategoryId = ref<number | null>(null)
  const selectedDifficultyLevel = ref<number | null>(null)

  // Categories methods
  function setCategories(items: VocabularyCategory[]) {
    categories.value = [...items]
  }
  
  function setCurrentCategory(category: VocabularyCategory | null) {
    currentCategory.value = category
  }
  
  function addCategory(category: VocabularyCategory) {
    categories.value.push(category)
  }
  
  function updateCategory(updatedCategory: VocabularyCategory) {
    const index = categories.value.findIndex(c => c.id === updatedCategory.id)
    if (index !== -1) {
      categories.value[index] = updatedCategory
    }
  }
  
  function removeCategory(categoryId: number) {
    categories.value = categories.value.filter(c => c.id !== categoryId)
  }

  // Words methods
  function setWords(items: VocabularyWord[]) {
    words.value = [...items]
  }
  
  function setCurrentWord(word: VocabularyWord | null) {
    currentWord.value = word
  }
  
  function addWord(word: VocabularyWord) {
    words.value.push(word)
  }
  
  function updateWord(updatedWord: VocabularyWord) {
    const index = words.value.findIndex(w => w.id === updatedWord.id)
    if (index !== -1) {
      words.value[index] = updatedWord
    }
  }
  
  function removeWord(wordId: number) {
    words.value = words.value.filter(w => w.id !== wordId)
  }

  // Pagination methods
  function setPagination(page: number, size: number, total: number) {
    currentPage.value = page
    pageSize.value = size
    totalItems.value = total
    totalPages.value = Math.ceil(total / size)
  }
  
  function setPage(page: number) {
    currentPage.value = page
  }
  
  function setPageSize(size: number) {
    pageSize.value = size
    currentPage.value = 1 // Reset to first page when changing page size
  }

  // Search & Filter methods
  function setSearchTerm(term: string) {
    searchTerm.value = term
    currentPage.value = 1 // Reset to first page when searching
  }
  
  function setSorting(field: string, order: 'asc' | 'desc') {
    sortBy.value = field
    sortOrder.value = order
  }
  
  function setFilters(categoryId: number | null, difficultyLevel: number | null) {
    selectedCategoryId.value = categoryId
    selectedDifficultyLevel.value = difficultyLevel
    currentPage.value = 1 // Reset to first page when filtering
  }

  // Loading methods
  function setLoading(loading: boolean) {
    isLoading.value = loading
  }
  
  function setLoadingWords(loading: boolean) {
    isLoadingWords.value = loading
  }

  // Reset methods
  function resetFilters() {
    searchTerm.value = ''
    sortBy.value = ''
    sortOrder.value = 'asc'
    selectedCategoryId.value = null
    selectedDifficultyLevel.value = null
    currentPage.value = 1
  }
  
  function resetState() {
    categories.value = []
    words.value = []
    currentCategory.value = null
    currentWord.value = null
    resetFilters()
    setPagination(1, 10, 0)
    setLoading(false)
    setLoadingWords(false)
  }

  return {
    // State
    categories: readonly(categories),
    words: readonly(words),
    currentCategory: readonly(currentCategory),
    currentWord: readonly(currentWord),
    isLoading: readonly(isLoading),
    isLoadingWords: readonly(isLoadingWords),
    currentPage: readonly(currentPage),
    pageSize: readonly(pageSize),
    totalItems: readonly(totalItems),
    totalPages: readonly(totalPages),
    searchTerm: readonly(searchTerm),
    sortBy: readonly(sortBy),
    sortOrder: readonly(sortOrder),
    selectedCategoryId: readonly(selectedCategoryId),
    selectedDifficultyLevel: readonly(selectedDifficultyLevel),
    
    // Methods
    setCategories,
    setCurrentCategory,
    addCategory,
    updateCategory,
    removeCategory,
    setWords,
    setCurrentWord,
    addWord,
    updateWord,
    removeWord,
    setPagination,
    setPage,
    setPageSize,
    setSearchTerm,
    setSorting,
    setFilters,
    setLoading,
    setLoadingWords,
    resetFilters,
    resetState
  }
})