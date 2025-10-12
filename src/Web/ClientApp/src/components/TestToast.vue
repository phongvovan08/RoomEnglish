<template>
  <div class="test-container">
    <h2>Toast Test</h2>
    <button @click="testToast" class="cyborg-btn cyborg-btn-gradient">
      Test Toast (5s Auto-Hide)
    </button>
    <p>Check console for debug logs</p>
  </div>
</template>

<script setup lang="ts">
import { useNotifications } from '@/composables/useNotifications'

const { success } = useNotifications()

const testToast = () => {
  console.log('🚀 Test button clicked!')
  success('Test Toast! 🎮', 'This should auto-hide in 5 seconds', 5000)
  console.log('🚀 success() called')
}
</script>

<style scoped>
.test-container {
  padding: 2rem;
  text-align: center;
}

.cyborg-btn {
  padding: 1rem 2rem;
  margin: 1rem;
  border: none;
  border-radius: 0.5rem;
  background: linear-gradient(135deg, #e75e8d, #667eea);
  color: white;
  cursor: pointer;
  font-weight: bold;
}

.cyborg-btn:hover {
  transform: translateY(-2px);
}
</style>