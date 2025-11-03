<template>
  <div class="learning-session-container">
    <!-- Restoring Position Overlay -->
    <div v-if="isRestoringPosition" class="restoring-overlay">
      <div class="restoring-content">
        <div class="cyber-spinner-large"></div>
        <h3>Đang khôi phục tiến trình...</h3>
        <p class="restore-info" v-if="restoreInfo">{{ restoreInfo }}</p>
      </div>
    </div>

    <!-- Main Content - Hidden during restore -->
    <template v-if="!isRestoringPosition">
      <!-- Session Header -->
      <div class="session-header">
      <button @click="$emit('back')" class="back-btn">
        <i class="mdi mdi-arrow-left"></i>
        Back to Categories
      </button>
      
      <div class="session-info">
        <h2>{{ category.name }}</h2>
        <div class="session-stats">
          <div class="stat-item">
            <i class="mdi mdi-target"></i>
            <span v-if="currentSessionType === 'dictation' && selectedGroupIndex !== null">
              Example {{ currentExampleInGroup }} / {{ currentGroupSize }} (Group {{ selectedGroupIndex + 1 }})
            </span>
            <span v-else-if="currentSessionType === 'dictation'">
              Example {{ currentExampleNumber }} / {{ totalExamples }}
            </span>
            <span v-else>
              Word {{ currentIndex + 1 }} / {{ totalWords }}
            </span>
          </div>
          <div class="stat-item">
            <i class="mdi mdi-check-circle"></i>
            <span>{{ correctCount }} correct</span>
          </div>
          <div class="stat-item">
            <i class="mdi mdi-timer"></i>
            <span>{{ formatTime(elapsedTime) }}</span>
          </div>
        </div>
      </div>

      <div class="session-controls">
        <select :value="sessionType" class="session-type-select" @change="changeSessionType">
          <option value="vocabulary">📚 Vocabulary</option>
          <option value="dictation">🎤 Dictation</option>
          <option value="mixed">🎯 Mixed</option>
        </select>
        <button 
          @click="showSpeechSettings = true"
          class="speech-settings-btn"
          title="Speech Settings"
        >
          <Icon icon="mdi:cog" class="w-4 h-4" />
          Settings
        </button>
      </div>
    </div>

    <!-- Progress Bar -->
    <div class="progress-container">
      <div class="progress-bar">
        <div 
          class="progress-fill"
          :style="{ width: `${progress}%` }"
        ></div>
      </div>
      <span class="progress-text">{{ Math.round(progress) }}%</span>
    </div>

    <!-- Learning Content -->
    <div class="learning-content" v-if="currentWord">
      <!-- Example Grid -->
      <ExampleGrid
        v-if="showExampleGrid"
        :word="currentWord"
        :examples="currentWord.examples || []"
        :completed-examples="completedExamples"
        @select-group="selectExampleGroup"
        @back="backToVocabulary"
      />

      <!-- Vocabulary Mode -->
      <div v-else-if="currentSessionType === 'vocabulary'" class="vocabulary-container">
        <WordSidebar 
          :words="sessionWords"
          :current-word-index="currentIndex"
          :has-more="hasMoreWords"
          :is-loading-more="isLoadingMore"
          :is-initial-loading="isLoading"
          :total-words="totalWords"
          @select-word="jumpToWord"
          @load-more="loadMoreWords"
        />
        
        <VocabularyCard 
          :word="currentWord || {}"
          :is-loading="isLoading"
          @next="nextWord"
          @learn-example="switchToDictation"
        />
      </div>

      <!-- Dictation Mode -->
      <div v-else-if="currentSessionType === 'dictation'" class="dictation-container">
        <ExampleSidebar 
          v-if="selectedGroupIndex !== null && currentWord && currentWord.examples"
          :examples="getCurrentGroupExamples()"
          :group-index="selectedGroupIndex"
          :current-index="currentExampleIndex - (selectedGroupIndex * 10)"
          :completed-examples="completedExamples"
          :group-start-index="selectedGroupIndex * 10"
          :word="currentWord"
          :submitting-example-index="submittingExampleIndex ?? undefined"
          @select-example="jumpToExample"
        />
        
        <DictationCard 
          :example="currentExample"
          :word="currentWord"
          :show-back-to-grid="selectedGroupIndex !== null"
          @submit="handleDictationSubmit"
          @next="nextWord"
          @back-to-grid="backToExampleGrid"
        />
      </div>

      <!-- Mixed Mode -->
      <component 
        v-else-if="currentSessionType === 'mixed'"
        :is="currentMode === 'vocabulary' ? 'VocabularyCard' : 'DictationCard'"
        :word="currentWord"
        :example="currentMode === 'dictation' ? currentExample : undefined"
        @submit="handleDictationSubmit"
        @next="nextWord"
      />
    </div>

    <!-- Error State -->
    <div v-if="error" class="error-state">
      <div class="error-icon">⚠️</div>
      <div class="error-message">{{ error }}</div>
      <button @click="retry" class="retry-btn">Try Again</button>
    </div>

    <!-- Session Complete Modal -->
    <div v-if="isSessionComplete" class="modal-overlay" @click="completeSession">
      <div class="session-complete-modal" @click.stop>
        <div class="modal-header">
          <h2>🎉 Session Complete!</h2>
        </div>
        <div class="modal-content">
          <div class="final-stats">
            <div class="stat-card">
              <div class="stat-value">{{ correctCount }}</div>
              <div class="stat-label">Correct Answers</div>
            </div>
            <div class="stat-card">
              <div class="stat-value">{{ Math.round(accuracy) }}%</div>
              <div class="stat-label">Accuracy</div>
            </div>
            <div class="stat-card">
              <div class="stat-value">{{ formatTime(elapsedTime) }}</div>
              <div class="stat-label">Time Spent</div>
            </div>
            <div class="stat-card">
              <div class="stat-value">{{ finalScore }}</div>
              <div class="stat-label">Final Score</div>
            </div>
          </div>
        </div>
        <div class="modal-actions">
          <button @click="completeSession" class="complete-btn">
            View Results
          </button>
        </div>
      </div>
    </div>

    <!-- Speech Settings Panel with Overlay -->
    <div v-if="showSpeechSettings" class="settings-overlay" @click="showSpeechSettings = false">
      <div @click.stop>
        <SpeechSettingsPanel 
          :show-panel="showSpeechSettings"
          @close="showSpeechSettings = false"
        />
      </div>
    </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRoute } from 'vue-router'
