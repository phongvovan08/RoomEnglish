<template>
  <div v-if="showPanel" class="speech-settings-panel">
    <div class="panel-header">
      <h3>🔊 Speech Settings</h3>
      <button @click="$emit('close')" class="close-btn">
        <Icon icon="mdi:close" class="w-4 h-4" />
      </button>
    </div>

    <!-- TTS Provider Selection -->
    <div class="setting-group">
      <label class="setting-label">TTS Provider:</label>
      <select :value="currentTTSProvider" @change="updateTTSProvider" class="provider-select">
        <option value="openai">OpenAI TTS (ChatGPT) 🤖</option>
        <option value="webspeech">Web Speech API 🗣️</option>
      </select>
    </div>

    <!-- OpenAI API Key -->
    <div v-if="currentTTSProvider === 'openai'" class="setting-group api-key-group">
      <label class="setting-label">OpenAI API Key:</label>
      <div class="api-key-input">
        <input 
          v-model="openaiApiKey" 
          type="password"
          placeholder="sk-..."
          class="api-key-field"
        />
        <button @click="saveApiKey" class="save-key-btn" :class="{ success: apiKeySaved }">
          <Icon :icon="apiKeySaved ? 'mdi:check' : 'mdi:content-save'" />
        </button>
      </div>
      <div class="api-key-note">
        💡 API key required to use ChatGPT TTS
      </div>
    </div>

    <!-- Speed Control -->
    <div class="setting-group">
      <label class="setting-label">Speed: {{ currentRate }}x</label>
      <input 
        :value="currentRate"
        @input="updateRate"
        type="range" 
        min="0.5" 
        max="2" 
        step="0.1"
        class="speed-slider"
      />
      <div class="speed-presets">
        <button @click="setSpeechRate(0.5)" class="preset-btn">Slow</button>
        <button @click="setSpeechRate(0.8)" class="preset-btn" :class="{ active: currentRate === 0.8 }">Normal</button>
        <button @click="setSpeechRate(1.0)" class="preset-btn">Fast</button>
      </div>
    </div>

    <!-- Voice Selection -->
    <div class="setting-group" v-if="shouldShowVoiceSelection">
      <label class="setting-label">Voice:</label>
      <select :value="currentProviderVoiceIndex" @change="updateVoice" class="voice-select">
        <option 
          v-for="(voice, index) in englishVoices" 
          :key="index" 
          :value="index"
        >
          {{ voice.name }} ({{ voice.lang }})
        </option>
      </select>
    </div>

    <!-- Message when OpenAI voices are hidden due to missing API key -->
    <div v-if="currentTTSProvider === 'openai' && openaiApiKey.trim().length === 0" class="setting-group">
      <div class="api-key-warning">
        ⚠️ Enter API key to view OpenAI voice list
      </div>
    </div>

    <!-- Pitch Control -->
    <div class="setting-group">
      <label class="setting-label">Pitch: {{ currentPitch }}</label>
      <input 
        :value="currentPitch"
        @input="updatePitch"
        type="range" 
        min="0.5" 
        max="2" 
        step="0.1"
        class="pitch-slider"
      />
    </div>

    <!-- Test Button -->
    <div class="setting-group">
      <button @click="testSpeech" class="test-btn" :disabled="isTesting">
        <Icon :icon="isTesting ? 'mdi:volume-variant-off' : 'mdi:volume-high'" class="w-4 h-4" />
        {{ isTesting ? 'Playing...' : 'Test Speech' }}
      </button>
    </div>

    <!-- Reset Button -->
    <div class="setting-group">
      <button @click="resetToDefault" class="reset-btn">
        <Icon icon="mdi:restore" class="w-4 h-4" />
        Reset to Default
      </button>
    </div>

    <!-- Audio Cache Management -->
    <div class="setting-group cache-group">
      <label class="setting-label">Audio Cache:</label>
      <div class="cache-stats">
        <div class="stat-item">
          <Icon icon="mdi:memory" class="w-4 h-4" />
          <span>Memory: {{ cacheStats.memory.count }} / {{ cacheStats.memory.maxSize }} items ({{ formatBytes(cacheStats.memory.size) }})</span>
        </div>
        <div class="stat-item">
          <Icon icon="mdi:clock-outline" class="w-4 h-4" />
          <span>TTL: {{ Math.floor(cacheStats.memory.ttl / 60000) }} minutes</span>
        </div>
        <div class="stat-item" v-if="cacheStats.memory.oldestEntryAge > 0">
          <Icon icon="mdi:history" class="w-4 h-4" />
          <span>Oldest: {{ Math.floor(cacheStats.memory.oldestEntryAge / 1000) }}s ago</span>
        </div>
      </div>
      <button @click="handleClearCache" class="clear-cache-btn">
        <Icon icon="mdi:delete-sweep" class="w-4 h-4" />
        Clear Audio Cache
      </button>
      <div class="cache-note">
        💡 Memory cache auto-cleans expired entries every 2 minutes
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { Icon } from '@iconify/vue'
import { useSpeechSettings } from '@/composables/useSpeechSettings'
import { useSpeechSynthesis } from '@/composables/useSpeechSynthesis'

