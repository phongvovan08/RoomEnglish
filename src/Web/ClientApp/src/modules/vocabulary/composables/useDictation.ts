import { ref, computed, readonly } from 'vue'
import { createAuthHeaders } from '@/utils/auth'
import type { 
  DictationResult,
  SubmitDictationCommand,
  VocabularyExample
} from '../types/vocabulary.types'

const API_BASE = '/api/vocabulary-learning'

export const useDictation = () => {
  const currentExample = ref<VocabularyExample | null>(null)
  const userInput = ref('')
  const isRecording = ref(false)
  const isPlaying = ref(false)
  const startTime = ref<number | null>(null)
  const dictationResult = ref<DictationResult | null>(null)
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  // Speech Recognition
  const recognition = ref<any>(null)
  const audioPlayer = ref<HTMLAudioElement | null>(null)

  // Computed properties
  const timeElapsed = computed(() => {
    if (!startTime.value) return 0
    return Math.floor((Date.now() - startTime.value) / 1000)
  })

  const accuracy = computed(() => {
    if (!dictationResult.value) return 0
    return dictationResult.value.accuracyPercentage
  })

  // Initialize Speech Recognition
  const initSpeechRecognition = () => {
    if ('webkitSpeechRecognition' in window || 'SpeechRecognition' in window) {
      const SpeechRecognition = (window as any).SpeechRecognition || (window as any).webkitSpeechRecognition
      recognition.value = new SpeechRecognition()
      
      recognition.value.continuous = false
      recognition.value.interimResults = false
      recognition.value.lang = 'en-US'

      recognition.value.onstart = () => {
        isRecording.value = true
        startTime.value = Date.now()
      }

      recognition.value.onresult = (event: any) => {
        const transcript = event.results[0][0].transcript
        userInput.value = transcript
      }

      recognition.value.onerror = (event: any) => {
        error.value = `Speech recognition error: ${event.error}`
        isRecording.value = false
      }

      recognition.value.onend = () => {
        isRecording.value = false
      }
    } else {
      error.value = 'Speech recognition not supported in this browser'
    }
  }

  // Audio playback
  const playAudio = async (audioUrl: string) => {
    try {
      if (audioPlayer.value) {
        audioPlayer.value.pause()
        audioPlayer.value.currentTime = 0
      }

      audioPlayer.value = new Audio(audioUrl)
      isPlaying.value = true

      audioPlayer.value.onended = () => {
        isPlaying.value = false
      }

      audioPlayer.value.onerror = () => {
        error.value = 'Failed to play audio'
        isPlaying.value = false
      }

      await audioPlayer.value.play()
    } catch (err) {
      error.value = 'Failed to play audio'
      isPlaying.value = false
    }
  }

  const stopAudio = () => {
    if (audioPlayer.value) {
      audioPlayer.value.pause()
      audioPlayer.value.currentTime = 0
    }
    isPlaying.value = false
  }

  // Speech recognition controls
  const startRecording = () => {
    if (!recognition.value) {
      initSpeechRecognition()
    }
    
    if (recognition.value && !isRecording.value) {
      userInput.value = ''
      dictationResult.value = null
      recognition.value.start()
    }
  }

  const stopRecording = () => {
    if (recognition.value && isRecording.value) {
      recognition.value.stop()
    }
  }

  // Submit dictation
  const submitDictation = async (exampleId: number, userText?: string): Promise<DictationResult> => {
    try {
      isLoading.value = true
      error.value = null

      const inputText = userText || userInput.value
      const timeTaken = startTime.value ? Math.floor((Date.now() - startTime.value) / 1000) : 0

      const command: SubmitDictationCommand = {
        exampleId,
        userInput: inputText,
        timeTakenSeconds: timeTaken
      }

      const response = await fetch(`${API_BASE}/dictation/submit`, {
        method: 'POST',
        headers: createAuthHeaders(),
        body: JSON.stringify(command)
      })

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`)
      }

      const result = await response.json()
      dictationResult.value = result
      return result
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to submit dictation'
      throw err
    } finally {
      isLoading.value = false
    }
  }

  // Utility functions
  const reset = () => {
    userInput.value = ''
    dictationResult.value = null
    startTime.value = null
    stopRecording()
    stopAudio()
    error.value = null
  }

  const setExample = (example: VocabularyExample) => {
    currentExample.value = example
    reset()
  }

  return {
    // State
    currentExample: readonly(currentExample),
    userInput,
    isRecording: readonly(isRecording),
    isPlaying: readonly(isPlaying),
    dictationResult: readonly(dictationResult),
    isLoading: readonly(isLoading),
    error: readonly(error),

    // Computed
    timeElapsed,
    accuracy,

    // Actions
    initSpeechRecognition,
    startRecording,
    stopRecording,
    playAudio,
    stopAudio,
    submitDictation,
    setExample,
    reset,

    // Utilities
    clearError: () => { error.value = null }
  }
}