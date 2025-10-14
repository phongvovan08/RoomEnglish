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
        
        <!-- Audio Button -->
        <button 
          v-if="word.audioUrl" 
          @click="playAudio"
          :disabled="isPlayingAudio"
          class="audio-btn"
        >
          <i class="mdi" :class="isPlayingAudio ? 'mdi-volume-high' : 'mdi-volume-high'"></i>
          {{ isPlayingAudio ? 'Playing...' : 'Listen' }}
        </button>
      </div>

      <!-- Definition Section -->
      <div class="definition-section">
        <div class="definition-card">
          <h3>Definition</h3>
          <p>{{ word.definition }}</p>
        </div>
      </div>

      <!-- Examples Section -->
      <div class="examples-section" v-if="word.examples.length">
        <h3>Examples</h3>
        <div class="examples-list">
          <div 
            v-for="example in word.examples" 
            :key="example.id"
            class="example-item"
          >
            <div class="example-sentence">
              <span class="sentence">{{ example.sentence }}</span>
              <button 
                v-if="example.audioUrl"
                @click="playExampleAudio(example.audioUrl!)"
                class="example-audio-btn"
              >
                <i class="mdi mdi-play"></i>
              </button>
            </div>
            <div class="example-translation">{{ example.translation }}</div>
          </div>
        </div>
      </div>

      <!-- Answer Section -->
      <div class="answer-section" v-if="!showAnswer">
        <div class="question-prompt">
          <h3>What does "{{ word.word }}" mean?</h3>
          <div class="difficulty-indicator">
            <span class="difficulty-label">Difficulty:</span>
            <div class="difficulty-stars">
              <i 
                v-for="n in 3" 
                :key="n"
                class="mdi mdi-star"
                :class="{ 'active': n <= word.difficultyLevel }"
              ></i>
            </div>
          </div>
        </div>

        <div class="answer-options">
          <button 
            v-for="option in answerOptions" 
            :key="option.id"
            @click="selectAnswer(option)"
            class="answer-option"
            :class="{ 'selected': selectedOption?.id === option.id }"
          >
            {{ option.text }}
          </button>
        </div>

        <div class="action-buttons">
          <button 
            @click="submitAnswer" 
            :disabled="!selectedOption"
            class="submit-btn"
          >
            Submit Answer
          </button>
          <button @click="showHint" class="hint-btn">
            <i class="mdi mdi-lightbulb"></i>
            Hint
          </button>
        </div>
      </div>

      <!-- Result Section -->
      <div class="result-section" v-if="showAnswer">
        <div class="result-feedback" :class="{ 'correct': isCorrectAnswer, 'incorrect': !isCorrectAnswer }">
          <div class="result-icon">
            <i class="mdi" :class="isCorrectAnswer ? 'mdi-check-circle' : 'mdi-close-circle'"></i>
          </div>
          <div class="result-message">
            <h3>{{ isCorrectAnswer ? 'Excellent!' : 'Not quite right' }}</h3>
            <p>{{ isCorrectAnswer ? 'You got it correct!' : 'The correct answer is:' }}</p>
          </div>
        </div>

        <div class="correct-answer">
          <div class="meaning-display">
            <strong>{{ word.meaning }}</strong>
          </div>
        </div>

        <div class="progress-info" v-if="word.userProgress">
          <h4>Your Progress</h4>
          <div class="progress-stats">
            <div class="stat">
              <span class="stat-label">Accuracy:</span>
              <span class="stat-value">{{ Math.round(word.userProgress.accuracyRate) }}%</span>
            </div>
            <div class="stat">
              <span class="stat-label">Studied:</span>
              <span class="stat-value">{{ word.userProgress.studiedTimes }} times</span>
            </div>
            <div class="stat" v-if="word.userProgress.isMastered">
              <span class="mastery-badge">🏆 Mastered</span>
            </div>
          </div>
        </div>

        <button @click="$emit('next')" class="next-btn">
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
import { ref, computed, onMounted } from 'vue'
import type { VocabularyWord } from '../types/vocabulary.types'

