// Base Entity Types
export interface BaseEntity {
  id: number
  createdAt: string
  isActive: boolean
}

// Category Management Types
export interface CategoryManagement extends BaseEntity {
  name: string
  description?: string
  vocabularyCount?: number
}

export interface CategoryForm {
  name: string
  description?: string
  isActive: boolean
}

export interface GetCategoriesQuery {
  pageNumber?: number
  pageSize?: number
  searchTerm?: string
  includeInactive?: boolean
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
}

// Vocabulary Management Types
export interface VocabularyManagement extends BaseEntity {
  word: string
  pronunciation?: string
  meaning: string
  description?: string
  example?: string
  exampleTranslation?: string
  categoryId?: number
  categoryName?: string
  difficultyLevel: DifficultyLevel
  imagePath?: string
  audioPath?: string
}

export interface VocabularyForm {
  word: string
  pronunciation?: string
  meaning: string
  description?: string
  example?: string
  exampleTranslation?: string
  categoryId?: number
  difficultyLevel: DifficultyLevel
  isActive: boolean
  imagePath?: string
  audioPath?: string
}

export interface GetVocabulariesQuery {
  pageNumber?: number
  pageSize?: number
  searchTerm?: string
  categoryId?: number
  difficultyLevel?: DifficultyLevel
  includeInactive?: boolean
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
}

// Example Management Types
export interface ExampleManagement extends BaseEntity {
  vocabularyId: number
  vocabularyWord?: string
  exampleText: string
  translation: string
  audioPath?: string
}

export interface ExampleForm {
  vocabularyId: number
  exampleText: string
  translation: string
  audioPath?: string
  isActive: boolean
}

export interface GetExamplesQuery {
  pageNumber?: number
  pageSize?: number
  searchTerm?: string
  vocabularyId?: number
  includeInactive?: boolean
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
}

// Grid Management Types
export interface GridColumn {
  key: string
  label: string
  sortable?: boolean
  width?: string
  align?: 'left' | 'center' | 'right'
  type?: 'text' | 'number' | 'date' | 'boolean' | 'enum' | 'actions'
  enumOptions?: { value: any; label: string; color?: string }[]
  formatter?: (value: any) => string
}

export interface GridConfig {
  columns: GridColumn[]
  actions?: {
    view?: boolean
    edit?: boolean
    delete?: boolean
    custom?: Array<{
      icon: string
      label: string
      action: string
      color?: string
      condition?: (item: any) => boolean
    }>
  }
  selection?: {
    enabled: boolean
    multiple?: boolean
  }
  export?: {
    enabled: boolean
    formats?: string[]
  }
  upload?: {
    enabled: boolean
    templateUrl?: string
    acceptedTypes?: string[]
  }
}

export interface GridState {
  items: any[]
  loading: boolean
  currentPage: number
  pageSize: number
  totalItems: number
  totalPages: number
  searchTerm: string
  sortBy: string
  sortOrder: 'asc' | 'desc'
  selectedIds: number[]
  filters: Record<string, any>
}

// Pagination Types
export interface PaginationMeta {
  currentPage: number
  pageSize: number
  totalItems: number
  totalPages: number
  hasPreviousPage: boolean
  hasNextPage: boolean
}

export interface PaginatedResult<T> {
  items: T[]
  meta: PaginationMeta
}

// Filter Types
export interface FilterOption {
  value: any
  label: string
  count?: number
}

export interface FilterConfig {
  key: string
  label: string
  type: 'select' | 'multiselect' | 'date' | 'daterange' | 'boolean' | 'text'
  options?: FilterOption[]
  placeholder?: string
  clearable?: boolean
}

// Search Types
export interface SearchConfig {
  placeholder: string
  debounce?: number
  minLength?: number
  fields?: string[]
  highlight?: boolean
}

// Sort Types
export interface SortOption {
  value: string
  label: string
  direction?: 'asc' | 'desc'
}

export interface SortConfig {
  options: SortOption[]
  defaultSort?: string
  defaultDirection?: 'asc' | 'desc'
  multiSort?: boolean
}

// Upload Types
export interface UploadConfig {
  endpoint: string
  templateEndpoint?: string
  maxSize: number
  acceptedTypes: string[]
  chunkSize?: number
  maxRetry?: number
}

export interface UploadResult {
  success: boolean
  message: string
  processedCount?: number
  errorCount?: number
  errors?: Array<{
    row: number
    field: string
    message: string
  }>
}

export interface UploadProgress {
  loaded: number
  total: number
  percentage: number
  stage: 'uploading' | 'processing' | 'completed' | 'error'
}

// Action Types
export interface GridAction {
  type: 'view' | 'edit' | 'delete' | 'custom'
  icon: string
  label: string
  color?: string
  variant?: 'text' | 'outlined' | 'contained'
  condition?: (item: any) => boolean
  handler: (item: any) => void | Promise<void>
}

export interface BulkAction {
  type: string
  icon: string
  label: string
  color?: string
  variant?: 'text' | 'outlined' | 'contained'
  confirmMessage?: string
  handler: (selectedIds: number[]) => void | Promise<void>
}

// Form Types
export interface FormField {
  key: string
  label: string
  type: 'text' | 'textarea' | 'number' | 'select' | 'multiselect' | 'date' | 'boolean' | 'file'
  required?: boolean
  placeholder?: string
  options?: { value: any; label: string }[]
  validation?: {
    minLength?: number
    maxLength?: number
    pattern?: RegExp
    min?: number
    max?: number
    custom?: (value: any) => boolean | string
  }
}

export interface FormConfig {
  fields: FormField[]
  submitLabel?: string
  cancelLabel?: string
  layout?: 'vertical' | 'horizontal' | 'grid'
  columns?: number
}

// Dialog/Modal Types
export interface DialogConfig {
  title: string
  width?: string
  height?: string
  persistent?: boolean
  scrollable?: boolean
  fullscreen?: boolean
}

// Common Types
export type DifficultyLevel = 'Easy' | 'Medium' | 'Hard'
export type SortDirection = 'asc' | 'desc'
export type LoadingState = 'idle' | 'loading' | 'success' | 'error'

// API Response Types
export interface ApiResponse<T = any> {
  success: boolean
  data: T
  message?: string
  errors?: string[]
}

export interface ApiError {
  status: number
  message: string
  details?: any
}

// Event Types
export interface ManagementEvent {
  type: 'create' | 'update' | 'delete' | 'upload' | 'refresh'
  entity: 'category' | 'vocabulary' | 'example'
  data?: any
  timestamp: Date
}

// State Types
export interface ManagementState {
  categories: {
    items: CategoryManagement[]
    loading: boolean
    error: string | null
    pagination: PaginationMeta
    filters: Record<string, any>
    selectedIds: number[]
  }
  vocabularies: {
    items: VocabularyManagement[]
    loading: boolean
    error: string | null
    pagination: PaginationMeta
    filters: Record<string, any>
    selectedIds: number[]
  }
  examples: {
    items: ExampleManagement[]
    loading: boolean
    error: string | null
    pagination: PaginationMeta
    filters: Record<string, any>
    selectedIds: number[]
  }
  ui: {
    activeTab: string
    sidebarOpen: boolean
    dialogOpen: boolean
    uploadProgress: UploadProgress | null
  }
}