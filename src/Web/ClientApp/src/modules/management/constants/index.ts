// API Endpoints
export const MANAGEMENT_API_ENDPOINTS = {
  // Categories
  CATEGORIES: '/api/vocabulary-categories',
  CATEGORIES_SEARCH: '/api/vocabulary-categories/search',
  
  // Vocabularies  
  VOCABULARIES: '/api/vocabulary-words',
  VOCABULARIES_UPLOAD: '/api/vocabulary-words/upload-excel',
  VOCABULARIES_TEMPLATE: '/api/vocabulary-words/template.xlsx',
  VOCABULARIES_IMPORT_JSON: '/api/vocabulary-words/import-json',
  VOCABULARIES_IMPORT_WORDS: '/api/vocabulary-words/import-words',
  VOCABULARIES_JSON_TEMPLATE: '/api/vocabulary-words/template.json',
  VOCABULARIES_SEARCH: '/api/vocabulary-words/search',
  
  // Examples
  EXAMPLES: '/api/vocabulary-examples',
  EXAMPLES_UPLOAD: '/api/vocabulary-examples/upload-excel', 
  EXAMPLES_TEMPLATE: '/api/vocabulary-examples/template.xlsx',
  EXAMPLES_IMPORT_JSON: '/api/vocabulary-examples/import-json',
  EXAMPLES_IMPORT_WORDS: '/api/vocabulary-examples/import-words',
  EXAMPLES_JSON_TEMPLATE: '/api/vocabulary-examples/template.json',
  EXAMPLES_SEARCH: '/api/vocabulary-examples/search'
} as const

// Grid Configuration
export const GRID_CONFIG = {
  DEFAULT_PAGE_SIZE: 10,
  PAGE_SIZE_OPTIONS: [5, 10, 20, 50, 100],
  DEFAULT_SORT_ORDER: 'asc' as const,
  SEARCH_DEBOUNCE: 500,
  AUTO_RELOAD_DELAY: 3000
} as const

// Difficulty Levels
export const DIFFICULTY_LEVELS = [
  { value: 'Easy', label: 'Dễ', color: 'success' },
  { value: 'Medium', label: 'Trung bình', color: 'warning' },
  { value: 'Hard', label: 'Khó', color: 'error' }
] as const

// Status Options
export const STATUS_OPTIONS = [
  { value: true, label: 'Hoạt động', color: 'success' },
  { value: false, label: 'Không hoạt động', color: 'error' }
] as const

// Sort Options for Categories
export const CATEGORY_SORT_OPTIONS = [
  { value: 'name', label: 'Tên' },
  { value: 'createdAt', label: 'Ngày tạo' },
  { value: 'vocabularyCount', label: 'Số từ vựng' },
  { value: 'isActive', label: 'Trạng thái' }
] as const

// Sort Options for Vocabularies  
export const VOCABULARY_SORT_OPTIONS = [
  { value: 'word', label: 'Từ vựng' },
  { value: 'meaning', label: 'Nghĩa' },
  { value: 'categoryName', label: 'Danh mục' },
  { value: 'difficultyLevel', label: 'Độ khó' },
  { value: 'createdAt', label: 'Ngày tạo' },
  { value: 'isActive', label: 'Trạng thái' }
] as const

// Sort Options for Examples
export const EXAMPLE_SORT_OPTIONS = [
  { value: 'exampleText', label: 'Câu ví dụ' },
  { value: 'translation', label: 'Dịch nghĩa' },
  { value: 'vocabularyWord', label: 'Từ vựng' },
  { value: 'createdAt', label: 'Ngày tạo' },
  { value: 'isActive', label: 'Trạng thái' }
] as const

// Error Messages
export const ERROR_MESSAGES = {
  LOAD_CATEGORIES: 'Không thể tải danh sách danh mục',
  LOAD_VOCABULARIES: 'Không thể tải danh sách từ vựng',
  LOAD_EXAMPLES: 'Không thể tải danh sách ví dụ',
  CREATE_CATEGORY: 'Không thể tạo danh mục mới',
  CREATE_VOCABULARY: 'Không thể tạo từ vựng mới',
  CREATE_EXAMPLE: 'Không thể tạo ví dụ mới',
  UPDATE_CATEGORY: 'Không thể cập nhật danh mục',
  UPDATE_VOCABULARY: 'Không thể cập nhật từ vựng',
  UPDATE_EXAMPLE: 'Không thể cập nhật ví dụ',
  DELETE_CATEGORY: 'Không thể xóa danh mục',
  DELETE_VOCABULARY: 'Không thể xóa từ vựng',
  DELETE_EXAMPLE: 'Không thể xóa ví dụ',
  UPLOAD_FILE: 'Không thể upload file',
  IMPORT_JSON: 'Không thể import JSON',
  IMPORT_WORDS: 'Không thể import từ vựng',
  DOWNLOAD_TEMPLATE: 'Không thể tải template',
  INVALID_FILE: 'File không hợp lệ',
  NETWORK_ERROR: 'Lỗi kết nối mạng',
  UNAUTHORIZED: 'Không có quyền truy cập',
  VALIDATION_ERROR: 'Dữ liệu không hợp lệ'
} as const

// Success Messages  
export const SUCCESS_MESSAGES = {
  CREATE_CATEGORY: 'Tạo danh mục thành công',
  CREATE_VOCABULARY: 'Tạo từ vựng thành công', 
  CREATE_EXAMPLE: 'Tạo ví dụ thành công',
  UPDATE_CATEGORY: 'Cập nhật danh mục thành công',
  UPDATE_VOCABULARY: 'Cập nhật từ vựng thành công',
  UPDATE_EXAMPLE: 'Cập nhật ví dụ thành công',
  DELETE_CATEGORY: 'Xóa danh mục thành công',
  DELETE_VOCABULARY: 'Xóa từ vựng thành công',
  DELETE_EXAMPLE: 'Xóa ví dụ thành công',
  UPLOAD_SUCCESS: 'Upload file thành công',
  IMPORT_JSON_SUCCESS: 'Import JSON thành công',
  IMPORT_WORDS_SUCCESS: 'Import từ vựng thành công',
  TEMPLATE_DOWNLOADED: 'Tải template thành công'
} as const

// Validation Rules
export const VALIDATION_RULES = {
  CATEGORY_NAME: {
    MIN_LENGTH: 2,
    MAX_LENGTH: 100,
    PATTERN: /^[a-zA-ZÀ-ỹ\s]+$/
  },
  VOCABULARY_WORD: {
    MIN_LENGTH: 1,
    MAX_LENGTH: 50,
    PATTERN: /^[a-zA-Z\s\-\'\.]+$/
  },
  MEANING: {
    MIN_LENGTH: 1,
    MAX_LENGTH: 500
  },
  EXAMPLE: {
    MIN_LENGTH: 5,
    MAX_LENGTH: 1000
  },
  FILE_UPLOAD: {
    MAX_SIZE: 5 * 1024 * 1024, // 5MB
    ALLOWED_TYPES: ['application/vnd.openxmlformats-officedocument.spreadsheetml.sheet']
  }
} as const

// Upload Configuration
export const UPLOAD_CONFIG = {
  CHUNK_SIZE: 1000,
  MAX_RETRY: 3,
  TIMEOUT: 30000,
  SUPPORTED_FORMATS: ['.xlsx'],
  TEMPLATE_HEADERS: {
    VOCABULARY: ['Word', 'Meaning', 'Pronunciation', 'Example', 'ExampleTranslation', 'CategoryName', 'DifficultyLevel'],
    EXAMPLE: ['VocabularyWord', 'ExampleText', 'Translation']
  }
} as const