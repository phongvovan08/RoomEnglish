<template>
  <div class="dictation-card">
    <div class="card-container">
      <!-- Header -->
      <div class="dictation-header">
        <h2>🎤 Dictation Practice</h2>
        <div class="instruction">
          Listen to the sentence and type what you hear
        </div>
      </div>

      <!-- Audio Section -->
      <div class="audio-section">
        <div class="audio-controls">
          <button 
            @click="playAudio" 
            :disabled="!example?.audioUrl || isPlayingAudio"
            class="play-btn"
            :class="{ 'playing': isPlayingAudio }"
          >
            <i class="mdi" :class="isPlayingAudio ? 'mdi-volume-high' : 'mdi-play-circle'"></i>
            <span>{{ isPlayingAudio ? 'Playing...' : 'Play Audio' }}</span>
          </button>
          
          <div class="playback-controls" v-if="example?.audioUrl">
            <button @click="changePlaybackSpeed" class="speed-btn">
              <i class="mdi mdi-speedometer"></i>
              {{ playbackSpeed }}x
            </button>
            
            <div class="play-count">
              Played: {{ playCount }} times
            </div>
          </div>
        </div>

        <div class="audio-waveform">
          <div class="waveform-bars">
            <div 
              v-for="i in 20" 
              :key="i"
              class="bar"
              :class="{ 'active': isPlayingAudio }"
              :style="{ animationDelay: `${i * 50}ms` }"
            ></div>
          </div>
        </div>
      </div>

      <!-- Input Section -->
      <div class="input-section" v-if="!showResult">
        <div class="input-container">
          <div class="input-header">
            <label for="dictation-input">Type what you hear:</label>
            <div class="timer" v-if="isRecording || userInput.length > 0">
              <i class="mdi mdi-timer"></i>
              {{ formatTime(elapsedTime) }}
            </div>
          </div>
          
          <div class="input-wrapper">
            <textarea
              id="dictation-input"
              v-model="userInput"
              @input="handleInput"
              :placeholder="isRecording ? 'Listening...' : 'Start typing or use voice input'"
              :disabled="isRecording"
              class="dictation-input"
              rows="4"
            ></textarea>
            
            <!-- Voice Input Controls -->
            <div class="voice-controls">
              <button 
                @click="toggleRecording"
                :disabled="!speechRecognitionSupported"
                class="voice-btn"
                :class="{ 
                  'recording': isRecording,
                  'disabled': !speechRecognitionSupported 
                }"
              >
                <i class="mdi" :class="isRecording ? 'mdi-microphone' : 'mdi-microphone-outline'"></i>
                <span>{{ isRecording ? 'Stop Recording' : 'Voice Input' }}</span>
              </button>
              
              <div class="recording-indicator" v-if="isRecording">
                <div class="pulse"></div>
                <span>Listening...</span>
              </div>
            </div>
          </div>
        </div>

        <div class="action-buttons">
          <button 
            @click="submitAnswer" 
            :disabled="!userInput.trim()"
            class="submit-btn"
          >
            <i class="mdi mdi-check"></i>
            Submit
          </button>
          
          <button @click="clearInput" class="clear-btn" v-if="userInput">
            <i class="mdi mdi-eraser"></i>
            Clear
          </button>
          
          <button @click="showHint" class="hint-btn">
            <i class="mdi mdi-lightbulb"></i>
            Hint
          </button>
        </div>
      </div>

      <!-- Result Section -->
      <div class="result-section" v-if="showResult && dictationResult">
        <div class="result-header" :class="{ 'correct': dictationResult.isCorrect, 'incorrect': !dictationResult.isCorrect }">
          <div class="result-icon">
            <i class="mdi" :class="dictationResult.isCorrect ? 'mdi-check-circle' : 'mdi-close-circle'"></i>
          </div>
          <div class="result-message">
            <h3>{{ dictationResult.isCorrect ? 'Excellent!' : 'Good try!' }}</h3>
            <div class="accuracy">
              Accuracy: {{ dictationResult.accuracyPercentage }}%
            </div>
          </div>
        </div>

        <div class="comparison-section">
          <div class="your-answer">
            <h4>Your Answer:</h4>
            <div class="answer-text user-text">{{ dictationResult.userInput }}</div>
          </div>
          
          <div class="correct-answer">
            <h4>Correct Answer:</h4>
            <div class="answer-text correct-text">{{ dictationResult.correctAnswer }}</div>
            <button @click="playCorrectAudio" class="replay-btn">
              <i class="mdi mdi-replay"></i>
              Listen Again
            </button>
          </div>
        </div>

        <div class="translation-section" v-if="example?.translation">
          <h4>Translation:</h4>
          <div class="translation-text">{{ example.translation }}</div>
        </div>

        <div class="performance-stats">
          <div class="stat-item">
            <i class="mdi mdi-clock"></i>
            <span>Time: {{ formatTime(dictationResult.timeTakenSeconds) }}</span>
          </div>
          <div class="stat-item">
            <i class="mdi mdi-target"></i>
            <span>Accuracy: {{ dictationResult.accuracyPercentage }}%</span>
          </div>
          <div class="stat-item" v-if="dictationResult.isCorrect">
            <i class="mdi mdi-trophy"></i>
            <span>Perfect!</span>
          </div>
        </div>

        <button @click="$emit('next')" class="next-btn">
          <span>Next Exercise</span>
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
          <div class="hint-list">
            <div class="hint-item">
              <i class="mdi mdi-information"></i>
              <span>The sentence has {{ example?.sentence.split(' ').length || 0 }} words</span>
            </div>
            <div class="hint-item" v-if="example?.sentence">
              <i class="mdi mdi-format-letter-case"></i>
              <span>First word starts with "{{ example.sentence.split(' ')[0]?.charAt(0) }}"</span>
            </div>
            <div class="hint-item">
              <i class="mdi mdi-volume-high"></i>
              <span>Listen carefully to pronunciation and punctuation</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, watchEffect } from 'vue'
