import { createAuthHeaders } from '@/utils/auth'
import { API_CONFIG } from '@/config/api.config'

const API_BASE = `${API_CONFIG.baseURL}/api/audio-cache`

export interface AudioCacheStats {
  totalEntries: number
  totalSizeBytes: number
  expiredEntries: number
  oldestEntry?: string
  newestEntry?: string
  totalHits: number
  averageSizeKB: number
}

export function useAudioCacheAPI() {
  const getCachedAudio = async (
    text: string,
    voice: string,
    rate: number = 1.0,
    provider: string = 'openai'
  ): Promise<Blob | null> => {
    const params = new URLSearchParams({
      text,
      voice,
      rate: rate.toString(),
      provider
    })

    let response: Response | null = null
    
    try {
      response = await fetch(`${API_BASE}?${params}`, {
        headers: createAuthHeaders()
      })
    } catch (err: any) {
      console.log('💤 Audio cache backend not available:', err.message)
      return null
    }

    if (!response) {
      return null
    }

    if (response.status === 404) {
      console.log('💾 Cache miss - audio not in database')
      return null
    }

    if (!response.ok) {
      console.warn('⚠️ Cache fetch failed:', response.status, response.statusText)
      return null
    }

    try {
      console.log('✅ Cache hit - loaded from database')
      return await response.blob()
    } catch (err) {
      console.log('💤 Failed to read cache blob:', err)
      return null
    }
  }

  const saveAudioToCache = async (
    text: string,
    voice: string,
    rate: number,
    provider: string,
    audioBlob: Blob,
    expiryDays: number = 30
  ): Promise<boolean> => {
    // Convert blob to base64
    let base64: string
    try {
      const arrayBuffer = await audioBlob.arrayBuffer()
      const bytes = new Uint8Array(arrayBuffer)
      let binary = ''
      for (let i = 0; i < bytes.length; i++) {
        binary += String.fromCharCode(bytes[i])
      }
      base64 = btoa(binary)
    } catch (err) {
      console.log('💤 Failed to convert audio to base64:', err)
      return false
    }

    let response: Response | null = null
    
    try {
      response = await fetch(API_BASE, {
        method: 'POST',
        headers: createAuthHeaders(),
        body: JSON.stringify({
          text,
          voice,
          rate,
          provider,
          audioDataBase64: base64,
          mimeType: audioBlob.type || 'audio/mpeg',
          expiryDays
        })
      })
    } catch (err: any) {
      console.log('💤 Audio cache backend not available:', err.message)
      return false
    }

    if (!response) {
      return false
    }

    if (response.ok) {
      console.log('✅ Audio saved to database cache')
      return true
    }

    console.warn('⚠️ Failed to save audio to cache:', response.status, response.statusText)
    return false
  }

  const getCacheStats = async (): Promise<AudioCacheStats | null> => {
    let response: Response | null = null
    
    try {
      response = await fetch(`${API_BASE}/stats`, {
        headers: createAuthHeaders()
      })
    } catch (err: any) {
      console.log('💤 Audio cache stats backend not available:', err.message)
      return null
    }

    if (!response || !response.ok) {
      return null
    }

    try {
      console.log('✅ Cache stats loaded from database')
      return await response.json()
    } catch (err) {
      console.log('💤 Failed to parse cache stats:', err)
      return null
    }
  }

  const cleanupCache = async (
    maxCacheSizeMB: number = 100,
    deleteExpired: boolean = true
  ): Promise<number> => {
    let response: Response | null = null
    
    try {
      response = await fetch(`${API_BASE}/cleanup`, {
        method: 'POST',
        headers: createAuthHeaders(),
        body: JSON.stringify({
          maxCacheSizeMB,
          deleteExpired
        })
      })
    } catch (err: any) {
      console.log('💤 Audio cache cleanup backend not available:', err.message)
      return 0
    }

    if (!response || !response.ok) {
      return 0
    }

    try {
      const result = await response.json()
      console.log(`✅ Cache cleanup completed - deleted ${result.deletedEntries} entries`)
      return result.deletedEntries || 0
    } catch (err) {
      console.log('💤 Failed to parse cleanup result:', err)
      return 0
    }
  }

  return {
    getCachedAudio,
    saveAudioToCache,
    getCacheStats,
    cleanupCache
  }
}
