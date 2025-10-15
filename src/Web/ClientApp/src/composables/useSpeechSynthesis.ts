import { ref, readonly } from 'vue'

export const useSpeechSynthesis = () => {
  const isSupported = ref(typeof window !== 'undefined' && 'speechSynthesis' in window)
  const playingInstances = ref(new Set<string>())

  interface SpeechOptions {
    lang?: string
    rate?: number
    pitch?: number
    volume?: number
    voiceIndex?: number
  }

  const speak = async (text: string, instanceId: string, options: SpeechOptions = {}) => {
    if (!isSupported.value || playingInstances.value.has(instanceId)) {
      return Promise.reject('Speech synthesis not supported or instance already playing')
    }

    return new Promise<void>((resolve, reject) => {
      try {
        // Cancel any ongoing speech
        window.speechSynthesis.cancel()

        const utterance = new SpeechSynthesisUtterance(text)
        
        // Set voice properties with defaults
        utterance.lang = options.lang || 'en-US'
        utterance.rate = options.rate || 0.8 // Slightly slower for learning
        utterance.pitch = options.pitch || 1.0
        utterance.volume = options.volume || 1.0

        // Get available voices
        const voices = window.speechSynthesis.getVoices()
        
        // Select voice based on options
        let selectedVoice: SpeechSynthesisVoice | undefined
        
        if (options.voiceIndex !== undefined && voices[options.voiceIndex]) {
          selectedVoice = voices[options.voiceIndex]
        } else {
          // Try to use a native English voice as fallback
          selectedVoice = voices.find(voice => 
            voice.lang.startsWith('en') && voice.localService
          )
        }
        
        if (selectedVoice) {
          utterance.voice = selectedVoice
        }

        utterance.onstart = () => {
          playingInstances.value.add(instanceId)
        }

        utterance.onend = () => {
          playingInstances.value.delete(instanceId)
          resolve()
        }

        utterance.onerror = (event) => {
          playingInstances.value.delete(instanceId)
          reject(event.error)
        }

        window.speechSynthesis.speak(utterance)
      } catch (error) {
        playingInstances.value.delete(instanceId)
        reject(error)
      }
    })
  }

  const stop = (instanceId?: string) => {
    if (isSupported.value) {
      window.speechSynthesis.cancel()
      if (instanceId) {
        playingInstances.value.delete(instanceId)
      } else {
        playingInstances.value.clear()
      }
    }
  }

  const isPlaying = (instanceId: string) => {
    return playingInstances.value.has(instanceId)
  }

  // Load voices (needed for some browsers)
  const loadVoices = () => {
    return new Promise<SpeechSynthesisVoice[]>((resolve) => {
      const voices = window.speechSynthesis.getVoices()
      if (voices.length) {
        resolve(voices)
      } else {
        window.speechSynthesis.onvoiceschanged = () => {
          resolve(window.speechSynthesis.getVoices())
        }
      }
    })
  }

  return {
    isPlaying,
    isSupported: readonly(isSupported),
    speak,
    stop,
    loadVoices
  }
}