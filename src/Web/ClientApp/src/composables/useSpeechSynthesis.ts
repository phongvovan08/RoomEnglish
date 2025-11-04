import { ref, readonly } from 'vue'
// import { OpenAITTS } from '@lobehub/tts'  // REMOVED: Using direct fetch instead
import { useAudioCacheAPI } from './useAudioCacheAPI'

const { getCachedAudio, saveAudioToCache } = useAudioCacheAPI()

// Shared state (singleton pattern) - all components use same cache
const isSupported = ref(typeof window !== 'undefined' && 'speechSynthesis' in window)
const playingInstances = ref(new Set<string>())
const audioInstances = ref(new Map<string, HTMLAudioElement>())
const currentTTSProvider = ref<'openai' | 'webspeech'>('webspeech') // Default to Web Speech API
const webSpeechVoices = ref<SpeechSynthesisVoice[]>([])

// Memory cache for current session (faster) - with timestamps
// SHARED across all component instances!
const MAX_MEMORY_CACHE_SIZE = 100
const memoryCache = ref(new Map<string, Blob>())
const memoryCacheTimestamps = ref(new Map<string, number>())
const MEMORY_CACHE_TTL = 10 * 60 * 1000 // 10 minutes in milliseconds

// Background cleanup interval
let cleanupIntervalId: number | null = null