import { useDictation } from '../composables/useDictation'
import type { VocabularyExample, DictationResult } from '../types/vocabulary.types'

interface Props {
  example: VocabularyExample | null
}

const props = defineProps<Props>()

const emit = defineEmits<{
  submit: [result: DictationResult]
  next: []
}>()

const {
  userInput,
  isRecording,
  isPlaying: isPlayingAudio,
  dictationResult,
  isLoading,
  error,
  timeElapsed,
  initSpeechRecognition,
  startRecording,
  stopRecording,
  playAudio: playDictationAudio,
  submitDictation,
  setExample,
  reset
} = useDictation()

// Local state
const showResult = ref(false)
const showHintModal = ref(false)
const playCount = ref(0)
const playbackSpeed = ref(1)
const speechRecognitionSupported = ref(false)
const startTime = ref<number | null>(null)
const elapsedTime = ref(0)

// Timer
let timer: number | null = null

// Computed properties
const formattedElapsedTime = computed(() => formatTime(elapsedTime.value))

// Methods
const startTimer = () => {
  startTime.value = Date.now()
  timer = setInterval(() => {
    if (startTime.value) {
      elapsedTime.value = Math.floor((Date.now() - startTime.value) / 1000)
    }
  }, 1000)
}

const stopTimer = () => {
  if (timer) {
    clearInterval(timer)
    timer = null
  }
}

const formatTime = (seconds: number): string => {
  const mins = Math.floor(seconds / 60)
  const secs = seconds % 60
  return `${mins}:${secs.toString().padStart(2, '0')}`
}

const playAudio = async () => {
  if (!props.example?.audioUrl) return
  
  try {
    await playDictationAudio(props.example.audioUrl)
    playCount.value++
    
    if (!startTime.value) {
      startTimer()
    }
  } catch (err) {
    console.error('Failed to play audio:', err)
  }
}

const playCorrectAudio = async () => {
  if (props.example?.audioUrl) {
    await playAudio()
  }
}

const handleInput = () => {
  if (!startTime.value) {
    startTimer()
  }
}

const toggleRecording = () => {
  if (isRecording.value) {
    stopRecording()
  } else {
    if (!startTime.value) {
      startTimer()
    }
    startRecording()
  }
}

const submitAnswer = async () => {
  if (!props.example || !userInput.value.trim()) return
  
  try {
    stopTimer()
    
    const result = await submitDictation(
      props.example.id,
      userInput.value.trim()
    )
    
    showResult.value = true
    emit('submit', result)
  } catch (err) {
    console.error('Failed to submit dictation:', err)
  }
}

