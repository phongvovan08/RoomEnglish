<template>
  <div class="dictation-card">
    <div class="card-container">
      <!-- Header -->
      <div class="dictation-header">
        <h2>🎤 Dictation Practice</h2>
        <div class="instruction">
          Listen to the sentence and type what you hear
        </div>
        <div v-if="example?.sentence && showSentence" class="example-sentence">
          <div class="sentence-label">
            <i class="mdi mdi-text"></i>
            Example Sentence:
          </div>
          <div class="sentence-text">
            {{ example.sentence }}
          </div>
          <button @click="showSentence = false" class="hide-sentence-btn">
            <i class="mdi mdi-eye-off"></i>
            Hide (Practice mode)
          </button>
        </div>
        <button 
          v-else-if="example?.sentence && !showSentence"
          @click="showSentence = true" 
          class="show-sentence-btn"
        >
          <i class="mdi mdi-eye"></i>
          Show Sentence (Study mode)
        </button>
      </div>

      <!-- Audio Player -->
      <AudioPlayer 
        :has-audio="!!example?.sentence"
        :is-playing="isPlayingAudio"
        :playback-speed="playbackSpeed"
        :play-count="playCount"
        @play="playAudio"
        @change-speed="changePlaybackSpeed"
      />

      <!-- Input Section -->
      <InputSection
        v-if="!showResult"
        v-model:user-input="userInput"
        :is-recording="isRecording"
        :elapsed-time="elapsedTime"
        :speech-recognition-supported="speechRecognitionSupported"
        @toggle-recording="toggleRecording"
        @check="checkAnswer"
        @submit="submitAnswer"
        @clear="clearInput"
        @show-hint="showHint"
      />

      <!-- Word Comparison (appears after check, before submit) -->
      <WordComparison 
        v-if="showComparison && !showResult && example?.sentence"
        :user-input="userInput"
        :correct-answer="example.sentence"
      />

      <!-- Result Display -->
      <ResultDisplay
        v-if="showResult && dictationResult"
        :result="dictationResult"
        :translation="example?.translation"
        :grammar="example?.grammar"
        @replay="playCorrectAudio"
        @next="$emit('next')"
      />

      <!-- Hint Modal -->
      <HintModal
        :show="showHintModal"
        :sentence="example?.sentence"
        @close="closeHint"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, watchEffect } from 'vue'
import { useDictation } from '../composables/useDictation'
import type { VocabularyExample, VocabularyWord, DictationResult } from '../types/vocabulary.types'
import AudioPlayer from './dictation/audio-player/AudioPlayer.vue'
import InputSection from './dictation/input-section/InputSection.vue'
import ResultDisplay from './dictation/result-display/ResultDisplay.vue'
import HintModal from './dictation/hint-modal/HintModal.vue'
import WordComparison from './dictation/word-comparison/WordComparison.vue'

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
const showSentence = ref(false)
const playCount = ref(0)
const playbackSpeed = ref(1)
const speechRecognitionSupported = ref(false)
const startTime = ref<number | null>(null)
const elapsedTime = ref(0)
const showTranslation = ref(false)
const showComparison = ref(false)

// Timer
let timer: ReturnType<typeof setInterval> | null = null

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
  if (!props.example?.sentence) return
  
  try {
    // Play the sentence text with current playback speed
    await playDictationAudio(props.example.sentence, playbackSpeed.value)
    playCount.value++
    
    if (!startTime.value) {
      startTimer()
    }
  } catch (err) {
    console.error('Failed to play audio:', err)
  }
}

const playCorrectAudio = async () => {
  if (props.example?.sentence) {
    await playAudio()
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
    showComparison.value = false // Hide comparison when showing full result
    emit('submit', result)
  } catch (err) {
    console.error('Failed to submit dictation:', err)
  }
}

const checkAnswer = () => {
  // Show word comparison without submitting
  if (userInput.value.trim()) {
    showComparison.value = true
  }
}

const clearInput = () => {
  userInput.value = ''
  showComparison.value = false // Hide comparison when clearing
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
  showSentence.value = false
  showComparison.value = false
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
    // Auto play audio when component loads
    setTimeout(() => {
      playAudio()
    }, 500)
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

.example-sentence {
  margin-top: 1.5rem;
  padding: 1.5rem;
  background: linear-gradient(135deg, rgba(116, 192, 252, 0.1), rgba(231, 94, 141, 0.1));
  border: 2px solid rgba(116, 192, 252, 0.3);
  border-radius: 15px;
  animation: fadeIn 0.5s ease-in-out;
}

.sentence-label {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  color: #74c0fc;
  font-size: 0.9rem;
  font-weight: 600;
  margin-bottom: 0.75rem;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.sentence-text {
  color: white;
  font-size: 1.3rem;
  line-height: 1.6;
  margin-bottom: 1rem;
  font-weight: 500;
  text-align: center;
  padding: 0.5rem 0;
}

.hide-sentence-btn,
.show-sentence-btn {
  background: rgba(231, 94, 141, 0.2);
  color: #e75e8d;
  border: 1px solid rgba(231, 94, 141, 0.5);
  padding: 0.5rem 1rem;
  border-radius: 20px;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.9rem;
  transition: all 0.3s ease;
  margin: 0 auto;
}

.hide-sentence-btn:hover,
.show-sentence-btn:hover {
  background: rgba(231, 94, 141, 0.3);
  border-color: #e75e8d;
  transform: translateY(-2px);
}

.show-sentence-btn {
  margin-top: 1rem;
  background: rgba(116, 192, 252, 0.2);
  color: #74c0fc;
  border-color: rgba(116, 192, 252, 0.5);
}

.show-sentence-btn:hover {
  background: rgba(116, 192, 252, 0.3);
  border-color: #74c0fc;
}

@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(-10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@media (max-width: 768px) {
  .card-container {
    padding: 1.5rem;
  }
  
  .dictation-header h2 {
    font-size: 1.5rem;
  }
}
</style>