<template>
  <div class="result-section">
    <div class="result-header" :class="{ 'correct': result.isCorrect, 'incorrect': !result.isCorrect }">
      <div class="result-icon">
        <i class="mdi" :class="result.isCorrect ? 'mdi-check-circle' : 'mdi-close-circle'"></i>
      </div>
      <div class="result-message">
        <h3>{{ result.isCorrect ? 'Excellent!' : 'Good try!' }}</h3>
        <div class="accuracy">
          Accuracy: {{ result.accuracyPercentage }}%
        </div>
      </div>
    </div>

    <div class="sentence-section" v-if="sentence">
      <div class="sentence-card">
        <h4>📝 Sentence:</h4>
        <p class="sentence-content">{{ sentence }}</p>
        <p class="translation-content">{{ translation }}</p>
        <p class="grammar-text">{{ grammar }}</p>
      </div>
    </div>

    <div class="performance-stats">
      <div class="stat-item">
        <i class="mdi mdi-clock"></i>
        <span>Time: {{ formatTime(result.timeTakenSeconds) }}</span>
      </div>
    </div>

    <button @click="$emit('next')" class="next-btn">
      <span>Next Exercise</span>
      <i class="mdi mdi-arrow-right"></i>
    </button>
  </div>
</template>

<script setup lang="ts">
import type { DictationResult } from '../../../types/vocabulary.types'

interface Props {
  result: DictationResult
  sentence?: string
  translation?: string
  grammar?: string
}

defineProps<Props>()

defineEmits<{
  replay: []
  next: []
}>()

const formatTime = (seconds: number): string => {
  const mins = Math.floor(seconds / 60)
  const secs = seconds % 60
  return `${mins}:${secs.toString().padStart(2, '0')}`
}
</script>

<style scoped>
.result-section {
  text-align: center;
}

.result-header {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 1.5rem;
  padding: 1.5rem;
  border-radius: 15px;
  margin-bottom: 2rem;
}

.result-header.correct {
  background: rgba(76, 175, 80, 0.2);
  border: 1px solid rgba(76, 175, 80, 0.5);
}

.result-header.incorrect {
  background: rgba(255, 193, 7, 0.2);
  border: 1px solid rgba(255, 193, 7, 0.5);
}

.result-icon {
  font-size: 3rem;
}

.result-header.correct .result-icon {
  color: #4caf50;
}

.result-header.incorrect .result-icon {
  color: #ffc107;
}

.result-message h3 {
  color: white;
  margin-bottom: 0.5rem;
}

.accuracy {
  font-size: 1.2rem;
  font-weight: bold;
}

.result-header.correct .accuracy {
  color: #4caf50;
}

.result-header.incorrect .accuracy {
  color: #ffc107;
}

.comparison-section {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  gap: 2rem;
  margin-bottom: 2rem;
}

.your-answer, .correct-answer {
  background: rgba(255, 255, 255, 0.05);
  border-radius: 15px;
  padding: 1.5rem;
  border: 1px solid rgba(255, 255, 255, 0.1);
}

.your-answer h4, .correct-answer h4 {
  color: #74c0fc;
  margin-bottom: 1rem;
  font-size: 1.1rem;
}

.answer-text {
  background: rgba(255, 255, 255, 0.1);
  border-radius: 10px;
  padding: 1rem;
  font-size: 1rem;
  line-height: 1.5;
  margin-bottom: 1rem;
  border: 1px solid rgba(255, 255, 255, 0.2);
}

.user-text {
  color: #fff;
}

.correct-text {
  color: #4caf50;
  border-color: rgba(76, 175, 80, 0.3);
}

.replay-btn {
  background: rgba(116, 192, 252, 0.2);
  color: #74c0fc;
  border: 1px solid rgba(116, 192, 252, 0.5);
  border-radius: 20px;
  padding: 0.5rem 1rem;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  transition: all 0.3s ease;
  margin: 0 auto;
}

.replay-btn:hover {
  background: rgba(116, 192, 252, 0.3);
}

.sentence-section {
  margin-bottom: 2rem;
}

.sentence-card {
  background: linear-gradient(135deg, rgba(116, 192, 252, 0.2), rgba(116, 192, 252, 0.1));
  border: 1px solid rgba(116, 192, 252, 0.3);
  border-radius: 15px;
  padding: 1.5rem;
  box-shadow: 0 6px 12px rgba(0, 0, 0, 0.15);
}

.sentence-card h4 {
  color: #e75e8d;
  margin-bottom: 1rem;
  font-size: 1.1rem;
}
.sentence-card p{
  margin-bottom: 1rem;
  font-size: 1.2rem;
}

.sentence-content {
  color: white;
  font-size: 1.3rem;
  line-height: 1.6;
  font-weight: 500;
  text-align: center;
}

.info-sections {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  gap: 1.5rem;
  margin-bottom: 2rem;
}

.translation-section,
.grammar-section {
  height: 100%;
}

.translation-card,
.grammar-card {
  height: 100%;
  border-radius: 15px;
  padding: 1.5rem;
  box-shadow: 0 6px 12px rgba(0, 0, 0, 0.15);
}

.translation-card {
  background: linear-gradient(135deg, rgba(116, 192, 252, 0.2), rgba(116, 192, 252, 0.1));
  border: 1px solid rgba(116, 192, 252, 0.3);
}

.translation-card h4 {
  color: #74c0fc;
  margin-bottom: 1rem;
  font-size: 1.1rem;
}

.translation-content {
  color: white;
  font-size: 1.1rem;
  line-height: 1.6;
  font-style: italic;
}

.grammar-card {
  background: linear-gradient(135deg, rgba(255, 193, 7, 0.2), rgba(255, 193, 7, 0.1));
  border: 1px solid rgba(255, 193, 7, 0.3);
}

.grammar-card h4 {
  color: #ffc107;
  margin-bottom: 1rem;
  font-size: 1.1rem;
}

.grammar-text {
  color: #ffc107;
  font-size: 1rem;
  line-height: 1.6;
  padding-top: 1rem;
  border-top: 2px solid rgba(255, 193, 7, 0.5);
}

.performance-stats {
  display: flex;
  justify-content: center;
  gap: 2rem;
  margin-bottom: 2rem;
  flex-wrap: wrap;
}

.stat-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  color: #b8b8b8;
  background: rgba(255, 255, 255, 0.05);
  padding: 0.75rem 1rem;
  border-radius: 20px;
}

.stat-item i {
  color: #74c0fc;
}

.next-btn {
  background: linear-gradient(135deg, #e75e8d, #74c0fc);
  color: white;
  border: none;
  padding: 1rem 2rem;
  border-radius: 25px;
  cursor: pointer;
  font-size: 1.1rem;
  display: flex;
  align-items: center;
  gap: 0.75rem;
  margin: 0 auto;
  transition: all 0.3s ease;
}

.next-btn:hover {
  transform: translateY(-3px);
  box-shadow: 0 10px 25px rgba(231, 94, 141, 0.4);
}

@media (max-width: 768px) {
  .comparison-section {
    grid-template-columns: 1fr;
  }
  
  .info-sections {
    grid-template-columns: 1fr;
  }
  
  .performance-stats {
    flex-direction: column;
    align-items: center;
  }
}
</style>
