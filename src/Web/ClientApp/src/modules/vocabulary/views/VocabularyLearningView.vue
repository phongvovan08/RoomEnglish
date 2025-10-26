<template>
  <div class="vocabulary-learning-container">

    <!-- Category Selection -->
    <div class="categories-section" v-if="!selectedCategory">
      <div class="section-title">
        <h2>📚 Choose Your Learning Path</h2>
      </div>
      
      <div class="categories-grid" v-if="categories.length">
        <div 
          v-for="category in categories" 
          :key="category.id"
          class="category-card"
          :style="{ '--category-color': getCategoryColor(category.color) }"
          :class="{ 'mastered': getCategoryProgress(category.id) >= 80 }"
          @click="selectCategory(category)"
        >
          <div class="card-background">
            <div class="mastery-badge" v-if="getCategoryProgress(category.id) >= 80">
              <i class="mdi mdi-crown"></i>
            </div>
            <div class="card-icon">
              <i :class="getIconClass(category.iconName)"></i>
            </div>
            <div class="card-content">
              <h3>{{ category.name }}</h3>
              <p>{{ category.description }}</p>
              <div class="stats">
                <span class="word-count">{{ category.wordCount }} words</span>
              </div>
              <div class="progress-info">
                <div class="progress-bar-mini">
                  <div 
                    class="progress-fill-mini"
                    :style="{ width: `${getCategoryProgress(category.id)}%` }"
                  ></div>
                </div>
                <span class="progress-text-mini">{{ getCategoryProgress(category.id) }}% Complete</span>
              </div>
            </div>
            <div class="card-glow"></div>
          </div>
        </div>
      </div>

      <!-- Loading State -->
      <div v-if="isLoading" class="loading-container">
        <div class="cyber-spinner"></div>
        <p>Loading categories...</p>
      </div>

      <!-- Error State -->
      <div v-if="error" class="error-container">
        <div class="error-message">{{ error }}</div>
        <button @click="loadCategories" class="retry-btn">Retry</button>
      </div>
    </div>

    <!-- Learning Session -->
    <div class="learning-session" v-if="selectedCategory && !isCompleted">
      <LearningSessionComponent 
        :category="selectedCategory"
        v-model:session-type="sessionType"
        @complete="handleSessionComplete"
        @back="goBack"
      />
    </div>

    <!-- Session Complete -->
    <div class="session-complete" v-if="isCompleted && sessionResult">
      <SessionResult 
        :result="sessionResult"
        @restart="restart"
        @new-category="goBack"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useVocabulary } from '../composables/useVocabulary'
import { useUserProgress } from '../composables/useUserProgress'
import type { VocabularyCategory, LearningSession } from '../types/vocabulary.types'
import LearningSessionComponent from '../components/LearningSession.vue'
import SessionResult from '../components/SessionResult.vue'

const { 
  categories, 
  isLoading, 
  error, 
  getCategories,
  clearError 
} = useVocabulary()

const {
  progress,
  getUserProgress,
  getCategoryProgress: getCategoryProgressFromAPI,
  recalculateCategoryProgress
} = useUserProgress()

const selectedCategory = ref<VocabularyCategory | null>(null)
const sessionType = ref<'vocabulary' | 'dictation' | 'mixed'>('vocabulary')
const isCompleted = ref(false)
const sessionResult = ref<LearningSession | null>(null)

const getCategoryColor = (color: string) => {
  const colors: Record<string, string> = {
    'Blue': '#0f3460',
    'Green': '#0e4b6b',
    'Purple': '#4a154b',
    'Red': '#d32f2f',
    'Orange': '#f57c00',
    'Pink': '#e75e8d'
  }
  return colors[color] || '#0f3460'
}

const getIconClass = (iconName: string) => {
  return iconName.startsWith('mdi:') ? `mdi ${iconName.replace('mdi:', 'mdi-')}` : iconName
}

const selectCategory = (category: VocabularyCategory) => {
  selectedCategory.value = category
  isCompleted.value = false
  sessionResult.value = null
}

const goBack = () => {
  selectedCategory.value = null
  isCompleted.value = false
  sessionResult.value = null
}

const handleSessionComplete = async (result: LearningSession) => {
  sessionResult.value = result
  isCompleted.value = true
  
  // Update category progress via API
  if (selectedCategory.value) {
    await recalculateCategoryProgress(selectedCategory.value.id)
  }
}

const restart = () => {
  isCompleted.value = false
  sessionResult.value = null
}

const loadCategories = async () => {
  try {
    clearError()
    await getCategories({ pageSize: 20, includeInactive: false })
  } catch (err) {
    console.error('Failed to load categories:', err)
  }
}

const getCategoryProgress = (categoryId: number): number => {
  const categoryProgress = getCategoryProgressFromAPI(categoryId)
  return categoryProgress ? categoryProgress.completionPercentage : 0
}

onMounted(async () => {
  await loadCategories()
  await getUserProgress()
})
</script>

