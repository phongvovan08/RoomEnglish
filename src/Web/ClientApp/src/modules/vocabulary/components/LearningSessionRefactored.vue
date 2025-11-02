<template>
  <div class="learning-session-container">
    <!-- Restoring Position Overlay -->
    <RestorePositionOverlay 
      :is-restoring="isRestoringPosition" 
      :info="restoreInfo" 
    />

    <!-- Main Content - Hidden during restore -->
    <template v-if="!isRestoringPosition">
      <!-- Session Header -->
      <SessionHeader
        :category-name="category.name"
        :session-type="currentSessionType"
        :selected-group-index="selectedGroupIndex"
        :current-index="currentIndex"
        :current-example-in-group="currentExampleInGroup"
        :current-group-size="currentGroupSize"
        :current-example-number="currentExampleNumber"
        :total-examples="totalExamples"
        :total-words="totalWords"
        :correct-count="correctCount"
        :formatted-time="formatTime(elapsedTime)"
        @back="$emit('back')"
        @change-session-type="changeSessionType"
        @open-settings="showSpeechSettings = true"
      />

      <!-- Progress Bar -->
      <SessionProgress :progress="progress" />

      <!-- Learning Content -->
      <div class="learning-content" v-if="currentWord">
        <!-- Example Grid -->
        <ExampleGrid
          v-if="showExampleGrid"
          :word="currentWord"
          :examples="currentWord.examples || []"
          :completed-examples="completedExamples"
          @select-group="selectExampleGroup"
          @back="navigation.backToVocabulary"
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
            @select-word="navigation.jumpToWord"
            @load-more="loadMoreWords"
          />
          
          <VocabularyCard 
            :word="currentWord || {}"
            :is-loading="isLoading"
            @next="navigation.nextWord"
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
            @select-example="navigation.jumpToExample"
          />
          
          <DictationCard 
            :example="currentExample"
            :word="currentWord"
            :show-back-to-grid="selectedGroupIndex !== null"
            @submit="handleDictationSubmit"
            @next="navigation.nextWord"
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
          @next="navigation.nextWord"
        />
      </div>

      <!-- Error State -->
      <div v-if="error" class="error-state">
        <div class="error-icon">⚠️</div>
        <div class="error-message">{{ error }}</div>
        <button @click="retry" class="retry-btn">Try Again</button>
      </div>

      <!-- Session Complete Modal -->
      <SessionCompleteModal
        v-if="isSessionComplete"
        :correct-count="correctCount"
        :accuracy="accuracy"
        :elapsed-time="elapsedTime"
        :final-score="finalScore"
        :format-time="formatTime"
        @complete="completeSession"
      />

      <!-- Speech Settings Panel -->
      <div v-if="showSpeechSettings" class="settings-overlay" @click="showSpeechSettings = false">
        <div @click.stop">
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
import { ref, onMounted, onUnmounted } from 'vue'
import { useRoute } from 'vue-router'
import { useLearningSession } from '../composables/useLearningSession'
import { useLearningNavigation } from '../composables/useLearningNavigation'
import type { VocabularyCategory, LearningSession } from '../types/vocabulary.types'
import VocabularyCard from './VocabularyCard.vue'
import DictationCard from './DictationCard.vue'
import ExampleGrid from './ExampleGrid.vue'
import ExampleSidebar from './ExampleSidebar.vue'
import WordSidebar from './WordSidebar.vue'
import SpeechSettingsPanel from '../../../components/SpeechSettingsPanel.vue'
import SessionHeader from './session/SessionHeader.vue'
import SessionProgress from './session/SessionProgress.vue'
import RestorePositionOverlay from './session/RestorePositionOverlay.vue'
import SessionCompleteModal from './session/SessionCompleteModal.vue'

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

const route = useRoute()

// Use composables
const session = useLearningSession(props.category, props.sessionType)

const navigation = useLearningNavigation({
  sessionWords: session.sessionWords,
  currentIndex: session.currentIndex,
  currentExampleIndex: session.currentExampleIndex,
  currentSessionType: session.currentSessionType,
  originalSessionType: session.originalSessionType,
  selectedGroupIndex: session.selectedGroupIndex,
  showExampleGrid: session.showExampleGrid,
  completedExamples: session.completedExamples,
  currentMode: session.currentMode,
  onFinish: session.finishSession,
  clearLearningPosition: session.clearLearningPosition
})

// Extract refs from session composable
const {
  sessionWords,
  currentIndex,
  currentExampleIndex,
  correctCount,
  totalAttempts,
  elapsedTime,
  isSessionComplete,
  currentSessionType,
  currentMode,
  showExampleGrid,
  completedExamples,
  selectedGroupIndex,
  submittingExampleIndex,
  hasMoreWords,
  isLoadingMore,
  isLoading,
  error,
  userProgressData,
  
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
  
  formatTime,
  loadSessionWords,
  loadMoreWords,
  updateAllWordsCompletionCount,
  completeSession: sessionCompleteSession,
  stopTimer,
  
  updateWordProgress,
  updateExampleProgress,
  getUserProgress,
  getLearningPosition,
  saveLearningPosition,
  clearLearningPosition
} = session

// Local state
const showSpeechSettings = ref(false)
const isRestoringPosition = ref(false)
const restoreInfo = ref<string>('')

// Get current group examples
const getCurrentGroupExamples = () => {
  if (!currentWord.value || selectedGroupIndex.value === null) return []
  const groupSize = 10
  const startIndex = selectedGroupIndex.value * groupSize
  const endIndex = Math.min(startIndex + groupSize, currentWord.value.examples?.length || 0)
  return currentWord.value.examples?.slice(startIndex, endIndex) || []
}

