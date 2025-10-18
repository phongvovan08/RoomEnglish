export interface VocabularyCategory {
  id: number
  name: string
  description?: string
  isActive: boolean
  createdAt: string
  vocabularyCount?: number
}

export interface GetVocabularyCategoriesQuery {
  pageNumber?: number
  pageSize?: number
  searchTerm?: string
  includeInactive?: boolean
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
}

export interface VocabularyWord {
  id: number
  word: string
  pronunciation?: string
  meaning: string
  description?: string
  example?: string
  exampleTranslation?: string
  categoryId?: number
  categoryName?: string
  difficultyLevel: 'Easy' | 'Medium' | 'Hard'
  isActive: boolean
  createdAt: string
  imagePath?: string
  audioPath?: string
}

export interface GetVocabularyWordsQuery {
  pageNumber?: number
  pageSize?: number
  searchTerm?: string
  categoryId?: number
  difficultyLevel?: string
  includeInactive?: boolean
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
}

export interface VocabularyExample {
  id: number
  vocabularyId: number
  vocabularyWord?: string
  exampleText: string
  translation: string
  audioPath?: string
  isActive: boolean
  createdAt: string
}

export interface GetVocabularyExamplesQuery {
  pageNumber?: number
  pageSize?: number
  searchTerm?: string
  vocabularyId?: number
  includeInactive?: boolean
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
}

// Form interfaces
export interface VocabularyCategoryForm {
  name: string
  description?: string
  isActive: boolean
}

export interface VocabularyWordForm {
  word: string
  pronunciation?: string
  meaning: string
  description?: string
  example?: string
  exampleTranslation?: string
  categoryId?: number
  difficultyLevel: 'Easy' | 'Medium' | 'Hard'
  isActive: boolean
  imagePath?: string
  audioPath?: string
}

export interface VocabularyExampleForm {
  vocabularyId: number
  exampleText: string
  translation: string
  audioPath?: string
  isActive: boolean
}

// Filter and sort types
export type DifficultyLevel = 'Easy' | 'Medium' | 'Hard'
export type SortDirection = 'asc' | 'desc'

// API response types
export interface PaginatedResponse<T> {
  items: T[]
  totalCount: number
  pageNumber: number
  pageSize: number
  totalPages: number
  hasPreviousPage: boolean
  hasNextPage: boolean
}

export interface ApiResponse<T = any> {
  success: boolean
  data: T
  message?: string
  errors?: string[]
}

// Upload types
export interface UploadResponse {
  success: boolean
  message: string
  processedCount?: number
  errors?: string[]
}

export interface ExcelTemplateColumn {
  header: string
  key: string
  required: boolean
  type: 'text' | 'number' | 'boolean' | 'enum'
  enumValues?: string[]
}