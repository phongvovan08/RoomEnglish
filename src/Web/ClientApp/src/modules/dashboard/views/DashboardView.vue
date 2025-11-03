<template>
  <div class="cyborg-dashboard fade-in-up">
    <!-- Main Banner -->
    <div class="main-banner cyborg-card hover-glow">
      <div class="banner-content">
        <h6>Welcome To</h6>
        <h4><em>Room</em>English Dashboard</h4>
        <p>{{ $t('dashboard.welcome') }}</p>

        <!-- Continue Learning Button -->
        <div v-if="lastPosition" class="main-button">
          <button 
            @click="continueLearning" 
            class="cyborg-btn continue-btn"
            :disabled="isNavigating"
          >
            <div class="btn-content">
              <Icon 
                :icon="isNavigating ? 'mdi:loading' : 'mdi:book-open-page-variant'" 
                :class="['icon', { 'spinning': isNavigating }]" 
              />
              <div class="btn-text">
                <span class="primary-text">
                  {{ isNavigating ? 'Đang tải...' : `Tiếp tục học: ${lastPosition.wordText}` }}
                </span>
                <span class="secondary-text" v-if="!isNavigating">
                  {{ lastPosition.categoryName }} • Ví dụ {{ lastPosition.lastExampleIndex + 1 }}
                </span>
              </div>
            </div>
            <div class="btn-shine"></div>
          </button>
        </div>
        
        <!-- Loading skeleton when fetching position -->
        <div v-else-if="isLoadingPosition" class="main-button">
          <div class="skeleton-btn">
            <div class="skeleton-icon pulse-slow"></div>
            <div class="skeleton-text">
              <div class="skeleton-line primary pulse-delay-1"></div>
              <div class="skeleton-line secondary pulse-delay-2"></div>
            </div>
          </div>
        </div>
        
        <!-- Placeholder when no position -->
        <div v-else class="main-button">
          <button class="cyborg-btn no-position-btn" disabled>
            <Icon icon="mdi:book-outline" class="icon" />
            <span>Chưa có tiến trình học</span>
          </button>
        </div>
      </div>
    </div>
    
    <!-- Gaming Library Stats -->
    <div class="gaming-library">
      <div class="section-heading">
        <h6>Your Dashboard</h6>
        <h4>Library <em>Statistics</em></h4>
      </div>
      
      <div class="cyborg-grid cyborg-grid-4">
        <div class="gaming-library-item hover-slide">
          <div class="left-image">
            <Icon icon="mdi:format-list-bulleted-square" class="stat-icon todo-lists" />
          </div>
          <div class="right-content">
            <h4>{{ stats.todoLists }}</h4>
            <span>{{ $t('menu.todoLists') }}</span>
            <div class="download">
              <span>Total Lists: {{ stats.todoLists }}</span>
            </div>
          </div>
        </div>

        <div class="gaming-library-item hover-slide">
          <div class="left-image">
            <Icon icon="mdi:checkbox-marked-circle" class="stat-icon todo-items" />
          </div>
          <div class="right-content">
            <h4>{{ stats.todoItems }}</h4>
            <span>{{ $t('menu.todoItems') }}</span>
            <div class="download">
              <span>Active Tasks: {{ stats.todoItems }}</span>
            </div>
          </div>
        </div>

        <div class="gaming-library-item hover-slide">
          <div class="left-image">
            <Icon icon="mdi:weather-cloudy" class="stat-icon weather" />
          </div>
          <div class="right-content">
            <h4>{{ stats.temperature }}°C</h4>
            <span>{{ $t('dashboard.currentTemp') }}</span>
            <div class="download">
              <span>Current Weather</span>
            </div>
          </div>
        </div>

        <div class="gaming-library-item hover-slide">
          <div class="left-image">
            <Icon icon="mdi:post" class="stat-icon posts" />
          </div>
          <div class="right-content">
            <h4>{{ stats.posts }}</h4>
            <span>{{ $t('menu.posts') }}</span>
            <div class="download">
              <span>Blog Posts</span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Most Popular Games (Quick Actions) -->
    <div class="most-popular cyborg-section">
      <div class="section-heading">
        <h6>Most Popular</h6>
        <h4>Quick <em>Actions</em> Right Now</h4>
      </div>
      
      <div class="cyborg-grid cyborg-grid-4">
        <div class="game-item hover-glow">
          <div class="thumb">
            <router-link :to="Routes.TodoLists.children.Create.path">
              <Icon icon="mdi:plus-circle" class="game-thumb-icon" />
            </router-link>
          </div>
          <div class="down-content">
            <h4>{{ $t('dashboard.createTodoList') }}</h4>
            <span>Quick Action</span>
            <ul>
              <li><i class="fa fa-star"></i> 4.8</li>
              <li><i class="fa fa-download"></i> 2.3M</li>
            </ul>
          </div>
        </div>

        <div class="game-item hover-glow">
          <div class="thumb">
            <router-link :to="Routes.TodoItems.children.Create.path">
              <Icon icon="mdi:checkbox-marked-circle" class="game-thumb-icon" />
            </router-link>
          </div>
          <div class="down-content">
            <h4>{{ $t('dashboard.createTodoItem') }}</h4>
            <span>Quick Action</span>
            <ul>
              <li><i class="fa fa-star"></i> 4.9</li>
              <li><i class="fa fa-download"></i> 1.8M</li>
            </ul>
          </div>
        </div>

        <div class="game-item hover-glow">
          <div class="thumb">
            <router-link :to="Routes.TodoLists.children.List.path">
              <Icon icon="mdi:view-list" class="game-thumb-icon" />
            </router-link>
          </div>
          <div class="down-content">
            <h4>View All Lists</h4>
            <span>Browse</span>
            <ul>
              <li><i class="fa fa-star"></i> 4.7</li>
              <li><i class="fa fa-download"></i> 1.2M</li>
            </ul>
          </div>
        </div>

        <div class="game-item hover-glow">
          <div class="thumb">
            <router-link :to="Routes.WeatherForecasts.path">
              <Icon icon="mdi:weather-partly-cloudy" class="game-thumb-icon" />
            </router-link>
          </div>
          <div class="down-content">
            <h4>Weather Forecast</h4>
            <span>Information</span>
            <ul>
              <li><i class="fa fa-star"></i> 4.6</li>
              <li><i class="fa fa-download"></i> 980K</li>
            </ul>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { Routes } from '@/router/constants'
