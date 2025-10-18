// API Endpoints
export const VOCABULARY_API_ENDPOINTS = {
  CATEGORIES: '/api/vocabulary-categories',
  WORDS: '/api/vocabulary-words',
  EXAMPLES: '/api/vocabulary-examples',
  LEARNING: '/api/vocabulary-learning',
  UPLOAD_EXCEL: '/api/vocabulary-words/upload-excel',
  EXCEL_TEMPLATE: '/api/vocabulary-words/template.xlsx',
  EXAMPLES_UPLOAD_EXCEL: '/api/vocabulary-examples/upload-excel',
  EXAMPLES_TEMPLATE: '/api/vocabulary-examples/template.xlsx'
} as const

// Default Values
export const DEFAULT_PAGE_SIZE = 10
export const DEFAULT_DIFFICULTY_LEVEL = 1
export const MAX_DIFFICULTY_LEVEL = 5

// Difficulty Levels
export const DIFFICULTY_LEVELS = [
  { value: 1, label: 'Dễ', color: '#4caf50' },
  { value: 2, label: 'Khá dễ', color: '#8bc34a' },
  { value: 3, label: 'Trung bình', color: '#ff9800' },
  { value: 4, label: 'Khó', color: '#f44336' },
  { value: 5, label: 'Rất khó', color: '#9c27b0' }
] as const

// Part of Speech Options
export const PARTS_OF_SPEECH = [
  'noun',
  'verb', 
  'adjective',
  'adverb',
  'pronoun',
  'preposition',
  'conjunction',
  'interjection',
  'article'
] as const

// Sort Options
export const SORT_OPTIONS = {
  WORD: 'word',
  MEANING: 'meaning',
  DIFFICULTY: 'difficultylevel',
  CREATED: 'createdat',
  CATEGORY: 'category'
} as const

// File Types
export const ALLOWED_FILE_TYPES = {
  EXCEL: ['.xlsx', '.xls'],
  AUDIO: ['.mp3', '.wav', '.ogg']
} as const

// Learning Session Settings
export const LEARNING_SESSION = {
  MIN_WORDS: 5,
  MAX_WORDS: 50,
  DEFAULT_WORDS: 10,
  SESSION_TYPES: {
    FLASHCARD: 'flashcard',
    DICTATION: 'dictation',
    QUIZ: 'quiz',
    MIXED: 'mixed'
  }
} as const

// Local Storage Keys
export const STORAGE_KEYS = {
  LAST_CATEGORY: 'vocabulary_last_category',
  PREFERRED_PAGE_SIZE: 'vocabulary_page_size',
  SORT_PREFERENCES: 'vocabulary_sort_prefs',
  LEARNING_PROGRESS: 'vocabulary_learning_progress'
} as const

// Error Messages
export const ERROR_MESSAGES = {
  LOAD_CATEGORIES: 'Không thể tải danh sách danh mục',
  LOAD_WORDS: 'Không thể tải danh sách từ vựng',
  LOAD_EXAMPLES: 'Không thể tải danh sách ví dụ',
  CREATE_WORD: 'Không thể tạo từ vựng mới',
  UPDATE_WORD: 'Không thể cập nhật từ vựng',
  DELETE_WORD: 'Không thể xóa từ vựng',
  UPLOAD_EXCEL: 'Không thể upload file Excel',
  INVALID_FILE: 'File không hợp lệ',
  NETWORK_ERROR: 'Lỗi kết nối mạng',
  PERMISSION_DENIED: 'Không có quyền thực hiện thao tác này'
} as const

// Success Messages
export const SUCCESS_MESSAGES = {
  WORD_CREATED: 'Tạo từ vựng thành công',
  WORD_UPDATED: 'Cập nhật từ vựng thành công',
  WORD_DELETED: 'Xóa từ vựng thành công',
  EXCEL_UPLOADED: 'Upload Excel thành công',
  TEMPLATE_DOWNLOADED: 'Tải template thành công'
} as const