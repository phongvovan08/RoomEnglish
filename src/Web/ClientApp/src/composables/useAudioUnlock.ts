/**
 * Audio Unlock Composable
 * Unlocks audio playback for browsers with autoplay restrictions
 * Call unlockAudio() on first user interaction (click, touch, etc.)
 */

import { ref } from 'vue'

const isAudioUnlocked = ref(false)

export const useAudioUnlock = () => {
  const unlockAudio = async () => {
    if (isAudioUnlocked.value) {
      console.log('🔓 Audio already unlocked')
      return
    }

    try {
      // Create a silent audio buffer
      const audioContext = new (window.AudioContext || (window as any).webkitAudioContext)()
      const buffer = audioContext.createBuffer(1, 1, 22050)
      const source = audioContext.createBufferSource()
      source.buffer = buffer
      source.connect(audioContext.destination)
      
      // Play silent audio to unlock
      source.start(0)
      
      // Also create and play a silent HTML5 audio element
      const audio = new Audio()
      audio.src = 'data:audio/mp3;base64,SUQzBAAAAAAAI1RTU0UAAAAPAAADTGF2ZjU4Ljc2LjEwMAAAAAAAAAAAAAAA//tQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAWGluZwAAAA8AAAACAAADhQC7u7u7u7u7u7u7u7u7u7u7u7u7u7u7u7u7u7u7u7u7u7u7u7u7u7u7u7u7u7u7u7u7//////////////////////////////////////////////////////////////////8AAAAATGF2YzU4LjEzAAAAAAAAAAAAAAAAJAAAAAAAAAAAA4WTQB7fAAAAAAAAAAAAAAAAAAAAAP/7UGQAAANUMEoFPeACNQV40KEYABEY41g5vAAA9RjpZxRwAImU+W8eshaFpAQgALAAYALATgACAa4AEBwAKBQf9kYHjRYc+8e0+L3Ua/GgKEAiWBC1gDAnzBMfJnJ8lv0+WQ38e7N5z/+z//uG/s+xU7pu5//pL5uWdv/r//u///Z/+j//r/r///rf/6+v//r///u///rf9P5z8///////Lb7rJvVf////t//7///f+3+f//////+fqe/////m6e////////rf///+v/8=C8NZAAAAAAAAAAAAAAAA//tQxAAABJQlSmPEgABABSN0eeAAE5QBmDmYAQDEDJgQIDBAGQg8BFEiAA5HADgEA6hQBBAAA//z/n8AgAAFhAAIAkv/9zs+///////+p7/z//////////p9/6v///////+j//+r/+/yP///0//z///////+r///+R/////6f/v/////P//0/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////8='
      audio.volume = 0.01 // Very low volume
      
      await audio.play()
      
      isAudioUnlocked.value = true
      console.log('🔓 Audio unlocked successfully!')
      
      // Clean up
      audio.pause()
      audio.src = ''
      audioContext.close()
      
    } catch (error) {
      console.warn('⚠️ Failed to unlock audio:', error)
    }
  }

  return {
    isAudioUnlocked,
    unlockAudio
  }
}
