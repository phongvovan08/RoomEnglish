import { ref, nextTick } from 'vue'

export interface Notification {
  id: string
  type: 'success' | 'error' | 'warning' | 'info'
  title: string
  message?: string
  duration?: number
  timeoutId?: number
}

const notifications = ref<Notification[]>([])

let notificationId = 0

export function useNotifications() {
  const addNotification = (notification: Omit<Notification, 'id' | 'timeoutId'>) => {
    const id = `notification-${++notificationId}`
    const duration = notification.duration || 5000 // Default 5 seconds
    
    console.log('🔔 Adding notification:', notification)
    
    // Auto remove after duration
    let timeoutId: number | undefined
    if (duration > 0) {
      timeoutId = window.setTimeout(() => {
        removeNotification(id)
      }, duration)
    }
    
    const newNotification: Notification = {
      id,
      duration,
      timeoutId,
      ...notification
    }
    
    notifications.value.push(newNotification)
    console.log('📋 Total notifications:', notifications.value.length)
    
    return id
  }

  const removeNotification = (id: string) => {
    const index = notifications.value.findIndex(n => n.id === id)
    
    if (index > -1) {
      const notification = notifications.value[index]
      
      // Clear the timeout if it exists
      if (notification.timeoutId) {
        window.clearTimeout(notification.timeoutId)
      }
      
      // Remove from array
      notifications.value.splice(index, 1)
      
      // Force reactivity update
      nextTick(() => {
        // Notification removed
      })
    }
  }

    const clearAll = () => {
    // Clear all timeouts
    notifications.value.forEach(notification => {
      if (notification.timeoutId) {
        window.clearTimeout(notification.timeoutId)
      }
    })
    
    notifications.value = []
  }

  // Convenience methods
  const showSuccess = (title: string, message?: string, duration?: number) => {
    console.log('✅ showSuccess called:', title, message)
    return addNotification({ type: 'success', title, message, duration })
  }

  const showError = (title: string, message?: string, duration?: number) => {
    console.log('❌ showError called:', title, message)
    return addNotification({ type: 'error', title, message, duration })
  }

  const showWarning = (title: string, message?: string, duration?: number) => {
    return addNotification({ type: 'warning', title, message, duration })
  }

  const showInfo = (title: string, message?: string, duration?: number) => {
    return addNotification({ type: 'info', title, message, duration })
  }

  return {
    notifications: notifications,
    addNotification,
    removeNotification,
    clearAll,
    showSuccess,
    showError,
    showWarning,
    showInfo
  }
}