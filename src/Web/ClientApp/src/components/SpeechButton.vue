<template>
  <button 
    v-if="isSupported" 
    @click="handlePlay"
    :disabled="isPlaying"
    :class="buttonClass"
    :title="isPlaying ? 'Playing...' : 'Click to listen'"
  >
    <i class="mdi" :class="isPlaying ? 'mdi-volume-variant-off' : 'mdi-volume-high'"></i>
    <span v-if="showText">{{ isPlaying ? 'Playing...' : 'Listen' }}</span>
  </button>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useSpeechSynthesis } from '@/composables/useSpeechSynthesis'

interface Props {
  text: string
  instanceId: string
  lang?: string
  showText?: boolean
  buttonClass?: string
}

const props = withDefaults(defineProps<Props>(), {
  lang: 'en-US',
  showText: false,
  buttonClass: 'speech-btn'
})

const { speak, isPlaying: checkIsPlaying, isSupported } = useSpeechSynthesis()

const isPlaying = computed(() => checkIsPlaying(props.instanceId))

const handlePlay = async () => {
  if (isPlaying.value) return
  
  try {
    await speak(props.text, props.instanceId, { lang: props.lang })
  } catch (error) {
    console.error('Failed to play audio:', error)
  }
}
</script>

<style scoped>
.speech-btn {
  background: rgba(231, 94, 141, 0.3);
  border: 1px solid rgba(231, 94, 141, 0.5);
  border-radius: 50%;
  width: 35px;
  height: 35px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  color: #e75e8d;
  transition: all 0.3s ease;
}

.speech-btn:hover:not(:disabled) {
  background: rgba(231, 94, 141, 0.5);
  transform: scale(1.1);
}

.speech-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
  animation: pulse 1.5s infinite;
}

.speech-btn.large {
  background: linear-gradient(135deg, #e75e8d, #74c0fc);
  color: white;
  border: none;
  padding: 0.75rem 1.5rem;
  border-radius: 25px;
  width: auto;
  height: auto;
  gap: 0.5rem;
  font-size: 1rem;
}

.speech-btn.large:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 5px 15px rgba(231, 94, 141, 0.4);
}

@keyframes pulse {
  0%, 100% { opacity: 0.5; }
  50% { opacity: 0.8; }
}
</style>