 import { ref, readonly } from 'vue'

// Global speech settings store
const speechRate = ref(0.8)
const speechPitch = ref(1.0)
const selectedVoiceIndex = ref(0)
const availableVoices = ref<SpeechSynthesisVoice[]>([])

export const useSpeechSettings = () => {
  // Load voices
  const loadVoices = () => {
    return new Promise<SpeechSynthesisVoice[]>((resolve) => {
      const voices = window.speechSynthesis.getVoices()
      if (voices.length) {
        availableVoices.value = voices
        resolve(voices)
      } else {
        window.speechSynthesis.onvoiceschanged = () => {
          const newVoices = window.speechSynthesis.getVoices()
          availableVoices.value = newVoices
          resolve(newVoices)
        }
      }
    })
  }

  // Get English voices
  const getEnglishVoices = () => {
    return availableVoices.value.filter(voice => 
      voice.lang.toLowerCase().startsWith('en')
    )
  }

  // Get current speech options
  const getCurrentOptions = () => {
    const englishVoices = getEnglishVoices()
    const selectedVoice = englishVoices[selectedVoiceIndex.value]
    
    return {
      lang: 'en-US',
      rate: speechRate.value,
      pitch: speechPitch.value,
      voiceIndex: selectedVoice 
        ? availableVoices.value.indexOf(selectedVoice)
        : undefined
    }
  }

  // Initialize default voice
  const initializeDefaultVoice = async () => {
    await loadVoices()
    const englishVoices = getEnglishVoices()
    const defaultIndex = englishVoices.findIndex(voice => 
      voice.default || voice.localService
    )
    if (defaultIndex >= 0) {
      selectedVoiceIndex.value = defaultIndex
    }
  }

  // Save settings to localStorage
  const saveSettings = () => {
    const settings = {
      rate: speechRate.value,
      pitch: speechPitch.value,
      voiceIndex: selectedVoiceIndex.value
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
    availableVoices: readonly(availableVoices),

    // Computed
    getEnglishVoices,
    getCurrentOptions,

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

    // Initialization
    loadVoices,
    initializeDefaultVoice,
    loadSettings,
    saveSettings
  }
}