<template>
  <div class="dictation-card">
    <div class="card-container">
      <!-- Header -->
      <div class="dictation-header">
        
        <div v-if="example?.sentence && showSentence && !showResult" class="example-sentence">

          <div class="sentence-text">
            {{ example.sentence }}
          </div>
          <button @click="showSentence = false" class="hide-sentence-btn">
            <Icon icon="mdi:eye-off" class="w-5 h-5" />
            Hide (Practice mode)
          </button>
        </div>
        <button 
          v-else-if="example?.sentence && !showSentence && !showResult"
          @click="showSentence = true" 
          class="show-sentence-btn"
        >
          <Icon icon="mdi:eye" class="w-5 h-5" />
          Show Sentence (Study mode)
        </button>
      </div>

      <!-- Audio Player - Using GlobalSpeechButton -->
      <div class="audio-player-section" v-if="example?.sentence">
        <div class="audio-controls-wrapper">
          <GlobalSpeechButton 
            ref="listenButton"
            :text="example.sentence"
            instance-id="dictation-audio"
            :show-text="true"
            button-class="speech-btn large"
            :custom-rate="playbackSpeed"
          />
          
          <!-- Speed Dropdown -->
          <div class="speed-control">
            <label for="speed-select" class="speed-label">
              <Icon icon="mdi:speedometer" class="w-4 h-4" />
              Speed:
            </label>
            <select 
              id="speed-select"
              v-model="playbackSpeed" 
              class="speed-select"
            > 
             <option :value="0.5">0.5x (Slow)</option>
              <option :value="0.75">0.75x</option>
              <option :value="1">1x (Normal)</option>
              <option :value="1.25">1.25x</option>
              <option :value="1.5">1.5x (Fast)</option>
              <option :value="2">2x</option>
             </select>
          </div>
        </div>
      </div>

      <!-- Input Section (textarea only, no buttons) -->
      <div v-if="!showResult" class="input-section">
        <div class="input-container">
          <div class="input-header">
            <label for="dictation-input">Type what you hear:</label>
            <div class="header-controls">
              <!-- Voice Input Icon Button -->
              <button 
                @click="toggleRecording"
                :disabled="!speechRecognitionSupported"
                class="voice-icon-btn"
                :class="{ 
                  'recording': isRecording,
                  'disabled': !speechRecognitionSupported 
                }"
                :title="isRecording ? 'Stop Recording' : 'Voice Input'"
              >
                <Icon :icon="isRecording ? 'mdi:microphone' : 'mdi:microphone-outline'" class="icon-size" />
              </button>
              
              <!-- Timer -->
              <div class="timer" v-if="isRecording || userInput.length > 0">
                <Icon icon="mdi:timer" class="w-4 h-4" />
                {{ formatTime(elapsedTime) }}
              </div>
            </div>
          </div>
          
          <div class="input-wrapper">
            <textarea
              ref="inputTextarea"
              id="dictation-input"
              v-model="userInput"
              :placeholder="isRecording ? 'Listening...' : 'Start typing or click microphone for voice input'"
              :disabled="isRecording"
              class="dictation-input"
              rows="3"
              @keydown.enter.exact="handleEnterKey"
            ></textarea>
            
            <!-- Recording Indicator -->
            <div class="recording-indicator" v-if="isRecording">
              <div class="pulse"></div>
              <span>Listening...</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Word Comparison (always show when user has input and not showing result) -->
      <WordComparison 
        ref="wordComparisonRef"
        v-if="!showResult && example?.sentence && userInput.trim()"
        :user-input="userInput"
        :correct-answer="example.sentence"
        :show-correct-words="hasSubmitted"
      />

      <!-- Action Buttons (below Word Comparison) -->
      <div v-if="!showResult" class="action-buttons">
       
        
        <button 
          @click="submitAnswer" 
          :disabled="!userInput.trim()"
          class="submit-btn"
        >
          <Icon icon="mdi:send" class="w-5 h-5" />
          Submit
        </button>
        
        <button @click="clearInput" class="clear-btn" v-if="userInput">
          <Icon icon="mdi:eraser" class="w-5 h-5" />
          Clear
        </button>
        
        <button 
          v-if="showBackToGrid"
          @click="$emit('backToGrid')" 
          class="back-to-grid-btn"
        >
          <Icon icon="mdi:arrow-left" class="w-5 h-5" />
          Back to Words
        </button>
      </div>

      <!-- Result Display -->
      <ResultDisplay
        v-if="showResult && dictationResult"
        :result="dictationResult"
        :sentence="example?.sentence"
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
import { ref, computed, onMounted, onUnmounted, watchEffect, nextTick } from 'vue'
import { Icon } from '@iconify/vue'
import { useDictation } from '../composables/useDictation'
import { useSpeechSynthesis } from '@/composables/useSpeechSynthesis'
import { useSpeechSettings } from '@/composables/useSpeechSettings'
import GlobalSpeechButton from '@/components/GlobalSpeechButton.vue'
import type { VocabularyExample, VocabularyWord, DictationResult } from '../types/vocabulary.types'
import ResultDisplay from './dictation/result-display/ResultDisplay.vue'
import HintModal from './dictation/hint-modal/HintModal.vue'
import WordComparison from './dictation/word-comparison/WordComparison.vue'

