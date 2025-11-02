<template>
  <div v-if="isRestoring" class="restoring-overlay">
    <div class="restoring-content">
      <div class="cyber-spinner-large"></div>
      <h3>Đang khôi phục tiến trình...</h3>
      <p class="restore-info" v-if="info">{{ info }}</p>
    </div>
  </div>
</template>

<script setup lang="ts">
interface Props {
  isRestoring: boolean
  info?: string
}

defineProps<Props>()
</script>

<style scoped>
.restoring-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: linear-gradient(135deg, rgba(26, 26, 46, 0.98) 0%, rgba(22, 33, 62, 0.98) 50%, rgba(15, 52, 96, 0.98) 100%);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 10000;
  backdrop-filter: blur(10px);
  animation: fadeIn 0.3s ease;
}

.restoring-content {
  text-align: center;
  color: white;
  max-width: 400px;
  padding: 2rem;
}

.restoring-content h3 {
  font-size: 1.5rem;
  margin: 1.5rem 0 1rem;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.restore-info {
  color: rgba(255, 255, 255, 0.7);
  font-size: 0.95rem;
  margin-top: 0.5rem;
  animation: pulse 1.5s ease-in-out infinite;
}

.cyber-spinner-large {
  width: 60px;
  height: 60px;
  border: 4px solid rgba(102, 126, 234, 0.2);
  border-top-color: #667eea;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin: 0 auto;
}

@keyframes fadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
}

@keyframes pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.6; }
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}
</style>
