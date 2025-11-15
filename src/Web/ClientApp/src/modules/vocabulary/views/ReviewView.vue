<template>
  <div class="review-view">
    <!-- Session Header -->
    <SessionHeader
      category-name="Daily Review"
      session-type="dictation"
      :selected-group-index="0"
      :current-index="0"
      :current-example-in-group="currentIndex + 1"
      :current-group-size="reviewItems.length"
      :current-example-number="currentIndex + 1"
      :total-examples="reviewItems.length"
      :total-words="reviewItems.length"
      :correct-count="correctCount"
      :formatted-time="''"
      @back="router.push('/learning')"
      @change-session-type="() => {}"
      @open-settings="() => {}"
    />

    <!-- Progress Bar -->
    <SessionProgress :progress="progress" />

    <div v-if="loading" class="loading-wrapper">
      <!-- Loading Text -->
      <div class="skeleton-loading-text">
        <div class="cyber-spinner-small"></div>
        <p>Preparing your learning session...</p>
      </div>
      
      <!-- Skeleton Grid -->
      <div class="review-content">
        <!-- Sidebar Skeleton -->
        <div class="sidebar-skeleton">
          <div class="skeleton-title"></div>
          <div class="skeleton-items">
            <div class="skeleton-item" v-for="i in 5" :key="i"></div>
          </div>
        </div>
        
        <!-- Card Skeleton -->
        <div class="card-skeleton">
          <div class="skeleton-header"></div>
          <div class="skeleton-audio"></div>
          <div class="skeleton-input"></div>
          <div class="skeleton-buttons"></div>
        </div>
      </div>
    </div>

    <div v-else-if="error" class="error-state">
      <Icon icon="mdi:alert-circle" class="w-12 h-12" />
      <p>{{ error }}</p>
      <button @click="loadReviewExamples" class="retry-btn">Retry</button>
    </div>

    <div v-else-if="!isComplete && currentItem" class="review-content">
      <!-- Sidebar with example list -->
      <div class="review-sidebar">
        <div class="sidebar-header">
          <div class="sidebar-title">
            <Icon icon="mdi:refresh-circle" class="title-icon" />
            <span>Daily Review</span>
          </div>
          <div class="sidebar-stats">
            <span class="stat-item">
              <Icon icon="mdi:check-circle" class="success" />
              {{ correctCount }}/{{ reviewItems.length }}
            </span>
          </div>
        </div>
        
        <div class="example-list">
          <div 
            v-for="(item, index) in reviewItems" 
            :key="item.exampleId"
            class="example-item"
            :class="{ 
              'completed': completedIndices.includes(index),
              'current': index === currentIndex 
            }"
            @click="jumpToExample(index)"
          >
            <div class="item-number">{{ index + 1 }}</div>
            <div class="item-content">
              <div class="word-name">{{ item.word }}</div>
              <div class="category-name">{{ item.categoryName }}</div>
            </div>
            <div class="item-status">
              <Icon 
                v-if="submittingIndex === index" 
                icon="mdi:loading" 
                class="status-icon submitting-icon" 
              />
              <Icon 
                v-else-if="completedIndices.includes(index)" 
                icon="mdi:check-circle" 
                class="status-icon completed-icon" 
              />
              <Icon 
                v-else-if="index === currentIndex" 
                icon="mdi:play-circle" 
                class="status-icon current-icon" 
              />
              <Icon 
                v-else 
                icon="mdi:circle-outline" 
                class="status-icon pending-icon" 
              />
            </div>
          </div>
        </div>
      </div>

      <!-- Dictation Card -->
      <DictationCard
        :example="{
          id: currentItem.exampleId,
          sentence: currentItem.sentence,
          translation: currentItem.translation || '',
          audioUrl: currentItem.audioUrl || '',
          difficultyLevel: currentItem.difficultyLevel,
          isActive: true,
          displayOrder: 0,
          wordId: currentItem.wordId
        }"
        :word="{
          id: currentItem.wordId,
          word: currentItem.word,
          phonetic: currentItem.phonetic || '',
          partOfSpeech: currentItem.partOfSpeech || '',
          meaning: currentItem.meaning,
          definition: '',
          vietnameseMeaning: currentItem.vietnameseMeaning || '',
          difficultyLevel: currentItem.difficultyLevel,
          isActive: true,
          viewCount: 0,
          correctCount: 0,
          incorrectCount: 0,
          categoryId: 0,
          categoryName: currentItem.categoryName,
          exampleCount: 0,
          completedExampleCount: 0,
          exampleCompletionPercentage: 0,
          examples: []
        }"
        :show-back-to-grid="false"
        @submit="handleSubmit"
        @next="nextExample"
      />
    </div>

    <div v-else-if="isComplete" class="complete-state">
      <div class="complete-content">
        <Icon icon="mdi:party-popper" class="complete-icon" />
        <h2>Review Complete!</h2>
        <div class="complete-stats">
          <div class="stat-card">
            <Icon icon="mdi:check-circle" class="stat-card-icon success" />
            <div class="stat-card-content">
              <span class="stat-card-value">{{ correctCount }}</span>
              <span class="stat-card-label">Correct</span>
            </div>
          </div>
          <div class="stat-card">
            <Icon icon="mdi:close-circle" class="stat-card-icon error" />
            <div class="stat-card-content">
              <span class="stat-card-value">{{ reviewItems.length - correctCount }}</span>
              <span class="stat-card-label">Incorrect</span>
            </div>
          </div>
          <div class="stat-card">
            <Icon icon="mdi:percent" class="stat-card-icon" />
            <div class="stat-card-content">
              <span class="stat-card-value">{{ accuracy }}%</span>
              <span class="stat-card-label">Accuracy</span>
            </div>
          </div>
        </div>
        <div class="complete-actions">
          <button @click="loadReviewExamples" class="action-btn primary">
            <Icon icon="mdi:refresh" />
            Review Again
          </button>
          <button @click="router.push('/learning')" class="action-btn">
            <Icon icon="mdi:home" />
            Back to Learning
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { Icon } from '@iconify/vue'
import { createAuthHeaders } from '@/utils/auth'
import DictationCard from '../components/DictationCard.vue'
import SessionHeader from '../components/session/SessionHeader.vue'
import SessionProgress from '../components/session/SessionProgress.vue'