import { getLastLearningPosition, type LastLearningPosition } from '@/features/vocabulary/api/getLastLearningPosition'
import { useRouter } from 'vue-router'

const router = useRouter()
const lastPosition = ref<LastLearningPosition | null>(null)
const isLoadingPosition = ref(true)
const isNavigating = ref(false)

// Mock data
const stats = reactive({
  todoLists: 12,
  todoItems: 45,
  temperature: 22,
  posts: 8
})

// Load last learning position
const loadLastPosition = async () => {
  try {
    isLoadingPosition.value = true
    const position = await getLastLearningPosition()
    lastPosition.value = position
  } catch (error) {
    console.warn('Failed to load last learning position:', error)
  } finally {
    isLoadingPosition.value = false
  }
}

// Continue learning from last position
const continueLearning = async () => {
  if (lastPosition.value && !isNavigating.value) {
    isNavigating.value = true

    // Small delay for smooth animation (button feedback)
    await new Promise(resolve => setTimeout(resolve, 200))

    // Navigate to vocabulary learning with position info
    await router.push({
      name: 'VocabularyLearningWords',
      query: {
        categoryId: lastPosition.value.categoryId,
        wordId: lastPosition.value.wordId,
        groupIndex: lastPosition.value.groupIndex,
        exampleIndex: lastPosition.value.lastExampleIndex
      }
    })
  }
}

// Animation on mount
onMounted(async () => {
  await loadLastPosition()
  
  // Add staggered animations to game items
  const gameItems = document.querySelectorAll('.game-item')
  gameItems.forEach((item, index) => {
    (item as HTMLElement).style.animationDelay = `${index * 0.1}s`
  })
})
</script>

<style scoped>
/* Cyborg Dashboard Styles */
.cyborg-dashboard {
  display: flex;
  flex-direction: column;
  gap: 60px;
}

/* Main Banner */
.main-banner {
  padding: 60px;
  position: relative;
  overflow: hidden;
}

