<template>
  <div class="test-tts">
    <h3>Test Multi-Provider TTS Integration</h3>
    
    <!-- TTS Provider Selection -->
    <div class="provider-selection">
      <label>TTS Provider:</label>
      <select v-model="selectedProvider" class="provider-select">
        <option value="openai">OpenAI TTS (ChatGPT)</option>
        <option value="webspeech">Web Speech API</option>
      </select>
    </div>

    <!-- OpenAI API Key Input -->
    <div v-if="selectedProvider === 'openai'" class="api-key-section">
      <div class="api-key-input">
        <label>OpenAI API Key:</label>
        <input 
          v-model="openaiApiKey" 
          type="password"
          placeholder="Enter your OpenAI API key"
          class="api-key-field"
        />
        <button @click="saveApiKey" class="save-key-btn">
          <Icon icon="mdi:content-save" />
          Save
        </button>
      </div>
      <div class="api-key-note">
        💡 Enter OpenAI API key to use ChatGPT TTS. Key will be saved in localStorage.
      </div>
    </div>
    
    <!-- TTS System Status -->
    <div class="tts-status">
      <div class="status-indicator">
        <Icon :icon="getProviderIcon()" />
        <span>Current Provider: {{ getProviderName() }}</span>
      </div>
    </div>
    
    <div class="test-controls">
      <input 
        v-model="testText" 
        placeholder="Enter text to speak"
        class="test-input"
      />
      <button 
        @click="handleSpeak" 
        :disabled="isPlaying('test')"
        class="test-button"
      >
        <Icon 
          :icon="isPlaying('test') ? 'mdi:stop' : 'mdi:volume-high'" 
          :class="{ 'text-red-500': isPlaying('test') }"
        />
        {{ isPlaying('test') ? 'Playing...' : 'Speak' }}
      </button>
      <button 
        @click="handleStop"
        class="test-button"
      >
        <Icon icon="mdi:stop" />
        Stop
      </button>
    </div>
    
    <div class="voice-selection">
      <label>Voice:</label>
      <select v-model="selectedVoiceIndex" class="voice-select">
        <option 
          v-for="(voice, index) in getFilteredVoices()"
          :key="voice.voiceName"
          :value="index"
        >
          {{ voice.name }}
        </option>
      </select>
    </div>

    <div class="controls">
      <label>Rate: {{ speechRate }}</label>
      <input 
        v-model.number="speechRate" 
        type="range" 
        min="0.1" 
        max="2" 
        step="0.1"
        class="range-input"
      />
      
      <label>Pitch: {{ speechPitch }}</label>
      <input 
        v-model.number="speechPitch" 
        type="range" 
        min="0.1" 
        max="2" 
        step="0.1"
        class="range-input"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'
import { Icon } from '@iconify/vue'
import { useSpeechSynthesis } from '@/composables/useSpeechSynthesis'

const testText = ref('Hello! This is a test of the multi-provider TTS system using ChatGPT, Edge, and Web Speech API!')
const speechRate = ref(0.8)
const speechPitch = ref(1.0)
const selectedVoiceIndex = ref(0)
const selectedProvider = ref<'openai' | 'webspeech'>('webspeech')
const openaiApiKey = ref(localStorage.getItem('openai_api_key') || '')

const { speak, isPlaying, stop, getAllVoices, initializeVoices } = useSpeechSynthesis()

// Get filtered voices based on selected provider
const getFilteredVoices = () => {
  const allVoices = getAllVoices()
  return allVoices.filter(voice => voice.provider === selectedProvider.value)
}

// Reset voice selection when provider changes
watch(selectedProvider, () => {
  selectedVoiceIndex.value = 0
})

const getProviderIcon = () => {
  switch (selectedProvider.value) {
    case 'openai': return 'simple-icons:openai'
    case 'webspeech': return 'mdi:web'
    default: return 'mdi:volume-high'
  }
}

const getProviderName = () => {
  switch (selectedProvider.value) {
    case 'openai': return 'OpenAI TTS (ChatGPT)'
    case 'webspeech': return 'Web Speech API'
    default: return 'Unknown'
  }
}

