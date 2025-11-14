import { computed, type Ref, type ComputedRef } from 'vue'
import type { VocabularyWord } from '../../types/vocabulary.types'

export const GROUP_SIZE = 100

interface SessionStateRefs {
  sessionWords: Ref<VocabularyWord[]>
  currentIndex: Ref<number>
  currentExampleIndex: Ref<number>
  correctCount: Ref<number>
  totalAttempts: Ref<number>
  totalCount: Ref<number>
  completedExamples: Ref<number[]>
  selectedGroupIndex: Ref<number | null>
  currentSessionType: Ref<'vocabulary' | 'dictation' | 'mixed'>
  elapsedTime?: Ref<number>
}

interface SessionComputedReturn {
  currentWord: ComputedRef<VocabularyWord | undefined>
  currentExample: ComputedRef<any>
  totalWords: ComputedRef<number>
  completedWordsCount: ComputedRef<number>
  totalExamples: ComputedRef<number>
  currentExampleNumber: ComputedRef<number>
  currentExampleInGroup: ComputedRef<number>
  currentGroupSize: ComputedRef<number>
  groupProgress: ComputedRef<number>
  progress: ComputedRef<number>
  accuracy: ComputedRef<number>
  finalScore: ComputedRef<number>
}

export function useSessionComputed(state: SessionStateRefs): SessionComputedReturn {
  const currentWord = computed(() => state.sessionWords.value[state.currentIndex.value])
  
  const currentExample = computed(() => {
    const word = currentWord.value
    if (!word?.examples || word.examples.length === 0) return null
    return word.examples[state.currentExampleIndex.value] || null
  })

  const totalWords = computed(() => state.totalCount.value || state.sessionWords.value.length)

  const completedWordsCount = computed(() => {
    return state.sessionWords.value.filter(word => {
      if (!word.exampleCount || word.exampleCount === 0) return false
      return (word.completedExampleCount || 0) === word.exampleCount
    }).length
  })

  const totalExamples = computed(() => {
    return state.sessionWords.value.reduce((total, word) => {
      return total + (word.examples?.length || 0)
    }, 0)
  })

  const currentExampleNumber = computed(() => {
    let count = 0
    for (let i = 0; i < state.currentIndex.value; i++) {
      count += state.sessionWords.value[i]?.examples?.length || 0
    }
    count += state.currentExampleIndex.value + 1
    return count
  })

  const currentExampleInGroup = computed(() => {
    if (state.selectedGroupIndex.value === null) return 0
    const groupStart = state.selectedGroupIndex.value * GROUP_SIZE
    return state.currentExampleIndex.value - groupStart + 1
  })

  const currentGroupSize = computed(() => {
    if (state.selectedGroupIndex.value === null || !currentWord.value) return 0
    const groupStart = state.selectedGroupIndex.value * GROUP_SIZE
    const groupEnd = Math.min(groupStart + GROUP_SIZE, currentWord.value.examples?.length || 0)
    return groupEnd - groupStart
  })

  const groupProgress = computed(() => {
    if (currentGroupSize.value === 0 || state.selectedGroupIndex.value === null) return 0
    
    const groupStart = state.selectedGroupIndex.value * GROUP_SIZE
    const groupEnd = groupStart + currentGroupSize.value
    
    const completedInGroup = state.completedExamples.value.filter(
      index => index >= groupStart && index < groupEnd
    ).length
    
    return (completedInGroup / currentGroupSize.value) * 100
  })

  const progress = computed(() => {
    if (state.currentSessionType.value === 'dictation') {
      const currentWordExampleCount = currentWord.value?.examples?.length || 0
      if (currentWordExampleCount === 0) return 0
      
      const completedInCurrentWord = state.completedExamples.value.length
      
      return (completedInCurrentWord / currentWordExampleCount) * 100
    } else {
      if (totalWords.value === 0) return 0
      return (completedWordsCount.value / totalWords.value) * 100
    }
  })

  const accuracy = computed(() => {
    if (state.totalAttempts.value === 0) return 0
    return (state.correctCount.value / state.totalAttempts.value) * 100
  })

  const finalScore = computed(() => {
    const baseScore = state.correctCount.value * 100
    const elapsed = state.elapsedTime?.value || 0
    const timeBonus = Math.max(0, 1000 - elapsed)
    const accuracyBonus = Math.round(accuracy.value * 10)
    return baseScore + timeBonus + accuracyBonus
  })

  return {
    currentWord,
    currentExample,
    totalWords,
    completedWordsCount,
    totalExamples,
    currentExampleNumber,
    currentExampleInGroup,
    currentGroupSize,
    groupProgress,
    progress,
    accuracy,
    finalScore
  }
}
