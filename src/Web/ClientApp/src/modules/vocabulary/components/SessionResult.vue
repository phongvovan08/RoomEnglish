<template>
  <div class="session-result-container">
    <div class="result-card">
      <!-- Header -->
      <div class="result-header">
        <div class="trophy-animation">
          <i class="mdi mdi-trophy" :class="getTrophyClass()"></i>
        </div>
        <h1 class="result-title">{{ getResultTitle() }}</h1>
        <div class="result-subtitle">{{ getResultSubtitle() }}</div>
      </div>

      <!-- Main Stats -->
      <div class="main-stats">
        <div class="score-display">
          <div class="score-circle" :class="getScoreClass()">
            <div class="score-number">{{ result.score }}</div>
            <div class="score-label">SCORE</div>
          </div>
        </div>

        <div class="primary-stats">
          <div class="stat-card accuracy" :class="getAccuracyClass()">
            <div class="stat-icon">
              <i class="mdi mdi-target"></i>
            </div>
            <div class="stat-content">
              <div class="stat-value">{{ Math.round(result.accuracyRate) }}%</div>
              <div class="stat-label">Accuracy</div>
            </div>
          </div>

          <div class="stat-card duration">
            <div class="stat-icon">
              <i class="mdi mdi-clock"></i>
            </div>
            <div class="stat-content">
              <div class="stat-value">{{ formatDuration(result.durationMinutes) }}</div>
              <div class="stat-label">Time Spent</div>
            </div>
          </div>

          <div class="stat-card progress">
            <div class="stat-icon">
              <i class="mdi mdi-chart-line"></i>
            </div>
            <div class="stat-content">
              <div class="stat-value">{{ result.correctAnswers }}/{{ result.totalWords }}</div>
              <div class="stat-label">Correct Answers</div>
            </div>
          </div>
        </div>
      </div>

      <!-- Performance Analysis -->
      <div class="performance-analysis">
        <h3>📊 Performance Analysis</h3>
        
        <div class="analysis-grid">
          <!-- Accuracy Meter -->
          <div class="analysis-item">
            <div class="meter-container">
              <div class="meter-title">Accuracy Level</div>
              <div class="accuracy-meter">
                <div class="meter-track">
                  <div 
                    class="meter-fill" 
                    :style="{ width: `${result.accuracyRate}%` }"
                    :class="getAccuracyClass()"
                  ></div>
                </div>
                <div class="meter-labels">
                  <span>0%</span>
                  <span>50%</span>
                  <span>100%</span>
                </div>
              </div>
            </div>
          </div>

          <!-- Speed Analysis -->
          <div class="analysis-item">
            <div class="speed-analysis">
              <div class="speed-title">Learning Speed</div>
              <div class="speed-metric">
                <i class="mdi mdi-speedometer"></i>
                <span>{{ getWordsPerMinute() }} words/min</span>
              </div>
              <div class="speed-feedback">{{ getSpeedFeedback() }}</div>
            </div>
          </div>

          <!-- Achievement Badges -->
          <div class="analysis-item">
            <div class="achievements">
              <div class="achievement-title">Achievements</div>
              <div class="badge-list">
                <div 
                  v-for="badge in getAchievements()" 
                  :key="badge.id"
                  class="achievement-badge"
                  :class="badge.class"
                >
                  <i :class="badge.icon"></i>
                  <span>{{ badge.name }}</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Session Details -->
      <div class="session-details">
        <h3>📝 Session Details</h3>
        
        <div class="details-grid">
          <div class="detail-item">
            <i class="mdi mdi-book-open-variant"></i>
            <div class="detail-content">
              <div class="detail-label">Category</div>
              <div class="detail-value">{{ result.categoryName }}</div>
            </div>
          </div>

          <div class="detail-item">
            <i class="mdi mdi-gamepad-variant"></i>
            <div class="detail-content">
              <div class="detail-label">Session Type</div>
              <div class="detail-value">{{ getSessionTypeLabel() }}</div>
            </div>
          </div>

          <div class="detail-item">
            <i class="mdi mdi-calendar-clock"></i>
            <div class="detail-content">
              <div class="detail-label">Started At</div>
              <div class="detail-value">{{ formatDateTime(result.startedAt) }}</div>
            </div>
          </div>

          <div class="detail-item">
            <i class="mdi mdi-check-all"></i>
            <div class="detail-content">
              <div class="detail-label">Completed At</div>
              <div class="detail-value">{{ formatDateTime(result.completedAt!) }}</div>
            </div>
          </div>
        </div>
      </div>

      <!-- Motivational Message -->
      <div class="motivation-section">
        <div class="motivation-card" :class="getMotivationClass()">
          <div class="motivation-icon">
            <i :class="getMotivationIcon()"></i>
          </div>
          <div class="motivation-content">
            <div class="motivation-title">{{ getMotivationTitle() }}</div>
            <div class="motivation-message">{{ getMotivationMessage() }}</div>
          </div>
        </div>
      </div>

      <!-- Action Buttons -->
      <div class="action-section">
        <button @click="$emit('restart')" class="action-btn restart-btn">
          <i class="mdi mdi-restart"></i>
          <span>Practice Again</span>
        </button>

        <button @click="$emit('new-category')" class="action-btn new-category-btn">
          <i class="mdi mdi-folder-multiple"></i>
          <span>Try Different Category</span>
        </button>

        <button @click="shareResults" class="action-btn share-btn">
          <i class="mdi mdi-share-variant"></i>
          <span>Share Results</span>
        </button>
      </div>

      <!-- Progress Particles (Decoration) -->
      <div class="particles" v-if="showParticles">
        <div 
          v-for="i in 20" 
          :key="i"
          class="particle"
          :style="getParticleStyle(i)"
        ></div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import type { LearningSession } from '../types/vocabulary.types'