import { useVocabulary } from '../composables/useVocabulary'
import { useUserProgress } from '../composables/useUserProgress'
import { useToast } from '@/composables/useToast'
import type { VocabularyCategory, VocabularyWord, VocabularyExample, LearningSession } from '../types/vocabulary.types'
import VocabularyCard from '../components/VocabularyCard.vue'
import DictationCard from '../components/DictationCard.vue'
import ExampleGrid from '../components/ExampleGrid.vue'
import ExampleSidebar from '../components/ExampleSidebar.vue'
import WordSidebar from '../components/WordSidebar.vue'
import SpeechSettingsPanel from '../../../components/SpeechSettingsPanel.vue'
import { Icon } from '@iconify/vue'

interface Props {
  category: VocabularyCategory
  sessionType: 'vocabulary' | 'dictation' | 'mixed'
}

const props = defineProps<Props>()

const emit = defineEmits<{
  complete: [result: LearningSession]
  back: []
  'update:sessionType': [value: 'vocabulary' | 'dictation' | 'mixed']
}>()

const {
  words,
  isLoading,
  error,
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

const { showSuccess } = useToast()
const route = useRoute()

// Session state
const sessionWords = ref<VocabularyWord[]>([])
const currentIndex = ref(0)
const currentExampleIndex = ref(0) // Track current example index
const correctCount = ref(0)
const totalAttempts = ref(0)
const startTime = ref<number>(Date.now())
const elapsedTime = ref(0)
const isSessionComplete = ref(false)
const currentSessionType = ref(props.sessionType)
const currentMode = ref<'vocabulary' | 'dictation'>('vocabulary')
const showSpeechSettings = ref(false)
const originalSessionType = ref(props.sessionType) // Remember original type
const showExampleGrid = ref(false) // Show example grid
const completedExamples = ref<number[]>([]) // Track completed example indices
const selectedGroupIndex = ref<number | null>(null) // Track selected group
const submittingExampleIndex = ref<number | null>(null) // Track example being submitted
const isRestoringPosition = ref(false) // Loading overlay when restoring from URL
const restoreInfo = ref<string>('') // Info text during restore

// Pagination state
const currentPage = ref(1)
const totalCount = ref(0)
const hasMoreWords = ref(true)
const isLoadingMore = ref(false)

// Timer
let timer: number | null = null

// Computed properties
const currentWord = computed(() => sessionWords.value[currentIndex.value])
const currentExample = computed(() => {
  const word = currentWord.value
  if (!word?.examples || word.examples.length === 0) return null
  return word.examples[currentExampleIndex.value] || null
})

// Use totalCount from API instead of loaded words length
const totalWords = computed(() => totalCount.value || sessionWords.value.length)

// Count completed words (all examples done)
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

// Group-specific progress tracking
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
  
  // Calculate based on completed examples in this group
  const groupStart = selectedGroupIndex.value * 10
  const groupEnd = groupStart + currentGroupSize.value
  
  const completedInGroup = completedExamples.value.filter(
    index => index >= groupStart && index < groupEnd
  ).length
  
  return (completedInGroup / currentGroupSize.value) * 100
})