const router = useRouter()

interface ReviewItem {
  exampleId: number
  wordId: number
  word: string
  phonetic?: string
  partOfSpeech?: string
  meaning: string
  vietnameseMeaning?: string
  categoryName: string
  sentence: string
  translation?: string
  audioUrl?: string
  difficultyLevel: number
  isMastered: boolean
  studiedTimes: number
}

const reviewItems = ref<ReviewItem[]>([])
const currentIndex = ref(0)
const correctCount = ref(0)
const completedIndices = ref<number[]>([])
const submittingIndex = ref<number | null>(null)
const loading = ref(false)
const error = ref<string | null>(null)
const isComplete = ref(false)

const currentItem = computed(() => reviewItems.value[currentIndex.value])
const progress = computed(() => {
  if (reviewItems.value.length === 0) return 0
  return (completedIndices.value.length / reviewItems.value.length) * 100
})
const accuracy = computed(() => {
  if (reviewItems.value.length === 0) return 0
  return Math.round((correctCount.value / reviewItems.value.length) * 100)
})

const loadReviewExamples = async () => {
  loading.value = true
  error.value = null
  isComplete.value = false
  currentIndex.value = 0
  correctCount.value = 0
  completedIndices.value = []

  try {
    const response = await fetch('/api/user-progress/review?count=20', {
      headers: createAuthHeaders()
    })

    if (!response.ok) {
      throw new Error('Failed to load review examples')
    }

    const data = await response.json()
    reviewItems.value = data.items || []

    if (reviewItems.value.length === 0) {
      error.value = 'No examples available for review'
    }
  } catch (err: any) {
    console.error('Failed to load review examples:', err)
    error.value = err.message || 'Failed to load review examples'
  } finally {
    loading.value = false
  }
}

const handleSubmit = async (result: any) => {
  submittingIndex.value = currentIndex.value
  
  const isCorrect = result.isCorrect || result.accuracyPercentage >= 75
  
  if (isCorrect) {
    correctCount.value++
    if (!completedIndices.value.includes(currentIndex.value)) {
      completedIndices.value.push(currentIndex.value)
    }
  }

  // Update progress
  try {
    await fetch(`/api/user-progress/example/${currentItem.value.exampleId}`, {
      method: 'POST',
      headers: createAuthHeaders(),
      body: JSON.stringify({ accuracyPercentage: result.accuracyPercentage })
    })
  } catch (err) {
    console.error('Failed to update progress:', err)
  } finally {
    submittingIndex.value = null
  }
}

const nextExample = () => {
  if (currentIndex.value < reviewItems.value.length - 1) {
    currentIndex.value++
  } else {
    isComplete.value = true
  }
}

const jumpToExample = (index: number) => {
  currentIndex.value = index
}

onMounted(() => {
  loadReviewExamples()
})
</script>

<style scoped>
.review-view {
  min-height: 100vh;
  background: linear-gradient(135deg, #1a1a2e 0%, #16213e 50%, #0f3460 100%);
  padding: 0;
}

.loading-state,
.error-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  min-height: 400px;
  color: white;
  gap: 1rem;
}

