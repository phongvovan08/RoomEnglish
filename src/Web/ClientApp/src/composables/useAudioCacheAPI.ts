import { createAuthHeaders } from '@/utils/auth'

const API_BASE = '/api/audio-cache'

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
    try {
      const params = new URLSearchParams({
        text,
        voice,
        rate: rate.toString(),
        provider
      })

      const response = await fetch(`${API_BASE}?${params}`, {
        headers: createAuthHeaders()
      })

      if (response.status === 404) {
        return null // Not cached
      }

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`)
      }

      return await response.blob()
    } catch (error) {
      console.error('Error getting cached audio:', error)
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
    try {
      // Convert blob to base64
      const arrayBuffer = await audioBlob.arrayBuffer()
      const bytes = new Uint8Array(arrayBuffer)
      let binary = ''
      for (let i = 0; i < bytes.length; i++) {
        binary += String.fromCharCode(bytes[i])
      }
      const base64 = btoa(binary)

      const response = await fetch(API_BASE, {
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

      return response.ok
    } catch (error) {
      console.error('Error saving audio to cache:', error)
      return false
    }
  }

  const getCacheStats = async (): Promise<AudioCacheStats | null> => {
    try {
      const response = await fetch(`${API_BASE}/stats`, {
        headers: createAuthHeaders()
      })

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`)
      }

      return await response.json()
    } catch (error) {
      console.error('Error getting cache stats:', error)
      return null
    }
  }

  const cleanupCache = async (
    maxCacheSizeMB: number = 100,
    deleteExpired: boolean = true
  ): Promise<number> => {
    try {
      const response = await fetch(`${API_BASE}/cleanup`, {
        method: 'POST',
        headers: createAuthHeaders(),
        body: JSON.stringify({
          maxCacheSizeMB,
          deleteExpired
        })
      })

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`)
      }

      const result = await response.json()
      return result.deletedEntries || 0
    } catch (error) {
      console.error('Error cleaning up cache:', error)
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
