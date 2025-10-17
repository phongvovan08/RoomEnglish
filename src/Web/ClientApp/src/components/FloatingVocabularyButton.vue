<template>
  <div class="floating-vocabulary-btn" v-if="isAuthenticated && showFloatingBtn">
    <router-link 
      :to="Routes.Vocabulary.children.Management.path" 
      class="fab"
      @click="handleClick"
      title="Quick Access: Vocabulary Management"
    >
      <Icon icon="mdi:database-import" class="fab-icon" />
      <span class="fab-text">Import</span>
    </router-link>
    
    <!-- Tooltip -->
    <div class="fab-tooltip">
      <span>Vocabulary Management</span>
      <small>Upload & Export Excel files</small>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRoute } from 'vue-router'
import { Routes } from '@/router/constants'
import { Icon } from '@iconify/vue'
import { useAuth } from '@/composables/useAuth'

const route = useRoute()
const { isAuthenticated } = useAuth()
const showFloatingBtn = ref(true)

// Hide on vocabulary management page
const shouldHideOnCurrentPage = computed(() => {
  return route.path === Routes.Vocabulary.children.Management.path ||
         route.path.startsWith('/vocabulary/management')
})

// Show/hide based on scroll position and current page
const handleScroll = () => {
  if (shouldHideOnCurrentPage.value) {
    showFloatingBtn.value = false
    return
  }
  
  // Show after scrolling down a bit
  showFloatingBtn.value = window.scrollY > 300
}

const handleClick = () => {
  // Add some click animation feedback
  const fabElement = document.querySelector('.fab')
  if (fabElement) {
    fabElement.classList.add('fab-clicked')
    setTimeout(() => {
      fabElement.classList.remove('fab-clicked')
    }, 200)
  }
}

onMounted(() => {
  window.addEventListener('scroll', handleScroll)
  handleScroll() // Initial check
})

onUnmounted(() => {
  window.removeEventListener('scroll', handleScroll)
})
</script>

<style scoped>
.floating-vocabulary-btn {
  position: fixed;
  bottom: 30px;
  right: 30px;
  z-index: 1000;
  transition: all 0.3s ease;
}

.fab {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 16px 20px;
  background: linear-gradient(135deg, #74c0fc, #339af0);
  color: white;
  border-radius: 50px;
  text-decoration: none;
  box-shadow: 0 8px 25px rgba(116, 192, 252, 0.4);
  transition: all 0.3s ease;
  font-weight: 600;
  font-size: 14px;
  min-width: 60px;
  justify-content: center;
}

.fab:hover {
  transform: translateY(-3px);
  box-shadow: 0 12px 35px rgba(116, 192, 252, 0.5);
  color: white;
}

.fab:hover .fab-tooltip {
  opacity: 1;
  visibility: visible;
  transform: translateX(-50%) translateY(-10px);
}

.fab-clicked {
  transform: scale(0.95) translateY(-3px);
}

.fab-icon {
  font-size: 24px;
  transition: transform 0.3s ease;
}

.fab:hover .fab-icon {
  transform: rotate(10deg);
}

.fab-text {
  font-size: 13px;
  font-weight: 600;
  letter-spacing: 0.5px;
}

.fab-tooltip {
  position: absolute;
  bottom: 100%;
  left: 50%;
  transform: translateX(-50%) translateY(-5px);
  background: rgba(0, 0, 0, 0.9);
  color: white;
  padding: 12px 16px;
  border-radius: 10px;
  font-size: 12px;
  white-space: nowrap;
  opacity: 0;
  visibility: hidden;
  transition: all 0.3s ease;
  pointer-events: none;
  text-align: center;
}

.fab-tooltip::after {
  content: '';
  position: absolute;
  top: 100%;
  left: 50%;
  transform: translateX(-50%);
  border: 6px solid transparent;
  border-top-color: rgba(0, 0, 0, 0.9);
}

.fab-tooltip span {
  display: block;
  font-weight: 600;
  margin-bottom: 4px;
}

.fab-tooltip small {
  display: block;
  opacity: 0.8;
  font-size: 10px;
}

/* Mobile Responsiveness */
@media (max-width: 768px) {
  .floating-vocabulary-btn {
    bottom: 20px;
    right: 20px;
  }

  .fab {
    padding: 14px 18px;
    gap: 10px;
  }

  .fab-text {
    display: none;
  }

  .fab {
    border-radius: 50%;
    width: 56px;
    height: 56px;
    min-width: auto;
  }

  .fab-tooltip {
    display: none;
  }
}

/* Animation entrance */
@keyframes fabEntrance {
  from {
    opacity: 0;
    transform: translateY(20px) scale(0.8);
  }
  to {
    opacity: 1;
    transform: translateY(0) scale(1);
  }
}

.floating-vocabulary-btn {
  animation: fabEntrance 0.4s ease-out;
}

/* Pulse animation for attention */
.fab::before {
  content: '';
  position: absolute;
  top: -2px;
  left: -2px;
  right: -2px;
  bottom: -2px;
  background: linear-gradient(135deg, #74c0fc, #339af0);
  border-radius: 50px;
  opacity: 0;
  z-index: -1;
  animation: fabPulse 2s infinite;
}

@keyframes fabPulse {
  0% {
    opacity: 0;
    transform: scale(1);
  }
  50% {
    opacity: 0.3;
    transform: scale(1.1);
  }
  100% {
    opacity: 0;
    transform: scale(1.2);
  }
}

/* Hide when printing */
@media print {
  .floating-vocabulary-btn {
    display: none;
  }
}
</style>