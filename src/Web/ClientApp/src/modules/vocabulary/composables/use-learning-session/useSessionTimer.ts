import { ref } from 'vue'

export const useSessionTimer = () => {
  const startTime = ref<number>(Date.now())
  const elapsedTime = ref(0)
  let timer: number | null = null

  const startTimer = () => {
    startTime.value = Date.now()
    elapsedTime.value = 0
    
    timer = window.setInterval(() => {
      elapsedTime.value = Math.floor((Date.now() - startTime.value) / 1000)
    }, 1000)
  }

  const stopTimer = () => {
    if (timer) {
      clearInterval(timer)
      timer = null
    }
  }

  const resetTimer = () => {
    stopTimer()
    startTime.value = Date.now()
    elapsedTime.value = 0
  }

  const formatTime = (seconds: number): string => {
    const mins = Math.floor(seconds / 60)
    const secs = seconds % 60
    return `${mins}:${secs.toString().padStart(2, '0')}`
  }

  return {
    elapsedTime,
    startTimer,
    stopTimer,
    resetTimer,
    formatTime
  }
}