// Switch to dictation
const switchToDictation = async () => {
  const word = currentWord.value
  
  if (!word?.examples || word.examples.length === 0) return
  
  await getUserProgress()
  updateAllWordsCompletionCount()
  
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
  
  await selectExampleGroup(0)
}

// Select example group
const selectExampleGroup = async (groupIndex: number) => {
  showExampleGrid.value = false
  selectedGroupIndex.value = groupIndex
  
  const startIndex = groupIndex * 10
  const endIndex = Math.min(startIndex + 10, currentWord.value?.examples?.length || 0)
  
  let targetIndex = startIndex
  
  if (currentWord.value) {
    const savedPosition = await getLearningPosition(currentWord.value.id)
    
    if (savedPosition && savedPosition.groupIndex === groupIndex) {
      if (savedPosition.lastExampleIndex >= startIndex && 
          savedPosition.lastExampleIndex < endIndex &&
          !completedExamples.value.includes(savedPosition.lastExampleIndex)) {
        targetIndex = savedPosition.lastExampleIndex
      } else {
        for (let i = startIndex; i < endIndex; i++) {
          if (!completedExamples.value.includes(i)) {
            targetIndex = i
            break
          }
        }
      }
    } else {
      for (let i = startIndex; i < endIndex; i++) {
        if (!completedExamples.value.includes(i)) {
          targetIndex = i
          break
        }
      }
    }
  }
  
  currentExampleIndex.value = targetIndex
  currentSessionType.value = 'dictation'
}

// Handle dictation submit
const handleDictationSubmit = async (result: any) => {
  totalAttempts.value++
  if (result.isCorrect) correctCount.value++
  
  const isPassingScore = result.accuracyPercentage >= 75
  
  if (isPassingScore && selectedGroupIndex.value !== null) {
    submittingExampleIndex.value = currentExampleIndex.value - (selectedGroupIndex.value * 10)
  }
  
  if (isPassingScore && currentExample.value && !completedExamples.value.includes(currentExampleIndex.value)) {
    completedExamples.value = [...completedExamples.value, currentExampleIndex.value]
    
    if (currentWord.value) {
      currentWord.value.completedExampleCount = completedExamples.value.length
    }
    
    updateAllWordsCompletionCount()
  }
  
  try {
    if (currentExample.value) {
      await updateExampleProgress(currentExample.value.id, result.accuracyPercentage)
    }
    
    if (currentWord.value) {
      await updateWordProgress(currentWord.value.id, result.isCorrect)
    }
    
    await getUserProgress()
    
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
      
      currentWord.value.completedExampleCount = completedExamples.value.length
    }
    
    updateAllWordsCompletionCount()
    
    if (currentWord.value && selectedGroupIndex.value !== null) {
      await saveLearningPosition(
        currentWord.value.id,
        selectedGroupIndex.value,
        currentExampleIndex.value
      )
    }
  } finally {
    submittingExampleIndex.value = null
  }
}

// Back to example grid
const backToExampleGrid = async () => {
  await navigation.backToExampleGrid(
    getUserProgress,
    updateAllWordsCompletionCount,
    userProgressData
  )
}

// Complete session
const completeSession = async () => {
  try {
    const result = await sessionCompleteSession()
    emit('complete', result)
  } catch (err) {
    console.error('Failed to complete session:', err)
  }
}

// Change session type
const changeSessionType = (event: Event) => {
  const target = event.target as HTMLSelectElement
  const newType = target.value as 'vocabulary' | 'dictation' | 'mixed'
  
  emit('update:sessionType', newType)
  currentSessionType.value = newType
  
  if (newType === 'mixed') {
    currentMode.value = 'vocabulary'
  }
}

// Retry
const retry = () => {
  loadSessionWords()
}

// Restore position from URL
const restorePosition = async () => {
  const { wordId, groupIndex, exampleIndex } = route.query
  const shouldRestore = wordId && groupIndex !== undefined && exampleIndex !== undefined
  
  if (!shouldRestore) return
  
  isRestoringPosition.value = true
  restoreInfo.value = 'Đang tải dữ liệu...'
  
  const targetWordId = Number(wordId)
  const targetGroupIndex = Number(groupIndex)
  const targetExampleIndex = Number(exampleIndex)
  
  const wordIndex = sessionWords.value.findIndex(w => w.id === targetWordId)
  
  if (wordIndex !== -1) {
    restoreInfo.value = 'Đang khôi phục vị trí học...'
    currentIndex.value = wordIndex
    
    await new Promise(resolve => setTimeout(resolve, 50))
    
    restoreInfo.value = 'Đang tải tiến trình...'
    
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
    }
    
    restoreInfo.value = 'Đang chuyển đến ví dụ...'
    
    await selectExampleGroup(targetGroupIndex)
    currentExampleIndex.value = targetExampleIndex
    
    await new Promise(resolve => setTimeout(resolve, 300))
  }
  
  isRestoringPosition.value = false
  restoreInfo.value = ''
}

// Lifecycle
onMounted(async () => {
  const shouldRestore = route.query.wordId && route.query.groupIndex !== undefined
  
  if (shouldRestore) {
    isRestoringPosition.value = true
    restoreInfo.value = 'Đang tải dữ liệu...'
  }
  
  await loadSessionWords()
  
  if (shouldRestore) {
    await restorePosition()
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
  .vocabulary-container,
  .dictation-container {
    grid-template-columns: 1fr;
  }
  
  .learning-content {
    max-width: 800px;
  }
}

.error-state {
  text-align: center;
  padding: 4rem;
  color: white;
}

.error-icon {
  font-size: 4rem;
  margin-bottom: 1rem;
}

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
</style>