.error-state {
  color: #ff6b6b;
}

.retry-btn {
  padding: 0.75rem 1.5rem;
  background: linear-gradient(135deg, #74c0fc 0%, #4dabf7 100%);
  color: white;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  font-size: 1rem;
  font-weight: 600;
  transition: all 0.3s;
}

.retry-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(116, 192, 252, 0.4);
}

.review-content {
  display: grid;
  grid-template-columns: 320px 1fr;
  gap: 2rem;
  padding: 2rem;
  max-width: 1600px;
  margin: 0 auto;
}

/* Skeleton Loading Styles */
.loading-wrapper {
  padding: 2rem;
}

.skeleton-loading-text {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 1rem;
  padding: 0 0 1rem 0;
  color: #74c0fc;
  font-size: 1.1rem;
  font-weight: 500;
}

.cyber-spinner-small {
  width: 24px;
  height: 24px;
  border: 3px solid rgba(116, 192, 252, 0.3);
  border-top: 3px solid #74c0fc;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.sidebar-skeleton,
.card-skeleton {
  background: rgba(255, 255, 255, 0.05);
  border-radius: 15px;
  padding: 1.5rem;
  border: 1px solid rgba(255, 255, 255, 0.1);
}

.sidebar-skeleton {
  height: 500px;
}

.skeleton-title {
  height: 30px;
  background: linear-gradient(90deg, rgba(255, 255, 255, 0.1) 25%, rgba(255, 255, 255, 0.2) 50%, rgba(255, 255, 255, 0.1) 75%);
  background-size: 200% 100%;
  animation: shimmer 1.5s infinite;
  border-radius: 8px;
  margin-bottom: 1.5rem;
}

.skeleton-items {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.skeleton-item {
  height: 50px;
  background: linear-gradient(90deg, rgba(255, 255, 255, 0.1) 25%, rgba(255, 255, 255, 0.2) 50%, rgba(255, 255, 255, 0.1) 75%);
  background-size: 200% 100%;
  animation: shimmer 1.5s infinite;
  border-radius: 10px;
}

.card-skeleton {
  height: 600px;
  display: flex;
  flex-direction: column;
  gap: 2rem;
}

.skeleton-header {
  height: 80px;
  background: linear-gradient(90deg, rgba(255, 255, 255, 0.1) 25%, rgba(255, 255, 255, 0.2) 50%, rgba(255, 255, 255, 0.1) 75%);
  background-size: 200% 100%;
  animation: shimmer 1.5s infinite;
  border-radius: 12px;
}

.skeleton-audio {
  height: 100px;
  background: linear-gradient(90deg, rgba(255, 255, 255, 0.1) 25%, rgba(255, 255, 255, 0.2) 50%, rgba(255, 255, 255, 0.1) 75%);
  background-size: 200% 100%;
  animation: shimmer 1.5s infinite;
  border-radius: 12px;
}

.skeleton-input {
  flex: 1;
  background: linear-gradient(90deg, rgba(255, 255, 255, 0.1) 25%, rgba(255, 255, 255, 0.2) 50%, rgba(255, 255, 255, 0.1) 75%);
  background-size: 200% 100%;
  animation: shimmer 1.5s infinite;
  border-radius: 12px;
}

.skeleton-buttons {
  height: 60px;
  background: linear-gradient(90deg, rgba(255, 255, 255, 0.1) 25%, rgba(255, 255, 255, 0.2) 50%, rgba(255, 255, 255, 0.1) 75%);
  background-size: 200% 100%;
  animation: shimmer 1.5s infinite;
  border-radius: 12px;
}

@keyframes shimmer {
  0% {
    background-position: -200% 0;
  }
  100% {
    background-position: 200% 0;
  }
}

.review-sidebar {
  background: rgba(0, 0, 0, 0.3);
  border-radius: 15px;
  padding: 1.5rem;
  border: 1px solid rgba(255, 255, 255, 0.1);
  height: fit-content;
  max-height: 80vh;
  overflow: hidden;
  position: sticky;
  top: 20px;
  display: flex;
  flex-direction: column;
}

.sidebar-header {
  margin-bottom: 1.5rem;
  padding-bottom: 1rem;
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
}

.sidebar-title {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  color: white;
  font-size: 1.2rem;
  font-weight: bold;
  margin-bottom: 0.75rem;
}

.title-icon {
  font-size: 1.5rem;
  color: #74c0fc;
}

.sidebar-stats {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  color: #74c0fc;
  font-size: 0.95rem;
}

.sidebar-stats .stat-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.sidebar-stats .success {
  color: #51cf66;
  font-size: 1.2rem;
}

.example-list {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  flex: 1;
  overflow-y: auto;
  overflow-x: hidden;
}

.example-item {
  background: rgba(255, 255, 255, 0.05);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 10px;
  padding: 0.75rem;
  cursor: pointer;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.example-item:hover {
  background: rgba(255, 255, 255, 0.1);
  border-color: rgba(116, 192, 252, 0.5);
  transform: translateX(5px);
}

.example-item.current {
  border-color: #e75e8d;
  background: rgba(231, 94, 141, 0.1);
  box-shadow: 0 0 10px rgba(231, 94, 141, 0.3);
}

.example-item.completed {
  border-color: rgba(76, 175, 80, 0.3);
}

.item-number {
  background: linear-gradient(135deg, #e75e8d, #74c0fc);
  color: white;
  width: 32px;
  height: 32px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 0.9rem;
  font-weight: bold;
  flex-shrink: 0;
}

.example-item.completed .item-number {
  background: linear-gradient(135deg, #4caf50, #66bb6a);
}

.item-content {
  flex: 1;
  min-width: 0;
}

.word-name {
  color: white;
  font-size: 0.95rem;
  font-weight: 600;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.category-name {
  color: #adb5bd;
  font-size: 0.8rem;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.item-status {
  flex-shrink: 0;
}

.status-icon {
  font-size: 1.5rem;
}

.submitting-icon {
  color: #ffa726;
  animation: spin 1s linear infinite;
}

.completed-icon {
  color: #4caf50;
}

.current-icon {
  color: #e75e8d;
  animation: pulse 1.5s ease-in-out infinite;
}

.pending-icon {
  color: #666;
}

@keyframes spin {
  from {
    transform: rotate(0deg);
  }
  to {
    transform: rotate(360deg);
  }
}

@keyframes pulse {
  0%, 100% {
    opacity: 1;
  }
  50% {
    opacity: 0.5;
  }
}

/* Scrollbar styling */
.example-list::-webkit-scrollbar {
  width: 6px;
}

.example-list::-webkit-scrollbar-track {
  background: rgba(255, 255, 255, 0.05);
  border-radius: 3px;
}

.example-list::-webkit-scrollbar-thumb {
  background: rgba(116, 192, 252, 0.5);
  border-radius: 3px;
}

.example-list::-webkit-scrollbar-thumb:hover {
  background: rgba(116, 192, 252, 0.7);
}

.complete-state {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 500px;
}

.complete-content {
  text-align: center;
  max-width: 600px;
}

.complete-icon {
  font-size: 5rem;
  margin-bottom: 1rem;
}

.complete-content h2 {
  font-size: 2.5rem;
  background: linear-gradient(135deg, #74c0fc 0%, #4dabf7 100%);
  background-clip: text;
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  margin-bottom: 2rem;
}

.complete-stats {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
  gap: 1rem;
  margin-bottom: 2rem;
}

.stat-card {
  background: rgba(255, 255, 255, 0.1);
  border: 1px solid rgba(255, 255, 255, 0.2);
  border-radius: 12px;
  padding: 1.5rem;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.75rem;
}

.stat-card-icon {
  font-size: 2.5rem;
  color: #74c0fc;
}

.stat-card-icon.success {
  color: #51cf66;
}

.stat-card-icon.error {
  color: #ff6b6b;
}

.stat-card-content {
  display: flex;
  flex-direction: column;
  align-items: center;
}

.stat-card-value {
  font-size: 2rem;
  font-weight: bold;
  color: white;
}

.stat-card-label {
  font-size: 0.9rem;
  color: #adb5bd;
}

.complete-actions {
  display: flex;
  gap: 1rem;
  justify-content: center;
  flex-wrap: wrap;
}

.action-btn {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.875rem 1.75rem;
  border: none;
  border-radius: 10px;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s;
  background: rgba(255, 255, 255, 0.1);
  color: white;
  border: 1px solid rgba(255, 255, 255, 0.2);
}

.action-btn.primary {
  background: linear-gradient(135deg, #74c0fc 0%, #4dabf7 100%);
  border: none;
}

.action-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(116, 192, 252, 0.3);
}

@media (max-width: 1024px) {
  .review-content {
    grid-template-columns: 1fr;
    padding: 1rem;
  }

  .review-sidebar {
    position: static;
    max-height: none;
    margin-bottom: 1.5rem;
  }

  .example-list {
    max-height: 300px;
  }
}

@media (max-width: 768px) {
  .complete-content h2 {
    font-size: 2rem;
  }
}
</style>