interface Props {
  word: VocabularyWord
  showAnswer: boolean
}

interface AnswerOption {
  id: number
  text: string
  isCorrect: boolean
}

const props = defineProps<Props>()

const emit = defineEmits<{
  answer: [isCorrect: boolean]
  next: []
  'play-audio': [url: string]
}>()

const selectedOption = ref<AnswerOption | null>(null)
const isCorrectAnswer = ref(false)
const showHintModal = ref(false)
const isPlayingAudio = ref(false)
const answerOptions = ref<AnswerOption[]>([])

// Generate answer options
const generateAnswerOptions = () => {
  // Create correct answer option
  const correctOption: AnswerOption = {
    id: 1,
    text: props.word.meaning,
    isCorrect: true
  }

  // Generate fake options (in real app, these would come from other words)
  const fakeOptions: AnswerOption[] = [
    { id: 2, text: "to run quickly", isCorrect: false },
    { id: 3, text: "a large building", isCorrect: false },
    { id: 4, text: "something very cold", isCorrect: false }
  ]

  // Combine and shuffle
  const allOptions = [correctOption, ...fakeOptions.slice(0, 3)]
  answerOptions.value = allOptions.sort(() => Math.random() - 0.5)
}

const selectAnswer = (option: AnswerOption) => {
  selectedOption.value = option
}

const submitAnswer = () => {
  if (!selectedOption.value) return
  
  isCorrectAnswer.value = selectedOption.value.isCorrect
  emit('answer', isCorrectAnswer.value)
}

const showHint = () => {
  showHintModal.value = true
}

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

const playAudio = async () => {
  if (!props.word.audioUrl || isPlayingAudio.value) return
  
  try {
    isPlayingAudio.value = true
    emit('play-audio', props.word.audioUrl)
    
    // Simulate audio duration (in real app, this would be actual audio duration)
    setTimeout(() => {
      isPlayingAudio.value = false
    }, 2000)
  } catch (error) {
    console.error('Failed to play audio:', error)
    isPlayingAudio.value = false
  }
}

const playExampleAudio = async (audioUrl: string) => {
  emit('play-audio', audioUrl)
}

onMounted(() => {
  generateAnswerOptions()
})
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
  margin: 1rem auto 0;
  transition: all 0.3s ease;
  font-size: 1rem;
}

.audio-btn:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 5px 15px rgba(231, 94, 141, 0.4);
}

.audio-btn:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

.definition-section {
  margin-bottom: 2rem;
}

.definition-card {
  background: rgba(116, 192, 252, 0.1);
  border: 1px solid rgba(116, 192, 252, 0.3);
  border-radius: 15px;
  padding: 1.5rem;
}

.definition-card h3 {
  color: #74c0fc;
  margin-bottom: 1rem;
  font-size: 1.2rem;
}

.definition-card p {
  color: white;
  font-size: 1.1rem;
  line-height: 1.6;
}

.examples-section {
  margin-bottom: 2rem;
}

.examples-section h3 {
  color: #e75e8d;
  margin-bottom: 1rem;
  font-size: 1.2rem;
}

.examples-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.example-item {
  background: rgba(231, 94, 141, 0.1);
  border: 1px solid rgba(231, 94, 141, 0.3);
  border-radius: 12px;
  padding: 1rem;
}

.example-sentence {
  display: flex;
  align-items: center;
  gap: 1rem;
  margin-bottom: 0.5rem;
}

.sentence {
  color: white;
  font-size: 1rem;
  flex: 1;
}

.example-audio-btn {
  background: rgba(231, 94, 141, 0.3);
  border: 1px solid rgba(231, 94, 141, 0.5);
  border-radius: 50%;
  width: 35px;
  height: 35px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  color: #e75e8d;
  transition: all 0.3s ease;
}

