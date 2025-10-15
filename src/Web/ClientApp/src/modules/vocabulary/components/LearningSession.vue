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
            <span>{{ currentIndex + 1 }} / {{ totalWords }}</span>
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
      <!-- Vocabulary Mode -->
      <VocabularyCard 
        v-if="sessionType === 'vocabulary'"
        :word="currentWord"
        :show-answer="showAnswer"
        @answer="handleAnswer"
        @next="nextWord"
        @play-audio="playWordAudio"
      />

      <!-- Dictation Mode -->
      <DictationCard 
        v-if="sessionType === 'dictation'"
        :example="currentExample"
        @submit="handleDictationSubmit"
        @next="nextWord"
      />

      <!-- Mixed Mode -->
      <component 
        v-if="sessionType === 'mixed'"
        :is="currentMode === 'vocabulary' ? 'VocabularyCard' : 'DictationCard'"
        :word="currentMode === 'vocabulary' ? currentWord : undefined"
        :example="currentMode === 'dictation' ? currentExample : undefined"
        :show-answer="showAnswer"
        @answer="handleAnswer"
        @submit="handleDictationSubmit"
        @next="nextWord"
        @play-audio="playWordAudio"
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
const correctCount = ref(0)
const totalAttempts = ref(0)
const startTime = ref<number>(Date.now())
const elapsedTime = ref(0)
const showAnswer = ref(false)
const isSessionComplete = ref(false)
const currentSessionType = ref(props.sessionType)
const currentMode = ref<'vocabulary' | 'dictation'>('vocabulary')
const showSpeechSettings = ref(false)

// Timer
let timer: number | null = null

// Computed properties
const currentWord = computed(() => sessionWords.value[currentIndex.value])
const currentExample = computed(() => {
  const word = currentWord.value
  return word?.examples?.[0] || null
})

const totalWords = computed(() => sessionWords.value.length)
const progress = computed(() => {
  if (totalWords.value === 0) return 0
  return (currentIndex.value / totalWords.value) * 100
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
  timer = setInterval(() => {
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

const handleAnswer = (isCorrect: boolean) => {
  totalAttempts.value++
  if (isCorrect) {
    correctCount.value++
  }
  
  showAnswer.value = true
  
  // Auto advance after 2 seconds
  setTimeout(() => {
    nextWord()
  }, 2000)
}

const handleDictationSubmit = (result: any) => {
  totalAttempts.value++
  if (result.isCorrect) {
    correctCount.value++
  }
  
  // Auto advance after showing result
  setTimeout(() => {
    nextWord()
  }, 3000)
}

const nextWord = () => {
  showAnswer.value = false
  
  if (currentIndex.value < sessionWords.value.length - 1) {
    currentIndex.value++
    
    // Switch mode for mixed sessions
    if (currentSessionType.value === 'mixed') {
      currentMode.value = Math.random() > 0.5 ? 'vocabulary' : 'dictation'
    }
  } else {
    finishSession()
  }
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

const playWordAudio = async (audioUrl: string) => {
  try {
    const audio = new Audio(audioUrl)
    await audio.play()
  } catch (err) {
    console.error('Failed to play audio:', err)
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
  border: 1px solid rgba(255, 255, 255, 0.2);
  border-radius: 10px;
  padding: 0.75rem 1rem;
  font-size: 1rem;
  cursor: pointer;
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