interface Props {
  showPanel: boolean
}

const props = defineProps<Props>()
defineEmits<{
  close: []
}>()

const {
  speechRate,
  speechPitch,
  selectedVoiceIndex,
  selectedTTSProvider,
  getAvailableVoices,
  getEnglishVoices,
  getVoicesByProvider,
  getCurrentOptions,
  setSpeechRate,
  setSpeechPitch,
  setSelectedVoiceIndex,
  setTTSProvider,
  initializeDefaultVoice,
  loadSettings
} = useSpeechSettings()

const { speak, isPlaying, getCacheStats, clearCache } = useSpeechSynthesis()

// API Key management
const openaiApiKey = ref(localStorage.getItem('openai_api_key') || '')
const apiKeySaved = ref(false)

// Cache stats
const cacheStats = ref({
  memory: { count: 0, size: 0, maxSize: 100, ttl: 0, oldestEntryAge: 0 }
})

// Computed values
const currentRate = computed(() => speechRate.value)
const currentPitch = computed(() => speechPitch.value)
const currentVoiceIndex = computed(() => selectedVoiceIndex.value)
const currentTTSProvider = computed(() => selectedTTSProvider.value)
const englishVoices = computed(() => {
  // Only show voices for current provider
  return getVoicesByProvider(currentTTSProvider.value)
})

// Get the provider-specific voice index for display
const currentProviderVoiceIndex = computed(() => {
  const allVoices = getAvailableVoices()
  const selectedVoice = allVoices[selectedVoiceIndex.value]
  
  console.log('Current voice selection:', {
    globalIndex: selectedVoiceIndex.value,
    selectedVoice: selectedVoice?.name,
    provider: currentTTSProvider.value
  })
  
  if (!selectedVoice || selectedVoice.provider !== currentTTSProvider.value) {
    console.log('Voice provider mismatch, defaulting to 0')
    return 0 // Default to first voice if current selection doesn't match provider
  }
  
  const providerVoices = englishVoices.value
  const providerIndex = providerVoices.findIndex(voice => voice.voiceName === selectedVoice.voiceName)
  
  console.log('Provider voice index:', providerIndex)
  return providerIndex >= 0 ? providerIndex : 0
})

const isTesting = computed(() => isPlaying('speech-test'))

// Computed to check whether voice selection should be displayed
const shouldShowVoiceSelection = computed(() => {
  const voices = englishVoices.value
  if (voices.length === 0) return false
  
  // If provider is OpenAI, check if API key exists
  if (currentTTSProvider.value === 'openai') {
    return openaiApiKey.value.trim().length > 0
  }
  
  // If Web Speech API, always show
  return true
})

// Methods
const updateRate = (event: Event) => {
  const target = event.target as HTMLInputElement
  setSpeechRate(parseFloat(target.value))
}

const updatePitch = (event: Event) => {
  const target = event.target as HTMLInputElement
  setSpeechPitch(parseFloat(target.value))
}

const updateVoice = (event: Event) => {
  const target = event.target as HTMLSelectElement
  const providerVoiceIndex = parseInt(target.value)
  
  // Convert provider-specific index to global index
  const providerVoices = englishVoices.value
  const selectedProviderVoice = providerVoices[providerVoiceIndex]
  
  if (selectedProviderVoice) {
    const allVoices = getAvailableVoices()
    const globalIndex = allVoices.findIndex(voice => 
      voice.voiceName === selectedProviderVoice.voiceName && 
      voice.provider === selectedProviderVoice.provider
    )
    
    if (globalIndex >= 0) {
      setSelectedVoiceIndex(globalIndex)
    }
  }
}

const testSpeech = async () => {
  if (isTesting.value) return
  
  try {
    const options = await getCurrentOptions()
    await speak('Hello! This is a test of your speech settings.', 'speech-test', options)
  } catch (error) {
    console.error('Failed to test speech:', error)
  }
}

