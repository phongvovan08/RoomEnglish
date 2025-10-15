import { ref, readonly } from 'vue'

export const useSpeechSynthesis = () => {
  const isSupported = ref(typeof window !== 'undefined' && 'speechSynthesis' in window)
  const playingInstances = ref(new Set<string>())

  const speak = async (text: string, instanceId: string, lang: string = 'en-US') => {
    if (!isSupported.value || playingInstances.value.has(instanceId)) {
      return Promise.reject('Speech synthesis not supported or instance already playing')
    }

    return new Promise<void>((resolve, reject) => {
      try {
        // Cancel any ongoing speech
        window.speechSynthesis.cancel()

        const utterance = new SpeechSynthesisUtterance(text)
        
        // Set voice properties
        utterance.lang = lang
        utterance.rate = 0.8 // Slightly slower for learning
        utterance.pitch = 1.0
        utterance.volume = 1.0

        // Try to use a native English voice
        const voices = window.speechSynthesis.getVoices()
        const englishVoice = voices.find(voice => 
          voice.lang.startsWith('en') && voice.localService
        )
        if (englishVoice) {
          utterance.voice = englishVoice
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