const clearInput = () => {
  userInput.value = ''
}

const showHint = () => {
  showHintModal.value = true
}

const closeHint = () => {
  showHintModal.value = false
}

const changePlaybackSpeed = () => {
  const speeds = [0.5, 0.75, 1, 1.25, 1.5]
  const currentIndex = speeds.indexOf(playbackSpeed.value)
  playbackSpeed.value = speeds[(currentIndex + 1) % speeds.length]
}

const resetComponent = () => {
  showResult.value = false
  showHintModal.value = false
  playCount.value = 0
  playbackSpeed.value = 1
  elapsedTime.value = 0
  startTime.value = null
  stopTimer()
  reset()
}

// Initialize component
onMounted(() => {
  // Check speech recognition support
  speechRecognitionSupported.value = 'webkitSpeechRecognition' in window || 'SpeechRecognition' in window
  
  if (speechRecognitionSupported.value) {
    initSpeechRecognition()
  }
  
  if (props.example) {
    setExample(props.example)
  }
})

onUnmounted(() => {
  stopTimer()
  stopRecording()
})

// Watch for example changes
const updateExample = () => {
  if (props.example) {
    setExample(props.example)
    resetComponent()
  }
}

// Watch for example changes
watchEffect(() => {
  updateExample()
})
</script>