.example-audio-btn:hover {
  background: rgba(231, 94, 141, 0.5);
  transform: scale(1.1);
}

.example-translation {
  color: #b8b8b8;
  font-size: 0.9rem;
  font-style: italic;
}

.answer-section {
  margin-bottom: 2rem;
}

.question-prompt {
  text-align: center;
  margin-bottom: 2rem;
}

.question-prompt h3 {
  color: white;
  font-size: 1.5rem;
  margin-bottom: 1rem;
}

.difficulty-indicator {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
}

.difficulty-label {
  color: #b8b8b8;
  font-size: 0.9rem;
}

.difficulty-stars {
  display: flex;
  gap: 0.2rem;
}

.difficulty-stars .mdi-star {
  color: #444;
  font-size: 1.2rem;
}

.difficulty-stars .mdi-star.active {
  color: #ffd700;
}

.answer-options {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 1rem;
  margin-bottom: 2rem;
}

.answer-option {
  background: rgba(255, 255, 255, 0.1);
  border: 2px solid rgba(255, 255, 255, 0.2);
  border-radius: 12px;
  padding: 1rem;
  color: white;
  cursor: pointer;
  transition: all 0.3s ease;
  font-size: 1rem;
  text-align: left;
}

.answer-option:hover {
  background: rgba(255, 255, 255, 0.15);
  border-color: rgba(231, 94, 141, 0.5);
  transform: translateY(-2px);
}

.answer-option.selected {
  background: rgba(231, 94, 141, 0.3);
  border-color: #e75e8d;
}

.action-buttons {
  display: flex;
  justify-content: center;
  gap: 1rem;
}

.submit-btn, .hint-btn {
  padding: 0.75rem 2rem;
  border-radius: 25px;
  border: none;
  cursor: pointer;
  font-size: 1rem;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.submit-btn {
  background: linear-gradient(135deg, #e75e8d, #74c0fc);
  color: white;
}

.submit-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.submit-btn:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 5px 15px rgba(231, 94, 141, 0.4);
}

.hint-btn {
  background: rgba(116, 192, 252, 0.2);
  color: #74c0fc;
  border: 1px solid rgba(116, 192, 252, 0.5);
}

.hint-btn:hover {
  background: rgba(116, 192, 252, 0.3);
}

.result-section {
  text-align: center;
}

.result-feedback {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 1rem;
  padding: 1.5rem;
  border-radius: 15px;
  margin-bottom: 1.5rem;
}

.result-feedback.correct {
  background: rgba(76, 175, 80, 0.2);
  border: 1px solid rgba(76, 175, 80, 0.5);
}

.result-feedback.incorrect {
  background: rgba(244, 67, 54, 0.2);
  border: 1px solid rgba(244, 67, 54, 0.5);
}

.result-icon {
  font-size: 3rem;
}

.result-feedback.correct .result-icon {
  color: #4caf50;
}

.result-feedback.incorrect .result-icon {
  color: #f44336;
}

.result-message h3 {
  color: white;
  margin-bottom: 0.5rem;
}

.result-message p {
  color: #b8b8b8;
}

.correct-answer {
  background: rgba(76, 175, 80, 0.1);
  border: 1px solid rgba(76, 175, 80, 0.3);
  border-radius: 12px;
  padding: 1rem;
  margin-bottom: 1.5rem;
}

.meaning-display {
  color: white;
  font-size: 1.2rem;
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

.mastery-badge {
  background: linear-gradient(135deg, #ffd700, #ffb347);
  color: #333;
  padding: 0.5rem 1rem;
  border-radius: 15px;
  font-weight: bold;
  font-size: 0.9rem;
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
  gap: 0.5rem;
  margin: 0 auto;
  transition: all 0.3s ease;
}

.next-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 10px 25px rgba(231, 94, 141, 0.4);
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
</style>