const resetToDefault = () => {
  setSpeechRate(0.8)
  setSpeechPitch(1.0)
  setSelectedVoiceIndex(0)
}

// TTS Provider methods
const updateTTSProvider = (event: Event) => {
  const target = event.target as HTMLSelectElement
  const provider = target.value as 'openai' | 'webspeech'
  setTTSProvider(provider)
  
  // Reset to first voice of new provider
  const providerVoices = getVoicesByProvider(provider)
  if (providerVoices.length > 0) {
    const allVoices = getAvailableVoices()
    const firstProviderVoice = providerVoices[0]
    const globalIndex = allVoices.findIndex(voice => 
      voice.voiceName === firstProviderVoice.voiceName && 
      voice.provider === firstProviderVoice.provider
    )
    
    if (globalIndex >= 0) {
      setSelectedVoiceIndex(globalIndex)
    }
  }
}

// API Key methods
const saveApiKey = () => {
  localStorage.setItem('openai_api_key', openaiApiKey.value)
  apiKeySaved.value = true
  setTimeout(() => {
    apiKeySaved.value = false
  }, 2000)
}

// Cache management
const updateCacheStats = () => {
  const stats = getCacheStats()
  console.log('📊 Cache stats from API:', stats)
  cacheStats.value = stats
}

const handleClearCache = () => {
  clearCache()
  updateCacheStats()
}

const formatBytes = (bytes: number): string => {
  if (bytes === 0) return '0 B'
  const k = 1024
  const sizes = ['B', 'KB', 'MB', 'GB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  return `${(bytes / Math.pow(k, i)).toFixed(2)} ${sizes[i]}`
}

onMounted(async () => {
  // Load voices first, then load settings
  await initializeDefaultVoice()
  loadSettings()
  updateCacheStats()
})

// Watch for panel opening to refresh cache stats
watch(() => props.showPanel, (isOpen) => {
  if (isOpen) {
    console.log('🔄 Panel opened, refreshing cache stats...')
    updateCacheStats()
  }
})
</script>

<style scoped>
.speech-settings-panel {
  position: fixed;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  background: linear-gradient(135deg, rgba(0, 0, 0, 0.95), rgba(30, 30, 30, 0.95));
  border: 2px solid rgba(231, 94, 141, 0.5);
  border-radius: 20px;
  padding: 2rem;
  min-width: 400px;
  max-width: 500px;
  z-index: 2000;
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.5);
  backdrop-filter: blur(10px);
}

.panel-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
  padding-bottom: 1rem;
  border-bottom: 1px solid rgba(255, 255, 255, 0.2);
}

