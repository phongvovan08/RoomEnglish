<template>
  <div class="progress-container">
    <div class="progress-bar">
      <div 
        class="progress-fill"
        :style="{ width: `${progress}%` }"
      ></div>
    </div>
    <span class="progress-text">{{ Math.round(progress) }}%</span>
  </div>
</template>

<script setup lang="ts">
interface Props {
  progress: number
}

defineProps<Props>()
</script>

<style scoped>
.progress-container {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.progress-bar {
  flex: 1;
  height: 10px;
  background: rgba(255, 255, 255, 0.1);
  border-radius: 5px;
  overflow: hidden;
}

.progress-fill {
  height: 100%;
  background: linear-gradient(135deg, #e75e8d, #74c0fc);
  border-radius: 5px;
  transition: width 0.3s ease;
}

.progress-text {
  color: white;
  font-weight: bold;
  min-width: 50px;
  text-align: right;
}
</style>