const saveApiKey = () => {
  localStorage.setItem('openai_api_key', openaiApiKey.value)
  alert('API key saved successfully!')
}

const handleSpeak = async () => {
  if (isPlaying('test')) return
  
  // Check for OpenAI API key if needed
  if (selectedProvider.value === 'openai' && !openaiApiKey.value) {
    alert('Please enter your OpenAI API key first!')
    return
  }
  
  try {
    const allVoices = getAllVoices()
    const filteredVoices = getFilteredVoices()
    
    // Find the actual index in the combined voice list
    let actualVoiceIndex = selectedVoiceIndex.value
    if (filteredVoices.length > 0) {
      const selectedVoice = filteredVoices[selectedVoiceIndex.value]
      actualVoiceIndex = allVoices.findIndex(voice => 
        voice.name === selectedVoice.name && voice.provider === selectedVoice.provider
      )
    }

    await speak(testText.value, 'test', {
      rate: speechRate.value,
      pitch: speechPitch.value,
      voiceIndex: actualVoiceIndex >= 0 ? actualVoiceIndex : 0,
      provider: selectedProvider.value
    })
  } catch (error) {
    console.error('Failed to play audio:', error)
    alert('Failed to play audio: ' + error)
  }
}

const handleStop = () => {
  stop('test')
}

// Initialize voices when component mounts
onMounted(async () => {
  await initializeVoices()
  console.log('Voices initialized:', getAllVoices().length)
})
</script>

<style scoped>
.test-tts {
  max-width: 700px;
  margin: 2rem auto;
  padding: 2rem;
  border: 2px solid #e5e5e5;
  border-radius: 12px;
  background: #f9f9f9;
}

.provider-selection {
  margin-bottom: 1rem;
  padding: 1rem;
  background: #fff3cd;
  border: 1px solid #ffeaa7;
  border-radius: 8px;
}

.provider-select {
  margin-left: 0.5rem;
  padding: 0.5rem;
  border: 1px solid #ccc;
  border-radius: 4px;
  font-weight: 500;
}

.api-key-section {
  margin-bottom: 1rem;
  padding: 1rem;
  background: #d1ecf1;
  border: 1px solid #bee5eb;
  border-radius: 8px;
}

.api-key-input {
  display: flex;
  gap: 0.5rem;
  align-items: center;
  margin-bottom: 0.5rem;
}

.api-key-field {
  flex: 1;
  padding: 0.5rem;
  border: 1px solid #ccc;
  border-radius: 4px;
}

.save-key-btn {
  padding: 0.5rem 1rem;
  border: 1px solid #007bff;
  border-radius: 4px;
  background: #007bff;
  color: white;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.save-key-btn:hover {
  background: #0056b3;
}

.api-key-note {
  font-size: 0.875rem;
  color: #0c5460;
}

.tts-status {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1rem;
  padding: 1rem;
  background: #e8f4f8;
  border-radius: 8px;
  border: 1px solid #b3d7e8;
}

.status-indicator {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-weight: 500;
}

.test-controls {
  display: flex;
  gap: 1rem;
  margin-bottom: 1rem;
  align-items: center;
}

.test-input {
  flex: 1;
  padding: 0.5rem;
  border: 1px solid #ccc;
  border-radius: 4px;
}

.test-button {
  padding: 0.5rem 1rem;
  border: 1px solid #007bff;
  border-radius: 4px;
  background: #007bff;
  color: white;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.test-button:hover {
  background: #0056b3;
}

.test-button:disabled {
  background: #6c757d;
  cursor: not-allowed;
}

.voice-selection {
  margin-bottom: 1rem;
}

.voice-select {
  margin-left: 0.5rem;
  padding: 0.25rem;
  border: 1px solid #ccc;
  border-radius: 4px;
}

.controls {
  display: grid;
  grid-template-columns: auto 1fr;
  gap: 0.5rem;
  align-items: center;
}

.range-input {
  width: 100%;
}

label {
  font-weight: 500;
}

.text-red-500 {
  color: #dc3545;
}
</style>