<style scoped>
.vocabulary-learning-container {
  min-height: 100vh;
  background: linear-gradient(135deg, #1a1a2e 0%, #16213e 50%, #0f3460 100%);
  position: relative;
  overflow-x: hidden;
}

.gaming-header {
  text-align: center;
  padding: 2rem 0;
  position: relative;
}

.header-background {
  background: linear-gradient(135deg, rgba(231, 94, 141, 0.1) 0%, rgba(15, 52, 96, 0.2) 100%);
  border-bottom: 2px solid rgba(231, 94, 141, 0.3);
  padding: 2rem 0;
  backdrop-filter: blur(10px);
}

.neon-text h1 {
  font-size: 3rem;
  font-weight: bold;
  color: transparent;
  background: linear-gradient(135deg, #e75e8d, #74c0fc, #e75e8d);
  background-clip: text;
  -webkit-background-clip: text;
  text-shadow: 0 0 30px rgba(231, 94, 141, 0.5);
  margin-bottom: 0.5rem;
  animation: glow 2s ease-in-out infinite alternate;
}

.subtitle {
  color: #74c0fc;
  font-size: 1.2rem;
  font-style: italic;
}

.categories-section {
  padding: 3rem 2rem;
  max-width: 1200px;
  margin: 0 auto;
}

.section-title h2 {
  color: #e75e8d;
  font-size: 2.5rem;
  text-align: center;
  margin-bottom: 3rem;
  text-shadow: 0 0 20px rgba(231, 94, 141, 0.5);
}

.categories-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(350px, 1fr));
  gap: 2rem;
  margin-top: 2rem;
}

.category-card {
  cursor: pointer;
  transition: all 0.3s ease;
  position: relative;
  height: 250px;
}

.card-background {
  height: 100%;
  background: linear-gradient(135deg, 
    rgba(255, 255, 255, 0.1) 0%, 
    rgba(255, 255, 255, 0.05) 100%);
  border: 1px solid rgba(255, 255, 255, 0.2);
  border-radius: 15px;
  backdrop-filter: blur(10px);
  padding: 2rem;
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  position: relative;
  overflow: hidden;
}

.card-background::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: linear-gradient(135deg, var(--category-color, #0f3460) 0%, transparent 70%);
  opacity: 0.1;
  transition: opacity 0.3s ease;
}

.category-card:hover .card-background::before {
  opacity: 0.2;
}

.card-icon {
  font-size: 4rem;
  color: var(--category-color, #e75e8d);
  margin-bottom: 1rem;
  filter: drop-shadow(0 0 10px rgba(231, 94, 141, 0.5));
}

.card-content h3 {
  color: white;
  font-size: 2rem;
  font-weight: bold;
  margin-bottom: 0.5rem;
}

.card-content p {
  color: #b8b8b8;
  font-size: 1.1rem;
  margin-bottom: 1rem;
  line-height: 1.5;
}

.stats {
  margin-top: auto;
}

.word-count {
  background: linear-gradient(135deg, #e75e8d, #74c0fc);
  color: white;
  padding: 0.5rem 1rem;
  border-radius: 20px;
  font-size: 1rem;
  font-weight: bold;
}

.progress-info {
  width: 100%;
  margin-top: 1rem;
}

.progress-bar-mini {
  width: 100%;
  height: 8px;
  background: rgba(255, 255, 255, 0.1);
  border-radius: 10px;
  overflow: hidden;
  margin-bottom: 0.5rem;
}

.progress-fill-mini {
  height: 100%;
  background: linear-gradient(90deg, #e75e8d, #74c0fc, #e75e8d);
  border-radius: 10px;
  transition: width 0.5s ease;
  box-shadow: 0 0 10px rgba(231, 94, 141, 0.5);
}

.progress-text-mini {
  font-size: 0.95rem;
  color: #74c0fc;
  font-weight: 600;
}

.card-glow {
  position: absolute;
  top: -2px;
  left: -2px;
  right: -2px;
  bottom: -2px;
  background: linear-gradient(135deg, var(--category-color, #e75e8d), transparent, var(--category-color, #e75e8d));
  border-radius: 15px;
  z-index: -1;
  opacity: 0;
  transition: opacity 0.3s ease;
}

.category-card:hover {
  transform: translateY(-10px) scale(1.02);
}

.category-card:hover .card-glow {
  opacity: 0.3;
}

.category-card.mastered {
  border: 2px solid rgba(255, 215, 0, 0.5);
}

.category-card.mastered .card-background::before {
  background: linear-gradient(135deg, rgba(255, 215, 0, 0.2) 0%, transparent 70%);
  opacity: 0.3;
}

.mastery-badge {
  position: absolute;
  top: 1rem;
  right: 1rem;
  background: linear-gradient(135deg, #ffd700, #ffed4e);
  color: #1a1a2e;
  width: 45px;
  height: 45px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.5rem;
  box-shadow: 0 0 20px rgba(255, 215, 0, 0.6);
  animation: pulse 2s ease-in-out infinite;
  z-index: 10;
}

@keyframes pulse {
  0%, 100% { transform: scale(1); }
  50% { transform: scale(1.1); }
}

.loading-container, .error-container {
  text-align: center;
  padding: 3rem;
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

.error-message {
  color: #ff6b6b;
  font-size: 1.2rem;
  margin-bottom: 1rem;
}

.retry-btn {
  background: linear-gradient(135deg, #e75e8d, #74c0fc);
  color: white;
  border: none;
  padding: 0.75rem 2rem;
  border-radius: 25px;
  font-size: 1rem;
  cursor: pointer;
  transition: all 0.3s ease;
}

.retry-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 5px 15px rgba(231, 94, 141, 0.4);
}

@keyframes glow {
  from { text-shadow: 0 0 20px rgba(231, 94, 141, 0.5); }
  to { text-shadow: 0 0 30px rgba(231, 94, 141, 0.8), 0 0 40px rgba(116, 192, 252, 0.3); }
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

@media (max-width: 768px) {
  .categories-grid {
    grid-template-columns: 1fr;
  }
  
  .neon-text h1 {
    font-size: 2rem;
  }
  
  .categories-section {
    padding: 2rem 1rem;
  }
}
</style>