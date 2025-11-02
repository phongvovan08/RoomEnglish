<template>
  <div class="modal-overlay" @click="$emit('complete')">
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
        <button @click="$emit('complete')" class="complete-btn">
          View Results
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
interface Props {
  correctCount: number
  accuracy: number
  elapsedTime: number
  finalScore: number
  formatTime: (seconds: number) => string
}

defineProps<Props>()
defineEmits<{
  complete: []
}>()
</script>

<style scoped>
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

@media (max-width: 768px) {
  .final-stats {
    grid-template-columns: 1fr;
  }
}
</style>
