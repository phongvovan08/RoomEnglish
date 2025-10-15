<template>
  <div v-if="showPanel" class="speech-settings-panel">
    <div class="panel-header">
      <h3>🔊 Speech Settings</h3>
      <button @click="$emit('close')" class="close-btn">
        <Icon icon="mdi:close" class="w-4 h-4" />
      </button>
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
    <div class="setting-group" v-if="englishVoices.length > 0">
      <label class="setting-label">Voice:</label>
      <select :value="currentVoiceIndex" @change="updateVoice" class="voice-select">
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
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { Icon } from '@iconify/vue'
import { useSpeechSettings } from '@/composables/useSpeechSettings'
import { useSpeechSynthesis } from '@/composables/useSpeechSynthesis'

interface Props {
  showPanel: boolean
}

defineProps<Props>()
defineEmits<{
  close: []
}>()

const {
  speechRate,
  speechPitch,
  selectedVoiceIndex,
  getEnglishVoices,
  getCurrentOptions,
  setSpeechRate,
  setSpeechPitch,
  setSelectedVoiceIndex,
  initializeDefaultVoice,
  loadSettings
} = useSpeechSettings()

const { speak, isPlaying } = useSpeechSynthesis()

// Computed values
const currentRate = computed(() => speechRate.value)
const currentPitch = computed(() => speechPitch.value)
const currentVoiceIndex = computed(() => selectedVoiceIndex.value)
const englishVoices = computed(() => getEnglishVoices())
const isTesting = computed(() => isPlaying('speech-test'))

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
  setSelectedVoiceIndex(parseInt(target.value))
}

const testSpeech = async () => {
  if (isTesting.value) return
  
  try {
    const options = getCurrentOptions()
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

onMounted(async () => {
  loadSettings()
  await initializeDefaultVoice()
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
</style>