export const useSpeechSynthesis = () => {
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
    console.log('🆕 CODE VERSION: 2024-11-02-18:45 - USING DIRECT FETCH API') // MARKER TO VERIFY NEW CODE
    const allVoices = getAllVoices()
    const selectedVoice = allVoices[options.voiceIndex || 0]
    
    if (selectedVoice.provider !== 'openai') {
      console.error('❌ Invalid voice for OpenAI:', selectedVoice)
      return Promise.reject('Invalid voice for OpenAI')
    }

    const voiceName = selectedVoice.voiceName
    // Always request audio at 1.0 speed from OpenAI
    // We'll use playbackRate in browser to control speed
    const normalSpeed = 1.0
    const provider = 'openai'
    
    // Create cache key for memory cache (use normal speed for caching)
    const cacheKey = `${text}_${voiceName}_${normalSpeed}`
    
    // Check memory cache first (fastest)
    let cachedBlob = memoryCache.value.get(cacheKey)
    const cacheTimestamp = memoryCacheTimestamps.value.get(cacheKey)
    
    // Check if cache is expired (> 10 minutes)
    if (cachedBlob && cacheTimestamp) {
      const age = Date.now() - cacheTimestamp
      if (age > MEMORY_CACHE_TTL) {
        // Expired - remove from memory cache
        memoryCache.value.delete(cacheKey)
        memoryCacheTimestamps.value.delete(cacheKey)
        cachedBlob = undefined
      }
    }
    
    if (cachedBlob) {
      console.log('💾 Using cached audio from memory:', cachedBlob.size, 'bytes')
      
      // VALIDATE cached audio before using it
      if (cachedBlob.size < 1024) {
        console.warn('⚠️ Cached memory audio too small, clearing and refetching:', cachedBlob.size, 'bytes')
        memoryCache.value.delete(cacheKey)
        memoryCacheTimestamps.value.delete(cacheKey)
        // Continue to check database or fetch from API
      } else {
        // Validate MP3 format
        const arrayBuffer = await cachedBlob.arrayBuffer()
        const uint8Array = new Uint8Array(arrayBuffer)
        const isValidMP3 = (uint8Array[0] === 0xFF && (uint8Array[1] & 0xE0) === 0xE0) || // MP3 frame header
                           (uint8Array[0] === 0x49 && uint8Array[1] === 0x44 && uint8Array[2] === 0x33) // ID3 tag
        
        if (!isValidMP3) {
          console.warn('⚠️ Cached memory audio has invalid MP3 format, clearing and refetching. First bytes:', Array.from(uint8Array.slice(0, 10)))
          memoryCache.value.delete(cacheKey)
          memoryCacheTimestamps.value.delete(cacheKey)
          // Continue to check database or fetch from API
        } else {
          return arrayBuffer
        }
      }
    }
    
    // Check database cache
    const dbCachedBlob = await getCachedAudio(text, voiceName, normalSpeed, provider)
    
    if (dbCachedBlob) {
      console.log('💾 Using cached audio from database:', dbCachedBlob.size, 'bytes')
      
      // VALIDATE cached audio before using it
      if (dbCachedBlob.size < 1024) {
        console.warn('⚠️ Cached database audio too small, ignoring and refetching:', dbCachedBlob.size, 'bytes')
        // Don't use corrupted cache, continue to fetch from API
      } else {
        // Validate MP3 format
        const arrayBuffer = await dbCachedBlob.arrayBuffer()
        const uint8Array = new Uint8Array(arrayBuffer)
        const isValidMP3 = (uint8Array[0] === 0xFF && (uint8Array[1] & 0xE0) === 0xE0) || // MP3 frame header
                           (uint8Array[0] === 0x49 && uint8Array[1] === 0x44 && uint8Array[2] === 0x33) // ID3 tag
        
        if (!isValidMP3) {
          console.warn('⚠️ Cached database audio has invalid MP3 format, ignoring. First bytes:', Array.from(uint8Array.slice(0, 10)))
          // Don't use corrupted cache, continue to fetch from API
        } else {
          // Save to memory for faster future access
          memoryCache.value.set(cacheKey, dbCachedBlob)
          memoryCacheTimestamps.value.set(cacheKey, Date.now())
          return arrayBuffer
        }
      }
    }
    
    console.log('🌐 No cache found, fetching from OpenAI API...')

    // Not cached, fetch from API
    const apiKey = import.meta.env.VITE_OPENAI_API_KEY || localStorage.getItem('openai_api_key')
    
    if (!apiKey) {
      throw new Error('OpenAI API key not found. Please add VITE_OPENAI_API_KEY to environment or set it in settings.')
    }

    console.log('🔑 Using API key:', apiKey.substring(0, 10) + '...' + apiKey.substring(apiKey.length - 4))
    console.log('🔑 API key length:', apiKey.length)

    // Call OpenAI API directly (instead of using @lobehub/tts which has issues)
    const payload = {
      model: 'tts-1',
      input: text,
      voice: voiceName,
      speed: normalSpeed
    }

    console.log('📤 OpenAI TTS payload:', payload)

    try {
      const response = await fetch('https://api.openai.com/v1/audio/speech', {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${apiKey}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(payload)
      })
      
      console.log('📡 OpenAI response status:', response.status, response.statusText)
      console.log('📡 OpenAI response headers:', Object.fromEntries(response.headers.entries()))
      
      // ALWAYS check what we received, even if status is 200
      const arrayBuffer = await response.arrayBuffer()
      console.log('📦 Received arrayBuffer:', arrayBuffer.byteLength, 'bytes')
      
      // Decode first 500 bytes as text to see if it's an error message
      const uint8Array = new Uint8Array(arrayBuffer)
      const textDecoder = new TextDecoder()
      const preview = textDecoder.decode(uint8Array.slice(0, Math.min(500, arrayBuffer.byteLength)))
      console.log('📄 Response preview (first 500 bytes):', preview)
      console.log('🔢 First 10 bytes:', Array.from(uint8Array.slice(0, 10)))
      
      if (response.ok) {
        
        // Validate audio data
        if (!arrayBuffer || arrayBuffer.byteLength === 0) {
          console.error('❌ OpenAI returned empty audio data')
          throw new Error('Received empty audio data from OpenAI. Please check your API key and quota.')
        }
        
        console.log(`✅ OpenAI TTS: Received ${arrayBuffer.byteLength} bytes`)
        
        // If received data is too small (< 1KB), it's likely an error response
        if (arrayBuffer.byteLength < 1024) {
          console.warn('⚠️ Received suspiciously small audio data:', arrayBuffer.byteLength, 'bytes')
          const textDecoder = new TextDecoder()
          const text = textDecoder.decode(arrayBuffer)
          console.error('Small response content:', text)
          throw new Error(`Invalid audio data (${arrayBuffer.byteLength} bytes). Response: ${text.substring(0, 200)}`)
        }
        
        // Check if response is actually audio (MP3 starts with ID3 or 0xFF)
        const uint8Array = new Uint8Array(arrayBuffer)
        const isValidMP3 = (uint8Array[0] === 0xFF && (uint8Array[1] & 0xE0) === 0xE0) || // MP3 frame header
                           (uint8Array[0] === 0x49 && uint8Array[1] === 0x44 && uint8Array[2] === 0x33) // ID3 tag
        
        if (!isValidMP3) {
          console.error('❌ Invalid audio format. First bytes:', Array.from(uint8Array.slice(0, 10)))
          // Try to decode as text to see error message
          const textDecoder = new TextDecoder()
          const text = textDecoder.decode(arrayBuffer)
          console.error('Response content:', text.substring(0, 500))
          throw new Error('Invalid audio format received from OpenAI. Check console for details.')
        }
        
        // Cache the audio blob
        const blob = new Blob([arrayBuffer], { type: 'audio/mpeg' })
        
        // Save to memory cache (instant) with timestamp
        // LRU eviction: If cache full, remove oldest entry
        if (memoryCache.value.size >= MAX_MEMORY_CACHE_SIZE) {
          let oldestKey: string | null = null
          let oldestTime = Date.now()
          
          for (const [key, timestamp] of memoryCacheTimestamps.value.entries()) {
            if (timestamp < oldestTime) {
              oldestTime = timestamp
              oldestKey = key
            }
          }
          
          if (oldestKey) {
            console.log('🗑️ Memory cache full, removing oldest entry:', oldestKey)
            memoryCache.value.delete(oldestKey)
            memoryCacheTimestamps.value.delete(oldestKey)
          }
        }
        
        memoryCache.value.set(cacheKey, blob)
        memoryCacheTimestamps.value.set(cacheKey, Date.now())
        
        // Save to database (async, fire and forget)
        saveAudioToCache(text, voiceName, normalSpeed, provider, blob, 30).catch(err => {
          console.error('Failed to save to database cache:', err)
        })
        
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
        utterance.rate = options.rate !== undefined ? options.rate : 0.8
        utterance.pitch = options.pitch || 1.0
        utterance.volume = options.volume || 1.0

        // Use voiceIndex from options (calculated for Web Speech API)
        if (options.voiceIndex !== undefined && webSpeechVoices.value[options.voiceIndex]) {
          const selectedVoice = webSpeechVoices.value[options.voiceIndex]
          utterance.voice = selectedVoice
        } else {
          // Fallback: find first English voice
          const fallbackVoice = webSpeechVoices.value.find(voice => 
            voice.lang.startsWith('en') && voice.localService
          ) || webSpeechVoices.value.find(voice => voice.lang.startsWith('en'))
          if (fallbackVoice) {
            utterance.voice = fallbackVoice
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
    console.log('🔊 SPEAK CALLED:', { text: text.substring(0, 50), instanceId, provider: options.provider })
    
    // Ensure voices are loaded
    await loadWebSpeechVoices()
    
    const allVoices = getAllVoices()
    const selectedVoice = allVoices[options.voiceIndex || 0]
    const provider = options.provider || selectedVoice?.provider || currentTTSProvider.value

    // Stop any existing audio for this instance first
    if (playingInstances.value.has(instanceId)) {
      stop(instanceId)
      // Wait a bit for cleanup
      await new Promise(resolve => setTimeout(resolve, 100))
    }

    try {
      playingInstances.value.add(instanceId)

      // Route to appropriate TTS provider
      if (provider === 'openai') {
        try {
          const audioBuffer = await speakWithOpenAI(text, instanceId, options)
          
          console.log('🎵 Creating audio blob from buffer:', audioBuffer.byteLength, 'bytes')
          
          // Play audio from buffer using Web Audio API for playback rate control
          const blob = new Blob([audioBuffer], { type: 'audio/mpeg' })
          console.log('🎵 Blob created:', blob.size, 'bytes, type:', blob.type)
          
          const audioUrl = URL.createObjectURL(blob)
          console.log('🎵 Blob URL created:', audioUrl)
          
          // Create audio element
          const audio = new Audio(audioUrl)
          console.log('🎵 Audio element created, readyState:', audio.readyState)
          
          audioInstances.value.set(instanceId, audio)
          audio.volume = options.volume || 1.0
          audio.autoplay = false
          
          // Set playback rate using HTMLAudioElement API
          const playbackRate = options.rate !== undefined ? options.rate : 1.0
          audio.playbackRate = playbackRate

          // Store cache key for cleanup on error
          const allVoices = getAllVoices()
          const selectedVoice = allVoices[options.voiceIndex || 0]
          const voiceName = selectedVoice.voiceName
          const normalSpeed = 1.0
          const cacheKey = `${text}_${voiceName}_${normalSpeed}`

          return new Promise<void>((resolve, reject) => {
            const cleanup = () => {
              playingInstances.value.delete(instanceId)
              audioInstances.value.delete(instanceId)
              URL.revokeObjectURL(audioUrl)
            }

            audio.onended = () => {
              cleanup()
              resolve()
            }

            audio.onerror = (event) => {
              console.error('❌ Audio playback error:', event, audio.error)
              
              let errorMessage = 'Audio playback failed'
              if (audio.error) {
                const errorCode = audio.error.code
                const errorMsg = audio.error.message
                
                console.error('   Error code:', errorCode, 'Message:', errorMsg)
                
                // Provide helpful error messages based on error code
                if (errorCode === 4) { // MEDIA_ERR_SRC_NOT_SUPPORTED or DEMUXER_ERROR
                  errorMessage = 'Audio format not supported or corrupted. This may be due to an invalid OpenAI API key or quota exceeded.'
                  
                  // Clear corrupted cache
                  console.warn('🗑️ Clearing corrupted audio from memory cache:', cacheKey)
                  memoryCache.value.delete(cacheKey)
                  memoryCacheTimestamps.value.delete(cacheKey)
                  
                } else if (errorCode === 3) { // MEDIA_ERR_DECODE
                  errorMessage = 'Failed to decode audio. The audio data may be corrupted.'
                  
                  // Clear corrupted cache
                  console.warn('🗑️ Clearing corrupted audio from memory cache:', cacheKey)
                  memoryCache.value.delete(cacheKey)
                  memoryCacheTimestamps.value.delete(cacheKey)
                  
                } else if (errorCode === 2) { // MEDIA_ERR_NETWORK
                  errorMessage = 'Network error while loading audio.'
                } else if (errorCode === 1) { // MEDIA_ERR_ABORTED
                  errorMessage = 'Audio loading was aborted.'
                }
              }
              
              cleanup()
              reject(new Error(errorMessage))
            }
            
            audio.onloadeddata = () => {
              // Ensure playback rate is set after data is loaded
              audio.playbackRate = playbackRate
              // Try to play as soon as data is loaded
              attemptPlay()
            }

            // Wait for audio to be ready before playing
            const attemptPlay = () => {
              // Ensure playback rate is set right before playing
              audio.playbackRate = playbackRate
              
              const playPromise = audio.play()
              if (playPromise !== undefined) {
                playPromise
                  .then(() => {
                    // Audio playing successfully
                  })
                  .catch((error) => {
                    console.error('❌ Audio.play() failed:', error.name, error.message)
                    cleanup()
                    // Only reject if it's not an abort error from stopping
                    if (error.name !== 'AbortError') {
                      reject(error)
                    } else {
                      // Silently handle abort errors
                      resolve()
                    }
                  })
              }
            }
            
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
    console.log('⏹️ STOP CALLED:', instanceId || 'ALL')
    
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
    startBackgroundCleanup() // Start cleanup timer
  }

  // Cleanup expired memory cache entries
  const cleanupExpiredMemoryCache = () => {
    const now = Date.now()
    let cleanedCount = 0
    
    for (const [key, timestamp] of memoryCacheTimestamps.value.entries()) {
      const age = now - timestamp
      if (age > MEMORY_CACHE_TTL) {
        memoryCache.value.delete(key)
        memoryCacheTimestamps.value.delete(key)
        cleanedCount++
      }
    }
    
    if (cleanedCount > 0) {
      console.log(`🧹 Auto-cleaned ${cleanedCount} expired memory cache entries`)
    }
    
    return cleanedCount
  }

  // Start background cleanup (runs every 2 minutes)
  const startBackgroundCleanup = () => {
    if (cleanupIntervalId !== null) return // Already running
    
    cleanupIntervalId = window.setInterval(() => {
      cleanupExpiredMemoryCache()
    }, 2 * 60 * 1000) // Check every 2 minutes
    
    console.log('🔄 Background cache cleanup started (every 2 minutes)')
  }

  // Stop background cleanup
  const stopBackgroundCleanup = () => {
    if (cleanupIntervalId !== null) {
      clearInterval(cleanupIntervalId)
      cleanupIntervalId = null
      console.log('⏹️ Background cache cleanup stopped')
    }
  }

  // Clear audio cache (useful for memory management)
  const clearCache = () => {
    const count = memoryCache.value.size
    memoryCache.value.clear()
    memoryCacheTimestamps.value.clear()
    console.log(`🧹 Memory cache cleared (${count} entries deleted)`)
    return count
  }

  // Get cache stats
  const getCacheStats = () => {
    // Calculate oldest entry age
    const now = Date.now()
    let oldestAge = 0
    for (const timestamp of memoryCacheTimestamps.value.values()) {
      const age = now - timestamp
      if (age > oldestAge) oldestAge = age
    }
    
    const stats = {
      memory: {
        count: memoryCache.value.size,
        maxSize: MAX_MEMORY_CACHE_SIZE,
        size: Array.from(memoryCache.value.values()).reduce((sum, blob) => sum + blob.size, 0),
        ttl: MEMORY_CACHE_TTL,
        oldestEntryAge: oldestAge
      }
    }
    
    // Pretty print to console
    console.log('📊 Audio Cache Statistics:')
    console.log(`   Entries: ${stats.memory.count} / ${stats.memory.maxSize}`)
    console.log(`   Total Size: ${(stats.memory.size / 1024 / 1024).toFixed(2)} MB`)
    console.log(`   TTL: ${stats.memory.ttl / 1000 / 60} minutes`)
    console.log(`   Oldest Entry: ${(stats.memory.oldestEntryAge / 1000).toFixed(0)} seconds ago`)
    
    return stats
  }
  
  // List all cached entries (for debugging)
  const listCachedEntries = () => {
    const now = Date.now()
    const entries: Array<{key: string, size: number, age: number}> = []
    
    for (const [key, blob] of memoryCache.value.entries()) {
      const timestamp = memoryCacheTimestamps.value.get(key) || now
      const age = now - timestamp
      entries.push({
        key,
        size: blob.size,
        age: Math.floor(age / 1000) // seconds
      })
    }
    
    // Sort by age (newest first)
    entries.sort((a, b) => a.age - b.age)
    
    console.log('📋 Cached Audio Entries:')
    entries.forEach((entry, i) => {
      console.log(`   ${i + 1}. ${entry.key.substring(0, 50)}... (${(entry.size / 1024).toFixed(1)} KB, ${entry.age}s ago)`)
    })
    
    return entries
  }

  // Clear specific cache entry (useful when corrupted)
  const clearCacheEntry = (text: string, voice?: string, rate?: number) => {
    const cacheKey = `${text}_${voice || 'alloy'}_${(rate || 1.0).toFixed(2)}`
    const deleted = memoryCache.value.delete(cacheKey)
    memoryCacheTimestamps.value.delete(cacheKey)
    
    if (deleted) {
      console.log(`🗑️ Cleared cache entry: ${cacheKey}`)
    } else {
      console.warn(`⚠️ Cache entry not found: ${cacheKey}`)
    }
    
    return deleted
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
    clearCacheEntry, // NEW: Clear specific cache entry
    getCacheStats,
    listCachedEntries, // NEW: List all cached entries
    cleanupExpiredMemoryCache, // Expose for manual cleanup
    stopBackgroundCleanup // Expose for cleanup on unmount
  }
}