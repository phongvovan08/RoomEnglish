<template>
  <div class="audio-section">
    <div class="audio-controls">
      <button 
        @click="$emit('play')" 
        :disabled="!hasAudio || isPlaying"
        class="play-btn"
        :class="{ 'playing': isPlaying }"
      >
        <i class="mdi" :class="isPlaying ? 'mdi-volume-high' : 'mdi-play-circle'"></i>
        <span>{{ isPlaying ? 'Playing...' : 'Play Audio' }}</span>
      </button>
      
      <div class="playback-controls" v-if="hasAudio">
        <button @click="$emit('change-speed')" class="speed-btn">
          <i class="mdi mdi-speedometer"></i>
          {{ playbackSpeed }}x
        </button>
        
        <div class="play-count">
          Played: {{ playCount }} times
        </div>
      </div>
    </div>

    <div class="audio-waveform">
      <div class="waveform-bars">
        <div 
          v-for="i in 20" 
          :key="i"
          class="bar"
          :class="{ 'active': isPlaying }"
          :style="{ animationDelay: `${i * 50}ms` }"
        ></div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
interface Props {
  hasAudio: boolean
  isPlaying: boolean
  playbackSpeed: number
  playCount: number
}

defineProps<Props>()

defineEmits<{
  play: []
  'change-speed': []
}>()
</script>

<style scoped>
.audio-section {
  margin-bottom: 2rem;
}

.audio-controls {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}

.play-btn {
  background: linear-gradient(135deg, #e75e8d, #74c0fc);
  color: white;
  border: none;
  padding: 1rem 2rem;
  border-radius: 25px;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 0.75rem;
  font-size: 1.1rem;
  transition: all 0.3s ease;
  min-width: 160px;
}

.play-btn:hover:not(:disabled) {
  transform: translateY(-3px);
  box-shadow: 0 8px 25px rgba(231, 94, 141, 0.4);
}

.play-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.play-btn.playing {
  animation: pulse 2s infinite;
}

.playback-controls {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.speed-btn {
  background: rgba(116, 192, 252, 0.2);
  color: #74c0fc;
  border: 1px solid rgba(116, 192, 252, 0.5);
  border-radius: 15px;
  padding: 0.5rem 1rem;
  cursor: pointer;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.speed-btn:hover {
  background: rgba(116, 192, 252, 0.3);
}

.play-count {
  color: #b8b8b8;
  font-size: 0.9rem;
}

.audio-waveform {
  background: rgba(255, 255, 255, 0.05);
  border-radius: 15px;
  padding: 1rem;
  display: flex;
  justify-content: center;
  align-items: center;
  height: 80px;
}

.waveform-bars {
  display: flex;
  align-items: end;
  gap: 3px;
  height: 50px;
}

.bar {
  width: 4px;
  background: linear-gradient(to top, #e75e8d, #74c0fc);
  border-radius: 2px;
  height: 20px;
  transition: height 0.3s ease;
}

.bar.active {
  animation: waveform 1.5s ease-in-out infinite;
}

@keyframes pulse {
  0% { box-shadow: 0 0 0 0 rgba(231, 94, 141, 0.7); }
  70% { box-shadow: 0 0 0 10px rgba(231, 94, 141, 0); }
  100% { box-shadow: 0 0 0 0 rgba(231, 94, 141, 0); }
}

@keyframes waveform {
  0%, 100% { height: 20px; }
  50% { height: 45px; }
}

@media (max-width: 768px) {
  .audio-controls {
    flex-direction: column;
    gap: 1rem;
    text-align: center;
  }
}
</style>
