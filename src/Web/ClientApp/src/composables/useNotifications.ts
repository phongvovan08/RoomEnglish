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
    
    console.log('🎮 Adding notification:', notification)
    
    // Auto remove after duration
    let timeoutId: number | undefined
    if (duration > 0) {
      console.log(`🎮 Setting timeout for ${duration}ms to remove:`, id)
      timeoutId = window.setTimeout(() => {
        console.log('🎮 Timeout triggered, removing:', id)
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
    console.log('🎮 Total notifications:', notifications.value.length)
    
    return id
  }

  const removeNotification = (id: string) => {
    const index = notifications.value.findIndex(n => n.id === id)
    console.log('🎮 Attempting to remove notification:', id, 'found at index:', index)
    
    if (index > -1) {
      const notification = notifications.value[index]
      
      // Clear the timeout if it exists
      if (notification.timeoutId) {
        console.log('🎮 Clearing timeout:', notification.timeoutId)
        window.clearTimeout(notification.timeoutId)
      }
      
      // Remove from array
      notifications.value.splice(index, 1)
      console.log('🎮 Notification removed! Remaining count:', notifications.value.length)
      
      // Force reactivity update
      nextTick(() => {
        console.log('🎮 NextTick - Final count:', notifications.value.length)
      })
    } else {
      console.log('🎮 Notification not found for removal:', id)
    }
  }

  const clearAll = () => {
    console.log('🎮 Clearing all notifications')
    
    // Clear all timeouts
    notifications.value.forEach(notification => {
      if (notification.timeoutId) {
        window.clearTimeout(notification.timeoutId)
      }
    })
    
    notifications.value = []
    console.log('🎮 All notifications cleared')
  }

  // Convenience methods
  const success = (title: string, message?: string, duration?: number) => {
    return addNotification({ type: 'success', title, message, duration })
  }

  const error = (title: string, message?: string, duration?: number) => {
    return addNotification({ type: 'error', title, message, duration })
  }

  const warning = (title: string, message?: string, duration?: number) => {
    return addNotification({ type: 'warning', title, message, duration })
  }

  const info = (title: string, message?: string, duration?: number) => {
    return addNotification({ type: 'info', title, message, duration })
  }

  return {
    notifications: notifications,
    addNotification,
    removeNotification,
    clearAll,
    success,
    error,
    warning,
    info
  }
}