interface Props {
  example: VocabularyExample | null
  word?: VocabularyWord | null
  showBackToGrid?: boolean // Show back to grid button
}

const props = withDefaults(defineProps<Props>(), {
  showBackToGrid: false
})

const emit = defineEmits<{
  submit: [result: DictationResult]
  next: []
  backToGrid: []
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
const listenButton = ref<InstanceType<typeof GlobalSpeechButton> | null>(null)
const inputTextarea = ref<HTMLTextAreaElement | null>(null)
const hasSubmitted = ref(false) // Track if user has submitted
const wordComparisonRef = ref<InstanceType<typeof WordComparison> | null>(null)

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
  if (!props.example?.sentence || !listenButton.value) return
  
  // Programmatically click the Listen button
  const buttonElement = listenButton.value.$el as HTMLButtonElement
  if (buttonElement && !buttonElement.disabled) {
    buttonElement.click()
    playCount.value++
    
    if (!startTime.value) {
      startTimer()
    }
  }
}

const playCorrectAudio = async () => {
  if (props.example?.sentence) {
    await playAudio()
  }
}

// No longer needed - using button click directly
// const triggerPlayFromKeyboard = () => {
//   if (!showResult.value && props.example?.sentence && !isPlayingAudio.value) {
//     playAudio()
//   }
// }

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
  console.log('📝 Submit button clicked')
  console.log('📝 Example:', props.example)
  console.log('📝 User input:', userInput.value)
  
  if (!props.example) {
    console.error('❌ No example provided')
    return
  }
  
  if (!userInput.value.trim()) {
    console.error('❌ No user input')
    return
  }
  
  // Mark as submitted to show correct words
  hasSubmitted.value = true
  
  // Wait for next tick to ensure WordComparison updates
  await nextTick()
  
  // Check if there are any errors
  if (wordComparisonRef.value?.hasErrors) {
    console.log('❌ There are errors in the answer. Focus on input to fix.')
    // Focus on the input to let user fix the errors
    inputTextarea.value?.focus()
    return
  }
  
  try {
    // Calculate final elapsed time before stopping timer
    if (startTime.value) {
      elapsedTime.value = Math.floor((Date.now() - startTime.value) / 1000)
    }
    stopTimer()
    
    console.log('📤 Submitting dictation...')
    console.log('⏱️ Elapsed time:', elapsedTime.value, 'seconds')
    
    const result = await submitDictation(
      props.example.id,
      userInput.value.trim(),
      elapsedTime.value
    )
    
    console.log('✅ Dictation result:', result)
    console.log('⏱️ Result time:', result.timeTakenSeconds, 'seconds')
    showResult.value = true
    emit('submit', result)
  } catch (err) {
    console.error('❌ Failed to submit dictation:', err)
    // Show error to user
    alert(`Failed to submit: ${err instanceof Error ? err.message : 'Unknown error'}`)
  }
}

const checkAnswer = () => {
  // Word comparison is always visible when userInput has content
  // This function can be used for additional actions if needed
}

const handleEnterKey = (event: KeyboardEvent) => {
  // Prevent default Enter behavior (new line in textarea)
  event.preventDefault()
  
  // Only submit if there's user input and not already showing result
  if (userInput.value.trim() && !showResult.value) {
    submitAnswer()
  }
}

const clearInput = () => {
  userInput.value = ''
  hasSubmitted.value = false // Reset submitted state
}

const showHint = () => {
  showHintModal.value = true
}

const closeHint = () => {
  showHintModal.value = false
}

const changePlaybackSpeed = (speed: number) => {
  playbackSpeed.value = speed
}

const resetComponent = () => {
  showResult.value = false
  showHintModal.value = false
  showSentence.value = false
  playCount.value = 0
  // Don't reset playbackSpeed - keep user preference
  // playbackSpeed.value = 1
  elapsedTime.value = 0
  startTime.value = null
  hasSubmitted.value = false
  stopTimer()
  reset()
}

