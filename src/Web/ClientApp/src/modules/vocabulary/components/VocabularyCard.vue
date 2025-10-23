<template>
  <div class="vocabulary-card">
    <div class="card-container">
      <!-- Word Display -->
      <div class="word-section">
        <div class="word-display">
          <h1 class="main-word">{{ word.word }}</h1>
          <div class="phonetic" v-if="word.phonetic">
            {{ word.phonetic }}
          </div>
          <div class="part-of-speech">
            {{ word.partOfSpeech }}
          </div>
        </div>
        
        <!-- Audio Controls -->
        <div class="audio-controls">
          <GlobalSpeechButton
            :text="word.word"
            :instance-id="WORD_AUDIO_ID"
            :show-text="true"
            button-class="audio-btn large"
          />
        </div>
      </div>

      <!-- Definition Section -->
      <div class="definition-section">
          <div class="definition-card">
              <h3>Definition (English)</h3>
              <p class="definition">{{ word.definition }}</p>
              <p class="vietnamese-meaning">{{ word.vietnameseMeaning }}</p>
          </div>
          
          <!-- Meaning Display -->
          <div class="meaning-card">
              <h3>Meaning (Vietnamese)</h3>
              <p class="meaning-display">{{ word.meaning }}</p>
          </div>
      </div>

      <!-- Progress Info -->
      <div class="progress-info" v-if="word.userProgress || word.exampleCompletionPercentage > 0">
        <h4>Your Progress</h4>
        <div class="progress-stats">
          <div class="stat" v-if="word.exampleCount > 0">
            <span class="stat-label">Examples Completed:</span>
            <span class="stat-value">{{ word.completedExampleCount }}/{{ word.exampleCount }}</span>
            <div class="progress-bar-small">
              <div 
                class="progress-fill-small"
                :style="{ width: `${word.exampleCompletionPercentage}%` }"
              ></div>
            </div>
            <span class="stat-percentage">{{ Math.round(word.exampleCompletionPercentage) }}%</span>
          </div>
          <div class="stat" v-if="word.userProgress">
            <span class="stat-label">Accuracy:</span>
            <span class="stat-value">{{ Math.round(word.userProgress.accuracyRate) }}%</span>
          </div>
          <div class="stat" v-if="word.userProgress">
            <span class="stat-label">Studied:</span>
            <span class="stat-value">{{ word.userProgress.studiedTimes }} times</span>
          </div>
          <div class="stat" v-if="word.userProgress?.isMastered">
            <span class="mastery-badge">🏆 Mastered</span>
          </div>
        </div>
      </div>

      <!-- Action Buttons -->
      <div class="action-buttons">
        <button 
          v-if="word.examples && word.examples.length > 0"
          @click="handleLearnExample" 
          class="example-btn"
        >
          <i class="mdi mdi-book-open-variant"></i>
          Learn with Example
        </button>
        <button @click="handleNext" class="next-btn">
          Next Word
          <i class="mdi mdi-arrow-right"></i>
        </button>
      </div>

      <!-- Hint Modal -->
      <div v-if="showHintModal" class="hint-modal" @click="closeHint">
        <div class="hint-content" @click.stop>
          <div class="hint-header">
            <h3>💡 Hint</h3>
            <button @click="closeHint" class="close-btn">
              <i class="mdi mdi-close"></i>
            </button>
          </div>
          <div class="hint-text">
            The word "{{ word.word }}" is a {{ word.partOfSpeech }}.
            <br>
            Think about: {{ getHintText() }}
          </div>
        </div>
      </div>
    </div>


  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, readonly } from 'vue'
import GlobalSpeechButton from '@/components/GlobalSpeechButton.vue'
import type { VocabularyWord } from '../types/vocabulary.types'

interface Props {
  word: VocabularyWord
}

const props = defineProps<Props>()

const emit = defineEmits<{
  next: []
  learnExample: []
}>()

const showHintModal = ref(false)

const handleNext = () => {
  console.log('Next button clicked!')
  console.log('Current word:', props.word.word)
  console.log('Emitting next event...')
  emit('next')
}

const handleLearnExample = () => {
  console.log('Learn Example button clicked!')
  console.log('Current word:', props.word.word)
  console.log('Examples count:', props.word.examples?.length || 0)
  emit('learnExample')
}

// Instance ID for word audio  
const WORD_AUDIO_ID = 'word-audio'

const closeHint = () => {
  showHintModal.value = false
}

const getHintText = (): string => {
  const hints = [
    `It starts with "${props.word.word.charAt(0).toUpperCase()}"`,
    `It has ${props.word.word.length} letters`,
    `Part of speech: ${props.word.partOfSpeech}`
  ]
  return hints[Math.floor(Math.random() * hints.length)]
}
</script>

<style scoped>
.vocabulary-card {
  max-width: 800px;
  margin: 0 auto;
}

.card-container {
  background: linear-gradient(135deg, 
    rgba(255, 255, 255, 0.1) 0%, 
    rgba(255, 255, 255, 0.05) 100%);
  border: 1px solid rgba(255, 255, 255, 0.2);
  border-radius: 20px;
  backdrop-filter: blur(10px);
  padding: 2rem;
  position: relative;
  overflow: hidden;
}

.card-container::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 4px;
  background: linear-gradient(135deg, #e75e8d, #74c0fc);
}

.word-section {
  text-align: center;
  margin-bottom: 2rem;
  padding-bottom: 2rem;
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
}

