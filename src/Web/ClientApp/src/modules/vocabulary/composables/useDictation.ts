import { ref, computed, readonly } from 'vue'
import { createAuthHeaders } from '@/utils/auth'
import { useSpeechSynthesis } from '@/composables/useSpeechSynthesis'
import { useSpeechSettings } from '@/composables/useSpeechSettings'
import type { 
  DictationResult,
  SubmitDictationCommand,
  VocabularyExample
} from '../types/vocabulary.types'

const API_BASE = '/api/vocabulary-learning'

// Create unique instance ID for dictation audio
const DICTATION_INSTANCE_ID = 'dictation-practice'

export const useDictation = () => {
  const currentExample = ref<VocabularyExample | null>(null)
  const userInput = ref('')
  const isRecording = ref(false)
  const startTime = ref<number | null>(null)
  const dictationResult = ref<DictationResult | null>(null)
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  // Speech Recognition
  const recognition = ref<any>(null)
  
  // Use the global speech synthesis system
  const { speak, isPlaying: checkIsPlaying, stop } = useSpeechSynthesis()
  const { getCurrentOptions } = useSpeechSettings()
  
  // Check if dictation audio is playing
  const isPlaying = computed(() => checkIsPlaying(DICTATION_INSTANCE_ID))

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

  // Audio playback using TTS
  const playAudio = async (text: string, rate: number = 1) => {
    try {
      console.log('🎵 Dictation playAudio called:', { text: text.substring(0, 50), rate })
      
      // Get current speech settings (voice, pitch, provider)
      const options = await getCurrentOptions()
      console.log('🔧 Speech options from settings:', options)
      
      // Override rate with the playback speed
      options.rate = rate
      
      // FORCE Web Speech API for Dictation
      // Web Speech API doesn't have autoplay restrictions
      // and works reliably with keyboard shortcuts
      options.provider = 'webspeech'
      
      // Use first Web Speech voice if current selection is OpenAI
      const { getAllVoices } = useSpeechSynthesis()
      const allVoices = getAllVoices()
      const webSpeechVoices = allVoices.filter(v => v.provider === 'webspeech')
      
      if (webSpeechVoices.length > 0) {
        // Find index of first web speech voice in all voices
        const webSpeechVoiceIndex = allVoices.findIndex(v => v.provider === 'webspeech')
        options.voiceIndex = webSpeechVoiceIndex
        console.log('🎤 Using Web Speech API for Dictation. Voice:', webSpeechVoices[0].name)
      }
      
      console.log('▶️ About to call speak with:', { instanceId: DICTATION_INSTANCE_ID, options })
      
      // Speak the text using the global speech synthesis system
      await speak(text, DICTATION_INSTANCE_ID, options)
      
      console.log('✅ Speak completed successfully')
    } catch (err: any) {
      // Ignore AbortError - it's expected when stopping audio
      if (err?.name === 'AbortError') {
        console.log('Audio playback was interrupted (expected)')
        return
      }
      
      console.error('❌ Failed to play audio:', err)
      error.value = 'Failed to play audio'
      throw err
    }
  }

  const stopAudio = () => {
    stop(DICTATION_INSTANCE_ID)
  }

  // Speech recognition controls
  const startRecording = () => {
    console.log('🎤 startRecording called, userInput before clear:', userInput.value)
    console.trace('🎤 Call stack:')
    
    if (!recognition.value) {
      initSpeechRecognition()
    }
    
    if (recognition.value && !isRecording.value) {
      userInput.value = ''
      console.log('🎤 userInput cleared!')
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
  const submitDictation = async (exampleId: number, userText?: string, elapsedTime?: number): Promise<DictationResult> => {
    try {
      isLoading.value = true
      error.value = null

      const inputText = userText || userInput.value
      const timeTaken = elapsedTime ?? (startTime.value ? Math.floor((Date.now() - startTime.value) / 1000) : 0)

      // Get correct answer from current example
      const correctAnswer = currentExample.value?.sentence || ''
      
      // Calculate accuracy on client-side
      const result = calculateDictationAccuracy(inputText, correctAnswer, timeTaken)
      
      dictationResult.value = result
      
      // Optionally send to server for tracking (fire and forget)
      try {
        const command: SubmitDictationCommand = {
          exampleId,
          userInput: inputText,
          timeTakenSeconds: timeTaken
        }

        fetch(`${API_BASE}/dictation/submit`, {
          method: 'POST',
          headers: createAuthHeaders(),
          body: JSON.stringify(command)
        }).catch(() => {
          // Ignore server errors - we have client-side result
          console.log('Note: Server tracking failed, but dictation result is available')
        })
      } catch {
        // Ignore - client-side calculation is enough
      }
      
      return result
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to submit dictation'
      throw err
    } finally {
      isLoading.value = false
    }
  }

  // Calculate dictation accuracy (client-side)
  const calculateDictationAccuracy = (
    userInput: string, 
    correctAnswer: string, 
    timeTaken: number
  ): DictationResult => {
    // Normalize text: lowercase, trim, remove punctuation
    const normalize = (text: string) => {
      return text
        .toLowerCase()
        .trim()
        .replace(/[.,!?;:]/g, '')
        .replace(/\s+/g, ' ')
    }

    const normalizedUser = normalize(userInput)
    const normalizedCorrect = normalize(correctAnswer)

    // Check if exactly correct
    const isCorrect = normalizedUser === normalizedCorrect

    // Calculate similarity percentage (simple word-based)
    const userWords = normalizedUser.split(' ').filter(w => w.length > 0)
    const correctWords = normalizedCorrect.split(' ').filter(w => w.length > 0)
    
    let matchingWords = 0
    const maxLength = Math.max(userWords.length, correctWords.length)
    
    for (let i = 0; i < Math.min(userWords.length, correctWords.length); i++) {
      if (userWords[i] === correctWords[i]) {
        matchingWords++
      }
    }

    const accuracyPercentage = maxLength > 0 
      ? Math.round((matchingWords / maxLength) * 100) 
      : 0

    return {
      id: 0, // Temporary ID for client-side result
      userId: '', // Will be set by server if needed
      exampleId: currentExample.value?.id || 0,
      isCorrect,
      accuracyPercentage,
      userInput,
      correctAnswer,
      timeTakenSeconds: timeTaken,
      completedAt: new Date().toISOString()
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