// Keyboard shortcut handler
const handleKeyDown = (event: KeyboardEvent) => {
  // Ctrl key to play audio
  if (event.ctrlKey && !event.shiftKey && !event.altKey && !event.metaKey) {
    // Only trigger if not in result view and has audio button
    if (!showResult.value && props.example?.sentence && listenButton.value) {
      event.preventDefault()
      
      // Programmatically click the Listen button
      const buttonElement = listenButton.value.$el as HTMLButtonElement
      if (buttonElement && !buttonElement.disabled) {
        buttonElement.click()
      }
    }
  }
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
  
  // Add keyboard event listener
  window.addEventListener('keydown', handleKeyDown)
})

// Watch dictationResult for debugging
watchEffect(() => {
  console.log('👀 DictationResult changed:', dictationResult.value)
  console.log('👀 showResult:', showResult.value)
})

// Watch userInput to start timer when user starts typing
watchEffect(() => {
  if (userInput.value.length > 0 && !startTime.value && !showResult.value) {
    startTimer()
  }
})

// Watch userInput changes after submit - reset hasSubmitted when user modifies input
let previousInput = ''
watchEffect(() => {
  if (hasSubmitted.value && userInput.value !== previousInput) {
    // User is editing after submit, hide correct words again
    hasSubmitted.value = false
  }
  previousInput = userInput.value
})

onUnmounted(() => {
  stopTimer()
  stopRecording()
  // Remove keyboard event listener
  window.removeEventListener('keydown', handleKeyDown)
})

// Watch for example changes
const updateExample = () => {
  if (props.example) {
    setExample(props.example)
    resetComponent()
    
    // Auto play audio when example changes - DISABLED to prevent auto-play loop
    // User can manually click play button
    // setTimeout(() => {
    //   playAudio()
    //   
    //   // Auto focus on input textarea
    //   if (inputTextarea.value && !showResult.value) {
    //     inputTextarea.value.focus()
    //   }
    // }, 300)
    
    // Just auto-focus, no auto-play
    setTimeout(() => {
      if (inputTextarea.value && !showResult.value) {
        inputTextarea.value.focus()
      }
    }, 100)
  }
}

// Watch for example changes
watchEffect(() => {
  updateExample()
})
</script>

<style scoped>
.dictation-card {
  width: 97%;
  margin: 0;
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
  margin-bottom: 1rem;
  padding-bottom: 1rem;
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
}

.word-info {
  display: flex;
  justify-content: center;
}

.word-badge {
  display: inline-flex;
  align-items: center;
  gap: 0.75rem;
  background: linear-gradient(135deg, rgba(231, 94, 141, 0.2), rgba(116, 192, 252, 0.2));
  border: 1px solid rgba(231, 94, 141, 0.4);
  padding: 0.75rem 1.5rem;
  border-radius: 25px;
  animation: fadeInScale 0.5s ease-out;
}

@keyframes fadeInScale {
  from {
    opacity: 0;
    transform: scale(0.9);
  }
  to {
    opacity: 1;
    transform: scale(1);
  }
}

.word-text {
  color: #e75e8d;
  font-size: 1.2rem;
  font-weight: bold;
}

.word-part-of-speech {
  color: #ffc107;
  font-size: 0.85rem;
  font-style: italic;
  padding: 0 0.5rem;
}

.word-meaning {
  color: #74c0fc;
  font-size: 0.95rem;
  font-style: italic;
  padding-left: 0.5rem;
  border-left: 1px solid rgba(255, 255, 255, 0.3);
}

.audio-player-section {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  align-items: center;
}

.audio-controls-wrapper {
  display: flex;
  align-items: center;
  gap: 1.5rem;
  flex-wrap: wrap;
  justify-content: center;
}