.main-word {
  font-size: 4rem;
  font-weight: bold;
  color: transparent;
  background: linear-gradient(135deg, #e75e8d, #74c0fc);
  background-clip: text;
  -webkit-background-clip: text;
  margin-bottom: 0.5rem;
  text-shadow: 0 0 30px rgba(231, 94, 141, 0.3);
}

.phonetic {
  font-size: 1.5rem;
  color: #74c0fc;
  font-style: italic;
  margin-bottom: 0.5rem;
}

.part-of-speech {
  font-size: 1rem;
  color: #e75e8d;
  background: rgba(231, 94, 141, 0.2);
  padding: 0.5rem 1rem;
  border-radius: 15px;
  display: inline-block;
}

.audio-controls {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  justify-content: center;
  margin: 1rem auto 0;
}

.audio-btn {
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
  font-size: 1rem;
}

.settings-btn {
  background: rgba(116, 192, 252, 0.3);
  border: 1px solid rgba(116, 192, 252, 0.5);
  border-radius: 50%;
  width: 40px;
  height: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  color: #74c0fc;
  transition: all 0.3s ease;
}

.settings-btn:hover {
  background: rgba(116, 192, 252, 0.5);
  transform: scale(1.1);
}

.audio-btn:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 5px 15px rgba(231, 94, 141, 0.4);
}

.audio-btn:disabled {
  opacity: 0.7;
  cursor: not-allowed;
  animation: pulse 1.5s infinite;
}

.definition-section {
  margin-bottom: 2rem;
  display: flex;
  flex-direction: column;
  gap: 1rem;
  text-align: center;
}

.definition-card, .meaning-card {
  background: rgba(116, 192, 252, 0.1);
  border: 1px solid rgba(116, 192, 252, 0.3);
  border-radius: 15px;
  padding: 1.5rem;
}

.definition-card h3, .meaning-card h3 {
  color: #74c0fc;
  margin-bottom: 1rem;
  font-size: 1.2rem;
}

.definition-card p, .meaning-card p {
  color: white;
  font-size: 1.1rem;
  line-height: 1.6;
}

.definition-card .vietnamese-meaning {
  color: #74c0fc;
  margin-top: 0.5rem;
}

.meaning-card {
  background: rgba(231, 94, 141, 0.1);
  border-color: rgba(231, 94, 141, 0.3);
  text-align: center;
}

.meaning-card h3 {
  color: #e75e8d;
}

.meaning-card .meaning-display {
  font-weight: 500;
  font-size: 2rem; 
}

.progress-info {
  background: rgba(255, 255, 255, 0.05);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 12px;
  padding: 1rem;
  margin-bottom: 2rem;
}

.progress-info h4 {
  color: #74c0fc;
  margin-bottom: 1rem;
}

.progress-stats {
  display: flex;
  justify-content: center;
  gap: 2rem;
  flex-wrap: wrap;
}

.stat {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.2rem;
}

.stat-label {
  color: #b8b8b8;
  font-size: 0.9rem;
}

.stat-value {
  color: white;
  font-weight: bold;
}

.progress-bar-small {
  width: 150px;
  height: 8px;
  background: rgba(255, 255, 255, 0.1);
  border-radius: 10px;
  overflow: hidden;
  margin: 0.5rem 0;
}

.progress-fill-small {
  height: 100%;
  background: linear-gradient(90deg, #e75e8d, #74c0fc);
  border-radius: 10px;
  transition: width 0.3s ease;
  box-shadow: 0 0 10px rgba(231, 94, 141, 0.5);
}

.stat-percentage {
  color: #74c0fc;
  font-size: 0.85rem;
  font-weight: 600;
}

.mastery-badge {
  background: linear-gradient(135deg, #ffd700, #ffb347);
  color: #333;
  padding: 0.5rem 1rem;
  border-radius: 15px;
  font-weight: bold;
  font-size: 0.9rem;
}

.action-buttons {
  display: flex;
  gap: 1rem;
  justify-content: center;
  align-items: center;
  flex-wrap: wrap;
}

.example-btn, .next-btn {
  background: linear-gradient(135deg, #e75e8d, #74c0fc);
  color: white;
  border: none;
  padding: 1rem 2rem;
  border-radius: 25px;
  cursor: pointer;
  font-size: 1.1rem;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  transition: all 0.3s ease;
}

.example-btn {
  background: linear-gradient(135deg, #74c0fc, #667eea);
}

.example-btn:hover, .next-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 10px 25px rgba(231, 94, 141, 0.4);
}

.example-btn:hover {
  box-shadow: 0 10px 25px rgba(116, 192, 252, 0.4);
}

.hint-modal {
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

.hint-content {
  background: linear-gradient(135deg, rgba(26, 26, 46, 0.95) 0%, rgba(22, 33, 62, 0.95) 100%);
  border: 1px solid rgba(116, 192, 252, 0.3);
  border-radius: 15px;
  padding: 2rem;
  max-width: 400px;
  width: 90%;
  backdrop-filter: blur(20px);
}

.hint-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1rem;
}

.hint-header h3 {
  color: #74c0fc;
  margin: 0;
}

.close-btn {
  background: none;
  border: none;
  color: #b8b8b8;
  font-size: 1.5rem;
  cursor: pointer;
  padding: 0.25rem;
  border-radius: 50%;
  transition: all 0.3s ease;
}

.close-btn:hover {
  background: rgba(255, 255, 255, 0.1);
  color: white;
}

.hint-text {
  color: white;
  line-height: 1.6;
}

@media (max-width: 768px) {
  .main-word {
    font-size: 2.5rem;
  }
  
  .answer-options {
    grid-template-columns: 1fr;
  }
  
  .action-buttons {
    flex-direction: column;
    align-items: center;
  }
  
  .progress-stats {
    flex-direction: column;
    gap: 1rem;
  }
}

.settings-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.7);
  z-index: 1999;
  backdrop-filter: blur(5px);
}
</style>