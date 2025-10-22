<template>
  <div class="word-comparison" v-if="userInput.trim()">
    <div class="comparison-header">
      <h4>
        <i class="mdi mdi-compare"></i>
        Word by Word Check:
      </h4>
    </div>
    
    <div class="word-list">
      <div 
        v-for="(word, index) in comparisonWords" 
        :key="index"
        class="word-item"
        :class="{
          'correct': word.isCorrect,
          'typing': word.isTypingCorrectly,
          'incorrect': !word.isCorrect && !word.isTypingCorrectly && word.userWord && word.correctWord,
          'missing': word.isMissing,
          'extra': word.isExtra
        }"
      >
        <div class="word-display">
          <span class="user-word">{{ word.userWord || '___' }}</span>
          <i v-if="word.isCorrect" class="mdi mdi-check status-icon"></i>
          <i v-else-if="word.isTypingCorrectly" class="mdi mdi-pencil status-icon"></i>
          <i v-else-if="!word.isCorrect && word.userWord && word.correctWord" class="mdi mdi-close status-icon"></i>
          <i v-else-if="word.isMissing" class="mdi mdi-help status-icon"></i>
        </div>
        
        <!-- Only show correct word if user typed wrong (not typing correctly) and has enough characters -->
        <div v-if="!word.isCorrect && !word.isTypingCorrectly && word.userWord && word.correctWord && word.userWord.length >= word.correctWord.length" class="correct-word">
          <i class="mdi mdi-arrow-down"></i>
          {{ word.correctWord }}
        </div>
      </div>
    </div>
    
    <div class="comparison-stats">
      <div class="stat-badge correct-count">
        <i class="mdi mdi-check-circle"></i>
        <span>{{ correctCount }} correct</span>
      </div>
      <div class="stat-badge incorrect-count" v-if="incorrectCount > 0">
        <i class="mdi mdi-close-circle"></i>
        <span>{{ incorrectCount }} incorrect</span>
      </div>
      <div class="stat-badge missing-count" v-if="missingCount > 0">
        <i class="mdi mdi-help-circle"></i>
        <span>{{ missingCount }} missing</span>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

interface WordComparison {
  userWord: string
  correctWord: string
  isCorrect: boolean
  isMissing: boolean
  isExtra: boolean
  isTypingCorrectly?: boolean
}

interface Props {
  userInput: string
  correctAnswer: string
}

const props = defineProps<Props>()

// Normalize text: lowercase, trim, remove extra spaces and punctuation
const normalizeText = (text: string): string => {
  return text
    .toLowerCase()
    .trim()
    .replace(/[.,!?;:]/g, '') // Remove punctuation
    .replace(/\s+/g, ' ') // Replace multiple spaces with single space
}

// Split text into words array
const getWords = (text: string): string[] => {
  return normalizeText(text).split(' ').filter(w => w.length > 0)
}

// Compare user input with correct answer word by word
const comparisonWords = computed((): WordComparison[] => {
  const userWords = getWords(props.userInput)
  const correctWords = getWords(props.correctAnswer)
  const maxLength = Math.max(userWords.length, correctWords.length)
  
  const result: WordComparison[] = []
  
  for (let i = 0; i < maxLength; i++) {
    const userWord = userWords[i] || ''
    const correctWord = correctWords[i] || ''
    
    const isCorrect = !!(userWord && correctWord && userWord === correctWord)
    const isMissing = !!(userWord === '' && correctWord) // User didn't type this word
    const isExtra = !!(userWord && correctWord === '') // User typed extra word
    
    // Check if user is typing correctly (userWord is a prefix of correctWord)
    const isTypingCorrectly = !!(
      userWord && 
      correctWord && 
      !isCorrect && 
      userWord.length < correctWord.length &&
      correctWord.startsWith(userWord)
    )
    
    result.push({
      userWord,
      correctWord,
      isCorrect,
      isMissing,
      isExtra,
      isTypingCorrectly
    })
  }
  
  return result
})

// Statistics
const correctCount = computed(() => {
  return comparisonWords.value.filter(w => w.isCorrect).length
})

const incorrectCount = computed(() => {
  return comparisonWords.value.filter(w => !w.isCorrect && w.userWord && w.correctWord).length
})

const missingCount = computed(() => {
  return comparisonWords.value.filter(w => w.isMissing).length
})
</script>