.speed-control {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.speed-label {
  display: flex;
  align-items: center;
  gap: 0.25rem;
  color: #74c0fc;
  font-size: 0.9rem;
  font-weight: 500;
}

.speed-select {
  background: rgba(116, 192, 252, 0.1);
  border: 2px solid rgba(116, 192, 252, 0.3);
  color: white;
  padding: 0.5rem 0.75rem;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.3s ease;
  font-size: 0.9rem;
  font-weight: 500;
  outline: none;
}

.speed-select:hover {
  background: rgba(116, 192, 252, 0.2);
  border-color: rgba(116, 192, 252, 0.5);
}

.speed-select:focus {
  border-color: #74c0fc;
  box-shadow: 0 0 10px rgba(116, 192, 252, 0.3);
}

.speed-select option {
  background: #1a1a2e;
  color: white;
}

.playback-controls {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  align-items: center;
}

.playback-controls label {
  color: #74c0fc;
  font-size: 0.9rem;
  font-weight: 500;
}

.speed-buttons {
  display: flex;
  gap: 0.5rem;
}

.speed-buttons button {
  background: rgba(116, 192, 252, 0.2);
  border: 2px solid rgba(116, 192, 252, 0.5);
  color: #74c0fc;
  padding: 0.4rem 0.8rem;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.3s ease;
  font-size: 0.85rem;
  font-weight: 500;
}

.speed-buttons button:hover {
  background: rgba(116, 192, 252, 0.3);
  border-color: #74c0fc;
  transform: translateY(-2px);
}

.speed-buttons button.active {
  background: rgba(116, 192, 252, 0.4);
  border-color: #74c0fc;
  box-shadow: 0 0 10px rgba(116, 192, 252, 0.3);
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
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.5rem;
}

.keyboard-hint {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.85rem;
  color: #74c0fc;
  font-style: normal;
  transition: all 0.3s ease;
}

.keyboard-hint kbd {
  background: linear-gradient(135deg, rgba(116, 192, 252, 0.2), rgba(51, 154, 240, 0.2));
  border: 1px solid rgba(116, 192, 252, 0.5);
  border-radius: 5px;
  padding: 0.25rem 0.5rem;
  font-family: monospace;
  font-size: 0.9rem;
  font-weight: 600;
  color: #74c0fc;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
  transition: all 0.3s ease;
}

.keyboard-hint-active {
  animation: pulseHint 0.6s ease;
}

.keyboard-hint-active kbd {
  animation: pressKbd 0.3s ease;
}

@keyframes pulseHint {
  0%, 100% {
    transform: scale(1);
  }
  50% {
    transform: scale(1.05);
  }
}

@keyframes pressKbd {
  0% {
    transform: translateY(0);
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
  }
  50% {
    transform: translateY(2px);
    box-shadow: 0 1px 2px rgba(0, 0, 0, 0.2);
    background: linear-gradient(135deg, rgba(116, 192, 252, 0.4), rgba(51, 154, 240, 0.4));
    border-color: rgba(116, 192, 252, 0.8);
  }
  100% {
    transform: translateY(0);
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
  }
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

/* Input Section Styles */
.input-section {
  margin-bottom: 1.5rem;
}

.input-container {
  margin-bottom: 0;
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

.header-controls {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.voice-icon-btn {
  background: rgba(231, 94, 141, 0.2);
  color: #e75e8d;
  border: 2px solid rgba(231, 94, 141, 0.5);
  border-radius: 50%;
  width: 40px;
  height: 40px;
  min-width: 40px;
  min-height: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all 0.3s ease;
  padding: 0;
  flex-shrink: 0;
}

.voice-icon-btn .icon-size {
  font-size: 1.3rem;
  width: 1.3rem;
  height: 1.3rem;
}

.voice-icon-btn:hover:not(:disabled) {
  background: rgba(231, 94, 141, 0.3);
  border-color: #e75e8d;
  transform: scale(1.1);
}

.voice-icon-btn.recording {
  background: rgba(231, 94, 141, 0.4);
  border-color: #e75e8d;
  animation: recordingPulse 2s infinite;
  box-shadow: 0 0 15px rgba(231, 94, 141, 0.5);
}

.voice-icon-btn.disabled {
  opacity: 0.5;
  cursor: not-allowed;
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
  font-size: 1.2rem;
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

.recording-indicator {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  color: #e75e8d;
  margin-top: 1rem;
  justify-content: center;
}

.pulse {
  width: 12px;
  height: 12px;
  background: #e75e8d;
  border-radius: 50%;
  animation: pulse 1.5s infinite;
}

/* Action Buttons Styles */
.action-buttons {
  display: flex;
  justify-content: center;
  gap: 1rem;
  flex-wrap: wrap;
  margin-top: 1.5rem;
}

.check-btn, .submit-btn, .clear-btn, .hint-btn {
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

.check-btn {
  background: linear-gradient(135deg, #3b82f6, #60a5fa);
  color: white;
}

.check-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.check-btn:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 5px 15px rgba(59, 130, 246, 0.4);
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
.back-to-grid-btn{
    background: rgba(255, 255, 255, 0.1);
    border: 1px solid rgba(255, 255, 255, 0.2);
    color: white;
    padding: 0.75rem 1.5rem;
    border-radius: 25px;
    cursor: pointer;
    display: inline-flex;
    align-items: center;
    gap: 0.5rem;
    font-size: 1rem;
    transition: all 0.3s ease;
}
.back-to-grid-btn:hover {
    background: rgba(255, 255, 255, 0.2);
    transform: translateY(-2px);
    box-shadow: 0 5px 15px rgba(0, 0, 0, 0.3);
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

@media (max-width: 768px) {
  .card-container {
    padding: 1.5rem;
  }
  
  .dictation-header h2 {
    font-size: 1.5rem;
  }
  
  .action-buttons {
    flex-direction: column;
    align-items: center;
  }
}
</style>