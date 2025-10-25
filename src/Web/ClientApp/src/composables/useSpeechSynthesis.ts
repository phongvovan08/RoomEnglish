import { ref, readonly } from 'vue'
import { OpenAITTS } from '@lobehub/tts'
import { useAudioCacheAPI } from './useAudioCacheAPI'

export const useSpeechSynthesis = () => {
  const isSupported = ref(typeof window !== 'undefined' && 'speechSynthesis' in window)
  const playingInstances = ref(new Set<string>())
  const audioInstances = ref(new Map<string, HTMLAudioElement>())
  const currentTTSProvider = ref<'openai' | 'webspeech'>('webspeech') // Default to Web Speech API
  const webSpeechVoices = ref<SpeechSynthesisVoice[]>([])
  
  // Memory cache for current session (faster)
  const memoryCache = ref(new Map<string, Blob>())
  
  // Database API for persistent cache
  const { 
    getCachedAudio, 
    saveAudioToCache, 
    getCacheStats: getAPICacheStats,
    cleanupCache: cleanupAPICache
  } = useAudioCacheAPI()

  interface SpeechOptions {
    lang?: string
    rate?: number
    pitch?: number
    volume?: number
    voiceIndex?: number
    provider?: 'openai' | 'webspeech'
  }

  // OpenAI TTS voices
  const openaiVoices = [
    { name: 'OpenAI - Alloy', voiceName: 'alloy', lang: 'en-US', provider: 'openai' as const },
    { name: 'OpenAI - Echo', voiceName: 'echo', lang: 'en-US', provider: 'openai' as const },
    { name: 'OpenAI - Fable', voiceName: 'fable', lang: 'en-US', provider: 'openai' as const },
    { name: 'OpenAI - Nova', voiceName: 'nova', lang: 'en-US', provider: 'openai' as const },
    { name: 'OpenAI - Onyx', voiceName: 'onyx', lang: 'en-US', provider: 'openai' as const },
    { name: 'OpenAI - Shimmer', voiceName: 'shimmer', lang: 'en-US', provider: 'openai' as const },
  ]

  // Load Web Speech API voices
  const loadWebSpeechVoices = () => {
    return new Promise<SpeechSynthesisVoice[]>((resolve) => {
      const voices = window.speechSynthesis.getVoices()
      if (voices.length) {
        const englishVoices = voices.filter(voice => 
          voice.lang.toLowerCase().startsWith('en')
        )
        webSpeechVoices.value = englishVoices
        resolve(englishVoices)
      } else {
        window.speechSynthesis.onvoiceschanged = () => {
          const newVoices = window.speechSynthesis.getVoices()
          const englishVoices = newVoices.filter(voice => 
            voice.lang.toLowerCase().startsWith('en')
          )
          webSpeechVoices.value = englishVoices
          resolve(englishVoices)
        }
      }
    })
  }

  // Convert Web Speech voices to our format
  const getWebSpeechVoicesFormatted = () => {
    return webSpeechVoices.value.map(voice => ({
      name: `${voice.name} (${voice.lang})`,
      voiceName: voice.name,
      lang: voice.lang,
      provider: 'webspeech' as const,
      originalVoice: voice
    }))
  }

  // Combined voice list (OpenAI + Web Speech API)
  const getAllVoices = () => {
    return [...openaiVoices, ...getWebSpeechVoicesFormatted()]
  }

  // OpenAI TTS
  const speakWithOpenAI = async (text: string, instanceId: string, options: SpeechOptions = {}) => {
    console.log('🤖 OpenAI TTS: Starting synthesis...')
    const allVoices = getAllVoices()
    const selectedVoice = allVoices[options.voiceIndex || 0]
    
    if (selectedVoice.provider !== 'openai') {
      console.error('❌ Invalid voice for OpenAI:', selectedVoice)
      return Promise.reject('Invalid voice for OpenAI')
    }

    const voiceName = selectedVoice.voiceName
    const rate = options.rate || 1.0
    const provider = 'openai'
    
    // Create cache key for memory cache
    const cacheKey = `${text}_${voiceName}_${rate}`
    
    // Check memory cache first (fastest)
    let cachedBlob = memoryCache.value.get(cacheKey)
    
    if (cachedBlob) {
      console.log('⚡ Using memory cache:', text.substring(0, 30))
      return cachedBlob.arrayBuffer()
    }
    
    // Check database cache
    const dbCachedBlob = await getCachedAudio(text, voiceName, rate, provider)
    
    if (dbCachedBlob) {
      console.log('💾 Loaded from database cache:', text.substring(0, 30))
      // Save to memory for faster future access
      memoryCache.value.set(cacheKey, dbCachedBlob)
      return dbCachedBlob.arrayBuffer()
    }

    // Not cached, fetch from API
    console.log('🌐 Fetching from OpenAI API...')
    
    // Get API key from environment or settings
    const apiKey = import.meta.env.VITE_OPENAI_API_KEY || localStorage.getItem('openai_api_key')
    console.log('🔑 API Key found:', apiKey ? `${apiKey.substring(0, 10)}...` : 'No key')
    
    if (!apiKey) {
      throw new Error('OpenAI API key not found. Please add VITE_OPENAI_API_KEY to environment or set it in settings.')
    }

    console.log('📝 Text to synthesize:', text.substring(0, 50) + '...')
    console.log('🎵 Voice:', voiceName)

    const tts = new OpenAITTS({ OPENAI_API_KEY: apiKey })
    
    const payload = {
      input: text,
      options: {
        model: 'tts-1', // or 'tts-1-hd' for higher quality
        voice: voiceName as any,
        speed: rate
      }
    }

    console.log('📤 Sending request to OpenAI:', payload)

    try {
      const response = await tts.create(payload)
      console.log('📥 Response status:', response.status, response.statusText)
      
      if (response.ok) {
        const arrayBuffer = await response.arrayBuffer()
        console.log('✅ OpenAI TTS success! Audio buffer size:', arrayBuffer.byteLength)
        
        // Cache the audio blob
        const blob = new Blob([arrayBuffer], { type: 'audio/mpeg' })
        
        // Save to memory cache (instant)
        memoryCache.value.set(cacheKey, blob)
        
        // Save to database (async, fire and forget)
        saveAudioToCache(text, voiceName, rate, provider, blob, 30).catch(err => {
          console.error('Failed to save to database cache:', err)
        })
        
        console.log('💾 Cached audio for future use')
        
        return arrayBuffer
      } else {
        const errorText = await response.text()
        console.error('❌ OpenAI TTS failed:', response.status, errorText)
        throw new Error(`OpenAI TTS failed: ${response.status} - ${errorText}`)
      }
    } catch (error) {
      console.error('❌ OpenAI TTS error:', error)
      throw error
    }
  }

  // Web Speech API fallback
  const speakWithWebAPI = async (text: string, instanceId: string, options: SpeechOptions = {}) => {
    return new Promise<void>((resolve, reject) => {
      try {
        window.speechSynthesis.cancel()

        const utterance = new SpeechSynthesisUtterance(text)
        utterance.lang = options.lang || 'en-US'
        utterance.rate = options.rate || 0.8
        utterance.pitch = options.pitch || 1.0
        utterance.volume = options.volume || 1.0

        // Use voiceIndex from options (calculated for Web Speech API)
        if (options.voiceIndex !== undefined && webSpeechVoices.value[options.voiceIndex]) {
          const selectedVoice = webSpeechVoices.value[options.voiceIndex]
          utterance.voice = selectedVoice
          console.log(`Web Speech API: Using voice ${selectedVoice.name} (${selectedVoice.lang})`)
        } else {
          // Fallback: find first English voice
          const fallbackVoice = webSpeechVoices.value.find(voice => 
            voice.lang.startsWith('en') && voice.localService
          ) || webSpeechVoices.value.find(voice => voice.lang.startsWith('en'))
          if (fallbackVoice) {
            utterance.voice = fallbackVoice
            console.log(`Web Speech API fallback: Using voice ${fallbackVoice.name} (${fallbackVoice.lang})`)
          }
        }

        utterance.onstart = () => playingInstances.value.add(instanceId)
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

  // Main speak function with provider selection
  const speak = async (text: string, instanceId: string, options: SpeechOptions = {}) => {
    console.log('🎙️ speak() called for instance:', instanceId)
    
    // Ensure voices are loaded
    await loadWebSpeechVoices()
    
    const allVoices = getAllVoices()
    const selectedVoice = allVoices[options.voiceIndex || 0]
    const provider = options.provider || selectedVoice?.provider || currentTTSProvider.value
    
    console.log('🎤 Provider:', provider, 'Voice:', selectedVoice?.name)

    // Stop any existing audio for this instance first
    if (playingInstances.value.has(instanceId)) {
      console.log('⏹️ Stopping existing instance:', instanceId)
      stop(instanceId)
      // Wait a bit for cleanup
      await new Promise(resolve => setTimeout(resolve, 100))
    }

    try {
      playingInstances.value.add(instanceId)
      console.log('📝 Added instance to playing set:', instanceId)

      // Route to appropriate TTS provider
      if (provider === 'openai') {
        try {
          const audioBuffer = await speakWithOpenAI(text, instanceId, options)
          
          // Play audio from buffer
          const blob = new Blob([audioBuffer], { type: 'audio/mpeg' })
          const audioUrl = URL.createObjectURL(blob)
          console.log('🎧 Created audio URL:', audioUrl, 'Blob size:', blob.size)
          
          // Create audio element and set src directly
          const audio = new Audio(audioUrl)
          console.log('🎧 Created Audio element with src:', audio.src)
          
          audioInstances.value.set(instanceId, audio)
          audio.volume = options.volume || 1.0
          audio.autoplay = false // Don't autoplay, we'll control it
          
          console.log('🎧 Audio volume:', audio.volume)

          return new Promise<void>((resolve, reject) => {
            const cleanup = () => {
              console.log('🧹 Cleaning up audio instance:', instanceId)
              playingInstances.value.delete(instanceId)
              audioInstances.value.delete(instanceId)
              URL.revokeObjectURL(audioUrl)
            }

            audio.onended = () => {
              console.log('🏁 Audio ended')
              cleanup()
              resolve()
            }

            audio.onerror = (event) => {
              console.error('❌ Audio error:', event, audio.error)
              if (audio.error) {
                console.error('   Error code:', audio.error.code, 'Message:', audio.error.message)
              }
              cleanup()
              reject(audio.error || new Error('Audio loading failed'))
            }
            
            audio.onloadeddata = () => {
              console.log('📊 Audio data loaded. Duration:', audio.duration, 'readyState:', audio.readyState)
              // Try to play as soon as data is loaded
              attemptPlay()
            }
            
            audio.onplaying = () => {
              console.log('🔊 Audio is now playing!')
            }
            
            audio.onpause = () => {
              console.log('⏸️ Audio paused')
            }

            // Wait for audio to be ready before playing
            const attemptPlay = () => {
              console.log('▶️ Attempting to play audio...')
              console.log('   readyState:', audio.readyState, 'paused:', audio.paused)
              
              const playPromise = audio.play()
              if (playPromise !== undefined) {
                playPromise
                  .then(() => {
                    console.log('✅ Audio.play() promise resolved - audio should be playing')
                  })
                  .catch((error) => {
                    console.error('❌ Audio.play() failed:', error.name, error.message)
                    cleanup()
                    // Only reject if it's not an abort error from stopping
                    if (error.name !== 'AbortError') {
                      reject(error)
                    } else {
                      // Silently handle abort errors
                      console.log('ℹ️ AbortError - audio was stopped')
                      resolve()
                    }
                  })
              } else {
                console.warn('⚠️ audio.play() returned undefined')
              }
            }
            
            console.log('⏳ Loading audio...')
            // Audio with src in constructor should auto-load, but let's be explicit
            audio.load()
          })
        } catch (error) {
          console.warn('OpenAI TTS failed, falling back to Web Speech API:', error)
          playingInstances.value.delete(instanceId)
          return speakWithWebAPI(text, instanceId, options)
        }
      } else {
        // Use Web Speech API
        return speakWithWebAPI(text, instanceId, options)
      }

    } catch (error) {
      playingInstances.value.delete(instanceId)
      throw error
    }
  }

  const stop = (instanceId?: string) => {
    if (instanceId) {
      const audio = audioInstances.value.get(instanceId)
      if (audio) {
        try {
          // Remove event listeners to prevent errors
          audio.onended = null
          audio.onerror = null
          
          // Stop audio safely
          if (!audio.paused) {
            audio.pause()
          }
          audio.currentTime = 0
          audio.src = '' // Clear source to fully stop
        } catch (e) {
          console.warn('Error stopping audio:', e)
        }
        audioInstances.value.delete(instanceId)
      }
      playingInstances.value.delete(instanceId)
      
      // Only stop Web Speech API if no OpenAI audio instances are active
      if (audioInstances.value.size === 0 && 'speechSynthesis' in window) {
        window.speechSynthesis.cancel()
      }
    } else {
      // Stop all instances
      audioInstances.value.forEach((audio) => {
        try {
          audio.onended = null
          audio.onerror = null
          if (!audio.paused) {
            audio.pause()
          }
          audio.currentTime = 0
          audio.src = ''
        } catch (e) {
          console.warn('Error stopping audio:', e)
        }
      })
      audioInstances.value.clear()
      playingInstances.value.clear()
      
      // Stop Web Speech API when stopping all
      if ('speechSynthesis' in window) {
        window.speechSynthesis.cancel()
      }
    }
  }

  const isPlaying = (instanceId: string) => {
    return playingInstances.value.has(instanceId)
  }

  // Set TTS provider
  const setTTSProvider = (provider: 'openai' | 'webspeech') => {
    currentTTSProvider.value = provider
    console.info(`Switched to ${provider} TTS`)
  }

  // Load voices (compatibility)
  const loadVoices = () => {
    return loadWebSpeechVoices().then(() => {
      const allVoices = getAllVoices()
      return allVoices.map((voice, index) => ({
        name: voice.name,
        lang: voice.lang,
        localService: voice.provider === 'webspeech',
        default: index === 0,
        voiceURI: voice.voiceName
      }) as any)
    })
  }

  // Initialize voices on load
  const initializeVoices = async () => {
    await loadWebSpeechVoices()
  }

  // Clear audio cache (useful for memory management)
  const clearCache = async () => {
    memoryCache.value.clear()
    const deleted = await cleanupAPICache(100, true)
    console.log(`🧹 Audio cache cleared (${deleted} entries deleted)`)
    return deleted
  }

  // Get cache stats
  const getCacheStats = async () => {
    const dbStats = await getAPICacheStats()
    return {
      memory: {
        count: memoryCache.value.size,
        size: Array.from(memoryCache.value.values()).reduce((sum, blob) => sum + blob.size, 0)
      },
      database: dbStats ? {
        count: dbStats.totalEntries,
        size: dbStats.totalSizeBytes,
        hits: dbStats.totalHits,
        expired: dbStats.expiredEntries
      } : null
    }
  }

  return {
    isPlaying,
    isSupported: readonly(isSupported),
    speak,
    stop,
    loadVoices,
    getAllVoices,
    loadWebSpeechVoices,
    initializeVoices,
    currentTTSProvider: readonly(currentTTSProvider),
    setTTSProvider,
    webSpeechVoices: readonly(webSpeechVoices),
    openaiVoices: readonly(openaiVoices),
    clearCache,
    getCacheStats
  }
}