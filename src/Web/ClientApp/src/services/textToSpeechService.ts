/**
 * Text-to-Speech Service
 * Supports both Web Speech API (browser) and OpenAI TTS API
 */

export class TextToSpeechService {
  private synthesis: SpeechSynthesis | null = null
  private currentUtterance: SpeechSynthesisUtterance | null = null
  private isPlaying: boolean = false

  constructor() {
    if ('speechSynthesis' in window) {
      this.synthesis = window.speechSynthesis
    }
  }

  /**
   * Speak text using Web Speech API
   */
  async speak(text: string, options?: {
    rate?: number // 0.1 to 10 (default: 1)
    pitch?: number // 0 to 2 (default: 1)
    volume?: number // 0 to 1 (default: 1)
    lang?: string // default: 'en-US'
    voice?: SpeechSynthesisVoice
  }): Promise<void> {
    return new Promise((resolve, reject) => {
      if (!this.synthesis) {
        reject(new Error('Speech synthesis not supported'))
        return
      }

      // Stop current speech if playing
      this.stop()

      const utterance = new SpeechSynthesisUtterance(text)
      
      // Set options
      utterance.rate = options?.rate ?? 1
      utterance.pitch = options?.pitch ?? 1
      utterance.volume = options?.volume ?? 1
      utterance.lang = options?.lang ?? 'en-US'
      
      if (options?.voice) {
        utterance.voice = options.voice
      }

      // Event handlers
      utterance.onstart = () => {
        this.isPlaying = true
      }

      utterance.onend = () => {
        this.isPlaying = false
        this.currentUtterance = null
        resolve()
      }

      utterance.onerror = (event) => {
        this.isPlaying = false
        this.currentUtterance = null
        reject(new Error(`Speech synthesis error: ${event.error}`))
      }

      this.currentUtterance = utterance
      this.synthesis.speak(utterance)
    })
  }

  /**
   * Stop current speech
   */
  stop(): void {
    if (this.synthesis) {
      this.synthesis.cancel()
      this.isPlaying = false
      this.currentUtterance = null
    }
  }

  /**
   * Pause current speech
   */
  pause(): void {
    if (this.synthesis && this.isPlaying) {
      this.synthesis.pause()
    }
  }

  /**
   * Resume paused speech
   */
  resume(): void {
    if (this.synthesis) {
      this.synthesis.resume()
    }
  }

  /**
   * Get available voices
   */
  getVoices(): SpeechSynthesisVoice[] {
    if (!this.synthesis) return []
    return this.synthesis.getVoices()
  }

  /**
   * Get English voices only
   */
  getEnglishVoices(): SpeechSynthesisVoice[] {
    return this.getVoices().filter(voice => 
      voice.lang.startsWith('en')
    )
  }

  /**
   * Find best voice for language
   */
  getBestVoice(lang: string = 'en-US'): SpeechSynthesisVoice | null {
    const voices = this.getVoices()
    
    // Try to find exact match
    let voice = voices.find(v => v.lang === lang && v.default)
    if (voice) return voice
    
    // Try to find language match
    voice = voices.find(v => v.lang.startsWith(lang.split('-')[0]))
    if (voice) return voice
    
    // Return first available
    return voices[0] || null
  }

  /**
   * Check if currently speaking
   */
  isSpeaking(): boolean {
    return this.isPlaying
  }

  /**
   * Check if TTS is supported
   */
  isSupported(): boolean {
    return !!this.synthesis
  }
}

// Singleton instance
export const ttsService = new TextToSpeechService()

// Helper function for quick usage
export async function speakText(
  text: string, 
  rate: number = 1
): Promise<void> {
  return ttsService.speak(text, { 
    rate,
    lang: 'en-US'
  })
}
