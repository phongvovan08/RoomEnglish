<template>
  <Teleport to="body">
    <div class="notification-container">
      <TransitionGroup name="notification" tag="div">
        <div
          v-for="notification in notifications"
          :key="notification.id"
          class="notification"
          :class="`notification-${notification.type}`"
          :style="{ '--duration': `${notification.duration || 5000}ms` }"
        >
          <div class="notification-content">
            <div class="notification-icon">
              <Icon :icon="getNotificationIcon(notification.type)" />
            </div>
            <div class="notification-text">
              <h4 class="notification-title">{{ notification.title }}</h4>
              <p v-if="notification.message" class="notification-message">
                {{ notification.message }}
              </p>
            </div>
            <button
              @click="removeNotification(notification.id)"
              class="notification-close"
            >
              <Icon icon="mdi:close" />
            </button>
          </div>
        </div>
      </TransitionGroup>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { Icon } from '@iconify/vue'
import { useNotifications } from '@/utils/notifications'

const { notifications, removeNotification } = useNotifications()

const getNotificationIcon = (type: string) => {
  const icons = {
    success: 'mdi:check-circle',
    error: 'mdi:alert-circle',
    warning: 'mdi:alert',
    info: 'mdi:information'
  }
  return icons[type as keyof typeof icons] || icons.info
}
</script>

<style scoped>
.notification-container {
  position: fixed;
  top: 1rem;
  right: 1rem;
  z-index: 9999;
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  max-width: 400px;
  width: 100%;
  pointer-events: none;
}

.notification {
  background: var(--bg-primary);
  border: 1px solid var(--border-color);
  border-radius: 0.75rem;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.3), 0 0 20px rgba(231, 94, 141, 0.1);
  overflow: hidden;
  pointer-events: auto;
  backdrop-filter: blur(10px);
  position: relative;
  animation: slideIn 0.3s ease-out;
}

.notification::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 2px;
  background: linear-gradient(90deg, var(--accent-pink), var(--accent-blue));
  animation: progressBar var(--duration, 5000ms) linear forwards;
}

@keyframes slideIn {
  from {
    transform: translateX(100%);
    opacity: 0;
  }
  to {
    transform: translateX(0);
    opacity: 1;
  }
}

@keyframes progressBar {
  from {
    width: 100%;
  }
  to {
    width: 0%;
  }
}

.notification-success {
  border-left: 4px solid #22c55e;
  background: linear-gradient(135deg, rgba(34, 197, 94, 0.1) 0%, var(--bg-primary) 100%);
}

.notification-error {
  border-left: 4px solid #ef4444;
  background: linear-gradient(135deg, rgba(239, 68, 68, 0.1) 0%, var(--bg-primary) 100%);
}

.notification-warning {
  border-left: 4px solid #f59e0b;
  background: linear-gradient(135deg, rgba(245, 158, 11, 0.1) 0%, var(--bg-primary) 100%);
}

.notification-info {
  border-left: 4px solid #3b82f6;
  background: linear-gradient(135deg, rgba(59, 130, 246, 0.1) 0%, var(--bg-primary) 100%);
}

.notification-content {
  display: flex;
  align-items: flex-start;
  gap: 0.75rem;
  padding: 1rem;
}

.notification-icon {
  flex-shrink: 0;
  width: 1.5rem;
  height: 1.5rem;
  margin-top: 0.125rem;
}

.notification-success .notification-icon {
  color: #22c55e;
}

.notification-error .notification-icon {
  color: #ef4444;
}

.notification-warning .notification-icon {
  color: #f59e0b;
}

.notification-info .notification-icon {
  color: #3b82f6;
}

.notification-text {
  flex-grow: 1;
  min-width: 0;
}

.notification-title {
  margin: 0 0 0.25rem 0;
  font-weight: 600;
  font-size: 0.875rem;
  color: var(--text-primary);
  line-height: 1.2;
}

.notification-message {
  margin: 0;
  font-size: 0.8rem;
  color: var(--text-secondary);
  line-height: 1.4;
}

.notification-close {
  flex-shrink: 0;
  width: 1.5rem;
  height: 1.5rem;
  border: none;
  background: none;
  color: var(--text-secondary);
  cursor: pointer;
  border-radius: 0.25rem;
  transition: all 0.2s ease;
  display: flex;
  align-items: center;
  justify-content: center;
}

.notification-close:hover {
  color: var(--text-primary);
  background: rgba(255, 255, 255, 0.1);
}

/* Transition animations */
.notification-enter-active {
  transition: all 0.3s cubic-bezier(0.175, 0.885, 0.32, 1.275);
}

.notification-leave-active {
  transition: all 0.3s ease-in;
}

.notification-enter-from {
  opacity: 0;
  transform: translateX(100%) scale(0.8);
}

.notification-leave-to {
  opacity: 0;
  transform: translateX(100%) scale(0.9);
}

.notification-move {
  transition: transform 0.3s ease;
}

/* Mobile responsiveness */
@media (max-width: 480px) {
  .notification-container {
    left: 1rem;
    right: 1rem;
    max-width: none;
  }
  
  .notification-content {
    padding: 0.875rem;
  }
  
  .notification-title {
    font-size: 0.8rem;
  }
  
  .notification-message {
    font-size: 0.75rem;
  }
}
</style>