<template>
  <div class="session-header">
    <button @click="$emit('back')" class="back-btn">
      <i class="mdi mdi-arrow-left"></i>
      Back to Categories
    </button>
    
    <div class="session-info">
      <h2>{{ categoryName }}</h2>
      <div class="session-stats">
        <div class="stat-item">
          <i class="mdi mdi-target"></i>
          <span v-if="sessionType === 'dictation' && selectedGroupIndex !== null">
            Example {{ currentExampleInGroup }} / {{ currentGroupSize }} (Group {{ selectedGroupIndex + 1 }})
          </span>
          <span v-else-if="sessionType === 'dictation'">
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
          <span>{{ formattedTime }}</span>
        </div>
      </div>
    </div>

    <div class="session-controls">
      <select :value="sessionType" class="session-type-select" @change="$emit('change-session-type', $event)">
        <option value="vocabulary">📚 Vocabulary</option>
        <option value="dictation">🎤 Dictation</option>
        <option value="mixed">🎯 Mixed</option>
      </select>
      <button 
        @click="$emit('open-settings')"
        class="speech-settings-btn"
        title="Speech Settings"
      >
        <Icon icon="mdi:cog" class="w-4 h-4" />
        Settings
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { Icon } from '@iconify/vue'

interface Props {
  categoryName: string
  sessionType: 'vocabulary' | 'dictation' | 'mixed'
  selectedGroupIndex: number | null
  currentIndex: number
  currentExampleInGroup: number
  currentGroupSize: number
  currentExampleNumber: number
  totalExamples: number
  totalWords: number
  correctCount: number
  formattedTime: string
}

defineProps<Props>()
defineEmits<{
  back: []
  'change-session-type': [event: Event]
  'open-settings': []
}>()
</script>

<style scoped>
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

@media (max-width: 768px) {
  .session-header {
    flex-direction: column;
    gap: 1rem;
    text-align: center;
  }
  
  .session-stats {
    justify-content: center;
  }
}
</style>
