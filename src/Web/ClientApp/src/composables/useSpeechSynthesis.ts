import { ref, readonly } from 'vue'

export const useSpeechSynthesis = () => {
  const isPlaying = ref(false)
  const isSupported = ref(typeof window !== 'undefined' && 'speechSynthesis' in window)

  const speak = async (text: string, lang: string = 'en-US') => {
    if (!isSupported.value || isPlaying.value) {
      return Promise.reject('Speech synthesis not supported or already playing')
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
          isPlaying.value = true
        }

        utterance.onend = () => {
          isPlaying.value = false
          resolve()
        }

        utterance.onerror = (event) => {
          isPlaying.value = false
          reject(event.error)
        }

        window.speechSynthesis.speak(utterance)
      } catch (error) {
        isPlaying.value = false
        reject(error)
      }
    })
  }

  const stop = () => {
    if (isSupported.value) {
      window.speechSynthesis.cancel()
      isPlaying.value = false
    }
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
    isPlaying: readonly(isPlaying),
    isSupported: readonly(isSupported),
    speak,
    stop,
    loadVoices
  }
}