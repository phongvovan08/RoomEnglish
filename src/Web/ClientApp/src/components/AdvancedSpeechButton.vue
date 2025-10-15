<template>
  <div class="advanced-speech-controls" v-if="isSupported">
    <!-- Speech Button -->
    <button 
      @click="handlePlay"
      :disabled="isPlaying"
      :class="buttonClass"
      :title="isPlaying ? 'Playing...' : 'Click to listen'"
    >
      <Icon :icon="isPlaying ? 'mdi:volume-variant-off' : 'mdi:volume-high'" class="w-5 h-5" />
      <span v-if="showText">{{ isPlaying ? 'Playing...' : 'Listen' }}</span>
    </button>

    <!-- Settings Button -->
    <button 
      v-if="showSettings"
      @click="toggleSettings"
      class="settings-btn"
      :title="showSettingsPanel ? 'Hide settings' : 'Show settings'"
    >
      <Icon icon="mdi:cog" class="w-4 h-4" />
    </button>

    <!-- Settings Panel -->
    <div v-if="showSettingsPanel" class="settings-panel">
      <!-- Speed Control -->
      <div class="setting-group">
        <label class="setting-label">Speed: {{ speechRate }}x</label>
        <input 
          v-model="speechRate" 
          type="range" 
          min="0.5" 
          max="2" 
          step="0.1"
          class="speed-slider"
        />
        <div class="speed-presets">
          <button @click="speechRate = 0.5" class="preset-btn">Slow</button>
          <button @click="speechRate = 0.8" class="preset-btn">Normal</button>
          <button @click="speechRate = 1.0" class="preset-btn">Fast</button>
        </div>
      </div>

      <!-- Voice Selection -->
      <div class="setting-group" v-if="availableVoices.length > 0">
        <label class="setting-label">Voice:</label>
        <select v-model="selectedVoiceIndex" class="voice-select">
          <option 
            v-for="(voice, index) in englishVoices" 
            :key="index" 
            :value="index"
          >
            {{ voice.name }} ({{ voice.lang }})
          </option>
        </select>
      </div>

      <!-- Pitch Control -->
      <div class="setting-group">
        <label class="setting-label">Pitch: {{ speechPitch }}</label>
        <input 
          v-model="speechPitch" 
          type="range" 
          min="0.5" 
          max="2" 
          step="0.1"
          class="pitch-slider"
        />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { Icon } from '@iconify/vue'
import { useSpeechSynthesis } from '@/composables/useSpeechSynthesis'

interface Props {
  text: string
  instanceId: string
  lang?: string
  showText?: boolean
  buttonClass?: string
  showSettings?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  lang: 'en-US',
  showText: false,
  buttonClass: 'speech-btn',
  showSettings: true
})

const { speak, isPlaying: checkIsPlaying, isSupported, loadVoices } = useSpeechSynthesis()

// State
const isPlaying = computed(() => checkIsPlaying(props.instanceId))
const showSettingsPanel = ref(false)
const availableVoices = ref<SpeechSynthesisVoice[]>([])
const speechRate = ref(0.8)
const speechPitch = ref(1.0)
const selectedVoiceIndex = ref(0)

// Computed
const englishVoices = computed(() => {
  return availableVoices.value.filter(voice => 
    voice.lang.toLowerCase().startsWith('en')
  )
})

// Methods
const toggleSettings = () => {
  showSettingsPanel.value = !showSettingsPanel.value
}

const handlePlay = async () => {
  if (isPlaying.value) return
  
  try {
    const selectedVoice = englishVoices.value[selectedVoiceIndex.value]
    const options = {
      lang: props.lang,
      rate: speechRate.value,
      pitch: speechPitch.value,
      voiceIndex: selectedVoice 
        ? availableVoices.value.indexOf(selectedVoice)
        : undefined
    }
    
    await speak(props.text, props.instanceId, options)
  } catch (error) {
    console.error('Failed to play audio:', error)
  }
}

// Load voices on mount
onMounted(async () => {
  try {
    availableVoices.value = await loadVoices()
    // Find default English voice
    const defaultEnglishIndex = englishVoices.value.findIndex(voice => 
      voice.default || voice.localService
    )
    if (defaultEnglishIndex >= 0) {
      selectedVoiceIndex.value = defaultEnglishIndex
    }
  } catch (error) {
    console.error('Failed to load voices:', error)
  }
})
</script>

<style scoped>
.advanced-speech-controls {
  display: flex;
  align-items: flex-start;
  gap: 0.5rem;
  position: relative;
}

.speech-btn {
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
  padding: 0;
}

.speech-btn :deep(.iconify) {
  width: 18px !important;
  height: 18px !important;
}

.speech-btn:hover:not(:disabled) {
  background: rgba(231, 94, 141, 0.5);
  transform: scale(1.1);
}

.speech-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
  animation: pulse 1.5s infinite;
}

.speech-btn.large {
  background: linear-gradient(135deg, #e75e8d, #74c0fc);
  color: white;
  border: none;
  padding: 0.75rem 1.5rem;
  border-radius: 25px;
  width: auto;
  height: auto;
  gap: 0.5rem;
  font-size: 1rem;
}

.speech-btn.large :deep(.iconify) {
  width: 20px !important;
  height: 20px !important;
}

.settings-btn {
  background: rgba(116, 192, 252, 0.3);
  border: 1px solid rgba(116, 192, 252, 0.5);
  border-radius: 50%;
  width: 30px;
  height: 30px;
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

.settings-panel {
  position: absolute;
  top: 100%;
  left: 0;
  background: rgba(0, 0, 0, 0.9);
  border: 1px solid rgba(255, 255, 255, 0.2);
  border-radius: 12px;
  padding: 1rem;
  min-width: 280px;
  z-index: 1000;
  margin-top: 0.5rem;
}

.setting-group {
  margin-bottom: 1rem;
}

.setting-group:last-child {
  margin-bottom: 0;
}

.setting-label {
  display: block;
  color: white;
  font-size: 0.875rem;
  margin-bottom: 0.5rem;
  font-weight: 500;
}

.speed-slider,
.pitch-slider {
  width: 100%;
  margin-bottom: 0.5rem;
}

.speed-presets {
  display: flex;
  gap: 0.5rem;
}

.preset-btn {
  background: rgba(231, 94, 141, 0.3);
  border: 1px solid rgba(231, 94, 141, 0.5);
  color: #e75e8d;
  padding: 0.25rem 0.75rem;
  border-radius: 15px;
  font-size: 0.75rem;
  cursor: pointer;
  transition: all 0.3s ease;
}

.preset-btn:hover {
  background: rgba(231, 94, 141, 0.5);
}

.voice-select {
  width: 100%;
  background: rgba(255, 255, 255, 0.1);
  border: 1px solid rgba(255, 255, 255, 0.3);
  border-radius: 6px;
  padding: 0.5rem;
  color: white;
  font-size: 0.875rem;
}

.voice-select option {
  background: #1a1a1a;
  color: white;
}

@keyframes pulse {
  0%, 100% { opacity: 0.5; }
  50% { opacity: 0.8; }
}
</style>