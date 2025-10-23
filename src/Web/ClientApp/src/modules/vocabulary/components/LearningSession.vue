<template>
  <div class="learning-session-container">
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
            <span v-if="currentSessionType === 'dictation'">
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
      <VocabularyCard 
        v-else-if="currentSessionType === 'vocabulary'"
        :word="currentWord"
        @next="nextWord"
        @learn-example="switchToDictation"
      />

      <!-- Dictation Mode -->
      <DictationCard 
        v-else-if="currentSessionType === 'dictation'"
        :example="currentExample"
        :word="currentWord"
        :show-back-to-grid="selectedGroupIndex !== null"
        @submit="handleDictationSubmit"
        @next="nextWord"
        @back-to-grid="backToExampleGrid"
      />

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

    <!-- Loading State -->
    <div v-if="isLoading" class="loading-state">
      <div class="cyber-spinner"></div>
      <p>Preparing your learning session...</p>
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
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useVocabulary } from '../composables/useVocabulary'
import type { VocabularyCategory, VocabularyWord, VocabularyExample, LearningSession } from '../types/vocabulary.types'
import VocabularyCard from '../components/VocabularyCard.vue'
import DictationCard from '../components/DictationCard.vue'
import ExampleGrid from '../components/ExampleGrid.vue'
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

// Timer
let timer: number | null = null

// Computed properties
const currentWord = computed(() => sessionWords.value[currentIndex.value])
const currentExample = computed(() => {
  const word = currentWord.value
  if (!word?.examples || word.examples.length === 0) return null
  return word.examples[currentExampleIndex.value] || null
})

const totalWords = computed(() => sessionWords.value.length)

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

const progress = computed(() => {
  if (currentSessionType.value === 'dictation') {
    // For dictation mode, calculate based on examples
    if (totalExamples.value === 0) return 0
    return (currentExampleNumber.value / totalExamples.value) * 100
  } else {
    // For vocabulary mode, calculate based on words
    if (totalWords.value === 0) return 0
    return (currentIndex.value / totalWords.value) * 100
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

const loadSessionWords = async () => {
  try {
    clearError()
    const result = await getWords({
      categoryId: props.category.id,
      includeExamples: true,
      includeUserProgress: true,
      pageSize: 20
    })
    
    sessionWords.value = result.items
    
    if (sessionWords.value.length === 0) {
      throw new Error('No words found in this category')
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

const handleDictationSubmit = (result: any) => {
  totalAttempts.value++
  if (result.isCorrect) {
    correctCount.value++
  }
  
  // Mark current example as completed
  if (!completedExamples.value.includes(currentExampleIndex.value)) {
    completedExamples.value.push(currentExampleIndex.value)
  }
  
  // Don't auto advance - user will click Next button in ResultDisplay
}

const nextWord = () => {
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
      // Return to example grid to show updated progress
      currentSessionType.value = 'vocabulary'
      showExampleGrid.value = true
      selectedGroupIndex.value = null
      currentExampleIndex.value = 0
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

const switchToDictation = () => {
  console.log('=== Switching to dictation mode ===')
  const word = currentWord.value
  
  if (!word?.examples || word.examples.length === 0) {
    console.log('No examples available for this word')
    return
  }
  
  console.log('Current word:', word.word)
  console.log('Examples count:', word.examples.length)
  
  // Show example grid instead of going directly to dictation
  showExampleGrid.value = true
  
  console.log('Showing example grid')
}

const selectExampleGroup = (groupIndex: number) => {
  console.log('=== Example group selected:', groupIndex, '===')
  
  // Hide grid and show dictation card for first example in group
  showExampleGrid.value = false
  selectedGroupIndex.value = groupIndex
  
  // Calculate start index of the group (each group has 10 examples)
  const startIndex = groupIndex * 10
  currentExampleIndex.value = startIndex
  currentSessionType.value = 'dictation'
  
  console.log('Switched to dictation mode for group', groupIndex, 'starting at example', startIndex)
}

const backToVocabulary = () => {
  console.log('=== Going back to vocabulary ===')
  showExampleGrid.value = false
  currentSessionType.value = originalSessionType.value
  currentExampleIndex.value = 0
  selectedGroupIndex.value = null
}

const backToExampleGrid = () => {
  console.log('=== Going back to example grid ===')
  // Return to example grid from dictation
  showExampleGrid.value = true
  currentSessionType.value = 'vocabulary'
  selectedGroupIndex.value = null
  currentExampleIndex.value = 0
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

onMounted(() => {
  loadSessionWords()
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

.session-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: rgba(255, 255, 255, 0.1);
  backdrop-filter: blur(10px);
  border-radius: 15px;
  padding: 1rem 2rem;
  margin-bottom: 2rem;
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
  margin-bottom: 2rem;
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

.learning-content {
  max-width: 800px;
  margin: 0 auto;
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