.main-banner::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: linear-gradient(45deg, rgba(236, 96, 144, 0.1) 0%, rgba(133, 82, 244, 0.1) 100%);
  z-index: -1;
}

.banner-content h6 {
  font-size: 15px;
  color: var(--accent-pink);
  font-weight: 400;
  margin-bottom: 15px;
  text-transform: uppercase;
  letter-spacing: 1px;
}

.banner-content h4 {
  font-size: 36px;
  font-weight: 700;
  line-height: 54px;
  margin-bottom: 25px;
}

.banner-content h4 em {
  font-style: normal;
  color: var(--accent-pink);
}

.banner-content p {
  color: var(--text-secondary);
  line-height: 28px;
  margin-bottom: 30px;
  font-size: 16px;
}

.main-button {
  margin-top: 30px;
  display: flex;
  gap: 15px;
  flex-wrap: wrap;
}

.continue-btn {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%) !important;
  position: relative;
  overflow: hidden;
  padding: 0 !important;
  min-height: 80px;
  transition: all 0.3s ease;
  border: 2px solid rgba(255, 255, 255, 0.2);
}

.continue-btn:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

.continue-btn:not(:disabled):hover {
  transform: translateY(-3px);
  box-shadow: 0 10px 30px rgba(102, 126, 234, 0.4);
  border-color: rgba(255, 255, 255, 0.4);
}

.btn-content {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 16px 24px;
  position: relative;
  z-index: 2;
}

.btn-content .icon {
  font-size: 32px;
  color: rgba(255, 255, 255, 0.9);
  transition: transform 0.3s ease;
}

.btn-content .icon.spinning {
  animation: spin 1s linear infinite;
}

.btn-text {
  display: flex;
  flex-direction: column;
  gap: 4px;
  text-align: left;
  flex: 1;
}

.primary-text {
  font-size: 16px;
  font-weight: 600;
  color: white;
  display: block;
}

.secondary-text {
  font-size: 13px;
  color: rgba(255, 255, 255, 0.7);
  display: flex;
  align-items: center;
  gap: 8px;
}

.btn-shine {
  position: absolute;
  top: 0;
  left: -100%;
  width: 100%;
  height: 100%;
  background: linear-gradient(90deg, transparent, rgba(255,255,255,0.3), transparent);
  transition: left 0.6s;
}

.continue-btn:not(:disabled):hover .btn-shine {
  left: 100%;
}

/* Skeleton loading styles */
.skeleton-btn {
  background: linear-gradient(135deg, rgba(102, 126, 234, 0.2) 0%, rgba(118, 75, 162, 0.2) 100%);
  border: 2px solid rgba(255, 255, 255, 0.1);
  border-radius: 12px;
  padding: 16px 24px;
  display: flex;
  align-items: center;
  gap: 16px;
  min-height: 80px;
  position: relative;
  overflow: hidden;
}

.skeleton-btn::before {
  content: '';
  position: absolute;
  top: 0;
  left: -100%;
  width: 100%;
  height: 100%;
  background: linear-gradient(90deg, transparent, rgba(255,255,255,0.1), transparent);
  animation: skeleton-wave 2s ease-in-out infinite;
}

.skeleton-icon {
  width: 32px;
  height: 32px;
  border-radius: 8px;
  background: linear-gradient(90deg, rgba(255,255,255,0.1) 0%, rgba(255,255,255,0.15) 50%, rgba(255,255,255,0.1) 100%);
  background-size: 200% 100%;
  position: relative;
  z-index: 1;
}

.skeleton-icon.pulse-slow {
  animation: shimmer 2s ease-in-out infinite;
}

.skeleton-text {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 8px;
  position: relative;
  z-index: 1;
}

.skeleton-line {
  height: 16px;
  border-radius: 4px;
  background: linear-gradient(90deg, rgba(255,255,255,0.1) 0%, rgba(255,255,255,0.15) 50%, rgba(255,255,255,0.1) 100%);
  background-size: 200% 100%;
}

.skeleton-line.pulse-delay-1 {
  width: 70%;
  animation: shimmer 2s ease-in-out 0.1s infinite;
}

.skeleton-line.pulse-delay-2 {
  width: 50%;
  height: 12px;
  animation: shimmer 2s ease-in-out 0.2s infinite;
}