<style scoped>
.word-comparison {
  background: rgba(255, 255, 255, 0.05);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 15px;
  padding: 1.5rem;
  margin: 1.5rem 0;
  animation: slideIn 0.3s ease-out;
}

@keyframes slideIn {
  from {
    opacity: 0;
    transform: translateY(-10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@keyframes pulse {
  0%, 100% { opacity: 0.6; }
  50% { opacity: 1; }
}

.comparison-header {
  margin-bottom: 1.5rem;
}

.comparison-header h4 {
  color: #74c0fc;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 1.1rem;
}

.word-list {
  display: flex;
  flex-wrap: wrap;
  gap: 1rem;
  margin-bottom: 1.5rem;
}

.word-item {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  padding: 0.75rem 1rem;
  border-radius: 10px;
  border: 2px solid;
  transition: all 0.3s ease;
  animation: fadeIn 0.4s ease-out backwards;
}

@keyframes fadeIn {
  from {
    opacity: 0;
    transform: scale(0.9);
  }
  to {
    opacity: 1;
    transform: scale(1);
  }
}

.word-item:nth-child(1) { animation-delay: 0.05s; }
.word-item:nth-child(2) { animation-delay: 0.1s; }
.word-item:nth-child(3) { animation-delay: 0.15s; }
.word-item:nth-child(4) { animation-delay: 0.2s; }
.word-item:nth-child(5) { animation-delay: 0.25s; }
.word-item:nth-child(n+6) { animation-delay: 0.3s; }

.word-item.correct {
  background: rgba(76, 175, 80, 0.2);
  border-color: rgba(76, 175, 80, 0.5);
}

.word-item.typing {
  background: rgba(116, 192, 252, 0.2);
  border-color: rgba(116, 192, 252, 0.5);
}

.word-item.incorrect {
  background: rgba(244, 67, 54, 0.2);
  border-color: rgba(244, 67, 54, 0.5);
  animation: shake 0.5s ease-out;
}

@keyframes shake {
  0%, 100% { transform: translateX(0); }
  25% { transform: translateX(-5px); }
  75% { transform: translateX(5px); }
}

.word-item.missing {
  background: rgba(255, 193, 7, 0.2);
  border-color: rgba(255, 193, 7, 0.5);
}

.word-item.extra {
  background: rgba(156, 39, 176, 0.2);
  border-color: rgba(156, 39, 176, 0.5);
}

.word-display {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.user-word {
  color: white;
  font-size: 1rem;
  font-weight: 500;
}

.status-icon {
  font-size: 1.2rem;
}

.word-item.correct .status-icon {
  color: #4caf50;
}

.word-item.typing .status-icon {
  color: #74c0fc;
  animation: pulse 1.5s infinite;
}

.word-item.incorrect .status-icon {
  color: #f44336;
}

.word-item.missing .status-icon {
  color: #ffc107;
}

.correct-word {
  display: flex;
  align-items: center;
  gap: 0.25rem;
  color: #4caf50;
  font-size: 0.9rem;
  font-weight: 500;
  padding-top: 0.5rem;
  border-top: 1px solid rgba(255, 255, 255, 0.1);
}

.correct-word i {
  font-size: 0.9rem;
}

.comparison-stats {
  display: flex;
  gap: 1rem;
  flex-wrap: wrap;
  padding-top: 1rem;
  border-top: 1px solid rgba(255, 255, 255, 0.1);
}

.stat-badge {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem 1rem;
  border-radius: 20px;
  font-size: 0.9rem;
  font-weight: 500;
}

.correct-count {
  background: rgba(76, 175, 80, 0.2);
  color: #4caf50;
  border: 1px solid rgba(76, 175, 80, 0.3);
}

.incorrect-count {
  background: rgba(244, 67, 54, 0.2);
  color: #f44336;
  border: 1px solid rgba(244, 67, 54, 0.3);
}

.missing-count {
  background: rgba(255, 193, 7, 0.2);
  color: #ffc107;
  border: 1px solid rgba(255, 193, 7, 0.3);
}

@media (max-width: 768px) {
  .word-list {
    gap: 0.75rem;
  }
  
  .word-item {
    padding: 0.5rem 0.75rem;
  }
  
  .user-word {
    font-size: 0.9rem;
  }
  
  .comparison-stats {
    justify-content: center;
  }
}
</style>