.panel-header h3 {
  color: #e75e8d;
  font-size: 1.5rem;
  font-weight: bold;
  margin: 0;
  background: linear-gradient(135deg, #e75e8d, #74c0fc);
  background-clip: text;
  -webkit-background-clip: text;
  color: transparent;
}

.close-btn {
  background: rgba(255, 255, 255, 0.1);
  border: 1px solid rgba(255, 255, 255, 0.3);
  border-radius: 50%;
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  color: white;
  transition: all 0.3s ease;
}

.close-btn:hover {
  background: rgba(255, 255, 255, 0.2);
  transform: scale(1.1);
}

.setting-group {
  margin-bottom: 1.5rem;
}

.setting-group:last-child {
  margin-bottom: 0;
}

.setting-label {
  display: block;
  color: white;
  font-size: 1rem;
  margin-bottom: 0.75rem;
  font-weight: 500;
}

.speed-slider,
.pitch-slider {
  width: 100%;
  margin-bottom: 0.75rem;
  accent-color: #e75e8d;
}

.speed-presets {
  display: flex;
  gap: 0.75rem;
}

.preset-btn {
  background: rgba(231, 94, 141, 0.3);
  border: 1px solid rgba(231, 94, 141, 0.5);
  color: #e75e8d;
  padding: 0.5rem 1rem;
  border-radius: 20px;
  font-size: 0.875rem;
  cursor: pointer;
  transition: all 0.3s ease;
  flex: 1;
}

.preset-btn:hover {
  background: rgba(231, 94, 141, 0.5);
}

.preset-btn.active {
  background: linear-gradient(135deg, #e75e8d, #74c0fc);
  color: white;
  border-color: transparent;
}

.voice-select {
  width: 100%;
  background: rgba(255, 255, 255, 0.1);
  border: 1px solid rgba(255, 255, 255, 0.3);
  border-radius: 10px;
  padding: 0.75rem;
  color: white;
  font-size: 1rem;
}

.voice-select option {
  background: #1a1a1a;
  color: white;
}

.test-btn,
.reset-btn {
  background: linear-gradient(135deg, #74c0fc, #e75e8d);
  color: white;
  border: none;
  padding: 0.75rem 1.5rem;
  border-radius: 25px;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 1rem;
  transition: all 0.3s ease;
  width: 100%;
  justify-content: center;
}

.test-btn:hover:not(:disabled),
.reset-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 5px 15px rgba(116, 192, 252, 0.4);
}

.test-btn:disabled {
  opacity: 0.7;
  cursor: not-allowed;
  animation: pulse 1.5s infinite;
}

.reset-btn {
  background: rgba(255, 255, 255, 0.1);
  border: 1px solid rgba(255, 255, 255, 0.3);
}

.reset-btn:hover {
  background: rgba(255, 255, 255, 0.2);
}

@keyframes pulse {
  0%, 100% { opacity: 0.7; }
  50% { opacity: 1; }
}

/* Provider Selection */
.provider-select {
  width: 100%;
  background: rgba(255, 255, 255, 0.1);
  border: 1px solid rgba(255, 255, 255, 0.3);
  border-radius: 10px;
  padding: 0.75rem;
  color: white;
  font-size: 1rem;
}

.provider-select option {
  background: #1a1a1a;
  color: white;
}

/* API Key Section */
.api-key-group {
  background: rgba(0, 123, 255, 0.1);
  border: 1px solid rgba(0, 123, 255, 0.3);
  border-radius: 15px;
  padding: 1rem;
}

.api-key-input {
  display: flex;
  gap: 0.5rem;
  align-items: center;
}

.api-key-field {
  flex: 1;
  background: rgba(255, 255, 255, 0.1);
  border: 1px solid rgba(255, 255, 255, 0.3);
  border-radius: 8px;
  padding: 0.5rem;
  color: white;
  font-size: 0.9rem;
}

.api-key-field::placeholder {
  color: rgba(255, 255, 255, 0.5);
}

.save-key-btn {
  background: rgba(40, 167, 69, 0.8);
  border: 1px solid rgba(40, 167, 69, 0.5);
  border-radius: 8px;
  padding: 0.5rem;
  color: white;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  min-width: 40px;
  transition: all 0.3s ease;
}

.save-key-btn:hover {
  background: rgba(40, 167, 69, 1);
  transform: scale(1.05);
}

.save-key-btn.success {
  background: rgba(40, 167, 69, 1);
  animation: successPulse 0.6s ease-out;
}

@keyframes successPulse {
  0% { transform: scale(1); }
  50% { transform: scale(1.2); }
  100% { transform: scale(1); }
}

.api-key-warning {
  background: rgba(255, 193, 7, 0.1);
  border: 1px solid rgba(255, 193, 7, 0.3);
  border-radius: 8px;
  padding: 0.75rem;
  color: #856404;
  font-size: 0.9rem;
  text-align: center;
}

.api-key-note {
  font-size: 0.8rem;
  color: rgba(255, 255, 255, 0.7);
  margin-top: 0.5rem;
  text-align: center;
}

/* Cache Management */
.cache-group {
  background: rgba(116, 192, 252, 0.05);
  border: 1px solid rgba(116, 192, 252, 0.2);
  border-radius: 12px;
  padding: 1rem !important;
}

.cache-stats {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  margin-bottom: 1rem;
}

.cache-stats .stat-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  color: rgba(255, 255, 255, 0.9);
  font-size: 0.9rem;
  padding: 0.5rem;
  background: rgba(255, 255, 255, 0.05);
  border-radius: 6px;
}

.cache-stats .stat-item svg {
  color: #74c0fc;
}

.clear-cache-btn {
  width: 100%;
  background: linear-gradient(135deg, #e75e8d, #74c0fc);
  border: none;
  border-radius: 10px;
  padding: 0.75rem 1rem;
  color: white;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  font-size: 1rem;
  font-weight: 600;
  transition: all 0.3s ease;
  margin-bottom: 0.5rem;
}

.clear-cache-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 5px 15px rgba(231, 94, 141, 0.4);
}

.cache-note {
  font-size: 0.8rem;
  color: rgba(255, 255, 255, 0.7);
  text-align: center;
}
</style>