/* No position placeholder */
.no-position-btn {
  background: linear-gradient(135deg, rgba(102, 126, 234, 0.2) 0%, rgba(118, 75, 162, 0.2) 100%) !important;
  border: 2px dashed rgba(255, 255, 255, 0.2);
  min-height: 80px;
  opacity: 0.6;
  cursor: not-allowed;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 12px;
  font-size: 15px;
  color: rgba(255, 255, 255, 0.6);
}

.no-position-btn .icon {
  font-size: 24px;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

@keyframes shimmer {
  0% { background-position: -200% 0; }
  100% { background-position: 200% 0; }
}

@keyframes skeleton-wave {
  0% { left: -100%; }
  50%, 100% { left: 100%; }
}

.category-badge {
  display: inline-block;
  padding: 2px 8px;
  background: rgba(255, 255, 255, 0.2);
  border-radius: 12px;
  font-size: 12px;
  margin-left: 8px;
}

.banner-image {
  display: flex;
  justify-content: center;
  align-items: center;
}

.gaming-icon {
  width: 200px;
  height: 200px;
  color: var(--accent-pink);
  opacity: 0.3;
  animation: float 3s ease-in-out infinite;
}

@keyframes float {
  0%, 100% { transform: translateY(0px); }
  50% { transform: translateY(-20px); }
}

/* Section Heading */
.section-heading {
  text-align: center;
  margin-bottom: 60px;
}

.section-heading h6 {
  font-size: 15px;
  color: var(--accent-pink);
  font-weight: 400;
  margin-bottom: 15px;
  text-transform: uppercase;
  letter-spacing: 1px;
}

.section-heading h4 {
  font-size: 36px;
  font-weight: 700;
  line-height: 45px;
  margin-bottom: 0;
}

.section-heading h4 em {
  font-style: normal;
  color: var(--accent-pink);
}

/* Gaming Library Stats */
.gaming-library-item .left-image {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 80px;
  height: 80px;
  border-radius: var(--radius-sm);
}

.stat-icon {
  width: 60px;
  height: 60px;
  border-radius: var(--radius-sm);
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
}

.stat-icon.todo-lists {
  background: var(--gradient-primary);
}

.stat-icon.todo-items {
  background: var(--gradient-secondary);
}

.stat-icon.weather {
  background: linear-gradient(105deg, #4facfe 0%, #00f2fe 100%);
}

.stat-icon.posts {
  background: linear-gradient(105deg, #43e97b 0%, #38f9d7 100%);
}

/* Game Items */
.game-item {
  animation: fadeInUp 0.6s ease-out both;
}

.game-item .thumb {
  height: 200px;
  background: var(--gradient-secondary);
  border-radius: var(--radius-md) var(--radius-md) 0 0;
  display: flex;
  align-items: center;
  justify-content: center;
  position: relative;
  overflow: hidden;
}

.game-item .thumb::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: linear-gradient(45deg, rgba(0,0,0,0.3), transparent);
}

.game-thumb-icon {
  width: 80px;
  height: 80px;
  color: white;
  z-index: 1;
  transition: var(--transition-normal);
}

.game-item:hover .game-thumb-icon {
  transform: scale(1.1) rotate(5deg);
}

.game-item .down-content ul {
  display: flex;
  gap: 15px;
  margin-top: 20px;
}

.game-item .down-content ul li {
  display: flex;
  align-items: center;
  gap: 5px;
  color: var(--text-muted);
  font-size: 14px;
}

.game-item .down-content ul li i {
  color: #ffcc00;
}

/* Animations */
@keyframes fadeInUp {
  from {
    opacity: 0;
    transform: translateY(30px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

/* Responsive Design */
@media (max-width: 768px) {
  .main-banner {
    padding: 40px 30px;
  }
  
  .banner-content h4 {
    font-size: 28px;
    line-height: 42px;
  }
  
  .gaming-icon {
    width: 150px;
    height: 150px;
  }
  
  .cyborg-grid-2,
  .cyborg-grid-4 {
    grid-template-columns: 1fr;
  }
  
  .gaming-library-item {
    flex-direction: column;
    text-align: center;
  }
  
  .gaming-library-item .left-image {
    margin-right: 0;
    margin-bottom: 20px;
  }
}
</style>