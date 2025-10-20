import { ref, readonly, computed } from 'vue'
import { useSpeechSynthesis } from './useSpeechSynthesis'

// Global speech settings store
const speechRate = ref(0.8)
const speechPitch = ref(1.0)
const selectedVoiceIndex = ref(0)
const selectedTTSProvider = ref<'openai' | 'webspeech'>('webspeech')
const isInitialized = ref(false)

export const useSpeechSettings = () => {
  const { getAllVoices, loadVoices: loadSpeechVoices, initializeVoices } = useSpeechSynthesis()

  // Load voices (compatibility method)
  const loadVoices = () => {
    return loadSpeechVoices()
  }

  // Get all available voices (OpenAI + Web Speech API)
  const getAvailableVoices = () => {
    return getAllVoices()
  }

  // Get English voices (all voices are English)
  const getEnglishVoices = () => {
    return getAllVoices()
  }

  // Get voices by provider
  const getVoicesByProvider = (provider: 'openai' | 'webspeech') => {
    return getAllVoices().filter(voice => voice.provider === provider)
  }

  // Get current speech options
  const getCurrentOptions = async () => {
    // Auto-initialize if not already done
    if (!isInitialized.value) {
      console.log('🔄 Auto-initializing speech settings...')
      await ensureInitialized()
    }
    
    const allVoices = getAllVoices()
    const selectedVoice = allVoices[selectedVoiceIndex.value]
    const provider = selectedVoice?.provider || selectedTTSProvider.value
    
    console.log(`📋 getCurrentOptions: voiceIndex=${selectedVoiceIndex.value}, provider=${provider}`)
    if (selectedVoice) {
      console.log(`   Selected voice: ${selectedVoice.name} (${selectedVoice.voiceName})`)
    } else {
      console.warn(`   ⚠️ No voice found at index ${selectedVoiceIndex.value}. Available voices: ${allVoices.length}`)
    }
    
    return {
      lang: 'en-US',
      rate: speechRate.value,
      pitch: speechPitch.value,
      voiceIndex: selectedVoiceIndex.value, // Use global index, not provider-specific
      provider: provider
    }
  }
  
  // Ensure voices are initialized
  const ensureInitialized = async () => {
    if (isInitialized.value) return
    
    await initializeVoices()
    loadSettings()
    
    const allVoices = getAllVoices()
    if (allVoices.length > 0 && selectedVoiceIndex.value >= allVoices.length) {
      // Reset to first voice if saved index is invalid
      selectedVoiceIndex.value = 0
      saveSettings()
    }
    
    isInitialized.value = true
    console.log(`✅ Speech settings initialized with ${allVoices.length} voices`)
  }

  // Initialize default voice
  const initializeDefaultVoice = async () => {
    await initializeVoices()
    const allVoices = getAllVoices()
    // Set first voice as default
    if (allVoices.length > 0) {
      // Default to first Web Speech API voice (more reliable than OpenAI)
      const webSpeechIndex = allVoices.findIndex(voice => voice.provider === 'webspeech')
      selectedVoiceIndex.value = webSpeechIndex >= 0 ? webSpeechIndex : 0
    }
  }

  // Save settings to localStorage
  const saveSettings = () => {
    const settings = {
      rate: speechRate.value,
      pitch: speechPitch.value,
      voiceIndex: selectedVoiceIndex.value,
      ttsProvider: selectedTTSProvider.value
    }
    localStorage.setItem('speechSettings', JSON.stringify(settings))
  }

  // Load settings from localStorage
  const loadSettings = () => {
    const saved = localStorage.getItem('speechSettings')
    if (saved) {
      try {
        const settings = JSON.parse(saved)
        speechRate.value = settings.rate || 0.8
        speechPitch.value = settings.pitch || 1.0
        selectedVoiceIndex.value = settings.voiceIndex || 0
        selectedTTSProvider.value = settings.ttsProvider || 'openai'
      } catch (error) {
        console.error('Failed to load speech settings:', error)
      }
    }
  }

  return {
    // State (readonly for external components)
    speechRate: readonly(speechRate),
    speechPitch: readonly(speechPitch),
    selectedVoiceIndex: readonly(selectedVoiceIndex),
    selectedTTSProvider: readonly(selectedTTSProvider),

    // Computed
    getAvailableVoices,
    getEnglishVoices,
    getCurrentOptions,
    getVoicesByProvider,

    // Actions
    setSpeechRate: (rate: number) => {
      speechRate.value = rate
      saveSettings()
    },
    setSpeechPitch: (pitch: number) => {
      speechPitch.value = pitch
      saveSettings()
    },
    setSelectedVoiceIndex: (index: number) => {
      selectedVoiceIndex.value = index
      saveSettings()
    },
    setTTSProvider: (provider: 'openai' | 'webspeech') => {
      selectedTTSProvider.value = provider
      saveSettings()
    },

    // Initialization
    loadVoices,
    initializeDefaultVoice,
    ensureInitialized,
    loadSettings,
    saveSettings
  }
}