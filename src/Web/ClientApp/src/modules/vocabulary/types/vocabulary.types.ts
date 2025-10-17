// Vocabulary Types
export interface VocabularyCategory {
  id: number
  name: string
  description: string
  color: string
  iconName: string
  isActive: boolean
  displayOrder: number
  wordCount: number
  created: string
}

export interface VocabularyWord {
  id: number
  word: string
  phonetic: string
  partOfSpeech: string
  meaning: string
  definition: string
  audioUrl?: string
  difficultyLevel: number
  isActive: boolean
  viewCount: number
  correctCount: number
  incorrectCount: number
  categoryId: number
  categoryName: string
  examples: VocabularyExample[]
  userProgress?: UserWordProgress
}

export interface VocabularyExample {
  id: number
  sentence: string
  translation: string
  grammar?: string
  audioUrl?: string
  difficultyLevel: number
  isActive: boolean
  displayOrder: number
  wordId: number
}

export interface UserWordProgress {
  id: number
  userId: string
  wordId: number
  studiedTimes: number
  correctAnswers: number
  totalAttempts: number
  firstStudiedAt?: string
  lastStudiedAt?: string
  isMastered: boolean
  masteryLevel: number
  accuracyRate: number
}

export interface DictationResult {
  id: number
  userId: string
  exampleId: number
  userInput: string
  correctAnswer: string
  isCorrect: boolean
  accuracyPercentage: number
  timeTakenSeconds: number
  completedAt: string
}

export interface LearningSession {
  id: number
  userId: string
  categoryId: number
  categoryName: string
  startedAt: string
  completedAt?: string
  totalWords: number
  correctAnswers: number
  durationMinutes: number
  sessionType: string
  score: number
  isCompleted: boolean
  accuracyRate: number
}

// API Request/Response Types
export interface GetVocabularyCategoriesQuery {
  pageNumber?: number
  pageSize?: number
  includeInactive?: boolean
}

export interface GetVocabularyWordsQuery {
  categoryId?: number
  difficultyLevel?: number
  searchTerm?: string
  pageNumber?: number
  pageSize?: number
  includeInactive?: boolean
  includeExamples?: boolean
  includeUserProgress?: boolean
  userId?: string
}

export interface StartLearningSessionCommand {
  categoryId: number
  sessionType: 'vocabulary' | 'dictation' | 'mixed'
  maxWords?: number
}

export interface CompleteLearningSessionCommand {
  sessionId: number
  correctAnswers: number
  score: number
}

export interface SubmitDictationCommand {
  exampleId: number
  userInput: string
  timeTakenSeconds: number
}

export interface PaginatedList<T> {
  items: T[]
  pageNumber: number
  totalPages: number
  totalCount: number
  hasPreviousPage: boolean
  hasNextPage: boolean
}

// Audio & Speech Types
export interface AudioPlayer {
  play: (url: string) => Promise<void>
  stop: () => void
  isPlaying: boolean
}

export interface SpeechRecognition {
  start: () => void
  stop: () => void
  isListening: boolean
  result: string
  confidence: number
}