import { ref } from 'vue'

interface CachedAudio {
  text: string
  voice: string
  rate: number
  audioData: string // Base64 encoded
  timestamp: number
  size: number
}

const CACHE_KEY_PREFIX = 'audio_cache_'
const MAX_CACHE_SIZE = 50 * 1024 * 1024 // 50MB
const MAX_CACHE_AGE = 7 * 24 * 60 * 60 * 1000 // 7 days

export function useAudioCache() {
  const memoryCache = ref(new Map<string, Blob>())

  const getCacheKey = (text: string, voice: string, rate: number = 1.0) => {
    return `${CACHE_KEY_PREFIX}${text}_${voice}_${rate}`
  }

  const getFromMemory = (text: string, voice: string, rate: number = 1.0): Blob | null => {
    const key = getCacheKey(text, voice, rate)
    return memoryCache.value.get(key) || null
  }

  const getFromLocalStorage = async (text: string, voice: string, rate: number = 1.0): Promise<Blob | null> => {
    try {
      const key = getCacheKey(text, voice, rate)
      const cached = localStorage.getItem(key)
      
      if (!cached) return null

      const data: CachedAudio = JSON.parse(cached)
      
      // Check if cache is expired
      if (Date.now() - data.timestamp > MAX_CACHE_AGE) {
        localStorage.removeItem(key)
        return null
      }

      // Convert base64 back to blob
      const binaryString = atob(data.audioData)
      const bytes = new Uint8Array(binaryString.length)
      for (let i = 0; i < binaryString.length; i++) {
        bytes[i] = binaryString.charCodeAt(i)
      }
      const blob = new Blob([bytes], { type: 'audio/mpeg' })
      
      // Store in memory cache for faster access
      memoryCache.value.set(key, blob)
      
      return blob
    } catch (error) {
      console.error('Error reading from audio cache:', error)
      return null
    }
  }

  const saveToMemory = (text: string, voice: string, rate: number, blob: Blob) => {
    const key = getCacheKey(text, voice, rate)
    memoryCache.value.set(key, blob)
  }

  const saveToLocalStorage = async (text: string, voice: string, rate: number, blob: Blob) => {
    try {
      // Only cache reasonable sized audio (< 5MB per file)
      if (blob.size > 5 * 1024 * 1024) {
        console.warn('Audio too large to cache:', blob.size)
        return
      }

      // Check total cache size
      const currentSize = getTotalCacheSize()
      if (currentSize + blob.size > MAX_CACHE_SIZE) {
        console.warn('Cache size limit reached, clearing old entries')
        clearOldestEntries()
      }

      const key = getCacheKey(text, voice, rate)
      const arrayBuffer = await blob.arrayBuffer()
      const bytes = new Uint8Array(arrayBuffer)
      
      // Convert to base64
      let binary = ''
      for (let i = 0; i < bytes.length; i++) {
        binary += String.fromCharCode(bytes[i])
      }
      const base64 = btoa(binary)

      const cacheData: CachedAudio = {
        text: text.substring(0, 100), // Store truncated text for reference
        voice,
        rate,
        audioData: base64,
        timestamp: Date.now(),
        size: blob.size
      }

      localStorage.setItem(key, JSON.stringify(cacheData))
      console.log(`💾 Cached audio to localStorage: ${text.substring(0, 30)}... (${blob.size} bytes)`)
    } catch (error) {
      console.error('Error saving to audio cache:', error)
      // If quota exceeded, clear some cache
      if (error instanceof DOMException && error.name === 'QuotaExceededError') {
        clearOldestEntries()
      }
    }
  }

  const getTotalCacheSize = (): number => {
    let total = 0
    for (let i = 0; i < localStorage.length; i++) {
      const key = localStorage.key(i)
      if (key?.startsWith(CACHE_KEY_PREFIX)) {
        const item = localStorage.getItem(key)
        if (item) {
          try {
            const data: CachedAudio = JSON.parse(item)
            total += data.size
          } catch (e) {
            // Invalid cache entry, remove it
            localStorage.removeItem(key)
          }
        }
      }
    }
    return total
  }

  const clearOldestEntries = () => {
    const entries: Array<{ key: string; timestamp: number; size: number }> = []
    
    for (let i = 0; i < localStorage.length; i++) {
      const key = localStorage.key(i)
      if (key?.startsWith(CACHE_KEY_PREFIX)) {
        const item = localStorage.getItem(key)
        if (item) {
          try {
            const data: CachedAudio = JSON.parse(item)
            entries.push({ key, timestamp: data.timestamp, size: data.size })
          } catch (e) {
            localStorage.removeItem(key)
          }
        }
      }
    }

    // Sort by timestamp (oldest first)
    entries.sort((a, b) => a.timestamp - b.timestamp)

    // Remove oldest 30% of entries
    const toRemove = Math.ceil(entries.length * 0.3)
    for (let i = 0; i < toRemove; i++) {
      localStorage.removeItem(entries[i].key)
      console.log(`🧹 Removed old cache entry: ${entries[i].key}`)
    }
  }

  const clearAllCache = () => {
    // Clear memory cache
    memoryCache.value.clear()

    // Clear localStorage cache
    const keys: string[] = []
    for (let i = 0; i < localStorage.length; i++) {
      const key = localStorage.key(i)
      if (key?.startsWith(CACHE_KEY_PREFIX)) {
        keys.push(key)
      }
    }
    keys.forEach(key => localStorage.removeItem(key))
    
    console.log(`🧹 Cleared ${keys.length} cache entries`)
  }

  const getCacheStats = () => {
    const memoryCount = memoryCache.value.size
    const memorySize = Array.from(memoryCache.value.values()).reduce((sum, blob) => sum + blob.size, 0)
    
    let storageCount = 0
    let storageSize = 0
    for (let i = 0; i < localStorage.length; i++) {
      const key = localStorage.key(i)
      if (key?.startsWith(CACHE_KEY_PREFIX)) {
        storageCount++
        const item = localStorage.getItem(key)
        if (item) {
          try {
            const data: CachedAudio = JSON.parse(item)
            storageSize += data.size
          } catch (e) {
            // ignore
          }
        }
      }
    }

    return {
      memory: { count: memoryCount, size: memorySize },
      storage: { count: storageCount, size: storageSize },
      total: { count: memoryCount + storageCount, size: memorySize + storageSize }
    }
  }

  return {
    getFromMemory,
    getFromLocalStorage,
    saveToMemory,
    saveToLocalStorage,
    clearAllCache,
    getCacheStats
  }
}
