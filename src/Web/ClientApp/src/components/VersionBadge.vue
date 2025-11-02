<template>
  <div class="version-badge">
    <span class="env-label" :class="envClass">{{ envLabel }}</span>
    <span class="version-number">v{{ version }}</span>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { getAppInfo } from '@/features/app/api/getAppInfo'

const version = ref('x.xx')
const environment = ref('Development')

const isDevelopment = computed(() => environment.value === 'Development')
const envLabel = computed(() => isDevelopment.value ? 'DEV' : 'PROD')
const envClass = computed(() => isDevelopment.value ? 'env-dev' : 'env-prod')

onMounted(async () => {
  try {
    const appInfo = await getAppInfo()
    version.value = appInfo.version
    environment.value = appInfo.environment
    console.log('✅ App Info loaded:', appInfo)
  } catch (error) {
    console.warn('❌ Failed to fetch app info, using defaults:', error)
  }
})
</script>

<style scoped>
.version-badge {
  position: fixed;
  bottom: 16px;
  right: 16px;
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 6px 12px;
  background: rgba(0, 0, 0, 0.75);
  backdrop-filter: blur(10px);
  border-radius: 20px;
  font-size: 12px;
  font-weight: 600;
  z-index: 9999;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  border: 1px solid rgba(255, 255, 255, 0.1);
  transition: all 0.3s ease;
}

.version-badge:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 16px rgba(0, 0, 0, 0.2);
}

.env-label {
  padding: 2px 8px;
  border-radius: 12px;
  font-size: 11px;
  font-weight: 700;
  letter-spacing: 0.5px;
}

.env-dev {
  background: linear-gradient(135deg, #10b981, #059669);
  color: white;
}

.env-prod {
  background: linear-gradient(135deg, #3b82f6, #2563eb);
  color: white;
}

.version-number {
  color: rgba(255, 255, 255, 0.9);
  font-family: 'Courier New', monospace;
}
</style>
