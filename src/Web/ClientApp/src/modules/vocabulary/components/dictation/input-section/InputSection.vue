<template>
  <div class="input-section">
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
          :value="userInput"
          @input="$emit('update:user-input', ($event.target as HTMLTextAreaElement).value)"
          @keydown.enter.prevent="handleEnterKey"
          :placeholder="isRecording ? 'Listening...' : 'Start typing or use voice input (Press Enter to check)'"
          :disabled="isRecording"
          class="dictation-input"
          rows="3"
        ></textarea>
        
        <!-- Voice Input Controls -->
        <div class="voice-controls">
          <button 
            @click="$emit('toggle-recording')"
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
        @click="$emit('check')" 
        :disabled="!userInput.trim()"
        class="check-btn"
      >
        <i class="mdi mdi-spellcheck"></i>
        Check
      </button>
      
      <button 
        @click="$emit('submit')" 
        :disabled="!userInput.trim()"
        class="submit-btn"
      >
        <i class="mdi mdi-send"></i>
        Submit
      </button>
      
      <button @click="$emit('clear')" class="clear-btn" v-if="userInput">
        <i class="mdi mdi-eraser"></i>
        Clear
      </button>
      
      <button @click="$emit('show-hint')" class="hint-btn">
        <i class="mdi mdi-lightbulb"></i>
        Hint
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
interface Props {
  userInput: string
  isRecording: boolean
  elapsedTime: number
  speechRecognitionSupported: boolean
}

defineProps<Props>()

const emit = defineEmits<{
  'update:user-input': [value: string]
  'toggle-recording': []
  submit: []
  check: []
  clear: []
  'show-hint': []
}>()

const handleEnterKey = () => {
  // Emit check event when Enter is pressed
  emit('check')
}

const formatTime = (seconds: number): string => {
  const mins = Math.floor(seconds / 60)
  const secs = seconds % 60
  return `${mins}:${secs.toString().padStart(2, '0')}`
}
</script>

<style scoped>
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
  .action-buttons {
    flex-direction: column;
    align-items: center;
  }
}
</style>
