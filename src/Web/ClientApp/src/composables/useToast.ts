import { useNotifications } from './useNotifications'

export function useToast() {
  const { success, error, warning, info } = useNotifications()

  // Gaming-style toast messages with auto-hide (5s)
  const showSuccess = (title: string, message?: string, duration: number = 5000) => {
    return success(`🎮 ${title}`, message, duration)
  }

  const showError = (title: string, message?: string, duration: number = 5000) => {
    return error(`❌ ${title}`, message, duration)
  }

  const showWarning = (title: string, message?: string, duration: number = 5000) => {
    return warning(`⚠️ ${title}`, message, duration)
  }

  const showInfo = (title: string, message?: string, duration: number = 5000) => {
    return info(`ℹ️ ${title}`, message, duration)
  }

  // API specific messages
  const showApiSuccess = (action: string, item?: string) => {
    const messages = {
      create: `Item Created! 🎉`,
      update: `Item Updated! ✏️`,
      delete: `Item Deleted! 🗑️`,
      load: `Data Loaded! 📋`,
      complete: `Task Completed! ✅`,
      reopen: `Task Reopened! 🔄`
    }
    
    const title = messages[action as keyof typeof messages] || `${action} Successful!`
    const message = item ? `"${item}" has been processed successfully` : undefined
    
    return showSuccess(title, message)
  }

  const showApiError = (action: string, error?: any) => {
    const messages = {
      create: 'Failed to create item',
      update: 'Failed to update item', 
      delete: 'Failed to delete item',
      load: 'Failed to load data',
      auth: 'Authentication required. Please login first.',
      network: 'Network error. Please check your connection.',
      server: 'Server error. Please try again later.',
      permission: 'You do not have permission for this action.'
    }

    let title = 'Operation Failed'
    let message = 'Please try again'

    if (typeof action === 'string' && messages[action as keyof typeof messages]) {
      title = action.charAt(0).toUpperCase() + action.slice(1) + ' Failed'
      message = messages[action as keyof typeof messages]
    }

    // Handle different error types
    if (error?.response?.status === 401) {
      title = 'Authentication Required'
      message = messages.auth
    } else if (error?.response?.status === 403) {
      title = 'Permission Denied'
      message = messages.permission
    } else if (error?.response?.status >= 500) {
      title = 'Server Error'
      message = messages.server
    } else if (!error?.response) {
      title = 'Network Error'
      message = messages.network
    }

    return showError(title, message, 5000) // 5 seconds for all messages
  }

  return {
    showSuccess,
    showError, 
    showWarning,
    showInfo,
    showApiSuccess,
    showApiError
  }
}