<style scoped>
.dictation-card {
  max-width: 900px;
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

.dictation-header {
  text-align: center;
  margin-bottom: 2rem;
  padding-bottom: 1rem;
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
}

.dictation-header h2 {
  color: transparent;
  background: linear-gradient(135deg, #e75e8d, #74c0fc);
  background-clip: text;
  -webkit-background-clip: text;
  font-size: 2rem;
  margin-bottom: 0.5rem;
}

.instruction {
  color: #b8b8b8;
  font-size: 1.1rem;
  font-style: italic;
}

.audio-section {
  margin-bottom: 2rem;
}

.audio-controls {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}

.play-btn {
  background: linear-gradient(135deg, #e75e8d, #74c0fc);
  color: white;
  border: none;
  padding: 1rem 2rem;
  border-radius: 25px;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 0.75rem;
  font-size: 1.1rem;
  transition: all 0.3s ease;
  min-width: 160px;
}

.play-btn:hover:not(:disabled) {
  transform: translateY(-3px);
  box-shadow: 0 8px 25px rgba(231, 94, 141, 0.4);
}

.play-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.play-btn.playing {
  animation: pulse 2s infinite;
}

.playback-controls {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.speed-btn {
  background: rgba(116, 192, 252, 0.2);
  color: #74c0fc;
  border: 1px solid rgba(116, 192, 252, 0.5);
  border-radius: 15px;
  padding: 0.5rem 1rem;
  cursor: pointer;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.speed-btn:hover {
  background: rgba(116, 192, 252, 0.3);
}

.play-count {
  color: #b8b8b8;
  font-size: 0.9rem;
}

.audio-waveform {
  background: rgba(255, 255, 255, 0.05);
  border-radius: 15px;
  padding: 1rem;
  display: flex;
  justify-content: center;
  align-items: center;
  height: 80px;
}

.waveform-bars {
  display: flex;
  align-items: end;
  gap: 3px;
  height: 50px;
}

.bar {
  width: 4px;
  background: linear-gradient(to top, #e75e8d, #74c0fc);
  border-radius: 2px;
  height: 20px;
  transition: height 0.3s ease;
}

.bar.active {
  animation: waveform 1.5s ease-in-out infinite;
}

.input-section {
  margin-bottom: 2rem;
}

.input-container {
  margin-bottom: 1.5rem;
}

.input-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1rem;
}

.input-header label {
  color: white;
  font-size: 1.1rem;
  font-weight: 500;
}

.timer {
  color: #74c0fc;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-family: monospace;
  font-size: 1rem;
}

.input-wrapper {
  position: relative;
}

.dictation-input {
  width: 100%;
  background: rgba(255, 255, 255, 0.1);
  border: 2px solid rgba(255, 255, 255, 0.2);
  border-radius: 15px;
  padding: 1rem;
  color: white;
  font-size: 1rem;
  resize: vertical;
  transition: all 0.3s ease;
  font-family: inherit;
}

.dictation-input:focus {
  outline: none;
  border-color: #e75e8d;
  box-shadow: 0 0 0 3px rgba(231, 94, 141, 0.2);
}

.dictation-input:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

.dictation-input::placeholder {
  color: #888;
}

.voice-controls {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 1rem;
}

.voice-btn {
  background: rgba(231, 94, 141, 0.2);
  color: #e75e8d;
  border: 2px solid rgba(231, 94, 141, 0.5);
  border-radius: 25px;
  padding: 0.75rem 1.5rem;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  transition: all 0.3s ease;
}

.voice-btn:hover:not(:disabled) {
  background: rgba(231, 94, 141, 0.3);
  border-color: #e75e8d;
}

.voice-btn.recording {
  background: rgba(231, 94, 141, 0.4);
  border-color: #e75e8d;
  animation: recordingPulse 2s infinite;
}

.voice-btn.disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.recording-indicator {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  color: #e75e8d;
}

.pulse {
  width: 12px;
  height: 12px;
  background: #e75e8d;
  border-radius: 50%;
  animation: pulse 1.5s infinite;
}

.action-buttons {
  display: flex;
  justify-content: center;
  gap: 1rem;
  flex-wrap: wrap;
}

.submit-btn, .clear-btn, .hint-btn {
  padding: 0.75rem 1.5rem;
  border-radius: 25px;
  border: none;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  transition: all 0.3s ease;
  font-size: 1rem;
}

.submit-btn {
  background: linear-gradient(135deg, #4caf50, #66bb6a);
  color: white;
}

.submit-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.submit-btn:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 5px 15px rgba(76, 175, 80, 0.4);
}

.clear-btn {
  background: rgba(244, 67, 54, 0.2);
  color: #f44336;
  border: 1px solid rgba(244, 67, 54, 0.5);
}

.clear-btn:hover {
  background: rgba(244, 67, 54, 0.3);
}

.hint-btn {
  background: rgba(255, 193, 7, 0.2);
  color: #ffc107;
  border: 1px solid rgba(255, 193, 7, 0.5);
}

.hint-btn:hover {
  background: rgba(255, 193, 7, 0.3);
}

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

.translation-section {
  background: rgba(231, 94, 141, 0.1);
  border: 1px solid rgba(231, 94, 141, 0.3);
  border-radius: 15px;
  padding: 1.5rem;
  margin-bottom: 2rem;
}

.translation-section h4 {
  color: #e75e8d;
  margin-bottom: 1rem;
}

.translation-text {
  color: white;
  font-size: 1.1rem;
  line-height: 1.5;
  font-style: italic;
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
  border: 1px solid rgba(255, 193, 7, 0.3);
  border-radius: 15px;
  padding: 2rem;
  max-width: 500px;
  width: 90%;
  backdrop-filter: blur(20px);
}

.hint-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}

.hint-header h3 {
  color: #ffc107;
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

.hint-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.hint-item {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 0.75rem;
  background: rgba(255, 255, 255, 0.05);
  border-radius: 10px;
  color: white;
}

.hint-item i {
  color: #ffc107;
  font-size: 1.2rem;
}

@keyframes pulse {
  0% { box-shadow: 0 0 0 0 rgba(231, 94, 141, 0.7); }
  70% { box-shadow: 0 0 0 10px rgba(231, 94, 141, 0); }
  100% { box-shadow: 0 0 0 0 rgba(231, 94, 141, 0); }
}

@keyframes recordingPulse {
  0% { border-color: rgba(231, 94, 141, 0.5); }
  50% { border-color: #e75e8d; }
  100% { border-color: rgba(231, 94, 141, 0.5); }
}

@keyframes waveform {
  0%, 100% { height: 20px; }
  50% { height: 45px; }
}

@media (max-width: 768px) {
  .audio-controls {
    flex-direction: column;
    gap: 1rem;
    text-align: center;
  }
  
  .comparison-section {
    grid-template-columns: 1fr;
  }
  
  .performance-stats {
    flex-direction: column;
    align-items: center;
  }
  
  .action-buttons {
    flex-direction: column;
    align-items: center;
  }
}
</style>