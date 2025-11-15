<template>
  <div class="cyborg-layout">
    <CyborgMenu />
    <main class="main-content">
      <slot></slot>
    </main>
    <NotificationToast />
  </div>
</template>

<script setup lang="ts">
import CyborgMenu from '../components/CyborgMenu.vue'
import NotificationToast from '@/components/ui/NotificationToast.vue'
</script>

<style scoped>
.cyborg-layout {
  min-height: 100vh;
  background-color: var(--primary-bg);
  background-image: 
    radial-gradient(circle at 20% 50%, rgba(236, 96, 144, 0.05) 0%, transparent 50%),
    radial-gradient(circle at 80% 20%, rgba(133, 82, 244, 0.05) 0%, transparent 50%),
    radial-gradient(circle at 40% 80%, rgba(236, 96, 144, 0.03) 0%, transparent 50%);
}

.main-content {
  padding-top: 60px;
  padding-left: 15px;
  padding-right: 15px;
  padding-bottom: 15px;
  max-width: 1320px;
  margin: 0 auto;
}

@media (max-width: 768px) {
  .main-content {
    padding-top: 40px;
  }
}
</style>
