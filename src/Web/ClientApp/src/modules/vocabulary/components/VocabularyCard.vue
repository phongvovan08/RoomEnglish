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
          <h3>Definition</h3>
          <p>{{ word.definition }}</p>
        </div>
      </div>

      <!-- Examples Section -->
      <div class="examples-section" v-if="word.examples.length">
        <h3>Examples</h3>
        <div class="examples-list">
          <div 
            v-for="(example, index) in word.examples" 
            :key="example.id"
            class="example-item"
            :class="{ 'flipped': visibleTranslations[index] }"
            @click="toggleTranslation(index)"
          >
            <div class="flip-card">
              <!-- Front side (English) -->
              <div class="flip-card-front">
                <div class="example-sentence">
                  <span class="sentence">{{ example.sentence }}</span>
                  <GlobalSpeechButton
                    :text="example.sentence"
                    :instance-id="getExampleAudioId(index)"
                    button-class="example-audio-btn"
                    @click.stop
                  />
                </div>
                <div v-if="example.grammar" class="example-grammar">
                  <span class="grammar-text">{{ example.grammar }}</span>
                </div>
                <div class="flip-hint">
                  <i class="mdi mdi-flip-vertical"></i>
                </div>
              </div>
              
              <!-- Back side (Vietnamese) -->
              <div class="flip-card-back">
                <div class="example-translation-content">
                  <div class="translation-text">{{ example.translation }}</div>
                </div>
                <div class="flip-hint">
                  <i class="mdi mdi-flip-horizontal"></i>
                </div>
              </div>
            </div>
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
import { ref, computed, onMounted, readonly } from 'vue'
import GlobalSpeechButton from '@/components/GlobalSpeechButton.vue'
import { Icon } from '@iconify/vue'
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
const answerOptions = ref<AnswerOption[]>([])
const visibleTranslations = ref<Record<number, boolean>>({})

// Instance IDs for different audio sources  
const WORD_AUDIO_ID = 'word-audio'
const getExampleAudioId = (index: number) => `example-audio-${index}`

// Toggle translation visibility
const toggleTranslation = (index: number) => {
  visibleTranslations.value[index] = !visibleTranslations.value[index]
}

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
  overflow: visible;
}

.example-item {
  background: transparent;
  border: none;
  border-radius: 12px;
  padding: 0;
  position: relative;
  overflow: visible;
  perspective: 1000px;
  height: 120px;
  cursor: pointer;
  transition: transform 0.3s ease;
}

.example-item:hover {
  transform: translateY(-2px);
}

.flip-card {
  position: relative;
  width: 100%;
  height: 100%;
  text-align: center;
  transition: transform 0.8s ease-in-out;
  transform-style: preserve-3d;
}

.example-item.flipped .flip-card {
  transform: rotateX(180deg);
}

.flip-card-front,
.flip-card-back {
  position: absolute;
  width: 100%;
  height: 100%;
  -webkit-backface-visibility: hidden;
  backface-visibility: hidden;
  border-radius: 12px;
  padding: 1rem;
  display: flex;
  flex-direction: column;
  justify-content: center;
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
}

.flip-card-front {
  background: linear-gradient(135deg, rgba(231, 94, 141, 0.2), rgba(231, 94, 141, 0.1));
  border: 1px solid rgba(231, 94, 141, 0.3);
  color: white;
}

.flip-card-back {
  background: linear-gradient(135deg, rgba(116, 192, 252, 0.2), rgba(116, 192, 252, 0.1));
  border: 1px solid rgba(116, 192, 252, 0.3);
  color: white;
  transform: rotateX(180deg);
}

.example-sentence {
  display: flex;
  align-items: center;
  gap: 1rem;
  margin-bottom: 0.5rem;
  flex: 1;
}

.sentence {
  color: white;
  font-size: 1rem;
  flex: 1;
  text-align: left;
}

.flip-hint {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  color: rgba(255, 255, 255, 0.7);
  font-size: 0.8rem;
  margin-top: 0.5rem;
}

.flip-hint i {
  font-size: 1rem;
}

.example-translation-content {
  flex: 1;
  display: flex;
  flex-direction: column;
  justify-content: center;
  text-align: center;
}

.example-translation-content h4 {
  color: #74c0fc;
  margin-bottom: 0.5rem;
  font-size: 0.9rem;
  font-weight: 600;
}

.example-translation-content .translation-text {
  color: white;
  font-size: 1rem;
  font-style: italic;
  line-height: 1.4;
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

.example-audio-btn:hover:not(:disabled) {
  background: rgba(231, 94, 141, 0.5);
  transform: scale(1.1);
}

.example-audio-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
  animation: pulse 1.5s infinite;
}

@keyframes pulse {
  0%, 100% { opacity: 0.5; }
  50% { opacity: 0.8; }
}

.example-translation {
  color: #b8b8b8;
  font-size: 0.9rem;
  font-style: italic;
  cursor: pointer;
  padding: 0.5rem;
  border-radius: 6px;
  border: 1px solid transparent;
  transition: all 0.3s ease;
  min-height: 1.2rem;
  display: flex;
  align-items: center;
}

.example-translation:hover {
  background: rgba(184, 184, 184, 0.1);
  border-color: rgba(184, 184, 184, 0.3);
}

.example-translation.hidden {
  background: rgba(116, 192, 252, 0.05);
  border-color: rgba(116, 192, 252, 0.2);
}

.example-translation.hidden:hover {
  background: rgba(116, 192, 252, 0.1);
  border-color: rgba(116, 192, 252, 0.4);
}

.translation-hint {
  color: #74c0fc;
  font-weight: 500;
  font-size: 0.85rem;
}

.example-grammar {
  margin-top: 0.5rem;
  padding: 0.5rem;
  background: rgba(116, 192, 252, 0.15);
  border-left: 2px solid #74c0fc;
  border-radius: 0 6px 6px 0;
  font-size: 0.75rem;
  text-align: left;
}

.grammar-label {
  color: #74c0fc;
  font-weight: 600;
  margin-right: 0.25rem;
  font-size: 0.7rem;
}

.grammar-text {
  color: #e0e0e0;
  line-height: 1.3;
  font-size: 0.75rem;
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