interface Props {
  result: LearningSession
}

interface Achievement {
  id: string
  name: string
  icon: string
  class: string
}

const props = defineProps<Props>()

const emit = defineEmits<{
  restart: []
  'new-category': []
}>()

const showParticles = ref(false)

// Computed properties
const isExcellent = computed(() => props.result.accuracyRate >= 90)
const isGood = computed(() => props.result.accuracyRate >= 70)
const isFair = computed(() => props.result.accuracyRate >= 50)

// Methods
const getTrophyClass = (): string => {
  if (isExcellent.value) return 'gold'
  if (isGood.value) return 'silver'
  if (isFair.value) return 'bronze'
  return 'participation'
}

const getResultTitle = (): string => {
  if (isExcellent.value) return '🏆 Outstanding Performance!'
  if (isGood.value) return '🌟 Great Job!'
  if (isFair.value) return '👏 Nice Work!'
  return '💪 Keep Practicing!'
}

const getResultSubtitle = (): string => {
  if (isExcellent.value) return 'You absolutely nailed it!'
  if (isGood.value) return 'You\'re doing really well!'
  if (isFair.value) return 'You\'re making good progress!'
  return 'Every step counts towards mastery!'
}

const getScoreClass = (): string => {
  if (props.result.score >= 800) return 'excellent'
  if (props.result.score >= 600) return 'good'
  if (props.result.score >= 400) return 'fair'
  return 'needs-improvement'
}

const getAccuracyClass = (): string => {
  if (isExcellent.value) return 'excellent'
  if (isGood.value) return 'good'
  if (isFair.value) return 'fair'
  return 'needs-improvement'
}

const formatDuration = (minutes: number): string => {
  if (minutes < 1) return '< 1 min'
  const hours = Math.floor(minutes / 60)
  const mins = minutes % 60
  if (hours > 0) {
    return `${hours}h ${mins}m`
  }
  return `${mins} min`
}

const formatDateTime = (dateTime: string): string => {
  return new Date(dateTime).toLocaleString()
}

const getWordsPerMinute = (): number => {
  if (props.result.durationMinutes === 0) return 0
  return Math.round(props.result.totalWords / props.result.durationMinutes)
}

