import { ref, computed } from 'vue'
import { useVocabulary } from './useVocabulary'
import { useUserProgress } from './useUserProgress'
import type { VocabularyCategory, VocabularyWord, LearningSession } from '../types/vocabulary.types'

export const useLearningSession = (category: VocabularyCategory, sessionType: 'vocabulary' | 'dictation' | 'mixed') => {
  const {
    getWords,
    startLearningSession,
    completeLearningSession,
    clearError
  } = useVocabulary()

  const {
    updateWordProgress,
    updateExampleProgress,
    getUserProgress,
    progress: userProgressData,
    getLearningPosition,
    saveLearningPosition,
    clearLearningPosition
  } = useUserProgress()

  // Session state
  const sessionWords = ref<VocabularyWord[]>([])
  const currentIndex = ref(0)
  const currentExampleIndex = ref(0)
  const correctCount = ref(0)
  const totalAttempts = ref(0)
  const startTime = ref<number>(Date.now())
  const elapsedTime = ref(0)
  const isSessionComplete = ref(false)
  const currentSessionType = ref(sessionType)
  const currentMode = ref<'vocabulary' | 'dictation'>('vocabulary')
  const originalSessionType = ref(sessionType)
  const showExampleGrid = ref(false)
  const completedExamples = ref<number[]>([])
  const selectedGroupIndex = ref<number | null>(null)
  const submittingExampleIndex = ref<number | null>(null)

  // Pagination
  const currentPage = ref(1)
  const totalCount = ref(0)
  const hasMoreWords = ref(true)
  const isLoadingMore = ref(false)
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  // Timer
  let timer: number | null = null

  // Computed properties
  const currentWord = computed(() => sessionWords.value[currentIndex.value])
  
  const currentExample = computed(() => {
    const word = currentWord.value
    if (!word?.examples || word.examples.length === 0) return null
    return word.examples[currentExampleIndex.value] || null
  })

  const totalWords = computed(() => totalCount.value || sessionWords.value.length)

  const completedWordsCount = computed(() => {
    return sessionWords.value.filter(word => {
      if (!word.exampleCount || word.exampleCount === 0) return false
      return (word.completedExampleCount || 0) === word.exampleCount
    }).length
  })

  const totalExamples = computed(() => {
    return sessionWords.value.reduce((total, word) => {
      return total + (word.examples?.length || 0)
    }, 0)
  })

  const currentExampleNumber = computed(() => {
    let count = 0
    for (let i = 0; i < currentIndex.value; i++) {
      count += sessionWords.value[i]?.examples?.length || 0
    }
    count += currentExampleIndex.value + 1
    return count
  })

  const currentExampleInGroup = computed(() => {
    if (selectedGroupIndex.value === null) return 0
    const groupStart = selectedGroupIndex.value * 10
    return currentExampleIndex.value - groupStart + 1
  })

  const currentGroupSize = computed(() => {
    if (selectedGroupIndex.value === null || !currentWord.value) return 0
    const groupStart = selectedGroupIndex.value * 10
    const groupEnd = Math.min(groupStart + 10, currentWord.value.examples?.length || 0)
    return groupEnd - groupStart
  })

  const groupProgress = computed(() => {
    if (currentGroupSize.value === 0 || selectedGroupIndex.value === null) return 0
    
    const groupStart = selectedGroupIndex.value * 10
    const groupEnd = groupStart + currentGroupSize.value
    
    const completedInGroup = completedExamples.value.filter(
      index => index >= groupStart && index < groupEnd
    ).length
    
    return (completedInGroup / currentGroupSize.value) * 100
  })

  const progress = computed(() => {
    if (currentSessionType.value === 'dictation') {
      if (selectedGroupIndex.value !== null) {
        return groupProgress.value
      }
      if (totalExamples.value === 0) return 0
      return (currentExampleNumber.value / totalExamples.value) * 100
    } else {
      if (totalWords.value === 0) return 0
      return (completedWordsCount.value / totalWords.value) * 100
    }
  })

  const accuracy = computed(() => {
    if (totalAttempts.value === 0) return 0
    return (correctCount.value / totalAttempts.value) * 100
  })

  const finalScore = computed(() => {
    const baseScore = correctCount.value * 100
    const timeBonus = Math.max(0, 1000 - elapsedTime.value)
    const accuracyBonus = Math.round(accuracy.value * 10)
    return baseScore + timeBonus + accuracyBonus
  })

  // Timer methods
  const startTimer = () => {
    timer = window.setInterval(() => {
      elapsedTime.value = Math.floor((Date.now() - startTime.value) / 1000)
    }, 1000)
  }

  const stopTimer = () => {
    if (timer) {
      clearInterval(timer)
      timer = null
    }
  }

  const formatTime = (seconds: number): string => {
    const mins = Math.floor(seconds / 60)
    const secs = seconds % 60
    return `${mins}:${secs.toString().padStart(2, '0')}`
  }

  // Update completion counts
  const updateAllWordsCompletionCount = () => {
    if (!userProgressData.value) return
    
    const exampleProgressList = userProgressData.value.exampleProgress
    
    sessionWords.value.forEach(word => {
      if (word.examples && word.examples.length > 0) {
        const completedCount = word.examples.filter(example =>
          exampleProgressList.some(p => p.exampleId === example.id && p.isCompleted)
        ).length
        
        word.completedExampleCount = completedCount
        word.exampleCompletionPercentage = (completedCount / word.examples.length) * 100
      }
    })
  }

  // Load words
  const loadSessionWords = async () => {
    try {
      clearError()
      isLoading.value = true
      currentPage.value = 1
      
      const result = await getWords({
        categoryId: category.id,
        includeExamples: true,
        includeUserProgress: true,
        pageSize: 20,
        pageNumber: 1
      })
      
      console.log('API returned words:', result.items.length, 'Total count:', result.totalCount)
      sessionWords.value = result.items
      totalCount.value = result.totalCount
      hasMoreWords.value = sessionWords.value.length < result.totalCount
      
      if (sessionWords.value.length === 0) {
        throw new Error('No words found in this category')
      }

      await getUserProgress()
      updateAllWordsCompletionCount()
      
      if (userProgressData.value && currentWord.value) {
        const exampleProgressList = userProgressData.value.exampleProgress
        completedExamples.value = currentWord.value.examples
          .map((example, index) => {
            const isCompleted = exampleProgressList.some(
              p => p.exampleId === example.id && p.isCompleted
            )
            return isCompleted ? index : -1
          })
          .filter(index => index !== -1)
      }

      await startLearningSession({
        categoryId: category.id,
        sessionType: currentSessionType.value,
        maxWords: sessionWords.value.length
      })

      startTimer()
    } catch (err) {
      console.error('Failed to load session:', err)
      error.value = err instanceof Error ? err.message : 'Failed to load session'
    } finally {
      isLoading.value = false
    }
  }

  // Load more words
  const loadMoreWords = async () => {
    if (isLoadingMore.value || !hasMoreWords.value) return
    
    try {
      isLoadingMore.value = true
      currentPage.value++
      
      const result = await getWords({
        categoryId: category.id,
        includeExamples: true,
        includeUserProgress: true,
        pageSize: 20,
        pageNumber: currentPage.value
      })
      
      sessionWords.value = [...sessionWords.value, ...result.items]
      hasMoreWords.value = sessionWords.value.length < totalCount.value
      updateAllWordsCompletionCount()
    } catch (err) {
      console.error('Failed to load more words:', err)
    } finally {
      isLoadingMore.value = false
    }
  }

  // Complete session
  const finishSession = () => {
    stopTimer()
    isSessionComplete.value = true
  }

  const completeSession = async () => {
    try {
      const result = await completeLearningSession({
        sessionId: 1,
        correctAnswers: correctCount.value,
        score: finalScore.value
      })
      
      return result
    } catch (err) {
      console.error('Failed to complete session:', err)
      throw err
    }
  }

  return {
    // State
    sessionWords,
    currentIndex,
    currentExampleIndex,
    correctCount,
    totalAttempts,
    elapsedTime,
    isSessionComplete,
    currentSessionType,
    currentMode,
    originalSessionType,
    showExampleGrid,
    completedExamples,
    selectedGroupIndex,
    submittingExampleIndex,
    hasMoreWords,
    isLoadingMore,
    isLoading,
    error,
    userProgressData,
    
    // Computed
    currentWord,
    currentExample,
    totalWords,
    totalExamples,
    currentExampleNumber,
    currentExampleInGroup,
    currentGroupSize,
    progress,
    accuracy,
    finalScore,
    
    // Methods
    formatTime,
    loadSessionWords,
    loadMoreWords,
    updateAllWordsCompletionCount,
    finishSession,
    completeSession,
    startTimer,
    stopTimer,
    
    // Progress methods
    updateWordProgress,
    updateExampleProgress,
    getUserProgress,
    getLearningPosition,
    saveLearningPosition,
    clearLearningPosition
  }
}