const progress = computed(() => {
  if (currentSessionType.value === 'dictation') {
    // If in group mode, show group progress
    if (selectedGroupIndex.value !== null) {
      return groupProgress.value
    }
    // For dictation mode, calculate based on examples
    if (totalExamples.value === 0) return 0
    return (currentExampleNumber.value / totalExamples.value) * 100
  } else {
    // For vocabulary mode, calculate based on completed words
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

// Methods
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

// Update completedExampleCount for all words based on current progress
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

const loadSessionWords = async () => {
  try {
    clearError()
    currentPage.value = 1
    const result = await getWords({
      categoryId: props.category.id,
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

    // Load user progress to get completed examples
    await getUserProgress()
    
    // Update all words' completion counts
    updateAllWordsCompletionCount()
    
    // Build completedExamples array from progress data
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

    // Start the learning session
    await startLearningSession({
      categoryId: props.category.id,
      sessionType: currentSessionType.value,
      maxWords: sessionWords.value.length
    })

    startTimer()
  } catch (err) {
    console.error('Failed to load session:', err)
  }
}

// Load more words when scrolling
const loadMoreWords = async () => {
  if (isLoadingMore.value || !hasMoreWords.value) return
  
  try {
    isLoadingMore.value = true
    currentPage.value++
    
    const result = await getWords({
      categoryId: props.category.id,
      includeExamples: true,
      includeUserProgress: true,
      pageSize: 20,
      pageNumber: currentPage.value
    })
    
    console.log('Loaded more words:', result.items.length, 'Page:', currentPage.value)
    
    // Append new words
    sessionWords.value = [...sessionWords.value, ...result.items]
    hasMoreWords.value = sessionWords.value.length < totalCount.value
    
    // Update progress for new words
    updateAllWordsCompletionCount()
  } catch (err) {
    console.error('Failed to load more words:', err)
  } finally {
    isLoadingMore.value = false
  }
}

const handleDictationSubmit = async (result: any) => {
  totalAttempts.value++
  if (result.isCorrect) {
    correctCount.value++
  }
  
  // Check if passing score first
  const isPassingScore = result.accuracyPercentage >= 75
  
  // Show loading state in sidebar ONLY if passing score
  if (isPassingScore && selectedGroupIndex.value !== null) {
    submittingExampleIndex.value = currentExampleIndex.value - (selectedGroupIndex.value * 10)
  }
  
  // Optimistic update: Add to completedExamples immediately ONLY if accuracy >= 75%
  if (isPassingScore && currentExample.value && !completedExamples.value.includes(currentExampleIndex.value)) {
    completedExamples.value = [...completedExamples.value, currentExampleIndex.value]
    
    // Update completedExampleCount for the current word immediately
    if (currentWord.value) {
      currentWord.value.completedExampleCount = completedExamples.value.length
    }
    
    // Update all words' completedExampleCount immediately
    updateAllWordsCompletionCount()
  }
  
  try {
    // Update example progress in database
    if (currentExample.value) {
      await updateExampleProgress(currentExample.value.id, result.accuracyPercentage)
    }
    
    // Update word progress in database
    if (currentWord.value) {
      await updateWordProgress(currentWord.value.id, result.isCorrect)
    }
    
    // Reload user progress to get updated data (in background)
    await getUserProgress()
    
    // Rebuild completedExamples array from updated progress data to ensure sync
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
      
      console.log(`✅ Updated completedExamples: ${completedExamples.value.length}/${currentWord.value.examples.length}`)
      
      // Update completedExampleCount for the current word
      currentWord.value.completedExampleCount = completedExamples.value.length
    }
    
    // Update all words' completedExampleCount again to ensure accuracy
    updateAllWordsCompletionCount()

    // Save next position to database (so when user returns, they continue from next example)
    if (currentWord.value && selectedGroupIndex.value !== null) {
      const groupStartIndex = selectedGroupIndex.value * 10
      const groupEndIndex = Math.min(groupStartIndex + 10, currentWord.value.examples.length)
      const nextExampleIndex = currentExampleIndex.value + 1

      // Only save if there's a next example in the current group
      if (nextExampleIndex < groupEndIndex) {
        await saveLearningPosition(
          currentWord.value.id,
          selectedGroupIndex.value,
          nextExampleIndex
        )
        console.log(`Saved next position: word ${currentWord.value.id}, group ${selectedGroupIndex.value}, example ${nextExampleIndex}`)
      } else {
        // Group is about to be completed, clear the position
        await clearLearningPosition(currentWord.value.id)
        console.log(`Last example in group completed, cleared position for word ${currentWord.value.id}`)
      }
    }
  } finally {
    // Clear loading state
    submittingExampleIndex.value = null
  }
  
  // Don't auto advance - user will click Next button in ResultDisplay
}

const nextWord = async () => {
  console.log('=== nextWord called ===')
  console.log('Current index:', currentIndex.value)
  console.log('Total words:', sessionWords.value.length)
  console.log('Session type:', currentSessionType.value)
  
  const word = currentWord.value
  console.log('Current word:', word?.word)
  
  // If we were in dictation mode from a group selection
  if (currentSessionType.value === 'dictation' && originalSessionType.value === 'vocabulary' && selectedGroupIndex.value !== null) {
    const groupSize = 10
    const groupStartIndex = selectedGroupIndex.value * groupSize
    const groupEndIndex = Math.min(groupStartIndex + groupSize - 1, (word?.examples?.length || 0) - 1)
    
    // Check if we're still within the selected group
    if (currentExampleIndex.value < groupEndIndex) {
      console.log('Moving to next example in group')
      currentExampleIndex.value++
      return
    } else {
      console.log('Finished group, returning to example grid')
      
      // Check if the group is now 100% complete
      const groupCompleted = Array.from({ length: groupEndIndex - groupStartIndex + 1 }, (_, i) => groupStartIndex + i)
        .every(i => completedExamples.value.includes(i))
      
      // Clear saved position if group is fully completed
      if (groupCompleted && word) {
        await clearLearningPosition(word.id)
        console.log(`✅ Group ${selectedGroupIndex.value} completed 100% - cleared database position`)
      }
      
      // Check if ALL examples of the current word are completed
      const allExamplesCompleted = word?.examples && 
        completedExamples.value.length === word.examples.length
      
      if (allExamplesCompleted) {
        // All examples completed - move to next word
        console.log('✅ All examples completed! Moving to next word...')
        
        // Show toast notification
        showSuccess(`Completed all examples for "${word?.word}"!`)
        
        currentSessionType.value = 'vocabulary'
        showExampleGrid.value = false
        selectedGroupIndex.value = null
        currentExampleIndex.value = 0
        
        // Move to next word
        if (currentIndex.value < sessionWords.value.length - 1) {
          currentIndex.value++
          console.log(`Moved to next word: ${sessionWords.value[currentIndex.value]?.word}`)
        } else {
          console.log('No more words - finishing session')
          finishSession()
        }
      } else {
        // Return to example grid to show updated progress
        currentSessionType.value = 'vocabulary'
        showExampleGrid.value = true
        selectedGroupIndex.value = null
        currentExampleIndex.value = 0
      }
      return
    }
  }
  
  // If we were in dictation mode for a single example (from grid), go back to vocabulary
  if (currentSessionType.value === 'dictation' && originalSessionType.value === 'vocabulary') {
    console.log('Finished example, returning to vocabulary mode')
    currentSessionType.value = 'vocabulary'
    currentExampleIndex.value = 0
    return
  }
  
  // Only cycle through examples in pure dictation mode
  if (currentSessionType.value === 'dictation' && word?.examples && currentExampleIndex.value < word.examples.length - 1) {
    console.log('Moving to next example of current word (dictation mode)')
    currentExampleIndex.value++
  } else {
    // Move to next word
    console.log('Moving to next word...')
    currentExampleIndex.value = 0 // Reset example index
    
    if (currentIndex.value < sessionWords.value.length - 1) {
      console.log('Incrementing currentIndex from', currentIndex.value, 'to', currentIndex.value + 1)
      currentIndex.value++
      
      // Switch mode for mixed sessions
      if (currentSessionType.value === 'mixed') {
        currentMode.value = Math.random() > 0.5 ? 'vocabulary' : 'dictation'
      }
    } else {
      console.log('No more words - finishing session')
      finishSession()
    }
  }
  
  console.log('After nextWord - currentIndex:', currentIndex.value)
}

const switchToDictation = async () => {
  console.log('=== Switching to dictation mode ===')
  const word = currentWord.value
  
  if (!word?.examples || word.examples.length === 0) {
    console.log('No examples available for this word')
    return
  }
  
  console.log('Current word:', word.word)
  console.log('Examples count:', word.examples.length)
  
  // Load progress to get completed examples
  await getUserProgress()
  
  // Update all words' completion counts
  updateAllWordsCompletionCount()
  
  // Update completedExamples array for current word
  if (userProgressData.value) {
    const exampleProgressList = userProgressData.value.exampleProgress
    completedExamples.value = word.examples
      .map((example, index) => {
        const isCompleted = exampleProgressList.some(
          p => p.exampleId === example.id && p.isCompleted
        )
        return isCompleted ? index : -1
      })
      .filter(index => index !== -1)
  }
  
  // Automatically select first group (group 0) and start learning
  console.log('Auto-selecting first group (group 0) and starting dictation')
  await selectExampleGroup(0)
}

const selectExampleGroup = async (groupIndex: number) => {
  console.log('=== Example group selected:', groupIndex, '===')
  
  // Hide grid and show dictation card
  showExampleGrid.value = false
  selectedGroupIndex.value = groupIndex
  
  // Calculate start and end index of the group (each group has 10 examples)
  const startIndex = groupIndex * 10
  const endIndex = Math.min(startIndex + 10, currentWord.value?.examples?.length || 0)
  
  let targetIndex = startIndex
  
  // Check if there's a saved position in database
  if (currentWord.value) {
    const savedPosition = await getLearningPosition(currentWord.value.id)
    
    if (savedPosition && savedPosition.groupIndex === groupIndex) {
      // Verify saved position is still valid and incomplete
      if (savedPosition.lastExampleIndex >= startIndex && 
          savedPosition.lastExampleIndex < endIndex &&
          !completedExamples.value.includes(savedPosition.lastExampleIndex)) {
        targetIndex = savedPosition.lastExampleIndex
        console.log(`Resuming from database position: example ${targetIndex}`)
      } else {
        // Saved position is complete or invalid, find first incomplete
        for (let i = startIndex; i < endIndex; i++) {
          if (!completedExamples.value.includes(i)) {
            targetIndex = i
            break
          }
        }
        console.log(`Saved position complete, starting at ${targetIndex} (first incomplete)`)
      }
    } else {
      // No saved position, find first incomplete example in this group
      for (let i = startIndex; i < endIndex; i++) {
        if (!completedExamples.value.includes(i)) {
          targetIndex = i
          break
        }
      }
      console.log(`Starting at example index ${targetIndex} (first incomplete in group)`)
    }
  }
  
  currentExampleIndex.value = targetIndex
  currentSessionType.value = 'dictation'
  
  // Emit session type change to parent
  emit('update:sessionType', 'dictation')
  
  console.log('Switched to dictation mode for group', groupIndex)
  console.log('Starting at example index', targetIndex, '(first incomplete in group)')
}

const backToVocabulary = () => {
  console.log('=== Going back to vocabulary ===')
  showExampleGrid.value = false
  currentSessionType.value = originalSessionType.value
  currentExampleIndex.value = 0
  selectedGroupIndex.value = null
}

// Get examples for current group
const getCurrentGroupExamples = () => {
  if (!currentWord.value || selectedGroupIndex.value === null) return []
  const groupSize = 10
  const startIndex = selectedGroupIndex.value * groupSize
  const endIndex = Math.min(startIndex + groupSize, currentWord.value.examples?.length || 0)
  return currentWord.value.examples?.slice(startIndex, endIndex) || []
}

// Jump to a specific example in the group
const jumpToExample = (localIndex: number) => {
  if (selectedGroupIndex.value === null) return
  const globalIndex = selectedGroupIndex.value * 10 + localIndex
  currentExampleIndex.value = globalIndex
  console.log(`Jumped to example ${globalIndex} (local index ${localIndex} in group ${selectedGroupIndex.value})`)
}

// Jump to a specific word
const jumpToWord = (wordIndex: number) => {
  if (wordIndex >= 0 && wordIndex < sessionWords.value.length) {
    currentIndex.value = wordIndex
    console.log(`Jumped to word ${wordIndex}: ${sessionWords.value[wordIndex].word}`)
  }
}

const backToExampleGrid = async () => {
  console.log('=== Going back to word list ===')

  // Reload progress to get updated completed examples
  await getUserProgress()

  // Update all words' completion counts
  updateAllWordsCompletionCount()

  // Update completedExamples array
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

    // Clear saved progress for current group if all examples completed
    if (selectedGroupIndex.value !== null && currentWord.value) {
      const groupStart = selectedGroupIndex.value * 10
      const groupEnd = Math.min(groupStart + 10, currentWord.value.examples.length)
      const groupCompleted = Array.from({ length: groupEnd - groupStart }, (_, i) => groupStart + i)
        .every(i => completedExamples.value.includes(i))

      if (groupCompleted) {
        await clearLearningPosition(currentWord.value.id)
        console.log(`Cleared database position for completed group ${selectedGroupIndex.value}`)
      }
    }
  }

  // Return to vocabulary card (word list)
  showExampleGrid.value = false
  currentSessionType.value = 'vocabulary'
  selectedGroupIndex.value = null
  currentExampleIndex.value = 0

  // Emit session type change to parent so it can update URL
  emit('update:sessionType', 'vocabulary')
  console.log('=== Emitted update:sessionType to vocabulary ===')
}

const finishSession = () => {
  stopTimer()
  isSessionComplete.value = true
}

const completeSession = async () => {
  try {
    const result = await completeLearningSession({
      sessionId: 1, // This should come from the started session
      correctAnswers: correctCount.value,
      score: finalScore.value
    })
    
    emit('complete', result)
  } catch (err) {
    console.error('Failed to complete session:', err)
  }
}

const changeSessionType = (event: Event) => {
  const target = event.target as HTMLSelectElement
  const newType = target.value as 'vocabulary' | 'dictation' | 'mixed'
  
  emit('update:sessionType', newType)
  
  // Update local state
  currentSessionType.value = newType
  // Reset current mode for mixed sessions
  if (newType === 'mixed') {
    currentMode.value = 'vocabulary'
  }
}

const retry = () => {
  clearError()
  loadSessionWords()
}

onMounted(async () => {
  // Check if we need to restore position from URL
  const { wordId, groupIndex, exampleIndex } = route.query
  const shouldRestore = wordId && groupIndex !== undefined && exampleIndex !== undefined
  
  if (shouldRestore) {
    isRestoringPosition.value = true
    restoreInfo.value = 'Đang tải dữ liệu...'
  }
  
  await loadSessionWords()
  
  // Restore learning position from query params (from "Continue Learning" button)
  if (shouldRestore) {
    const targetWordId = Number(wordId)
    const targetGroupIndex = Number(groupIndex)
    const targetExampleIndex = Number(exampleIndex)
    
    // Find the word in loaded session words
    const wordIndex = sessionWords.value.findIndex(w => w.id === targetWordId)
    
    if (wordIndex !== -1) {
      console.log(`🎯 Restoring position: word ${targetWordId}, group ${targetGroupIndex}, example ${targetExampleIndex}`)
      
      restoreInfo.value = 'Đang khôi phục vị trí học...'
      
      // Jump to the word
      currentIndex.value = wordIndex
      
      // Wait for word to load
      await new Promise(resolve => setTimeout(resolve, 50))
      
      restoreInfo.value = 'Đang tải tiến trình...'
      
      // Rebuild completedExamples for the target word
      const targetWord = sessionWords.value[wordIndex]
      if (userProgressData.value && targetWord?.examples) {
        const exampleProgressList = userProgressData.value.exampleProgress
        completedExamples.value = targetWord.examples
          .map((example, index) => {
            const isCompleted = exampleProgressList.some(
              p => p.exampleId === example.id && p.isCompleted
            )
            return isCompleted ? index : -1
          })
          .filter(index => index !== -1)
        
        console.log(`📊 Loaded ${completedExamples.value.length} completed examples for word ${targetWord.word}`)
      }
      
      restoreInfo.value = 'Đang chuyển đến ví dụ...'

      // Select the group - it will automatically restore to the saved position from database
      // or find the first incomplete example if the saved position is already completed
      await selectExampleGroup(targetGroupIndex)

      console.log(`✅ Restored to group ${targetGroupIndex}, example ${currentExampleIndex.value}`)
      console.log(`✅ Completed examples in this word:`, completedExamples.value)
      
      // Wait a bit before removing overlay for smooth transition
      await new Promise(resolve => setTimeout(resolve, 300))
      isRestoringPosition.value = false
      restoreInfo.value = ''
    } else {
      isRestoringPosition.value = false
      restoreInfo.value = ''
    }
  }
})

onUnmounted(() => {
  stopTimer()
})
</script>

<style scoped>
.learning-session-container {
  min-height: 100vh;
  background: linear-gradient(135deg, #1a1a2e 0%, #16213e 50%, #0f3460 100%);
  padding: 1rem;
}

.learning-content {
  max-width: 1400px;
  margin: 0 auto;
}

.vocabulary-container {
  display: grid;
  grid-template-columns: 30% 70%;
  gap: 1.5rem;
}

.dictation-container {
  display: grid;
  grid-template-columns: 30% 70%;
  gap: 1.5rem;
}

@media (max-width: 1024px) {
  .vocabulary-container {
    grid-template-columns: 1fr;
  }
  
  .dictation-container {
    grid-template-columns: 1fr;
  }
  
  .learning-content {
    max-width: 800px;
  }
}

.session-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: rgba(255, 255, 255, 0.1);
  backdrop-filter: blur(10px);
  border-radius: 15px;
  padding: 1rem 2rem;
  margin-bottom: 0.5rem;
  border: 1px solid rgba(255, 255, 255, 0.2);
}

.back-btn {
  background: linear-gradient(135deg, #e75e8d, #74c0fc);
  color: white;
  border: none;
  padding: 0.75rem 1.5rem;
  border-radius: 25px;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  transition: all 0.3s ease;
}

.back-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 5px 15px rgba(231, 94, 141, 0.4);
}

.session-info h2 {
  color: white;
  margin-bottom: 0.5rem;
  font-size: 1.8rem;
}

.session-stats {
  display: flex;
  gap: 1.5rem;
}

.stat-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  color: #b8b8b8;
  font-size: 0.9rem;
}

.stat-item i {
  color: #e75e8d;
}

.session-controls {
  display: flex;
  gap: 1rem;
  align-items: center;
}

.session-type-select {
  background: rgba(255, 255, 255, 0.1);
  color: white;
  border: 1px solid rgba(255, 255, 255, 0.3);
  border-radius: 12px;
  padding: 0.75rem 2.5rem 0.75rem 1rem;
  font-size: 1rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.3s ease;
  appearance: none;
  background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='12' height='12' viewBox='0 0 12 12'%3E%3Cpath fill='white' d='M6 9L1 4h10z'/%3E%3C/svg%3E");
  background-repeat: no-repeat;
  background-position: right 1rem center;
  background-size: 12px;
  min-width: 180px;
}

.session-type-select:hover {
  border-color: rgba(255, 255, 255, 0.5);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}

.session-type-select:focus {
  outline: none;
  border-color: #74c0fc;
  box-shadow: 0 0 0 3px rgba(116, 192, 252, 0.2);
}

.session-type-select option {
  background: #2d2d2d;
  color: white;
  padding: 0.5rem;
  font-weight: 500;
}

.speech-settings-btn {
  background: rgba(255, 255, 255, 0.1);
  color: white;
  border: 1px solid rgba(255, 255, 255, 0.2);
  border-radius: 10px;
  padding: 0.75rem 1rem;
  font-size: 0.9rem;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  transition: all 0.3s ease;
}

.speech-settings-btn:hover {
  background: rgba(255, 255, 255, 0.2);
  transform: translateY(-2px);
}

.progress-container {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.progress-bar {
  flex: 1;
  height: 10px;
  background: rgba(255, 255, 255, 0.1);
  border-radius: 5px;
  overflow: hidden;
}

.progress-fill {
  height: 100%;
  background: linear-gradient(135deg, #e75e8d, #74c0fc);
  border-radius: 5px;
  transition: width 0.3s ease;
}

.progress-text {
  color: white;
  font-weight: bold;
  min-width: 50px;
  text-align: right;
}

.loading-state, .error-state {
  text-align: center;
  padding: 4rem;
  color: white;
}

.cyber-spinner {
  width: 60px;
  height: 60px;
  border: 3px solid rgba(231, 94, 141, 0.3);
  border-top: 3px solid #e75e8d;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin: 0 auto 1rem;
}

.error-icon {
  font-size: 4rem;
  margin-bottom: 1rem;
}

.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.8);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.session-complete-modal {
  background: linear-gradient(135deg, rgba(26, 26, 46, 0.95) 0%, rgba(22, 33, 62, 0.95) 100%);
  border: 1px solid rgba(231, 94, 141, 0.3);
  border-radius: 20px;
  padding: 2rem;
  max-width: 600px;
  width: 90%;
  backdrop-filter: blur(20px);
}

.modal-header h2 {
  color: #e75e8d;
  text-align: center;
  font-size: 2rem;
  margin-bottom: 2rem;
}

.final-stats {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 1.5rem;
  margin-bottom: 2rem;
}

.stat-card {
  background: rgba(255, 255, 255, 0.1);
  border-radius: 15px;
  padding: 1.5rem;
  text-align: center;
  border: 1px solid rgba(255, 255, 255, 0.2);
}

.stat-value {
  font-size: 2rem;
  font-weight: bold;
  color: #74c0fc;
  margin-bottom: 0.5rem;
}

.stat-label {
  color: #b8b8b8;
  font-size: 0.9rem;
}

.modal-actions {
  text-align: center;
}

.complete-btn {
  background: linear-gradient(135deg, #e75e8d, #74c0fc);
  color: white;
  border: none;
  padding: 1rem 2rem;
  border-radius: 25px;
  font-size: 1.1rem;
  cursor: pointer;
  transition: all 0.3s ease;
}

.complete-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 10px 25px rgba(231, 94, 141, 0.4);
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

/* Restoring Position Overlay */
.restoring-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: linear-gradient(135deg, rgba(26, 26, 46, 0.98) 0%, rgba(22, 33, 62, 0.98) 50%, rgba(15, 52, 96, 0.98) 100%);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 10000;
  backdrop-filter: blur(10px);
  animation: fadeIn 0.3s ease;
}

.restoring-content {
  text-align: center;
  color: white;
  max-width: 400px;
  padding: 2rem;
}

.restoring-content h3 {
  font-size: 1.5rem;
  margin: 1.5rem 0 1rem;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.restore-info {
  color: rgba(255, 255, 255, 0.7);
  font-size: 0.95rem;
  margin-top: 0.5rem;
  animation: pulse 1.5s ease-in-out infinite;
}

.cyber-spinner-large {
  width: 60px;
  height: 60px;
  border: 4px solid rgba(102, 126, 234, 0.2);
  border-top-color: #667eea;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin: 0 auto;
}

@keyframes fadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
}

@keyframes pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.6; }
}

/* Settings overlay styles */
.settings-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 9999;
  backdrop-filter: blur(2px);
}

@media (max-width: 768px) {
  .session-header {
    flex-direction: column;
    gap: 1rem;
    text-align: center;
  }
  
  .session-stats {
    justify-content: center;
  }
  
  .final-stats {
    grid-template-columns: 1fr;
  }
}
</style>