const getSpeedFeedback = (): string => {
  const wpm = getWordsPerMinute()
  if (wpm >= 5) return 'Lightning fast! ⚡'
  if (wpm >= 3) return 'Good pace! 🚀'
  if (wpm >= 2) return 'Steady progress 🐢'
  return 'Take your time 🌱'
}

const getSessionTypeLabel = (): string => {
  const typeLabels: Record<string, string> = {
    'vocabulary': '📚 Vocabulary Learning',
    'dictation': '🎤 Dictation Practice',
    'mixed': '🎯 Mixed Practice'
  }
  return typeLabels[props.result.sessionType] || props.result.sessionType
}

const getAchievements = (): Achievement[] => {
  const achievements: Achievement[] = []

  if (props.result.accuracyRate === 100) {
    achievements.push({
      id: 'perfect',
      name: 'Perfect Score',
      icon: 'mdi mdi-star',
      class: 'gold'
    })
  }

  if (props.result.accuracyRate >= 90) {
    achievements.push({
      id: 'excellence',
      name: 'Excellence',
      icon: 'mdi mdi-trophy',
      class: 'gold'
    })
  }

  if (props.result.durationMinutes <= 5 && props.result.totalWords >= 10) {
    achievements.push({
      id: 'speed',
      name: 'Speed Demon',
      icon: 'mdi mdi-flash',
      class: 'blue'
    })
  }

  if (props.result.totalWords >= 20) {
    achievements.push({
      id: 'endurance',
      name: 'Endurance',
      icon: 'mdi mdi-dumbbell',
      class: 'green'
    })
  }

  if (achievements.length === 0) {
    achievements.push({
      id: 'participant',
      name: 'Participant',
      icon: 'mdi mdi-medal',
      class: 'participation'
    })
  }

  return achievements
}

const getMotivationClass = (): string => {
  if (isExcellent.value) return 'excellent'
  if (isGood.value) return 'good'
  return 'encouraging'
}

const getMotivationIcon = (): string => {
  if (isExcellent.value) return 'mdi mdi-emoticon-excited'
  if (isGood.value) return 'mdi mdi-emoticon-happy'
  return 'mdi mdi-emoticon-wink'
}

const getMotivationTitle = (): string => {
  if (isExcellent.value) return 'Incredible Achievement!'
  if (isGood.value) return 'Excellent Progress!'
  return 'Keep Going!'
}

const getMotivationMessage = (): string => {
  if (isExcellent.value) return 'You\'ve mastered this category! Your dedication is paying off beautifully.'
  if (isGood.value) return 'You\'re well on your way to mastery! Keep up the fantastic work.'
  return 'Every practice session makes you stronger. You\'re building solid foundations!'
}

const shareResults = () => {
  const text = `🎯 I just completed an English learning session!\n📊 Score: ${props.result.score}\n🎯 Accuracy: ${Math.round(props.result.accuracyRate)}%\n⏱️ Time: ${formatDuration(props.result.durationMinutes)}\n\n#EnglishLearning #VocabularyMastery`
  
  if (navigator.share) {
    navigator.share({
      title: 'My English Learning Results',
      text: text
    })
  } else {
    navigator.clipboard.writeText(text)
    // Could show a toast notification here
    alert('Results copied to clipboard!')
  }
}

const getParticleStyle = (index: number) => {
  const delay = index * 100
  const duration = 2000 + Math.random() * 1000
  const left = Math.random() * 100
  
  return {
    left: `${left}%`,
    animationDelay: `${delay}ms`,
    animationDuration: `${duration}ms`
  }
}

onMounted(() => {
  // Show particles for excellent results
  if (isExcellent.value) {
    showParticles.value = true
  }
})
</script>

