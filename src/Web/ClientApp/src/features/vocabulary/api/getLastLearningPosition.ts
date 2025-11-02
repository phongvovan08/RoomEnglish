import { apiService } from '@/services/api'

export interface LastLearningPosition {
  wordId: number
  wordText: string
  categoryId: number
  categoryName: string
  groupIndex: number
  lastExampleIndex: number
  lastAccessedAt: string
}

export const getLastLearningPosition = async (): Promise<LastLearningPosition | null> => {
  return apiService.get<LastLearningPosition | null>('/vocabulary-learning/learning-position/last')
}
