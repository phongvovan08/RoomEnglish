import { ref, type Ref } from 'vue'
import { useToast } from '@/composables/useToast'
import type { VocabularyWord } from '../types/vocabulary.types'

interface NavigationOptions {
  sessionWords: Ref<VocabularyWord[]>
  currentIndex: Ref<number>
  currentExampleIndex: Ref<number>
  currentSessionType: Ref<'vocabulary' | 'dictation' | 'mixed'>
  originalSessionType: Ref<'vocabulary' | 'dictation' | 'mixed'>
  selectedGroupIndex: Ref<number | null>
  showExampleGrid: Ref<boolean>
  completedExamples: Ref<number[]>
  currentMode: Ref<'vocabulary' | 'dictation'>
  onFinish: () => void
  clearLearningPosition: (wordId: number) => Promise<void>
}

export const useLearningNavigation = (options: NavigationOptions) => {
  const { showSuccess } = useToast()
  
  const {
    sessionWords,
    currentIndex,
    currentExampleIndex,
    currentSessionType,
    originalSessionType,
    selectedGroupIndex,
    showExampleGrid,
    completedExamples,
    currentMode,
    onFinish,
    clearLearningPosition
  } = options

  const nextWord = async () => {
    console.log('=== nextWord called ===')
    const word = sessionWords.value[currentIndex.value]
    
    // If in dictation mode from group selection
    if (currentSessionType.value === 'dictation' && originalSessionType.value === 'vocabulary' && selectedGroupIndex.value !== null) {
      const groupSize = 10
      const groupStartIndex = selectedGroupIndex.value * groupSize
      const groupEndIndex = Math.min(groupStartIndex + groupSize - 1, (word?.examples?.length || 0) - 1)
      
      if (currentExampleIndex.value < groupEndIndex) {
        currentExampleIndex.value++
        return
      } else {
        // Check if group is complete
        const groupCompleted = Array.from({ length: groupEndIndex - groupStartIndex + 1 }, (_, i) => groupStartIndex + i)
          .every(i => completedExamples.value.includes(i))
        
        if (groupCompleted && word) {
          await clearLearningPosition(word.id)
          console.log(`✅ Group ${selectedGroupIndex.value} completed 100% - cleared position`)
        }
        
        // Check if all examples completed
        const allExamplesCompleted = word?.examples && completedExamples.value.length === word.examples.length
        
        if (allExamplesCompleted) {
          showSuccess(`Completed all examples for "${word?.word}"!`)
          currentSessionType.value = 'vocabulary'
          showExampleGrid.value = false
          selectedGroupIndex.value = null
          currentExampleIndex.value = 0
          
          if (currentIndex.value < sessionWords.value.length - 1) {
            currentIndex.value++
          } else {
            onFinish()
          }
        } else {
          currentSessionType.value = 'vocabulary'
          showExampleGrid.value = true
          selectedGroupIndex.value = null
          currentExampleIndex.value = 0
        }
        return
      }
    }
    
    // If in dictation from single example
    if (currentSessionType.value === 'dictation' && originalSessionType.value === 'vocabulary') {
      currentSessionType.value = 'vocabulary'
      currentExampleIndex.value = 0
      return
    }
    
    // Pure dictation mode
    if (currentSessionType.value === 'dictation' && word?.examples && currentExampleIndex.value < word.examples.length - 1) {
      currentExampleIndex.value++
    } else {
      currentExampleIndex.value = 0
      
      if (currentIndex.value < sessionWords.value.length - 1) {
        currentIndex.value++
        
        if (currentSessionType.value === 'mixed') {
          currentMode.value = Math.random() > 0.5 ? 'vocabulary' : 'dictation'
        }
      } else {
        onFinish()
      }
    }
  }

  const jumpToWord = (wordIndex: number) => {
    if (wordIndex >= 0 && wordIndex < sessionWords.value.length) {
      currentIndex.value = wordIndex
      console.log(`Jumped to word ${wordIndex}`)
    }
  }

  const jumpToExample = (localIndex: number) => {
    if (selectedGroupIndex.value === null) return
    const globalIndex = selectedGroupIndex.value * 10 + localIndex
    currentExampleIndex.value = globalIndex
    console.log(`Jumped to example ${globalIndex}`)
  }

  const backToVocabulary = () => {
    showExampleGrid.value = false
    currentSessionType.value = originalSessionType.value
    currentExampleIndex.value = 0
    selectedGroupIndex.value = null
  }

  const backToExampleGrid = async (
    getUserProgress: () => Promise<void>,
    updateAllWordsCompletionCount: () => void,
    userProgressData: any
  ) => {
    await getUserProgress()
    updateAllWordsCompletionCount()
    
    const word = sessionWords.value[currentIndex.value]
    if (userProgressData.value && word) {
      const exampleProgressList = userProgressData.value.exampleProgress
      completedExamples.value = word.examples
        .map((example: any, index: number) => {
          const isCompleted = exampleProgressList.some(
            (p: any) => p.exampleId === example.id && p.isCompleted
          )
          return isCompleted ? index : -1
        })
        .filter((index: number) => index !== -1)
      
      if (selectedGroupIndex.value !== null && word) {
        const groupStart = selectedGroupIndex.value * 10
        const groupEnd = Math.min(groupStart + 10, word.examples.length)
        const groupCompleted = Array.from({ length: groupEnd - groupStart }, (_, i) => groupStart + i)
          .every(i => completedExamples.value.includes(i))
        
        if (groupCompleted) {
          await clearLearningPosition(word.id)
        }
      }
    }
    
    showExampleGrid.value = false
    currentSessionType.value = 'vocabulary'
    selectedGroupIndex.value = null
    currentExampleIndex.value = 0
  }

  return {
    nextWord,
    jumpToWord,
    jumpToExample,
    backToVocabulary,
    backToExampleGrid
  }
}