<style scoped>
.session-result-container {
  min-height: 100vh;
  background: linear-gradient(135deg, #1a1a2e 0%, #16213e 50%, #0f3460 100%);
  padding: 2rem;
  display: flex;
  align-items: center;
  justify-content: center;
}

.result-card {
  background: linear-gradient(135deg, 
    rgba(255, 255, 255, 0.1) 0%, 
    rgba(255, 255, 255, 0.05) 100%);
  border: 1px solid rgba(255, 255, 255, 0.2);
  border-radius: 25px;
  backdrop-filter: blur(15px);
  padding: 3rem;
  max-width: 900px;
  width: 100%;
  position: relative;
  overflow: hidden;
}

.result-card::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 6px;
  background: linear-gradient(135deg, #e75e8d, #74c0fc, #e75e8d);
}

.result-header {
  text-align: center;
  margin-bottom: 3rem;
}

.trophy-animation {
  font-size: 5rem;
  margin-bottom: 1rem;
  animation: bounce 2s infinite;
}

.trophy-animation .mdi-trophy.gold {
  color: #ffd700;
  filter: drop-shadow(0 0 20px rgba(255, 215, 0, 0.6));
}

.trophy-animation .mdi-trophy.silver {
  color: #c0c0c0;
  filter: drop-shadow(0 0 20px rgba(192, 192, 192, 0.6));
}

.trophy-animation .mdi-trophy.bronze {
  color: #cd7f32;
  filter: drop-shadow(0 0 20px rgba(205, 127, 50, 0.6));
}

.trophy-animation .mdi-trophy.participation {
  color: #74c0fc;
  filter: drop-shadow(0 0 20px rgba(116, 192, 252, 0.6));
}

.result-title {
  font-size: 3rem;
  font-weight: bold;
  color: transparent;
  background: linear-gradient(135deg, #e75e8d, #74c0fc);
  background-clip: text;
  -webkit-background-clip: text;
  margin-bottom: 0.5rem;
  text-shadow: 0 0 30px rgba(231, 94, 141, 0.3);
}

.result-subtitle {
  color: #b8b8b8;
  font-size: 1.3rem;
  font-style: italic;
}

.main-stats {
  display: grid;
  grid-template-columns: auto 1fr;
  gap: 3rem;
  align-items: center;
  margin-bottom: 3rem;
}

.score-display {
  display: flex;
  justify-content: center;
}

.score-circle {
  width: 180px;
  height: 180px;
  border-radius: 50%;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  position: relative;
  border: 4px solid;
  animation: scoreGlow 3s ease-in-out infinite alternate;
}

.score-circle.excellent {
  border-color: #4caf50;
  background: radial-gradient(circle, rgba(76, 175, 80, 0.2), transparent);
  box-shadow: 0 0 30px rgba(76, 175, 80, 0.4);
}

.score-circle.good {
  border-color: #2196f3;
  background: radial-gradient(circle, rgba(33, 150, 243, 0.2), transparent);
  box-shadow: 0 0 30px rgba(33, 150, 243, 0.4);
}

.score-circle.fair {
  border-color: #ff9800;
  background: radial-gradient(circle, rgba(255, 152, 0, 0.2), transparent);
  box-shadow: 0 0 30px rgba(255, 152, 0, 0.4);
}

.score-circle.needs-improvement {
  border-color: #f44336;
  background: radial-gradient(circle, rgba(244, 67, 54, 0.2), transparent);
  box-shadow: 0 0 30px rgba(244, 67, 54, 0.4);
}

.score-number {
  font-size: 3rem;
  font-weight: bold;
  color: white;
}

.score-label {
  font-size: 1rem;
  color: #b8b8b8;
  margin-top: 0.5rem;
}

.primary-stats {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1.5rem;
}

.stat-card {
  background: rgba(255, 255, 255, 0.05);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 15px;
  padding: 1.5rem;
  display: flex;
  align-items: center;
  gap: 1rem;
  transition: all 0.3s ease;
}

.stat-card:hover {
  transform: translateY(-5px);
  background: rgba(255, 255, 255, 0.08);
}

.stat-card.excellent {
  border-color: rgba(76, 175, 80, 0.3);
  box-shadow: 0 0 15px rgba(76, 175, 80, 0.2);
}

.stat-card.good {
  border-color: rgba(33, 150, 243, 0.3);
  box-shadow: 0 0 15px rgba(33, 150, 243, 0.2);
}

.stat-card.fair {
  border-color: rgba(255, 152, 0, 0.3);
  box-shadow: 0 0 15px rgba(255, 152, 0, 0.2);
}

.stat-icon {
  font-size: 2.5rem;
  color: #74c0fc;
}

.stat-content {
  flex: 1;
}

.stat-value {
  font-size: 2rem;
  font-weight: bold;
  color: white;
}

.stat-label {
  color: #b8b8b8;
  font-size: 0.9rem;
  margin-top: 0.25rem;
}

.performance-analysis, .session-details {
  margin-bottom: 3rem;
}

.performance-analysis h3, .session-details h3 {
  color: #e75e8d;
  font-size: 1.8rem;
  margin-bottom: 2rem;
  text-align: center;
}

.analysis-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  gap: 2rem;
}

.analysis-item {
  background: rgba(255, 255, 255, 0.05);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 15px;
  padding: 2rem;
}

.meter-container, .speed-analysis, .achievements {
  text-align: center;
}

.meter-title, .speed-title, .achievement-title {
  color: white;
  font-weight: bold;
  margin-bottom: 1.5rem;
  font-size: 1.1rem;
}

.accuracy-meter {
  margin-bottom: 1rem;
}

.meter-track {
  height: 12px;
  background: rgba(255, 255, 255, 0.1);
  border-radius: 6px;
  overflow: hidden;
  margin-bottom: 0.5rem;
}

.meter-fill {
  height: 100%;
  border-radius: 6px;
  transition: width 1s ease;
}

.meter-fill.excellent {
  background: linear-gradient(135deg, #4caf50, #66bb6a);
}

.meter-fill.good {
  background: linear-gradient(135deg, #2196f3, #42a5f5);
}

.meter-fill.fair {
  background: linear-gradient(135deg, #ff9800, #ffb74d);
}

.meter-fill.needs-improvement {
  background: linear-gradient(135deg, #f44336, #ef5350);
}

.meter-labels {
  display: flex;
  justify-content: space-between;
  color: #b8b8b8;
  font-size: 0.8rem;
}

.speed-metric {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  color: #74c0fc;
  font-size: 1.2rem;
  font-weight: bold;
  margin-bottom: 1rem;
}

.speed-feedback {
  color: #b8b8b8;
  font-style: italic;
}

.badge-list {
  display: flex;
  flex-wrap: wrap;
  gap: 1rem;
  justify-content: center;
}

.achievement-badge {
  background: rgba(255, 255, 255, 0.1);
  border: 1px solid rgba(255, 255, 255, 0.2);
  border-radius: 20px;
  padding: 0.75rem 1rem;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  color: white;
  font-size: 0.9rem;
  transition: all 0.3s ease;
}

.achievement-badge:hover {
  transform: scale(1.05);
}

.achievement-badge.gold {
  border-color: rgba(255, 215, 0, 0.5);
  background: rgba(255, 215, 0, 0.1);
  color: #ffd700;
}

.achievement-badge.silver {
  border-color: rgba(192, 192, 192, 0.5);
  background: rgba(192, 192, 192, 0.1);
  color: #c0c0c0;
}

.achievement-badge.bronze {
  border-color: rgba(205, 127, 50, 0.5);
  background: rgba(205, 127, 50, 0.1);
  color: #cd7f32;
}

.achievement-badge.blue {
  border-color: rgba(33, 150, 243, 0.5);
  background: rgba(33, 150, 243, 0.1);
  color: #2196f3;
}

.achievement-badge.green {
  border-color: rgba(76, 175, 80, 0.5);
  background: rgba(76, 175, 80, 0.1);
  color: #4caf50;
}

.details-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 1.5rem;
}

.detail-item {
  background: rgba(255, 255, 255, 0.05);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 12px;
  padding: 1.5rem;
  display: flex;
  align-items: center;
  gap: 1rem;
}

.detail-item i {
  font-size: 2rem;
  color: #74c0fc;
}

.detail-content {
  flex: 1;
}

.detail-label {
  color: #b8b8b8;
  font-size: 0.9rem;
  margin-bottom: 0.25rem;
}

.detail-value {
  color: white;
  font-weight: bold;
  font-size: 1rem;
}

.motivation-section {
  margin-bottom: 3rem;
}

.motivation-card {
  background: rgba(255, 255, 255, 0.05);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 20px;
  padding: 2rem;
  display: flex;
  align-items: center;
  gap: 2rem;
  text-align: center;
}

.motivation-card.excellent {
  border-color: rgba(76, 175, 80, 0.3);
  background: rgba(76, 175, 80, 0.05);
}

.motivation-card.good {
  border-color: rgba(33, 150, 243, 0.3);
  background: rgba(33, 150, 243, 0.05);
}

.motivation-card.encouraging {
  border-color: rgba(231, 94, 141, 0.3);
  background: rgba(231, 94, 141, 0.05);
}

.motivation-icon {
  font-size: 4rem;
  color: #e75e8d;
}

.motivation-content {
  flex: 1;
}

.motivation-title {
  color: white;
  font-size: 1.5rem;
  font-weight: bold;
  margin-bottom: 0.5rem;
}

.motivation-message {
  color: #b8b8b8;
  font-size: 1rem;
  line-height: 1.5;
}

.action-section {
  display: flex;
  justify-content: center;
  gap: 1.5rem;
  flex-wrap: wrap;
}

.action-btn {
  background: linear-gradient(135deg, #e75e8d, #74c0fc);
  color: white;
  border: none;
  padding: 1rem 2rem;
  border-radius: 25px;
  cursor: pointer;
  font-size: 1rem;
  display: flex;
  align-items: center;
  gap: 0.75rem;
  transition: all 0.3s ease;
  font-weight: 500;
}

.action-btn:hover {
  transform: translateY(-3px);
  box-shadow: 0 10px 25px rgba(231, 94, 141, 0.4);
}

.restart-btn {
  background: linear-gradient(135deg, #4caf50, #66bb6a);
}

.restart-btn:hover {
  box-shadow: 0 10px 25px rgba(76, 175, 80, 0.4);
}

.new-category-btn {
  background: linear-gradient(135deg, #ff9800, #ffb74d);
}

.new-category-btn:hover {
  box-shadow: 0 10px 25px rgba(255, 152, 0, 0.4);
}

.particles {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  pointer-events: none;
  overflow: hidden;
}

.particle {
  position: absolute;
  width: 6px;
  height: 6px;
  background: linear-gradient(135deg, #e75e8d, #74c0fc);
  border-radius: 50%;
  animation: fall linear infinite;
}

@keyframes bounce {
  0%, 20%, 50%, 80%, 100% { transform: translateY(0); }
  40% { transform: translateY(-20px); }
  60% { transform: translateY(-10px); }
}

@keyframes scoreGlow {
  from { box-shadow: 0 0 20px rgba(255, 255, 255, 0.2); }
  to { box-shadow: 0 0 40px rgba(255, 255, 255, 0.4); }
}

@keyframes fall {
  0% { transform: translateY(-100vh) rotate(0deg); opacity: 1; }
  100% { transform: translateY(100vh) rotate(360deg); opacity: 0; }
}

@media (max-width: 768px) {
  .result-card {
    padding: 2rem 1rem;
  }
  
  .result-title {
    font-size: 2rem;
  }
  
  .main-stats {
    grid-template-columns: 1fr;
    gap: 2rem;
  }
  
  .score-circle {
    width: 140px;
    height: 140px;
  }
  
  .score-number {
    font-size: 2.5rem;
  }
  
  .primary-stats {
    grid-template-columns: 1fr;
  }
  
  .analysis-grid, .details-grid {
    grid-template-columns: 1fr;
  }
  
  .motivation-card {
    flex-direction: column;
    text-align: center;
  }
  
  .action-section {
    flex-direction: column;
    align-items: center;
  }
  
  .action-btn {
    width: 100%;
    max-width: 300px;
    justify-content: center;
  }
}
</style>